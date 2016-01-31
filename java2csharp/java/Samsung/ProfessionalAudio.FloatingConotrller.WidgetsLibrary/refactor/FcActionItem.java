package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.graphics.drawable.Drawable;

/**
 * @brief Interface for floating controller actions
 *
 * Actions are divided in two types:
 *
 * "Call action" is an action that changes state of an application running in the background.
 * Example of such an action could be a "bypass" action in effects, which enable/disable DSP part
 * of an effect.
 *
 * "Return action" is an action that opens some activity in the application. It could be a simple
 * "move to default activity" action, but also more customized, such as "open mixer activity"
 * or "open multitrack activity".
 */
public interface FcActionItem {

    /**
     * @brief Return icon that represents the action
     *
     * The icon might be null in some cases (such as default return action)
     *
     * @param fcContext     Context to the FloatingController common internals
     *
     * @return  Customized drawable or null for default icons
     */
    Drawable getIcon(FcContext fcContext);

    /**
     * @brief Return executor of the action as Runnable object
     *
     * @return Runnable behind the action (non null)
     */
    Runnable getActionRunnable();

    /**
     * @brief Return name of the action
     *
     * It could be used to set the contentDescription parameter of the ImageButton that
     * represents the action.
     *
     * The name might null in some cases (such as default return action)
     *
     * @param fcContext     Context to the FloatingController common internals
     *
     * @return Name of the action if present, or null for not defined
     */
    String getName(FcContext fcContext);

    /**
     * @brief Return id of the action
     *
     * @return Id of the action
     */
    String getId();

    /**
     * @brief Checks whether the action is enabled
     *
     * It could be used to disable/enable UI element representing the action
     *
     * @return True if action is enabled, false otherwise
     */
    boolean isEnabled();

    /**
     * @brief Checks whether the UI element representing action should be visible
     *
     * @return true if action should be visible, false otherwise
     */
    boolean isVisible();

}
