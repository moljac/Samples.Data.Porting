/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach.activity;

import static com.microsoft.azure.engagement.reach.EngagementReachAgent.INTENT_EXTRA_CONTENT_ID;
import static com.microsoft.azure.engagement.utils.EngagementResourcesUtils.getId;
import android.annotation.TargetApi;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Build;
import android.os.Bundle;
import android.view.Window;

import com.microsoft.azure.engagement.activity.EngagementActivity;
import com.microsoft.azure.engagement.reach.EngagementReachAgent;

/**
 * Activity displaying a loading screen before displaying a content. Add this in the
 * AndroidManifest.xml file to use it:
 * 
 * <pre>
 * {@code <activity
 *   android:name="com.microsoft.azure.engagement.reach.activity.EngagementLoadingActivity"
 *   android:theme="@android:style/Theme.Dialog">
 *   <intent-filter>
 *     <action android:name="com.microsoft.azure.engagement.reach.intent.action.LOADING"/>
 *     <category android:name="android.intent.category.DEFAULT"/>
 *   </intent-filter>
 * </activity>}
 * </pre>
 */
public class EngagementLoadingActivity extends EngagementActivity
{
  @Override
  protected void onCreate(Bundle savedInstanceState)
  {
    super.onCreate(savedInstanceState);
    configureWindow();
    setContentView(getId(this, getLayoutName(), "layout"));
  }

  /**
   * Configure window features, theme etc... before setContentView is called. Override this method
   * to customize window.
   */
  @TargetApi(Build.VERSION_CODES.ICE_CREAM_SANDWICH)
  protected void configureWindow()
  {
    /* Remove activity title */
    requestWindowFeature(Window.FEATURE_NO_TITLE);

    /*
     * Cumulative theme to android:theme="@android:style/Theme.Dialog" in Manifest for changing
     * progress bar look and feel to better match device version. You still need to keep
     * android:theme="@android:style/Theme.Dialog" in Manifest otherwise it would be full screen and
     * not semi-transparent, themes seem to combine correctly only when the first one is in Manifest
     * and the second one in code.
     */
    if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.ICE_CREAM_SANDWICH)
      setTheme(android.R.style.Theme_DeviceDefault_Dialog);

    /* Remove dialog frame, we can't do it in layout */
    getWindow().setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
  }

  @Override
  protected void onUserLeaveHint()
  {
    finish();
  }

  @Override
  protected void onResume()
  {
    long id = getIntent().getLongExtra(INTENT_EXTRA_CONTENT_ID, -1);
    if (!EngagementReachAgent.getInstance(this).isLoading(id))
      finish();
    super.onResume();
  }

  @Override
  protected void onPause()
  {
    if (isFinishing())
    {
      long id = getIntent().getLongExtra(INTENT_EXTRA_CONTENT_ID, -1);
      EngagementReachAgent.getInstance(this).exitLoading(id);
    }
    super.onPause();
  }

  /** Get layout name */
  protected String getLayoutName()
  {
    return "engagement_loading";
  }
}
