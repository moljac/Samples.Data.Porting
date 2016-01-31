package com.samsung.android.sdk.professionalaudio.widgets.refactor;

/**
 * @brief Interface to call remote actions using SAPA service
 */
public interface FcSapaActionDispatcher {

    /**
     * @brief Call action of given <code>actionId</code> using given instance of
     *        remote application
     *
     * @param instanceId    unique instance id
     * @param actionId      id of the action to call
     */
    void callAction(String instanceId, String actionId);

}
