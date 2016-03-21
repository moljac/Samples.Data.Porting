namespace com.samsung.android.sdk.professionalaudio.sample.simpleclient
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using AndroidRuntimeException = android.util.AndroidRuntimeException;
	using Log = android.util.Log;
	using View = android.view.View;
	using Button = android.widget.Button;
	using Toast = android.widget.Toast;


	public class SapaSimpleClientActivity : Activity
	{

		private const string TAG = "SapaSimpleClient";

		internal Button mPlayButton, mActivateButton, mDeactivateButton;

		internal SapaService mService;
		internal SapaProcessor mClient;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_apasimple_client;

			try
			{
				Sapa sapa = new Sapa();
				sapa.initialize(this);
				mService = new SapaService();
				mService.stop(true);
				mService.start(SapaService.START_PARAM_DEFAULT_LATENCY);
				mClient = new SapaProcessor(this, null, new StatusListenerAnonymousInnerClassHelper(this));
				mService.register(mClient);

			}
			catch (SsdkUnsupportedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "not support professional audio", Toast.LENGTH_LONG).show();
				finish();
				return;
			}
			catch (InstantiationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "fail to instantiate", Toast.LENGTH_LONG).show();
				finish();
				return;
			}
			catch (AndroidRuntimeException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "fail to start", Toast.LENGTH_LONG).show();
				finish();
				return;
			}

			mPlayButton = (Button) this.findViewById(R.id.play_button);
			mPlayButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mActivateButton = (Button) this.findViewById(R.id.button_activate);
			mActivateButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

			mDeactivateButton = (Button) this.findViewById(R.id.button_deactivate);
			mDeactivateButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);
		}

		private class StatusListenerAnonymousInnerClassHelper : SapaProcessor.StatusListener
		{
			private readonly SapaSimpleClientActivity outerInstance;

			public StatusListenerAnonymousInnerClassHelper(SapaSimpleClientActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onKilled()
			{
				Log.v(TAG, "SapaSimpleClient will be closed. because of the SapaProcessor was closed.");
				outerInstance.mService.stop(true);
				finish();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : Button.OnClickListener
		{
			private readonly SapaSimpleClientActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(SapaSimpleClientActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				if (outerInstance.mClient != null)
				{
					outerInstance.mClient.activate();
					outerInstance.mPlayButton.Text = R.@string.playing_sound;
					outerInstance.mPlayButton.Enabled = false;
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : Button.OnClickListener
		{
			private readonly SapaSimpleClientActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(SapaSimpleClientActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				if (outerInstance.mClient != null)
				{
					outerInstance.mClient.activate();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : Button.OnClickListener
		{
			private readonly SapaSimpleClientActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(SapaSimpleClientActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				if (outerInstance.mClient != null)
				{
					outerInstance.mClient.deactivate();
				}
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			if (mService != null)
			{
				if (mClient != null)
				{
					mService.unregister(mClient);
				}
				mService.stop(true);
			}
		}
	}

}