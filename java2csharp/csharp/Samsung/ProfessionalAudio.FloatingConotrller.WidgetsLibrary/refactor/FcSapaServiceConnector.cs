using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;

	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using SapaAppStateListener = com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
	using SapaConnectionNotSetException = com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
	using SapaServiceConnectListener = com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;
	using SapaUndeclaredActionException = com.samsung.android.sdk.professionalaudio.app.SapaUndeclaredActionException;

	/// <summary>
	/// @brief Class used as communication layer between the SapaAppService and Model.
	/// </summary>
	public class FcSapaServiceConnector : SapaAppStateListener, SapaServiceConnectListener, FcSapaActionDispatcher
	{

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		public static readonly string TAG = typeof(FcSapaServiceConnector).FullName;

		/// <summary>
		/// @brief key used for acquiring track_order of active applications from the MainApp bundle </summary>
		public const string TRACK_ORDER_KEY = "track_order";

		/// <summary>
		/// @brief key used to determine which applications are freeze, it is store in the MainApp bundle </summary>
		public const string FREEZE_APPS_KEY = "freeze_apps";

		/// <summary>
		/// @brief used to communicate with the SapaAppService </summary>
		private SapaAppService mSapaAppService;

		/// <summary>
		/// @brief represents main (host) application </summary>
		private SapaAppInfo mMainApp;

		/// <summary>
		/// @brief represents main (host) application's package name </summary>
		private string mMainAppPackageName;

		/// <summary>
		/// @brief Model instance which stores info about current FloatingController state </summary>
		private FcSapaModel mFcSapaModel;

		/// <summary>
		/// Constructs the connector object with needed info about SapaAppService and
		/// the package name of main application.
		/// </summary>
		/// <param name="sapaAppService"> reference to SapaAppService </param>
		/// <param name="mainAppPackageName"> package name for main (host) application </param>
		/// <param name="sapaModel"> model containing current state of FloatingController </param>
		internal FcSapaServiceConnector(FcSapaModel sapaModel, SapaAppService sapaAppService, string mainAppPackageName)
		{
			mFcSapaModel = sapaModel;
			setSapaAppService(sapaAppService, mainAppPackageName);
		}

		/// <summary>
		/// @brief TODO
		/// 
		/// @return
		/// </summary>
		public virtual SapaAppService SapaAppService
		{
			get
			{
				return mSapaAppService;
			}
		}

		/// <summary>
		/// @brief Get intent that started application of given instanceId
		/// 
		/// TODO: Describe catched exceptions
		/// </summary>
		/// <param name="instanceId">    unique instance id
		/// </param>
		/// <returns> Intent object or null </returns>
		public virtual Intent getLaunchIntent(string instanceId)
		{
			SapaAppInfo appInfo = getAppInfo(instanceId);
			if (appInfo == null)
			{
				Log.w(TAG, "Cannot start activity for application: " + instanceId + ": app not active");
				return null;
			}

			SapaAppService sapaAppService = SapaAppService;
			Intent intent = null;
			try
			{
				intent = sapaAppService.getLaunchIntent(appInfo.App);

			}
			catch (SapaConnectionNotSetException)
			{
				Log.w(TAG, "Cannot start activity for application " + instanceId + ": sapa connection has not been set");

			}
			catch (IllegalAccessException)
			{
				Log.w(TAG, "Cannot start activity for application " + instanceId + ": illegal access");
			}

			return intent;
		}

		private void setSapaAppService(SapaAppService sapaAppService, string mainAppPackageName)
		{
			Log.d(TAG, "Setting sapa app service with main: " + mainAppPackageName);
			// unregister listeners from previous one
			removeSapaListeners();

			mSapaAppService = sapaAppService;
			mMainAppPackageName = mainAppPackageName;
			mMainApp = findMainApp(sapaAppService, mainAppPackageName);

			// register listeners for the new one
			addSapaListeners();
		}

		private static SapaAppInfo findMainApp(SapaAppService sapaAppService, string mainAppPackageName)
		{
			if (sapaAppService != null)
			{
				IList<SapaAppInfo> activeApps = null;
				try
				{
					activeApps = sapaAppService.AllActiveApp;
					if (activeApps != null)
					{
						foreach (SapaAppInfo appInfo in activeApps)
						{
							if (mainAppPackageName.Equals(appInfo.App.PackageName))
							{
								return appInfo;
							}
						}
					}
				}
				catch (SapaConnectionNotSetException e)
				{
					Log.d(TAG, e.Message);
				}
			}
			return null;
		}

		private void addSapaListeners()
		{
			if (mSapaAppService != null)
			{
				mSapaAppService.addAppStateListener(this);
				mSapaAppService.addConnectionListener(this);
			}
		}

		private void removeSapaListeners()
		{
			if (mSapaAppService != null)
			{
				mSapaAppService.removeAppStateListener(this);
				mSapaAppService.removeConnectionListener(this);
			}
		}

		/// <summary>
		/// @brief Initialize Floating Controller's model by setting it's host application,
		///        adding active Sapa applications and passing an ordering to it.
		/// </summary>
		public virtual void initializeModel()
		{
			Log.d(TAG, "Initializing model");
			sendActiveAppsToModel();
			sendOrderToModel();
			sendFreezeToModel();
		}

		private void sendFreezeToModel()
		{
			if (null == mMainApp)
			{
				Log.d(TAG, "Cannot send track order to the model: main application not set");
				return;
			}

			Bundle bundle = mMainApp.Configuration;
			if (null != bundle)
			{
				IList<string> freezeList = bundle.getStringArrayList(FREEZE_APPS_KEY);
				if (null != freezeList)
				{
					Log.d(TAG, "FloatingController: found support for freeze track");
					mFcSapaModel.FreezeApps = freezeList;
				}
				else
				{
					Log.d(TAG, "FloatingController: support for track ordering not found");
				}
			}
		}

		private void sendActiveAppsToModel()
		{
			if (mSapaAppService == null)
			{
				Log.w(TAG, "Cannot send active apps to model: sapa app service is null");
				return;
			}

			try
			{
				IList<SapaAppInfo> activeApps = mSapaAppService.AllActiveApp;
				if (activeApps != null)
				{
					foreach (SapaAppInfo appInfo in activeApps)
					{
						mFcSapaModel.insertApp(appInfo, getAppType(appInfo.App));
					}
				}
			}
			catch (SapaConnectionNotSetException)
			{
				Log.w(TAG, "Cannot send active apps to model: sapa connection not set");
			}
		}

		private void sendOrderToModel()
		{
			if (null == mMainApp)
			{
				Log.d(TAG, "Cannot send track order to the model: main application not set");
				return;
			}

			Log.d(TAG, "sendOrderToModel: Main app = " + mMainApp.App.InstanceId);

			Bundle bundle = mMainApp.Configuration;
			if (null != bundle)
			{
				IList<string> orderingList = bundle.getStringArrayList(TRACK_ORDER_KEY);
				if (null != orderingList)
				{
					Log.d(TAG, "FloatingController: found support for track ordering");
					mFcSapaModel.orderApps(orderingList);
				}
				else
				{
					Log.d(TAG, "FloatingController: support for track ordering not found");
				}
			}
		}

		public override void onAppInstalled(SapaApp sapaApp)
		{
			checkMainApp();
		}

		public override void onAppUninstalled(SapaApp sapaApp)
		{
			// Assumption: onAppDeactivated should be called
		}

		public override void onAppActivated(SapaApp sapaApp)
		{
			Log.d(TAG, "Activated app: " + sapaApp.InstanceId);
			try
			{
				mFcSapaModel.insertApp(mSapaAppService.getActiveApp(sapaApp), getAppType(sapaApp));

			}
			catch (SapaConnectionNotSetException)
			{
				Log.w(TAG, "Cannot insert app: sapa connection not set");
			}
		}

		public override void onAppDeactivated(SapaApp sapaApp)
		{
			Log.d(TAG, "Deactivated app: " + sapaApp.InstanceId);
			mFcSapaModel.removeApp(sapaApp.InstanceId, getAppType(sapaApp));
		}

		public override void onAppChanged(SapaApp sapaApp)
		{
			Log.d(TAG, "Changed app: " + sapaApp.InstanceId);
			try
			{
				mFcSapaModel.updateApp(mSapaAppService.getActiveApp(sapaApp), getAppType(sapaApp));

			}
			catch (SapaConnectionNotSetException)
			{
				Log.w(TAG, "Cannot update app: sapa connection not set");
			}
		}

		public override void onTransportMasterChanged(SapaApp sapaApp)
		{
			// NO-OP
		}

		public override void onServiceConnected()
		{
			Log.d(TAG, "Sapa service has been connected");
			checkMainApp();
			initializeModel();
		}

		public override void onServiceDisconnected()
		{
			Log.d(TAG, "Sapa service has been disconnected");
			mFcSapaModel.clear();
		}

		public virtual void callAction(string instanceId, string actionId)
		{
			Log.d(TAG, "Calling remote action (id: " + actionId + ", instance: " + instanceId + ")");
			SapaAppInfo info = getAppInfo(instanceId);
			if (null != info)
			{
				try
				{
					mSapaAppService.callAction(info.App, actionId);

				}
				catch (SapaConnectionNotSetException)
				{
					Log.d(TAG, "Cannot call action (id: " + actionId + ") for instance " + instanceId + ": Sapa connection not set");

				}
				catch (SapaUndeclaredActionException)
				{
					Log.d(TAG, "Cannot call action (id: " + actionId + ") for instance " + instanceId + ": action undeclared");
				}
			}
			else
			{
				Log.w(TAG, "Cannot call action (id: " + actionId + ") for instance " + instanceId + ": SapaApp not found");
			}
		}

		private void checkMainApp()
		{
			if (mSapaAppService == null || mMainAppPackageName == null || mMainAppPackageName.Length == 0)
			{
				return;
			}
			try
			{
				foreach (SapaAppInfo appInfo in mSapaAppService.AllActiveApp)
				{
					if (appInfo.PackageName.Equals(mMainAppPackageName) && appInfo.MultiInstanceEnabled)
					{
						throw new System.InvalidOperationException("Only single instance application can be " + "declared as main for Floating Controler");
					}
				}
			}
			catch (SapaConnectionNotSetException e)
			{
				Log.d(TAG, e.Message);
			}
		}

		private int getAppType(SapaApp sapaApp)
		{
			return isMainApp(sapaApp) ? FcConstants.APP_TYPE_MAIN : FcConstants.APP_TYPE_ORDINAL;
		}

		/// <summary>
		/// @brief Find SapaAppInfo structure for a given instanceId from list of active apps
		/// </summary>
		/// <param name="instanceId">    unique instance identifier
		/// </param>
		/// <returns> SapaAppInfo of a given instanceId or null if not found </returns>
		private SapaAppInfo getAppInfo(string instanceId)
		{
			SapaAppInfo found = null;
			try
			{
				foreach (SapaAppInfo appInfo in mSapaAppService.AllActiveApp)
				{
					if (appInfo.App.InstanceId.Equals(instanceId))
					{
						found = appInfo;
						break;
					}
				}
			}
			catch (SapaConnectionNotSetException e)
			{
				Log.e(TAG, "");
			}

			return found;
		}

		/// <summary>
		/// @brief
		/// 
		/// @return
		/// </summary>
		public virtual SapaAppInfo MainApp
		{
			get
			{
				return mMainApp;
			}
		}


		private bool isMainApp(SapaApp sapaApp)
		{
			return mMainAppPackageName.Equals(sapaApp.PackageName);
		}

	}

}