/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.ContentValues;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

/**
 * Engagement Poll abstraction.
 */
public class EngagementPoll extends EngagementReachInteractiveContent
{
  /** Intent poll action used by the reach SDK. */
  public static final String INTENT_ACTION = "com.microsoft.azure.engagement.reach.intent.action.POLL";

  /** Questions as a bundle */
  private JSONArray mQuestions;

  /** Answer form */
  private final Bundle mAnswers;

  /**
   * Parse a poll.
   * @param campaignId already parsed campaign id.
   * @param values campaign data.
   * @throws JSONException if a parsing error occurs.
   */
  EngagementPoll(CampaignId campaignId, ContentValues values) throws JSONException
  {
    super(campaignId, values);
    mAnswers = new Bundle();
  }

  @Override
  void setPayload(JSONObject payload) throws JSONException
  {
    super.setPayload(payload);
    mQuestions = payload.getJSONArray("questions");
  }

  @Override
  Intent buildIntent()
  {
    Intent intent = new Intent(INTENT_ACTION);
    String category = getCategory();
    if (category != null)
      intent.addCategory(category);
    return intent;
  }

  /**
   * Get questions for this poll as a JSON array. Each question is a JSON object with the following
   * structure:
   * <ul>
   * <li>"id" -> String</li>
   * <li>"title" -> String</li>
   * <li>"choices" -> JSONArray
   * <ul>
   * <li>"id" -> String
   * <li>"title" -> String</li>
   * <li>"isDefault" -> boolean (optional, default is false)</li>
   * </ul>
   * </ul>
   * </li> </ul> </li> </ul>
   * @return questions definition.
   */
  public JSONArray getQuestions()
  {
    return mQuestions;
  }

  /**
   * Fill answer for a given question. Answers are sent when calling {@link #actionContent(Context)}
   * .
   * @param questionId question id as specified in the Bundle returned by {@link #getQuestions()}.
   * @param choiceId choice id as specified in the Bundle returned by {@link #getQuestions()}.
   */
  public void fillAnswer(String questionId, String choiceId)
  {
    mAnswers.putString(questionId, choiceId);
  }

  @Override
  public void actionContent(Context context)
  {
    process(context, "content-actioned", mAnswers);
  }
}
