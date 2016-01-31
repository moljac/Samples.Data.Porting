
package com.samsung.android.example.slookdemos;

import android.app.Activity;
import android.content.ActivityNotFoundException;
import android.content.ComponentName;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.samsung.android.sdk.look.smartclip.SlookSmartClip;
import com.samsung.android.sdk.look.smartclip.SlookSmartClipMetaTag;

public class SmartClipActivity extends Activity {

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_smartclip);

        TextView tv = (TextView) findViewById(R.id.text_static);

        SlookSmartClip sc = new SlookSmartClip(tv);
        sc.clearAllMetaTag();
        sc.addMetaTag(new SlookSmartClipMetaTag(SlookSmartClipMetaTag.TAG_TYPE_URL,
                "http://www.samsung.com"));
        sc.addMetaTag(new SlookSmartClipMetaTag(SlookSmartClipMetaTag.TAG_TYPE_PLAIN_TEXT,
                "This is android textview."));

        Button gotoPinAll = (Button) findViewById(R.id.gotopinboard);
        gotoPinAll.setOnClickListener(new View.OnClickListener() {

            public void onClick(View v) {
                Intent intent = new Intent();
                intent.setComponent(new ComponentName("com.samsung.android.app.pinboard",
                        "com.samsung.android.app.pinboard.ui.PinboardActivity"));
                try {
                    startActivity(intent);
                } catch (ActivityNotFoundException e) {
                    Toast.makeText(SmartClipActivity.this,
                            "Pinboard application is not installed.", Toast.LENGTH_SHORT).show();
                }
            }
        });

    }

}
