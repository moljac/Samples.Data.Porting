package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

import java.util.List;

/**
 * @brief Interface for model to communicate with Sapa API
 */
public interface FcSapaModel {

    /**
     * @brief Register new application in the model
     *
     * The application might be either main or ordinary, depending on the <code>mode</code>
     * parameter.
     *
     * @param appInfo
     * @param mode
     */
    void insertApp(SapaAppInfo appInfo, int mode);

    /**
     * @brief Unregister application from the model
     *
     * @param instanceId
     * @param mode
     */
    void removeApp(String instanceId, int mode);

    /**
     * @brief Update application from the model
     *
     * @param appInfo
     * @param mode
     */
    void updateApp(SapaAppInfo appInfo, int mode);

    /**
     * @brief Clear the model to the empty state
     *
     * It might be called when main application is shut down
     */
    void clear();

    /**
     * Order applications to display them properly in FloatingController
     *
     * @param trackOrder applications order according to their position on tracks
     */
    void orderApps(List<String> trackOrder);

    /**
     * @brief Set given application as the active one
     *
     * Active application is the one that displays floating controller
     *
     * @param appInfo
     */
    void setActiveApp(SapaAppInfo appInfo);

    void setFreezeApps(List<String> freezeList);
}
