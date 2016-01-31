
package com.samsung.android.example.slookdemos;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.drawable.BitmapDrawable;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;

import com.samsung.android.sdk.look.writingbuddy.SlookWritingBuddy;

public class WritingBuddyEditTextActivity extends Activity {

    private SlookWritingBuddy mWritingBuddy;

    private ImageView mOutputImageView;

    /* State */
    private boolean mIsEnabled = true;

    private boolean mEnableImage = false;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_writingbuddy_edittext);

        EditText editInput = (EditText) findViewById(R.id.edit_input);
        mOutputImageView = (ImageView) findViewById(R.id.image_output);

        mWritingBuddy = new SlookWritingBuddy(editInput);

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

        Button modeButton = (Button) findViewById(R.id.btn_enable_image);
        modeButton.setOnClickListener(new View.OnClickListener() {

            public void onClick(View v) {
                if (mEnableImage) {
                    mEnableImage = false;
                    mWritingBuddy.setImageWritingListener(null);
                    ((Button) v).setText("Enable Image Listener");
                } else {
                    mEnableImage = true;
                    mWritingBuddy
                            .setImageWritingListener(new SlookWritingBuddy.ImageWritingListener() {
                                public void onImageReceived(Bitmap arg0) {
                                    mOutputImageView.setBackground(new BitmapDrawable(
                                            getResources(), arg0));
                                }
                            });
                    ((Button) v).setText("Disable Image Listener");
                }
            }
        });

    }
}
