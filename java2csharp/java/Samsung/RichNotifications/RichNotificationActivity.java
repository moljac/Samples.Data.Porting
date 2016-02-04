
package com.samsung.android.richnotification.sample;

import java.util.UUID;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Spinner;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.richnotification.Srn;
import com.samsung.android.sdk.richnotification.SrnRichNotificationManager;
import com.samsung.android.sdk.richnotification.SrnRichNotificationManager.ErrorType;
import com.samsung.android.sdk.richnotification.SrnRichNotificationManager.EventListener;

public class RichNotificationActivity extends Activity implements EventListener {

    public enum TemplateTypes {
        SMALL_HEADER,
        MEDIUM_HEADER,
        LARGE_HEADER,
        FULL_SCREEN,
        EVENT,
        IMAGE;
    }

    private SrnRichNotificationManager mRichNotificationManager;
    private Spinner mSpinner;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.notification_main);

        Srn srn = new Srn();
        try {
            // Initialize an instance of Srn.
            srn.initialize(this);
        } catch (SsdkUnsupportedException e) {
            // Error handling
        }

        mRichNotificationManager = new SrnRichNotificationManager(getApplicationContext());

        mSpinner = (Spinner) findViewById(R.id.spinner1);

        Toast.makeText(this, "isConnected : " + mRichNotificationManager.isConnected(),
                Toast.LENGTH_LONG).show();
    }

    @Override
    protected void onResume() {
        super.onResume();

        mRichNotificationManager.start();
        mRichNotificationManager.registerRichNotificationListener(this);
    }

    @Override
    protected void onPause() {
        super.onPause();

        mRichNotificationManager.unregisterRichNotificationListener(this);
        mRichNotificationManager.stop();
    }

    public void onSendClick(View v) {
        perform(mSpinner.getSelectedItemPosition());
    }

    public void perform(int primary) {
        if (primary < 0 || primary >= TemplateTypes.values().length) {
            return;
        }

        Toast.makeText(RichNotificationActivity.this, "Sending Notification ...",
                Toast.LENGTH_SHORT).show();

        switch (TemplateTypes.values()[primary]) {
            case SMALL_HEADER:
                performExample(new SmallHeaderExample(getApplicationContext()));
                break;

            case MEDIUM_HEADER:
                performExample(new MediumHeaderExample(getApplicationContext()));
                break;

            case LARGE_HEADER:
                performExample(new LargeHeaderExample(getApplicationContext()));
                break;

            case FULL_SCREEN:
                performExample(new FullScreenExample(getApplicationContext()));
                break;

            case EVENT:
                performExample(new EventExample(getApplicationContext()));
                break;

            case IMAGE:
                performExample(new ImageExample(getApplicationContext()));
                break;
        }
    }

    private void performExample(IExample example) {
        UUID uuid = mRichNotificationManager.notify(example.createRichNoti());

        Toast.makeText(RichNotificationActivity.this, "Notification Id : " + uuid,
                Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onError(UUID arg0, ErrorType arg1) {
        // TODO Auto-generated method stub
        Toast.makeText(getApplicationContext(),
                "Something wrong with uuid" + arg0.toString() + "Error:" + arg1.toString(),
                Toast.LENGTH_LONG).show();
    }

    @Override
    public void onRead(UUID arg0) {
        // TODO Auto-generated method stub
        Toast.makeText(getApplicationContext(), "Read uuid" + arg0.toString(), Toast.LENGTH_LONG)
                .show();

    }

    @Override
    public void onRemoved(UUID arg0) {
        // TODO Auto-generated method stub
        Toast.makeText(getApplicationContext(), "Removed uuid" + arg0.toString(), Toast.LENGTH_LONG)
                .show();

    }
}
