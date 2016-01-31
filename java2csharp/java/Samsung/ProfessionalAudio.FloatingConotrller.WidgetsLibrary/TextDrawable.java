package com.samsung.android.sdk.professionalaudio.widgets;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.ColorFilter;
import android.graphics.Paint;
import android.graphics.PixelFormat;
import android.graphics.Rect;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.ShapeDrawable;

/**
 * Created by k.betlej on 9/23/15.
 */
public class TextDrawable extends Drawable {

    private final Paint mPaint;
    private final String mText;

    public TextDrawable(String text, int textSize, int textColor) {
        mText = text;
        mPaint = new Paint();
        mPaint.setColor(textColor);
        mPaint.setTextSize(textSize);
        mPaint.setAntiAlias(true);
//        mPaint.setShadowLayer(6f, 0, 0, Color.BLACK);
        mPaint.setStyle(Paint.Style.FILL);
        mPaint.setTextAlign(Paint.Align.CENTER);
    }

    @Override
    public void draw(Canvas canvas) {
        Rect r = getBounds();
        Paint paint = mPaint;

        // only draw shape if it may affect output
        if (paint.getAlpha() != 0 || paint.getXfermode() != null) {
//            canvas.drawRect(r, paint);
            Rect textBounds = getTextBounds();
            canvas.drawText(mText, r.left + r.width()/2,
                    r.top + r.height()/2 + textBounds.height()/2, paint);
        }

        // restore
    }

    @Override
    public void setAlpha(int alpha) {
        mPaint.setAlpha(alpha);
    }

    @Override
    public void setColorFilter(ColorFilter cf) {
        mPaint.setColorFilter(cf);
    }

    @Override
    public int getOpacity() {
        return PixelFormat.TRANSLUCENT;
    }

    public Rect getTextBounds() {
        Rect bounds = new Rect();
        mPaint.getTextBounds(mText, 0, mText.length(), bounds);
        return bounds;
    }
}
