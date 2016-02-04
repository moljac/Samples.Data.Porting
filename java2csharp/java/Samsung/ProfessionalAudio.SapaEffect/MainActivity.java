package com.samsung.audiosuite.sapaeffectsample;

import java.lang.ref.WeakReference;

import android.content.ComponentName;
import android.content.Intent;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.View;

import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.widgets.FloatingController;

public class MainActivity extends EffectSampleActivity {

    static final String VOLUME_DOWN = "sapaeffectsample.volumedown";
    static final String VOLUME_UP = "sapaeffectsample.volumeup";

    private static final String TAG = "audiosuite:sapaeffectsample:j:MainActivity";

    // Reference to local service.
    WeakReference<MainService> mService;

    private SapaAppInfo mVisibleAppInfo;

    // Floating controller.
    FloatingController mFloatingController;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Log.d(TAG, "onCreate");
        super.onCreate(savedInstanceState);
        
        this.mFloatingController = (FloatingController) findViewById(R.id.jam_control);

        this.mService = null;
        // binding to local service.
        this.bindService(new Intent(this, MainService.class), this.mConnection, 0);

        Intent intent = getIntent();
        if (intent != null) {
            this.readIntent(intent);
        }

        this.mVolDownButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                if (mService != null && mService.get() != null && mVisibleAppInfo != null) {
                    try {
                        mService.get().decreaseVolume(MainActivity.this.mVisibleAppInfo.getApp());
                        if (mService.get().isMinVolume(MainActivity.this.mVisibleAppInfo.getApp())) {
                            MainActivity.this.setMinVolumeStateOnButtons();
                        } else {
                            MainActivity.this.setBetweenVolumeStateOnButtons();
                        }
                    } catch (NullPointerException e) {
                        ;
                    }
                }
            }
        });
        this.mVolUpButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                if (mService != null && mService.get() != null && mVisibleAppInfo != null) {
                    try {
                        mService.get().increaseVolume(MainActivity.this.mVisibleAppInfo.getApp());
                        if (mService.get().isMaxVolume(MainActivity.this.mVisibleAppInfo.getApp())) {
                            setMaxVolumeStateOnButtons();
                        } else {
                            MainActivity.this.setBetweenVolumeStateOnButtons();
                        }
                    } catch (NullPointerException e) {
                        ;
                    }
                }
            }
        });
    }

    @Override
    protected void onResume() {
        Log.d(TAG, "onResume");
        super.onResume();
        if (mService != null && mService.get() != null) {
            try {
                updateVolumeTextView(mService.get().getVolumeText(mVisibleAppInfo));
                if (mVisibleAppInfo != null) {
                    MainActivity.this.setCurrentState(MainActivity.this.mVisibleAppInfo.getApp());
                }
            } catch (NullPointerException e) {
                ;
            }
        } else {
            updateVolumeTextView("Service, not ready");
        }
    }

    @Override
    public void onNewIntent(Intent intent) {
        Log.d(TAG, "onNewIntent");
        this.readIntent(intent);
    }

    private void readIntent(Intent intent) {
        SapaAppInfo info = SapaAppInfo.getAppInfo(intent);
        if (info != null) {
            this.mVisibleAppInfo = info;

        }
    }

    @Override
    protected void onDestroy() {
        Log.d(TAG, "onDestroy");
        unbindService(mConnection);
        super.onDestroy();
    }

    private ServiceConnection mConnection = new ServiceConnection() {

        @Override
        public void onServiceConnected(ComponentName className, IBinder binder) {
            Log.d(TAG, "onServiceConnected");
            MainActivity.this.mService = new WeakReference<MainService>(
                    ((MainService.LocalBinder) binder).getMainService(MainActivity.this));
            // Connection bridge is set to the FloatingController.
            // You can have only one AudioAppConnectionBridge in the application
            // so you need to pass it from service to FloatingController.
            if (mService != null && mService.get() != null && mFloatingController != null) {
                try {
                    MainActivity.this.mFloatingController
                            .setSapaAppService(MainActivity.this.mService.get().getSapaAppService());
                } catch (NullPointerException e) {
                    ;
                }
            }
            // Set buttons in state from the service.
            if (mVisibleAppInfo != null) {
                MainActivity.this.setCurrentState(MainActivity.this.mVisibleAppInfo.getApp());
            }
            if (mService != null && mService.get() != null) {
                try {
                    MainActivity.this.updateVolumeTextView(mService.get().getVolumeText(
                            mVisibleAppInfo));
                } catch (NullPointerException e) {
                    ;
                }
            }
        }

        @Override
        public void onServiceDisconnected(ComponentName name) {
            Log.d(TAG, "onServiceDisconnected");
            MainActivity.this.mService = null;
        }

    };

    private void setCurrentState(SapaApp sapaApp) {
        if (this.mService != null && mService.get() != null) {
            try {
                if (this.mService.get().isMaxVolume(sapaApp)) {
                    this.setMaxVolumeStateOnButtons();
                } else if (this.mService.get().isMinVolume(sapaApp)) {
                    this.setMinVolumeStateOnButtons();
                } else {
                    this.setBetweenVolumeStateOnButtons();
                }
            } catch (NullPointerException e) {
                ;
            }
        }
    }


    void onMaxVolume() {
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                setMaxVolumeStateOnButtons();
            }
        });
    }

    void onMinVolume() {
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                setMinVolumeStateOnButtons();
            }
        });
    }

    void onBetweenVolume() {
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                setBetweenVolumeStateOnButtons();
            }
        });
    }

    SapaAppInfo getVisibleApp() {
        return mVisibleAppInfo;
    }
}
