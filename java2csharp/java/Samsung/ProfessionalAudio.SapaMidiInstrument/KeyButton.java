package com.samsung.audiosuite.sapamidisample;

import android.widget.Button;
import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;

public class KeyButton extends Button {

    private int mNote = 69;
    private int mVelocity = 80;

    public KeyButton(Context context, AttributeSet attrs) {
        super(context, attrs);
        TypedArray a = context.obtainStyledAttributes(attrs, R.styleable.KeyButton);

        mNote = a.getInteger(R.styleable.KeyButton_note, 69);
        mVelocity = a.getInteger(R.styleable.KeyButton_velocity, 80);

        a.recycle();
    }

    public void setNote(int note) {
        mNote = note;
    }

    public void setVelocity(int velocity) {
        mVelocity = velocity;
    }

    public int getNote() {
        return mNote;
    }

    public int getVelocity() {
        return mVelocity;
    }
}
