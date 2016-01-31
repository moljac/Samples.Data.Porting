
package com.samsung.android.example.slookdemos;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.TextView;

import com.samsung.android.sdk.look.writingbuddy.SlookWritingBuddy;

public class WritingBuddyViewGroupActivity extends Activity {

    private SlookWritingBuddy mWritingBuddy;

    private TextView mOutputTextView;

    /* State */
    private boolean mIsEnabled = true;

    private int mType = SlookWritingBuddy.TYPE_EDITOR_TEXT;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_writingbuddy_viewgroup);

        FrameLayout fl = (FrameLayout) findViewById(R.id.input);
        mOutputTextView = (TextView) findViewById(R.id.text_output);

        mWritingBuddy = new SlookWritingBuddy(fl);

        mWritingBuddy.setTextWritingListener(new SlookWritingBuddy.TextWritingListener() {

            public void onTextReceived(CharSequence arg0) {
                mOutputTextView.setText(arg0);
            }
        });

        Button enableButton = (Button) findViewById(R.id.btn_enable);
        enableButton.setOnClickListener(new View.OnClickListener() {

            public void onClick(View v) {
                if (mIsEnabled) {
                    ((Button) v).setText("Enable");
                    mIsEnabled = false;
                } else {
                    ((Button) v).setText("Disable");
                    mIsEnabled = true;
                }
                mWritingBuddy.setEnabled(mIsEnabled);
            }
        });

        Button typeButton = (Button) findViewById(R.id.btn_type);
        typeButton.setOnClickListener(new View.OnClickListener() {

            public void onClick(View v) {
                if (mType == SlookWritingBuddy.TYPE_EDITOR_TEXT) {
                    mType = SlookWritingBuddy.TYPE_EDITOR_NUMBER;
                    ((Button) v).setText("String+Number");
                } else {
                    mType = SlookWritingBuddy.TYPE_EDITOR_TEXT;
                    ((Button) v).setText("Number");
                }
                mWritingBuddy.setEditorType(mType);
            }
        });
    }
}
