package com.samsung.android.sdk.professionalaudio.widgets;

import android.content.Context;
import android.content.res.Resources;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.ImageButton;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaAppService;

import java.lang.ref.WeakReference;

class OrdinaryDeviceLayout extends DeviceActionsLayout {

    private static String TAG = "professionalaudioconnection:widget:j:OrdinaryDeviceLayout";
    private int perpendicular;
    private int along;

    protected OrdinaryDeviceLayout(Context context, DeviceActionsLayoutData data,
                                   SapaAppService sapaAppService, boolean isExpanded, int orientation,
                                   ControlBar bar) {
        super(context, data, sapaAppService, isExpanded, orientation, bar);
    }

    @Override
    protected ImageButton createDeviceButton() {
        ImageButton button = (ImageButton)LayoutInflater.from(getContext()).inflate(R.layout.app_view, this,false);
        button.setImageDrawable(this.mData.mInstanceIcon);

        if(mData.mActionList.size()>0) {
            button.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    Log.d(TAG, "Device " + mData.mSapaApp + " clicked.");
                    if (mIsExpanded) {
                        collapse();
                    } else {
                        expand();
                    }
                }
            });
        } else {
            button.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View v) {
                    openAppActivity();
                }
            });
        }

        Resources res = getResources();
        perpendicular = res.getDimensionPixelSize(R.dimen.max_app_ic_size);
        along = res.getDimensionPixelSize(R.dimen.max_app_ic_size);

        return button;
    }

    protected LinearLayout.LayoutParams getAppBtnLayoutParams(int orientation){
        return orientation == LinearLayout.HORIZONTAL ?
                new LinearLayout.LayoutParams(along, perpendicular ) :
                new LinearLayout.LayoutParams(perpendicular , along);
    }

    @Override
    void expand() {
        if (mIsExpanded) {
            return;
        }
        mIsExpanded = true;
        swapExpandedDeviceLayouts();
        show();
        focusProperDevice();
        mParent.getInfo().setDeviceExpanded(mData.mSapaApp.getInstanceId(), true);
    }

    private void swapExpandedDeviceLayouts() {
        // make this layout the expanded one while simultaneously collapsing previously expanded if applies
        if (mParent.getExpandedLayout() != null && mParent.getExpandedLayout() != this) {
            mParent.getExpandedLayout().collapse();
        }
        mParent.setExpandedLayout(this);
    }

    private void focusProperDevice() {
        // focus properly on expanded device
        mParent.setExpandCalled(true);
        int previousExpandedX = getPreviouslyExpandedX();
        int scrollTargetX = computeScrollTargetX(previousExpandedX);
        mParent.setScrollTargetX(scrollTargetX);
    }

    private int getPreviouslyExpandedX() {
        // return X coordinate of previously expanded OrdinaryDeviceLayout
        int previousX = 0;
        if (mParent.getExpandedLayout() != null) {
            previousX = mParent.getExpandedLayout().getLeft();
        }
        return previousX;
    }

    private int computeScrollTargetX(int prevActiveX) {
        // compute X coordinate of scrolling for parent horizontal view
        // depending on the positioning of the ControlBar
        int resX = this.getLeft() - mParent.getEndItemWidth();

        if (mParent.getCurrentPosition() == CornerFloatingControlerHandler.ALIGN_PARENT_BOTTOM_LEFT
                || mParent.getCurrentPosition() == CornerFloatingControlerHandler.ALIGN_PARENT_TOP_LEFT) {
            resX -= prevActiveX - mParent.getEndItemWidth() + this.getWidth();
        }

        return resX;
    }

    @Override
    void collapse() {
        if (!mIsExpanded) {
            return;
        }
        mIsExpanded = false;
        if (mParent.getExpandedLayout() == this) {
            mParent.setExpandedLayout(null);
        }
        hide();
        mParent.getInfo().setDeviceExpanded(mData.mSapaApp.getInstanceId(), false);
    }
}
