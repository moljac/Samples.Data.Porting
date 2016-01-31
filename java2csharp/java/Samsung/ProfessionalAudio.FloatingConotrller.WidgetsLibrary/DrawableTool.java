package com.samsung.android.sdk.professionalaudio.widgets;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.LayerDrawable;
import android.util.Log;
import android.view.View;

import com.samsung.android.sdk.professionalaudio.widgets.refactor.FcContext;

/**
 * 
 * Class for obtaining drawables with numbers on them.
 * 
 */
public class DrawableTool {
    
    private static final int DEFFAULT_PROPORTION_TO_ICON = 50;

    /**
     * 
     * This method returns the given drawable with a number on it.
     * 
     * @param baseDrawable
     *            Drawable to which number will be added
     * @param number
     *            An integer which will be expressed by the drawable
     * @param shiftFromLeft
     *            Left margin of number drawable
     * @param shiftFromBottom
     *            Bottom margin of number drawable
     * @param context
     *            The base context
     * @return Drawable with number
     */
    public static Drawable getDrawableWithNumber(Drawable baseDrawable, int number,
            int shiftFromLeft, int shiftFromBottom, Context context) {

        if (number < 0 || number > 9) {
            number = 0;
        }
        int textSize = context.getResources().getDimensionPixelSize(R.dimen.number_icon_text_size);

        // TODO: Remove hardcoded text drawable color
        TextDrawable textDrawable = new TextDrawable(String.valueOf(number), textSize, Color.WHITE);
        Drawable numberDrawable = context.getResources().getDrawable(R.drawable.number_round);

        textDrawable.setBounds(0, 0, numberDrawable.getIntrinsicWidth(),
                numberDrawable.getIntrinsicHeight());

        Drawable[] drawables = { baseDrawable, numberDrawable, textDrawable };
        LayerDrawable layerDrawable = new LayerDrawable(drawables);
        layerDrawable.setLayerInset(1, shiftFromLeft, 0, 0, shiftFromBottom);
        layerDrawable.setLayerInset(2, shiftFromLeft, 0, 0, shiftFromBottom);

        return layerDrawable;
    }

    public static Drawable getDrawableWithNumber(Drawable baseDrawable, int number, int iconSize, Context context) {

        if (number < 0 || number > 9) {
            number = 0;
        }
        Drawable numberDrawable = context.getResources().getDrawable(R.drawable.number_round);
        if(numberDrawable == null) {
            return baseDrawable;
        }
        int appIconShift = context.getResources().getDimensionPixelOffset(R.dimen.ord_app_icon_shift);
        Bitmap bitmap = Bitmap.createBitmap(iconSize, iconSize, Bitmap.Config.ARGB_8888);
        Canvas canvas = new Canvas(bitmap);
        int textSize = context.getResources().getDimensionPixelSize(R.dimen.number_icon_text_size);
        int textColor = context.getResources().getColor(R.color.fc_app_icon_number_color);
        TextDrawable textDrawable = new TextDrawable(String.valueOf(number), textSize, textColor);


        baseDrawable.setBounds(0, 0, iconSize, iconSize);
        baseDrawable.draw(canvas);

        numberDrawable.setBounds(
                iconSize - (appIconShift + numberDrawable.getIntrinsicWidth()), appIconShift,
                iconSize - (appIconShift), appIconShift + numberDrawable.getIntrinsicHeight());
        numberDrawable.draw(canvas);

        textDrawable.setBounds(
                iconSize - (appIconShift + numberDrawable.getIntrinsicWidth()), appIconShift,
                iconSize - (appIconShift), appIconShift + numberDrawable.getIntrinsicHeight());
        textDrawable.draw(canvas);
        return new BitmapDrawable(context.getResources(), bitmap);
    }
    
    /**
     * This method returns the given drawable with a number on it. The number drawable will be on
     * the top left of base drawable and its size with respect to the base drawable shall be set in
     * theme attributes R.attr.numberIconWidthAsPercentOfAppIcon and
     * R.attr.numberIconHeightAsPercentOfAppIcon (default is 50%)
     * 
     * @param baseDrawable
     *            Drawable to which number will be added
     * @param number
     *            An integer which will be expressed by the drawable
     * @param context
     *            The base context
     * @return Drawable with number
     */
    public static Drawable getDefaultDrawableWithNumber(Drawable baseDrawable, int number,
    		Context context){
    	int[] percentArray = new int[] {
                R.attr.numberIconWidthAsPercentOfAppIcon,
                R.attr.numberIconHeightAsPercentOfAppIcon
        };

    	TypedArray array = context.obtainStyledAttributes(percentArray);
    	array.getInt(0, 50);

        float appIconWidthRatio = (float)array.getInt(0, DEFFAULT_PROPORTION_TO_ICON) / 100f;
        float appIconHeightRatio = (float)array.getInt(1, DEFFAULT_PROPORTION_TO_ICON) / 100f;

    	int shiftFromLeft = (int)((1.0f - appIconWidthRatio) * baseDrawable.getIntrinsicWidth());
    	int shiftFromBottom = (int)((1.0f - appIconHeightRatio) * baseDrawable.getIntrinsicHeight());
    	array.recycle();
    	
    	return getDrawableWithNumber(baseDrawable, number, shiftFromLeft, shiftFromBottom, context);
    }

    public static void setBackground(final View view, final int index, final int numberOfElements, final FcContext fcContext) {
        Drawable background;

        // It is the only button
        if (index == 0 && numberOfElements == 1) {
            background = fcContext.getDrawable(R.drawable.action_button_background_only);
        // It is the first button of many
        } else if (index == 0 &&  numberOfElements > 1) {
            background = fcContext.getDrawable(R.drawable.action_button_background_start);
        // It is the last button of many
        } else if (index == numberOfElements - 1) {
            background = fcContext.getDrawable(R.drawable.action_button_background_end);
        // In other case it is center button
        } else {
            background = fcContext.getDrawable(R.drawable.action_button_background_center);
        }

        view.setBackground(background);
    }

    private DrawableTool() {
    }
}
