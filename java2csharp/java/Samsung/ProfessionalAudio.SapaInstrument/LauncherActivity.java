package com.samsung.audiosuite.sapainstrumentsample;

import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.widget.ImageButton;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaPort;
import com.samsung.android.sdk.professionalaudio.SapaPortConnection;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaProcessor.StatusListener;
import com.samsung.android.sdk.professionalaudio.SapaService;

/**
 * Activity started when application is started with launcher.
 * 
 * Such an instance of application is not visible from other audio applications as active one. It
 * also does not get notification about state of other apps. It connects itself to system output.
 */
public class LauncherActivity extends Activity {

    private static final String TAG = "audiosuite:sapainstrumentsample:j:LauncherActivity";

    private ImageButton mPlayButton;
    private ImageButton mStopButton;

    // SapaProcessor of the standalone instance of the application. It handles
    // communication with native part from the activity and does not use service
    // at all.
    private SapaProcessor mProcessor = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Log.d(TAG, "onCreate");
        setContentView(R.layout.activity_main);
        super.onCreate(savedInstanceState);
        
        this.setTitle(getString(R.string.app_name) + " for standalone");
        
        // Views are being set from the layout.
        this.mPlayButton = (ImageButton) findViewById(R.id.playButton);
        this.mStopButton = (ImageButton) findViewById(R.id.stopButton);

        // Native part of application is being started.
        this.startSapaProcessor();

        // Controls actions are being set.
        // Only one button is visible at a time, so visibility needs to be
        // changed.
        this.mPlayButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                mPlayButton.setVisibility(View.GONE);
                mStopButton.setVisibility(View.VISIBLE);
                Logic.startPlaying(LauncherActivity.this.mProcessor);
            }
        });
        this.mStopButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                mPlayButton.setVisibility(View.VISIBLE);
                mStopButton.setVisibility(View.GONE);
                Logic.stopPlaying(LauncherActivity.this.mProcessor);
            }
        });
    }

    /**
     * This method is responsible for unregistering the processor of standalone instance, which
     * means stopping it native part.
     */
    private void stopSapaProcessor() {
        try {
            SapaService sapaService = new SapaService();
            sapaService.unregister(this.mProcessor);
        } catch (InstantiationException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }
    }

    /**
     * This method is responsible for starting native part of the standalone instance of
     * application. This means registering the processor, registering ports and connecting to system
     * audio ports.
     */
    private void startSapaProcessor() {
        try {
            (new Sapa()).initialize(this);
            SapaService sapaService = new SapaService();
            if (!sapaService.isStarted()) {
                sapaService.start(SapaService.START_PARAM_DEFAULT_LATENCY);
            }
            // Creating processor for stand alone version.
            this.mProcessor = new SapaProcessor(this, null, new StatusListener() {

                @Override
                public void onKilled() {
                    Log.d(TAG, "Standalone SapaInstrumentSample was killed.");
                    try {
                    	LauncherActivity.this.stopSapaProcessor();
                    	// The force param should be false 
                    	// to protect killing the SapaProcessors which is running with the Soundcamp.
						(new SapaService()).stop(false);
					} catch (InstantiationException e) {
						e.printStackTrace();
					}
                    finish();
                }
            });
            // The processor is being registered.
            sapaService.register(this.mProcessor);

            // The processor is being activated.
            this.mProcessor.activate();

            // Audio output ports are being connected to system input ports.
            connectPorts(sapaService);

        } catch (InstantiationException e) {
            Log.w(TAG, "SapaService was not created");
            e.printStackTrace();
        } catch (IllegalArgumentException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        } catch (SsdkUnsupportedException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }
    }

    /**
     * This method is responsible for connecting ports of standalone instance of application to
     * system ports.
     * 
     * @param sapaService
     */
    private void connectPorts(SapaService sapaService) {

        // Get list of ports for connecting to system ports.
        List<SapaPort> ports = new ArrayList<SapaPort>();
        for (SapaPort port : mProcessor.getPorts()) {
            if (port.getInOutType() == SapaPort.INOUT_TYPE_OUT) {
                ports.add(port);
            }
        }

        // Connect ports from two lists (first to first, second to second).
        if (ports.size() >= 2) {
            sapaService.connect(new SapaPortConnection(ports.get(0), sapaService.getSystemPort("playback_1")));
            sapaService.connect(new SapaPortConnection(ports.get(1), sapaService.getSystemPort("playback_2")));
        }
    }

    @Override
    protected void onDestroy() {
        Log.d(TAG, "onDestroy");
        // Native part is being stopped.
        this.stopSapaProcessor();
        SapaService sapaService;
        try {
            sapaService = new SapaService();
            // the force param should be false 
            // to protect killing the SapaProcessors which is running with the Soundcamp.
            sapaService.stop(false); 
        } catch (InstantiationException e) {
        }
        super.onDestroy();
    }
}
