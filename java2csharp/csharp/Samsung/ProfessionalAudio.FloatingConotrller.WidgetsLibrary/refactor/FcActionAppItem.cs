namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Drawable = android.graphics.drawable.Drawable;
	using Log = android.util.Log;

	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;

	/// <summary>
	/// @brief Class to represent remote action of the application
	/// </summary>
	/// <seealso cref= FcActionItem </seealso>
	public class FcActionAppItem : FcActionItem
	{

		private static readonly string TAG = typeof(FcActionAppItem).Name;
		private readonly SapaActionInfo mActionInfo;
		private Runnable mRunnable;

		/// <summary>
		/// @brief Construct action item from the SapaActionInfo structure
		/// </summary>
		/// <param name="actionInfo">    Sapa structure describing action details </param>
		public FcActionAppItem(SapaActionInfo actionInfo)
		{
			mActionInfo = actionInfo;
		}

		public virtual Drawable getIcon(FcContext fcContext)
		{
			Drawable drawable = null;
			try
			{
				drawable = mActionInfo.getIcon(fcContext.Context);

			}
			catch (NameNotFoundException)
			{
				Log.w(TAG, "Cannot retrieve action icon: name not found");
			}

			return drawable;
		}

		public virtual string Id
		{
			get
			{
				return mActionInfo.Id;
			}
		}

		public virtual Runnable ActionRunnable
		{
			get
			{
				return mRunnable;
			}
			set
			{
				mRunnable = value;
			}
		}

		public virtual string getName(FcContext fcContext)
		{
			Log.d(TAG, "getName");
			string name = null;
			try
			{
				name = mActionInfo.getName(fcContext.Context);

			}
			catch (NameNotFoundException)
			{
				Log.w(TAG, "Cannot retrieve action name: name not found");
			}
			Log.d(TAG, "name:" + name);
			return name;
		}

		public virtual bool Enabled
		{
			get
			{
				return mActionInfo.Enabled;
			}
		}

		public virtual bool Visible
		{
			get
			{
				return mActionInfo.Visible;
			}
		}

	}

}