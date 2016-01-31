package com.samsung.android.sdk.pass.sample;

import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.util.SparseArray;
import android.view.Menu;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.pass.Spass;
import com.samsung.android.sdk.pass.SpassFingerprint;
import com.samsung.android.sdk.pass.SpassInvalidStateException;

public class SampleActivity extends Activity {

    private SpassFingerprint mSpassFingerprint;
    private Spass mSpass;
    private Context mContext;
    private ListView mListView;
    private List<String> mItemArray = new ArrayList<String>();
    private ArrayAdapter<String> mListAdapter;
    private boolean onReadyIdentify = false;
    private boolean onReadyEnroll = false;
    boolean isFeatureEnabled = false;

    private BroadcastReceiver mPassReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            final String action = intent.getAction();
            if (SpassFingerprint.ACTION_FINGERPRINT_RESET.equals(action)) {
                Toast.makeText(mContext, "all fingerprints are removed", Toast.LENGTH_SHORT).show();
            } else if (SpassFingerprint.ACTION_FINGERPRINT_REMOVED.equals(action)) {
                int fingerIndex = intent.getIntExtra("fingerIndex", 0);
                Toast.makeText(mContext, fingerIndex + " fingerprints is removed",Toast.LENGTH_SHORT).show();
            } else if (SpassFingerprint.ACTION_FINGERPRINT_ADDED.equals(action)) {
                int fingerIndex = intent.getIntExtra("fingerIndex", 0);
                Toast.makeText(mContext, fingerIndex + " fingerprints is added", Toast.LENGTH_SHORT).show();
            }
        }
    };

    private void registerBroadcastReceiver(){
        IntentFilter filter = new IntentFilter();
        filter.addAction(SpassFingerprint.ACTION_FINGERPRINT_RESET);
        filter.addAction(SpassFingerprint.ACTION_FINGERPRINT_REMOVED);
        filter.addAction(SpassFingerprint.ACTION_FINGERPRINT_ADDED);
        mContext.registerReceiver(mPassReceiver, filter);
    };
    
    private void unregisterBroadcastReceiver() {
        try {
            if (mContext != null) {
                mContext.unregisterReceiver(mPassReceiver);
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
    
    private SpassFingerprint.IdentifyListener listener = new SpassFingerprint.IdentifyListener() {
        @Override
        public void onFinished(int eventStatus) {
            log("identify finished : reason=" + getEventStatusName(eventStatus));
            onReadyIdentify = false;
            int FingerprintIndex = 0;
            try {
                FingerprintIndex = mSpassFingerprint.getIdentifiedFingerprintIndex();
            } catch (IllegalStateException ise) {
                log(ise.getMessage());
            }
            if (eventStatus == SpassFingerprint.STATUS_AUTHENTIFICATION_SUCCESS) {
                log("onFinished() : Identify authentification Success with FingerprintIndex : " + FingerprintIndex);
            } else if (eventStatus == SpassFingerprint.STATUS_AUTHENTIFICATION_PASSWORD_SUCCESS) {
                log("onFinished() : Password authentification Success");
            } else {
                log("onFinished() : Authentification Fail for identify");
            }
        }

        @Override
        public void onReady() {
            log("identify state is ready");
        }

        @Override
        public void onStarted() {
            log("User touched fingerprint sensor!");
        }
    };
    
    private SpassFingerprint.RegisterListener mRegisterListener = new SpassFingerprint.RegisterListener() {

        @Override
        public void onFinished() {
            onReadyEnroll = false;
            log("RegisterListener.onFinished()");

        }
    };
    private static String getEventStatusName(int eventStatus) {
        switch (eventStatus) {
        case SpassFingerprint.STATUS_AUTHENTIFICATION_SUCCESS:
            return "STATUS_AUTHENTIFICATION_SUCCESS";
        case SpassFingerprint.STATUS_AUTHENTIFICATION_PASSWORD_SUCCESS:
            return "STATUS_AUTHENTIFICATION_PASSWORD_SUCCESS";
        case SpassFingerprint.STATUS_TIMEOUT_FAILED:
            return "STATUS_TIMEOUT";
        case SpassFingerprint.STATUS_SENSOR_FAILED:
            return "STATUS_SENSOR_ERROR";
        case SpassFingerprint.STATUS_USER_CANCELLED:
            return "STATUS_USER_CANCELLED";
        case SpassFingerprint.STATUS_QUALITY_FAILED:
            return "STATUS_QUALITY_FAILED";
        case SpassFingerprint.STATUS_USER_CANCELLED_BY_TOUCH_OUTSIDE:
            return "STATUS_USER_CANCELLED_BY_TOUCH_OUTSIDE";
        case SpassFingerprint.STATUS_AUTHENTIFICATION_FAILED:
        default:
            return "STATUS_AUTHENTIFICATION_FAILED";
        }
        
    }
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        mContext = this;
        mListAdapter = new ArrayAdapter<String>(this, R.layout.list_entry, mItemArray);
        mListView = (ListView)findViewById(R.id.listView1);
        
        if (mListView != null) {
            mListView.setAdapter(mListAdapter);
        }
        mSpass = new Spass();

        try {
            mSpass.initialize(SampleActivity.this);
        } catch (SsdkUnsupportedException e) {
            log("Exception: " + e);
        } catch (UnsupportedOperationException e){
            log("Fingerprint Service is not supported in the device");
        }
        isFeatureEnabled = mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT);
        
        if(isFeatureEnabled){
            mSpassFingerprint = new SpassFingerprint(SampleActivity.this);
            log("Fingerprint Service is supported in the device.");
            log("SDK version : " + mSpass.getVersionName());
        } else { 
            logClear();
            log("Fingerprint Service is not supported in the device.");
        }
        SparseArray<View.OnClickListener> listeners = new SparseArray<View.OnClickListener>();
        registerBroadcastReceiver();
        listeners.put(R.id.buttonHasRegisteredFinger, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    boolean hasRegisteredFinger = mSpassFingerprint.hasRegisteredFinger();
                    log("hasRegisteredFinger() = " + hasRegisteredFinger);
                } catch (UnsupportedOperationException e) {
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        
        listeners.put(R.id.buttonIdentify, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            try {
                                onReadyIdentify = true;
                                mSpassFingerprint.startIdentify(listener);
                                log("Please identify finger to verify you");
                            } catch (SpassInvalidStateException ise) {
                                onReadyIdentify = false;
                                if (ise.getType() == SpassInvalidStateException.STATUS_OPERATION_DENIED) {
                                    log("Exception: " + ise.getMessage());
                                }
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e) {
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });

        listeners.put(R.id.buttonShowIdentifyDialogWithPW, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try{
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            onReadyIdentify = true;
                            if(!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_AVAILABLE_PASSWORD)){
                                log("The Backup Password is not supported.");
                            }
                            try {
                                mSpassFingerprint.startIdentifyWithDialog(SampleActivity.this, listener, true);
                                log("Please identify finger to verify you");
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        
        listeners.put(R.id.buttonShowIdentifyDialogWithoutPW, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            onReadyIdentify = true;
                            try {
                                mSpassFingerprint.startIdentifyWithDialog(SampleActivity.this, listener, false);
                                log("Please identify finger to verify you");
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });

        listeners.put(R.id.buttonCancel, new View.OnClickListener() {
            @Override
            public void onClick(View v) { 
                logClear();
                try {
                    if (onReadyIdentify == true) {
                        try {
                            mSpassFingerprint.cancelIdentify();
                            log("cancelIdentify is called");
                        } catch (IllegalStateException ise) {
                            log(ise.getMessage());
                        }
                        onReadyIdentify = false;
                    } else {
                        log("Please request Identify first");
                    }
                } catch (UnsupportedOperationException e) {
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        listeners.put(R.id.buttonRegisterFinger, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (onReadyIdentify == false) {
                        if (onReadyEnroll == false) {
                            onReadyEnroll = true;
                            mSpassFingerprint.registerFinger(SampleActivity.this, mRegisterListener);
                            log("Jump to the Enroll screen");
                        } else {
                            log("Please wait and try to register again");
                        }
                    } else {
                        log("Please cancel Identify first");
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        listeners.put(R.id.buttonGetRegisteredFingerprintName, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    log("=Fingerprint Name=");
                    SparseArray<String> mList = mSpassFingerprint.getRegisteredFingerprintName();
                    if (mList == null) {
                        log("Registered fingerprint is not existed.");
                    } else {
                        for (int i = 0; i < mList.size(); i++) {
                            int index = mList.keyAt(i);
                            String name = mList.get(index);
                            log("index " + index + ", Name is " + name);
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        listeners.put(R.id.buttonGetRegisteredFingerprintID, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_UNIQUE_ID)) {
                        SparseArray<String> mList = null;
                        try {
                            log("=Fingerprint Unique ID=");
                            mList = mSpassFingerprint.getRegisteredFingerprintUniqueID();
                            if (mList == null) {
                                log("Registered fingerprint is not existed.");
                            } else {
                                for (int i = 0; i < mList.size(); i++) {
                                    int index = mList.keyAt(i);
                                    String ID = mList.get(index);
                                    log("index " + index + ", Unique ID is " + ID);
                                }
                            }
                        } catch (IllegalStateException ise) {
                            log(ise.getMessage());
                        }
                    } else {
                        log("To get Fingerprint ID is not supported in the device");
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });

        listeners.put(R.id.buttonIdentifyWithIndex, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            try {
                                onReadyIdentify = true;
                                if (mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_FINGER_INDEX)) {
                                    ArrayList<Integer> designatedFingers = new ArrayList<Integer>();
                                    designatedFingers.add(1);
                                    mSpassFingerprint.setIntendedFingerprintIndex(designatedFingers);
                                }
                                mSpassFingerprint.startIdentify(listener);
                                log("Please identify fingerprint index 1 to verify you");
                            } catch (SpassInvalidStateException ise) {
                                onReadyIdentify = false;
                                if (ise.getType() == SpassInvalidStateException.STATUS_OPERATION_DENIED) {
                                    log("Exception: " + ise.getMessage());
                                }
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        
        listeners.put(R.id.buttonShowIdentifyDialogWithIndex, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            onReadyIdentify = true;
                            if (mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_FINGER_INDEX)) {
                                ArrayList<Integer> designatedFingers = new ArrayList<Integer>();
                                designatedFingers.add(2);
                                designatedFingers.add(3);
                                try {
                                    mSpassFingerprint.setIntendedFingerprintIndex(designatedFingers);
                                } catch (IllegalStateException ise) {
                                    log(ise.getMessage());
                                }
                            }
                            try {
                                mSpassFingerprint.startIdentifyWithDialog(SampleActivity.this, listener, true);
                                log("Please identify fingerprint index 2,3 to verify you");
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        listeners.put(R.id.buttonShowIdentifyDialogWithTitleNLogo, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            onReadyIdentify = true;
                            if (mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG)) {
                                try {
                                    mSpassFingerprint.setDialogTitle("Customized Dialog With Logo", 0x000000);
                                    mSpassFingerprint.setDialogIcon("ic_launcher");
                                } catch (IllegalStateException ise) {
                                    log(ise.getMessage());
                                }
                            }
                            try {
                                mSpassFingerprint.startIdentifyWithDialog(SampleActivity.this, listener, false);
                                log("Please Identify fingerprint to verify you");
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        });
        listeners.put(R.id.buttonCustomizedDialogWithTransparency, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            onReadyIdentify = true;
                            if (mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG)) {
                                try {
                                    mSpassFingerprint.setDialogTitle("Customized Dialog With Transparency", 0x000000);
                                    mSpassFingerprint.setDialogBgTransparency(0);
                                } catch (IllegalStateException ise) {
                                    log(ise.getMessage());
                                }
                            }
                            try {
                                mSpassFingerprint.startIdentifyWithDialog(SampleActivity.this, listener, false);
                                log("Please identify fingerprint to verify you");
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        }); 
        listeners.put(R.id.buttonCustomizedDialogWithSetDialogDismiss, new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                logClear();
                try {
                    if (!mSpassFingerprint.hasRegisteredFinger()) {
                        log("Please register finger first");
                    } else {
                        if (onReadyIdentify == false) {
                            onReadyIdentify = true;
                            if (mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG)) {
                                try {
                                    mSpassFingerprint.setDialogTitle("Customized Dialog With Setting Dialog dismiss", 0x000000);
                                    mSpassFingerprint.setCanceledOnTouchOutside(true);
                                } catch (IllegalStateException ise) {
                                    log(ise.getMessage());
                                }
                            }
                            try {
                                mSpassFingerprint.startIdentifyWithDialog(SampleActivity.this, listener, false);
                                log("Please identify fingerprint to verify you");
                            } catch (IllegalStateException e) {
                                onReadyIdentify = false;
                                log("Exception: " + e);
                            }
                        } else {
                            log("Please cancel Identify first");
                        }
                    }
                } catch (UnsupportedOperationException e){
                    log("Fingerprint Service is not supported in the device");
                }
            }
        }); 
        final int N = listeners.size();
        for (int i = 0; i < N; i++) {
            int id = listeners.keyAt(i);
            Button button = (Button)findViewById(id);
            if (button != null) {
                button.setOnClickListener(listeners.valueAt(i));
                button.setTextAppearance(mContext, R.style.ButtonStyle);
                if (!isFeatureEnabled) {
                    button.setEnabled(false);
                } else {
                    if(id == R.id.buttonShowIdentifyDialogWithPW){
                        if(!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_AVAILABLE_PASSWORD)){
                            button.setEnabled(false);
                        }
                    } else if (id == R.id.buttonGetRegisteredFingerprintID) {
                        if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_UNIQUE_ID)) {
                            button.setEnabled(false);
                        }
                    } else if (id == R.id.buttonShowIdentifyDialogWithIndex || id == R.id.buttonIdentifyWithIndex) {
                        if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_FINGER_INDEX)) {
                            button.setEnabled(false);
                        }
                    } else if (id == R.id.buttonShowIdentifyDialogWithTitleNLogo || id == R.id.buttonCustomizedDialogWithTransparency || id == R.id.buttonCustomizedDialogWithSetDialogDismiss) {
                        if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG)) {
                            button.setEnabled(false);
                        }
                    }
                }
            }
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        unregisterBroadcastReceiver();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.main, menu);
        return true;
    }

    public void log(String text) {
        final String txt = text;

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                mItemArray.add(0, txt);
                mListAdapter.notifyDataSetChanged();
            }
        });
    }

    public void logClear() {
        if (mItemArray != null) {
            mItemArray.clear();
        }
        if (mListAdapter != null) {
            mListAdapter.notifyDataSetChanged();
        }
    }
    
}
