using System;
using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.app.Activity.RESULT_CANCELED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.app.Activity.RESULT_OK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.content.Context.NOTIFICATION_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.content.Intent.CATEGORY_DEFAULT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_PAYLOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.ACTION_URL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.CAMPAIGN_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.CATEGORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.CONTENT_DISPLAYED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DELIVERY_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DLC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DLC_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DOWNLOAD_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_ACTIONED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_BIG_PICTURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_BIG_TEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_CLOSEABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_FIRST_DISPLAYED_DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_ICON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_LAST_DISPLAYED_DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_SOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_TITLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_VIBRATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.OID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.PAYLOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.USER_TIME_ZONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachContent.FLAG_DLC_CONTENT;


	using JSONObject = org.json.JSONObject;

	using Activity = android.app.Activity;
	using AlarmManager = android.app.AlarmManager;
	using Application = android.app.Application;
	using NotificationManager = android.app.NotificationManager;
	using PendingIntent = android.app.PendingIntent;
	using ActivityNotFoundException = android.content.ActivityNotFoundException;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using ComponentName = android.content.ComponentName;
	using ContentValues = android.content.ContentValues;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ActivityInfo = android.content.pm.ActivityInfo;
	using ResolveInfo = android.content.pm.ResolveInfo;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using LruCache = android.support.v4.util.LruCache;
	using View = android.view.View;

	using Callback = com.microsoft.azure.engagement.EngagementAgent.Callback;
	using EngagementNotificationUtilsV11 = com.microsoft.azure.engagement.reach.v11.EngagementNotificationUtilsV11;
	using EngagementStorage = com.microsoft.azure.engagement.storage.EngagementStorage;
	using Scanner = com.microsoft.azure.engagement.storage.EngagementStorage.Scanner;

	/// <summary>
	/// This is the class that manages the Reach functionalities. It listen messages thanks to
	/// <seealso cref="EngagementReachReceiver"/> and notify the user about contents. You usually don't need to
	/// access this class directly, you rather integrate the <seealso cref="EngagementReachReceiver"/> broadcast
	/// receiver in your AndroidManifest.xml file.<br/> </summary>
	/// <seealso cref= EngagementReachReceiver </seealso>
	public class EngagementReachAgent
	{
	  /// <summary>
	  /// Intent prefix </summary>
	  private const string INTENT_PREFIX = "com.microsoft.azure.engagement.reach.intent.";

	  /// <summary>
	  /// Intent action prefix </summary>
	  private static readonly string INTENT_ACTION_PREFIX = INTENT_PREFIX + "action.";

	  /// <summary>
	  /// Intent extra prefix </summary>
	  private static readonly string INTENT_EXTRA_PREFIX = INTENT_PREFIX + "extra.";

	  /// <summary>
	  /// Intent action used when a reach notification has been actioned e.g. clicked </summary>
	  public static readonly string INTENT_ACTION_ACTION_NOTIFICATION = INTENT_ACTION_PREFIX + "ACTION_NOTIFICATION";

	  /// <summary>
	  /// Intent action used when a reach notification has been exited (clear button on notification
	  /// panel).
	  /// </summary>
	  public static readonly string INTENT_ACTION_EXIT_NOTIFICATION = INTENT_ACTION_PREFIX + "EXIT_NOTIFICATION";

	  /// <summary>
	  /// Intent action used to react to a download timeout </summary>
	  public static readonly string INTENT_ACTION_DOWNLOAD_TIMEOUT = INTENT_ACTION_PREFIX + "DOWNLOAD_TIMEOUT";

	  /// <summary>
	  /// Intent action used to launch loading screen. </summary>
	  public static readonly string INTENT_ACTION_LOADING = INTENT_ACTION_PREFIX + "LOADING";

	  /// <summary>
	  /// Used a long extra field in notification and view intents, containing the content identifier </summary>
	  public static readonly string INTENT_EXTRA_CONTENT_ID = INTENT_EXTRA_PREFIX + "CONTENT_ID";

	  /// <summary>
	  /// Used an int extra field in notification intents, containing the system notification identifier
	  /// (to be able to explicitly remove the notification).
	  /// </summary>
	  public static readonly string INTENT_EXTRA_NOTIFICATION_ID = INTENT_EXTRA_PREFIX + "NOTIFICATION_ID";

	  /// <summary>
	  /// Used as an extra field in activity launch intent (see
	  /// <seealso cref="#INTENT_ACTION_ACTION_NOTIFICATION"/> action) to represent the component that will display
	  /// the content (an activity).
	  /// </summary>
	  public static readonly string INTENT_EXTRA_COMPONENT = INTENT_EXTRA_PREFIX + "COMPONENT";

	  /// <summary>
	  /// Undefined intent result (used for datapush) </summary>
	  private const int RESULT_UNDEFINED = -2;

	  /// <summary>
	  /// Reach XML namespace </summary>
	  internal const string REACH_NAMESPACE = "urn:ubikod:ermin:reach:0";

	  /// <summary>
	  /// Download meta-data store </summary>
	  private const string DOWNLOAD_SETTINGS = "engagement.reach.downloads";

	  /// <summary>
	  /// Unique instance </summary>
	  private static EngagementReachAgent sInstance;

	  /// <summary>
	  /// Activity manager </summary>
	  private static readonly EngagementActivityManager sActivityManager = EngagementActivityManager.Instance;

	  /// <summary>
	  /// Context used for binding to the Engagement service and other Android API calls </summary>
	  private readonly Context mContext;

	  /// <summary>
	  /// Last time application was updated </summary>
	  private readonly long mAppLastUpdateTime;

	  /// <summary>
	  /// Notification handlers by category, a default one is set at init time </summary>
	  private readonly IDictionary<string, EngagementNotifier> mNotifiers = new Dictionary<string, EngagementNotifier>();

	  /// <summary>
	  /// Storage for contents </summary>
	  private readonly EngagementStorage mDB;

	  /// <summary>
	  /// List of parameters to inject in announcement's action URL and body </summary>
	  private readonly IDictionary<string, string> mInjectedParams = new Dictionary<string, string>();

	  /// <summary>
	  /// States </summary>
	  private enum State
	  {
		/// <summary>
		/// When we are waiting for new content </summary>
		IDLE,

		/// <summary>
		/// A content is being notified in-app </summary>
		NOTIFYING_IN_APP,

		/// <summary>
		/// A content is being loaded </summary>
		LOADING,

		/// <summary>
		/// A content is being shown </summary>
		SHOWING
	  }

	  /// <summary>
	  /// Current state </summary>
	  private State mState = State.IDLE;

	  /// <summary>
	  /// True if in the process of scanning </summary>
	  private bool mScanning;

	  /// <summary>
	  /// The current content (identifier) being shown (in a viewing activity), set when mState ==
	  /// State.SHOWING.
	  /// </summary>
	  private long? mCurrentShownContentId;

	  /// <summary>
	  /// The component elected to show the content </summary>
	  private ComponentName mShowingActivity;

	  /// <summary>
	  /// Notifications (content identifiers) that are pending (for example because of a background
	  /// download). Used to avoid processing them again at each activity change.
	  /// </summary>
	  private readonly ISet<long?> mPendingNotifications = new HashSet<long?>();

	  /// <summary>
	  /// Content LRU RAM cache, generally contains <seealso cref="#mCurrentShownContent"/> and the ones in
	  /// <seealso cref="#mPendingNotifications"/>.
	  /// </summary>
	  private readonly LruCache<long?, EngagementReachContent> mContentCache = new LruCache<long?, EngagementReachContent>(10);

	  /// <summary>
	  /// DLC we already requested to service during this process lifetime (key = localId) </summary>
	  private readonly ISet<long?> mPendingDLCs = new HashSet<long?>();

	  /// <summary>
	  /// Last activity weak reference that the agent is aware of </summary>
	  private WeakReference<Activity> mLastActivity = new WeakReference<Activity>(null);

	  /// <summary>
	  /// Activity listener, when current activity changes we try to show a content notification from
	  /// local database.
	  /// </summary>
	  private readonly EngagementActivityManager.Listener mActivityListener = new ListenerAnonymousInnerClassHelper();

	  private class ListenerAnonymousInnerClassHelper : EngagementActivityManager.Listener
	  {
		  public ListenerAnonymousInnerClassHelper()
		  {
		  }

		  public virtual void onCurrentActivityChanged(WeakReference<Activity> currentActivity, string engagementAlias)
		  {
			/* Hide notifications when entering new activity (it may contain areas embedded in the layout) */
			Activity activity = currentActivity.get();
			Activity lastActivity = outerInstance.mLastActivity.get();
			if (activity != null && !activity.Equals(lastActivity))
			{
			  hideInAppNotifications(activity);
			}

			/* If we were notifying in activity and exit that one */
			if (outerInstance.mState == State.NOTIFYING_IN_APP && lastActivity != null && !lastActivity.Equals(activity))
			{
			  /* Hide notifications */
			  hideInAppNotifications(lastActivity);

			  /* We are now idle */
			  outerInstance.setIdle();
			}

			/* Update last activity (if entering a new one) */
			outerInstance.mLastActivity = currentActivity;

			/*
			 * Guard against application being put to background in showing state but not yet inside
			 * showing activity, make sure the state machine is not stuck in showing. To produce, click on
			 * notification, then quickly press HOME in loading activity while showing code already
			 * triggered...
			 */
			if (outerInstance.mState == State.SHOWING && activity != null && !activity.ComponentName.Equals(outerInstance.mShowingActivity))
			{
			  /* Check if content not yet processed */
			  EngagementReachInteractiveContent content = outerInstance.getContent(outerInstance.mCurrentShownContentId);
			  if (content != null)
			  {
				/* In case of a system notification, there is no way to go back to content */
				if (content.SystemNotification)
				{
				  /* So make sure display/exit feedbacks are sent */
				  content.displayContent(outerInstance.mContext);
				  content.exitContent(outerInstance.mContext);

				  /* Return now to avoid extra scan */
				  return;
				}

				/* Otherwise just set idle */
				else
				{
				  outerInstance.setIdle();
				}
			  }
			}

			/* If we are idle, pick a content */
			if (outerInstance.mState == State.IDLE)
			{
			  outerInstance.scanContent(false);
			}
		  }

		  /// <summary>
		  /// Hide all possible overlays and notification areas in the specified activity. </summary>
		  /// <param name="activity"> activity to operate on. </param>
		  private void hideInAppNotifications(Activity activity)
		  {
			/* For all categories */
			foreach (KeyValuePair<string, EngagementNotifier> entry in outerInstance.mNotifiers.SetOfKeyValuePairs())
			{
			  /* Hide overlays */
			  string category = entry.Key;
			  EngagementNotifier notifier = entry.Value;
			  int? overlayId = notifier.getOverlayViewId(category);
			  if (overlayId != null)
			  {
				View overlayView = activity.findViewById(overlayId);
				if (overlayView != null)
				{
				  overlayView.Visibility = View.GONE;
				}
			  }

			  /* Hide areas */
			  int? areaId = notifier.getInAppAreaId(category);
			  if (areaId != null)
			  {
				View areaView = activity.findViewById(areaId);
				if (areaView != null)
				{
				  areaView.Visibility = View.GONE;
				}
			  }
			}
		  }
	  }

	  /// <summary>
	  /// Datapush campaigns being broadcasted </summary>
	  private readonly ISet<long?> mPendingDataPushes = new HashSet<long?>();

	  /// <summary>
	  /// Init the reach agent. </summary>
	  /// <param name="context"> application context. </param>
	  private EngagementReachAgent(Context context)
	  {
		/* Keep application context */
		mContext = context;

		/* Get app last update time */
		long appLastUpdateTime;
		try
		{
		  appLastUpdateTime = context.PackageManager.getPackageInfo(context.PackageName, 0).lastUpdateTime;
		}
		catch (Exception)
		{
		  /* If package manager crashed, assume no upgrade */
		  appLastUpdateTime = 0;
		}
		mAppLastUpdateTime = appLastUpdateTime;

		/* Install default category notifier, can be overridden by user */
		mNotifiers[CATEGORY_DEFAULT] = new EngagementDefaultNotifier(context);

		/* Open reach database */
		ContentValues schema = new ContentValues();
		schema.put(DLC_ID, "");
		schema.put(CAMPAIGN_ID, "");
		schema.put(DLC, 1);
		schema.put(CATEGORY, "");
		schema.put(DELIVERY_TIME, "");
		schema.put(TTL, 1L);
		schema.put(USER_TIME_ZONE, 1);
		schema.put(NOTIFICATION_TYPE, "");
		schema.put(NOTIFICATION_ICON, 1);
		schema.put(NOTIFICATION_CLOSEABLE, 1);
		schema.put(NOTIFICATION_VIBRATION, 1);
		schema.put(NOTIFICATION_SOUND, 1);
		schema.put(NOTIFICATION_TITLE, "");
		schema.put(NOTIFICATION_MESSAGE, "");
		schema.put(NOTIFICATION_BIG_TEXT, "");
		schema.put(NOTIFICATION_BIG_PICTURE, "");
		schema.put(ACTION_URL, "");
		schema.put(PAYLOAD, "");
		schema.put(DOWNLOAD_ID, 1L);
		schema.put(NOTIFICATION_FIRST_DISPLAYED_DATE, 1L);
		schema.put(NOTIFICATION_LAST_DISPLAYED_DATE, 1L);
		schema.put(NOTIFICATION_ACTIONED, 1);
		schema.put(CONTENT_DISPLAYED, 1);
		mDB = new EngagementStorage(context, "engagement.reach.db", 7, "content", schema, null);

		/* Retrieve device id */
		EngagementAgent.getInstance(context).getDeviceId(new CallbackAnonymousInnerClassHelper(this));
	  }

	  private class CallbackAnonymousInnerClassHelper : EngagementAgent.Callback<string>
	  {
		  private readonly EngagementReachAgent outerInstance;

		  public CallbackAnonymousInnerClassHelper(EngagementReachAgent outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void onResult(string deviceId)
		  {
			/* Update parameters */
			outerInstance.mInjectedParams["{deviceid}"] = deviceId;

			/*
			 * Watch current activity, if we still have not exited the constructor we have to delay the
			 * call so that singleton is set. It can happen in the unlikely scenario where getDeviceId
			 * returns synchronously the result.
			 */
			if (sInstance != null)
			{
			  sActivityManager.addCurrentActivityListener(mActivityListener);
			}
			else
			{
			  (new Handler()).post(() =>
			  {
			  sActivityManager.addCurrentActivityListener(mActivityListener);
			  });
			}
		  }
	  }

	  /// <summary>
	  /// Get the unique instance. </summary>
	  /// <param name="context"> any valid context </param>
	  public static EngagementReachAgent getInstance(Context context)
	  {
		/* Always check this even if we instantiate once to trigger null pointer in all cases */
		if (sInstance == null)
		{
		  sInstance = new EngagementReachAgent(context.ApplicationContext);
		}
		return sInstance;
	  }

	  /// <summary>
	  /// Register a custom notifier for a set of content categories. You have to call this method in
	  /// <seealso cref="Application#onCreate()"/> because notifications can happen at any time. </summary>
	  /// <param name="notifier"> notifier to register for a set of categories. </param>
	  /// <param name="categories"> one or more category. </param>
	  public virtual void registerNotifier(EngagementNotifier notifier, params string[] categories)
	  {
		foreach (string category in categories)
		{
		  mNotifiers[category] = notifier;
		}
	  }

	  /// <summary>
	  /// Get content by its local identifier. </summary>
	  /// <param name="localId"> the content local identifier. </param>
	  /// <returns> the content if found, null otherwise. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public <T extends EngagementReachContent> T getContent(long localId)
	  public virtual T getContent<T>(long localId) where T : EngagementReachContent
	  {
		/* Return content from cache if possible */
		EngagementReachContent cachedContent = mContentCache.get(localId);
		if (cachedContent != null)
		{
		  try
		  {
			return (T) cachedContent;
		  }
		  catch (System.InvalidCastException)
		  {
			/* Invalid type */
			return null;
		  }
		}

		/*
		 * Otherwise fetch in SQLite: required if the application process has been killed while clicking
		 * on a system notification or while fetching another content than the current one.
		 */
		else
		{
		  /* Fetch from storage */
		  ContentValues values = mDB.get(localId);
		  if (values != null)
		  {
			try
			{
			  return (T) parseContentFromStorage(values);
			}
			catch (System.InvalidCastException)
			{
			  /* Invalid type */
			}
			catch (Exception)
			{
			  /*
			   * Delete content that cannot be parsed, may be corrupted data, we cannot send "dropped"
			   * feedback as we need the Reach contentId and kind.
			   */
			  deleteContent(localId, values.getAsLong(DOWNLOAD_ID));
			}
		  }

		  /* Not found, invalid type or an error occurred */
		  return null;
		}
	  }

	  /// <summary>
	  /// Get content by its intent (containing the content local identifier such as in intents
	  /// associated with the <seealso cref="#INTENT_ACTION_ACTION_NOTIFICATION"/> action). </summary>
	  /// <param name="intent"> intent containing the local identifier under the
	  ///          {@value #INTENT_EXTRA_CONTENT_ID} extra key (as a long). </param>
	  /// <returns> the content if found, null otherwise. </returns>
	  public virtual T getContent<T>(Intent intent) where T : EngagementReachContent
	  {
		return getContent(intent.getLongExtra(INTENT_EXTRA_CONTENT_ID, 0));
	  }

	  /// <summary>
	  /// Get content by a download identifier. </summary>
	  /// <param name="downloadId"> intent containing the local identifier under the
	  ///          {@value #INTENT_EXTRA_CONTENT_ID} extra key (as a long). </param>
	  /// <returns> the content if found, null otherwise. </returns>
	  public virtual T getContentByDownloadId<T>(long downloadId) where T : EngagementReachContent
	  {
		return getContent(mContext.getSharedPreferences(DOWNLOAD_SETTINGS, 0).getLong(downloadId.ToString(), 0));
	  }

	  /// <summary>
	  /// If for some reason you accepted a content in
	  /// <seealso cref="EngagementNotifier#handleNotification(EngagementReachInteractiveContent)"/> but returned
	  /// null to tell that the notification was not ready to be displayed, call this function once the
	  /// notification is ready. For example this is used once the big picture of a system notification
	  /// has been downloaded (or failed to be downloaded). If the content has not been shown or dropped,
	  /// this will trigger a new call to
	  /// <seealso cref="EngagementNotifier#handleNotification(EngagementReachInteractiveContent)"/> if the current
	  /// U.I context allows so (activity/session/any time filters are evaluated again). </summary>
	  /// <param name="content"> content to notify. </param>
	  public virtual void notifyPendingContent(EngagementReachInteractiveContent content)
	  {
		/* Notification is not managed anymore can be submitted to notifiers again */
		long localId = content.LocalId;
		mPendingNotifications.Remove(localId);

		/* Update notification if not too late e.g. notification not yet dismissed */
		if ((mState != State.LOADING && mState != State.SHOWING) || mCurrentShownContentId != localId)
		{
		  try
		  {
			notifyContent(content, false);
		  }
		  catch (Exception)
		  {
			content.dropContent(mContext);
		  }
		}
	  }

	  /// <summary>
	  /// Report loading screen has been exited. </summary>
	  /// <param name="contentId"> content associated to loading screen. </param>
	  public virtual void exitLoading(long contentId)
	  {
		/*
		 * Make sure we are not already in showing state: loading will be exiting by starting the real
		 * activity. Check also if a loading screen replaces another loading screen, in that case stay
		 * in loading state. If we are sure we exit a loading screen to return to application or
		 * background, then we can set back to idle state.
		 */
		if (isLoading(contentId))
		{
		  setIdle();
		}
	  }

	  /// <summary>
	  /// Check if the agent is loading a particular content. </summary>
	  /// <param name="contentId"> content id to test. </param>
	  /// <returns> true iff agent is loading that content. </returns>
	  public virtual bool isLoading(long contentId)
	  {
		return mState == State.LOADING && contentId == mCurrentShownContentId;
	  }

	  /// <summary>
	  /// Called when a new content is received. </summary>
	  /// <param name="payload"> native push payload. </param>
	  internal virtual void onContentReceived(Bundle payload)
	  {
		/* Parse campaign id before everything else for early feedbacks */
		string ci = payload.getString(CAMPAIGN_ID);
		CampaignId campaignId;
		try
		{
		  campaignId = new CampaignId(ci);
		}
		catch (Exception)
		{
		  /* If even campaign id parsing fails, we cannot send any feedback */
		  return;
		}

		/* Convert String Bundle to typed ContentValues (storage format), handle default values here. */
		ContentValues values = new ContentValues();
		values.put(CAMPAIGN_ID, ci);
		values.put(DLC, parseInt(payload, DLC, 0));
		values.put(DLC_ID, payload.getString(DLC_ID));
		values.put(CATEGORY, payload.getString(CATEGORY));
		values.put(DELIVERY_TIME, payload.containsKey(DELIVERY_TIME) ? payload.getString(DELIVERY_TIME) : "a");
		values.put(TTL, parseLong(payload, TTL));
		values.put(USER_TIME_ZONE, parseInt(payload, USER_TIME_ZONE, 0));
		values.put(NOTIFICATION_TYPE, payload.containsKey(NOTIFICATION_TYPE) ? payload.getString(NOTIFICATION_TYPE) : "p");
		values.put(NOTIFICATION_ICON, parseInt(payload, NOTIFICATION_ICON, 1));
		values.put(NOTIFICATION_CLOSEABLE, parseInt(payload, NOTIFICATION_CLOSEABLE, 1));
		values.put(NOTIFICATION_VIBRATION, parseInt(payload, NOTIFICATION_VIBRATION, 0));
		values.put(NOTIFICATION_SOUND, parseInt(payload, NOTIFICATION_SOUND, 0));
		values.put(NOTIFICATION_TITLE, payload.getString(NOTIFICATION_TITLE));
		values.put(NOTIFICATION_MESSAGE, payload.getString(NOTIFICATION_MESSAGE));
		values.put(NOTIFICATION_BIG_TEXT, payload.getString(NOTIFICATION_BIG_TEXT));
		values.put(NOTIFICATION_BIG_PICTURE, payload.getString(NOTIFICATION_BIG_PICTURE));
		values.put(ACTION_URL, payload.getString(ACTION_URL));

		/* Parse content */
		EngagementReachContent content;
		try
		{
		  content = parseContent(campaignId, values);
		}
		catch (Exception)
		{
		  /* Send dropped feedback */
		  campaignId.sendFeedBack(mContext, "dropped", null);
		  return;
		}

		/* Content is parsed, send delivered feedback */
		campaignId.sendFeedBack(mContext, "delivered", null);

		/* Proceed */
		try
		{
		  /* Store content in SQLite */
		  long localId = mDB.put(values).Value;
		  content.LocalId = localId;

		  /*
		   * If we don't know device id yet, keep it for later. If we are idle, check if the content
		   * notification can be shown during the current U.I context. Datapush can be "notified" even
		   * when not idle.
		   */
		  if (mInjectedParams.ContainsKey("{deviceid}"))
		  {
			notifyContent(content, false);
		  }
		}
		catch (Exception)
		{
		  /* Drop content on error */
		  content.dropContent(mContext);
		}
	  }

	  /// <summary>
	  /// GCM & ADM push payload values are always strings unfortunately. We have to parse numbers. </summary>
	  /// <param name="bundle"> bundle. </param>
	  /// <param name="key"> key to extract </param>
	  /// <param name="defaultValue"> default value if key is missing or not an int. </param>
	  /// <returns> string value parsed as an int. </returns>
	  private static int parseInt(Bundle bundle, string key, int defaultValue)
	  {
		try
		{
		  return int.Parse(bundle.getString(key));
		}
		catch (Exception)
		{
		  return defaultValue;
		}
	  }

	  /// <summary>
	  /// GCM & ADM push payload values are always strings unfortunately. We have to parse numbers. </summary>
	  /// <param name="bundle"> bundle. </param>
	  /// <param name="key"> key to extract </param>
	  /// <returns> string value parsed as a Long. If key is missing or not a long, returns null. </returns>
	  private static long? parseLong(Bundle bundle, string key)
	  {
		try
		{
		  return long.Parse(bundle.getString(key));
		}
		catch (Exception)
		{
		  return null;
		}
	  }

	  /// <summary>
	  /// Called when a message download completes. </summary>
	  /// <param name="message"> message parameters. </param>
	  internal virtual void onMessageDownloaded(Bundle message)
	  {
		/*
		 * Get all campaigns from storage that matches this DLC identifier (manual push can have several
		 * pushes for same DLC id).
		 */
		string id = message.getString(INTENT_EXTRA_ID);
		if (id == null)
		{
		  return;
		}
		EngagementStorage.Scanner scanner = mDB.getScanner(DLC_ID, id);
		foreach (ContentValues values in scanner)
		{
		  /* Parse it */
		  EngagementReachContent content = null;
		  try
		  {
			/* Parse and restore state */
			content = parseContentFromStorage(values);
			long localId = content.LocalId;
			mPendingDLCs.Remove(localId);
			bool mustNotify = !content.DlcCompleted && (content.hasNotificationDLC() || content is EngagementDataPush);

			/* Parse downloaded payload as JSON */
			string rawPayload = message.getString(INTENT_EXTRA_PAYLOAD);
			JSONObject payload = new JSONObject(rawPayload);
			content.Payload = payload;

			/* Store it */
			ContentValues update = new ContentValues();
			update.put(PAYLOAD, rawPayload);
			mDB.update(localId, update);

			/* Cache is out of date, update it */
			mContentCache.put(localId, content);

			/* Start content if we were loading it */
			if (mState == State.LOADING && localId == mCurrentShownContentId)
			{
			  showContent(content);
			}

			/* Notify if we were waiting for DLC to generate notification */
			else if (mustNotify && mInjectedParams.ContainsKey("{deviceid}"))
			{
			  notifyContent(content, false);
			}
		  }
		  catch (Exception)
		  {
			/*
			 * Delete content and send dropped feedback if possible depending on how much state we could
			 * restore.
			 */
			if (content == null)
			{
			  long? oid = values.getAsLong(OID);
			  if (oid != null)
			  {
				delete(oid.Value);
			  }
			}
			else
			{
			  content.dropContent(mContext);
			}
		  }
		}
		scanner.close();
	  }

	  /// <summary>
	  /// Called when a download for a content has been scheduled. </summary>
	  /// <param name="content"> content. </param>
	  /// <param name="downloadId"> download identifier. </param>
	  internal virtual void onDownloadScheduled(EngagementReachContent content, long downloadId)
	  {
		/* Save download identifier */
		ContentValues values = new ContentValues();
		values.put(DOWNLOAD_ID, downloadId);
		mDB.update(content.LocalId, values);
		mContext.getSharedPreferences(DOWNLOAD_SETTINGS, 0).edit().putLong(downloadId.ToString(), content.LocalId).commit();
	  }

	  /// <summary>
	  /// Called when download has been completed. </summary>
	  /// <param name="content"> content. </param>
	  internal virtual void onDownloadComplete(EngagementReachInteractiveContent content)
	  {
		/* Cancel alarm */
		Intent intent = new Intent(INTENT_ACTION_DOWNLOAD_TIMEOUT);
		intent.Package = mContext.PackageName;
		int requestCode = (int) content.LocalId;
		PendingIntent operation = PendingIntent.getBroadcast(mContext, requestCode, intent, 0);
		AlarmManager alarmManager = (AlarmManager) mContext.getSystemService(Context.ALARM_SERVICE);
		alarmManager.cancel(operation);

		/* Update notification if not too late e.g. notification not yet dismissed */
		notifyPendingContent(content);
	  }

	  /// <summary>
	  /// Called when a download takes too much time for a content. </summary>
	  /// <param name="content"> content. </param>
	  internal virtual void onDownloadTimeout(EngagementReachInteractiveContent content)
	  {
		/* Notify without downloaded data */
		notifyPendingContent(content);
	  }

	  /// <summary>
	  /// Called when a notification is reported as displayed. </summary>
	  /// <param name="content"> displayed content's notification. </param>
	  internal virtual void onNotificationDisplayed(EngagementReachInteractiveContent content)
	  {
		ContentValues values = new ContentValues();
		values.put(NOTIFICATION_FIRST_DISPLAYED_DATE, content.NotificationFirstDisplayedDate);
		values.put(NOTIFICATION_LAST_DISPLAYED_DATE, content.NotificationLastDisplayedDate);
		mDB.update(content.LocalId, values);
	  }

	  /// <summary>
	  /// Called when a notification is actioned. </summary>
	  /// <param name="content"> content associated to the notification. </param>
	  /// <param name="launchIntent"> true to launch intent. </param>
	  internal virtual void onNotificationActioned(EngagementReachInteractiveContent content, bool launchIntent)
	  {
		/* Intents can fail */
		try
		{
		  /* Persist content state */
		  updateContentStatusTrue(content, NOTIFICATION_ACTIONED);

		  /* Nothing more to do if intent must not be launched */
		  long localId = content.LocalId;
		  if (!launchIntent)
		  {
			/* Assume shown */
			mState = State.SHOWING;
			mCurrentShownContentId = localId;
			return;
		  }

		  /* Notification announcement */
		  if (content is EngagementNotifAnnouncement)
		  {
			/* Execute action and cancel notification if system */
			EngagementNotifAnnouncement announcement = (EngagementNotifAnnouncement) content;
			getNotifier(content).executeNotifAnnouncementAction(announcement);
			cancelSystemNotification(announcement);
		  }

		  /* If we have the DLC start a content activity in its own task */
		  else if (checkRequestDlc(content))
		  {
			showContent(content);
		  }

		  /*
		   * Download has been scheduled, show progress dialog if not already done for this content (do
		   * nothing if clicking several time on same system notification).
		   */
		  else if (!isLoading(localId))
		  {
			/* Update state */
			mState = State.LOADING;
			mCurrentShownContentId = localId;

			/* Start loading activity */
			showOrRefreshLoading(content);
		  }
		}
		catch (Exception)
		{
		  /* Drop content on any error */
		  content.dropContent(mContext);
		}
	  }

	  /// <summary>
	  /// Called when a content is reported as displayed. </summary>
	  /// <param name="content"> displayed content. </param>
	  internal virtual void onContentDisplayed(EngagementReachInteractiveContent content)
	  {
		/* Make sure we update state in DB */
		updateContentStatusTrue(content, CONTENT_DISPLAYED);

		/* Cancel notification */
		cancelSystemNotification(content);
	  }

	  /// <summary>
	  /// When a content is processed, we can remove it from SQLite. We can also check if a new one can
	  /// be shown.
	  /// </summary>
	  internal virtual void onContentProcessed(EngagementReachContent content)
	  {
		/* Delete content */
		deleteContent(content);

		/* If loading update loading window to display error message */
		if (mState == State.LOADING)
		{
		  /* Only if not replaced by another loading window */
		  if (content.LocalId == mCurrentShownContentId)
		  {
			/* We are now idle */
			setIdle();

			/*
			 * Refresh loading activity saying that the content is now deleted (intent with no id and
			 * isLoading will return false)
			 */
			showOrRefreshLoading(content, Intent.FLAG_ACTIVITY_SINGLE_TOP);
		  }
		}

		/* If not loading and we were not scanning, set idle and scan for next content */
		else if (!mScanning)
		{
		  /* We are now idle */
		  setIdle();

		  /* Look for new in-app content if just exiting an in-app content */
		  if (!content.SystemNotification && !(content is EngagementDataPush))
		  {
			scanContent(false);
		  }
		}
	  }

	  /// <summary>
	  /// Called when the device has rebooted. </summary>
	  internal virtual void onDeviceBoot()
	  {
		/* Replay system notifications */
		scanContent(true);
	  }

	  /// <summary>
	  /// Get notifier for a content depending on its category. </summary>
	  /// <param name="content"> content to notify. </param>
	  /// <returns> notifier for a content depending on its category. </returns>
	  private EngagementNotifier getNotifier(EngagementReachContent content)
	  {
		/* Delegate to notifiers, select the right one for the current category */
		EngagementNotifier notifier = mNotifiers[content.Category];

		/* Fail over default category if not found */
		if (notifier == null)
		{
		  notifier = mNotifiers[CATEGORY_DEFAULT];
		}
		return notifier;
	  }

	  /// <summary>
	  /// Parse a content. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> content values. </param>
	  /// <returns> content. </returns>
	  /// <exception cref="Exception"> parsing problem. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private EngagementReachContent parseContent(CampaignId campaignId, android.content.ContentValues values) throws Exception
	  private EngagementReachContent parseContent(CampaignId campaignId, ContentValues values)
	  {
		switch (campaignId.getKind())
		{
		  case ANNOUNCEMENT:
			if ((values.getAsInteger(DLC) & FLAG_DLC_CONTENT) == FLAG_DLC_CONTENT)
			{
			  return new EngagementAnnouncement(campaignId, values, mInjectedParams);
			}
			return new EngagementNotifAnnouncement(campaignId, values, mInjectedParams);

		  case POLL:
			return new EngagementPoll(campaignId, values);

		  case DATAPUSH:
			return new EngagementDataPush(campaignId, values, mInjectedParams);

		  default:
			throw new System.ArgumentException("Invalid campaign id");
		}
	  }

	  /// <summary>
	  /// Parse a content retrieved from storage. </summary>
	  /// <param name="values"> content as returned by the storage. </param>
	  /// <returns> content. </returns>
	  /// <exception cref="Exception"> parsing problem, most likely invalid XML. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private EngagementReachContent parseContentFromStorage(android.content.ContentValues values) throws Exception
	  private EngagementReachContent parseContentFromStorage(ContentValues values)
	  {
		/* Parse payload */
		CampaignId campaignId = new CampaignId(values.getAsString(CAMPAIGN_ID));
		EngagementReachContent content = parseContent(campaignId, values);

		/* Parse state */
		content.State = values;

		/* Parse local id */
		content.LocalId = values.getAsLong(OID);
		return content;
	  }

	  /// <summary>
	  /// Cancel system notification </summary>
	  private void cancelSystemNotification(EngagementReachInteractiveContent content)
	  {
		if (content.SystemNotification)
		{
		  NotificationManager notificationManager = (NotificationManager) mContext.getSystemService(NOTIFICATION_SERVICE);
		  notificationManager.cancel(getNotifier(content).getNotificationId(content));
		}
	  }

	  /// <summary>
	  /// Update a content's status. </summary>
	  /// <param name="content"> content to update. </param>
	  /// <param name="status"> status to set to true. </param>
	  private void updateContentStatusTrue(EngagementReachContent content, string status)
	  {
		ContentValues values = new ContentValues();
		values.put(status, 1);
		mDB.update(content.LocalId, values);
	  }

	  /// <summary>
	  /// Show or refresh loading activity. </summary>
	  /// <param name="content"> campaign being loaded. </param>
	  /// <param name="flags"> additional activity flags. </param>
	  private void showOrRefreshLoading(EngagementReachContent content, params int[] flags)
	  {
		Intent intent = new Intent(INTENT_ACTION_LOADING);
		string category = content.Category;
		if (category != null)
		{
		  intent.addCategory(category);
		}
		filterIntentWithCategory(intent);
		setContentIdExtra(intent, content);
		intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_EXCLUDE_FROM_RECENTS);
		foreach (int flag in flags)
		{
		  intent.addFlags(flag);
		}
		mContext.startActivity(intent);
	  }

	  /// <summary>
	  /// Show content activity </summary>
	  private void showContent(EngagementReachContent content)
	  {
		/* Update state */
		mState = State.SHOWING;
		mCurrentShownContentId = content.LocalId;

		/* Start activity */
		Intent intent = content.Intent;
		filterIntentWithCategory(intent);
		intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_NO_HISTORY;
		mShowingActivity = intent.Component;
		mContext.startActivity(intent);
	  }

	  /// <summary>
	  /// Scan reach database and notify the first content that match the current U.I context </summary>
	  /// <param name="replaySystemNotifications"> true iff system notifications must be replayed. </param>
	  private void scanContent(bool replaySystemNotifications)
	  {
		/* Change state */
		mScanning = true;

		/* For all database rows */
		EngagementStorage.Scanner scanner = mDB.getScanner();
		foreach (ContentValues values in scanner)
		{
		  /* Parsing may fail */
		  EngagementReachContent content = null;
		  try
		  {
			/* Parse content */
			content = parseContentFromStorage(values);

			/* Possibly generate a notification */
			notifyContent(content, replaySystemNotifications);
		  }
		  catch (Exception)
		  {
			/*
			 * If the content was parsed but an error occurred while notifying, send "dropped" feedback
			 * and delete
			 */
			if (content != null)
			{
			  content.dropContent(mContext);
			}

			/* Otherwise we just delete */
			else
			{
			  deleteContent(values.getAsLong(OID), values.getAsLong(DOWNLOAD_ID));
			}

			/* In any case we continue parsing */
		  }
		}

		/* Close scanner */
		scanner.close();

		/* Scan finished */
		mScanning = false;
	  }

	  /// <summary>
	  /// Fill an intent with a content identifier as extra. </summary>
	  /// <param name="intent"> intent. </param>
	  /// <param name="content"> content. </param>
	  internal static void setContentIdExtra(Intent intent, EngagementReachContent content)
	  {
		intent.putExtra(INTENT_EXTRA_CONTENT_ID, content.LocalId);
	  }

	  /// <summary>
	  /// Try to notify the content to the user. </summary>
	  /// <param name="content"> reach content. </param>
	  /// <param name="replaySystemNotifications"> true iff system notifications must be replayed. </param>
	  /// <exception cref="RuntimeException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void notifyContent(final EngagementReachContent content, boolean replaySystemNotifications) throws RuntimeException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  private void notifyContent(EngagementReachContent content, bool replaySystemNotifications)
	  {
		/* Check expiry */
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long localId = content.getLocalId();
		long localId = content.LocalId;
		if (content.hasExpired())
		{
		  /* Delete */
		  deleteContent(content);
		  return;
		}

		/* If datapush, just broadcast, can be done in parallel with another content */
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.content.Intent intent = content.getIntent();
		Intent intent = content.Intent;
		if (content is EngagementDataPush)
		{
		  /* Delay data push until DLC completes */
		  if (!checkRequestDlc(content))
		  {
			return;
		  }

		  /* If it's a datapush it may already be in the process of broadcasting. */
		  if (!mPendingDataPushes.Add(localId))
		  {
			return;
		  }

		  /* Broadcast intent */
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EngagementDataPush dataPush = (EngagementDataPush) content;
		  EngagementDataPush dataPush = (EngagementDataPush) content;
		  intent.Package = mContext.PackageName;
		  mContext.sendOrderedBroadcast(intent, null, new BroadcastReceiverAnonymousInnerClassHelper(this, localId, intent, dataPush), null, RESULT_UNDEFINED, null, null);

		  /* Datapush processed */
		  return;
		}

		/* If notification has pending downloadable content, delay */
		if (content.hasNotificationDLC() && !checkRequestDlc(content))
		{
		  return;
		}

		/* Don't notify in-app if we are already notifying in app or showing a content */
		if (mState != State.IDLE && !content.SystemNotification)
		{
		  return;
		}

		/* Don't process again a pending notification */
		if (mPendingNotifications.Contains(localId))
		{
		  return;
		}

		/* Not an interactive content, exit (but there is no other type left, this is just a cast guard) */
		if (!(content is EngagementReachInteractiveContent))
		{
		  return;
		}
		EngagementReachInteractiveContent iContent = (EngagementReachInteractiveContent) content;

		/* Don't replay system notification unless told otherwise. */
		if (!replaySystemNotifications && iContent.SystemNotification && iContent.NotificationLastDisplayedDate != null && iContent.NotificationLastDisplayedDate > mAppLastUpdateTime)
		{
		  return;
		}

		/* Check if the content can be notified in the current context (behavior) */
		if (!iContent.canNotify(sActivityManager.CurrentActivityAlias))
		{
		  return;
		}

		/* If there is a show intent */
		if (intent != null)
		{
		  filterIntentWithCategory(intent);
		}

		/* Delegate notification */
		bool? notifierResult = getNotifier(content).handleNotification(iContent);

		/* Check if notifier rejected content notification for now */
		if (false.Equals(notifierResult))

		  /* The notifier rejected the content, nothing more to do */
		{
		  return;
		}

		/* Cache content if accepted, it will most likely be used again soon for the next steps. */
		mContentCache.put(localId, content);

		/*
		 * If notifier did not return null (e.g. returned true, meaning actually accepted the content),
		 * we assume the notification is correctly displayed.
		 */
		if (true.Equals(notifierResult))
		{
		  /* Report displayed feedback */
		  iContent.displayNotification(mContext);

		  /* Track in-app content life cycle: one at a time */
		  if (!iContent.SystemNotification)
		  {
			mState = State.NOTIFYING_IN_APP;
		  }
		}

		/* Track pending notifications to avoid re-processing them every time we change activity. */
		if (notifierResult == null)
		{
		  mPendingNotifications.Add(localId);
		}
	  }

	  private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
	  {
		  private readonly EngagementReachAgent outerInstance;

		  private long localId;
		  private Intent intent;
		  private com.microsoft.azure.engagement.reach.EngagementDataPush dataPush;

		  public BroadcastReceiverAnonymousInnerClassHelper(EngagementReachAgent outerInstance, long localId, Intent intent, com.microsoft.azure.engagement.reach.EngagementDataPush dataPush)
		  {
			  this.outerInstance = outerInstance;
			  this.localId = localId;
			  this.intent = intent;
			  this.dataPush = dataPush;
		  }

		  public override void onReceive(Context context, Intent intent)
		  {
			/* The last broadcast receiver to set a defined result wins (to determine which result). */
			switch (ResultCode)
			{
			  case RESULT_OK:
				dataPush.actionContent(context);
				break;

			  case RESULT_CANCELED:
				dataPush.exitContent(context);
				break;

			  default:
				dataPush.dropContent(context);
			break;
			}

			/* Clean broadcast state */
			outerInstance.mPendingDataPushes.Remove(localId);
		  }
	  }

	  /// <summary>
	  /// Check DLC state and triggers download if needed. </summary>
	  /// <param name="content"> content. </param>
	  /// <returns> true if DLC is not needed or is ready. </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private boolean checkRequestDlc(final EngagementReachContent content)
	  private bool checkRequestDlc(EngagementReachContent content)
	  {
		if (content.hasDLC())
		{
		  bool dlcCompleted = content.DlcCompleted;
		  if (!dlcCompleted && mPendingDLCs.Add(content.LocalId))
		  {
			EngagementAgent.getInstance(mContext).getMessage(content.DlcId);
		  }
		  return dlcCompleted;
		}
		return true;
	  }

	  /// <summary>
	  /// Set idle, that means we are ready for a next content to be notified in-app. </summary>
	  private void setIdle()
	  {
		mState = State.IDLE;
		mCurrentShownContentId = null;
	  }

	  /// <summary>
	  /// Filter the intent to a single activity so a chooser won't pop up. Do not handle fall back here. </summary>
	  /// <param name="intent"> intent to filter. </param>
	  private void filterIntent(Intent intent)
	  {
		foreach (ResolveInfo resolveInfo in mContext.PackageManager.queryIntentActivities(intent, 0))
		{
		  ActivityInfo activityInfo = resolveInfo.activityInfo;
		  string packageName = mContext.PackageName;
		  if (activityInfo.packageName.Equals(packageName))
		  {
			intent.Component = new ComponentName(packageName, activityInfo.name);
			break;
		  }
		}
	  }

	  /// <summary>
	  /// Filter the intent to a single activity so a chooser won't pop up. If not found, it tries to
	  /// resolve intent by falling back to default category. </summary>
	  /// <param name="intent"> intent to filter. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void filterIntentWithCategory(final android.content.Intent intent)
	  private void filterIntentWithCategory(Intent intent)
	  {
		/* Filter intent for the target package name */
		filterIntent(intent);

		/* If the intent could not be resolved */
		if (intent.Component == null)
		{
		  /* If there was no category */
		  if (intent.Categories == null)

			/* Notification cannot be done */
		  {
			throw new ActivityNotFoundException();
		  }

		  /* Remove categories */
		  ICollection<string> categories = new HashSet<string>(intent.Categories);
		  foreach (string category in categories)
		  {
			intent.removeCategory(category);
		  }

		  /* Try filtering again */
		  filterIntent(intent);

		  /* Notification cannot be done, skip content */
		  if (intent.Component == null)
		  {
			throw new ActivityNotFoundException();
		  }
		}
	  }

	  /// <summary>
	  /// Delete content from storage and any associated download or notification. </summary>
	  /// <param name="content"> content to delete. </param>
	  private void deleteContent(EngagementReachContent content)
	  {
		if (content is EngagementReachInteractiveContent)
		{
		  cancelSystemNotification((EngagementReachInteractiveContent) content);
		}
		deleteContent(content.LocalId, content.DownloadId);
	  }

	  /// <summary>
	  /// Delete content from storage and any associated download. </summary>
	  /// <param name="localId"> content identifier to delete. </param>
	  /// <param name="downloadId"> download identifier to delete if any. </param>
	  private void deleteContent(long localId, long? downloadId)
	  {
		/* Delete all references */
		delete(localId);

		/* Delete associated download if any */
		if (downloadId != null)
		{
		  /* Delete mapping */
		  mContext.getSharedPreferences(DOWNLOAD_SETTINGS, 0).edit().remove(downloadId.ToString()).commit();

		  /* Cancel download and delete file */
		  EngagementNotificationUtilsV11.deleteDownload(mContext, downloadId.Value);
		}
	  }

	  /// <summary>
	  /// Delete content </summary>
	  private void delete(long localId)
	  {
		mDB.delete(localId);
		mPendingNotifications.Remove(localId);
		mContentCache.remove(localId);
	  }
	}

}