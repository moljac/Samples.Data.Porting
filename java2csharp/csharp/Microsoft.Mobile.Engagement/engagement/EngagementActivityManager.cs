using System.Collections.Generic;
using System.Collections.Concurrent;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{


	using Activity = android.app.Activity;

	/// <summary>
	/// This class helps to track the current activity (must be an Engagement activity) </summary>
	public class EngagementActivityManager
	{
		private bool InstanceFieldsInitialized = false;

		public EngagementActivityManager()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mCurrentActivity = mNullActivity;
		}

	  /// <summary>
	  /// Interface to listen on current activity changes </summary>
	  public interface Listener
	  {
		/// <summary>
		/// Called when the current activity changed. </summary>
		/// <param name="currentActivity"> weak reference on current activity, referent may be null if no current
		///          activity </param>
		/// <param name="engagementAlias"> current activity name as reported in Engagement logs. </param>
		void onCurrentActivityChanged(WeakReference<Activity> currentActivity, string engagementAlias);
	  }

	  /// <summary>
	  /// Unique instance </summary>
	  private static EngagementActivityManager sInstance = new EngagementActivityManager();

	  /// <summary>
	  /// Get unique instance. </summary>
	  /// <returns> unique instance. </returns>
	  public static EngagementActivityManager Instance
	  {
		  get
		  {
			return sInstance;
		  }
	  }

	  /// <summary>
	  /// Null weak reference, this is useful for calling <seealso cref="WeakReference#get()"/> without having to
	  /// check for the null pointer on the WeakReference object...
	  /// </summary>
	  private WeakReference<Activity> mNullActivity = new WeakReference<Activity>(null);

	  /// <summary>
	  /// Current activity weak reference </summary>
	  private WeakReference<Activity> mCurrentActivity;

	  /// <summary>
	  /// Current activity alias (name) </summary>
	  private string mCurrentActivityAlias;

	  /// <summary>
	  /// Current activity listeners </summary>
	  private IDictionary<Listener, object> mListeners = new ConcurrentDictionary<Listener, object>();

	  /// <summary>
	  /// Dummy value to insert in the listener map (we only use keys) </summary>
	  private object mDummyValue = new object();

	  /// <summary>
	  /// Get current activity weak reference. May be null even if <seealso cref="#getCurrentActivityAlias()"/>
	  /// returns something not null. </summary>
	  /// <returns> current activity weak reference. </returns>
	  public virtual WeakReference<Activity> CurrentActivity
	  {
		  get
		  {
			return mCurrentActivity;
		  }
	  }

	  /// <summary>
	  /// Get current activity alias as reported by Engagement logs. </summary>
	  /// <returns> current activity alias as reported by Engagement logs, null if the current activity is
	  ///         null. </returns>
	  public virtual string CurrentActivityAlias
	  {
		  get
		  {
			return mCurrentActivityAlias;
		  }
	  }

	  /// <summary>
	  /// Set the current activity, Engagement activity classes call this in their
	  /// <seealso cref="Activity#onResume()"/> </summary>
	  /// <param name="activity"> current activity. </param>
	  /// <param name="engagementAlias"> alias as reported in Engagement logs. </param>
	  public virtual void setCurrentActivity(Activity activity, string engagementAlias)
	  {
		mCurrentActivity = new WeakReference<Activity>(activity);
		if (engagementAlias == null)
		{
		  mCurrentActivityAlias = "default";
		}
		else
		{
		  mCurrentActivityAlias = engagementAlias.Trim();
		}
		foreach (Listener listener in mListeners.Keys)
		{
		  listener.onCurrentActivityChanged(mCurrentActivity, mCurrentActivityAlias);
		}
	  }

	  /// <summary>
	  /// Remove the current activity. Engagement activity classes call this in their
	  /// <seealso cref="Activity#onPause()"/>. This will be called when switching between two activities.
	  /// </summary>
	  public virtual void removeCurrentActivity()
	  {
		mCurrentActivity = mNullActivity;
		mCurrentActivityAlias = null;
		foreach (Listener listener in mListeners.Keys)
		{
		  listener.onCurrentActivityChanged(mCurrentActivity, mCurrentActivityAlias);
		}
	  }

	  /// <summary>
	  /// Install a listener on current activity changes, this will trigger it with the current values. </summary>
	  /// <param name="listener"> the listener to install. </param>
	  public virtual void addCurrentActivityListener(Listener listener)
	  {
		mListeners[listener] = mDummyValue;
		listener.onCurrentActivityChanged(mCurrentActivity, mCurrentActivityAlias);
	  }

	  /// <summary>
	  /// Uninstall a listener on current activity changes. </summary>
	  /// <param name="listener"> the listener to uninstall. </param>
	  public virtual void removeCurrentActivityListener(Listener listener)
	  {
		mListeners.Remove(listener);
	  }
	}

}