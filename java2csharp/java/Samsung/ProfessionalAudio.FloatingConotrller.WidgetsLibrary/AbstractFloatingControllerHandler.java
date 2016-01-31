package com.samsung.android.sdk.professionalaudio.widgets;

import android.app.Activity;
import android.content.res.TypedArray;
import android.view.View;

import com.samsung.android.sdk.professionalaudio.app.SapaAppService;

import java.util.HashMap;
import java.util.Map;

abstract class AbstractFloatingControllerHandler implements IFloatingControlerHandler {

	protected ControlBar mControlBar;
	protected View mMainView;

    @Override
    public void setSapaAppService(SapaAppService sapaAppService,
            String mainPackagename) {
        mControlBar.setSapaAppService(sapaAppService, mainPackagename);
    }

    @Override
    public void stopConnections() {
        this.mControlBar.removeListeners();
    }

    @Override
    public boolean getBarExpanded() {
        if (this.mControlBar != null) {
            return this.mControlBar.getBarExpanded();
        }
        return false;
    }

    @Override
    public Map<String, Boolean> getDevicesExpanded() {
        if (this.mControlBar != null) {
            return this.mControlBar.getDevicesExpanded();
        }
        return new HashMap<String, Boolean>();
    }
    
    protected abstract int getBorder(float x, float y);
    
    protected void initControlBar(Activity activity, TypedArray array){
    	DeviceActionsLayout.BUTTON_HEIGHT = array.getLayoutDimension(R.styleable.FloatingControler_bar_thickness, activity.getResources().getDimensionPixelSize(R.dimen.default_controlbar_thickness));
    	DeviceActionsLayout.BUTTON_WIDTH = activity.getResources().getDimensionPixelSize(R.dimen.button_width);
    	if(DeviceActionsLayout.BUTTON_WIDTH==0) DeviceActionsLayout.BUTTON_WIDTH = (int) Math.ceil(((double)DeviceActionsLayout.BUTTON_HEIGHT * 68) / 56);
    	//mBarThickness=array.getLayoutDimension(R.styleable.FloatingControler_bar_thickness, LayoutParams.WRAP_CONTENT);
    }
}
