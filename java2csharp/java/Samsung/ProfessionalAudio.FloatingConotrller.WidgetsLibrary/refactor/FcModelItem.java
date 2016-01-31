package com.samsung.android.sdk.professionalaudio.widgets.refactor;


import android.content.pm.PackageManager.NameNotFoundException;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.util.SparseArray;
import android.view.ViewGroup;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

import java.util.ArrayList;
import java.util.Arrays;
import java.util.Collections;
import java.util.List;

/**
 * @brief Model item representing application within the SAPA environment
 *
 * There are two kind of applications:
 * - Main (host) such as soundcamp
 * - Ordinal, which is instrument or effect
 */
public class FcModelItem {

    public static final int VOLUME_UP_INDEX = 0;
    public static final int VOLUME_DOWN_INDEX = 1;
    public static final int VOLUME_SIZE = 2;

    private static final String TAG = FcModelItem.class.getSimpleName();
    private static final int DEFAULT_CAPACITY = 2;

    /**
     * @brief Construct model item from SapaAppInfo structure
     *
     * @param info  Data structure with information retrieved from SAPA
     *
     * @return Model item representing ordinal application
     */
    public static FcModelItem createOrdinal(FcContext fcContext, SapaAppInfo info) {
        return new FcModelItem(fcContext, info, FcConstants.APP_TYPE_ORDINAL);
    }

    /**
     * @brief Construct model item from SapaAppInfo structure
     *
     * @param info  Data structure with information retrieved from SAPA
     *
     * @return Model item representing main application
     */
    public static FcModelItem createMain(FcContext fcContext, SapaAppInfo info) {
        return new FcModelItem(fcContext, info, FcConstants.APP_TYPE_MAIN);
    }

    private boolean mScrollFocus;
    private boolean mActive = false;
    private List<FcActionItem> mCallActions = new ArrayList<FcActionItem>(DEFAULT_CAPACITY);
    private Drawable mIcon;
    private final String mInstanceId;
    private final int mType;
    private final String mPackageName;
    private List<FcActionItem> mReturnActions = new ArrayList<FcActionItem>(DEFAULT_CAPACITY);
    private FcActionItem[] mVolumeActions = new FcActionItem[VOLUME_SIZE];
    private boolean mExpanded = false;
    private int mDirection;

    /**
     * @brief Construct model item from SapaAppInfo structure
     *
     * @param info  Data structure with information retrieved from SAPA
     * @param type  Type of the item (main or ordinal)
     */
    FcModelItem(FcContext fcContext, SapaAppInfo info, int type) {
        SapaApp app = info.getApp();

        try {
            mIcon = info.getIcon(fcContext.getContext());
        } catch (NameNotFoundException e) {
            LogUtils.throwable(TAG, "Drawable not set: name not found", e);
        }
        mInstanceId = app.getInstanceId();
        mPackageName = app.getPackageName();
        mType = type;

        prepareActions(fcContext, info);
    }

    /**
     * @brief Get read-only list of call actions
     *
     * @return Unmodifiable list of call actions
     */
    public List<FcActionItem> getCallActions() {
        return Collections.unmodifiableList(mCallActions);
    }

    /**
     * @brief Get drawable representing icon of the application
     *
     * @return Application icon (might be null)
     */
    public synchronized Drawable getIcon() {
        return mIcon;
    }

    /**
     * @brief Get instance unique id
     *
     * The current implementation use a pattern of common prefix and a number after separator,
     * for example: bassguitar^2 (second instance of the bass guitar)
     *
     * @return unique instance id
     */
    public String getInstanceId() {
        return mInstanceId;
    }

    /**
     * @brief Get read-only list of return actions
     *
     * @return Unmodifiable list of return actions
     */
    public List<FcActionItem> getReturnActions() {
        return Collections.unmodifiableList(mReturnActions);
    }

    /**
     * @brief Get type of the item (main or ordinal)
     *
     * @return type of the item
     *
     * @see FcConstants#APP_TYPE_MAIN
     * @see FcConstants#APP_TYPE_ORDINAL
     */
    public int getType() {
        return mType;
    }

    /**
     * @brief Get read-only list of volume actions
     *
     * @return Unmodifiable list of volume actions
     */
    public List<FcActionItem> getVolumeActions() {
        if (!hasVolumeActions()) {
            return Collections.emptyList();
        }
        return Collections.unmodifiableList(Arrays.asList(mVolumeActions));
    }

    /**
     * @brief Checks if this item got volume actions
     *
     * @return true if volume actions are present, false if not
     */
    public synchronized boolean hasVolumeActions() {
        return mVolumeActions[VOLUME_UP_INDEX] != null
                && mVolumeActions[VOLUME_DOWN_INDEX] != null;
    }

    /**
     * @brief Check whether the app under this model item is an active app or not
     *
     * @see #setActive(boolean)
     *
     * @return true if the item is marked as active, false otherwise
     */
    public synchronized boolean isActive() {
        return mActive;
    }

    /**
     * @brief Get the horizontal direction of given model item layouts based on Control Bar's direction
     *
     * @return horizontal direction of the item
     */
    public synchronized int getDirection() {
        return mDirection;
    }

    /**
     * @brief Set the horizontal direction of given model item layouts based on Control Bar's direction
     *
     * @param direction
     */
    public synchronized void setDirection(int direction) {
        if (mDirection != direction) {
            reverseActions();
        }
        mDirection = direction;
    }
    /**
     * @brief Check whether given item got the same package as the other item
     *
     * Might be used to check whether two references to {@link FcModelItem} classes represents
     * the same application (but instance of the application might be different)
     *
     * @param other Other model item to check
     *
     * @return
     */
    public synchronized boolean samePackageName(FcModelItem other) {
        return mPackageName.equals(other.mPackageName);
    }

    /**
     * @brief Mark given model item as the active item
     *
     * This might be used by the UI to display a marker to indicate that given application is
     * the one that displays floating controller
     *
     * @param active
     */
    public synchronized void setActive(boolean active) {
        mActive = active;
    }

    /**
     * @brief Check whether given model item is expanded in the Floating Controller
     *
     * @return
     */
    public synchronized boolean isExpanded() {
        return mExpanded;
    }

    /**
     *
     * @return
     */
    public synchronized boolean isScrollFocus() {
        return mScrollFocus;
    }
    /**
     * @brief Mark given model item as expanded in the Floating Controller
     *
     * @return
     */
    public synchronized void setExpanded(boolean isExpanded) {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "setExpanded(" + mInstanceId + ") = " + isExpanded);
        }
        mExpanded = isExpanded;
    }

    /**
     *
     * @param scrollFocus
     */
    public synchronized void setScrollFocus(boolean scrollFocus) {
        mScrollFocus = scrollFocus;
    }

    /**
     * @param appInfo
     */
    public synchronized void update(FcContext context, SapaAppInfo appInfo) {
        Log.d(TAG, "Updating model for " + mInstanceId);
        try {
            mIcon = appInfo.getIcon(context.getContext());
        } catch (NameNotFoundException e) {
            LogUtils.throwable(TAG, "Drawable not set: name not found", e);
        }

        clearActions();
        prepareActions(context, appInfo);
    }

    private void clearActions() {
        mVolumeActions[VOLUME_UP_INDEX] = null;
        mVolumeActions[VOLUME_DOWN_INDEX] = null;
        mCallActions.clear();
        mReturnActions.clear();
    }

    private void prepareActions(FcContext fcContext, SapaAppInfo info) {
        prepareReturnActions(fcContext, info);
        prepareCallActions(fcContext, info);
        if (mDirection == LinearLayout.LAYOUT_DIRECTION_RTL) {
            reverseActions();
        }
    }

    private void prepareCallActions(FcContext fcContext, SapaAppInfo appInfo) {
        FcActionFactory factory = fcContext.getActionFactory();
        SparseArray<SapaActionInfo> actions = appInfo.getActions();
        for (int i = 0; i < actions.size(); ++i) {
            SapaActionInfo actionInfo = actions.get(i);
            FcActionItem action = factory.newAppItem(appInfo, actionInfo);
            String actionName = actionInfo.getId();

            // Volume buttons needs to be separated from the rest of actions
            if (FcConstants.ACTION_VOLUME_UP.equals(actionName)) {
                mVolumeActions[VOLUME_UP_INDEX] = action;
            } else if (FcConstants.ACTION_VOLUME_DOWN.equals(actionName)) {
                mVolumeActions[VOLUME_DOWN_INDEX] = action;
            } else {
                mCallActions.add(action);
            }
        }
    }

    private void reverseActions() {
        Collections.reverse(mCallActions);
        List<FcActionItem> volumeActionsList = Arrays.asList(mVolumeActions);
        Collections.reverse(volumeActionsList);
        mVolumeActions = (FcActionItem[]) volumeActionsList.toArray();
        Collections.reverse(mReturnActions);
    }

    private void prepareReturnActions(FcContext fcContext, SapaAppInfo info) {
        Bundle config = info.getConfiguration();
        if (null != config) {
            int[] retButtonsIds = config.getIntArray(FcConstants.KEY_RETURN_BUTTONS);
            int[] retButtonsOpts = config.getIntArray(FcConstants.KEY_RETURN_BUTTONS_OPTS);

            if (retButtonsIds == null || retButtonsOpts == null) {
                prepareDefaultReturnActions(fcContext, info);

            } else if (retButtonsIds.length == retButtonsOpts.length) {
                prepareCustomReturnActions(fcContext, info, retButtonsIds, retButtonsOpts);

            } else {
                Log.w(TAG, "Sizes of arrays: " + FcConstants.KEY_RETURN_BUTTONS + " and "
                        + FcConstants.KEY_RETURN_BUTTONS_OPTS + " are not equal");
                prepareDefaultReturnActions(fcContext, info);
            }
        } else {
            prepareDefaultReturnActions(fcContext, info);
        }
    }

    private void prepareCustomReturnActions(FcContext fcContext, SapaAppInfo info, int[] drawableIds,
                                            int[] activityIds) {
        FcActionFactory factory = fcContext.getActionFactory();
        for (int i = 0; i < drawableIds.length; ++i) {
            int drawableId = drawableIds[i];
            int activityId = activityIds[i];

            mReturnActions.add(factory.newCustomReturnItem(info, drawableId, activityId));
        }
    }

    private void prepareDefaultReturnActions(FcContext fcContext, SapaAppInfo info) {
        FcActionFactory factory = fcContext.getActionFactory();
        mReturnActions.add(factory.newDefaultReturnItem(info));
    }

    @Override
    public String toString() {
        return "FcModelItem<" + mInstanceId
                + ((mActive ? " ACTIVE" : ""))
                + ((mExpanded ? " " : " NOT_")) + "EXPANDED"
                + ">";
    }

}
