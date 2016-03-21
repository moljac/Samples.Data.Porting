using System;

namespace com.sinch.android.rtc.sample.calling
{

	using Context = android.content.Context;
	using AssetFileDescriptor = android.content.res.AssetFileDescriptor;
	using AudioFormat = android.media.AudioFormat;
	using AudioManager = android.media.AudioManager;
	using AudioTrack = android.media.AudioTrack;
	using MediaPlayer = android.media.MediaPlayer;
	using Uri = android.net.Uri;
	using Log = android.util.Log;


	public class AudioPlayer
	{

		internal static readonly string LOG_TAG = typeof(AudioPlayer).Name;

		private Context mContext;

		private MediaPlayer mPlayer;

		private AudioTrack mProgressTone;

		private const int SAMPLE_RATE = 16000;

		public AudioPlayer(Context context)
		{
			this.mContext = context.ApplicationContext;
		}

		public virtual void playRingtone()
		{
			AudioManager audioManager = (AudioManager) mContext.getSystemService(Context.AUDIO_SERVICE);

			// Honour silent mode
			switch (audioManager.RingerMode)
			{
				case AudioManager.RINGER_MODE_NORMAL:
					mPlayer = new MediaPlayer();
					mPlayer.AudioStreamType = AudioManager.STREAM_RING;

					try
					{
						mPlayer.setDataSource(mContext, Uri.parse("android.resource://" + mContext.PackageName + "/" + R.raw.phone_loud1));
						mPlayer.prepare();
					}
					catch (IOException e)
					{
						Log.e(LOG_TAG, "Could not setup media player for ringtone");
						mPlayer = null;
						return;
					}
					mPlayer.Looping = true;
					mPlayer.start();
					break;
			}
		}

		public virtual void stopRingtone()
		{
			if (mPlayer != null)
			{
				mPlayer.stop();
				mPlayer.release();
				mPlayer = null;
			}
		}

		public virtual void playProgressTone()
		{
			stopProgressTone();
			try
			{
				mProgressTone = createProgressTone(mContext);
				mProgressTone.play();
			}
			catch (Exception e)
			{
				Log.e(LOG_TAG, "Could not play progress tone", e);
			}
		}

		public virtual void stopProgressTone()
		{
			if (mProgressTone != null)
			{
				mProgressTone.stop();
				mProgressTone.release();
				mProgressTone = null;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static android.media.AudioTrack createProgressTone(android.content.Context context) throws java.io.IOException
		private static AudioTrack createProgressTone(Context context)
		{
			AssetFileDescriptor fd = context.Resources.openRawResourceFd(R.raw.progress_tone);
			int length = (int) fd.Length;

			AudioTrack audioTrack = new AudioTrack(AudioManager.STREAM_VOICE_CALL, SAMPLE_RATE, AudioFormat.CHANNEL_OUT_MONO, AudioFormat.ENCODING_PCM_16BIT, length, AudioTrack.MODE_STATIC);

			sbyte[] data = new sbyte[length];
			readFileToBytes(fd, data);

			audioTrack.write(data, 0, data.Length);
			audioTrack.setLoopPoints(0, data.Length / 2, 30);

			return audioTrack;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void readFileToBytes(android.content.res.AssetFileDescriptor fd, byte[] data) throws java.io.IOException
		private static void readFileToBytes(AssetFileDescriptor fd, sbyte[] data)
		{
			System.IO.FileStream inputStream = fd.createInputStream();

			int bytesRead = 0;
			while (bytesRead < data.Length)
			{
				int res = inputStream.Read(data, bytesRead, (data.Length - bytesRead));
				if (res == -1)
				{
					break;
				}
				bytesRead += res;
			}
		}
	}

}