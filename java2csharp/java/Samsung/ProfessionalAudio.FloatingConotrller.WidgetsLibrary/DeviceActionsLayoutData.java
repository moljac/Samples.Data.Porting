package com.samsung.android.sdk.professionalaudio.widgets;

import java.util.HashMap;
import java.util.Map;

import android.content.Context;
import android.content.pm.PackageManager.NameNotFoundException;
import android.graphics.drawable.Drawable;
import android.util.SparseArray;

import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

class DeviceActionsLayoutData {
    Drawable mInstanceIcon;
    SapaApp mSapaApp;
    String mInstancePackageName;
    SparseArray<SapaActionInfo> mActionList;
    boolean mIsMultiInstanceEnabled;
    Map<String, SapaActionInfo> mActionMap;
    SapaAppInfo mAppInfo;
    int number = -1;

    DeviceActionsLayoutData() {
        mIsMultiInstanceEnabled = false;
    }

    DeviceActionsLayoutData(SapaAppInfo info, Context context, boolean isMultiInstance) throws NameNotFoundException {
        mAppInfo = info;
        this.mInstanceIcon = info.getIcon(context);
        this.mSapaApp = info.getApp();
        this.mInstancePackageName = info.getPackageName();
        this.mActionList = info.getActions();
        if (this.mActionList == null) {
            this.mActionList = new SparseArray<SapaActionInfo>();
        }
        mActionMap = new HashMap<String, SapaActionInfo>();
        loadActionMap();
        this.mIsMultiInstanceEnabled = info.isMultiInstanceEnabled();
        if (this.mIsMultiInstanceEnabled && isMultiInstance) {
            char c = this.mSapaApp.getInstanceId().charAt(this.mSapaApp.getInstanceId().length() - 1);
            number = Character.digit(c, 10);
            if(number > 0) {
                mInstanceIcon = DrawableTool.getDefaultDrawableWithNumber(mInstanceIcon, number, context);
            }
        }
    }

    void loadActionMap() {
        mActionMap.clear();
        for (int i = mActionList.size()-1; i >=0; --i) {
            SapaActionInfo action = mActionList.valueAt(i);
            mActionMap.put(action.getId(), action);
        }
    }

    public void setActions(SparseArray<SapaActionInfo> ActionList) {
        this.mActionList = ActionList;
        loadActionMap();
    }

    @Override
    public boolean equals(Object o) {
        if (o == null) return false;
        if (this == o) return true;
        if (o instanceof DeviceActionsLayoutData) {
            DeviceActionsLayoutData obj = (DeviceActionsLayoutData) o;
            return (mInstancePackageName.equals(obj.mInstancePackageName) && mSapaApp
                    .equals(obj.mSapaApp));
        }
        return false;
    }
}