package com.samsung.audiosuite.sapainstrumentsample;

import java.lang.ref.WeakReference;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.View;
import android.widget.ImageButton;

import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.widgets.FloatingController;

/**
 * This activity is used when application is started from other audio apps using SapaAppService.
 */
public class MainActivity extends Activity {

    static final String FLOATING_ALIGNMENT_STATE_TAG = "floating_alignment_state_tag";
    // Id of action for start playing.
    static final String ACTION_PLAY = "sapainstrumentsample.play";
    // Id of action for stop playing.
    static final String ACTION_STOP = "sapainstrumentsample.stop";

    private static final String TAG = "audiosuite:sapainstrumentsample:j:MainActivity";

    // Reference to local service.
    WeakReference<MainService> mService;

    private ImageButton mPlayButton;
    private ImageButton mStopButton;

    // Floating controller.
    FloatingController mFloatingController;

    // Reference to currently visible SapaAppInfo.
    private SapaAppInfo mVisibleAppInfo;

    @Override
    public void onNewIntent(Intent intent) {
        Log.d(TAG, "onNewIntent");
        this.readIntent(intent);
        changeTitle();
    }

    /**
     * This method is responsible for updating visible SapaAppInfo and sets state of view
     * accordingly.
     * 
     * @param intent
     *            Intent received when activity is shown.
     */
    private void readIntent(Intent intent) {
        SapaAppInfo info = SapaAppInfo.getAppInfo(intent);
        if (info != null) {
            this.mVisibleAppInfo = info;
            if (this.mVisibleAppInfo.getActionInfo(ACTION_PLAY) != null) {
                if (this.mVisibleAppInfo.getActionInfo(ACTION_PLAY).isVisible()) {
                    this.changeButtonsOnStop();
                } else {
                    this.changeButtonsOnPlay();
                }
            }
        }
    }
    
    private void changeTitle(){
    	if(mVisibleAppInfo != null && mVisibleAppInfo.getApp() != null){
    		this.setTitle(getString(R.string.app_name) + " for " + mVisibleAppInfo.getApp().getInstanceId());
    	}
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Log.d(TAG, "onCreate");
        setContentView(R.layout.activity_main);
        super.onCreate(savedInstanceState);
        this.mService = null;

        
        
        // binding to local service.
        this.bindService(new Intent(this, MainService.class), this.mConnection, 0);

        // Views are being set from the layout.
        mFloatingController = (FloatingController) findViewById(R.id.jam_control);
        //Load position form shared preference
        SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
        int barAlignment = preferences.getInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.getBarAlignment());
        /**This method is used to set floating controller position
         To set position you must used one of below value:
         HORIZONTAL POSITION
         5 - BOTTOM RIGHT
         6 - BOTTOM LEFT
         4 - TOP RIGHT
         7 - TOP LEFT

         VERTICAL POSITION
         15 - BOTTOM RIGHT
         16 - BOTTOM LEFT
         14 - TOP RIGHT
         17 - TOP LEFT
         */
        mFloatingController.loadBarState(barAlignment);
        this.mPlayButton = (ImageButton) findViewById(R.id.playButton);
        this.mStopButton = (ImageButton) findViewById(R.id.stopButton);

        // Received intent is being read.
        Intent intent = getIntent();
        if (intent != null) {
            this.readIntent(intent);
            changeTitle();
        }

        // Controls actions are being set.
        // Only one button is visible at a time, so visibility needs to be
        // changed.
        this.mPlayButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                mPlayButton.setVisibility(View.GONE);
                mStopButton.setVisibility(View.VISIBLE);
                if (mService != null && mService.get() != null && mVisibleAppInfo != null) {
                    try {
                        mService.get().play(mVisibleAppInfo.getApp());
                    } catch (NullPointerException e) {
                        ;
                    }
                }
            }
        });
        this.mStopButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                mPlayButton.setVisibility(View.VISIBLE);
                mStopButton.setVisibility(View.GONE);
                if (mService != null && mService.get() != null && mVisibleAppInfo != null) {
                    try {
                        mService.get().stop(mVisibleAppInfo.getApp());
                    } catch (NullPointerException e) {
                        ;
                    }
                }
            }
        });
    }

    @Override
    protected void onDestroy() {
        Log.d(TAG, "onDestroy");
        // Local MainService is being unbound.
        unbindService(mConnection);
        SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
        preferences.edit()
                .putInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.getBarAlignment())
                .apply();
        super.onDestroy();
    }

    // This instance of ServiceConnection is responsible for connection to local
    // MainService.
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

            MainActivity.this.setButtonsState();
        }

        @Override
        public void onServiceDisconnected(ComponentName name) {
            Log.d(TAG, "onServiceDisconnected");
            MainActivity.this.mService = null;
        }

    };

    // When play is called from FloatingController we need to change UI
    // accordingly.
    void changeButtonsOnPlay() {
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                mPlayButton.setVisibility(View.GONE);
                mStopButton.setVisibility(View.VISIBLE);
            }
        });
    }

    // When stop is called from FloatingController we need to change UI
    // accordingly.
    void changeButtonsOnStop() {
        this.runOnUiThread(new Runnable() {

            @Override
            public void run() {
                mPlayButton.setVisibility(View.VISIBLE);
                mStopButton.setVisibility(View.GONE);
            }
        });
    }

    /**
     * This method is responsible for setting buttons state according to current state.
     */
    private void setButtonsState() {
        if (this.mService != null && mService.get() != null) {
            try {
                if (this.mService.get().isPlaying()) {
                    mPlayButton.setVisibility(View.GONE);
                    mStopButton.setVisibility(View.VISIBLE);
                } else {
                    mPlayButton.setVisibility(View.VISIBLE);
                    mStopButton.setVisibility(View.GONE);
                }
            } catch (NullPointerException e) {
                ;
            }
        }
    }

    /**
     * This method returns information whether given in parameter sapaApp instance is currently
     * visible for user.
     * 
     * @param sapaApp
     *            True when given in parameter sapaApp instance is currently visible for user, false
     *            otherwise.
     * @return
     */
    boolean isVisibleInstance(SapaApp sapaApp) {
        if (this.mVisibleAppInfo != null && this.mVisibleAppInfo.getApp() != null) {
            return this.mVisibleAppInfo.getApp().getInstanceId().equals(sapaApp.getInstanceId());
        }
        return false;
    }
}
