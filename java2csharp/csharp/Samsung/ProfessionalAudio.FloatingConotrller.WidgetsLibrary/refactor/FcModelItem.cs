using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{


	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using SparseArray = android.util.SparseArray;
	using ViewGroup = android.view.ViewGroup;
	using LinearLayout = android.widget.LinearLayout;

	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;


	/// <summary>
	/// @brief Model item representing application within the SAPA environment
	/// 
	/// There are two kind of applications:
	/// - Main (host) such as soundcamp
	/// - Ordinal, which is instrument or effect
	/// </summary>
	public class FcModelItem
	{

		public const int VOLUME_UP_INDEX = 0;
		public const int VOLUME_DOWN_INDEX = 1;
		public const int VOLUME_SIZE = 2;

		private static readonly string TAG = typeof(FcModelItem).Name;
		private const int DEFAULT_CAPACITY = 2;

		/// <summary>
		/// @brief Construct model item from SapaAppInfo structure
		/// </summary>
		/// <param name="info">  Data structure with information retrieved from SAPA
		/// </param>
		/// <returns> Model item representing ordinal application </returns>
		public static FcModelItem createOrdinal(FcContext fcContext, SapaAppInfo info)
		{
			return new FcModelItem(fcContext, info, FcConstants.APP_TYPE_ORDINAL);
		}

		/// <summary>
		/// @brief Construct model item from SapaAppInfo structure
		/// </summary>
		/// <param name="info">  Data structure with information retrieved from SAPA
		/// </param>
		/// <returns> Model item representing main application </returns>
		public static FcModelItem createMain(FcContext fcContext, SapaAppInfo info)
		{
			return new FcModelItem(fcContext, info, FcConstants.APP_TYPE_MAIN);
		}

		private bool mScrollFocus;
		private bool mActive = false;
		private IList<FcActionItem> mCallActions = new List<FcActionItem>(DEFAULT_CAPACITY);
		private Drawable mIcon;
		private readonly string mInstanceId;
		private readonly int mType;
		private readonly string mPackageName;
		private IList<FcActionItem> mReturnActions = new List<FcActionItem>(DEFAULT_CAPACITY);
		private FcActionItem[] mVolumeActions = new FcActionItem[VOLUME_SIZE];
		private bool mExpanded = false;
		private int mDirection;

		/// <summary>
		/// @brief Construct model item from SapaAppInfo structure
		/// </summary>
		/// <param name="info">  Data structure with information retrieved from SAPA </param>
		/// <param name="type">  Type of the item (main or ordinal) </param>
		internal FcModelItem(FcContext fcContext, SapaAppInfo info, int type)
		{
			SapaApp app = info.App;

			try
			{
				mIcon = info.getIcon(fcContext.Context);
			}
			catch (NameNotFoundException e)
			{
				LogUtils.throwable(TAG, "Drawable not set: name not found", e);
			}
			mInstanceId = app.InstanceId;
			mPackageName = app.PackageName;
			mType = type;

			prepareActions(fcContext, info);
		}

		/// <summary>
		/// @brief Get read-only list of call actions
		/// </summary>
		/// <returns> Unmodifiable list of call actions </returns>
		public virtual IList<FcActionItem> CallActions
		{
			get
			{
				return Collections.unmodifiableList(mCallActions);
			}
		}

		/// <summary>
		/// @brief Get drawable representing icon of the application
		/// </summary>
		/// <returns> Application icon (might be null) </returns>
		public virtual Drawable Icon
		{
			get
			{
				lock (this)
				{
					return mIcon;
				}
			}
		}

		/// <summary>
		/// @brief Get instance unique id
		/// 
		/// The current implementation use a pattern of common prefix and a number after separator,
		/// for example: bassguitar^2 (second instance of the bass guitar)
		/// </summary>
		/// <returns> unique instance id </returns>
		public virtual string InstanceId
		{
			get
			{
				return mInstanceId;
			}
		}

		/// <summary>
		/// @brief Get read-only list of return actions
		/// </summary>
		/// <returns> Unmodifiable list of return actions </returns>
		public virtual IList<FcActionItem> ReturnActions
		{
			get
			{
				return Collections.unmodifiableList(mReturnActions);
			}
		}

		/// <summary>
		/// @brief Get type of the item (main or ordinal)
		/// </summary>
		/// <returns> type of the item
		/// </returns>
		/// <seealso cref= FcConstants#APP_TYPE_MAIN </seealso>
		/// <seealso cref= FcConstants#APP_TYPE_ORDINAL </seealso>
		public virtual int Type
		{
			get
			{
				return mType;
			}
		}

		/// <summary>
		/// @brief Get read-only list of volume actions
		/// </summary>
		/// <returns> Unmodifiable list of volume actions </returns>
		public virtual IList<FcActionItem> VolumeActions
		{
			get
			{
				if (!hasVolumeActions())
				{
					return Collections.emptyList();
				}
				return Collections.unmodifiableList(Arrays.asList(mVolumeActions));
			}
		}

		/// <summary>
		/// @brief Checks if this item got volume actions
		/// </summary>
		/// <returns> true if volume actions are present, false if not </returns>
		public virtual bool hasVolumeActions()
		{
			lock (this)
			{
				return mVolumeActions[VOLUME_UP_INDEX] != null && mVolumeActions[VOLUME_DOWN_INDEX] != null;
			}
		}

		/// <summary>
		/// @brief Check whether the app under this model item is an active app or not
		/// </summary>
		/// <seealso cref= #setActive(boolean)
		/// </seealso>
		/// <returns> true if the item is marked as active, false otherwise </returns>
		public virtual bool Active
		{
			get
			{
				lock (this)
				{
					return mActive;
				}
			}
			set
			{
				lock (this)
				{
					mActive = value;
				}
			}
		}

		/// <summary>
		/// @brief Get the horizontal direction of given model item layouts based on Control Bar's direction
		/// </summary>
		/// <returns> horizontal direction of the item </returns>
		public virtual int Direction
		{
			get
			{
				lock (this)
				{
					return mDirection;
				}
			}
			set
			{
				lock (this)
				{
					if (mDirection != value)
					{
						reverseActions();
					}
					mDirection = value;
				}
			}
		}

		/// <summary>
		/// @brief Check whether given item got the same package as the other item
		/// 
		/// Might be used to check whether two references to <seealso cref="FcModelItem"/> classes represents
		/// the same application (but instance of the application might be different)
		/// </summary>
		/// <param name="other"> Other model item to check
		/// 
		/// @return </param>
		public virtual bool samePackageName(FcModelItem other)
		{
			lock (this)
			{
				return mPackageName.Equals(other.mPackageName);
			}
		}


		/// <summary>
		/// @brief Check whether given model item is expanded in the Floating Controller
		/// 
		/// @return
		/// </summary>
		public virtual bool Expanded
		{
			get
			{
				lock (this)
				{
					return mExpanded;
				}
			}
			set
			{
				lock (this)
				{
					if (FcConstants.OPT_DETAILED_LOGS)
					{
						Log.d(TAG, "setExpanded(" + mInstanceId + ") = " + value);
					}
					mExpanded = value;
				}
			}
		}

		/// 
		/// <summary>
		/// @return
		/// </summary>
		public virtual bool ScrollFocus
		{
			get
			{
				lock (this)
				{
					return mScrollFocus;
				}
			}
			set
			{
				lock (this)
				{
					mScrollFocus = value;
				}
			}
		}


		/// <param name="appInfo"> </param>
		public virtual void update(FcContext context, SapaAppInfo appInfo)
		{
			lock (this)
			{
				Log.d(TAG, "Updating model for " + mInstanceId);
				try
				{
					mIcon = appInfo.getIcon(context.Context);
				}
				catch (NameNotFoundException e)
				{
					LogUtils.throwable(TAG, "Drawable not set: name not found", e);
				}
        
				clearActions();
				prepareActions(context, appInfo);
			}
		}

		private void clearActions()
		{
			mVolumeActions[VOLUME_UP_INDEX] = null;
			mVolumeActions[VOLUME_DOWN_INDEX] = null;
			mCallActions.Clear();
			mReturnActions.Clear();
		}

		private void prepareActions(FcContext fcContext, SapaAppInfo info)
		{
			prepareReturnActions(fcContext, info);
			prepareCallActions(fcContext, info);
			if (mDirection == LinearLayout.LAYOUT_DIRECTION_RTL)
			{
				reverseActions();
			}
		}

		private void prepareCallActions(FcContext fcContext, SapaAppInfo appInfo)
		{
			FcActionFactory factory = fcContext.ActionFactory;
			SparseArray<SapaActionInfo> actions = appInfo.Actions;
			for (int i = 0; i < actions.size(); ++i)
			{
				SapaActionInfo actionInfo = actions.get(i);
				FcActionItem action = factory.newAppItem(appInfo, actionInfo);
				string actionName = actionInfo.Id;

				// Volume buttons needs to be separated from the rest of actions
				if (FcConstants.ACTION_VOLUME_UP.Equals(actionName))
				{
					mVolumeActions[VOLUME_UP_INDEX] = action;
				}
				else if (FcConstants.ACTION_VOLUME_DOWN.Equals(actionName))
				{
					mVolumeActions[VOLUME_DOWN_INDEX] = action;
				}
				else
				{
					mCallActions.Add(action);
				}
			}
		}

		private void reverseActions()
		{
			mCallActions.Reverse();
			IList<FcActionItem> volumeActionsList = Arrays.asList(mVolumeActions);
			volumeActionsList.Reverse();
			mVolumeActions = (FcActionItem[]) volumeActionsList.ToArray();
			mReturnActions.Reverse();
		}

		private void prepareReturnActions(FcContext fcContext, SapaAppInfo info)
		{
			Bundle config = info.Configuration;
			if (null != config)
			{
				int[] retButtonsIds = config.getIntArray(FcConstants.KEY_RETURN_BUTTONS);
				int[] retButtonsOpts = config.getIntArray(FcConstants.KEY_RETURN_BUTTONS_OPTS);

				if (retButtonsIds == null || retButtonsOpts == null)
				{
					prepareDefaultReturnActions(fcContext, info);

				}
				else if (retButtonsIds.Length == retButtonsOpts.Length)
				{
					prepareCustomReturnActions(fcContext, info, retButtonsIds, retButtonsOpts);

				}
				else
				{
					Log.w(TAG, "Sizes of arrays: " + FcConstants.KEY_RETURN_BUTTONS + " and " + FcConstants.KEY_RETURN_BUTTONS_OPTS + " are not equal");
					prepareDefaultReturnActions(fcContext, info);
				}
			}
			else
			{
				prepareDefaultReturnActions(fcContext, info);
			}
		}

		private void prepareCustomReturnActions(FcContext fcContext, SapaAppInfo info, int[] drawableIds, int[] activityIds)
		{
			FcActionFactory factory = fcContext.ActionFactory;
			for (int i = 0; i < drawableIds.Length; ++i)
			{
				int drawableId = drawableIds[i];
				int activityId = activityIds[i];

				mReturnActions.Add(factory.newCustomReturnItem(info, drawableId, activityId));
			}
		}

		private void prepareDefaultReturnActions(FcContext fcContext, SapaAppInfo info)
		{
			FcActionFactory factory = fcContext.ActionFactory;
			mReturnActions.Add(factory.newDefaultReturnItem(info));
		}

		public override string ToString()
		{
			return "FcModelItem<" + mInstanceId + ((mActive ? " ACTIVE" : "")) + ((mExpanded ? " " : " NOT_")) + "EXPANDED" + ">";
		}

	}

}