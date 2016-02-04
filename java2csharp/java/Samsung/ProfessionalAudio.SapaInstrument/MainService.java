package com.samsung.audiosuite.sapainstrumentsample;

import java.lang.ref.WeakReference;
import java.util.HashMap;
import java.util.Iterator;
import java.util.LinkedList;
import java.util.List;
import java.util.Map;
import java.util.Map.Entry;
import java.util.Queue;
import java.util.Set;
import java.util.concurrent.ConcurrentHashMap;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
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

public class MainService extends Service implements SapaServiceConnectListener,
        SapaActionDefinerInterface {

    private static final String TAG = "audiosuite:sapainstrumentsample:j:MainService";

    // Binder for activity
    private LocalBinder mBinder;

    private SapaAppService mSapaAppService;

    // SapaAppInfo package representing this application
    HashMap<String, SapaAppInfo> mAppInfoList;

    HashMap<String, SapaProcessor> mProcessorList;

    HashMap<String, ActionsPack> mActionsList;

    // Reference to the activity of application
    private WeakReference<MainActivity> mActivity;

    private SapaService mSapaService;

    private boolean mIsPlaying;

    SparseArray<SapaActionInfo> mActionArray;

    private Map<String, String> mCallerPackageNameList;

    private Queue<Intent> mDelayedIntents = new LinkedList<Intent>();

    private boolean mServiceConnected;

    @Override
    public void onCreate() {
        Log.d(TAG, "onCreate");
        super.onCreate();
        this.mBinder = new LocalBinder();
        this.mIsPlaying = false;
        this.mSapaAppService = null;
        this.mServiceConnected = false;
        this.mActionsList = new HashMap<String, MainService.ActionsPack>();
        this.mProcessorList = new HashMap<String, SapaProcessor>();
        this.mAppInfoList = new HashMap<String, SapaAppInfo>();
        this.mCallerPackageNameList = new ConcurrentHashMap<String, String>();
    }

    @Override
    public int onStartCommand(Intent intent, int flags, int startId) {
    	Log.d(TAG, "onStartCommand");
        if (intent != null){
	        if (this.mServiceConnected) {
	            handleIntent(intent);
	        } else {
	            this.mDelayedIntents.add(intent);
	        }
        }

        // Connection to remote service of ProfessionalAudio system is set.
        if (this.mSapaAppService == null) {
            connectConnectionBridge();
        }

        return super.onStartCommand(intent, flags, startId);
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

    void updateBpm() {
        if (mSapaAppService != null) {
            try {
                List<SapaAppInfo> infos = mSapaAppService.getAllActiveApp();
                for (SapaAppInfo info : infos) {
                    if (info.getApp().getPackageName().contentEquals("com.sec.musicstudio")) {
                        if (info.getConfiguration() != null) {
                            int bpm = info.getConfiguration().getInt("bpm");
                            Log.d(TAG, "bpm is " + bpm);
                        }
                    }
                }
            } catch (SapaConnectionNotSetException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }
        }
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
                // Actions are being set in SapaAppInfo representing this
                // application.
                mActionArray = new SparseArray<SapaActionInfo>();
                mActionArray.put(0, new SapaActionInfo(MainActivity.ACTION_PLAY,
                        R.drawable.ctrl_btn_play_default, this.getPackageName()));
                mActionArray.put(1, new SapaActionInfo(MainActivity.ACTION_STOP,
                        R.drawable.ctrl_btn_stop_default, this.getPackageName(), true, false));
                info.setActions(mActionArray);

                // Native part is being initialised.
                SapaProcessor sapaProcessor = new SapaProcessor(this, null,
                        new SapaProcessorStateListener(info.getApp()));
                this.mSapaService.register(sapaProcessor);
                sapaProcessor.activate();
                this.mProcessorList.put(info.getApp().getInstanceId(), sapaProcessor);

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
                Log.e(TAG, "SapaService can not be instantiate...");
                e.printStackTrace();
			}
        }
        if (mActionsList != null && info != null && info.getApp() != null) {
            this.mActionsList.put(info.getApp().getInstanceId(), new ActionsPack(info.getApp()));
        }
    }

    private synchronized void handleDeactivationIntent(Intent intent) {
        SapaApp sapaApp = SapaAppInfo.getApp(intent);
        this.mActionsList.remove(sapaApp.getInstanceId());
        SapaProcessor processor = this.mProcessorList.get(sapaApp.getInstanceId());
        if (processor != null) {
            processor.deactivate();
            this.mSapaService.unregister(processor);
        }
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
    public synchronized Runnable getActionDefinition(SapaApp sapaApp, String actionId) {
        // Basing on actionId appropriate action definition is returned.
        Log.d(TAG, "instanceid " + sapaApp.getInstanceId() + " actionId " + actionId);
        if (actionId.equals(MainActivity.ACTION_PLAY)) {
            return this.mActionsList.get(sapaApp.getInstanceId()).getPlayAction();
        } else if (actionId.equals(MainActivity.ACTION_STOP)) {
            return this.mActionsList.get(sapaApp.getInstanceId()).getStopAction();
        }
        return null;
    }

    boolean isPlaying() {
        return this.mIsPlaying;
    }

    void play(SapaApp sapaApp) {
        SapaAppInfo info = this.mAppInfoList.get(sapaApp.getInstanceId());
        // Play action is being deactivated.
        modifyAction(info, MainActivity.ACTION_PLAY, R.drawable.ctrl_btn_play_disabled, false);
        // Stop action is being activated.
        modifyAction(info, MainActivity.ACTION_STOP, R.drawable.ctrl_btn_stop_default, true);
        try {
            // Notification that AppInfo has been changed is sent.
            this.mSapaAppService.changeAppInfo(info);
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "State of application cound not be changed due to connection exception");
        }

        this.mIsPlaying = true;
        // Play command is sent to native part.
        Logic.startPlaying(this.mProcessorList.get(sapaApp.getInstanceId()));

    }

    void stop(SapaApp sapaApp) {
        SapaAppInfo info = this.mAppInfoList.get(sapaApp.getInstanceId());
        // Play action is being activated.
        modifyAction(info, MainActivity.ACTION_PLAY, R.drawable.ctrl_btn_play_default, true);
        // Stop action is being deactivated.
        modifyAction(info, MainActivity.ACTION_STOP, R.drawable.ctrl_btn_stop_disabled, false);
        try {
            // Notification that AppInfo has been changed is sent.
            this.mSapaAppService.changeAppInfo(info);
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "State of application cound not be changed due to connection exception");
        }

        this.mIsPlaying = false;

        // Stop command is sent to native part.
        Logic.stopPlaying(this.mProcessorList.get(sapaApp.getInstanceId()));
    }

    SapaAppService getSapaAppService() {
        return this.mSapaAppService;
    }

    private void modifyAction(SapaAppInfo info, String actionKey, int iconResId, boolean isEnabled) {
        // In this example only one action can be active at the same time.
        // We need to change state of actions every time action is called.
        if (info != null && mSapaAppService != null) {
            // Obtaining current ActionInfo.
            SapaActionInfo action = info.getActionInfo(actionKey);
            if (action != null) {
                // Settling state.
                action.setVisible(isEnabled);
                // Settling icon of action by setting id of the resource.
                action.setIcon(iconResId);
            }
        }
    }

    private void connectConnectionBridge() {
        Log.d(TAG, "Connect using bridge");
        this.mSapaAppService = new SapaAppService(this);

        // MainService starts listening for establishment of connection.
        // MainService needs to implement AudioServiceConnectListener to be able
        // to listen for this event.
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
                        // Close this instance. because the caller is closed by
                        // unknow reason. So, It will be not used anymore.
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
            // update bpm
            Log.d(TAG, "onAppChanged");
            updateBpm();

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

    class ActionsPack {

        private SapaApp mSapaApp;
        private Runnable mPlayAction;
        private Runnable mStopAction;
        private boolean mIsPlaying;

        ActionsPack(SapaApp sapaApp) {
            this.mSapaApp = sapaApp;
            this.mPlayAction = null;
            this.mStopAction = null;
            this.mIsPlaying = false;
        }

        boolean isPlaying() {
            return this.mIsPlaying;
        }

        Runnable getPlayAction() {
            if (this.mPlayAction == null) {
                setPlayAction();
            }
            return this.mPlayAction;
        }

        Runnable getStopAction() {
            if (this.mStopAction == null) {
                setStopAction();
            }
            return this.mStopAction;
        }

        private void setPlayAction() {

            // Definition of volume down action
            this.mPlayAction = new Runnable() {

                @Override
                public void run() {
                    // Here you write body of your action
                    // boolean wasMaxVolume =
                    // isMaxVolume(ActionsPack.this.mSapaApp);

                    try {
                        play(ActionsPack.this.mSapaApp);
                        ActionsPack.this.mIsPlaying = true;
                        if (mActivity != null) {
                            MainActivity activity = mActivity.get();
                            if (activity != null
                                    && activity.isVisibleInstance(ActionsPack.this.mSapaApp)) {
                                activity.changeButtonsOnPlay();
                            }
                        }
                    } catch (NullPointerException e) {
                        ;
                    }

                }
            };
        }

        private synchronized void setStopAction() {

            // Definition of volume down action
            this.mStopAction = new Runnable() {

                @Override
                public void run() {
                    // Here you write body of your action

                    try {
                        stop(ActionsPack.this.mSapaApp);
                        ActionsPack.this.mIsPlaying = false;
                        if (mActivity != null) {
                            MainActivity activity = mActivity.get();
                            if (activity.isVisibleInstance(ActionsPack.this.mSapaApp)) {
                                activity.changeButtonsOnStop();
                            }
                        }
                    } catch (NullPointerException e) {
                        ;
                    }

                }
            };
        }
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
        // Nothing to do.
    }
}
