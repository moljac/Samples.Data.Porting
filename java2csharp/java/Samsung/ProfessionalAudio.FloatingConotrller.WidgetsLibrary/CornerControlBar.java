package com.samsung.android.sdk.professionalaudio.widgets;

import android.content.Context;
import android.util.AttributeSet;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;

import java.util.List;

class CornerControlBar extends ControlBar {

    private final static String TAG = "professionalaudioconnection:widget:j:CornerControlBar";

    protected CornerControlBar(Context activity, Info info) throws NotActivityException {
        super(activity, info, R.layout.corner_controlbar);
    }

    public CornerControlBar(Context context, AttributeSet attrs) throws NotActivityException {
        super(context, attrs);
    }

    @Override
    protected void addMainDevLayout() {
        Log.d(TAG, "mainLayout add");
        if (mDevicesLayout.isShown()) {
            ViewGroup mainParent = ((ViewGroup) CornerControlBar.this.findViewById(R.id.total_panel));
            if(mainParent.getChildAt(1).equals(mMainDeviceLayout)){
                mMainDeviceLayout.collapse();
                mainParent.removeView(mMainDeviceLayout);
            }

            ((ViewGroup) CornerControlBar.this.findViewById(R.id.total_panel)).addView(
                    CornerControlBar.this.mMainDeviceLayout, 1);
        }
    }

    protected ViewGroup getMainDevParent(){
        return ((ViewGroup) CornerControlBar.this.findViewById(R.id.total_panel));
    }

    @Override
    void addSubViews(List<? extends View> subViews, View view) {
        int index = mDevicesLayout.indexOfChild(view);
        for (int i = 0, size = subViews.size(); i < size; ++i) {
            View v = subViews.get(calculateArrayIndex(i, size));
            if (mDevicesLayout.indexOfChild(v) != -1)
                mDevicesLayout.removeView(v);
            //v.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_WIDTH, DeviceActionsLayout.BUTTON_HEIGHT));
            mDevicesLayout.addView(v, ++index);
        }
    }

    private int calculateArrayIndex(int initialIndex, int arraySize){
        if(mDevicesLayout.getLayoutDirection()==LAYOUT_DIRECTION_LTR)
            return initialIndex;
        else
            return arraySize-1-initialIndex;
    }

    @Override
    void clearSubViews(int count, View view) {
        int index = mDevicesLayout.indexOfChild(view);
        mDevicesLayout.removeViews(index + 1, count);
    }

	@Override
	public void onServiceDisconnected() {

	}

}
