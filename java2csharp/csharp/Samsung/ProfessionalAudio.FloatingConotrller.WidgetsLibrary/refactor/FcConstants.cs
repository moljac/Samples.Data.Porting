namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	/// <summary>
	/// @brief Groups together magic number and strings such as bundle keys or intent names
	/// </summary>
	public class FcConstants
	{
		public const string INTENT_SAPA_SWITCH = "com.samsung.android.sdk.professionalaudio.ACTION.SWITCH_TO_SAPA_APP";

		public const string INTENT_SAPA_SWITCH_EXTRAS_INSTANCEID = "com.samsung.android.sdk.professionalaudio.switchTo.instanceID";

		public const string INTENT_SAPA_SWITCH_EXTRAS_PACKAGE = "com.samsung.android.sdk.professionalaudio.switchTo.packageName";

		public const string PERMISSION_USE_CONNETION_SERVICE = "com.samsung.android.sdk.professionalaudio.permission.USE_CONNECTION_SERVICE";

		public const string KEY_RETURN_BUTTONS = "app_ret_buttons";
		public const string KEY_RETURN_BUTTONS_OPTS = "app_ret_buttons_options";

		public const string ACTION_VOLUME_UP = "volume_up";
		public const string ACTION_VOLUME_DOWN = "volume_down";

		/// <summary>
		/// @brief Constant describing main (host) application type </summary>
		public const int APP_TYPE_MAIN = 0;

		/// <summary>
		/// @brief Constant describing ordinal (instrument, effect) application type </summary>
		public const int APP_TYPE_ORDINAL = 1;

		/// <summary>
		/// @brief Constant describing no app, but spacer item; used in adapter </summary>
		public const int APP_TYPE_SPACER = 2;

		/// <summary>
		/// @brief Default duration of animation
		/// 
		/// Note: DEFAULT_FADE_ANIM_DURATION needs to be much lower than DEFAULT_ANIM_DURATION
		/// </summary>
		private const int ANIM_MULTIPLIER = 1;
		public static readonly int DEFAULT_ANIM_DURATION = 300 * ANIM_MULTIPLIER;
		public static readonly int DEFAULT_SCALE_ANIM_DURATION = 200 * ANIM_MULTIPLIER;
		public static readonly int DEFAULT_FADE_ANIM_DURATION = 100 * ANIM_MULTIPLIER;

		public const bool OPT_DETAILED_LOGS = false;
	}

}