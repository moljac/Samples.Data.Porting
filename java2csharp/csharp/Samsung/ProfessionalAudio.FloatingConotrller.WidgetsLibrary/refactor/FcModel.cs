using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using LayoutDirection = android.util.LayoutDirection;
	using Log = android.util.Log;

	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;


	/// <summary>
	/// @brief Floating controller model class
	/// </summary>
	public class FcModel : FcSapaModel, FcAdapterModel
	{

		private const int DEFAULT_APP_CAPACITY = 25;
		private static readonly string TAG = typeof(FcModel).Name;

		/// <summary>
		/// @brief represents the running application </summary>
		private string mActiveApplication;

		/// <summary>
		/// @brief data structure to hold data representing active applications </summary>
		private IDictionary<string, FcModelItem> mApplicationMap;

		/// <summary>
		/// @brief represents main (host) application </summary>
		private string mMainApplication;

		/// <summary>
		/// @brief Object to be notified when model is changed </summary>
		private FcAdapterModel_FcModelChangedListener mModelChangedListener;

		/// <summary>
		/// @brief used to preserve track order </summary>
		private IList<string> mOrderedApplications;

		/// <summary>
		/// @brief used to determine which apps are freeze </summary>
		private IList<string> mFreezeApplications;

		/// <summary>
		/// @brief Context of android application </summary>
		private readonly FcContext mFcContext;

		/// <summary>
		/// @brief Current item direction </summary>
		private int mCurrentDirection = LayoutDirection.LTR;

		/// <summary>
		/// @brief Default constructor
		/// </summary>
		public FcModel(FcContext fcContext)
		{
			mFcContext = fcContext;
			mApplicationMap = new Dictionary<string, FcModelItem>(DEFAULT_APP_CAPACITY);
			mOrderedApplications = new List<string>(DEFAULT_APP_CAPACITY);
		}

		//
		// FcAdapterModel interface
		//

		public virtual int getInstanceNumber(FcModelItem item)
		{
			// Ordered list could be used to access instanceId faster, but this method is
			// more readable and refactor-friendly.
			string instanceId = item.InstanceId;
			return char.digit(instanceId[instanceId.Length - 1], 10);
		}

		public virtual FcModelItem getItem(int position)
		{
			lock (this)
			{
				return mApplicationMap[getInstanceId(position)];
			}
		}

		public virtual int ItemCount
		{
			get
			{
				lock (this)
				{
					return mApplicationMap.Count;
				}
			}
		}

		public virtual int getItemType(int position)
		{
			lock (this)
			{
				return (null != mMainApplication && position == 0) ? FcConstants.APP_TYPE_MAIN : FcConstants.APP_TYPE_ORDINAL;
			}
		}

		public virtual bool isMultiInstance(FcModelItem srcItem)
		{
			lock (this)
			{
				int count = 0;
				// No reason to cache counters, there's only (at max) 25 items
				foreach (FcModelItem item in mApplicationMap.Values)
				{
					if (item.samePackageName(srcItem))
					{
						++count;
					}
				}
        
				return count > 1;
			}
		}

		public virtual FcAdapterModel_FcModelChangedListener FcModelChangedListener
		{
			set
			{
				lock (this)
				{
					mModelChangedListener = value;
				}
			}
		}

		public virtual void changeItemDirection(int layoutDirection)
		{
			lock (this)
			{
				mCurrentDirection = layoutDirection;
				foreach (FcModelItem item in mApplicationMap.Values)
				{
					item.Direction = layoutDirection;
				}
				notifyModelInvalidated();
			}
		}

		public virtual FcModelItem ExpandedItem
		{
			get
			{
				lock (this)
				{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final int itemCount = getItemCount();
					int itemCount = ItemCount;
            
					for (int i = 0; i < itemCount; ++i)
					{
						FcModelItem item = getItem(i);
						if (item.Expanded)
						{
							return item;
						}
					}
            
					return null;
				}
			}
		}

		public virtual int getItemPosition(FcModelItem item)
		{
			lock (this)
			{
				int count = ItemCount;
				for (int i = 0; i < count; ++i)
				{
					FcModelItem otherItem = getItem(i);
					if (item.InstanceId.Equals(otherItem.InstanceId))
					{
						return i;
					}
				}
				return -1;
			}
		}

		//
		// FcSapaModel interface
		//

		public virtual void clear()
		{
			lock (this)
			{
				mMainApplication = null;
				mActiveApplication = null;
				mOrderedApplications.Clear();
				mApplicationMap.Clear();
        
				notifyModelInvalidated();
			}
		}

		public virtual void insertApp(SapaAppInfo appInfo, int mode)
		{
			lock (this)
			{
				FcModelItem item = null;
				string instanceId = null;
				int position = -1;
				if (mode == FcConstants.APP_TYPE_ORDINAL)
				{
					item = FcModelItem.createOrdinal(mFcContext, appInfo);
					instanceId = item.InstanceId;
					mOrderedApplications.Add(instanceId);
					position = mOrderedApplications.Count - 1;
        
				}
				else if (mode == FcConstants.APP_TYPE_MAIN)
				{
					item = FcModelItem.createMain(mFcContext, appInfo);
					instanceId = item.InstanceId;
					mMainApplication = instanceId;
					position = 0;
        
				}
				else
				{
					throw new InvalidParameterException("Invalid mode: " + mode);
				}
        
				if (null == instanceId)
				{
					throw new System.InvalidOperationException("Model item not created: null reference!");
				}
				if (isActiveApp(instanceId))
				{
					item.Active = true;
				}
				item.Direction = mCurrentDirection;
        
				mApplicationMap[instanceId] = item;
        
				Log.d(TAG, "Inserting application to the model: " + instanceId);
				if (null != mModelChangedListener && position >= 0)
				{
					mModelChangedListener.onFcModelItemInserted(position);
				}
			}
		}

		public virtual void orderApps(IList<string> newOrder)
		{
			lock (this)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.v(TAG, "New apps order (size: " + newOrder.Count + ")");
					foreach (string instanceId in newOrder)
					{
						Log.d(TAG, "    " + instanceId);
					}
        
					Log.v(TAG, "Old apps order (size: " + mOrderedApplications.Count + ")");
					foreach (string instanceId in mOrderedApplications)
					{
						Log.d(TAG, "    " + instanceId);
					}
				}
        
				if (newOrder.Count != mOrderedApplications.Count)
				{
					Log.w(TAG, "Logic error: sizes of current and new app lists don't match");
					return;
				}
        
				IList<string> currentOrder = new List<string>(mOrderedApplications);
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the java.util.Collection 'retainAll' method:
				currentOrder.retainAll(newOrder);
        
				if (newOrder.Count != currentOrder.Count)
				{
					Log.w(TAG, "Logic error: current and new app lists don't contain the same elements");
					return;
				}
        
        
				mOrderedApplications = newOrder;
				notifyModelInvalidated();
			}
		}

		public virtual IList<string> FreezeApps
		{
			set
			{
				lock (this)
				{
					mFreezeApplications = value;
					notifyModelInvalidated();
				}
			}
		}

		public virtual void removeApp(string instanceId, int mode)
		{
			lock (this)
			{
				int position;
				if (mode == FcConstants.APP_TYPE_MAIN)
				{
					if (!isMainApp(instanceId))
					{
						throw new System.ArgumentException("Invalid instanceId of the main application");
					}
					mMainApplication = null;
					position = 0;
        
				}
				else if (mode == FcConstants.APP_TYPE_ORDINAL)
				{
					position = mOrderedApplications.IndexOf(instanceId);
					mOrderedApplications.Remove(instanceId);
        
					if (null != mMainApplication)
					{
						position += 1;
					}
        
				}
				else
				{
					throw new System.ArgumentException("Unsupported mode: " + mode);
				}
        
				mApplicationMap.Remove(instanceId);
				if (isActiveApp(instanceId))
				{
					mActiveApplication = null;
				}
        
				Log.d(TAG, "Removing application from the model: " + instanceId);
				if (null != mModelChangedListener && position >= 0)
				{
					mModelChangedListener.onFcModelItemRemoved(position);
				}
			}
		}

		public virtual void updateApp(SapaAppInfo appInfo, int mode)
		{
			lock (this)
			{
				string instanceId = appInfo.App.InstanceId;
				Log.d(TAG, "updateApp(" + instanceId + ", mode: " + mode + ")");
        
				FcModelItem item = getItemForInstanceId(appInfo.App.InstanceId);
				if (null == item)
				{
					Log.w(TAG, "Cannot update app " + instanceId + ": unknown app");
					return;
				}
        
				int position;
				if (mode == FcConstants.APP_TYPE_MAIN)
				{
					position = 0;
        
				}
				else if (mode == FcConstants.APP_TYPE_ORDINAL)
				{
					position = mOrderedApplications.IndexOf(instanceId);
					if (position < 0)
					{
						Log.w(TAG, "Cannot update app " + instanceId + ": not found on ordered list");
						return;
					}
					if (null != mMainApplication)
					{
						position += 1;
					}
        
				}
				else
				{
					throw new InvalidParameterException("Unsupported mode " + mode);
				}
        
				// This needs to be updated after all the logic guards
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "Updating application in a model: " + instanceId);
				}
				item.update(mFcContext, appInfo);
        
				if (null != mModelChangedListener)
				{
					mModelChangedListener.onFcModelChanged(position);
				}
			}
		}

		private FcModelItem getItemForInstanceId(string instanceId)
		{
			lock (this)
			{
				return mApplicationMap[instanceId];
			}
		}

		public virtual SapaAppInfo ActiveApp
		{
			set
			{
				lock (this)
				{
					Log.d(TAG, "Set active app");
					if (null == value || null == value.App)
					{
						Log.w(TAG, "Calling setActiveApp with null appInfo");
						return;
					}
            
					string instanceId = value.App.InstanceId;
					foreach (FcModelItem item in mApplicationMap.Values)
					{
						item.Active = false;
					}
            
					FcModelItem item = mApplicationMap[instanceId];
					if (null == item)
					{
						Log.w(TAG, "Not setting active app: no application for instance " + instanceId);
						return;
					}
            
					item.Active = true;
					mActiveApplication = instanceId;
            
					// Updating whole model as the active app has been changed
					notifyModelInvalidated();
				}
			}
		}

		public virtual bool isAppFreeze(string instanceId)
		{
			lock (this)
			{
				return FreezeApplications.Contains(instanceId);
			}
		}

		//
		// Privates
		//

		private string getInstanceId(int position)
		{
			if (null == mMainApplication)
			{
				return mOrderedApplications[position];
			}

			return (position == 0) ? mMainApplication : mOrderedApplications[position - 1];
		}

		/// <summary>
		/// @brief Call to check if the current active application (the one being displayed)
		/// matches given <code>instanceId</code>.
		/// </summary>
		/// <param name="instanceId">
		/// @return </param>
		private bool isActiveApp(string instanceId)
		{
			return mActiveApplication != null && mActiveApplication.Equals(instanceId);
		}

		/// <summary>
		/// @brief Call to check if the main application matches given <code>instanceId</code>
		/// </summary>
		/// <param name="instanceId">
		/// @return </param>
		private bool isMainApp(string instanceId)
		{
			return mMainApplication != null && mMainApplication.Equals(instanceId);
		}

		private void notifyModelInvalidated()
		{
			if (null != mModelChangedListener)
			{
				mModelChangedListener.onFcModelChanged();
			}
		}

		public virtual IList<string> FreezeApplications
		{
			get
			{
				lock (this)
				{
					if (mFreezeApplications == null)
					{
						mFreezeApplications = new List<string>();
					}
					return mFreezeApplications;
				}
			}
		}
	}

}