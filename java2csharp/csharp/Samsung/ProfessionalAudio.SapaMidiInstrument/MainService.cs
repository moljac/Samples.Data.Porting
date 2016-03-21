using System;

namespace com.samsung.audiosuite.sapamidisample
{

	using Service = android.app.Service;
	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using SparseArray = android.util.SparseArray;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Sapa = com.samsung.android.sdk.professionalaudio.Sapa;
	using SapaProcessor = com.samsung.android.sdk.professionalaudio.SapaProcessor;
	using SapaService = com.samsung.android.sdk.professionalaudio.SapaService;
	using SapaActionDefinerInterface = com.samsung.android.sdk.professionalaudio.app.SapaActionDefinerInterface;
	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using SapaAppStateListener = com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
	using SapaConnectionNotSetException = com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
	using SapaServiceConnectListener = com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;


	public class MainService : Service, SapaServiceConnectListener, SapaActionDefinerInterface
	{

		private const string TAG = "audiosuite:sapamidisample:j:MainService";

		//Commands for communication with native part
		private const string COMMAND_SHUSH = "cmd:shush";

		//Binder for activity
		private LocalBinder mBinder;

		private SapaAppService mSapaAppService;

		//SapaAppInfo package representing this application
		private SapaAppInfo mMyInfo;

		//Reference to the activity of application
		//private WeakReference<MainActivity> mActivity;

		private SapaService mSapaService;
		private SapaProcessor mSapaProcessor;

		//Only note on/off that is why 3 bytes allocated
		private ByteBuffer mMidiEvent = ByteBuffer.allocateDirect(3);

		internal SparseArray<SapaActionInfo> mActionArray;

		private string mCallerPackageName;

		public override void onCreate()
		{
			Log.d(TAG, "onCreate");
			base.onCreate();
			mBinder = new LocalBinder(this);
			this.mSapaAppService = null;
		}

		public override int onStartCommand(Intent intent, int flags, int startId)
		{
			if (intent != null)
			{
				//When application is activated from ProfessionalAudio system it receives SapaAppInfo object describing it.
				//To obtain this object static method getSapaAppInfo() is to be used.
				SapaAppInfo info = SapaAppInfo.getAppInfo(intent);
				mCallerPackageName = intent.getStringExtra("com.samsung.android.sdk.professionalaudio.key.callerpackagename");
				//Toast.makeText(this, mCallerPackageName, Toast.LENGTH_SHORT).show();
				if (info != null)
				{
					//Received SapaAppInfo is saved to class field.
					mMyInfo = info;
				}
			}

			//Connection to remote service of ProfessionalAudio system is set.
			if (mSapaAppService == null)
			{
				connectConnectionBridge();
			}

			return base.onStartCommand(intent, flags, startId);
		}

		public override IBinder onBind(Intent intent)
		{
			Log.d(TAG, "onBind");
			//Binder for local activity.
			return mBinder;
		}

		public override void onDestroy()
		{
			Log.d(TAG, "onDestroy");

			//Native part of application is being deactivated.
			if (mSapaService != null && mSapaProcessor != null)
			{
				mSapaProcessor.deactivate();
				mSapaService.unregister(mSapaProcessor);
				this.mSapaProcessor = null;
			}

			if (this.mSapaAppService != null)
			{
				try
				{
					if (this.mMyInfo != null && this.mMyInfo.App != null)
					{
					//Application needs to declare that it was successfully deactivated.
					this.mSapaAppService.removeFromActiveApps(this.mMyInfo.App);
					}

					//Action definer is being removed.
					this.mSapaAppService.removeActionDefiner();
				}
				catch (SapaConnectionNotSetException e)
				{
					Log.e(TAG, "Instance could not be removed from active list because of connection exception.");
				}
				//Connection with remote service is finished.
				this.mSapaAppService.disconnect();
				this.mSapaAppService = null;
			}

			this.mMyInfo = null;


			base.onDestroy();
		}

		public override void onServiceConnected()
		{
			Log.d(TAG, "onServiceConnected");
			try
			{
				if (this.mMyInfo == null)
				{
					mMyInfo = mSapaAppService.getInstalledApp(this.PackageName);
				}
				if (mMyInfo != null)
				{

					//Actions are being set in SapaAppInfo representing this application.
					mActionArray = new SparseArray<SapaActionInfo>();
					mActionArray.put(0, new SapaActionInfo(COMMAND_SHUSH, R.drawable.ctrl_btn_stop_default, PackageName));
					mMyInfo.Actions = mActionArray;

					//Native part is being initialised.
					Sapa sapa = new Sapa();
					sapa.initialize(this);
					mSapaService = new SapaService();
					mSapaProcessor = new SapaProcessor(this, null, new SapaProcessorStateListener(this, mMyInfo.App));
					mSapaService.register(mSapaProcessor);
					mSapaProcessor.activate();

					//Information about ports is being set in SapaAppInfo representing this app.
					//It can not be done before activating SapaProcessor.
					mMyInfo.PortFromSapaProcessor = mSapaProcessor;

					//Application needs to declare that it was successfully activated.
					if (mSapaAppService != null)
					{
						this.mSapaAppService.addActiveApp(this.mMyInfo);
					}
				}
			}
			catch (SapaConnectionNotSetException e)
			{
				Log.e(TAG, "App could not be added to active as connection has not been made.");
			}
			catch (System.ArgumentException e)
			{
				Log.e(TAG, "Initialisation of Sapa is not possible due to invalid context of application");
			}
			catch (SsdkUnsupportedException e)
			{
				Log.e(TAG, "Initialisation of Sapa is not possible as Sapa is not available on the device");
			}
			catch (InstantiationException e)
			{
				Log.e(TAG, "SapaService can not be instantiate");
			}
		}

		public override Runnable getActionDefinition(SapaApp sapaApp, string actionId)
		{
			Log.d(TAG, "getActionDefinition: actionId=" + actionId);
			if (actionId.Equals(COMMAND_SHUSH))
			{
				return () =>
				{
					MainService.this.shush();
				};
			}
			return null;
		}

		internal virtual SapaAppService SapaAppService
		{
			get
			{
				return mSapaAppService;
			}
		}

		private void connectConnectionBridge()
		{
			Log.d(TAG, "Connect using bridge");
			mSapaAppService = new SapaAppService(this);

			//MainService starts listening for establishment of connection.
			//MainService needs to implement AudioServiceConnectListener to be able to listen for this event.
			//When event occurs onServiceConnected() method is called.
			mSapaAppService.addConnectionListener(this);

			//MainService declares that it defines actions for this application.
			//MainService needs to implement AudioActionDefinerInterface.
			mSapaAppService.ActionDefiner = this;

			mSapaAppService.addAppStateListener(mSapaAppStateListener);

			//Connection to AudioConnectionService starts being created.
			mSapaAppService.connect();
		}

		private SapaAppStateListener mSapaAppStateListener = new SapaAppStateListenerAnonymousInnerClassHelper();

		private class SapaAppStateListenerAnonymousInnerClassHelper : SapaAppStateListener
		{
			public SapaAppStateListenerAnonymousInnerClassHelper()
			{
			}


			public override void onTransportMasterChanged(SapaApp arg0)
			{
				// TODO Auto-generated method stub

			}

			public override void onAppUninstalled(SapaApp arg0)
			{
				// TODO Auto-generated method stub

			}

			public override void onAppInstalled(SapaApp arg0)
			{
				// TODO Auto-generated method stub

			}

			public override void onAppDeactivated(SapaApp sapaApp)
			{
				Log.d(TAG, "onAppDeactivated: " + sapaApp.InstanceId + " was deactivated.");
				if (sapaApp.PackageName.contentEquals(outerInstance.mCallerPackageName))
				{
					Log.d(TAG, "Caller is closed.");
					stopSelf();
				}

			}

			public override void onAppChanged(SapaApp arg0)
			{
			}

			public override void onAppActivated(SapaApp arg0)
			{
				// TODO Auto-generated method stub

			}
		}


		internal virtual void sendNoteOn(int note, int velocity)
		{
			mMidiEvent.put(0, unchecked((sbyte) 0x90));
			mMidiEvent.put(1, (sbyte) note);
			mMidiEvent.put(2, (sbyte) velocity);
			//mSapaProcessor.sendCommand(new String(mMidiEvent.array()));
			mSapaProcessor.sendStream(mMidiEvent, 0x90);
		}

		internal virtual void sendNoteOff(int note, int velocity)
		{
			mMidiEvent.put(0, unchecked((sbyte) 0x80));
			mMidiEvent.put(1, (sbyte) note);
			mMidiEvent.put(2, (sbyte) velocity);
			//mSapaProcessor.sendCommand(new String(mMidiEvent.array()));
			mSapaProcessor.sendStream(mMidiEvent, 0x80);
		}

		internal virtual void shush()
		{
			mSapaProcessor.sendCommand(COMMAND_SHUSH);
		}

		internal class SapaProcessorStateListener : SapaProcessor.StatusListener
		{
			private readonly MainService outerInstance;


			internal SapaApp mSapaApp;

			public SapaProcessorStateListener(MainService outerInstance, SapaApp sapaApp)
			{
				this.outerInstance = outerInstance;
				mSapaApp = sapaApp;
			}

			public override void onKilled()
			{
				Log.d(TAG, mSapaApp.InstanceId + " was killed.");


				try
				{
					outerInstance.mSapaProcessor = null;

					// remove from active apps // This will notify to the Soundcamp.
					outerInstance.mSapaAppService.removeFromActiveApps(mSapaApp);

					outerInstance.stopSelf();

				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

		}

		internal class LocalBinder : Binder
		{
			private readonly MainService outerInstance;

			public LocalBinder(MainService outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			internal virtual MainService getMainService(MainActivity activity)
			{
				Log.d(TAG, "getMainService");
				//MainService.this.mActivity = new WeakReference<MainActivity>(activity);
				return outerInstance;
			}
		}

		public override void onServiceDisconnected()
		{
			Log.d(TAG, "Nothing to be done");
		}
	}

}