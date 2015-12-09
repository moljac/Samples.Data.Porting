/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import static com.microsoft.azure.engagement.reach.ContentStorage.CONTENT_DISPLAYED;
import static com.microsoft.azure.engagement.reach.ContentStorage.DELIVERY_TIME;
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

import java.util.HashSet;
import java.util.Set;

import org.json.JSONArray;
import org.json.JSONException;

import android.content.ContentValues;
import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.util.Base64;

/** Common class to announcements and polls */
public abstract class EngagementReachInteractiveContent extends EngagementReachContent
{
  /** Title */
  private String mTitle;

  /** Action's label */
  private String mActionLabel;

  /** Exit label */
  private String mExitLabel;

  /** True if this content has a notification that must be set in status bar */
  private final boolean mSystemNotification;

  /** Does the notification have a resource icon in notification content? */
  private final boolean mNotificationIcon;

  /** Can the notification be closed ? */
  private final boolean mNotiticationCloseable;

  /** Make the telephone ring ? */
  private final boolean mNotificationSound;

  /** Make the telephone vibrate ? */
  private final boolean mNotificationVibrate;

  /** Notification's title */
  private final String mNotificationTitle;

  /** Notification's message */
  private final String mNotificationMessage;

  /** Notification's big text */
  private final String mNotificationBigText;

  /** Notification's big picture */
  private final String mNotificationBigPicture;

  /** Notification image base64 string */
  private String mNotificationImageString;

  /** Notification image bitmap null until requested */
  private Bitmap mNotificationImage;

  /** Behavior types */
  private enum Behavior
  {
    ANYTIME,
    SESSION;
  }

  /** Behavior */
  private final Behavior mBehavior;

  /** Activities to restrict this content */
  private Set<String> mAllowedActivities;

  /** True if notification actioned */
  private boolean mNotificationActioned;

  /** Get date at which the notification was displayed the first time */
  private Long mNotificationFirstDisplayedDate;

  /** Get date at which the notification was displayed the last time */
  private Long mNotificationLastDisplayedDate;

  /** True if content displayed */
  private boolean mContentDisplayed;

  EngagementReachInteractiveContent(CampaignId campaignId, ContentValues values)
    throws JSONException
  {
    /* Base fields */
    super(campaignId, values);

    /* Behavior */
    String deliveryTime = values.getAsString(DELIVERY_TIME);
    if (deliveryTime.equals("s"))
      mBehavior = Behavior.SESSION;
    else
      mBehavior = Behavior.ANYTIME;

    /* Notification type */
    mSystemNotification = "s".equals(values.getAsString(NOTIFICATION_TYPE));

    /* Is notification closeable? */
    mNotiticationCloseable = parseBoolean(values, NOTIFICATION_CLOSEABLE);

    /* Has notification icon? */
    mNotificationIcon = parseBoolean(values, NOTIFICATION_ICON);

    /* Sound and vibration */
    mNotificationSound = parseBoolean(values, NOTIFICATION_SOUND);
    mNotificationVibrate = parseBoolean(values, NOTIFICATION_VIBRATION);

    /* Parse texts */
    mNotificationTitle = values.getAsString(NOTIFICATION_TITLE);
    mNotificationMessage = values.getAsString(NOTIFICATION_MESSAGE);

    /* Big text */
    mNotificationBigText = values.getAsString(NOTIFICATION_BIG_TEXT);

    /* Big picture */
    mNotificationBigPicture = values.getAsString(NOTIFICATION_BIG_PICTURE);
  }

  @Override
  void setPayload(org.json.JSONObject payload) throws JSONException
  {
    /* Get parent fields */
    super.setPayload(payload);

    /* Get image data, we decode the bitmap in a lazy way */
    mNotificationImageString = payload.optString("notificationImage", null);

    /* Delivery activities */
    JSONArray deliveryActivities = payload.optJSONArray("deliveryActivities");
    if (deliveryActivities != null)
    {
      mAllowedActivities = new HashSet<String>();
      for (int i = 0; i < deliveryActivities.length(); i++)
        mAllowedActivities.add(deliveryActivities.getString(i));
    }

    /* Get body related fields */
    mTitle = payload.optString("title", null);
    mActionLabel = payload.optString("actionButtonText", null);
    mExitLabel = payload.optString("exitButtonText", null);
  }

  /**
   * Get content's title.
   * @return content's title
   */
  public String getTitle()
  {
    return mTitle;
  }

  /**
   * Get action's label.
   * @return action's label.
   */
  public String getActionLabel()
  {
    return mActionLabel;
  }

  /**
   * Get exit label.
   * @return exit label.
   */
  public String getExitLabel()
  {
    return mExitLabel;
  }

  /**
   * Check whether this content has a system notification.
   * @return true if this content has to be notified in the status bar, false to embed the
   *         notification in activity.
   */
  @Override
  public boolean isSystemNotification()
  {
    return mSystemNotification;
  }

  /**
   * Check whether the notification can be closed without looking at the content.
   * @return true if the notification can be closed without looking at the content, false otherwise.
   */
  public boolean isNotificationCloseable()
  {
    return mNotiticationCloseable;
  }

  /**
   * Check whether the notification has a resource icon in notification content.
   * @return true if the notification has a resource icon in notification content, false otherwise.
   */
  public boolean hasNotificationIcon()
  {
    return mNotificationIcon;
  }

  /**
   * Check whether the notification makes the telephone ring.
   * @return true iff the notification makes the telephone ring.
   */
  public boolean isNotificationSound()
  {
    return mNotificationSound;
  }

  /**
   * Check whether the notification makes the telephone vibrate.
   * @return true iff the notification makes the telephone vibrate.
   */
  public boolean isNotificationVibrate()
  {
    return mNotificationVibrate;
  }

  /**
   * Get notification's title.
   * @return notification's title
   */
  public String getNotificationTitle()
  {
    return mNotificationTitle;
  }

  /**
   * Get notification's message.
   * @return notification's message.
   */
  public String getNotificationMessage()
  {
    return mNotificationMessage;
  }

  /**
   * Get notification big text message (displayed only on Android 4.1+).
   * @return notification's big text message.
   */
  public String getNotificationBigText()
  {
    return mNotificationBigText;
  }

  /**
   * Get notification big picture URL (displayed only on Android 4.1+).
   * @return notification big picture URL.
   */
  public String getNotificationBigPicture()
  {
    return mNotificationBigPicture;
  }

  /**
   * Get notification image for in app notifications. For system notification this field corresponds
   * to the large icon (displayed only on Android 3+).
   * @return notification image.
   */
  public Bitmap getNotificationImage()
  {
    /* Decode as bitmap now if not already done */
    if (mNotificationImageString != null && mNotificationImage == null)
    {
      /* Decode base 64 then decode as a bitmap */
      byte[] data = Base64.decode(mNotificationImageString, Base64.DEFAULT);
      if (data != null)
        try
        {
          mNotificationImage = BitmapFactory.decodeByteArray(data, 0, data.length);
        }
        catch (OutOfMemoryError e)
        {
          /* Abort */
        }

      /* On any error, don't retry next time */
      if (mNotificationImage == null)
        mNotificationImageString = null;
    }
    return mNotificationImage;
  }

  /**
   * Test if this content can be notified in the current UI context.
   * @param activity current activity name, null if no current activity.
   * @return true if this content can be notified in the current UI context.
   */
  boolean canNotify(String activity)
  {
    /*
     * If the system notification has already been displayed, always allows to replay it (for
     * example at boot) disregarding U.I. context. A system notification remains visible even if you
     * leave U.I. context that triggered it so it makes sense to replay it (would be weird to replay
     * it only when U.I context is triggered again which can happen very late or never).
     */
    if (mSystemNotification && mNotificationFirstDisplayedDate != null)
      return true;

    /* Otherwise it depends on current UI context and this campaign settings */
    switch (mBehavior)
    {
      case ANYTIME:
        return mSystemNotification || activity != null;

      case SESSION:
        return activity != null
          && (mAllowedActivities == null || mAllowedActivities.contains(activity));
    }
    return false;
  }

  @Override
  void setState(ContentValues values)
  {
    super.setState(values);
    this.mNotificationActioned = parseBoolean(values, NOTIFICATION_ACTIONED);
    this.mNotificationFirstDisplayedDate = values.getAsLong(NOTIFICATION_FIRST_DISPLAYED_DATE);
    this.mNotificationLastDisplayedDate = values.getAsLong(NOTIFICATION_LAST_DISPLAYED_DATE);
    this.mContentDisplayed = parseBoolean(values, CONTENT_DISPLAYED);
  }

  /**
   * Get a status prefix equals to "in-app-notification-" or "system-notification-", depending of
   * this notification type.
   * @return "in-app-notification-" or "system-notification-".
   */
  String getNotificationStatusPrefix()
  {
    String status;
    if (isSystemNotification())
      status = "system";
    else
      status = "in-app";
    status += "-notification-";
    return status;
  }

  /**
   * Report notification has been displayed.
   * @param context any application context.
   */
  public void displayNotification(Context context)
  {
    /* Update last displayed date */
    mNotificationLastDisplayedDate = System.currentTimeMillis();

    /* First date and reach feedback the first time */
    if (mNotificationFirstDisplayedDate == null)
    {
      mNotificationFirstDisplayedDate = mNotificationLastDisplayedDate;
      mCampaignId.sendFeedBack(context, getNotificationStatusPrefix() + "displayed", null);
    }

    /* Notify reach agent */
    EngagementReachAgent.getInstance(context).onNotificationDisplayed(this);
  }

  /**
   * Get time at which the notification was displayed the first time or null if not yet displayed.
   * @return time in ms since epoch or null.
   */
  public Long getNotificationFirstDisplayedDate()
  {
    return mNotificationFirstDisplayedDate;
  }

  /**
   * Get time at which the notification was displayed the last time or null if not yet displayed.
   * @return time in ms since epoch or null.
   */
  public Long getNotificationLastDisplayedDate()
  {
    return mNotificationLastDisplayedDate;
  }

  /**
   * Action the notification: this will display the announcement or poll, or will launch the action
   * URL associated to the notification, depending of the content kind. This will also report the
   * notification has been actioned.
   * @param context any application context.
   * @param launchIntent true to launch intent, false to just report the notification action and
   *          change internal state. If you call this method passing false, be sure that the content
   *          is either a notification only announcement or that you properly manage the content
   *          display and its life cycle (by calling actionContent or exitContent when the user is
   *          done viewing the content).
   */
  public void actionNotification(Context context, boolean launchIntent)
  {
    /* Notify agent if intent must be launched */
    EngagementReachAgent.getInstance(context).onNotificationActioned(this, launchIntent);

    /* Send feedback */
    if (!mNotificationActioned)
    {
      mCampaignId.sendFeedBack(context, getNotificationStatusPrefix() + "actioned", null);
      mNotificationActioned = true;
    }
  }

  /**
   * Report notification has been exited.
   * @param context any application context.
   */
  public void exitNotification(Context context)
  {
    process(context, getNotificationStatusPrefix() + "exited", null);
  }

  /**
   * Report content has been displayed.
   * @param context any application context.
   */
  public void displayContent(Context context)
  {
    /* Notify reach agent */
    EngagementReachAgent.getInstance(context).onContentDisplayed(this);

    /* Guard against multiple calls for feedback */
    if (!mContentDisplayed)
    {
      mCampaignId.sendFeedBack(context, "content-displayed", null);
      mContentDisplayed = true;
    }
  }
}
