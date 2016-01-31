
package com.samsung.android.example.slookdemos;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

import com.samsung.android.sdk.look.SlookPointerIcon;

public class PointerIconActivity extends Activity {

    private boolean mIsDefault = false;

    private TextView mHoverTextView;

    private SlookPointerIcon mPointerIcon = new SlookPointerIcon();

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_pointericon);

        Button changeIcon = (Button) findViewById(R.id.btn_changeicon);

        mHoverTextView = (TextView) findViewById(R.id.text_hoverarea);
        mPointerIcon.setHoverIcon(mHoverTextView, getResources()
                .getDrawable(R.drawable.pointericon));
        changeIcon.setOnClickListener(new View.OnClickListener() {

            public void onClick(View v) {
                if (mIsDefault) {
                    mPointerIcon.setHoverIcon(mHoverTextView,
                            getResources().getDrawable(R.drawable.pointericon));
                    mIsDefault = false;
                    ((Button) v).setText("Change default icon");
                } else {
                    mPointerIcon.setHoverIcon(mHoverTextView, null);
                    mIsDefault = true;
                    ((Button) v).setText("Change special icon");
                }
            }
        });
    }
}
