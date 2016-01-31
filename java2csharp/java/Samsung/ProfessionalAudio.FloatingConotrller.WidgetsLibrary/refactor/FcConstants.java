package com.samsung.android.sdk.professionalaudio.widgets.refactor;

/**
 * @brief Groups together magic number and strings such as bundle keys or intent names
  */
public class FcConstants {
    public static final String INTENT_SAPA_SWITCH =
            "com.samsung.android.sdk.professionalaudio.ACTION.SWITCH_TO_SAPA_APP";

    public static final String INTENT_SAPA_SWITCH_EXTRAS_INSTANCEID =
            "com.samsung.android.sdk.professionalaudio.switchTo.instanceID";

    public static final String INTENT_SAPA_SWITCH_EXTRAS_PACKAGE =
            "com.samsung.android.sdk.professionalaudio.switchTo.packageName";

    public static final String PERMISSION_USE_CONNETION_SERVICE =
            "com.samsung.android.sdk.professionalaudio.permission.USE_CONNECTION_SERVICE";

    public static final String KEY_RETURN_BUTTONS = "app_ret_buttons";
    public static final String KEY_RETURN_BUTTONS_OPTS = "app_ret_buttons_options";

    public static final String ACTION_VOLUME_UP = "volume_up";
    public static final String ACTION_VOLUME_DOWN = "volume_down";

    /** @brief Constant describing main (host) application type */
    public static final int APP_TYPE_MAIN = 0;

    /** @brief Constant describing ordinal (instrument, effect) application type */
    public static final int APP_TYPE_ORDINAL = 1;

    /** @brief Constant describing no app, but spacer item; used in adapter */
    public static final int APP_TYPE_SPACER = 2;

    /**
     * @brief Default duration of animation
     *
     * Note: DEFAULT_FADE_ANIM_DURATION needs to be much lower than DEFAULT_ANIM_DURATION
     */
    private static final int ANIM_MULTIPLIER = 1;
    public static final int DEFAULT_ANIM_DURATION = 300 * ANIM_MULTIPLIER;
    public static final int DEFAULT_SCALE_ANIM_DURATION = 200 * ANIM_MULTIPLIER;
    public static final int DEFAULT_FADE_ANIM_DURATION = 100 * ANIM_MULTIPLIER;

    public static final boolean OPT_DETAILED_LOGS = false;
}
