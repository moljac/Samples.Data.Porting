package com.samsung.audiosuite.sapaeffectsample;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.util.Log;
import android.util.SparseArray;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaService;
import com.samsung.android.sdk.professionalaudio.app.SapaActionDefinerInterface;
import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
import com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
import com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;

import java.lang.ref.WeakReference;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Queue;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

public class MainService extends Service implements SapaServiceConnectListener,
        SapaActionDefinerInterface {

    private static final String TAG = "audiosuite:sapaeffectsample:j:MainService";

    // Binder for activity
    private LocalBinder mBinder;

    private SapaAppService mSapaAppService;

    // SapaAppInfo package representing this application
    HashMap<String, SapaAppInfo> mAppInfoList;

    HashMap<String, SapaProcessor> mProcessorList;

    HashMap<String, ActionsPack> mActionsList;

    private Map<String, String> mCallerPackageNameList;

    // Reference to the activity of application
    private WeakReference<MainActivity> mActivity;

    private SapaService mSapaService;

    SparseArray<SapaActionInfo> mActionArray;

    private Queue<Intent> mDelayedIntents = new LinkedList<Intent>();

    private boolean mServiceConnected;

    @Override
    public void onCreate() {
        Log.d(TAG, "onCreate");
        super.onCreate();
        this.mBinder = new LocalBinder();
        this.mServiceConnected = false;
        this.mActionsList = new HashMap<String, MainService.ActionsPack>();
        this.mProcessorList = new HashMap<String, SapaProcessor>();
        this.mAppInfoList = new HashMap<String, SapaAppInfo>();
        this.mCallerPackageNameList = new ConcurrentHashMap<String, String>();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
        if (intent != null)

        if (this.mServiceConnected) {
            handleIntent(intent);
        } else {
            this.mDelayedIntents.add(intent);
        }

        // Connection to remote service of ProfessionalAudio system is set.
        if (this.mSapaAppService == null) {
            connectConnectionBridge();
        }

        return super.onStartCommand(intent, flags, startId);
    }

    /*
     * This method is responsible for handling received intent. It can only be called when
     * connection is set.
     */
    private void handleIntent(Intent intent) {
        int state = SapaAppInfo.getState(intent);
        switch (state) {
            case SapaAppInfo.STATE_ACTIVATING:
                this.handleActivationIntent(intent);
                break;
            case SapaAppInfo.STATE_DEACTIVATING:
                this.handleDeactivationIntent(intent);

                break;
        }
    }

    private void handleActivationIntent(Intent intent) {
        // When application is activated from ProfessionalAudio system it
        // receives SapaAppInfo object describing it.
        // To obtain this object static method getSapaAppInfo() is to be used.
        SapaAppInfo info = SapaAppInfo.getAppInfo(intent);

        if (info != null) {
            String callerPackageName = intent
                    .getStringExtra("com.samsung.android.sdk.professionalaudio.key.callerpackagename");
            if (callerPackageName == null) {
                callerPackageName = new String("unknown");
            }
            mCallerPackageNameList.put(info.getApp().getInstanceId(), callerPackageName);
            Log.d(TAG, "info.getApp.getInstanceId() " + info.getApp().getInstanceId());
            if (!this.mAppInfoList.containsKey(info.getApp().getInstanceId())) {
                this.mAppInfoList.put(info.getApp().getInstanceId(), info);
            }
            try {
                if (info.getConfiguration() == null) {
                    Log.d(TAG, "kaka, not exist configuration, volume[" + Logic.DEFAULT_VOLUME
                            + "] will be set in " + info.getApp().getInstanceId());
                    this.setConfiguration(info.getApp(), Logic.DEFAULT_VOLUME);

                } else {
                    Log.d(TAG, "kaka volume[" + info.getConfiguration().getInt("CONFIG")
                            + "] was got from " + info.getApp().getInstanceId());
                }
                // Actions are being set in SapaAppInfo representing this
                // application.
                mActionArray = new SparseArray<SapaActionInfo>();
                mActionArray.put(0, new SapaActionInfo(MainActivity.VOLUME_DOWN,
                        R.drawable.ctrl_btn_volume_down_default, this.getPackageName()));
                mActionArray.put(1, new SapaActionInfo(MainActivity.VOLUME_UP,
                        R.drawable.ctrl_btn_volume_up_default, this.getPackageName()));
                info.setActions(mActionArray);

                // Native part is being initialised.
                SapaProcessor sapaProcessor = new SapaProcessor(this, null,
                        new SapaProcessorStateListener(info.getApp()));
                this.mSapaService.register(sapaProcessor);
                sapaProcessor.activate();
                this.mProcessorList.put(info.getApp().getInstanceId(), sapaProcessor);

                Logic.sendVolume(sapaProcessor, this.getCurrectVolume(info.getApp()));

                // Information about ports is being set in SapaAppInfo
                // representing this app.
                // It can not be done before activating SapaProcessor.
                info.setPortFromSapaProcessor(sapaProcessor);

                // Application needs to declare that it was successfully
                // activated.
                Log.d(TAG, "addactiveapp " + info.getApp().getInstanceId());
                this.mSapaAppService.addActiveApp(info);
            } catch (SapaConnectionNotSetException e) {
                Log.e(TAG, "App could not be added to active as connection has not been made.");

            } catch (InstantiationException e) {
                Log.e(TAG, "SapaService can not be instantiate");
            }
        }
        if (mActionsList != null && info != null && info.getApp() != null) {
            this.mActionsList.put(info.getApp().getInstanceId(), new ActionsPack(info.getApp()));
        }
    }

    class SapaProcessorStateListener implements SapaProcessor.StatusListener {

        private SapaApp mSapaApp;

        public SapaProcessorStateListener(SapaApp sapaApp) {
            mSapaApp = sapaApp;
        }

        @Override
        public void onKilled() {
            Log.d(TAG, mSapaApp.getInstanceId() + " was killed.");

            try {
                mProcessorList.remove(mSapaApp.getInstanceId());
                mAppInfoList.remove(mSapaApp.getInstanceId());
                mActionsList.remove(mSapaApp.getInstanceId());

                // remove from active apps // This will notify to the Soundcamp.
                mSapaAppService.removeFromActiveApps(mSapaApp);

                if (mProcessorList.isEmpty()) {
                    MainService.this.stopSelf();
                }

            } catch (Exception e) {
                e.printStackTrace();
            }
        }

    }

    private synchronized void handleDeactivationIntent(Intent intent) {
        SapaApp sapaApp = SapaAppInfo.getApp(intent);
        this.mActionsList.remove(sapaApp.getInstanceId());
        for (Entry<String, SapaProcessor> entry : this.mProcessorList.entrySet()) {
            Log.d(TAG, "kakakaka " + entry.getKey() + " -> " + entry.getValue());
        }
        SapaProcessor processor = this.mProcessorList.get(sapaApp.getInstanceId());
        Log.d(TAG, "kakakaka instanceId " + sapaApp.getInstanceId() + " processor " + processor);
        processor.deactivate();
        this.mSapaService.unregister(this.mProcessorList.get(sapaApp.getInstanceId()));
        this.mProcessorList.remove(sapaApp.getInstanceId());
        try {
            this.mSapaAppService.removeFromActiveApps(sapaApp);
        } catch (SapaConnectionNotSetException e) {
            Log.e(TAG,
                    "Instance could not be removed from active list because of connection exception.");
        }
        this.mAppInfoList.remove(sapaApp.getInstanceId());
    }

    @Override
    public IBinder onBind(Intent intent) {
        Log.d(TAG, "onBind");
        // Binder for local activity.
        return this.mBinder;
    }

    @Override
    public void onDestroy() {
        Log.d(TAG, "onDestroy");

        // Native part of application is being deactivated.
        if (this.mSapaService != null && this.mProcessorList != null
                && !this.mProcessorList.isEmpty()) {
            for (SapaProcessor processor : this.mProcessorList.values()) {
                processor.deactivate();
                this.mSapaService.unregister(processor);
            }
        }

        if (this.mAppInfoList != null && !this.mAppInfoList.isEmpty()) {
            for (SapaAppInfo sapaAppInfo : this.mAppInfoList.values()) {
                if (sapaAppInfo.getApp() != null) {
                    try {
                        // Application needs to declare that its instance was
                        // successfully deactivated.
                        this.mSapaAppService.removeFromActiveApps(sapaAppInfo.getApp());
                    } catch (SapaConnectionNotSetException e) {
                        Log.e(TAG,
                                "Instance could not be removed from active list because of connection exception.");
                    }
                }
            }
        }

        // Action definer is being removed.
        this.mSapaAppService.removeActionDefiner();
        // Connection with remote service is finished.
        this.mSapaAppService.disconnect();

        super.onDestroy();
    }

    @Override
    public void onServiceConnected() {
        Log.d(TAG, "onServiceConnected");
        try {
            Sapa sapa = new Sapa();
            sapa.initialize(this);
            this.mSapaService = new SapaService();
            this.mServiceConnected = true;
        } catch (SsdkUnsupportedException e) {
            Log.e(TAG,
                    "Initialisation of Sapa is not possible as Sapa is not available on the device");
        } catch (InstantiationException e) {
            Log.e(TAG, "SapaService can not be instantiate");
        }
        if (this.mServiceConnected) {
            while (!mDelayedIntents.isEmpty()) {
                Intent intent = mDelayedIntents.poll();
                if (intent != null) {
                    handleIntent(intent);
                }
            }
        }
    }

    @Override
    public synchronized Runnable getActionDefinition(SapaApp sapaApp, String actionId) {
        // Basing on actionId appropriate action definition is returned.
        Log.d(TAG, "instanceid " + sapaApp.getInstanceId() + " actionId " + actionId);
        if (actionId.equals(MainActivity.VOLUME_DOWN)) {
            return this.mActionsList.get(sapaApp.getInstanceId()).getVolumeDownAction();
        } else if (actionId.equals(MainActivity.VOLUME_UP)) {
            return this.mActionsList.get(sapaApp.getInstanceId()).getVolumeUpAction();
        }
        return null;
    }

    class ActionsPack {

        private SapaApp mSapaApp;
        private Runnable mVolDown;
        private Runnable mVolUp;

        ActionsPack(SapaApp sapaApp) {
            this.mSapaApp = sapaApp;
            this.mVolDown = null;
            this.mVolUp = null;
        }

        Runnable getVolumeDownAction() {
            if (this.mVolDown == null) {
                setVolumeDown();
            }
            return this.mVolDown;
        }

        Runnable getVolumeUpAction() {
            if (this.mVolUp == null) {
                setVolumeUp();
            }
            return this.mVolUp;
        }

        private void setVolumeDown() {

            // Definition of volume down action
            this.mVolDown = new Runnable() {

                @Override
                public void run() {
                    // Here you write body of your action
                    // boolean wasMaxVolume = isMaxVolume(ActionsPack.this.mSapaApp);

                    try {
                        decreaseVolume(ActionsPack.this.mSapaApp);
                    } catch (NullPointerException e) {
                        ;
                    }

                }
            };
        }

        private synchronized void setVolumeUp() {

            // Definition of volume down action
            this.mVolUp = new Runnable() {

                @Override
                public void run() {
                    // Here you write body of your action

                    try {
                        increaseVolume(ActionsPack.this.mSapaApp);
                    } catch (NullPointerException e) {
                        ;
                    }

                }
            };
        }
    }

    private int getCurrectVolume(SapaApp sapaApp) {
        Bundle bundle = this.mAppInfoList.get(sapaApp.getInstanceId()).getConfiguration();
        if (bundle != null) {
            return bundle.getInt("CONFIG");
        }
        return -1;
    }

    boolean isMinVolume(SapaApp sapaApp) {
        return Logic.isMinVolume(this.getCurrectVolume(sapaApp));
    }

    boolean isMaxVolume(SapaApp sapaApp) {
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

    private boolean isVisibieView(SapaApp sapaApp) {
        if (mActivity != null
                && mActivity.get() != null
                && mActivity.get().getVisibleApp() != null
                && mActivity.get().getVisibleApp().getApp() != null
                && mActivity.get().getVisibleApp().getApp().getInstanceId() != null
                && sapaApp != null
                && mActivity.get().getVisibleApp().getApp().getInstanceId()
                        .equals(sapaApp.getInstanceId()) == true) {
            return true;
        } else {
            return false;
        }
    }

    synchronized void decreaseVolume(SapaApp sapaApp) {
        if (isMaxVolume(sapaApp)) {
            modifyAction(sapaApp, MainActivity.VOLUME_UP, R.drawable.ctrl_btn_volume_up_default,
                    true);
        }
        int currentVolume = getCurrectVolume(sapaApp);
        currentVolume = Logic.decreaseVolume(currentVolume);

        if (Logic.isMinVolume(currentVolume)) {
            modifyAction(sapaApp, MainActivity.VOLUME_DOWN,
                    R.drawable.ctrl_btn_volume_down_disabled, false);
            if (isVisibieView(sapaApp) == true) {
                MainService.this.mActivity.get().onMinVolume();
                showVolume(currentVolume, sapaApp);
            }
        } else {
            if (isVisibieView(sapaApp) == true) {
                MainService.this.mActivity.get().onBetweenVolume();
                showVolume(currentVolume, sapaApp);
            }
        }
        try {
            // Setting current configuration in AppInfo
            setConfiguration(sapaApp, currentVolume);
            // Notification that AppInfo has been changed is sent.
            this.mSapaAppService.changeAppInfo(mAppInfoList.get(sapaApp.getInstanceId()));
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "State of application could not be changed due to connection exception");
        }
        // Volume command is sent to native part.
        Logic.sendVolume(this.mProcessorList.get(sapaApp.getInstanceId()), currentVolume);

    }

    synchronized void increaseVolume(SapaApp sapaApp) {
        if (isMinVolume(sapaApp)) {
            modifyAction(sapaApp, MainActivity.VOLUME_DOWN,
                    R.drawable.ctrl_btn_volume_down_default, true);
        }
        int currentVolume = getCurrectVolume(sapaApp);
        currentVolume = Logic.increaseVolume(currentVolume);

        if (Logic.isMaxVolume(currentVolume)) {
            modifyAction(sapaApp, MainActivity.VOLUME_UP, R.drawable.ctrl_btn_volume_up_disabled,
                    false);
            if (isVisibieView(sapaApp) == true) {
                MainService.this.mActivity.get().onMaxVolume();
                this.showVolume(currentVolume, sapaApp);
            }
        } else {
            if (isVisibieView(sapaApp) == true) {
                MainService.this.mActivity.get().onBetweenVolume();
                this.showVolume(currentVolume, sapaApp);
            }
        }
        try {
            // Setting current configuration in AppInfo
            setConfiguration(sapaApp, currentVolume);
            // Notification that AppInfo has been changed is sent.
            this.mSapaAppService.changeAppInfo(mAppInfoList.get(sapaApp.getInstanceId()));
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "State of application could not be changed due to connection exception");
        }
        // Volume command is sent to native part.
        Logic.sendVolume(this.mProcessorList.get(sapaApp.getInstanceId()), currentVolume);
    }

    /**
     * This method saves current configuration in AppInfo of this application
     * */
    private void setConfiguration(SapaApp sapaApp, int volume) {
        Bundle bundle = new Bundle();
        bundle.putInt("CONFIG", volume);
        SapaAppInfo info = this.mAppInfoList.get(sapaApp.getInstanceId());
        Log.d(TAG, "kaka volume[" + volume + "] was set in " + sapaApp.getInstanceId());
        info.setConfiguration(bundle);
    }

    SapaAppService getSapaAppService() {
        return this.mSapaAppService;
    }

    final Handler mHandler = new Handler();

    private void showVolume(final int volume, SapaApp sapaApp) {
        mHandler.post(new Runnable() {
            @Override
            public void run() {
                // Toast.makeText(MainService.this, volume + " dB", Toast.LENGTH_SHORT).show();
                if (mActivity != null && mActivity.get() != null) {
                    try {
                        mActivity.get().updateVolumeTextView(volume + " dB");
                    } catch (NullPointerException e) {
                        ;
                    }
                }
            }
        });
    }

    public String getVolumeText(SapaAppInfo info) {
        // update the volume
        if (info == null) {
            return "N/A";
        }
        return ("" + getCurrectVolume(info.getApp()) + " dB");
    }

    private void modifyAction(SapaApp sapaApp, String actionKey, int iconResId, boolean isEnabled) {

        if (this.mAppInfoList.get(sapaApp.getInstanceId()) != null && mSapaAppService != null) {
            // Obtaining current SapaActionInfo.
            SapaActionInfo action = this.mAppInfoList.get(sapaApp.getInstanceId()).getActionInfo(
                    actionKey);
            if (action != null) {
                // Settling state.
                action.setEnabled(isEnabled);
                // Settling icon of action by setting id of the resource.
                action.setIcon(iconResId);
            }
        }
    }

    private void connectConnectionBridge() {
        Log.d(TAG, "Connect using bridge");
        this.mSapaAppService = new SapaAppService(this);

        // MainService starts listening for establishment of connection.
        // MainService needs to implement AudioServiceConnectListener to be able to listen for this
        // event.
        // When event occurs onServiceConnected() method is called.
        this.mSapaAppService.addConnectionListener(this);

        // MainService declares that it defines actions for this application.
        // MainService needs to implement AudioActionDefinerInterface.
        this.mSapaAppService.setActionDefiner(this);

        this.mSapaAppService.addAppStateListener(mSapaAppStateListener);

        // Connection to AudioConnectionService starts being created.
        this.mSapaAppService.connect();
    }

    private SapaAppStateListener mSapaAppStateListener = new SapaAppStateListener() {

        @Override
        public void onTransportMasterChanged(SapaApp arg0) {
            // TODO Auto-generated method stub

        }

        @Override
        public void onAppUninstalled(SapaApp arg0) {
            // TODO Auto-generated method stub

        }

        @Override
        public void onAppInstalled(SapaApp arg0) {
            // TODO Auto-generated method stub

        }

        @Override
        public void onAppDeactivated(SapaApp sapaApp) {
            Log.d(TAG, "onAppDeactivated: " + sapaApp.getInstanceId() + " was deactivated.");

            if (!mCallerPackageNameList.isEmpty()) {
                Set<Entry<String, String>> entrySet = mCallerPackageNameList.entrySet();
                Iterator<Entry<String, String>> iterator = entrySet.iterator();
                while (iterator.hasNext()) {
                    Map.Entry<String, String> entry = iterator.next();

                    if (sapaApp.getPackageName().contentEquals(entry.getValue())) {
                        Log.d(TAG, "Caller is closed.");
                        // Close this instance. because the caller is closed by unknow reason. So,
                        // It will be not used anymore.
                        closeAnInstance(entry.getKey());
                    }
                }
            }

            if (mCallerPackageNameList.isEmpty()) {
                stopSelf();
            }

        }

        @Override
        public void onAppChanged(SapaApp arg0) {
        }

        @Override
        public void onAppActivated(SapaApp arg0) {
            // TODO Auto-generated method stub

        }
    };

    private void closeAnInstance(String instanceId) {
        // Native part of application is being deactivated.
        if (this.mSapaService != null && this.mProcessorList != null
                && !this.mProcessorList.isEmpty()) {
            SapaProcessor processor = this.mProcessorList.get(instanceId);
            if (processor != null) {
                processor.deactivate();
                mSapaService.unregister(processor);
                mProcessorList.remove(instanceId);
            }
        }

        if (this.mAppInfoList != null && !this.mAppInfoList.isEmpty()) {
            SapaAppInfo sapaAppInfo = this.mAppInfoList.get(instanceId);
            if (sapaAppInfo != null && sapaAppInfo.getApp() != null) {
                try {
                    // Application needs to declare that its instance was
                    // successfully deactivated.
                    mSapaAppService.removeFromActiveApps(sapaAppInfo.getApp());
                    mAppInfoList.remove(instanceId);
                } catch (SapaConnectionNotSetException e) {
                    Log.e(TAG,
                            "Instance could not be removed from active list because of connection exception.");
                }
            }
        }
        mCallerPackageNameList.remove(instanceId);
        mActionsList.remove(instanceId);
    }

    class LocalBinder extends Binder {

        MainService getMainService(MainActivity activity) {
            Log.d(TAG, "getMainService");
            MainService.this.mActivity = new WeakReference<MainActivity>(activity);
            return MainService.this;
        }
    }

    @Override
    public void onServiceDisconnected() {
        // Nothing to do
    }
}
