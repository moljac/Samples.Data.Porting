namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	/// <summary>
	/// @brief Interface to call remote actions using SAPA service
	/// </summary>
	public interface FcSapaActionDispatcher
	{

		/// <summary>
		/// @brief Call action of given <code>actionId</code> using given instance of
		///        remote application
		/// </summary>
		/// <param name="instanceId">    unique instance id </param>
		/// <param name="actionId">      id of the action to call </param>
		void callAction(string instanceId, string actionId);

	}

}