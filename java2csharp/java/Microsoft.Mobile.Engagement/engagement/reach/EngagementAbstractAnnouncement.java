/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import static com.microsoft.azure.engagement.reach.ContentStorage.ACTION_URL;

import java.util.Map;

import org.json.JSONException;

import android.content.ContentValues;

/** Base class for all kind of announcements. */
public abstract class EngagementAbstractAnnouncement extends EngagementReachInteractiveContent
{
  /** Action's URL */
  private final String mActionURL;

  /**
   * Parse an announcement.
   * @param campaignId already parsed campaign id.
   * @param values campaign data.
   * @param params special parameters to inject in the action URL of the announcement.
   * @throws JSONException if a parsing error occurs.
   */
  EngagementAbstractAnnouncement(CampaignId campaignId, ContentValues values,
    Map<String, String> params) throws JSONException
  {
    super(campaignId, values);
    String actionURL = values.getAsString(ACTION_URL);
    for (Map.Entry<String, String> param : params.entrySet())
    {
      if (actionURL != null)
        actionURL = actionURL.replace(param.getKey(), param.getValue());
    }
    mActionURL = actionURL;
  }

  /**
   * Get action's URL.
   * @return action's URL.
   */
  public String getActionURL()
  {
    return mActionURL;
  }
}
