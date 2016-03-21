namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Drawable = android.graphics.drawable.Drawable;

	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

	/// <summary>
	/// @brief Class to represent a return action
	/// </summary>
	/// <seealso cref= FcActionItem </seealso>
	public class FcActionReturnItem : FcActionItem
	{

		private readonly SapaAppInfo mAppInfo;
		private int mDrawableId = -1;
		private bool mIsDefault = false;
		private Runnable mRunnable;

		/// <summary>
		/// @brief Construct FcActionReturnItem from a structure describing application
		/// </summary>
		/// <param name="appInfo">       Sapa structure describing application </param>
		public FcActionReturnItem(SapaAppInfo appInfo)
		{
			mAppInfo = appInfo;
		}

		public virtual Drawable getIcon(FcContext fcContext)
		{
			return Default ? fcContext.getDrawable(mDrawableId) : fcContext.getApplicationDrawable(mAppInfo.PackageName, mDrawableId);
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
			return fcContext.getApplicationName(mAppInfo.PackageName);
		}

		public virtual string Id
		{
			get
			{
				return "";
			}
		}

		public virtual bool Enabled
		{
			get
			{
				// Always enabled
				return true;
			}
		}

		public virtual bool Visible
		{
			get
			{
				// Always visible
				return true;
			}
		}


		/// <summary>
		/// @brief Set whether the return action should be default or custom
		/// </summary>
		/// <param name="isDefault"> true for default return action, false for custom
		/// </param>
		/// <seealso cref= FcActionItem </seealso>
		internal virtual bool Default
		{
			set
			{
				mIsDefault = value;
			}
			get
			{
				return mIsDefault;
			}
		}

		/// <summary>
		/// @brief Set a Drawable Id to be used for custom return actions
		/// </summary>
		/// <param name="drawableId"> </param>
		internal virtual int DrawableId
		{
			set
			{
				mDrawableId = value;
			}
		}

	}

}