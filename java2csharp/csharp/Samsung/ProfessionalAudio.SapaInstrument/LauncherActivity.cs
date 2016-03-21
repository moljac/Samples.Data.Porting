using System.Collections.Generic;

namespace com.samsung.audiosuite.sapainstrumentsample
{


	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using ImageButton = android.widget.ImageButton;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Sapa = com.samsung.android.sdk.professionalaudio.Sapa;
	using SapaPort = com.samsung.android.sdk.professionalaudio.SapaPort;
	using SapaPortConnection = com.samsung.android.sdk.professionalaudio.SapaPortConnection;
	using SapaProcessor = com.samsung.android.sdk.professionalaudio.SapaProcessor;
	using StatusListener = com.samsung.android.sdk.professionalaudio.SapaProcessor.StatusListener;
	using SapaService = com.samsung.android.sdk.professionalaudio.SapaService;

	/// <summary>
	/// Activity started when application is started with launcher.
	/// 
	/// Such an instance of application is not visible from other audio applications as active one. It
	/// also does not get notification about state of other apps. It connects itself to system output.
	/// </summary>
	public class LauncherActivity : Activity
	{

		private const string TAG = "audiosuite:sapainstrumentsample:j:LauncherActivity";

		private ImageButton mPlayButton;
		private ImageButton mStopButton;

		// SapaProcessor of the standalone instance of the application. It handles
		// communication with native part from the activity and does not use service
		// at all.
		private SapaProcessor mProcessor = null;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.d(TAG, "onCreate");
			ContentView = R.layout.activity_main;
			base.onCreate(savedInstanceState);

			this.Title = getString(R.@string.app_name) + " for standalone";

			// Views are being set from the layout.
			this.mPlayButton = (ImageButton) findViewById(R.id.playButton);
			this.mStopButton = (ImageButton) findViewById(R.id.stopButton);

			// Native part of application is being started.
			this.startSapaProcessor();

			// Controls actions are being set.
			// Only one button is visible at a time, so visibility needs to be
			// changed.
			this.mPlayButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			this.mStopButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
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
				outerInstance.mPlayButton.Visibility = View.GONE;
				outerInstance.mStopButton.Visibility = View.VISIBLE;
				Logic.startPlaying(outerInstance.mProcessor);
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
				outerInstance.mPlayButton.Visibility = View.VISIBLE;
				outerInstance.mStopButton.Visibility = View.GONE;
				Logic.stopPlaying(outerInstance.mProcessor);
			}
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
				Log.d(TAG, "Standalone SapaInstrumentSample was killed.");
				try
				{
					outerInstance.stopSapaProcessor();
					// The force param should be false 
					// to protect killing the SapaProcessors which is running with the Soundcamp.
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

			// Get list of ports for connecting to system ports.
			IList<SapaPort> ports = new List<SapaPort>();
			foreach (SapaPort port in mProcessor.Ports)
			{
				if (port.InOutType == SapaPort.INOUT_TYPE_OUT)
				{
					ports.Add(port);
				}
			}

			// Connect ports from two lists (first to first, second to second).
			if (ports.Count >= 2)
			{
				sapaService.connect(new SapaPortConnection(ports[0], sapaService.getSystemPort("playback_1")));
				sapaService.connect(new SapaPortConnection(ports[1], sapaService.getSystemPort("playback_2")));
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
				// the force param should be false 
				// to protect killing the SapaProcessors which is running with the Soundcamp.
				sapaService.stop(false);
			}
			catch (InstantiationException)
			{
			}
			base.onDestroy();
		}
	}

}