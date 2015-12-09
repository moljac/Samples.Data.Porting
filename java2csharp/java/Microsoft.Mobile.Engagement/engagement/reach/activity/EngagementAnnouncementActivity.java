/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach.activity;

import android.content.Intent;

import com.microsoft.azure.engagement.reach.EngagementAnnouncement;

/**
 * Base class for all announcement activities.
 */
public abstract class EngagementAnnouncementActivity extends
  EngagementContentActivity<EngagementAnnouncement>
{
  /**
   * Track if we already executed an action, we want to do it only once (in web announcements we can
   * have an action URL and links in web view which count also as actions).
   */
  private boolean mActioned;

  @Override
  protected void onAction()
  {
    /* Report action */
    mContent.actionContent(getApplicationContext());

    /* Action the URL if specified */
    String url = mContent.getActionURL();
    if (url != null)
      executeActionURL(url);
  }

  /**
   * Execute action URL. Only the first call will be processed (whatever the URL). Other calls are
   * ignored.
   * @param url action URL (not null).
   */
  protected void executeActionURL(String url)
  {
    if (!mActioned)
      try
      {
        startActivity(Intent.parseUri(url, 0));
        mActioned = true;
      }
      catch (Exception e)
      {
        /* Ignore */
      }
  }
}
