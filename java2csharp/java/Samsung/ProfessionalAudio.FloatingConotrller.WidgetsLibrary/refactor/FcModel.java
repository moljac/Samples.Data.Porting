package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.util.LayoutDirection;
import android.util.Log;

import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

import java.security.InvalidParameterException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * @brief Floating controller model class
 */
public class FcModel implements FcSapaModel, FcAdapterModel {

    private static final int DEFAULT_APP_CAPACITY = 25;
    private static final String TAG = FcModel.class.getSimpleName();

    /** @brief represents the running application */
    private String mActiveApplication;

    /** @brief data structure to hold data representing active applications */
    private Map<String, FcModelItem> mApplicationMap;

    /** @brief represents main (host) application */
    private String mMainApplication;

    /** @brief Object to be notified when model is changed */
    private FcModelChangedListener mModelChangedListener;

    /** @brief used to preserve track order */
    private List<String> mOrderedApplications;

    /** @brief used to determine which apps are freeze */
    private List<String> mFreezeApplications;

    /** @brief Context of android application */
    private final FcContext mFcContext;

    /** @brief Current item direction */
    private int mCurrentDirection = LayoutDirection.LTR;

    /**
     * @brief Default constructor
     */
    public FcModel(FcContext fcContext) {
        mFcContext = fcContext;
        mApplicationMap = new HashMap<String, FcModelItem>(DEFAULT_APP_CAPACITY);
        mOrderedApplications = new ArrayList<String>(DEFAULT_APP_CAPACITY);
    }

    //
    // FcAdapterModel interface
    //

    @Override
    public int getInstanceNumber(FcModelItem item) {
        // Ordered list could be used to access instanceId faster, but this method is
        // more readable and refactor-friendly.
        String instanceId = item.getInstanceId();
        return Character.digit(instanceId.charAt(instanceId.length() - 1), 10);
    }

    @Override
    public synchronized FcModelItem getItem(int position) {
        return mApplicationMap.get(getInstanceId(position));
    }

    @Override
    public synchronized int getItemCount() {
        return mApplicationMap.size();
    }

    @Override
    public synchronized int getItemType(int position) {
        return (null != mMainApplication && position == 0)
                ? FcConstants.APP_TYPE_MAIN
                : FcConstants.APP_TYPE_ORDINAL;
    }

    @Override
    public synchronized boolean isMultiInstance(FcModelItem srcItem) {
        int count = 0;
        // No reason to cache counters, there's only (at max) 25 items
        for (FcModelItem item : mApplicationMap.values()) {
            if (item.samePackageName(srcItem)) {
                ++count;
            }
        }

        return count > 1;
    }

    @Override
    public synchronized void setFcModelChangedListener(FcModelChangedListener listener) {
        mModelChangedListener = listener;
    }

    @Override
    public synchronized void changeItemDirection(int layoutDirection) {
        mCurrentDirection = layoutDirection;
        for (FcModelItem item : mApplicationMap.values()) {
            item.setDirection(layoutDirection);
        }
        notifyModelInvalidated();
    }

    @Override
    public synchronized FcModelItem getExpandedItem() {
        final int itemCount = getItemCount();

        for (int i = 0; i < itemCount; ++i) {
            FcModelItem item = getItem(i);
            if (item.isExpanded()) {
                return item;
            }
        }

        return null;
    }

    @Override
    public synchronized int getItemPosition(FcModelItem item) {
        int count = getItemCount();
        for (int i = 0; i < count; ++i) {
            FcModelItem otherItem = getItem(i);
            if (item.getInstanceId().equals(otherItem.getInstanceId())) {
                return i;
            }
        }
        return -1;
    }

    //
    // FcSapaModel interface
    //

    @Override
    public synchronized void clear() {
        mMainApplication = null;
        mActiveApplication = null;
        mOrderedApplications.clear();
        mApplicationMap.clear();

        notifyModelInvalidated();
    }

    @Override
    public synchronized void insertApp(SapaAppInfo appInfo, int mode) {
        FcModelItem item = null;
        String instanceId = null;
        int position = -1;
        if (mode == FcConstants.APP_TYPE_ORDINAL) {
            item = FcModelItem.createOrdinal(mFcContext, appInfo);
            instanceId = item.getInstanceId();
            mOrderedApplications.add(instanceId);
            position = mOrderedApplications.size() - 1;

        } else if (mode == FcConstants.APP_TYPE_MAIN){
            item = FcModelItem.createMain(mFcContext, appInfo);
            instanceId = item.getInstanceId();
            mMainApplication = instanceId;
            position = 0;

        } else {
            throw new InvalidParameterException("Invalid mode: " + mode);
        }

        if (null == instanceId) {
            throw new IllegalStateException("Model item not created: null reference!");
        }
        if (isActiveApp(instanceId)) {
            item.setActive(true);
        }
        item.setDirection(mCurrentDirection);

        mApplicationMap.put(instanceId, item);

        Log.d(TAG, "Inserting application to the model: " + instanceId);
        if (null != mModelChangedListener && position >= 0) {
            mModelChangedListener.onFcModelItemInserted(position);
        }
    }

    @Override
    public synchronized void orderApps(List<String> newOrder) {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.v(TAG, "New apps order (size: " + newOrder.size() + ")");
            for (String instanceId : newOrder) {
                Log.d(TAG, "    " + instanceId);
            }

            Log.v(TAG, "Old apps order (size: " + mOrderedApplications.size() + ")");
            for (String instanceId : mOrderedApplications) {
                Log.d(TAG, "    " + instanceId);
            }
        }

        if (newOrder.size() != mOrderedApplications.size()) {
            Log.w(TAG, "Logic error: sizes of current and new app lists don't match");
            return;
        }

        List<String> currentOrder = new ArrayList<String>(mOrderedApplications);
        currentOrder.retainAll(newOrder);

        if (newOrder.size() != currentOrder.size()) {
            Log.w(TAG, "Logic error: current and new app lists don't contain the same elements");
            return;
        }


        mOrderedApplications = newOrder;
        notifyModelInvalidated();
    }

    @Override
    public synchronized void setFreezeApps(List<String> freezeApps) {
        mFreezeApplications = freezeApps;
        notifyModelInvalidated();
    }

    @Override
    public synchronized void removeApp(String instanceId, int mode) {
        int position;
        if (mode == FcConstants.APP_TYPE_MAIN) {
            if (!isMainApp(instanceId)) {
                throw new IllegalArgumentException("Invalid instanceId of the main application");
            }
            mMainApplication = null;
            position = 0;

        } else if (mode == FcConstants.APP_TYPE_ORDINAL) {
            position = mOrderedApplications.indexOf(instanceId);
            mOrderedApplications.remove(instanceId);

            if (null != mMainApplication) {
                position += 1;
            }

        } else {
            throw new IllegalArgumentException("Unsupported mode: " + mode);
        }

        mApplicationMap.remove(instanceId);
        if (isActiveApp(instanceId)) {
            mActiveApplication = null;
        }

        Log.d(TAG, "Removing application from the model: " + instanceId);
        if (null != mModelChangedListener && position >= 0) {
            mModelChangedListener.onFcModelItemRemoved(position);
        }
    }

    @Override
    public synchronized void updateApp(SapaAppInfo appInfo, int mode) {
        String instanceId = appInfo.getApp().getInstanceId();
        Log.d(TAG, "updateApp(" + instanceId + ", mode: " + mode + ")");

        FcModelItem item = getItemForInstanceId(appInfo.getApp().getInstanceId());
        if (null == item) {
            Log.w(TAG, "Cannot update app " + instanceId + ": unknown app");
            return;
        }

        int position;
        if (mode == FcConstants.APP_TYPE_MAIN) {
            position = 0;

        } else if (mode == FcConstants.APP_TYPE_ORDINAL) {
            position = mOrderedApplications.indexOf(instanceId);
            if (position < 0) {
                Log.w(TAG, "Cannot update app " + instanceId + ": not found on ordered list");
                return;
            }
            if (null != mMainApplication) {
                position += 1;
            }

        } else {
            throw new InvalidParameterException("Unsupported mode " + mode);
        }

        // This needs to be updated after all the logic guards
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "Updating application in a model: " + instanceId);
        }
        item.update(mFcContext, appInfo);

        if (null != mModelChangedListener) {
            mModelChangedListener.onFcModelChanged(position);
        }
    }

    private synchronized FcModelItem getItemForInstanceId(String instanceId) {
        return mApplicationMap.get(instanceId);
    }

    @Override
    public synchronized void setActiveApp(SapaAppInfo appInfo) {
        Log.d(TAG, "Set active app");
        if (null == appInfo || null == appInfo.getApp()) {
            Log.w(TAG, "Calling setActiveApp with null appInfo");
            return;
        }

        String instanceId = appInfo.getApp().getInstanceId();
        for (FcModelItem item : mApplicationMap.values()) {
            item.setActive(false);
        }

        FcModelItem item = mApplicationMap.get(instanceId);
        if (null == item) {
            Log.w(TAG, "Not setting active app: no application for instance " + instanceId);
            return;
        }

        item.setActive(true);
        mActiveApplication = instanceId;

        // Updating whole model as the active app has been changed
        notifyModelInvalidated();
    }

    @Override
    public synchronized boolean isAppFreeze(String instanceId) {
        return getFreezeApplications().contains(instanceId);
    }

    //
    // Privates
    //

    private String getInstanceId(int position) {
        if (null == mMainApplication) {
            return mOrderedApplications.get(position);
        }

        return (position == 0)
                ? mMainApplication
                : mOrderedApplications.get(position - 1);
    }

    /**
     * @brief Call to check if the current active application (the one being displayed)
     * matches given <code>instanceId</code>.
     *
     * @param instanceId
     * @return
     */
    private boolean isActiveApp(String instanceId) {
        return mActiveApplication != null && mActiveApplication.equals(instanceId);
    }

    /**
     * @brief Call to check if the main application matches given <code>instanceId</code>
     *
     * @param instanceId
     * @return
     */
    private boolean isMainApp(String instanceId) {
        return mMainApplication != null && mMainApplication.equals(instanceId);
    }

    private void notifyModelInvalidated() {
        if (null != mModelChangedListener) {
            mModelChangedListener.onFcModelChanged();
        }
    }

    public synchronized List<String> getFreezeApplications() {
        if (mFreezeApplications == null) {
            mFreezeApplications = new ArrayList<String>();
        }
        return mFreezeApplications;
    }
}
