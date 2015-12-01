/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import static com.microsoft.azure.engagement.reach.ContentStorage.CATEGORY;
import static com.microsoft.azure.engagement.reach.ContentStorage.DLC;
import static com.microsoft.azure.engagement.reach.ContentStorage.DLC_ID;
import static com.microsoft.azure.engagement.reach.ContentStorage.PAYLOAD;
import static com.microsoft.azure.engagement.reach.ContentStorage.TTL;
import static com.microsoft.azure.engagement.reach.ContentStorage.USER_TIME_ZONE;

import java.util.TimeZone;

import org.json.JSONException;
import org.json.JSONObject;

import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

/** Abstract class for reach content such as announcements and polls. */
public abstract class EngagementReachContent
{
  /** If set, the notification has downloadble content */
  public static final int FLAG_DLC_NOTIFICATION = 1;

  /** If set, the clickable content exists */
  public static final int FLAG_DLC_CONTENT = 2;

  /** Local identifier (which is a SQLite auto-increment) */
  private long mLocalID;

  /** DLC flags */
  private final int mDlc;

  /** DLC id */
  private final String mDlcId;

  /** Campaign id */
  final CampaignId mCampaignId;

  /** Category, null for default */
  final String mCategory;

  /** Body */
  String mBody;

  /** Expiry timestamp (ms since epoch), if any */
  private final Long mExpiry;

  /** Intent that must be launched for viewing this content */
  private Intent mIntent;

  /** True if replied, thus processed entirely, to avoid making that several times */
  private boolean mProcessed;

  /** Download identifier */
  private Long mDownloadId;

  /** Flag to remember DLC completes */
  private boolean mDlcCompleted;

  /**
   * Parse a campaign.
   * @param campaignId already parsed campaign id.
   * @param values content data.
   * @throws JSONException if payload parsing failure.
   */
  EngagementReachContent(CampaignId campaignId, ContentValues values) throws JSONException
  {
    /* Parse base fields */
    mCampaignId = campaignId;
    mDlc = values.getAsInteger(DLC);
    mDlcId = values.getAsString(DLC_ID);
    mCategory = values.getAsString(CATEGORY);
    Long expiry = values.getAsLong(TTL);
    if (expiry != null)
    {
      expiry *= 1000L;
      if (parseBoolean(values, USER_TIME_ZONE))
        expiry -= TimeZone.getDefault().getOffset(expiry);
    }
    mExpiry = expiry;
    if (values.containsKey(PAYLOAD))
      setPayload(new JSONObject(values.getAsString(PAYLOAD)));
  }

  /**
   * Parse boolean from content values.
   * @param values content values.
   * @param key key.
   * @return boolean value.
   */
  static boolean parseBoolean(ContentValues values, String key)
  {
    Integer val = values.getAsInteger(key);
    return val != null && val == 1;
  }

  /**
   * Get local identifier.
   * @return the local identifier.
   */
  public long getLocalId()
  {
    return mLocalID;
  }

  /**
   * Set the local identifier.
   * @param localId the local identifier.
   */
  void setLocalId(long localId)
  {
    /* Remember id */
    mLocalID = localId;
  }

  /** @return campaign id */
  public CampaignId getCampaignId()
  {
    return mCampaignId;
  }

  /** @return true if a download is necessary */
  public boolean hasDLC()
  {
    return mDlc > 0;
  }

  /** @return true if a download is necessary to display notification */
  public boolean hasNotificationDLC()
  {
    return (mDlc & FLAG_DLC_NOTIFICATION) == FLAG_DLC_NOTIFICATION;
  }

  /** @return true if a download is necessary after notification click */
  public boolean hasContentDLC()
  {
    return (mDlc & FLAG_DLC_CONTENT) == FLAG_DLC_CONTENT;
  }

  /** @return DLC id */
  public String getDlcId()
  {
    return mDlcId;
  }

  /** Set payload */
  void setPayload(JSONObject payload) throws JSONException
  {
    /* Read field */
    mDlcCompleted = true;
    mBody = payload.optString("body", null);
  }

  /**
   * Check if DLC completed.
   * @return true if DLC completed.
   */
  public boolean isDlcCompleted()
  {
    return mDlcCompleted;
  }

  /**
   * Get the category of this content. This category is also added in the intent that launches the
   * viewing activity if any. If this method returns <tt>null</tt>, the category list in the intent
   * will only contain <tt>android.intent.category.DEFAULT</tt>.
   * @return category.
   */
  public String getCategory()
  {
    return mCategory;
  }

  /**
   * Get content's body.
   * @return content's body.
   */
  public String getBody()
  {
    return mBody;
  }

  /**
   * Get the base intent to launch to view this content. The intent will be filtered with a
   * component name.
   * @return the base intent to launch to view this content.
   */
  public Intent getIntent()
  {
    if (mIntent == null && mDlcCompleted)
    {
      mIntent = buildIntent();
      if (mIntent != null)
        EngagementReachAgent.setContentIdExtra(mIntent, this);
    }
    return mIntent;
  }

  /**
   * Build the intent to launch to view this content.
   * @return the intent to launch to view this content.
   */
  abstract Intent buildIntent();

  /**
   * Get content expiration date (in ms since epoch).
   * @return content expiration date or null if not specified.
   */
  public Long getExpiry()
  {
    return mExpiry;
  }

  /**
   * Check if this content is now expired.
   * @return true iff this content is expired now.
   */
  public boolean hasExpired()
  {
    return mExpiry != null && System.currentTimeMillis() >= mExpiry;
  }

  /**
   * Drop content.
   * @param context any application context.
   */
  public void dropContent(Context context)
  {
    process(context, "dropped", null);
  }

  /**
   * Report content has been actioned.
   * @param context any application context.
   */
  public void actionContent(Context context)
  {
    process(context, "content-actioned", null);
  }

  /**
   * Report content been exited.
   * @param context any application context.
   */
  public void exitContent(Context context)
  {
    process(context, "content-exited", null);
  }

  /**
   * Check whether this content has a system notification.
   * @return true iff the content has a system notification.
   */
  public boolean isSystemNotification()
  {
    /* By default */
    return false;
  }

  /**
   * Dispose of this content so that new content can be notified. Possibly send feedback to the
   * service that sent it.
   * @param context application context.
   * @param status feedback status if any (null not to send anything).
   * @param extras extra information like poll answers.
   */
  void process(Context context, String status, Bundle extras)
  {
    /* Do it once */
    if (!mProcessed)
    {
      /* Send feedback if any */
      if (status != null)
        mCampaignId.sendFeedBack(context, status, extras);

      /* Mark this announcement as processed */
      mProcessed = true;

      /* Tell the reach application manager that announcement has been processed */
      EngagementReachAgent.getInstance(context).onContentProcessed(this);
    }
  }

  /**
   * Set content status.
   * @param values values from storage.
   */
  void setState(ContentValues values)
  {
    mDownloadId = values.getAsLong(ContentStorage.DOWNLOAD_ID);
  }

  /**
   * Get download identifier if a download has ever been scheduled for this content.
   * @return download identifier or null if download has never been scheduled.
   */
  public Long getDownloadId()
  {
    return mDownloadId;
  }

  /**
   * Set download identifier for this content.
   * @param context any application context.
   * @param downloadId download identifier.
   */
  public void setDownloadId(Context context, long downloadId)
  {
    mDownloadId = downloadId;
    EngagementReachAgent.getInstance(context).onDownloadScheduled(this, downloadId);
  }
}
