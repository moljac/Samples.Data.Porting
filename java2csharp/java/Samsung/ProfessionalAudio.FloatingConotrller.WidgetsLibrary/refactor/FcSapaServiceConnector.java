package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.content.Intent;
import android.os.Bundle;
import android.util.Log;

import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
import com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
import com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;
import com.samsung.android.sdk.professionalaudio.app.SapaUndeclaredActionException;

import java.util.List;

/**
 * @brief Class used as communication layer between the SapaAppService and Model.
 */
public class FcSapaServiceConnector implements SapaAppStateListener, SapaServiceConnectListener,
        FcSapaActionDispatcher {

    public static final String TAG = FcSapaServiceConnector.class.getName();

    /** @brief key used for acquiring track_order of active applications from the MainApp bundle */
    public static final String TRACK_ORDER_KEY = "track_order";

    /** @brief key used to determine which applications are freeze, it is store in the MainApp bundle */
    public static final String FREEZE_APPS_KEY = "freeze_apps";

    /** @brief used to communicate with the SapaAppService */
    private SapaAppService mSapaAppService;

    /** @brief represents main (host) application */
    private SapaAppInfo mMainApp;

    /** @brief represents main (host) application's package name */
    private String mMainAppPackageName;

    /** @brief Model instance which stores info about current FloatingController state */
    private FcSapaModel mFcSapaModel;

    /**
     * Constructs the connector object with needed info about SapaAppService and
     * the package name of main application.
     *
     * @param sapaAppService reference to SapaAppService
     * @param mainAppPackageName package name for main (host) application
     * @param sapaModel model containing current state of FloatingController
     */
    FcSapaServiceConnector(FcSapaModel sapaModel, SapaAppService sapaAppService, String mainAppPackageName) {
        mFcSapaModel = sapaModel;
        setSapaAppService(sapaAppService, mainAppPackageName);
    }

    /**
     * @brief TODO
     *
     * @return
     */
    public SapaAppService getSapaAppService() {
        return mSapaAppService;
    }

    /**
     * @brief Get intent that started application of given instanceId
     *
     * TODO: Describe catched exceptions
     *
     * @param instanceId    unique instance id
     *
     * @return Intent object or null
     */
    public Intent getLaunchIntent(String instanceId) {
        SapaAppInfo appInfo = getAppInfo(instanceId);
        if (appInfo == null) {
            Log.w(TAG, "Cannot start activity for application: " + instanceId
                    + ": app not active");
            return null;
        }

        SapaAppService sapaAppService = getSapaAppService();
        Intent intent = null;
        try {
            intent = sapaAppService.getLaunchIntent(appInfo.getApp());

        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "Cannot start activity for application " + instanceId
                    + ": sapa connection has not been set");

        } catch (IllegalAccessException e) {
            Log.w(TAG, "Cannot start activity for application " + instanceId
                    + ": illegal access");
        }

        return intent;
    }

    private void setSapaAppService(SapaAppService sapaAppService, String mainAppPackageName) {
        Log.d(TAG, "Setting sapa app service with main: " + mainAppPackageName);
        // unregister listeners from previous one
        removeSapaListeners();

        mSapaAppService = sapaAppService;
        mMainAppPackageName = mainAppPackageName;
        mMainApp = findMainApp(sapaAppService, mainAppPackageName);

        // register listeners for the new one
        addSapaListeners();
    }

    private static SapaAppInfo findMainApp(SapaAppService sapaAppService, String mainAppPackageName) {
        if (sapaAppService != null) {
            List<SapaAppInfo> activeApps = null;
            try {
                activeApps = sapaAppService.getAllActiveApp();
                if (activeApps != null) {
                    for (SapaAppInfo appInfo : activeApps) {
                        if (mainAppPackageName.equals(appInfo.getApp().getPackageName())) {
                            return appInfo;
                        }
                    }
                }
            } catch (SapaConnectionNotSetException e) {
                Log.d(TAG, e.getMessage());
            }
        }
        return null;
    }

    private void addSapaListeners() {
        if (mSapaAppService != null) {
            mSapaAppService.addAppStateListener(this);
            mSapaAppService.addConnectionListener(this);
        }
    }

    private void removeSapaListeners() {
        if (mSapaAppService != null) {
            mSapaAppService.removeAppStateListener(this);
            mSapaAppService.removeConnectionListener(this);
        }
    }

    /**
     * @brief Initialize Floating Controller's model by setting it's host application,
     *        adding active Sapa applications and passing an ordering to it.
     */
    public void initializeModel() {
        Log.d(TAG, "Initializing model");
        sendActiveAppsToModel();
        sendOrderToModel();
        sendFreezeToModel();
    }

    private void sendFreezeToModel() {
        if (null == mMainApp) {
            Log.d(TAG, "Cannot send track order to the model: main application not set");
            return;
        }

        Bundle bundle = mMainApp.getConfiguration();
        if (null != bundle) {
            List<String> freezeList = bundle.getStringArrayList(FREEZE_APPS_KEY);
            if (null != freezeList) {
                Log.d(TAG, "FloatingController: found support for freeze track");
                mFcSapaModel.setFreezeApps(freezeList);
            } else {
                Log.d(TAG, "FloatingController: support for track ordering not found");
            }
        }
    }

    private void sendActiveAppsToModel() {
        if (mSapaAppService == null) {
            Log.w(TAG, "Cannot send active apps to model: sapa app service is null");
            return;
        }

        try {
            List<SapaAppInfo> activeApps = mSapaAppService.getAllActiveApp();
            if (activeApps != null) {
                for (SapaAppInfo appInfo : activeApps) {
                    mFcSapaModel.insertApp(appInfo, getAppType(appInfo.getApp()));
                }
            }
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "Cannot send active apps to model: sapa connection not set");
        }
    }

    private void sendOrderToModel() {
        if (null == mMainApp) {
            Log.d(TAG, "Cannot send track order to the model: main application not set");
            return;
        }

        Log.d(TAG, "sendOrderToModel: Main app = " + mMainApp.getApp().getInstanceId());

        Bundle bundle = mMainApp.getConfiguration();
        if (null != bundle) {
            List<String> orderingList = bundle.getStringArrayList(TRACK_ORDER_KEY);
            if (null != orderingList) {
                Log.d(TAG, "FloatingController: found support for track ordering");
                mFcSapaModel.orderApps(orderingList);
            } else {
                Log.d(TAG, "FloatingController: support for track ordering not found");
            }
        }
    }

    @Override
    public void onAppInstalled(SapaApp sapaApp) {
        checkMainApp();
    }

    @Override
    public void onAppUninstalled(SapaApp sapaApp) {
        // Assumption: onAppDeactivated should be called
    }

    @Override
    public void onAppActivated(SapaApp sapaApp) {
        Log.d(TAG, "Activated app: " + sapaApp.getInstanceId());
        try {
            mFcSapaModel.insertApp(mSapaAppService.getActiveApp(sapaApp), getAppType(sapaApp));

        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "Cannot insert app: sapa connection not set");
        }
    }

    @Override
    public void onAppDeactivated(SapaApp sapaApp) {
        Log.d(TAG, "Deactivated app: " + sapaApp.getInstanceId());
        mFcSapaModel.removeApp(sapaApp.getInstanceId(), getAppType(sapaApp));
    }

    @Override
    public void onAppChanged(SapaApp sapaApp) {
        Log.d(TAG, "Changed app: " + sapaApp.getInstanceId());
        try {
            mFcSapaModel.updateApp(mSapaAppService.getActiveApp(sapaApp), getAppType(sapaApp));

        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "Cannot update app: sapa connection not set");
        }
    }

    @Override
    public void onTransportMasterChanged(SapaApp sapaApp) {
        // NO-OP
    }

    @Override
    public void onServiceConnected() {
        Log.d(TAG, "Sapa service has been connected");
        checkMainApp();
        initializeModel();
    }

    @Override
    public void onServiceDisconnected() {
        Log.d(TAG, "Sapa service has been disconnected");
        mFcSapaModel.clear();
    }

    @Override
    public void callAction(String instanceId, String actionId) {
        Log.d(TAG, "Calling remote action (id: " + actionId + ", instance: " + instanceId + ")");
        SapaAppInfo info = getAppInfo(instanceId);
        if (null != info) {
            try {
                mSapaAppService.callAction(info.getApp(), actionId);

            } catch (SapaConnectionNotSetException e) {
                Log.d(TAG, "Cannot call action (id: " + actionId + ") for instance "
                        + instanceId + ": Sapa connection not set");

            } catch (SapaUndeclaredActionException e) {
                Log.d(TAG, "Cannot call action (id: " + actionId + ") for instance "
                        + instanceId + ": action undeclared");
            }
        } else {
            Log.w(TAG, "Cannot call action (id: " + actionId + ") for instance "
                    + instanceId + ": SapaApp not found");
        }
    }

    private void checkMainApp() {
        if (mSapaAppService == null
                || mMainAppPackageName == null
                || mMainAppPackageName.isEmpty()) {
            return;
        }
        try {
            for (SapaAppInfo appInfo : mSapaAppService.getAllActiveApp()) {
                if (appInfo.getPackageName().equals(mMainAppPackageName)
                        && appInfo.isMultiInstanceEnabled()) {
                    throw new IllegalStateException("Only single instance application can be " +
                            "declared as main for Floating Controler");
                }
            }
        } catch (SapaConnectionNotSetException e) {
            Log.d(TAG, e.getMessage());
        }
    }

    private int getAppType(SapaApp sapaApp) {
        return isMainApp(sapaApp) ? FcConstants.APP_TYPE_MAIN : FcConstants.APP_TYPE_ORDINAL;
    }

    /**
     * @brief Find SapaAppInfo structure for a given instanceId from list of active apps
     *
     * @param instanceId    unique instance identifier
     *
     * @return SapaAppInfo of a given instanceId or null if not found
     */
    private SapaAppInfo getAppInfo(String instanceId) {
        SapaAppInfo found = null;
        try {
            for (SapaAppInfo appInfo : mSapaAppService.getAllActiveApp()) {
                if (appInfo.getApp().getInstanceId().equals(instanceId)) {
                    found = appInfo;
                    break;
                }
            }
        } catch (SapaConnectionNotSetException e) {
            Log.e(TAG, "");
        }

        return found;
    }

    /**
     * @brief
     *
     * @return
     */
    public SapaAppInfo getMainApp() {
        return mMainApp;
    }


    private boolean isMainApp(SapaApp sapaApp) {
        return mMainAppPackageName.equals(sapaApp.getPackageName());
    }

}
