/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import java.util.Map;

import org.json.JSONException;
import org.json.JSONObject;

import android.content.ContentValues;
import android.content.Intent;

/**
 * Engagement Announcement abstraction.
 */
public class EngagementAnnouncement extends EngagementAbstractAnnouncement
{
  /** Intent action used by the Reach SDK. */
  public static final String INTENT_ACTION = "com.microsoft.azure.engagement.reach.intent.action.ANNOUNCEMENT";

  /** Special parameters to inject in the body of the datapush. */
  private final Map<String, String> params;

  /** MIME type */
  private String mType;

  /**
   * Parse an announcement.
   * @param campaignId already parsed campaign id.
   * @param values campaign data.
   * @param params special parameters to inject in the action URL and body of the announcement.
   * @throws JSONException if a parsing error occurs.
   */
  EngagementAnnouncement(CampaignId campaignId, ContentValues values, Map<String, String> params)
    throws JSONException
  {
    super(campaignId, values, params);
    this.params = params;
    replacePayloadParams();
  }

  @Override
  void setPayload(JSONObject payload) throws JSONException
  {
    super.setPayload(payload);
    replacePayloadParams();
    mType = payload.getString("type");
  }

  /** Replace parameters within payload fields that support them */
  private void replacePayloadParams()
  {
    if (params != null)
      for (Map.Entry<String, String> param : params.entrySet())
      {
        if (mBody != null)
          mBody = mBody.replace(param.getKey(), param.getValue());
      }
  }

  @Override
  Intent buildIntent()
  {
    Intent intent = new Intent(INTENT_ACTION);
    intent.setType(getType());
    String category = getCategory();
    if (category != null)
      intent.addCategory(category);
    return intent;
  }

  /**
   * Get the mime type for this announcement. This is useful to interpret the text returned by
   * {@link #getBody()}. This type will also be set in the intent that launches the viewing
   * activity.
   * @return mime type.
   */
  public String getType()
  {
    return mType;
  }
}
