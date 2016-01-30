package com.samsung.android.sdk.camera.sample.cases.util;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Point;
import android.graphics.Rect;
import android.util.AttributeSet;
import android.util.Size;
import android.view.View;

/**
 * A Simple {@link View} that can draw rect.
 */
public class RectView extends View {

    public static final int GRAVITY_START = 0x00000001;
    public static final int GRAVITY_TOP = 0x00000002;
    public static final int GRAVITY_END = 0x00000004;
    public static final int GRAVITY_BOTTOM = 0x00000008;

    public static final int GRAVITY_CENTER = 0x00000010;
    public static final int GRAVITY_CENTER_VERTICAL = 0x00000020;
    public static final int GRAVITY_CENTER_HORIZONTAL = 0x00000040;

    private Paint mPaint;
    private Rect mRect;
    private Point mOffset;
    private int mGravity;

    public RectView(Context context) {
        this(context, null);
    }

    public RectView(Context context, AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public RectView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        clearRect();
    }

    public void setRectPaint(Paint paint) {
        mPaint = paint;
    }

    public void setRectSize(int width, int height) {
        mRect.set(mRect.left, mRect.top, mRect.left + width, mRect.top + height);
    }

    public Size getRectSize() {
        return new Size(mRect.width(), mRect.height());
    }

    public void setRectGravity(int gravity) {
        mGravity = gravity;
    }

    public int getRectGravity() {
        return mGravity;
    }

    public void setRectOffset(int x, int y) {
        mOffset.set(x, y);
    }

    public void clearRect() {
        mPaint = new Paint();
        mPaint.setColor(Color.BLUE);
        mPaint.setStyle(Paint.Style.STROKE);
        mPaint.setStrokeWidth(3);
        
        mRect = new Rect();
        mOffset = new Point();
        mGravity = GRAVITY_START;
    }

    @Override
    protected void onDraw(Canvas canvas) {
        // Setup gravity.
        if((mGravity & GRAVITY_START) > 0) {
            mRect.set(0, mRect.top, mRect.width(), mRect.bottom);
        } else if((mGravity & GRAVITY_TOP) > 0) {
            mRect.set(mRect.left, 0, mRect.right, mRect.height());
        } else if((mGravity & GRAVITY_END) > 0) {
            mRect.set(canvas.getWidth() - mRect.width(), mRect.top, canvas.getWidth(), mRect.bottom);
        } else if((mGravity & GRAVITY_BOTTOM) > 0) {
            mRect.set(mRect.left, canvas.getHeight() - mRect.height(), mRect.right, canvas.getHeight());
        }

        if((mGravity & GRAVITY_CENTER) > 0) {
            mRect.set((canvas.getWidth() / 2) - (mRect.width() / 2), (canvas.getHeight() / 2) - (mRect.height() / 2),
                    (canvas.getWidth() / 2) - (mRect.width() / 2) + mRect.width(), (canvas.getHeight() / 2) - (mRect.height() / 2) + mRect.height());
        } else if((mGravity & GRAVITY_CENTER_HORIZONTAL) > 0) {
            mRect.set((canvas.getWidth() / 2) - (mRect.width() / 2), mRect.top, (canvas.getWidth() / 2) - (mRect.width() / 2) + mRect.width(), mRect.bottom);
        } else if((mGravity & GRAVITY_CENTER_VERTICAL) > 0) {
            mRect.set(mRect.left, (canvas.getHeight() / 2) - (mRect.height() / 2), mRect.right, (canvas.getHeight() / 2) - (mRect.height() / 2) + mRect.height());
        }

        // Merge offset.
        mRect.offset(mOffset.x, mOffset.y);

        canvas.drawRect(mRect, mPaint);
    }
}
