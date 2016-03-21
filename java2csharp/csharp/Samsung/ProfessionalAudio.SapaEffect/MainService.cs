using System;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace com.samsung.audiosuite.sapaeffectsample
{

	using Service = android.app.Service;
	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
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

		private const string TAG = "audiosuite:sapaeffectsample:j:MainService";

		// Binder for activity
		private LocalBinder mBinder;

		private SapaAppService mSapaAppService;

		// SapaAppInfo package representing this application
		internal Dictionary<string, SapaAppInfo> mAppInfoList;

		internal Dictionary<string, SapaProcessor> mProcessorList;

		internal Dictionary<string, ActionsPack> mActionsList;

		private IDictionary<string, string> mCallerPackageNameList;

		// Reference to the activity of application
		private WeakReference<MainActivity> mActivity;

		private SapaService mSapaService;

		internal SparseArray<SapaActionInfo> mActionArray;

		private LinkedList<Intent> mDelayedIntents = new LinkedList<Intent>();

		private bool mServiceConnected;

		public override void onCreate()
		{
			Log.d(TAG, "onCreate");
			base.onCreate();
			this.mBinder = new LocalBinder(this);
			this.mServiceConnected = false;
			this.mActionsList = new Dictionary<string, MainService.ActionsPack>();
			this.mProcessorList = new Dictionary<string, SapaProcessor>();
			this.mAppInfoList = new Dictionary<string, SapaAppInfo>();
			this.mCallerPackageNameList = new ConcurrentDictionary<string, string>();
		}

		public override int onStartCommand(Intent intent, int flags, int startId)
		{
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
					if (info.Configuration == null)
					{
						Log.d(TAG, "kaka, not exist configuration, volume[" + Logic.DEFAULT_VOLUME + "] will be set in " + info.App.InstanceId);
						this.setConfiguration(info.App, Logic.DEFAULT_VOLUME);

					}
					else
					{
						Log.d(TAG, "kaka volume[" + info.Configuration.getInt("CONFIG") + "] was got from " + info.App.InstanceId);
					}
					// Actions are being set in SapaAppInfo representing this
					// application.
					mActionArray = new SparseArray<SapaActionInfo>();
					mActionArray.put(0, new SapaActionInfo(MainActivity.VOLUME_DOWN, R.drawable.ctrl_btn_volume_down_default, this.PackageName));
					mActionArray.put(1, new SapaActionInfo(MainActivity.VOLUME_UP, R.drawable.ctrl_btn_volume_up_default, this.PackageName));
					info.Actions = mActionArray;

					// Native part is being initialised.
					SapaProcessor sapaProcessor = new SapaProcessor(this, null, new SapaProcessorStateListener(this, info.App));
					this.mSapaService.register(sapaProcessor);
					sapaProcessor.activate();
					this.mProcessorList[info.App.InstanceId] = sapaProcessor;

					Logic.sendVolume(sapaProcessor, this.getCurrectVolume(info.App));

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
					Log.e(TAG, "SapaService can not be instantiate");
				}
			}
			if (mActionsList != null && info != null && info.App != null)
			{
				this.mActionsList[info.App.InstanceId] = new ActionsPack(this, info.App);
			}
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

		private void handleDeactivationIntent(Intent intent)
		{
			lock (this)
			{
				SapaApp sapaApp = SapaAppInfo.getApp(intent);
				this.mActionsList.Remove(sapaApp.InstanceId);
				foreach (KeyValuePair<string, SapaProcessor> entry in this.mProcessorList.SetOfKeyValuePairs())
				{
					Log.d(TAG, "kakakaka " + entry.Key + " -> " + entry.Value);
				}
				SapaProcessor processor = this.mProcessorList[sapaApp.InstanceId];
				Log.d(TAG, "kakakaka instanceId " + sapaApp.InstanceId + " processor " + processor);
				processor.deactivate();
				this.mSapaService.unregister(this.mProcessorList[sapaApp.InstanceId]);
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

		public override Runnable getActionDefinition(SapaApp sapaApp, string actionId)
		{
			lock (this)
			{
				// Basing on actionId appropriate action definition is returned.
				Log.d(TAG, "instanceid " + sapaApp.InstanceId + " actionId " + actionId);
				if (actionId.Equals(MainActivity.VOLUME_DOWN))
				{
					return this.mActionsList[sapaApp.InstanceId].VolumeDownAction;
				}
				else if (actionId.Equals(MainActivity.VOLUME_UP))
				{
					return this.mActionsList[sapaApp.InstanceId].VolumeUpAction;
				}
				return null;
			}
		}

		internal class ActionsPack
		{
			private readonly MainService outerInstance;


			internal SapaApp mSapaApp;
			internal Runnable mVolDown;
			internal Runnable mVolUp;

			internal ActionsPack(MainService outerInstance, SapaApp sapaApp)
			{
				this.outerInstance = outerInstance;
				this.mSapaApp = sapaApp;
				this.mVolDown = null;
				this.mVolUp = null;
			}

			internal virtual Runnable VolumeDownAction
			{
				get
				{
					if (this.mVolDown == null)
					{
						setVolumeDown();
					}
					return this.mVolDown;
				}
			}

			internal virtual Runnable VolumeUpAction
			{
				get
				{
					if (this.mVolUp == null)
					{
						setVolumeUp();
					}
					return this.mVolUp;
				}
			}

			internal virtual void setVolumeDown()
			{

				// Definition of volume down action
				this.mVolDown = () =>
				{

					// Here you write body of your action
					// boolean wasMaxVolume = isMaxVolume(ActionsPack.this.mSapaApp);

					try
					{
						outerInstance.decreaseVolume(ActionsPack.this.mSapaApp);
					}
					catch (System.NullReferenceException)
					{
						;
					}

				};
			}

			internal virtual void setVolumeUp()
			{
				lock (this)
				{
        
					// Definition of volume down action
					this.mVolUp = () =>
					{
        
						// Here you write body of your action

						try
						{
							outerInstance.increaseVolume(ActionsPack.this.mSapaApp);
						}
						catch (System.NullReferenceException)
						{
							;
						}

					};
				}
			}
		}

		private int getCurrectVolume(SapaApp sapaApp)
		{
			Bundle bundle = this.mAppInfoList[sapaApp.InstanceId].Configuration;
			if (bundle != null)
			{
				return bundle.getInt("CONFIG");
			}
			return -1;
		}

		internal virtual bool isMinVolume(SapaApp sapaApp)
		{
			return Logic.isMinVolume(this.getCurrectVolume(sapaApp));
		}

		internal virtual bool isMaxVolume(SapaApp sapaApp)
		{
			return Logic.isMaxVolume(this.getCurrectVolume(sapaApp));
		}

		// void setVolume(SapaApp sapaApp, int volume) {
		// try {
		// boolean wasMaxVolume = isMaxVolume(sapaApp);
		// boolean wasMinVolume = isMinVolume(sapaApp);
		// setConfiguration(sapaApp, volume);
		// boolean isMaxVolume = isMaxVolume(sapaApp);
		// boolean isMinVolume = isMinVolume(sapaApp);
		//
		// if (wasMaxVolume && !isMaxVolume) {
		// modifyAction(sapaApp, MainActivity.VOLUME_UP,
		// R.drawable.ctrl_btn_volume_up_default, true);
		// } else if (isMaxVolume && !wasMaxVolume) {
		// modifyAction(sapaApp, MainActivity.VOLUME_UP,
		// R.drawable.ctrl_btn_volume_up_disabled, false);
		// }
		//
		// if (wasMinVolume && !isMinVolume) {
		// modifyAction(sapaApp, MainActivity.VOLUME_DOWN,
		// R.drawable.ctrl_btn_volume_down_default, true);
		// } else if (isMinVolume && !wasMinVolume) {
		// modifyAction(sapaApp, MainActivity.VOLUME_DOWN,
		// R.drawable.ctrl_btn_volume_down_disabled, false);
		// }
		//
		// this.mSapaAppService.changeAppInfo(mAppInfoList.get(sapaApp.getInstanceId()));
		//
		// showVolume(volume);
		// } catch (SapaConnectionNotSetException e) {
		// Log.w(TAG,
		// "State of application could not be changed due to connection exception");
		// }
		//

		private bool isVisibieView(SapaApp sapaApp)
		{
			if (mActivity != null && mActivity.get() != null && mActivity.get().VisibleApp != null && mActivity.get().VisibleApp.App != null && mActivity.get().VisibleApp.App.InstanceId != null && sapaApp != null && mActivity.get().VisibleApp.App.InstanceId.Equals(sapaApp.InstanceId) == true)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		internal virtual void decreaseVolume(SapaApp sapaApp)
		{
			lock (this)
			{
				if (isMaxVolume(sapaApp))
				{
					modifyAction(sapaApp, MainActivity.VOLUME_UP, R.drawable.ctrl_btn_volume_up_default, true);
				}
				int currentVolume = getCurrectVolume(sapaApp);
				currentVolume = Logic.decreaseVolume(currentVolume);
        
				if (Logic.isMinVolume(currentVolume))
				{
					modifyAction(sapaApp, MainActivity.VOLUME_DOWN, R.drawable.ctrl_btn_volume_down_disabled, false);
					if (isVisibieView(sapaApp) == true)
					{
						MainService.this.mActivity.get().onMinVolume();
						showVolume(currentVolume, sapaApp);
					}
				}
				else
				{
					if (isVisibieView(sapaApp) == true)
					{
						MainService.this.mActivity.get().onBetweenVolume();
						showVolume(currentVolume, sapaApp);
					}
				}
				try
				{
					// Setting current configuration in AppInfo
					setConfiguration(sapaApp, currentVolume);
					// Notification that AppInfo has been changed is sent.
					this.mSapaAppService.changeAppInfo(mAppInfoList[sapaApp.InstanceId]);
				}
				catch (SapaConnectionNotSetException)
				{
					Log.w(TAG, "State of application could not be changed due to connection exception");
				}
				// Volume command is sent to native part.
				Logic.sendVolume(this.mProcessorList[sapaApp.InstanceId], currentVolume);
        
			}
		}

		internal virtual void increaseVolume(SapaApp sapaApp)
		{
			lock (this)
			{
				if (isMinVolume(sapaApp))
				{
					modifyAction(sapaApp, MainActivity.VOLUME_DOWN, R.drawable.ctrl_btn_volume_down_default, true);
				}
				int currentVolume = getCurrectVolume(sapaApp);
				currentVolume = Logic.increaseVolume(currentVolume);
        
				if (Logic.isMaxVolume(currentVolume))
				{
					modifyAction(sapaApp, MainActivity.VOLUME_UP, R.drawable.ctrl_btn_volume_up_disabled, false);
					if (isVisibieView(sapaApp) == true)
					{
						MainService.this.mActivity.get().onMaxVolume();
						this.showVolume(currentVolume, sapaApp);
					}
				}
				else
				{
					if (isVisibieView(sapaApp) == true)
					{
						MainService.this.mActivity.get().onBetweenVolume();
						this.showVolume(currentVolume, sapaApp);
					}
				}
				try
				{
					// Setting current configuration in AppInfo
					setConfiguration(sapaApp, currentVolume);
					// Notification that AppInfo has been changed is sent.
					this.mSapaAppService.changeAppInfo(mAppInfoList[sapaApp.InstanceId]);
				}
				catch (SapaConnectionNotSetException)
				{
					Log.w(TAG, "State of application could not be changed due to connection exception");
				}
				// Volume command is sent to native part.
				Logic.sendVolume(this.mProcessorList[sapaApp.InstanceId], currentVolume);
			}
		}

		/// <summary>
		/// This method saves current configuration in AppInfo of this application
		/// 
		/// </summary>
		private void setConfiguration(SapaApp sapaApp, int volume)
		{
			Bundle bundle = new Bundle();
			bundle.putInt("CONFIG", volume);
			SapaAppInfo info = this.mAppInfoList[sapaApp.InstanceId];
			Log.d(TAG, "kaka volume[" + volume + "] was set in " + sapaApp.InstanceId);
			info.Configuration = bundle;
		}

		internal virtual SapaAppService SapaAppService
		{
			get
			{
				return this.mSapaAppService;
			}
		}

		internal readonly Handler mHandler = new Handler();

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void showVolume(final int volume, com.samsung.android.sdk.professionalaudio.app.SapaApp sapaApp)
		private void showVolume(int volume, SapaApp sapaApp)
		{
			mHandler.post(() =>
			{
				// Toast.makeText(MainService.this, volume + " dB", Toast.LENGTH_SHORT).show();
				if (mActivity != null && mActivity.get() != null)
				{
					try
					{
						mActivity.get().updateVolumeTextView(volume + " dB");
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
			});
		}

		public virtual string getVolumeText(SapaAppInfo info)
		{
			// update the volume
			if (info == null)
			{
				return "N/A";
			}
			return ("" + getCurrectVolume(info.App) + " dB");
		}

		private void modifyAction(SapaApp sapaApp, string actionKey, int iconResId, bool isEnabled)
		{

			if (this.mAppInfoList[sapaApp.InstanceId] != null && mSapaAppService != null)
			{
				// Obtaining current SapaActionInfo.
				SapaActionInfo action = this.mAppInfoList[sapaApp.InstanceId].getActionInfo(actionKey);
				if (action != null)
				{
					// Settling state.
					action.Enabled = isEnabled;
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
			// MainService needs to implement AudioServiceConnectListener to be able to listen for this
			// event.
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
							// Close this instance. because the caller is closed by unknow reason. So,
							// It will be not used anymore.
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
			// Nothing to do
		}
	}

}