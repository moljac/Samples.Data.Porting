namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Drawable = android.graphics.drawable.Drawable;

	/// <summary>
	/// @brief Interface for floating controller actions
	/// 
	/// Actions are divided in two types:
	/// 
	/// "Call action" is an action that changes state of an application running in the background.
	/// Example of such an action could be a "bypass" action in effects, which enable/disable DSP part
	/// of an effect.
	/// 
	/// "Return action" is an action that opens some activity in the application. It could be a simple
	/// "move to default activity" action, but also more customized, such as "open mixer activity"
	/// or "open multitrack activity".
	/// </summary>
	public interface FcActionItem
	{

		/// <summary>
		/// @brief Return icon that represents the action
		/// 
		/// The icon might be null in some cases (such as default return action)
		/// </summary>
		/// <param name="fcContext">     Context to the FloatingController common internals
		/// </param>
		/// <returns>  Customized drawable or null for default icons </returns>
		Drawable getIcon(FcContext fcContext);

		/// <summary>
		/// @brief Return executor of the action as Runnable object
		/// </summary>
		/// <returns> Runnable behind the action (non null) </returns>
		Runnable ActionRunnable {get;}

		/// <summary>
		/// @brief Return name of the action
		/// 
		/// It could be used to set the contentDescription parameter of the ImageButton that
		/// represents the action.
		/// 
		/// The name might null in some cases (such as default return action)
		/// </summary>
		/// <param name="fcContext">     Context to the FloatingController common internals
		/// </param>
		/// <returns> Name of the action if present, or null for not defined </returns>
		string getName(FcContext fcContext);

		/// <summary>
		/// @brief Return id of the action
		/// </summary>
		/// <returns> Id of the action </returns>
		string Id {get;}

		/// <summary>
		/// @brief Checks whether the action is enabled
		/// 
		/// It could be used to disable/enable UI element representing the action
		/// </summary>
		/// <returns> True if action is enabled, false otherwise </returns>
		bool Enabled {get;}

		/// <summary>
		/// @brief Checks whether the UI element representing action should be visible
		/// </summary>
		/// <returns> true if action should be visible, false otherwise </returns>
		bool Visible {get;}

	}

}