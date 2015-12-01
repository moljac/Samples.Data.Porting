/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import android.os.Bundle;
import com.microsoft.azure.engagement.EngagementConfiguration;
import com.microsoft.azure.engagement.EngagementNativePushToken;

/**
 * Engagement API, the Engagement service returns this remote interface when binding to it.<br/>
 * The binding intent has just the Engagement service as component name, nothing else.
 * This class is not designed to be used directly in user code, please use {@link com.microsoft.azure.engagement.EngagementAgent} instead.
 * @see com.microsoft.azure.engagement.EngagementAgent
 */
interface IEngagementService
{
  oneway void init(in EngagementConfiguration configuration);

  oneway void startActivity(String activityName, in Bundle extras);

  oneway void endActivity();

  oneway void startJob(String name, in Bundle extras);

  oneway void endJob(String name);

  oneway void sendEvent(String name, in Bundle extras);

  oneway void sendSessionEvent(String name, in Bundle extras);

  oneway void sendJobEvent(String name, String jobName, in Bundle extras);

  oneway void sendError(String name, in Bundle extras);

  oneway void sendSessionError(String name, in Bundle extras);

  oneway void sendJobError(String name, String jobName, in Bundle extras);

  String getDeviceId();

  oneway void sendAppInfo(in Bundle appInfo);

  oneway void sendReachFeedback(String kind, String contentId, String status, in Bundle extras);

  oneway void registerNativePush(in EngagementNativePushToken token);

  oneway void getMessage(String id);
}
