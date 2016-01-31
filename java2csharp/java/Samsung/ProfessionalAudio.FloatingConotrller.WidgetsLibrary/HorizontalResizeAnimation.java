package com.samsung.android.sdk.professionalaudio.widgets;

import android.view.View;
import android.widget.LinearLayout.LayoutParams;

class HorizontalResizeAnimation extends ResizeAnimation {

    public HorizontalResizeAnimation(View v, float fromWidth, float toWidth, float height,
            int duration) {
        super(v, fromWidth, toWidth, height, height, duration);
    }

    @Override
    public void endCondition(float width, float height) {
        if (width == this.mToWidth && this.mToWidth > this.mFromWidth) {
            this.mView.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT,
                    LayoutParams.WRAP_CONTENT));
        }
    }
}
