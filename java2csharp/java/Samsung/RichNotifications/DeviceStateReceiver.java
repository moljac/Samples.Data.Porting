package com.samsung.android.richnotification.sample;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Log;
import android.widget.Toast;

public class DeviceStateReceiver extends BroadcastReceiver {

    private static final String TAG = DeviceStateReceiver.class.getSimpleName();
    
    @Override
    public void onReceive(Context context, Intent intent) {
        boolean isConnected = intent.getBooleanExtra("isConnected", false);
        
        Log.d(TAG, "Connected : " + isConnected);
        
        Toast.makeText(context, "Connected : " + isConnected, Toast.LENGTH_LONG).show();
        
    }

}
