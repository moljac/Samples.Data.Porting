namespace com.example.floatingcontrollersample
{

	using FloatingController = com.samsung.android.sdk.professionalaudio.widgets.FloatingController;
	using SapaActionDefinerInterface = com.samsung.android.sdk.professionalaudio.app.SapaActionDefinerInterface;
	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using SapaServiceConnectListener = com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;

	public class MainActivity : Activity, SapaActionDefinerInterface
	{

		// Floating controller.
		internal FloatingController mFloatingController;

		internal SapaServiceConnectListener mListener = new SapaServiceConnectListenerAnonymousInnerClassHelper();

		private class SapaServiceConnectListenerAnonymousInnerClassHelper : SapaServiceConnectListener
		{
			public SapaServiceConnectListenerAnonymousInnerClassHelper()
			{
			}


			public override void onServiceConnected()
			{
				outerInstance.mFloatingController.SapaAppService = outerInstance.mBridge;
			}

			public override void onServiceDisconnected()
			{

			}
		}

		internal SapaAppService mBridge;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;
			mBridge = new SapaAppService(this);
			mBridge.addConnectionListener(mListener);
			mBridge.connect();
			mFloatingController = ((FloatingController) findViewById(R.id.jam_control));
		}

		protected internal override void onDestroy()
		{
			mBridge.removeConnectionListener(mListener);
			mBridge.disconnect();
			base.onDestroy();
		}


		public override Runnable getActionDefinition(SapaApp arg0, string arg1)
		{
			return null;
		}

	}

}