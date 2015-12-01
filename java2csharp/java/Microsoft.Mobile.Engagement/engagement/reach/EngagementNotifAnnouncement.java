/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import java.util.Map;

import org.json.JSONException;

import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;

/**
 * Engagement Notification Announcement abstraction.
 */
public class EngagementNotifAnnouncement extends EngagementAbstractAnnouncement
{
  /**
   * Parse a notification announcement.
   * @param campaignId already parsed campaign id.
   * @param values campaign data.
   * @param params special parameters to inject in the action URL of the announcement.
   * @throws JSONException if a parsing error occurs.
   */
  EngagementNotifAnnouncement(CampaignId campaignId, ContentValues values,
    Map<String, String> params) throws JSONException
  {
    super(campaignId, values, params);
  }

  @Override
  Intent buildIntent()
  {
    return null;
  }

  @Override
  public void actionNotification(Context context, boolean launchIntent)
  {
    /* Normal behavior */
    super.actionNotification(context, launchIntent);

    /* This is the final step in this content kind */
    process(context, null, null);
  }

  @Override
  public void actionContent(Context context)
  {
    /* Forbid this action on notification only announcements */
    forbidAction();
  }

  @Override
  public void exitContent(Context context)
  {
    /* Forbid this action on notification only announcements */
    forbidAction();
  }

  /**
   * Throws an exception to indicate the caller that the call is forbidden.
   * @throws UnsupportedOperationException always throws it.
   */
  private void forbidAction() throws UnsupportedOperationException
  {
    throw new UnsupportedOperationException("This is a notification only announcement.");
  }
}
