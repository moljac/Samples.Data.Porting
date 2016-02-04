package com.samsung.audiosuite.sapaeffectsample;

import android.app.Activity;
import android.os.Bundle;
import android.widget.ImageButton;
import android.widget.TextView;

/**
 * Activity class with functionality same for stand alone activity and activity started from other
 * applications.
 */
public abstract class EffectSampleActivity extends Activity {

    protected ImageButton mVolDownButton;
    protected ImageButton mVolUpButton;
    private TextView mVolTextView;

    public EffectSampleActivity() {
        super();
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        setContentView(R.layout.activity_main);
        super.onCreate(savedInstanceState);

        // Views are being set from the layout.

        this.mVolDownButton = (ImageButton) findViewById(R.id.vDownButton);
        this.mVolUpButton = (ImageButton) findViewById(R.id.vUpButton);
        this.mVolTextView = (TextView) findViewById(R.id.current_value_textview);
    }

    /**
     * This method sets appropriate state on buttons when volume has neither maximum nor minimum
     * value.
     */
    protected void setBetweenVolumeStateOnButtons() {
        mVolDownButton.setEnabled(true);
        mVolUpButton.setEnabled(true);
    }

    /**
     * This method sets appropriate state on buttons when volume has minimum value.
     */
    protected void setMinVolumeStateOnButtons() {
        mVolDownButton.setEnabled(false);
        mVolUpButton.setEnabled(true);
    }

    /**
     * This method sets appropriate state on buttons when volume has maximum value.
     */
    protected void setMaxVolumeStateOnButtons() {
        mVolDownButton.setEnabled(true);
        mVolUpButton.setEnabled(false);
    }

    /**
     * This method sets text on view of value of volume.
     * 
     * @param text
     *            Text to be set.
     */
    protected void updateVolumeTextView(String text) {
        mVolTextView.setText(text);
    }

}