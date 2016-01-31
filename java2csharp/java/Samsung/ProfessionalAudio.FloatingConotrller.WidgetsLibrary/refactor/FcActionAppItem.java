package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.content.pm.PackageManager.NameNotFoundException;
import android.graphics.drawable.Drawable;
import android.util.Log;

import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;

/**
 * @brief Class to represent remote action of the application
 *
 * @see FcActionItem
 */
public class FcActionAppItem implements FcActionItem {

    private static final String TAG = FcActionAppItem.class.getSimpleName();
    private final SapaActionInfo mActionInfo;
    private Runnable mRunnable;

    /**
     * @brief Construct action item from the SapaActionInfo structure
     *
     * @param actionInfo    Sapa structure describing action details
     */
    public FcActionAppItem(SapaActionInfo actionInfo) {
        mActionInfo = actionInfo;
    }

    @Override
    public Drawable getIcon(FcContext fcContext) {
        Drawable drawable = null;
        try {
            drawable = mActionInfo.getIcon(fcContext.getContext());

        } catch (NameNotFoundException e) {
            Log.w(TAG, "Cannot retrieve action icon: name not found");
        }

        return drawable;
    }

    @Override
    public String getId() {
        return mActionInfo.getId();
    }

    @Override
    public Runnable getActionRunnable() {
        return mRunnable;
    }

    @Override
    public String getName(FcContext fcContext) {
        Log.d(TAG, "getName");
        String name = null;
        try {
            name = mActionInfo.getName(fcContext.getContext());

        } catch (NameNotFoundException e) {
            Log.w(TAG, "Cannot retrieve action name: name not found");
        }
        Log.d(TAG, "name:" + name);
        return name;
    }

    @Override
    public boolean isEnabled() {
        return mActionInfo.isEnabled();
    }

    @Override
    public boolean isVisible() {
        return mActionInfo.isVisible();
    }

    /**
     * @brief Set Runnable to be run when this action is executed
     *
     * @param runnable  Action executor as Runnable object
     */
    public void setActionRunnable(Runnable runnable) {
        mRunnable = runnable;
    }
}
