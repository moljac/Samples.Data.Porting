
package com.samsung.android.example.slookdemos;

import android.app.Activity;
import android.os.Bundle;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.look.Slook;

public class InformationActivity extends Activity {

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_information);

        Slook slook = new Slook();
        LinearLayout l = (LinearLayout) findViewById(R.id.information);

        try {
            slook.initialize(this);
        } catch (SsdkUnsupportedException e) {
            l.addView(createTextView(e.toString()));
            return;
        }

        l.addView(createTextView("*VersionCode:" + slook.getVersionCode()));
        l.addView(createTextView("*Feature"));
        l.addView(createTextView("   - AirButton:" + slook.isFeatureEnabled(Slook.AIRBUTTON)));
        l.addView(createTextView("   - PointerIcon:"
                + slook.isFeatureEnabled(Slook.SPEN_HOVER_ICON)));
        l.addView(createTextView("   - SmartClip:" + slook.isFeatureEnabled(Slook.SMARTCLIP)));
        l.addView(createTextView("   - WritingBuddy:" + slook.isFeatureEnabled(Slook.WRITINGBUDDY)));
    }

    private TextView createTextView(String str) {
        TextView tv = new TextView(this);
        tv.setText(str);
        return tv;
    }
}
