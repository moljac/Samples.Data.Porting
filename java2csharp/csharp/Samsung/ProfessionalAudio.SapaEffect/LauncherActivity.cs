using System.Collections.Generic;

namespace com.samsung.audiosuite.sapaeffectsample
{


	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Sapa = com.samsung.android.sdk.professionalaudio.Sapa;
	using SapaPort = com.samsung.android.sdk.professionalaudio.SapaPort;
	using SapaPortConnection = com.samsung.android.sdk.professionalaudio.SapaPortConnection;
	using SapaProcessor = com.samsung.android.sdk.professionalaudio.SapaProcessor;
	using StatusListener = com.samsung.android.sdk.professionalaudio.SapaProcessor.StatusListener;
	using SapaService = com.samsung.android.sdk.professionalaudio.SapaService;

	public class LauncherActivity : EffectSampleActivity
	{

		private const string TAG = "audiosuite:sapaeffectsample:j:LauncherActivity";

		private int mCurrentVolume;

		// SapaProcessor of the standalone instance of the application. It handles
		// communication with native part from the activity and does not use service
		// at all.
		private SapaProcessor mProcessor = null;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.d(TAG, "onCreate");
			base.onCreate(savedInstanceState);

			this.mCurrentVolume = Logic.DEFAULT_VOLUME;

			// Native part of application is being started.
			this.startSapaProcessor();

			this.mVolDownButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			this.mVolUpButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{

				try
				{
					outerInstance.mCurrentVolume = Logic.decreaseVolume(outerInstance.mCurrentVolume);
					if (Logic.isMinVolume(outerInstance.mCurrentVolume))
					{
						outerInstance.setMinVolumeStateOnButtons();
					}
					else
					{
						outerInstance.setBetweenVolumeStateOnButtons();
					}
					outerInstance.updateVolumeTextView(Logic.getVolumeText(outerInstance.mCurrentVolume));
					Logic.sendVolume(outerInstance.mProcessor, outerInstance.mCurrentVolume);
				}
				catch (System.NullReferenceException)
				{
					;
				}

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{

				try
				{
					outerInstance.mCurrentVolume = Logic.increaseVolume(outerInstance.mCurrentVolume);
					if (Logic.isMaxVolume(outerInstance.mCurrentVolume))
					{
						outerInstance.setMaxVolumeStateOnButtons();
					}
					else
					{
						outerInstance.setBetweenVolumeStateOnButtons();
					}
					outerInstance.updateVolumeTextView(Logic.getVolumeText(outerInstance.mCurrentVolume));
					Logic.sendVolume(outerInstance.mProcessor, outerInstance.mCurrentVolume);
				}
				catch (System.NullReferenceException)
				{
					;
				}

			}
		}

		protected internal override void onResume()
		{
			Log.d(TAG, "onResume");
			base.onResume();
			updateVolumeTextView(Logic.getVolumeText(mCurrentVolume));
			Logic.sendVolume(mProcessor, mCurrentVolume);
			this.setCurrentState();
		}

		/// <summary>
		/// This method is responsible for unregistering the processor of standalone instance, which
		/// means stopping it native part.
		/// </summary>
		private void stopSapaProcessor()
		{
			try
			{
				SapaService sapaService = new SapaService();
				sapaService.unregister(this.mProcessor);
			}
			catch (InstantiationException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		/// <summary>
		/// This method is responsible for starting native part of the standalone instance of
		/// application. This means registering the processor, registering ports and connecting to system
		/// audio ports.
		/// </summary>
		private void startSapaProcessor()
		{
			try
			{
				(new Sapa()).initialize(this);
				SapaService sapaService = new SapaService();
				if (!sapaService.Started)
				{
					sapaService.start(SapaService.START_PARAM_DEFAULT_LATENCY);
				}
				// Creating processor for stand alone version.
				this.mProcessor = new SapaProcessor(this, null, new StatusListenerAnonymousInnerClassHelper(this));
				// The processor is being registered.
				sapaService.register(this.mProcessor);

				// The processor is being activated.
				this.mProcessor.activate();

				// Audio output ports are being connected to system input ports.
				connectPorts(sapaService);

			}
			catch (InstantiationException e)
			{
				Log.w(TAG, "SapaService was not created");
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (System.ArgumentException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (SsdkUnsupportedException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private class StatusListenerAnonymousInnerClassHelper : SapaProcessor.StatusListener
		{
			private readonly LauncherActivity outerInstance;

			public StatusListenerAnonymousInnerClassHelper(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onKilled()
			{
				Log.d(TAG, "Standalone SapaEffectSample was killed.");
				try
				{
					outerInstance.stopSapaProcessor();
					(new SapaService()).stop(false);
				}
				catch (InstantiationException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				finish();
			}
		}

		/// <summary>
		/// This method is responsible for connecting ports of standalone instance of application to
		/// system ports.
		/// </summary>
		/// <param name="sapaService"> </param>
		private void connectPorts(SapaService sapaService)
		{

			// Get lists of ports for connecting to system ports.
			IList<SapaPort> outPorts = new List<SapaPort>();
			IList<SapaPort> inPorts = new List<SapaPort>();
			foreach (SapaPort port in mProcessor.Ports)
			{
				if (port.InOutType == SapaPort.INOUT_TYPE_OUT)
				{
					outPorts.Add(port);
				}
				else
				{
					inPorts.Add(port);
				}
			}

			// Create list of system ports to connect to.
			IList<SapaPort> systemInPorts = new List<SapaPort>();
			IList<SapaPort> systemOutPorts = new List<SapaPort>();
			foreach (SapaPort port in sapaService.SystemPorts)
			{
				if (port.InOutType == SapaPort.INOUT_TYPE_IN && port.Name.contains("playback"))
				{
					systemInPorts.Add(port);
				}
				else if (port.InOutType == SapaPort.INOUT_TYPE_OUT && port.Name.contains("capture"))
				{
					systemOutPorts.Add(port);
				}
			}

			// Connect ports from two lists (first to first, second to second).
			if (outPorts.Count >= 2 && systemInPorts.Count >= 2)
			{
				sapaService.connect(new SapaPortConnection(outPorts[0], systemInPorts[0]));
				sapaService.connect(new SapaPortConnection(outPorts[1], systemInPorts[1]));
			}
			if (inPorts.Count >= 2 && systemOutPorts.Count >= 2)
			{
				sapaService.connect(new SapaPortConnection(systemOutPorts[0], inPorts[0]));
				sapaService.connect(new SapaPortConnection(systemOutPorts[1], inPorts[1]));
			}
		}

		protected internal override void onDestroy()
		{
			Log.d(TAG, "onDestroy");
			// Native part is being stopped.
			this.stopSapaProcessor();
			SapaService sapaService;
			try
			{
				sapaService = new SapaService();
				sapaService.stop(false);
			}
			catch (InstantiationException)
			{
			}

			base.onDestroy();
		}

		private void setCurrentState()
		{

			try
			{
				if (Logic.isMaxVolume(mCurrentVolume))
				{
					this.setMaxVolumeStateOnButtons();
				}
				else if (Logic.isMinVolume(mCurrentVolume))
				{
					this.setMinVolumeStateOnButtons();
				}
				else
				{
					this.setBetweenVolumeStateOnButtons();
				}
			}
			catch (System.NullReferenceException)
			{
				;
			}

		}
	}

}