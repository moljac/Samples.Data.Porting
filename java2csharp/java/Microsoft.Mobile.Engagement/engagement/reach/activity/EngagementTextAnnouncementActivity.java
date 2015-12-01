/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach.activity;

/**
 * Activity displaying a plain text Engagement announcement. Add this in the AndroidManifest.xml
 * file to use it:
 * 
 * <pre>
 * {@code <activity
 *   android:name="com.microsoft.azure.engagement.reach.activity.EngagementTextAnnouncementActivity"
 *   android:theme="@android:style/Theme.Light">
 *     <intent-filter>
 *       <action android:name="com.microsoft.azure.engagement.reach.intent.action.ANNOUNCEMENT" />
 *       <category android:name="android.intent.category.DEFAULT" />
 *       <data android:mimeType="text/plain" />
 *     </intent-filter>
 * </activity>}
 * </pre>
 */
public class EngagementTextAnnouncementActivity extends EngagementAnnouncementActivity
{
  @Override
  protected String getLayoutName()
  {
    return "engagement_text_announcement";
  }
}
