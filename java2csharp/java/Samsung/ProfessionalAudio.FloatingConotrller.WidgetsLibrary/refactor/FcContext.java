package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.res.Resources;
import android.graphics.drawable.Drawable;
import android.os.Handler;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;

import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

import java.lang.ref.WeakReference;
import java.util.HashMap;
import java.util.Map;

/**
 * @brief Android context hidden behind methods to be used within FloatingController
 *
 * The methods are used to retrieve various external information, such as application
 * name taken from PackageManager, or drawables of external applications.
 */
public class FcContext {

    public interface FcContextStateChanged {
        void onActivityFinished();
    }

    private static final String TAG = FcContext.class.getSimpleName();
    private static final int NO_FLAGS = 0;
    private final WeakReference<Context> mContext;
    private final Handler mHandler;
    private FcSapaServiceConnector mSapaServiceConnector;
    private FcActionFactory mActionFactory;
    private SapaAppInfo mActiveApp;
    private WeakReference<FcContextStateChanged> mListener = new WeakReference<FcContextStateChanged>(null);

    /**
     * @brief Constructor of FloatingController context class
     *
     * @param context Android context
     */
    public FcContext(Context context) {
        mContext = new WeakReference<Context>(context);
        mHandler = new Handler(context.getMainLooper());
    }

    /**
     * @brief Get a factory to create and setup actions
     *
     * Throws an IllegalStateException if FcActionFactory has not been set
     *
     * @return FcActionFactory (non null)
     */
    public FcActionFactory getActionFactory() {
        if (null == mActionFactory) {
            throw new IllegalStateException("Accessing null action factory");
        }
        return mActionFactory;
    }

    /**
     * @brief Get android context
     *
     * @return Context of the current android application (might be null)
     */
    public Context getContext() {
        return mContext.get();
    }

    /**
     * @brief Read application name (label) from package manager
     *
     * @param packageName   application package
     *
     * @return Application name retrieved from PackageManager
     */
    public String getApplicationName(String packageName) {
        String appName = "<None>";
        Context context = getContext();
        if (null == context) {
            Log.w(TAG, "Cannot retrieve application name: context is null");
            return appName;
        }

        PackageManager pm = context.getPackageManager();
        ApplicationInfo appInfo = null;
        try {
            appInfo = pm.getApplicationInfo(packageName, NO_FLAGS);

        } catch (NameNotFoundException e) {
            Log.w(TAG, "Cannot retrieve application info for package: " + packageName);
        }

        if (null != appInfo) {
            appName = pm.getApplicationLabel(appInfo).toString();
        }
        return appName;
    }

    /**
     * @brief Retrieve drawable from resources of application given by package name
     *
     * Drawable is retrieved using PackageManager.
     *
     * @param packageName   package name of the application to retrieve drawables from
     * @param drawableId    id of the drawable to retrieve
     *
     * @return Drawable or null in case of problems
     */
    public Drawable getApplicationDrawable(String packageName, int drawableId) {
        Context context = getContext();
        if (null == context) {
            Log.w(TAG, "Cannot retrieve drawable from application (" + packageName + ") " +
                    "as the context is null");
            return null;
        }

        if (drawableId < 0) {
            Log.w(TAG, "Cannot retrieve drawable of negative Id");
            return null;
        }

        PackageManager pm = context.getPackageManager();
        Drawable drawable = null;
        try {
            Resources res = pm.getResourcesForApplication(packageName);
            drawable = res.getDrawable(drawableId);

        } catch (NameNotFoundException e) {
            Log.w(TAG, "Cannot retrieve drawable from application (" + packageName + "): " +
                "name not found");
        }

        return drawable;
    }

    /**
     * @brief
     *
     * @param resId
     * @return
     */
    public float getDimension(int resId) {
        Context context = mContext.get();
        if (null == context) {
            throw new IllegalStateException("Context is null");
        }
        return context.getResources().getDimension(resId);
    }

    /**
     * @brief
     *
     * @param resId
     * @return
     */
    public float getDimensionPixelSize(int resId) {
        Context context = mContext.get();
        if (null == context) {
            throw new IllegalStateException("Context is null");
        }
        return context.getResources().getDimensionPixelSize(resId);
    }

    /**
     * @brief Return dispatcher for remote actions
     *
     * @return FcSapaActionDispatcher implementation
     */
    public FcSapaActionDispatcher getSapaActionDispatcher() {
        return mSapaServiceConnector;
    }

    /**
     * @brief TODO
     *
     * @param resId
     * @param parent
     * @param <T>
     *
     * @return
     */
    public <T extends View> T inflate(int resId, ViewGroup parent) {
        return inflate(resId, parent, false);
    }

    /**
     * @brief TODO
     *
     * @param resId
     * @param parent
     * @param isAdded
     * @param <T>
     *
     * @return
     */
    public <T extends View> T inflate(int resId, ViewGroup parent, boolean isAdded) {
        return (T) LayoutInflater.from(mContext.get()).inflate(resId, parent, isAdded);
    }

    /**
     * @brief TODO
     *
     * @param packageName
     * @param instanceId
     * @param mode
     */
    public void openSapaAppActivity(String packageName, String instanceId, int mode) {
        if(mActiveApp == null || !mActiveApp.getApp().getInstanceId().equals(instanceId)) {
            FcContextStateChanged listener = mListener.get();
            if(listener != null) {
                listener.onActivityFinished();
            }
            Log.d(TAG, "Open Sapa app");
            if (!startSapaActivity(instanceId, mode)) {
                return;
            }
            if (!sendSapaSwitchBroadcast(packageName, instanceId)) {
                return;
            }

            finishCurrentActivity(packageName);
        }
    }

    public void setFxContextStateChangeListener(FcContextStateChanged listener) {
        mListener = new WeakReference<FcContextStateChanged>(listener);
    }

    /**
     * @brief Broadcast com.samsung.android.sdk.professionalaudio.ACTION.SWITCH_TO_SAPA_APP intent
     *
     * TODO: Describe what's the intent for
     *
     * @param packageName
     * @param instanceId
     */

    public void setActiveApp(SapaAppInfo sapaAppInfo) {
        mActiveApp = sapaAppInfo;
    }
    private boolean sendSapaSwitchBroadcast(String packageName, String instanceId) {

        Context context = getContext();
        if (null == context) {
            Log.w(TAG, "Cannot send sapa switch broadcast (pkg: " + packageName
                    + ", instance: " + instanceId + "): context is null");
            return false;
        }

        Intent intent = new Intent(FcConstants.INTENT_SAPA_SWITCH);
        intent.putExtra(FcConstants.INTENT_SAPA_SWITCH_EXTRAS_INSTANCEID, instanceId);
        intent.putExtra(FcConstants.INTENT_SAPA_SWITCH_EXTRAS_PACKAGE, packageName);

        context.sendBroadcast(intent, FcConstants.PERMISSION_USE_CONNETION_SERVICE);

        return true;
    }

    /**
     * @brief Update reference to FcSapaServiceConnector object
     *
     * @param connector     Connector with Sapa services
     */
    public void setSapaServiceConnector(FcSapaServiceConnector connector) {
        mSapaServiceConnector = connector;
    }

    /**
     * @brief Starts the activity in the sapa environment
     *
     * Setup intent by adding custom class loader and extras edit_mode
     * TODO: Description on what's the edit_mode
     *
     * @param mode
     */
    private boolean startSapaActivity(String instanceId, int mode) {
        Context context = getContext();
        Log.d(TAG, "Start sapa app");
        Log.d(TAG, "instanceId:" + instanceId);
        if (null == context) {
            Log.w(TAG, "Cannot start activity for instance " + instanceId
                    + ": null context");
            return false;
        }

        if (null == mSapaServiceConnector) {
            Log.w(TAG, "Cannot start activity for instance " + instanceId
                    + ": service connector not set to the FC context");
            return false;
        }

        Intent intent = mSapaServiceConnector.getLaunchIntent(instanceId);
        if (null == intent) {
            Log.w(TAG, "Cannot start activity for instance " + instanceId
                    + ": launch intent is null");
            return false;
        }

        intent.setExtrasClassLoader(SapaAppInfo.class.getClassLoader());
        intent.putExtra("Edit_mode", mode);
        intent.setFlags(Intent.FLAG_ACTIVITY_BROUGHT_TO_FRONT);

        context.startActivity(intent);

        return true;
    }

    /**
     * @brief Finish current activity if the package name is matched
     *
     * @param packageName   Package name of the activity to stop
     */
    private boolean finishCurrentActivity(String packageName) {
        Context context = getContext();
        if (null == context) {
            Log.w(TAG, "Cannot finish activity " + packageName + ": context is null");
            return false;
        }

        if (!(context instanceof Activity)) {
            Log.w(TAG, "Cannot finish activity: " + packageName
                    + ": context not an instance of Activity");
            return false;
        }

        if (!context.getPackageName().contentEquals(packageName)) {
            ((Activity) context).finish();
        }
        return true;
    }

    /**
     * @brief Execute runnable on the main thread
     *
     * @param runnable
     */
    public void runOnMainThread(Runnable runnable) {
        mHandler.post(runnable);
    }

    /**
     * @brief Update reference to FcActionFactory object
     *
     * @param factory   reference to FcActionFactory
     */
    public void setActionFactory(FcActionFactory factory) {
        mActionFactory = factory;
    }

    /**
     * @brief Returns {@link Drawable} for given resource ID
     *
     * @param resId Resource ID
     * @return {@link Drawable} for given resource ID
     */
    public Drawable getDrawable(final int resId) {
        Context context = getContext();
        if (null == context) {
            Log.w(TAG, "Cannot retrieve Drawable for resource ID=" + resId
                    + " as the context is null");
            return null;
        }

        return context.getResources().getDrawable(resId);
    }
}
