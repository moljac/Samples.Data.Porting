package com.samsung.android.sdk.professionalaudio.widgets;

import android.content.Context;
import android.util.AttributeSet;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;

/**
 * This class is a LinearLayout which displays its children in reverse order when its orientation is
 * vertical and layoutdirection is RTL
 */
public class ReversableLinearLayout extends LinearLayout {

    public ReversableLinearLayout(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
    }

    public ReversableLinearLayout(Context context) {
        super(context);
    }

    public ReversableLinearLayout(Context context, AttributeSet attrs) {
        super(context, attrs);
    }

    /**
     * IMPORTANT
     * When the orientation is vertical and layoutdirection is RTL this method returns view counting
     * from the end. For example when index is 0 then last view is returnd.
     */
    @Override
    public View getChildAt(int index) {
        if (getOrientation() == LinearLayout.VERTICAL
                && getLayoutDirection() == ViewGroup.LAYOUT_DIRECTION_RTL) {
            index = getChildCount() - index - 1;
        }
        return super.getChildAt(index);
    }

}
