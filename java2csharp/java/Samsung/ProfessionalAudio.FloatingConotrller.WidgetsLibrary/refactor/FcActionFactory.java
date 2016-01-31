package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.util.Log;

import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

import com.samsung.android.sdk.professionalaudio.widgets.R;

/**
 * @brief Factory class to simplify creation of action items
 *
 * The main responsibility is to hide details on how to create and setup action
 * item instances. Clients of this class should have no knowledge on how to initialize action
 * instances, and classes implementing {@link FcActionItem} interface should be known only to
 * <code>FcActionFactory</code>.
 *
 * The class initializes various fields such as icon drawable or action name, but also
 * creates a <code>Runnable</code> to be run when action is executed.
 */
public class FcActionFactory {

    private static final String TAG = FcActionFactory.class.getSimpleName();
    private static final int DEFAULT_ACTIVITY_ID = 1;
    private static final int DEFAULT_RES_ID = R.drawable.icon_bubble_goto;
    private final FcContext mFcContext;
    private int mDefaultResId = DEFAULT_RES_ID;

    /**
     * @brief Constructor initializing factory from the FcContext
     *
     * Note: Reference to FcActionFactory inside FcContext is changed by this constructor
     *
     * @param fcContext     Context to the FloatingController common internals
     */
    public FcActionFactory(FcContext fcContext) {
        mFcContext = fcContext;
    }

    /**
     * @brief Create action item describing single action of the application
     *
     * An example of the action could be a "volume up" or "volume down" action.
     *
     * @param appInfo       Sapa structure describing owner of the action
     * @param actionInfo    Sapa structure describing action details
     *
     * @return  new instance of the FcActionItem
     */
    public FcActionItem newAppItem(SapaAppInfo appInfo, SapaActionInfo actionInfo) {
        FcActionAppItem item = new FcActionAppItem(actionInfo);

        SapaApp app = appInfo.getApp();
        item.setActionRunnable(newCallActionRunnable(app.getInstanceId(), actionInfo.getId()));

        return item;
    }

    /**
     * @brief Create action item describing a default return action
     *
     * When action is executed, the default activity of the application will be opened.
     *
     * @param appInfo       Sapa structure describing owner of the action
     *
     * @return  new instance of the FcActionItem
     */
    public FcActionItem newDefaultReturnItem(SapaAppInfo appInfo) {

        SapaApp app = appInfo.getApp();

        FcActionReturnItem item = new FcActionReturnItem(appInfo);
        item.setDefault(true);
        item.setDrawableId(mDefaultResId);
        item.setActionRunnable(newOpenActivityRunnable(app.getPackageName(),
                app.getInstanceId(), DEFAULT_ACTIVITY_ID));

        return item;
    }

    /**
     * @brief Create action item describing a custom return action
     *
     * When action is executed, the activity described by a <code>mode</code> parameter
     * will be opened
     *
     * @param appInfo       Sapa structure describing application
     * @param resId         Id of drawable from resources of the given application
     * @param activityId    Id of the activity to be opened
     *
     * @return  new instance of the FcActionItem
     */
    public FcActionItem newCustomReturnItem(SapaAppInfo appInfo, int resId, int activityId) {
        SapaApp app = appInfo.getApp();

        FcActionReturnItem item = new FcActionReturnItem(appInfo);
        item.setDefault(false);
        item.setDrawableId(resId);
        item.setActionRunnable(newOpenActivityRunnable(app.getPackageName(),
                app.getInstanceId(), activityId));

        return item;
    }

    /**
     * @brief
     *
     * @param resId
     */
    public void setDefaultReturnDrawable(int resId) {
        mDefaultResId = resId;
    }

    /**
     * @brief Create action executor that requests application to execute action of a
     *        given <code>actionId</code>
     *
     * Action is executed using {@link FcSapaActionDispatcher} object.
     *
     * @param instanceId    Unique instance id of the application
     * @param actionId      Id of the action
     *
     * @return  Runnable of the action
     */
    private Runnable newCallActionRunnable(final String instanceId, final String actionId) {
        return new Runnable() {
            @Override
            public void run() {
                FcSapaActionDispatcher dispatcher = mFcContext.getSapaActionDispatcher();
                if (null == dispatcher) {
                    Log.w(TAG, "Logic error: Sapa action dispatcher is null");
                    return;
                }
                dispatcher.callAction(instanceId, actionId);
            }
        };
    }

    /**
     * @brief Create action executor that opens activity of the application
     *
     * Activity is determined by a given <code>activityId</code>.
     *
     * @param packageName   Package name of the application
     * @param instanceId    Unique instance id of the application
     * @param activityId    Id of the activity to be opened
     *
     * @return  Runnable of the action
     */
    private Runnable newOpenActivityRunnable(final String packageName, final String instanceId,
                                             final int activityId) {
        return new Runnable() {
            @Override
            public void run() {
                mFcContext.openSapaAppActivity(packageName, instanceId, activityId);
            }
        };
    }
}
