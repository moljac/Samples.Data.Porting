using System;
using System.Threading;

namespace com.opentok.android.demo.audio
{

	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using AudioFormat = android.media.AudioFormat;
	using AudioManager = android.media.AudioManager;
	using AudioRecord = android.media.AudioRecord;
	using AudioTrack = android.media.AudioTrack;
	using AudioSource = android.media.MediaRecorder.AudioSource;
	using Log = android.util.Log;


	public class CustomAudioDevice : BaseAudioDevice
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			m_renderEvent = m_rendererLock.newCondition();
			m_captureEvent = m_captureLock.newCondition();
		}


		private const string LOG_TAG = "opentok-defaultaudiodevice";

		private const int SAMPLING_RATE = 44100;
		private const int NUM_CHANNELS_CAPTURING = 1;
		private const int NUM_CHANNELS_RENDERING = 1;

		private const int MAX_SAMPLES = 2 * 480 * 2; // Max 10 ms @ 48 kHz

		private Context m_context;

		private AudioTrack m_audioTrack;
		private AudioRecord m_audioRecord;

		// Capture & render buffers
		private ByteBuffer m_playBuffer;
		private ByteBuffer m_recBuffer;
		private sbyte[] m_tempBufPlay;
		private sbyte[] m_tempBufRec;

		private readonly ReentrantLock m_rendererLock = new ReentrantLock(true);
		private Condition m_renderEvent;
		private volatile bool m_isRendering = false;
		private volatile bool m_shutdownRenderThread = false;

		private readonly ReentrantLock m_captureLock = new ReentrantLock(true);
		private Condition m_captureEvent;
		private volatile bool m_isCapturing = false;
		private volatile bool m_shutdownCaptureThread = false;

		private AudioSettings m_captureSettings;
		private AudioSettings m_rendererSettings;

		// Capturing delay estimation
		private int m_estimatedCaptureDelay = 0;

		// Rendering delay estimation
		private int m_bufferedPlaySamples = 0;
		private int m_playPosition = 0;
		private int m_estimatedRenderDelay = 0;

		private AudioManager m_audioManager;

		public CustomAudioDevice(Context context)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
			this.m_context = context;

			try
			{
				m_playBuffer = ByteBuffer.allocateDirect(MAX_SAMPLES);
				m_recBuffer = ByteBuffer.allocateDirect(MAX_SAMPLES);
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, e.Message);
			}

			m_tempBufPlay = new sbyte[MAX_SAMPLES];
			m_tempBufRec = new sbyte[MAX_SAMPLES];

			m_captureSettings = new AudioSettings(SAMPLING_RATE, NUM_CHANNELS_CAPTURING);
			m_rendererSettings = new AudioSettings(SAMPLING_RATE, NUM_CHANNELS_RENDERING);

			m_audioManager = (AudioManager) m_context.getSystemService(Context.AUDIO_SERVICE);

			m_audioManager.Mode = AudioManager.MODE_IN_COMMUNICATION;
		}

		public override bool initCapturer()
		{

			// get the minimum buffer size that can be used
			int minRecBufSize = AudioRecord.getMinBufferSize(m_captureSettings.SampleRate, NUM_CHANNELS_CAPTURING == 1 ? AudioFormat.CHANNEL_IN_MONO : AudioFormat.CHANNEL_IN_STEREO, AudioFormat.ENCODING_PCM_16BIT);

			// double size to be more safe
			int recBufSize = minRecBufSize * 2;

			// release the object
			if (m_audioRecord != null)
			{
				m_audioRecord.release();
				m_audioRecord = null;
			}

			try
			{
				m_audioRecord = new AudioRecord(AudioSource.VOICE_COMMUNICATION, m_captureSettings.SampleRate, NUM_CHANNELS_CAPTURING == 1 ? AudioFormat.CHANNEL_IN_MONO : AudioFormat.CHANNEL_IN_STEREO, AudioFormat.ENCODING_PCM_16BIT, recBufSize);

			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, e.Message);
				return false;
			}

			// check that the audioRecord is ready to be used
			if (m_audioRecord.State != AudioRecord.STATE_INITIALIZED)
			{
				Log.i(LOG_TAG, "Audio capture is not initialized " + m_captureSettings.SampleRate);

				return false;
			}

			m_shutdownCaptureThread = false;
			(new Thread(m_captureThread)).Start();

			return true;
		}

		public override bool destroyCapturer()
		{
			m_captureLock.@lock();
			// release the object
			m_audioRecord.release();
			m_audioRecord = null;
			m_shutdownCaptureThread = true;
			m_captureEvent.signal();

			m_captureLock.unlock();
			return true;
		}

		public virtual int EstimatedCaptureDelay
		{
			get
			{
				return m_estimatedCaptureDelay;
			}
		}

		public override bool startCapturer()
		{
			// start recording
			try
			{
				m_audioRecord.startRecording();

			}
			catch (System.InvalidOperationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return false;
			}

			m_captureLock.@lock();
			m_isCapturing = true;
			m_captureEvent.signal();
			m_captureLock.unlock();

			return true;
		}

		public override bool stopCapturer()
		{
			m_captureLock.@lock();
			try
			{
				// only stop if we are recording
				if (m_audioRecord.RecordingState == AudioRecord.RECORDSTATE_RECORDING)
				{
					// stop recording
					try
					{
						m_audioRecord.stop();
					}
					catch (System.InvalidOperationException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return false;
					}
				}
			}
			finally
			{
				// Ensure we always unlock
				m_isCapturing = false;
				m_captureLock.unlock();
			}

			return true;
		}

		private Runnable m_captureThread = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public virtual void run()
			{

				int samplesToRec = SAMPLING_RATE / 100;
				int samplesRead = 0;

				try
				{
					android.os.Process.ThreadPriority = android.os.Process.THREAD_PRIORITY_URGENT_AUDIO;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				while (!outerInstance.m_shutdownCaptureThread)
				{

					outerInstance.m_captureLock.@lock();

					try
					{

						if (!outerInstance.m_isCapturing)
						{
							outerInstance.m_captureEvent.@await();
							continue;

						}
						else
						{

							if (outerInstance.m_audioRecord == null)
							{
								continue;
							}

							int lengthInBytes = (samplesToRec << 1) * NUM_CHANNELS_CAPTURING;
							int readBytes = outerInstance.m_audioRecord.read(outerInstance.m_tempBufRec, 0, lengthInBytes);

							outerInstance.m_recBuffer.rewind();
							outerInstance.m_recBuffer.put(outerInstance.m_tempBufRec);

							samplesRead = (readBytes >> 1) / NUM_CHANNELS_CAPTURING;

						}
					}
					catch (Exception e)
					{
						Log.e(LOG_TAG, "RecordAudio try failed: " + e.Message);
						continue;

					}
					finally
					{
						// Ensure we always unlock
						outerInstance.m_captureLock.unlock();
					}

					AudioBus.writeCaptureData(outerInstance.m_recBuffer, samplesRead);
					outerInstance.m_estimatedCaptureDelay = samplesRead * 1000 / SAMPLING_RATE;
				}
			}
		}

		public override bool initRenderer()
		{

			// get the minimum buffer size that can be used
			int minPlayBufSize = AudioTrack.getMinBufferSize(m_rendererSettings.SampleRate, NUM_CHANNELS_RENDERING == 1 ? AudioFormat.CHANNEL_OUT_MONO : AudioFormat.CHANNEL_OUT_STEREO, AudioFormat.ENCODING_PCM_16BIT);

			int playBufSize = minPlayBufSize;
			if (playBufSize < 6000)
			{
				playBufSize *= 2;
			}

			// release the object
			if (m_audioTrack != null)
			{
				m_audioTrack.release();
				m_audioTrack = null;
			}

			try
			{
				m_audioTrack = new AudioTrack(AudioManager.STREAM_VOICE_CALL, m_rendererSettings.SampleRate, NUM_CHANNELS_RENDERING == 1 ? AudioFormat.CHANNEL_OUT_MONO : AudioFormat.CHANNEL_OUT_STEREO, AudioFormat.ENCODING_PCM_16BIT, playBufSize, AudioTrack.MODE_STREAM);
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, e.Message);
				return false;
			}

			// check that the audioRecord is ready to be used
			if (m_audioTrack.State != AudioTrack.STATE_INITIALIZED)
			{
				Log.i(LOG_TAG, "Audio renderer not initialized " + m_rendererSettings.SampleRate);
				return false;
			}

			m_bufferedPlaySamples = 0;

			OutputMode = OutputMode.SpeakerPhone;

			m_shutdownRenderThread = false;
			(new Thread(m_renderThread)).Start();

			return true;
		}

		public override bool destroyRenderer()
		{
			m_rendererLock.@lock();
			// release the object
			m_audioTrack.release();
			m_audioTrack = null;
			m_shutdownRenderThread = true;
			m_renderEvent.signal();
			m_rendererLock.unlock();

			unregisterHeadsetReceiver();
			m_audioManager.SpeakerphoneOn = false;
			m_audioManager.Mode = AudioManager.MODE_NORMAL;

			return true;
		}

		public virtual int EstimatedRenderDelay
		{
			get
			{
				return m_estimatedRenderDelay;
			}
		}

		public override bool startRenderer()
		{
			// start playout
			try
			{
				m_audioTrack.play();

			}
			catch (System.InvalidOperationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return false;
			}

			m_rendererLock.@lock();
			m_isRendering = true;
			m_renderEvent.signal();
			m_rendererLock.unlock();

			return true;
		}

		public override bool stopRenderer()
		{
			m_rendererLock.@lock();
			try
			{
				// only stop if we are playing
				if (m_audioTrack.PlayState == AudioTrack.PLAYSTATE_PLAYING)
				{
					// stop playout
					try
					{
						m_audioTrack.stop();
					}
					catch (System.InvalidOperationException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return false;
					}

					// flush the buffers
					m_audioTrack.flush();
				}

			}
			finally
			{
				// Ensure we always unlock, both for success, exception or error
				// return.
				m_isRendering = false;

				m_rendererLock.unlock();
			}

			return true;
		}

		private Runnable m_renderThread = new RunnableAnonymousInnerClassHelper2();

		private class RunnableAnonymousInnerClassHelper2 : Runnable
		{
			public RunnableAnonymousInnerClassHelper2()
			{
			}


			public virtual void run()
			{
				int samplesToPlay = SAMPLING_RATE / 100;

				try
				{
					android.os.Process.ThreadPriority = android.os.Process.THREAD_PRIORITY_URGENT_AUDIO;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				while (!outerInstance.m_shutdownRenderThread)
				{
					outerInstance.m_rendererLock.@lock();
					try
					{
						if (!outerInstance.m_isRendering)
						{
							outerInstance.m_renderEvent.@await();
							continue;

						}
						else
						{
							outerInstance.m_rendererLock.unlock();

							// Don't lock on audioBus calls
							outerInstance.m_playBuffer.clear();
							int samplesRead = AudioBus.readRenderData(outerInstance.m_playBuffer, samplesToPlay);

							// Log.d(LOG_TAG, "Samples read: " + samplesRead);

							outerInstance.m_rendererLock.@lock();

							// After acquiring the lock again
							// we must check if we are still playing
							if (outerInstance.m_audioTrack == null || !outerInstance.m_isRendering)
							{
								continue;
							}

							int bytesRead = (samplesRead << 1) * NUM_CHANNELS_RENDERING;
							outerInstance.m_playBuffer.get(outerInstance.m_tempBufPlay, 0, bytesRead);

							int bytesWritten = outerInstance.m_audioTrack.write(outerInstance.m_tempBufPlay, 0, bytesRead);

							// increase by number of written samples
							outerInstance.m_bufferedPlaySamples += (bytesWritten >> 1) / NUM_CHANNELS_RENDERING;

							// decrease by number of played samples
							int pos = outerInstance.m_audioTrack.PlaybackHeadPosition;
							if (pos < outerInstance.m_playPosition)
							{
								// wrap or reset by driver
								outerInstance.m_playPosition = 0;
							}
							outerInstance.m_bufferedPlaySamples -= (pos - outerInstance.m_playPosition);
							outerInstance.m_playPosition = pos;

							// we calculate the estimated delay based on the
							// buffered samples
							outerInstance.m_estimatedRenderDelay = outerInstance.m_bufferedPlaySamples * 1000 / SAMPLING_RATE;

						}
					}
					catch (Exception e)
					{
						Log.e(LOG_TAG, "Exception: " + e.Message);
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
					finally
					{
						outerInstance.m_rendererLock.unlock();
					}
				}
			}
		}

		public override AudioSettings CaptureSettings
		{
			get
			{
				return this.m_captureSettings;
			}
		}

		public override AudioSettings RenderSettings
		{
			get
			{
				return this.m_rendererSettings;
			}
		}

		/// <summary>
		/// Communication modes handling
		/// </summary>

		public virtual bool setOutputMode(OutputMode mode)
		{
			base.OutputMode = mode;
			if (mode == OutputMode.Handset)
			{
				unregisterHeadsetReceiver();
				m_audioManager.SpeakerphoneOn = false;
			}
			else
			{
				m_audioManager.SpeakerphoneOn = true;
				registerHeadsetReceiver();
			}
			return true;
		}

		private BroadcastReceiver m_headsetReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}

			public override void onReceive(Context context, Intent intent)
			{
				if (intent.Action.compareTo(Intent.ACTION_HEADSET_PLUG) == 0)
				{
					int state = intent.getIntExtra("state", 0);
					if (state == 0)
					{
						outerInstance.m_audioManager.SpeakerphoneOn = true;
					}
					else
					{
						outerInstance.m_audioManager.SpeakerphoneOn = false;
					}
				}
			}
		}

		private bool m_receiverRegistered;

		private void registerHeadsetReceiver()
		{
			if (!m_receiverRegistered)
			{
				IntentFilter receiverFilter = new IntentFilter(Intent.ACTION_HEADSET_PLUG);

				m_context.registerReceiver(m_headsetReceiver, receiverFilter);
				m_receiverRegistered = true;
			}
		}

		private void unregisterHeadsetReceiver()
		{
			if (m_receiverRegistered)
			{
				try
				{
					m_context.unregisterReceiver(m_headsetReceiver);
				}
				catch (System.ArgumentException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				m_receiverRegistered = false;
			}
		}

		public override void onPause()
		{
			if (OutputMode == OutputMode.SpeakerPhone)
			{
				unregisterHeadsetReceiver();
			}
		}

		public override void onResume()
		{
			if (OutputMode == OutputMode.SpeakerPhone)
			{
				registerHeadsetReceiver();
			}
		}
	}
}