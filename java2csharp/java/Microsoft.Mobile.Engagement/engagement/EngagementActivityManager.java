/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import java.lang.ref.WeakReference;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

import android.app.Activity;

/** This class helps to track the current activity (must be an Engagement activity) */
public class EngagementActivityManager
{
  /** Interface to listen on current activity changes */
  public interface Listener
  {
    /**
     * Called when the current activity changed.
     * @param currentActivity weak reference on current activity, referent may be null if no current
     *          activity
     * @param engagementAlias current activity name as reported in Engagement logs.
     */
    void onCurrentActivityChanged(WeakReference<Activity> currentActivity, String engagementAlias);
  }

  /** Unique instance */
  private static EngagementActivityManager sInstance = new EngagementActivityManager();

  /**
   * Get unique instance.
   * @return unique instance.
   */
  public static EngagementActivityManager getInstance()
  {
    return sInstance;
  }

  /**
   * Null weak reference, this is useful for calling {@link WeakReference#get()} without having to
   * check for the null pointer on the WeakReference object...
   */
  private WeakReference<Activity> mNullActivity = new WeakReference<Activity>(null);

  /** Current activity weak reference */
  private WeakReference<Activity> mCurrentActivity = mNullActivity;

  /** Current activity alias (name) */
  private String mCurrentActivityAlias;

  /** Current activity listeners */
  private Map<Listener, Object> mListeners = new ConcurrentHashMap<Listener, Object>();

  /** Dummy value to insert in the listener map (we only use keys) */
  private Object mDummyValue = new Object();

  /**
   * Get current activity weak reference. May be null even if {@link #getCurrentActivityAlias()}
   * returns something not null.
   * @return current activity weak reference.
   */
  public WeakReference<Activity> getCurrentActivity()
  {
    return mCurrentActivity;
  }

  /**
   * Get current activity alias as reported by Engagement logs.
   * @return current activity alias as reported by Engagement logs, null if the current activity is
   *         null.
   */
  public String getCurrentActivityAlias()
  {
    return mCurrentActivityAlias;
  }

  /**
   * Set the current activity, Engagement activity classes call this in their
   * {@link Activity#onResume()}
   * @param activity current activity.
   * @param engagementAlias alias as reported in Engagement logs.
   */
  public void setCurrentActivity(Activity activity, String engagementAlias)
  {
    mCurrentActivity = new WeakReference<Activity>(activity);
    if (engagementAlias == null)
      mCurrentActivityAlias = "default";
    else
      mCurrentActivityAlias = engagementAlias.trim();
    for (Listener listener : mListeners.keySet())
      listener.onCurrentActivityChanged(mCurrentActivity, mCurrentActivityAlias);
  }

  /**
   * Remove the current activity. Engagement activity classes call this in their
   * {@link Activity#onPause()}. This will be called when switching between two activities.
   */
  public void removeCurrentActivity()
  {
    mCurrentActivity = mNullActivity;
    mCurrentActivityAlias = null;
    for (Listener listener : mListeners.keySet())
      listener.onCurrentActivityChanged(mCurrentActivity, mCurrentActivityAlias);
  }

  /**
   * Install a listener on current activity changes, this will trigger it with the current values.
   * @param listener the listener to install.
   */
  public void addCurrentActivityListener(Listener listener)
  {
    mListeners.put(listener, mDummyValue);
    listener.onCurrentActivityChanged(mCurrentActivity, mCurrentActivityAlias);
  }

  /**
   * Uninstall a listener on current activity changes.
   * @param listener the listener to uninstall.
   */
  public void removeCurrentActivityListener(Listener listener)
  {
    mListeners.remove(listener);
  }
}
