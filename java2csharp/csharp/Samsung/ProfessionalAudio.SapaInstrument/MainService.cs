using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace com.samsung.audiosuite.sapainstrumentsample
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

		private const string TAG = "audiosuite:sapainstrumentsample:j:MainService";

		// Binder for activity
		private LocalBinder mBinder;

		private SapaAppService mSapaAppService;

		// SapaAppInfo package representing this application
		internal Dictionary<string, SapaAppInfo> mAppInfoList;

		internal Dictionary<string, SapaProcessor> mProcessorList;

		internal Dictionary<string, ActionsPack> mActionsList;

		// Reference to the activity of application
		private WeakReference<MainActivity> mActivity;

		private SapaService mSapaService;

		private bool mIsPlaying;

		internal SparseArray<SapaActionInfo> mActionArray;

		private IDictionary<string, string> mCallerPackageNameList;

		private LinkedList<Intent> mDelayedIntents = new LinkedList<Intent>();

		private bool mServiceConnected;

		public override void onCreate()
		{
			Log.d(TAG, "onCreate");
			base.onCreate();
			this.mBinder = new LocalBinder(this);
			this.mIsPlaying = false;
			this.mSapaAppService = null;
			this.mServiceConnected = false;
			this.mActionsList = new Dictionary<string, MainService.ActionsPack>();
			this.mProcessorList = new Dictionary<string, SapaProcessor>();
			this.mAppInfoList = new Dictionary<string, SapaAppInfo>();
			this.mCallerPackageNameList = new ConcurrentDictionary<string, string>();
		}

		public override int onStartCommand(Intent intent, int flags, int startId)
		{
			Log.d(TAG, "onStartCommand");
			if (intent != null)
			{
				if (this.mServiceConnected)
				{
					handleIntent(intent);
				}
				else
				{
					this.mDelayedIntents.AddLast(intent);
				}
			}

			// Connection to remote service of ProfessionalAudio system is set.
			if (this.mSapaAppService == null)
			{
				connectConnectionBridge();
			}

			return base.onStartCommand(intent, flags, startId);
		}

		public override IBinder onBind(Intent intent)
		{
			Log.d(TAG, "onBind");
			// Binder for local activity.
			return this.mBinder;
		}

		public override void onDestroy()
		{
			Log.d(TAG, "onDestroy");

			// Native part of application is being deactivated.
			if (this.mSapaService != null && this.mProcessorList != null && this.mProcessorList.Count > 0)
			{
				foreach (SapaProcessor processor in this.mProcessorList.Values)
				{
					processor.deactivate();
					this.mSapaService.unregister(processor);
				}
			}

			if (this.mAppInfoList != null && this.mAppInfoList.Count > 0)
			{
				foreach (SapaAppInfo sapaAppInfo in this.mAppInfoList.Values)
				{
					if (sapaAppInfo.App != null)
					{
						try
						{
							// Application needs to declare that its instance was
							// successfully deactivated.
							this.mSapaAppService.removeFromActiveApps(sapaAppInfo.App);
						}
						catch (SapaConnectionNotSetException e)
						{
							Log.e(TAG, "Instance could not be removed from active list because of connection exception.");
						}
					}
				}
			}

			// Action definer is being removed.
			this.mSapaAppService.removeActionDefiner();
			// Connection with remote service is finished.
			this.mSapaAppService.disconnect();

			base.onDestroy();
		}

		internal virtual void updateBpm()
		{
			if (mSapaAppService != null)
			{
				try
				{
					IList<SapaAppInfo> infos = mSapaAppService.AllActiveApp;
					foreach (SapaAppInfo info in infos)
					{
						if (info.App.PackageName.contentEquals("com.sec.musicstudio"))
						{
							if (info.Configuration != null)
							{
								int bpm = info.Configuration.getInt("bpm");
								Log.d(TAG, "bpm is " + bpm);
							}
						}
					}
				}
				catch (SapaConnectionNotSetException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		public override void onServiceConnected()
		{
			Log.d(TAG, "onServiceConnected");
			try
			{
				Sapa sapa = new Sapa();
				sapa.initialize(this);
				this.mSapaService = new SapaService();
				this.mServiceConnected = true;
			}
			catch (SsdkUnsupportedException e)
			{
				Log.e(TAG, "Initialisation of Sapa is not possible as Sapa is not available on the device");
			}
			catch (InstantiationException e)
			{
				Log.e(TAG, "SapaService can not be instantiate");
			}
			if (this.mServiceConnected)
			{
				while (mDelayedIntents.Count > 0)
				{
					Intent intent = mDelayedIntents.RemoveFirst();
					if (intent != null)
					{
						handleIntent(intent);
					}
				}
			}
		}

		/*
		 * This method is responsible for handling received intent. It can only be called when
		 * connection is set.
		 */
		private void handleIntent(Intent intent)
		{
			int state = SapaAppInfo.getState(intent);
			switch (state)
			{
				case SapaAppInfo.STATE_ACTIVATING:
					this.handleActivationIntent(intent);
					break;
				case SapaAppInfo.STATE_DEACTIVATING:
					this.handleDeactivationIntent(intent);

					break;
			}
		}

		private void handleActivationIntent(Intent intent)
		{
			// When application is activated from ProfessionalAudio system it
			// receives SapaAppInfo object describing it.
			// To obtain this object static method getSapaAppInfo() is to be used.
			SapaAppInfo info = SapaAppInfo.getAppInfo(intent);

			if (info != null)
			{
				string callerPackageName = intent.getStringExtra("com.samsung.android.sdk.professionalaudio.key.callerpackagename");
				if (callerPackageName == null)
				{
					callerPackageName = "unknown";
				}
				mCallerPackageNameList[info.App.InstanceId] = callerPackageName;
				Log.d(TAG, "info.getApp.getInstanceId() " + info.App.InstanceId);
				if (!this.mAppInfoList.ContainsKey(info.App.InstanceId))
				{
					this.mAppInfoList[info.App.InstanceId] = info;
				}
				try
				{
					// Actions are being set in SapaAppInfo representing this
					// application.
					mActionArray = new SparseArray<SapaActionInfo>();
					mActionArray.put(0, new SapaActionInfo(MainActivity.ACTION_PLAY, R.drawable.ctrl_btn_play_default, this.PackageName));
					mActionArray.put(1, new SapaActionInfo(MainActivity.ACTION_STOP, R.drawable.ctrl_btn_stop_default, this.PackageName, true, false));
					info.Actions = mActionArray;

					// Native part is being initialised.
					SapaProcessor sapaProcessor = new SapaProcessor(this, null, new SapaProcessorStateListener(this, info.App));
					this.mSapaService.register(sapaProcessor);
					sapaProcessor.activate();
					this.mProcessorList[info.App.InstanceId] = sapaProcessor;

					// Information about ports is being set in SapaAppInfo
					// representing this app.
					// It can not be done before activating SapaProcessor.
					info.PortFromSapaProcessor = sapaProcessor;

					// Application needs to declare that it was successfully
					// activated.
					Log.d(TAG, "addactiveapp " + info.App.InstanceId);
					this.mSapaAppService.addActiveApp(info);
				}
				catch (SapaConnectionNotSetException e)
				{
					Log.e(TAG, "App could not be added to active as connection has not been made.");

				}
				catch (InstantiationException e)
				{
					Log.e(TAG, "SapaService can not be instantiate...");
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			if (mActionsList != null && info != null && info.App != null)
			{
				this.mActionsList[info.App.InstanceId] = new ActionsPack(this, info.App);
			}
		}

		private void handleDeactivationIntent(Intent intent)
		{
			lock (this)
			{
				SapaApp sapaApp = SapaAppInfo.getApp(intent);
				this.mActionsList.Remove(sapaApp.InstanceId);
				SapaProcessor processor = this.mProcessorList[sapaApp.InstanceId];
				if (processor != null)
				{
					processor.deactivate();
					this.mSapaService.unregister(processor);
				}
				this.mProcessorList.Remove(sapaApp.InstanceId);
				try
				{
					this.mSapaAppService.removeFromActiveApps(sapaApp);
				}
				catch (SapaConnectionNotSetException e)
				{
					Log.e(TAG, "Instance could not be removed from active list because of connection exception.");
				}
				this.mAppInfoList.Remove(sapaApp.InstanceId);
			}
		}

		public override Runnable getActionDefinition(SapaApp sapaApp, string actionId)
		{
			lock (this)
			{
				// Basing on actionId appropriate action definition is returned.
				Log.d(TAG, "instanceid " + sapaApp.InstanceId + " actionId " + actionId);
				if (actionId.Equals(MainActivity.ACTION_PLAY))
				{
					return this.mActionsList[sapaApp.InstanceId].PlayAction;
				}
				else if (actionId.Equals(MainActivity.ACTION_STOP))
				{
					return this.mActionsList[sapaApp.InstanceId].StopAction;
				}
				return null;
			}
		}

		internal virtual bool Playing
		{
			get
			{
				return this.mIsPlaying;
			}
		}

		internal virtual void play(SapaApp sapaApp)
		{
			SapaAppInfo info = this.mAppInfoList[sapaApp.InstanceId];
			// Play action is being deactivated.
			modifyAction(info, MainActivity.ACTION_PLAY, R.drawable.ctrl_btn_play_disabled, false);
			// Stop action is being activated.
			modifyAction(info, MainActivity.ACTION_STOP, R.drawable.ctrl_btn_stop_default, true);
			try
			{
				// Notification that AppInfo has been changed is sent.
				this.mSapaAppService.changeAppInfo(info);
			}
			catch (SapaConnectionNotSetException)
			{
				Log.w(TAG, "State of application cound not be changed due to connection exception");
			}

			this.mIsPlaying = true;
			// Play command is sent to native part.
			Logic.startPlaying(this.mProcessorList[sapaApp.InstanceId]);

		}

		internal virtual void stop(SapaApp sapaApp)
		{
			SapaAppInfo info = this.mAppInfoList[sapaApp.InstanceId];
			// Play action is being activated.
			modifyAction(info, MainActivity.ACTION_PLAY, R.drawable.ctrl_btn_play_default, true);
			// Stop action is being deactivated.
			modifyAction(info, MainActivity.ACTION_STOP, R.drawable.ctrl_btn_stop_disabled, false);
			try
			{
				// Notification that AppInfo has been changed is sent.
				this.mSapaAppService.changeAppInfo(info);
			}
			catch (SapaConnectionNotSetException)
			{
				Log.w(TAG, "State of application cound not be changed due to connection exception");
			}

			this.mIsPlaying = false;

			// Stop command is sent to native part.
			Logic.stopPlaying(this.mProcessorList[sapaApp.InstanceId]);
		}

		internal virtual SapaAppService SapaAppService
		{
			get
			{
				return this.mSapaAppService;
			}
		}

		private void modifyAction(SapaAppInfo info, string actionKey, int iconResId, bool isEnabled)
		{
			// In this example only one action can be active at the same time.
			// We need to change state of actions every time action is called.
			if (info != null && mSapaAppService != null)
			{
				// Obtaining current ActionInfo.
				SapaActionInfo action = info.getActionInfo(actionKey);
				if (action != null)
				{
					// Settling state.
					action.Visible = isEnabled;
					// Settling icon of action by setting id of the resource.
					action.Icon = iconResId;
				}
			}
		}

		private void connectConnectionBridge()
		{
			Log.d(TAG, "Connect using bridge");
			this.mSapaAppService = new SapaAppService(this);

			// MainService starts listening for establishment of connection.
			// MainService needs to implement AudioServiceConnectListener to be able
			// to listen for this event.
			// When event occurs onServiceConnected() method is called.
			this.mSapaAppService.addConnectionListener(this);

			// MainService declares that it defines actions for this application.
			// MainService needs to implement AudioActionDefinerInterface.
			this.mSapaAppService.ActionDefiner = this;

			this.mSapaAppService.addAppStateListener(mSapaAppStateListener);

			// Connection to AudioConnectionService starts being created.
			this.mSapaAppService.connect();
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

				if (outerInstance.mCallerPackageNameList.Count > 0)
				{
					ISet<KeyValuePair<string, string>> entrySet = outerInstance.mCallerPackageNameList.SetOfKeyValuePairs();
					ISet<KeyValuePair<string, string>>.Enumerator iterator = entrySet.GetEnumerator();
					while (iterator.MoveNext())
					{
						KeyValuePair<string, string> entry = iterator.Current;

						if (sapaApp.PackageName.contentEquals(entry.Value))
						{
							Log.d(TAG, "Caller is closed.");
							// Close this instance. because the caller is closed by
							// unknow reason. So, It will be not used anymore.
							outerInstance.closeAnInstance(entry.Key);
						}
					}
				}

				if (outerInstance.mCallerPackageNameList.Count == 0)
				{
					stopSelf();
				}

			}

			public override void onAppChanged(SapaApp arg0)
			{
				// update bpm
				Log.d(TAG, "onAppChanged");
				outerInstance.updateBpm();

			}

			public override void onAppActivated(SapaApp arg0)
			{
				// TODO Auto-generated method stub

			}
		}

		private void closeAnInstance(string instanceId)
		{
			// Native part of application is being deactivated.
			if (this.mSapaService != null && this.mProcessorList != null && this.mProcessorList.Count > 0)
			{
				SapaProcessor processor = this.mProcessorList[instanceId];
				if (processor != null)
				{
					processor.deactivate();
					mSapaService.unregister(processor);
					mProcessorList.Remove(instanceId);
				}
			}

			if (this.mAppInfoList != null && this.mAppInfoList.Count > 0)
			{
				SapaAppInfo sapaAppInfo = this.mAppInfoList[instanceId];
				if (sapaAppInfo != null && sapaAppInfo.App != null)
				{
					try
					{
						// Application needs to declare that its instance was
						// successfully deactivated.
						mSapaAppService.removeFromActiveApps(sapaAppInfo.App);
						mAppInfoList.Remove(instanceId);
					}
					catch (SapaConnectionNotSetException e)
					{
						Log.e(TAG, "Instance could not be removed from active list because of connection exception.");
					}
				}
			}
			mCallerPackageNameList.Remove(instanceId);
			mActionsList.Remove(instanceId);
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
					outerInstance.mProcessorList.Remove(mSapaApp.InstanceId);
					outerInstance.mAppInfoList.Remove(mSapaApp.InstanceId);
					outerInstance.mActionsList.Remove(mSapaApp.InstanceId);

					// remove from active apps // This will notify to the Soundcamp.
					outerInstance.mSapaAppService.removeFromActiveApps(mSapaApp);

					if (outerInstance.mProcessorList.Count == 0)
					{
						outerInstance.stopSelf();
					}

				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

			}

		}

		internal class ActionsPack
		{
			private readonly MainService outerInstance;


			internal SapaApp mSapaApp;
			internal Runnable mPlayAction;
			internal Runnable mStopAction;
			internal bool mIsPlaying;

			internal ActionsPack(MainService outerInstance, SapaApp sapaApp)
			{
				this.outerInstance = outerInstance;
				this.mSapaApp = sapaApp;
				this.mPlayAction = null;
				this.mStopAction = null;
				this.mIsPlaying = false;
			}

			internal virtual bool Playing
			{
				get
				{
					return this.mIsPlaying;
				}
			}

			internal virtual Runnable PlayAction
			{
				get
				{
					if (this.mPlayAction == null)
					{
						setPlayAction();
					}
					return this.mPlayAction;
				}
			}

			internal virtual Runnable StopAction
			{
				get
				{
					if (this.mStopAction == null)
					{
						setStopAction();
					}
					return this.mStopAction;
				}
			}

			internal virtual void setPlayAction()
			{

				// Definition of volume down action
				this.mPlayAction = () =>
				{

					// Here you write body of your action
					// boolean wasMaxVolume =
					// isMaxVolume(ActionsPack.this.mSapaApp);

					try
					{
						outerInstance.play(ActionsPack.this.mSapaApp);
						ActionsPack.this.mIsPlaying = true;
						if (outerInstance.mActivity != null)
						{
							MainActivity activity = outerInstance.mActivity.get();
							if (activity != null && activity.isVisibleInstance(ActionsPack.this.mSapaApp))
							{
								activity.changeButtonsOnPlay();
							}
						}
					}
					catch (System.NullReferenceException)
					{
						;
					}

				};
			}

			internal virtual void setStopAction()
			{
				lock (this)
				{
        
					// Definition of volume down action
					this.mStopAction = () =>
					{
        
						// Here you write body of your action

						try
						{
							outerInstance.stop(ActionsPack.this.mSapaApp);
							ActionsPack.this.mIsPlaying = false;
							if (outerInstance.mActivity != null)
							{
								MainActivity activity = outerInstance.mActivity.get();
								if (activity.isVisibleInstance(ActionsPack.this.mSapaApp))
								{
									activity.changeButtonsOnStop();
								}
							}
						}
						catch (System.NullReferenceException)
						{
							;
						}

					};
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
				outerInstance.mActivity = new WeakReference<MainActivity>(activity);
				return outerInstance;
			}
		}

		public override void onServiceDisconnected()
		{
			// Nothing to do.
		}
	}

}