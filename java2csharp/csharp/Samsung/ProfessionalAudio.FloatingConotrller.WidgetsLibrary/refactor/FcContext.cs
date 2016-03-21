namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Activity = android.app.Activity;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ApplicationInfo = android.content.pm.ApplicationInfo;
	using PackageManager = android.content.pm.PackageManager;
	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Resources = android.content.res.Resources;
	using Drawable = android.graphics.drawable.Drawable;
	using Handler = android.os.Handler;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;

	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;


	/// <summary>
	/// @brief Android context hidden behind methods to be used within FloatingController
	/// 
	/// The methods are used to retrieve various external information, such as application
	/// name taken from PackageManager, or drawables of external applications.
	/// </summary>
	public class FcContext
	{

		public interface FcContextStateChanged
		{
			void onActivityFinished();
		}

		private static readonly string TAG = typeof(FcContext).Name;
		private const int NO_FLAGS = 0;
		private readonly WeakReference<Context> mContext;
		private readonly Handler mHandler;
		private FcSapaServiceConnector mSapaServiceConnector;
		private FcActionFactory mActionFactory;
		private SapaAppInfo mActiveApp;
		private WeakReference<FcContextStateChanged> mListener = new WeakReference<FcContextStateChanged>(null);

		/// <summary>
		/// @brief Constructor of FloatingController context class
		/// </summary>
		/// <param name="context"> Android context </param>
		public FcContext(Context context)
		{
			mContext = new WeakReference<Context>(context);
			mHandler = new Handler(context.MainLooper);
		}

		/// <summary>
		/// @brief Get a factory to create and setup actions
		/// 
		/// Throws an IllegalStateException if FcActionFactory has not been set
		/// </summary>
		/// <returns> FcActionFactory (non null) </returns>
		public virtual FcActionFactory ActionFactory
		{
			get
			{
				if (null == mActionFactory)
				{
					throw new System.InvalidOperationException("Accessing null action factory");
				}
				return mActionFactory;
			}
			set
			{
				mActionFactory = value;
			}
		}

		/// <summary>
		/// @brief Get android context
		/// </summary>
		/// <returns> Context of the current android application (might be null) </returns>
		public virtual Context Context
		{
			get
			{
				return mContext.get();
			}
		}

		/// <summary>
		/// @brief Read application name (label) from package manager
		/// </summary>
		/// <param name="packageName">   application package
		/// </param>
		/// <returns> Application name retrieved from PackageManager </returns>
		public virtual string getApplicationName(string packageName)
		{
			string appName = "<None>";
			Context context = Context;
			if (null == context)
			{
				Log.w(TAG, "Cannot retrieve application name: context is null");
				return appName;
			}

			PackageManager pm = context.PackageManager;
			ApplicationInfo appInfo = null;
			try
			{
				appInfo = pm.getApplicationInfo(packageName, NO_FLAGS);

			}
			catch (PackageManager.NameNotFoundException)
			{
				Log.w(TAG, "Cannot retrieve application info for package: " + packageName);
			}

			if (null != appInfo)
			{
				appName = pm.getApplicationLabel(appInfo).ToString();
			}
			return appName;
		}

		/// <summary>
		/// @brief Retrieve drawable from resources of application given by package name
		/// 
		/// Drawable is retrieved using PackageManager.
		/// </summary>
		/// <param name="packageName">   package name of the application to retrieve drawables from </param>
		/// <param name="drawableId">    id of the drawable to retrieve
		/// </param>
		/// <returns> Drawable or null in case of problems </returns>
		public virtual Drawable getApplicationDrawable(string packageName, int drawableId)
		{
			Context context = Context;
			if (null == context)
			{
				Log.w(TAG, "Cannot retrieve drawable from application (" + packageName + ") " + "as the context is null");
				return null;
			}

			if (drawableId < 0)
			{
				Log.w(TAG, "Cannot retrieve drawable of negative Id");
				return null;
			}

			PackageManager pm = context.PackageManager;
			Drawable drawable = null;
			try
			{
				Resources res = pm.getResourcesForApplication(packageName);
				drawable = res.getDrawable(drawableId);

			}
			catch (PackageManager.NameNotFoundException)
			{
				Log.w(TAG, "Cannot retrieve drawable from application (" + packageName + "): " + "name not found");
			}

			return drawable;
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="resId">
		/// @return </param>
		public virtual float getDimension(int resId)
		{
			Context context = mContext.get();
			if (null == context)
			{
				throw new System.InvalidOperationException("Context is null");
			}
			return context.Resources.getDimension(resId);
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="resId">
		/// @return </param>
		public virtual float getDimensionPixelSize(int resId)
		{
			Context context = mContext.get();
			if (null == context)
			{
				throw new System.InvalidOperationException("Context is null");
			}
			return context.Resources.getDimensionPixelSize(resId);
		}

		/// <summary>
		/// @brief Return dispatcher for remote actions
		/// </summary>
		/// <returns> FcSapaActionDispatcher implementation </returns>
		public virtual FcSapaActionDispatcher SapaActionDispatcher
		{
			get
			{
				return mSapaServiceConnector;
			}
		}

		/// <summary>
		/// @brief TODO
		/// </summary>
		/// <param name="resId"> </param>
		/// <param name="parent"> </param>
		/// @param <T>
		/// 
		/// @return </param>
		public virtual T inflate<T>(int resId, ViewGroup parent) where T : android.view.View
		{
			return inflate(resId, parent, false);
		}

		/// <summary>
		/// @brief TODO
		/// </summary>
		/// <param name="resId"> </param>
		/// <param name="parent"> </param>
		/// <param name="isAdded"> </param>
		/// @param <T>
		/// 
		/// @return </param>
		public virtual T inflate<T>(int resId, ViewGroup parent, bool isAdded) where T : android.view.View
		{
			return (T) LayoutInflater.from(mContext.get()).inflate(resId, parent, isAdded);
		}

		/// <summary>
		/// @brief TODO
		/// </summary>
		/// <param name="packageName"> </param>
		/// <param name="instanceId"> </param>
		/// <param name="mode"> </param>
		public virtual void openSapaAppActivity(string packageName, string instanceId, int mode)
		{
			if (mActiveApp == null || !mActiveApp.App.InstanceId.Equals(instanceId))
			{
				FcContextStateChanged listener = mListener.get();
				if (listener != null)
				{
					listener.onActivityFinished();
				}
				Log.d(TAG, "Open Sapa app");
				if (!startSapaActivity(instanceId, mode))
				{
					return;
				}
				if (!sendSapaSwitchBroadcast(packageName, instanceId))
				{
					return;
				}

				finishCurrentActivity(packageName);
			}
		}

		public virtual FcContextStateChanged FxContextStateChangeListener
		{
			set
			{
				mListener = new WeakReference<FcContextStateChanged>(value);
			}
		}

		/// <summary>
		/// @brief Broadcast com.samsung.android.sdk.professionalaudio.ACTION.SWITCH_TO_SAPA_APP intent
		/// 
		/// TODO: Describe what's the intent for
		/// </summary>
		/// <param name="packageName"> </param>
		/// <param name="instanceId"> </param>

		public virtual SapaAppInfo ActiveApp
		{
			set
			{
				mActiveApp = value;
			}
		}
		private bool sendSapaSwitchBroadcast(string packageName, string instanceId)
		{

			Context context = Context;
			if (null == context)
			{
				Log.w(TAG, "Cannot send sapa switch broadcast (pkg: " + packageName + ", instance: " + instanceId + "): context is null");
				return false;
			}

			Intent intent = new Intent(FcConstants.INTENT_SAPA_SWITCH);
			intent.putExtra(FcConstants.INTENT_SAPA_SWITCH_EXTRAS_INSTANCEID, instanceId);
			intent.putExtra(FcConstants.INTENT_SAPA_SWITCH_EXTRAS_PACKAGE, packageName);

			context.sendBroadcast(intent, FcConstants.PERMISSION_USE_CONNETION_SERVICE);

			return true;
		}

		/// <summary>
		/// @brief Update reference to FcSapaServiceConnector object
		/// </summary>
		/// <param name="connector">     Connector with Sapa services </param>
		public virtual FcSapaServiceConnector SapaServiceConnector
		{
			set
			{
				mSapaServiceConnector = value;
			}
		}

		/// <summary>
		/// @brief Starts the activity in the sapa environment
		/// 
		/// Setup intent by adding custom class loader and extras edit_mode
		/// TODO: Description on what's the edit_mode
		/// </summary>
		/// <param name="mode"> </param>
		private bool startSapaActivity(string instanceId, int mode)
		{
			Context context = Context;
			Log.d(TAG, "Start sapa app");
			Log.d(TAG, "instanceId:" + instanceId);
			if (null == context)
			{
				Log.w(TAG, "Cannot start activity for instance " + instanceId + ": null context");
				return false;
			}

			if (null == mSapaServiceConnector)
			{
				Log.w(TAG, "Cannot start activity for instance " + instanceId + ": service connector not set to the FC context");
				return false;
			}

			Intent intent = mSapaServiceConnector.getLaunchIntent(instanceId);
			if (null == intent)
			{
				Log.w(TAG, "Cannot start activity for instance " + instanceId + ": launch intent is null");
				return false;
			}

			intent.ExtrasClassLoader = typeof(SapaAppInfo).ClassLoader;
			intent.putExtra("Edit_mode", mode);
			intent.Flags = Intent.FLAG_ACTIVITY_BROUGHT_TO_FRONT;

			context.startActivity(intent);

			return true;
		}

		/// <summary>
		/// @brief Finish current activity if the package name is matched
		/// </summary>
		/// <param name="packageName">   Package name of the activity to stop </param>
		private bool finishCurrentActivity(string packageName)
		{
			Context context = Context;
			if (null == context)
			{
				Log.w(TAG, "Cannot finish activity " + packageName + ": context is null");
				return false;
			}

			if (!(context is Activity))
			{
				Log.w(TAG, "Cannot finish activity: " + packageName + ": context not an instance of Activity");
				return false;
			}

			if (!context.PackageName.contentEquals(packageName))
			{
				((Activity) context).finish();
			}
			return true;
		}

		/// <summary>
		/// @brief Execute runnable on the main thread
		/// </summary>
		/// <param name="runnable"> </param>
		public virtual void runOnMainThread(Runnable runnable)
		{
			mHandler.post(runnable);
		}


		/// <summary>
		/// @brief Returns <seealso cref="Drawable"/> for given resource ID
		/// </summary>
		/// <param name="resId"> Resource ID </param>
		/// <returns> <seealso cref="Drawable"/> for given resource ID </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public android.graphics.drawable.Drawable getDrawable(final int resId)
		public virtual Drawable getDrawable(int resId)
		{
			Context context = Context;
			if (null == context)
			{
				Log.w(TAG, "Cannot retrieve Drawable for resource ID=" + resId + " as the context is null");
				return null;
			}

			return context.Resources.getDrawable(resId);
		}
	}

}