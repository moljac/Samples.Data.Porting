/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.adm;

import static com.microsoft.azure.engagement.EngagementNativePushToken.Type.ADM;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;

import com.microsoft.azure.engagement.EngagementNativePushToken;
import com.microsoft.azure.engagement.nativepush.EngagementNativePushAgent;

/**
 * This class is required to communicate the ADM registration id to the Engagement Push service and
 * to receive ADM messages. Add this to your AndroidManifest.xml:
 * 
 * <pre>
 * {@code
 * <receiver android:name="com.microsoft.azure.engagement.gcm.EngagementADMReceiver" android:permission="com.google.android.c2dm.permission.SEND">
 *   <intent-filter>
 *     <action android:name="com.google.android.c2dm.intent.REGISTRATION" />
 *     <action android:name="com.google.android.c2dm.intent.RECEIVE" />
 *     <category android:name="<your_package_name>" />
 *   </intent-filter>
 * </receiver>
 * }
 * </pre>
 * 
 * Please ensure you have the following permissions:
 * 
 * <pre>
 * {@code
 * <uses-permission android:name="android.permission.WAKE_LOCK"/>
 * <uses-permission android:name="com.amazon.device.messaging.permission.RECEIVE" />
 * <uses-permission android:name="<your_package_name>.permission.RECEIVE_ADM_MESSAGE" />
 * <permission android:name="<your_package_name>.permission.RECEIVE_ADM_MESSAGE" android:protectionLevel="signature" />
 * }
 * </pre>
 * 
 * You also have to integrate {@link EngagementADMEnabler} if you don't already manage the ADM
 * integration yourself.
 * @see EngagementADMEnabler
 */
public class EngagementADMReceiver extends BroadcastReceiver
{
  /** Action when we receive token */
  private static final String INTENT_ACTION_REGISTRATION = "com.amazon.device.messaging.intent.REGISTRATION";

  /** Token key in intent result */
  private static final String INTENT_EXTRA_REGISTRATION = "registration_id";

  /** Action when we receive a push */
  public static final String INTENT_ACTION_RECEIVE = "com.amazon.device.messaging.intent.RECEIVE";

  @Override
  public void onReceive(Context context, Intent intent)
  {
    /* Registration result action */
    String action = intent.getAction();
    if (INTENT_ACTION_REGISTRATION.equals(action))
    {
      /* Handle register if successful (otherwise we'll retry next time process is started) */
      String registrationId = intent.getStringExtra(INTENT_EXTRA_REGISTRATION);
      if (registrationId != null)
      {
        EngagementNativePushAgent nativePushAgent = EngagementNativePushAgent.getInstance(context);
        nativePushAgent.registerNativePush(new EngagementNativePushToken(registrationId, ADM));
      }
    }

    /* Received message action */
    else if (INTENT_ACTION_RECEIVE.equals(action))
      EngagementNativePushAgent.getInstance(context).onPushReceived(intent.getExtras());
  }
}
