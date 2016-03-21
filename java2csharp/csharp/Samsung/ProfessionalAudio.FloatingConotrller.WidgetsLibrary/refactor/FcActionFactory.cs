namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Log = android.util.Log;

	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

	/// <summary>
	/// @brief Factory class to simplify creation of action items
	/// 
	/// The main responsibility is to hide details on how to create and setup action
	/// item instances. Clients of this class should have no knowledge on how to initialize action
	/// instances, and classes implementing <seealso cref="FcActionItem"/> interface should be known only to
	/// <code>FcActionFactory</code>.
	/// 
	/// The class initializes various fields such as icon drawable or action name, but also
	/// creates a <code>Runnable</code> to be run when action is executed.
	/// </summary>
	public class FcActionFactory
	{

		private static readonly string TAG = typeof(FcActionFactory).Name;
		private const int DEFAULT_ACTIVITY_ID = 1;
		private static readonly int DEFAULT_RES_ID = R.drawable.icon_bubble_goto;
		private readonly FcContext mFcContext;
		private int mDefaultResId = DEFAULT_RES_ID;

		/// <summary>
		/// @brief Constructor initializing factory from the FcContext
		/// 
		/// Note: Reference to FcActionFactory inside FcContext is changed by this constructor
		/// </summary>
		/// <param name="fcContext">     Context to the FloatingController common internals </param>
		public FcActionFactory(FcContext fcContext)
		{
			mFcContext = fcContext;
		}

		/// <summary>
		/// @brief Create action item describing single action of the application
		/// 
		/// An example of the action could be a "volume up" or "volume down" action.
		/// </summary>
		/// <param name="appInfo">       Sapa structure describing owner of the action </param>
		/// <param name="actionInfo">    Sapa structure describing action details
		/// </param>
		/// <returns>  new instance of the FcActionItem </returns>
		public virtual FcActionItem newAppItem(SapaAppInfo appInfo, SapaActionInfo actionInfo)
		{
			FcActionAppItem item = new FcActionAppItem(actionInfo);

			SapaApp app = appInfo.App;
			item.ActionRunnable = newCallActionRunnable(app.InstanceId, actionInfo.Id);

			return item;
		}

		/// <summary>
		/// @brief Create action item describing a default return action
		/// 
		/// When action is executed, the default activity of the application will be opened.
		/// </summary>
		/// <param name="appInfo">       Sapa structure describing owner of the action
		/// </param>
		/// <returns>  new instance of the FcActionItem </returns>
		public virtual FcActionItem newDefaultReturnItem(SapaAppInfo appInfo)
		{

			SapaApp app = appInfo.App;

			FcActionReturnItem item = new FcActionReturnItem(appInfo);
			item.Default = true;
			item.DrawableId = mDefaultResId;
			item.ActionRunnable = newOpenActivityRunnable(app.PackageName, app.InstanceId, DEFAULT_ACTIVITY_ID);

			return item;
		}

		/// <summary>
		/// @brief Create action item describing a custom return action
		/// 
		/// When action is executed, the activity described by a <code>mode</code> parameter
		/// will be opened
		/// </summary>
		/// <param name="appInfo">       Sapa structure describing application </param>
		/// <param name="resId">         Id of drawable from resources of the given application </param>
		/// <param name="activityId">    Id of the activity to be opened
		/// </param>
		/// <returns>  new instance of the FcActionItem </returns>
		public virtual FcActionItem newCustomReturnItem(SapaAppInfo appInfo, int resId, int activityId)
		{
			SapaApp app = appInfo.App;

			FcActionReturnItem item = new FcActionReturnItem(appInfo);
			item.Default = false;
			item.DrawableId = resId;
			item.ActionRunnable = newOpenActivityRunnable(app.PackageName, app.InstanceId, activityId);

			return item;
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="resId"> </param>
		public virtual int DefaultReturnDrawable
		{
			set
			{
				mDefaultResId = value;
			}
		}

		/// <summary>
		/// @brief Create action executor that requests application to execute action of a
		///        given <code>actionId</code>
		/// 
		/// Action is executed using <seealso cref="FcSapaActionDispatcher"/> object.
		/// </summary>
		/// <param name="instanceId">    Unique instance id of the application </param>
		/// <param name="actionId">      Id of the action
		/// </param>
		/// <returns>  Runnable of the action </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private Runnable newCallActionRunnable(final String instanceId, final String actionId)
		private Runnable newCallActionRunnable(string instanceId, string actionId)
		{
			return () =>
			{
				FcSapaActionDispatcher dispatcher = mFcContext.SapaActionDispatcher;
				if (null == dispatcher)
				{
					Log.w(TAG, "Logic error: Sapa action dispatcher is null");
					return;
				}
				dispatcher.callAction(instanceId, actionId);
			};
		}

		/// <summary>
		/// @brief Create action executor that opens activity of the application
		/// 
		/// Activity is determined by a given <code>activityId</code>.
		/// </summary>
		/// <param name="packageName">   Package name of the application </param>
		/// <param name="instanceId">    Unique instance id of the application </param>
		/// <param name="activityId">    Id of the activity to be opened
		/// </param>
		/// <returns>  Runnable of the action </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private Runnable newOpenActivityRunnable(final String packageName, final String instanceId, final int activityId)
		private Runnable newOpenActivityRunnable(string packageName, string instanceId, int activityId)
		{
			return () =>
			{
				mFcContext.openSapaAppActivity(packageName, instanceId, activityId);
			};
		}
	}

}