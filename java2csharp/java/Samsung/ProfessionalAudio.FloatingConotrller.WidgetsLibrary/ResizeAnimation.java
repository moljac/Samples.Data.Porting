package com.samsung.android.sdk.professionalaudio.widgets;

import android.view.View;
import android.view.animation.Animation;
import android.view.animation.Transformation;
import android.widget.LinearLayout.LayoutParams;

abstract class ResizeAnimation extends Animation {

    protected View mView;
    protected float mToHeight;
    protected float mFromHeight;
    protected float mToWidth;
    protected float mFromWidth;

    public ResizeAnimation(View v, float fromWidth, float toWidth, float fromHeight,
            float toHeight, int duration) {
        mToHeight = toHeight;
        mToWidth = toWidth;
        mFromHeight = fromHeight;
        mFromWidth = fromWidth;
        mView = v;
        this.setDuration(duration);
    }

    @Override
    protected void applyTransformation(float interpolatedTime, Transformation t) {
        float height = (mToHeight - mFromHeight) * interpolatedTime + mFromHeight;
        float width = (mToWidth - mFromWidth) * interpolatedTime + mFromWidth;
        LayoutParams p = (LayoutParams) mView.getLayoutParams();
        p.height = (int) height;
        p.width = (int) width;
        this.endCondition(width, height);
        if (width == 0 && this.mToWidth == 0) {
            p.height = 0;
        }
        if (height == 0 && this.mToHeight == 0) {
            p.width = 0;
        }
        mView.requestLayout();
        mView.invalidate();
    }

    public abstract void endCondition(float width, float height);

}
