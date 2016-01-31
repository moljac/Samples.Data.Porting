package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.graphics.drawable.Drawable;

import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

/**
 * @brief Class to represent a return action
 *
 * @see FcActionItem
 */
public class FcActionReturnItem implements FcActionItem {

    private final SapaAppInfo mAppInfo;
    private int mDrawableId = -1;
    private boolean mIsDefault = false;
    private Runnable mRunnable;

    /**
     * @brief Construct FcActionReturnItem from a structure describing application
     *
     * @param appInfo       Sapa structure describing application
     */
    public FcActionReturnItem(SapaAppInfo appInfo) {
        mAppInfo = appInfo;
    }

    @Override
    public Drawable getIcon(FcContext fcContext) {
        return isDefault()
                ? fcContext.getDrawable(mDrawableId)
                : fcContext.getApplicationDrawable(mAppInfo.getPackageName(), mDrawableId);
    }

    @Override
    public Runnable getActionRunnable() {
        return mRunnable;
    }

    @Override
    public String getName(FcContext fcContext) {
        return fcContext.getApplicationName(mAppInfo.getPackageName());
    }

    @Override
    public String getId() {
        return "";
    }

    @Override
    public boolean isEnabled() {
        // Always enabled
        return true;
    }

    @Override
    public boolean isVisible() {
        // Always visible
        return true;
    }

    /**
     * @brief Set Runnable to be run when this action is executed
     *
     * @param runnable  Action executor as Runnable object
     */
    public void setActionRunnable(Runnable runnable) {
        mRunnable = runnable;
    }

    /**
     * @brief Set whether the return action should be default or custom
     *
     * @param isDefault true for default return action, false for custom
     *
     * @see FcActionItem
     */
    void setDefault(boolean isDefault) {
        mIsDefault = isDefault;
    }

    /**
     * @brief Set a Drawable Id to be used for custom return actions
     *
     * @param drawableId
     */
    void setDrawableId(int drawableId) {
        mDrawableId = drawableId;
    }

    private boolean isDefault() {
        return mIsDefault;
    }
}
