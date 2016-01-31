package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.content.Context;
import android.content.res.TypedArray;
import android.support.v7.widget.RecyclerView;
import android.util.AttributeSet;
import android.util.Log;

import com.samsung.android.sdk.professionalaudio.widgets.R;

/**
 * Created by k.betlej on 10/26/15.
 */
public class FcRecycleView extends RecyclerView {

    public static final String TAG = FcRecycleView.class.getSimpleName();
    private int mMaxWidth;

    public FcRecycleView(Context context) {
        this(context, null);
    }

    public FcRecycleView(Context context, AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public FcRecycleView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        if(attrs != null) {
            TypedArray array = context.obtainStyledAttributes(attrs, R.styleable.FcRecycleView, 0, 0);
            mMaxWidth = array.getLayoutDimension(R.styleable.FcRecycleView_max_width, LayoutParams.MATCH_PARENT);
            array.recycle();
        }
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        super.onMeasure(widthMeasureSpec, heightMeasureSpec);
        int measureWidth = getMeasuredWidth();
        if(mMaxWidth != LayoutParams.MATCH_PARENT && measureWidth > mMaxWidth) {
            Log.d(TAG, "width:" + measureWidth);
            Log.d(TAG, "max width:" + mMaxWidth);
            setMeasuredDimension(mMaxWidth, getMeasuredHeight());
        }
    }
}
