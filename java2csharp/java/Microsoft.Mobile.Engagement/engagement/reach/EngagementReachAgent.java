/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import static android.app.Activity.RESULT_CANCELED;
import static android.app.Activity.RESULT_OK;
import static android.content.Context.NOTIFICATION_SERVICE;
import static android.content.Intent.CATEGORY_DEFAULT;
import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_ID;
import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_PAYLOAD;
import static com.microsoft.azure.engagement.reach.ContentStorage.ACTION_URL;
import static com.microsoft.azure.engagement.reach.ContentStorage.CAMPAIGN_ID;
import static com.microsoft.azure.engagement.reach.ContentStorage.CATEGORY;
import static com.microsoft.azure.engagement.reach.ContentStorage.CONTENT_DISPLAYED;
import static com.microsoft.azure.engagement.reach.ContentStorage.DELIVERY_TIME;
import static com.microsoft.azure.engagement.reach.ContentStorage.DLC;
import static com.microsoft.azure.engagement.reach.ContentStorage.DLC_ID;
import static com.microsoft.azure.engagement.reach.ContentStorage.DOWNLOAD_ID;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_ACTIONED;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_BIG_PICTURE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_BIG_TEXT;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_CLOSEABLE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_FIRST_DISPLAYED_DATE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_ICON;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_LAST_DISPLAYED_DATE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_MESSAGE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_SOUND;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_TITLE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_TYPE;
import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_VIBRATION;
import static com.microsoft.azure.engagement.reach.ContentStorage.OID;
import static com.microsoft.azure.engagement.reach.ContentStorage.PAYLOAD;
import static com.microsoft.azure.engagement.reach.ContentStorage.TTL;
import static com.microsoft.azure.engagement.reach.ContentStorage.USER_TIME_ZONE;
import static com.microsoft.azure.engagement.reach.EngagementReachContent.FLAG_DLC_CONTENT;

import java.lang.ref.WeakReference;
import java.util.Collection;
import java.util.HashMap;
import java.util.HashSet;
import java.util.Map;
import java.util.Set;

import org.json.JSONObject;

import android.app.Activity;
import android.app.AlarmManager;
import android.app.Application;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.ActivityNotFoundException;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.content.pm.ResolveInfo;
import android.os.Bundle;
import android.os.Handler;
import android.support.v4.util.LruCache;
import android.view.View;

import com.microsoft.azure.engagement.EngagementActivityManager;
import com.microsoft.azure.engagement.EngagementAgent;
import com.microsoft.azure.engagement.EngagementAgent.Callback;
import com.microsoft.azure.engagement.reach.v11.EngagementNotificationUtilsV11;
import com.microsoft.azure.engagement.storage.EngagementStorage;
import com.microsoft.azure.engagement.storage.EngagementStorage.Scanner;

/**
 * This is the class that manages the Reach functionalities. It listen messages thanks to
 * {@link EngagementReachReceiver} and notify the user about contents. You usually don't need to
 * access this class directly, you rather integrate the {@link EngagementReachReceiver} broadcast
 * receiver in your AndroidManifest.xml file.<br/>
 * @see EngagementReachReceiver
 */
public class EngagementReachAgent
{
  /** Intent prefix */
  private static final String INTENT_PREFIX = "com.microsoft.azure.engagement.reach.intent.";

  /** Intent action prefix */
  private static final String INTENT_ACTION_PREFIX = INTENT_PREFIX + "action.";

  /** Intent extra prefix */
  private static final String INTENT_EXTRA_PREFIX = INTENT_PREFIX + "extra.";

  /** Intent action used when a reach notification has been actioned e.g. clicked */
  public static final String INTENT_ACTION_ACTION_NOTIFICATION = INTENT_ACTION_PREFIX
    + "ACTION_NOTIFICATION";

  /**
   * Intent action used when a reach notification has been exited (clear button on notification
   * panel).
   */
  public static final String INTENT_ACTION_EXIT_NOTIFICATION = INTENT_ACTION_PREFIX
    + "EXIT_NOTIFICATION";

  /** Intent action used to react to a download timeout */
  public static final String INTENT_ACTION_DOWNLOAD_TIMEOUT = INTENT_ACTION_PREFIX
    + "DOWNLOAD_TIMEOUT";

  /** Intent action used to launch loading screen. */
  public static final String INTENT_ACTION_LOADING = INTENT_ACTION_PREFIX + "LOADING";

  /** Used a long extra field in notification and view intents, containing the content identifier */
  public static final String INTENT_EXTRA_CONTENT_ID = INTENT_EXTRA_PREFIX + "CONTENT_ID";

  /**
   * Used an int extra field in notification intents, containing the system notification identifier
   * (to be able to explicitly remove the notification).
   */
  public static final String INTENT_EXTRA_NOTIFICATION_ID = INTENT_EXTRA_PREFIX + "NOTIFICATION_ID";

  /**
   * Used as an extra field in activity launch intent (see
   * {@link #INTENT_ACTION_ACTION_NOTIFICATION} action) to represent the component that will display
   * the content (an activity).
   */
  public static final String INTENT_EXTRA_COMPONENT = INTENT_EXTRA_PREFIX + "COMPONENT";

  /** Undefined intent result (used for datapush) */
  private static final int RESULT_UNDEFINED = -2;

  /** Reach XML namespace */
  static final String REACH_NAMESPACE = "urn:ubikod:ermin:reach:0";

  /** Download meta-data store */
  private static final String DOWNLOAD_SETTINGS = "engagement.reach.downloads";

  /** Unique instance */
  private static EngagementReachAgent sInstance;

  /** Activity manager */
  private static final EngagementActivityManager sActivityManager = EngagementActivityManager.getInstance();

  /** Context used for binding to the Engagement service and other Android API calls */
  private final Context mContext;

  /** Last time application was updated */
  private final long mAppLastUpdateTime;

  /** Notification handlers by category, a default one is set at init time */
  private final Map<String, EngagementNotifier> mNotifiers = new HashMap<String, EngagementNotifier>();

  /** Storage for contents */
  private final EngagementStorage mDB;

  /** List of parameters to inject in announcement's action URL and body */
  private final Map<String, String> mInjectedParams = new HashMap<String, String>();

  /** States */
  private enum State
  {
    /** When we are waiting for new content */
    IDLE,

    /** A content is being notified in-app */
    NOTIFYING_IN_APP,

    /** A content is being loaded */
    LOADING,

    /** A content is being shown */
    SHOWING
  }

  /** Current state */
  private State mState = State.IDLE;

  /** True if in the process of scanning */
  private boolean mScanning;

  /**
   * The current content (identifier) being shown (in a viewing activity), set when mState ==
   * State.SHOWING.
   */
  private Long mCurrentShownContentId;

  /** The component elected to show the content */
  private ComponentName mShowingActivity;

  /**
   * Notifications (content identifiers) that are pending (for example because of a background
   * download). Used to avoid processing them again at each activity change.
   */
  private final Set<Long> mPendingNotifications = new HashSet<Long>();

  /**
   * Content LRU RAM cache, generally contains {@link #mCurrentShownContent} and the ones in
   * {@link #mPendingNotifications}.
   */
  private final LruCache<Long, EngagementReachContent> mContentCache = new LruCache<Long, EngagementReachContent>(
    10);

  /** DLC we already requested to service during this process lifetime (key = localId) */
  private final Set<Long> mPendingDLCs = new HashSet<Long>();

  /** Last activity weak reference that the agent is aware of */
  private WeakReference<Activity> mLastActivity = new WeakReference<Activity>(null);

  /**
   * Activity listener, when current activity changes we try to show a content notification from
   * local database.
   */
  private final EngagementActivityManager.Listener mActivityListener = new EngagementActivityManager.Listener()
  {
    @Override
    public void onCurrentActivityChanged(WeakReference<Activity> currentActivity,
      String engagementAlias)
    {
      /* Hide notifications when entering new activity (it may contain areas embedded in the layout) */
      Activity activity = currentActivity.get();
      Activity lastActivity = mLastActivity.get();
      if (activity != null && !activity.equals(lastActivity))
        hideInAppNotifications(activity);

      /* If we were notifying in activity and exit that one */
      if (mState == State.NOTIFYING_IN_APP && lastActivity != null
        && !lastActivity.equals(activity))
      {
        /* Hide notifications */
        hideInAppNotifications(lastActivity);

        /* We are now idle */
        setIdle();
      }

      /* Update last activity (if entering a new one) */
      mLastActivity = currentActivity;

      /*
       * Guard against application being put to background in showing state but not yet inside
       * showing activity, make sure the state machine is not stuck in showing. To produce, click on
       * notification, then quickly press HOME in loading activity while showing code already
       * triggered...
       */
      if (mState == State.SHOWING && activity != null
        && !activity.getComponentName().equals(mShowingActivity))
      {
        /* Check if content not yet processed */
        EngagementReachInteractiveContent content = getContent(mCurrentShownContentId);
        if (content != null)
        {
          /* In case of a system notification, there is no way to go back to content */
          if (content.isSystemNotification())
          {
            /* So make sure display/exit feedbacks are sent */
            content.displayContent(mContext);
            content.exitContent(mContext);

            /* Return now to avoid extra scan */
            return;
          }

          /* Otherwise just set idle */
          else
            setIdle();
        }
      }

      /* If we are idle, pick a content */
      if (mState == State.IDLE)
        scanContent(false);
    }

    /**
     * Hide all possible overlays and notification areas in the specified activity.
     * @param activity activity to operate on.
     */
    private void hideInAppNotifications(Activity activity)
    {
      /* For all categories */
      for (Map.Entry<String, EngagementNotifier> entry : mNotifiers.entrySet())
      {
        /* Hide overlays */
        String category = entry.getKey();
        EngagementNotifier notifier = entry.getValue();
        Integer overlayId = notifier.getOverlayViewId(category);
        if (overlayId != null)
        {
          View overlayView = activity.findViewById(overlayId);
          if (overlayView != null)
            overlayView.setVisibility(View.GONE);
        }

        /* Hide areas */
        Integer areaId = notifier.getInAppAreaId(category);
        if (areaId != null)
        {
          View areaView = activity.findViewById(areaId);
          if (areaView != null)
            areaView.setVisibility(View.GONE);
        }
      }
    }
  };

  /** Datapush campaigns being broadcasted */
  private final Set<Long> mPendingDataPushes = new HashSet<Long>();

  /**
   * Init the reach agent.
   * @param context application context.
   */
  private EngagementReachAgent(Context context)
  {
    /* Keep application context */
    mContext = context;

    /* Get app last update time */
    long appLastUpdateTime;
    try
    {
      appLastUpdateTime = context.getPackageManager().getPackageInfo(context.getPackageName(), 0).lastUpdateTime;
    }
    catch (Exception e)
    {
      /* If package manager crashed, assume no upgrade */
      appLastUpdateTime = 0;
    }
    mAppLastUpdateTime = appLastUpdateTime;

    /* Install default category notifier, can be overridden by user */
    mNotifiers.put(CATEGORY_DEFAULT, new EngagementDefaultNotifier(context));

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
    EngagementAgent.getInstance(context).getDeviceId(new Callback<String>()
    {
      @Override
      public void onResult(String deviceId)
      {
        /* Update parameters */
        mInjectedParams.put("{deviceid}", deviceId);

        /*
         * Watch current activity, if we still have not exited the constructor we have to delay the
         * call so that singleton is set. It can happen in the unlikely scenario where getDeviceId
         * returns synchronously the result.
         */
        if (sInstance != null)
          sActivityManager.addCurrentActivityListener(mActivityListener);
        else
          new Handler().post(new Runnable()
          {
            @Override
            public void run()
            {
              sActivityManager.addCurrentActivityListener(mActivityListener);
            }
          });
      }
    });
  }

  /**
   * Get the unique instance.
   * @param context any valid context
   */
  public static EngagementReachAgent getInstance(Context context)
  {
    /* Always check this even if we instantiate once to trigger null pointer in all cases */
    if (sInstance == null)
      sInstance = new EngagementReachAgent(context.getApplicationContext());
    return sInstance;
  }

  /**
   * Register a custom notifier for a set of content categories. You have to call this method in
   * {@link Application#onCreate()} because notifications can happen at any time.
   * @param notifier notifier to register for a set of categories.
   * @param categories one or more category.
   */
  public void registerNotifier(EngagementNotifier notifier, String... categories)
  {
    for (String category : categories)
      mNotifiers.put(category, notifier);
  }

  /**
   * Get content by its local identifier.
   * @param localId the content local identifier.
   * @return the content if found, null otherwise.
   */
  @SuppressWarnings("unchecked")
  public <T extends EngagementReachContent> T getContent(long localId)
  {
    /* Return content from cache if possible */
    EngagementReachContent cachedContent = mContentCache.get(localId);
    if (cachedContent != null)
      try
      {
        return (T) cachedContent;
      }
      catch (ClassCastException cce)
      {
        /* Invalid type */
        return null;
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
        try
        {
          return (T) parseContentFromStorage(values);
        }
        catch (ClassCastException cce)
        {
          /* Invalid type */
        }
        catch (Exception e)
        {
          /*
           * Delete content that cannot be parsed, may be corrupted data, we cannot send "dropped"
           * feedback as we need the Reach contentId and kind.
           */
          deleteContent(localId, values.getAsLong(DOWNLOAD_ID));
        }

      /* Not found, invalid type or an error occurred */
      return null;
    }
  }

  /**
   * Get content by its intent (containing the content local identifier such as in intents
   * associated with the {@link #INTENT_ACTION_ACTION_NOTIFICATION} action).
   * @param intent intent containing the local identifier under the
   *          {@value #INTENT_EXTRA_CONTENT_ID} extra key (as a long).
   * @return the content if found, null otherwise.
   */
  public <T extends EngagementReachContent> T getContent(Intent intent)
  {
    return getContent(intent.getLongExtra(INTENT_EXTRA_CONTENT_ID, 0));
  }

  /**
   * Get content by a download identifier.
   * @param downloadId intent containing the local identifier under the
   *          {@value #INTENT_EXTRA_CONTENT_ID} extra key (as a long).
   * @return the content if found, null otherwise.
   */
  public <T extends EngagementReachContent> T getContentByDownloadId(long downloadId)
  {
    return getContent(mContext.getSharedPreferences(DOWNLOAD_SETTINGS, 0).getLong(
      String.valueOf(downloadId), 0));
  }

  /**
   * If for some reason you accepted a content in
   * {@link EngagementNotifier#handleNotification(EngagementReachInteractiveContent)} but returned
   * null to tell that the notification was not ready to be displayed, call this function once the
   * notification is ready. For example this is used once the big picture of a system notification
   * has been downloaded (or failed to be downloaded). If the content has not been shown or dropped,
   * this will trigger a new call to
   * {@link EngagementNotifier#handleNotification(EngagementReachInteractiveContent)} if the current
   * U.I context allows so (activity/session/any time filters are evaluated again).
   * @param content content to notify.
   */
  public void notifyPendingContent(EngagementReachInteractiveContent content)
  {
    /* Notification is not managed anymore can be submitted to notifiers again */
    long localId = content.getLocalId();
    mPendingNotifications.remove(localId);

    /* Update notification if not too late e.g. notification not yet dismissed */
    if ((mState != State.LOADING && mState != State.SHOWING) || mCurrentShownContentId != localId)
      try
      {
        notifyContent(content, false);
      }
      catch (RuntimeException e)
      {
        content.dropContent(mContext);
      }
  }

  /**
   * Report loading screen has been exited.
   * @param contentId content associated to loading screen.
   */
  public void exitLoading(long contentId)
  {
    /*
     * Make sure we are not already in showing state: loading will be exiting by starting the real
     * activity. Check also if a loading screen replaces another loading screen, in that case stay
     * in loading state. If we are sure we exit a loading screen to return to application or
     * background, then we can set back to idle state.
     */
    if (isLoading(contentId))
      setIdle();
  }

  /**
   * Check if the agent is loading a particular content.
   * @param contentId content id to test.
   * @return true iff agent is loading that content.
   */
  public boolean isLoading(long contentId)
  {
    return mState == State.LOADING && contentId == mCurrentShownContentId;
  }

  /**
   * Called when a new content is received.
   * @param payload native push payload.
   */
  void onContentReceived(Bundle payload)
  {
    /* Parse campaign id before everything else for early feedbacks */
    String ci = payload.getString(CAMPAIGN_ID);
    CampaignId campaignId;
    try
    {
      campaignId = new CampaignId(ci);
    }
    catch (Exception e)
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
    values.put(DELIVERY_TIME, payload.containsKey(DELIVERY_TIME) ? payload.getString(DELIVERY_TIME)
      : "a");
    values.put(TTL, parseLong(payload, TTL));
    values.put(USER_TIME_ZONE, parseInt(payload, USER_TIME_ZONE, 0));
    values.put(NOTIFICATION_TYPE,
      payload.containsKey(NOTIFICATION_TYPE) ? payload.getString(NOTIFICATION_TYPE) : "p");
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
    catch (Exception e)
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
      long localId = mDB.put(values);
      content.setLocalId(localId);

      /*
       * If we don't know device id yet, keep it for later. If we are idle, check if the content
       * notification can be shown during the current U.I context. Datapush can be "notified" even
       * when not idle.
       */
      if (mInjectedParams.containsKey("{deviceid}"))
        notifyContent(content, false);
    }
    catch (Exception e)
    {
      /* Drop content on error */
      content.dropContent(mContext);
    }
  }

  /**
   * GCM & ADM push payload values are always strings unfortunately. We have to parse numbers.
   * @param bundle bundle.
   * @param key key to extract
   * @param defaultValue default value if key is missing or not an int.
   * @return string value parsed as an int.
   */
  private static int parseInt(Bundle bundle, String key, int defaultValue)
  {
    try
    {
      return Integer.parseInt(bundle.getString(key));
    }
    catch (RuntimeException e)
    {
      return defaultValue;
    }
  }

  /**
   * GCM & ADM push payload values are always strings unfortunately. We have to parse numbers.
   * @param bundle bundle.
   * @param key key to extract
   * @return string value parsed as a Long. If key is missing or not a long, returns null.
   */
  private static Long parseLong(Bundle bundle, String key)
  {
    try
    {
      return Long.parseLong(bundle.getString(key));
    }
    catch (RuntimeException e)
    {
      return null;
    }
  }

  /**
   * Called when a message download completes.
   * @param message message parameters.
   */
  void onMessageDownloaded(Bundle message)
  {
    /*
     * Get all campaigns from storage that matches this DLC identifier (manual push can have several
     * pushes for same DLC id).
     */
    String id = message.getString(INTENT_EXTRA_ID);
    if (id == null)
      return;
    Scanner scanner = mDB.getScanner(DLC_ID, id);
    for (ContentValues values : scanner)
    {
      /* Parse it */
      EngagementReachContent content = null;
      try
      {
        /* Parse and restore state */
        content = parseContentFromStorage(values);
        long localId = content.getLocalId();
        mPendingDLCs.remove(localId);
        boolean mustNotify = !content.isDlcCompleted()
          && (content.hasNotificationDLC() || content instanceof EngagementDataPush);

        /* Parse downloaded payload as JSON */
        String rawPayload = message.getString(INTENT_EXTRA_PAYLOAD);
        JSONObject payload = new JSONObject(rawPayload);
        content.setPayload(payload);

        /* Store it */
        ContentValues update = new ContentValues();
        update.put(PAYLOAD, rawPayload);
        mDB.update(localId, update);

        /* Cache is out of date, update it */
        mContentCache.put(localId, content);

        /* Start content if we were loading it */
        if (mState == State.LOADING && localId == mCurrentShownContentId)
          showContent(content);

        /* Notify if we were waiting for DLC to generate notification */
        else if (mustNotify && mInjectedParams.containsKey("{deviceid}"))
          notifyContent(content, false);
      }
      catch (Exception e)
      {
        /*
         * Delete content and send dropped feedback if possible depending on how much state we could
         * restore.
         */
        if (content == null)
        {
          Long oid = values.getAsLong(OID);
          if (oid != null)
            delete(oid);
        }
        else
          content.dropContent(mContext);
      }
    }
    scanner.close();
  }

  /**
   * Called when a download for a content has been scheduled.
   * @param content content.
   * @param downloadId download identifier.
   */
  void onDownloadScheduled(EngagementReachContent content, long downloadId)
  {
    /* Save download identifier */
    ContentValues values = new ContentValues();
    values.put(DOWNLOAD_ID, downloadId);
    mDB.update(content.getLocalId(), values);
    mContext.getSharedPreferences(DOWNLOAD_SETTINGS, 0)
      .edit()
      .putLong(String.valueOf(downloadId), content.getLocalId())
      .commit();
  }

  /**
   * Called when download has been completed.
   * @param content content.
   */
  void onDownloadComplete(EngagementReachInteractiveContent content)
  {
    /* Cancel alarm */
    Intent intent = new Intent(INTENT_ACTION_DOWNLOAD_TIMEOUT);
    intent.setPackage(mContext.getPackageName());
    int requestCode = (int) content.getLocalId();
    PendingIntent operation = PendingIntent.getBroadcast(mContext, requestCode, intent, 0);
    AlarmManager alarmManager = (AlarmManager) mContext.getSystemService(Context.ALARM_SERVICE);
    alarmManager.cancel(operation);

    /* Update notification if not too late e.g. notification not yet dismissed */
    notifyPendingContent(content);
  }

  /**
   * Called when a download takes too much time for a content.
   * @param content content.
   */
  void onDownloadTimeout(EngagementReachInteractiveContent content)
  {
    /* Notify without downloaded data */
    notifyPendingContent(content);
  }

  /**
   * Called when a notification is reported as displayed.
   * @param content displayed content's notification.
   */
  void onNotificationDisplayed(EngagementReachInteractiveContent content)
  {
    ContentValues values = new ContentValues();
    values.put(NOTIFICATION_FIRST_DISPLAYED_DATE, content.getNotificationFirstDisplayedDate());
    values.put(NOTIFICATION_LAST_DISPLAYED_DATE, content.getNotificationLastDisplayedDate());
    mDB.update(content.getLocalId(), values);
  }

  /**
   * Called when a notification is actioned.
   * @param content content associated to the notification.
   * @param launchIntent true to launch intent.
   */
  void onNotificationActioned(EngagementReachInteractiveContent content, boolean launchIntent)
  {
    /* Intents can fail */
    try
    {
      /* Persist content state */
      updateContentStatusTrue(content, NOTIFICATION_ACTIONED);

      /* Nothing more to do if intent must not be launched */
      long localId = content.getLocalId();
      if (!launchIntent)
      {
        /* Assume shown */
        mState = State.SHOWING;
        mCurrentShownContentId = localId;
        return;
      }

      /* Notification announcement */
      if (content instanceof EngagementNotifAnnouncement)
      {
        /* Execute action and cancel notification if system */
        EngagementNotifAnnouncement announcement = (EngagementNotifAnnouncement) content;
        getNotifier(content).executeNotifAnnouncementAction(announcement);
        cancelSystemNotification(announcement);
      }

      /* If we have the DLC start a content activity in its own task */
      else if (checkRequestDlc(content))
        showContent(content);

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
    catch (RuntimeException e)
    {
      /* Drop content on any error */
      content.dropContent(mContext);
    }
  }

  /**
   * Called when a content is reported as displayed.
   * @param content displayed content.
   */
  void onContentDisplayed(EngagementReachInteractiveContent content)
  {
    /* Make sure we update state in DB */
    updateContentStatusTrue(content, CONTENT_DISPLAYED);

    /* Cancel notification */
    cancelSystemNotification(content);
  }

  /**
   * When a content is processed, we can remove it from SQLite. We can also check if a new one can
   * be shown.
   */
  void onContentProcessed(EngagementReachContent content)
  {
    /* Delete content */
    deleteContent(content);

    /* If loading update loading window to display error message */
    if (mState == State.LOADING)
    {
      /* Only if not replaced by another loading window */
      if (content.getLocalId() == mCurrentShownContentId)
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
      if (!content.isSystemNotification() && !(content instanceof EngagementDataPush))
        scanContent(false);
    }
  }

  /** Called when the device has rebooted. */
  void onDeviceBoot()
  {
    /* Replay system notifications */
    scanContent(true);
  }

  /**
   * Get notifier for a content depending on its category.
   * @param content content to notify.
   * @return notifier for a content depending on its category.
   */
  private EngagementNotifier getNotifier(EngagementReachContent content)
  {
    /* Delegate to notifiers, select the right one for the current category */
    EngagementNotifier notifier = mNotifiers.get(content.getCategory());

    /* Fail over default category if not found */
    if (notifier == null)
      notifier = mNotifiers.get(CATEGORY_DEFAULT);
    return notifier;
  }

  /**
   * Parse a content.
   * @param campaignId already parsed campaign id.
   * @param values content values.
   * @return content.
   * @throws Exception parsing problem.
   */
  private EngagementReachContent parseContent(CampaignId campaignId, ContentValues values)
    throws Exception
  {
    switch (campaignId.getKind())
    {
      case ANNOUNCEMENT:
        if ((values.getAsInteger(DLC) & FLAG_DLC_CONTENT) == FLAG_DLC_CONTENT)
          return new EngagementAnnouncement(campaignId, values, mInjectedParams);
        return new EngagementNotifAnnouncement(campaignId, values, mInjectedParams);

      case POLL:
        return new EngagementPoll(campaignId, values);

      case DATAPUSH:
        return new EngagementDataPush(campaignId, values, mInjectedParams);

      default:
        throw new IllegalArgumentException("Invalid campaign id");
    }
  }

  /**
   * Parse a content retrieved from storage.
   * @param values content as returned by the storage.
   * @return content.
   * @throws Exception parsing problem, most likely invalid XML.
   */
  private EngagementReachContent parseContentFromStorage(ContentValues values) throws Exception
  {
    /* Parse payload */
    CampaignId campaignId = new CampaignId(values.getAsString(CAMPAIGN_ID));
    EngagementReachContent content = parseContent(campaignId, values);

    /* Parse state */
    content.setState(values);

    /* Parse local id */
    content.setLocalId(values.getAsLong(OID));
    return content;
  }

  /** Cancel system notification */
  private void cancelSystemNotification(EngagementReachInteractiveContent content)
  {
    if (content.isSystemNotification())
    {
      NotificationManager notificationManager = (NotificationManager) mContext.getSystemService(NOTIFICATION_SERVICE);
      notificationManager.cancel(getNotifier(content).getNotificationId(content));
    }
  }

  /**
   * Update a content's status.
   * @param content content to update.
   * @param status status to set to true.
   */
  private void updateContentStatusTrue(EngagementReachContent content, String status)
  {
    ContentValues values = new ContentValues();
    values.put(status, 1);
    mDB.update(content.getLocalId(), values);
  }

  /**
   * Show or refresh loading activity.
   * @param content campaign being loaded.
   * @param flags additional activity flags.
   */
  private void showOrRefreshLoading(EngagementReachContent content, int... flags)
  {
    Intent intent = new Intent(INTENT_ACTION_LOADING);
    String category = content.getCategory();
    if (category != null)
      intent.addCategory(category);
    filterIntentWithCategory(intent);
    setContentIdExtra(intent, content);
    intent.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_EXCLUDE_FROM_RECENTS);
    for (int flag : flags)
      intent.addFlags(flag);
    mContext.startActivity(intent);
  }

  /** Show content activity */
  private void showContent(EngagementReachContent content)
  {
    /* Update state */
    mState = State.SHOWING;
    mCurrentShownContentId = content.getLocalId();

    /* Start activity */
    Intent intent = content.getIntent();
    filterIntentWithCategory(intent);
    intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_NO_HISTORY);
    mShowingActivity = intent.getComponent();
    mContext.startActivity(intent);
  }

  /**
   * Scan reach database and notify the first content that match the current U.I context
   * @param replaySystemNotifications true iff system notifications must be replayed.
   */
  private void scanContent(boolean replaySystemNotifications)
  {
    /* Change state */
    mScanning = true;

    /* For all database rows */
    Scanner scanner = mDB.getScanner();
    for (ContentValues values : scanner)
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
      catch (Exception e)
      {
        /*
         * If the content was parsed but an error occurred while notifying, send "dropped" feedback
         * and delete
         */
        if (content != null)
          content.dropContent(mContext);

        /* Otherwise we just delete */
        else
          deleteContent(values.getAsLong(OID), values.getAsLong(DOWNLOAD_ID));

        /* In any case we continue parsing */
      }
    }

    /* Close scanner */
    scanner.close();

    /* Scan finished */
    mScanning = false;
  }

  /**
   * Fill an intent with a content identifier as extra.
   * @param intent intent.
   * @param content content.
   */
  static void setContentIdExtra(Intent intent, EngagementReachContent content)
  {
    intent.putExtra(INTENT_EXTRA_CONTENT_ID, content.getLocalId());
  }

  /**
   * Try to notify the content to the user.
   * @param content reach content.
   * @param replaySystemNotifications true iff system notifications must be replayed.
   * @throws RuntimeException if an error occurs.
   */
  private void notifyContent(final EngagementReachContent content, boolean replaySystemNotifications)
    throws RuntimeException
  {
    /* Check expiry */
    final long localId = content.getLocalId();
    if (content.hasExpired())
    {
      /* Delete */
      deleteContent(content);
      return;
    }

    /* If datapush, just broadcast, can be done in parallel with another content */
    final Intent intent = content.getIntent();
    if (content instanceof EngagementDataPush)
    {
      /* Delay data push until DLC completes */
      if (!checkRequestDlc(content))
        return;

      /* If it's a datapush it may already be in the process of broadcasting. */
      if (!mPendingDataPushes.add(localId))
        return;

      /* Broadcast intent */
      final EngagementDataPush dataPush = (EngagementDataPush) content;
      intent.setPackage(mContext.getPackageName());
      mContext.sendOrderedBroadcast(intent, null, new BroadcastReceiver()
      {
        @Override
        public void onReceive(Context context, Intent intent)
        {
          /* The last broadcast receiver to set a defined result wins (to determine which result). */
          switch (getResultCode())
          {
            case RESULT_OK:
              dataPush.actionContent(context);
              break;

            case RESULT_CANCELED:
              dataPush.exitContent(context);
              break;

            default:
              dataPush.dropContent(context);
          }

          /* Clean broadcast state */
          mPendingDataPushes.remove(localId);
        }
      }, null, RESULT_UNDEFINED, null, null);

      /* Datapush processed */
      return;
    }

    /* If notification has pending downloadable content, delay */
    if (content.hasNotificationDLC() && !checkRequestDlc(content))
      return;

    /* Don't notify in-app if we are already notifying in app or showing a content */
    if (mState != State.IDLE && !content.isSystemNotification())
      return;

    /* Don't process again a pending notification */
    if (mPendingNotifications.contains(localId))
      return;

    /* Not an interactive content, exit (but there is no other type left, this is just a cast guard) */
    if (!(content instanceof EngagementReachInteractiveContent))
      return;
    EngagementReachInteractiveContent iContent = (EngagementReachInteractiveContent) content;

    /* Don't replay system notification unless told otherwise. */
    if (!replaySystemNotifications && iContent.isSystemNotification()
      && iContent.getNotificationLastDisplayedDate() != null
      && iContent.getNotificationLastDisplayedDate() > mAppLastUpdateTime)
      return;

    /* Check if the content can be notified in the current context (behavior) */
    if (!iContent.canNotify(sActivityManager.getCurrentActivityAlias()))
      return;

    /* If there is a show intent */
    if (intent != null)
      filterIntentWithCategory(intent);

    /* Delegate notification */
    Boolean notifierResult = getNotifier(content).handleNotification(iContent);

    /* Check if notifier rejected content notification for now */
    if (Boolean.FALSE.equals(notifierResult))

      /* The notifier rejected the content, nothing more to do */
      return;

    /* Cache content if accepted, it will most likely be used again soon for the next steps. */
    mContentCache.put(localId, content);

    /*
     * If notifier did not return null (e.g. returned true, meaning actually accepted the content),
     * we assume the notification is correctly displayed.
     */
    if (Boolean.TRUE.equals(notifierResult))
    {
      /* Report displayed feedback */
      iContent.displayNotification(mContext);

      /* Track in-app content life cycle: one at a time */
      if (!iContent.isSystemNotification())
        mState = State.NOTIFYING_IN_APP;
    }

    /* Track pending notifications to avoid re-processing them every time we change activity. */
    if (notifierResult == null)
      mPendingNotifications.add(localId);
  }

  /**
   * Check DLC state and triggers download if needed.
   * @param content content.
   * @return true if DLC is not needed or is ready.
   */
  private boolean checkRequestDlc(final EngagementReachContent content)
  {
    if (content.hasDLC())
    {
      boolean dlcCompleted = content.isDlcCompleted();
      if (!dlcCompleted && mPendingDLCs.add(content.getLocalId()))
        EngagementAgent.getInstance(mContext).getMessage(content.getDlcId());
      return dlcCompleted;
    }
    return true;
  }

  /** Set idle, that means we are ready for a next content to be notified in-app. */
  private void setIdle()
  {
    mState = State.IDLE;
    mCurrentShownContentId = null;
  }

  /**
   * Filter the intent to a single activity so a chooser won't pop up. Do not handle fall back here.
   * @param intent intent to filter.
   */
  private void filterIntent(Intent intent)
  {
    for (ResolveInfo resolveInfo : mContext.getPackageManager().queryIntentActivities(intent, 0))
    {
      ActivityInfo activityInfo = resolveInfo.activityInfo;
      String packageName = mContext.getPackageName();
      if (activityInfo.packageName.equals(packageName))
      {
        intent.setComponent(new ComponentName(packageName, activityInfo.name));
        break;
      }
    }
  }

  /**
   * Filter the intent to a single activity so a chooser won't pop up. If not found, it tries to
   * resolve intent by falling back to default category.
   * @param intent intent to filter.
   */
  private void filterIntentWithCategory(final Intent intent)
  {
    /* Filter intent for the target package name */
    filterIntent(intent);

    /* If the intent could not be resolved */
    if (intent.getComponent() == null)
    {
      /* If there was no category */
      if (intent.getCategories() == null)

        /* Notification cannot be done */
        throw new ActivityNotFoundException();

      /* Remove categories */
      Collection<String> categories = new HashSet<String>(intent.getCategories());
      for (String category : categories)
        intent.removeCategory(category);

      /* Try filtering again */
      filterIntent(intent);

      /* Notification cannot be done, skip content */
      if (intent.getComponent() == null)
        throw new ActivityNotFoundException();
    }
  }

  /**
   * Delete content from storage and any associated download or notification.
   * @param content content to delete.
   */
  private void deleteContent(EngagementReachContent content)
  {
    if (content instanceof EngagementReachInteractiveContent)
      cancelSystemNotification((EngagementReachInteractiveContent) content);
    deleteContent(content.getLocalId(), content.getDownloadId());
  }

  /**
   * Delete content from storage and any associated download.
   * @param localId content identifier to delete.
   * @param downloadId download identifier to delete if any.
   */
  private void deleteContent(long localId, Long downloadId)
  {
    /* Delete all references */
    delete(localId);

    /* Delete associated download if any */
    if (downloadId != null)
    {
      /* Delete mapping */
      mContext.getSharedPreferences(DOWNLOAD_SETTINGS, 0)
        .edit()
        .remove(String.valueOf(downloadId))
        .commit();

      /* Cancel download and delete file */
      EngagementNotificationUtilsV11.deleteDownload(mContext, downloadId);
    }
  }

  /** Delete content */
  private void delete(long localId)
  {
    mDB.delete(localId);
    mPendingNotifications.remove(localId);
    mContentCache.remove(localId);
  }
}
