package com.samsung.android.sdk.professionalaudio.widgets;

import android.content.Context;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.view.LayoutInflater;
import android.widget.ImageButton;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaAppService;

import java.lang.ref.WeakReference;

class MainDeviceLayout extends DeviceActionsLayout {

    private int perpendicular;
    private int along;

    protected MainDeviceLayout(Context context, DeviceActionsLayoutData data,
            SapaAppService sapaAppService, boolean isExpanded, int orientation,
            ControlBar bar) {
        super(context, data, sapaAppService, isExpanded, orientation, bar);
    }

    @Override
    protected ImageButton createDeviceButton() {
        ImageButton button = (ImageButton)LayoutInflater.from(getContext()).inflate(R.layout.main_app_view, this,false);
        Drawable drawable = this.mData.mInstanceIcon;
        if(drawable.getIntrinsicHeight()> mDeviceIconMaxSize){
            drawable.setBounds(0,0,mDeviceIconMaxSize,mDeviceIconMaxSize);
        }
        button.setImageDrawable(drawable);
        button.setClickable(false);
        button.setLongClickable(true);

        Resources res = getResources();
        perpendicular = res.getDimensionPixelSize(R.dimen.default_controlbar_widget);
        along = res.getDimensionPixelSize(R.dimen.main_app_btn_length);
        return button;
    }

    protected LinearLayout.LayoutParams getAppBtnLayoutParams(int orientation){
        return orientation == LinearLayout.HORIZONTAL ?
                new LinearLayout.LayoutParams(along, perpendicular ) :
                new LinearLayout.LayoutParams(perpendicular , along);
    }
}
