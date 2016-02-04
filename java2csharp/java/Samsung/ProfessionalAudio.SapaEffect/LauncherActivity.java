package com.samsung.audiosuite.sapaeffectsample;

import java.util.ArrayList;
import java.util.List;

import android.os.Bundle;
import android.util.Log;
import android.view.View;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaPort;
import com.samsung.android.sdk.professionalaudio.SapaPortConnection;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaProcessor.StatusListener;
import com.samsung.android.sdk.professionalaudio.SapaService;

public class LauncherActivity extends EffectSampleActivity {

    private static final String TAG = "audiosuite:sapaeffectsample:j:LauncherActivity";

    private int mCurrentVolume;
    
    // SapaProcessor of the standalone instance of the application. It handles
    // communication with native part from the activity and does not use service
    // at all.
    private SapaProcessor mProcessor = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        Log.d(TAG, "onCreate");
        super.onCreate(savedInstanceState);

        this.mCurrentVolume = Logic.DEFAULT_VOLUME;
        
        // Native part of application is being started.
        this.startSapaProcessor();

        this.mVolDownButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {

                try {
                    mCurrentVolume = Logic.decreaseVolume(mCurrentVolume);
                    if (Logic.isMinVolume(mCurrentVolume)) {
                        LauncherActivity.this.setMinVolumeStateOnButtons();
                    } else {
                        LauncherActivity.this.setBetweenVolumeStateOnButtons();
                    }
                    updateVolumeTextView(Logic.getVolumeText(mCurrentVolume));
                    Logic.sendVolume(mProcessor, mCurrentVolume);
                } catch (NullPointerException e) {
                    ;
                }

            }
        });
        this.mVolUpButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {

                try {
                    mCurrentVolume = Logic.increaseVolume(mCurrentVolume);
                    if (Logic.isMaxVolume(mCurrentVolume)) {
                        setMaxVolumeStateOnButtons();
                    } else {
                        LauncherActivity.this.setBetweenVolumeStateOnButtons();
                    }
                    updateVolumeTextView(Logic.getVolumeText(mCurrentVolume));
                    Logic.sendVolume(mProcessor, mCurrentVolume);
                } catch (NullPointerException e) {
                    ;
                }

            }
        });
    }

    @Override
    protected void onResume() {
        Log.d(TAG, "onResume");
        super.onResume();        
        updateVolumeTextView(Logic.getVolumeText(mCurrentVolume));
        Logic.sendVolume(mProcessor, mCurrentVolume);
        this.setCurrentState();
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
                    Log.d(TAG, "Standalone SapaEffectSample was killed.");
                    try {
                    	LauncherActivity.this.stopSapaProcessor();
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

        // Get lists of ports for connecting to system ports.
        List<SapaPort> outPorts = new ArrayList<SapaPort>();
        List<SapaPort> inPorts = new ArrayList<SapaPort>();
        for (SapaPort port : mProcessor.getPorts()) {
            if (port.getInOutType() == SapaPort.INOUT_TYPE_OUT) {
                outPorts.add(port);
            } else {
                inPorts.add(port);
            }
        }

        // Create list of system ports to connect to.
        List<SapaPort> systemInPorts = new ArrayList<SapaPort>();
        List<SapaPort> systemOutPorts = new ArrayList<SapaPort>();
        for (SapaPort port : sapaService.getSystemPorts()) {
            if (port.getInOutType() == SapaPort.INOUT_TYPE_IN
                    && port.getName().contains("playback")) {
                systemInPorts.add(port);
            } else if (port.getInOutType() == SapaPort.INOUT_TYPE_OUT
                    && port.getName().contains("capture")) {
                systemOutPorts.add(port);
            }
        }

        // Connect ports from two lists (first to first, second to second).
        if (outPorts.size() >= 2 && systemInPorts.size() >= 2) {
            sapaService.connect(new SapaPortConnection(outPorts.get(0), systemInPorts.get(0)));
            sapaService.connect(new SapaPortConnection(outPorts.get(1), systemInPorts.get(1)));
        }
        if(inPorts.size()>=2 && systemOutPorts.size()>=2){
            sapaService.connect(new SapaPortConnection(systemOutPorts.get(0), inPorts.get(0)));
            sapaService.connect(new SapaPortConnection(systemOutPorts.get(1), inPorts.get(1)));
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
            sapaService.stop(false);
        } catch (InstantiationException e) {
        }
        
        super.onDestroy();
    }

    private void setCurrentState() {

        try {
            if (Logic.isMaxVolume(mCurrentVolume)) {
                this.setMaxVolumeStateOnButtons();
            } else if (Logic.isMinVolume(mCurrentVolume)) {
                this.setMinVolumeStateOnButtons();
            } else {
                this.setBetweenVolumeStateOnButtons();
            }
        } catch (NullPointerException e) {
            ;
        }

    }
}
