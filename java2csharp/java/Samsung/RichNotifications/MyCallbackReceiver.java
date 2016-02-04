
package com.samsung.android.richnotification.sample;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.widget.Toast;

public class MyCallbackReceiver extends BroadcastReceiver {

    @Override
    public void onReceive(Context context, Intent intent) {

        String data = intent.getStringExtra("extra_action_data");

        if (data != null) {
            Toast.makeText(context, data, Toast.LENGTH_SHORT).show();
        }
    }
}
