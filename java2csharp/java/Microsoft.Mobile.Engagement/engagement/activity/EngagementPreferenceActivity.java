/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.activity;

import android.os.Bundle;
import android.preference.PreferenceActivity;

import com.microsoft.azure.engagement.EngagementAgent;
import com.microsoft.azure.engagement.EngagementAgentUtils;

/**
 * Helper class used to replace Android's android.app.PreferenceActivity class.
 */
public abstract class EngagementPreferenceActivity extends PreferenceActivity
{
  private EngagementAgent mEngagementAgent;

  @Override
  protected void onCreate(Bundle savedInstanceState)
  {
    super.onCreate(savedInstanceState);
    mEngagementAgent = EngagementAgent.getInstance(this);
  }

  @Override
  protected void onResume()
  {
    mEngagementAgent.startActivity(this, getEngagementActivityName(), getEngagementActivityExtra());
    super.onResume();
  }

  @Override
  protected void onPause()
  {
    mEngagementAgent.endActivity();
    super.onPause();
  }

  /**
   * Get the Engagement agent attached to this activity.
   * @return the Engagement agent
   */
  public final EngagementAgent getEngagementAgent()
  {
    return mEngagementAgent;
  }

  /**
   * Override this to specify the name reported by your activity. The default implementation returns
   * the simple name of the class and removes the "Activity" suffix if any (e.g.
   * "com.mycompany.MainActivity" -> "Main").
   * @return the activity name reported by the Engagement service.
   */
  protected String getEngagementActivityName()
  {
    return EngagementAgentUtils.buildEngagementActivityName(getClass());
  }

  /**
   * Override this to attach extra information to your activity. The default implementation attaches
   * no extra information (i.e. return null).
   * @return activity extra information, null or empty if no extra.
   */
  protected Bundle getEngagementActivityExtra()
  {
    return null;
  }
}
