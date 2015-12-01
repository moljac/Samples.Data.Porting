/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.nativepush;

import static com.microsoft.azure.engagement.EngagementIntents.INTENT_ACTION_MESSAGE;
import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE;
import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE_PUSH;
import static com.microsoft.azure.engagement.EngagementNativePushToken.typeFromInt;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;
import android.os.Bundle;

import com.microsoft.azure.engagement.EngagementAgent;
import com.microsoft.azure.engagement.EngagementNativePushToken;
import com.microsoft.azure.engagement.adm.EngagementADMEnabler;
import com.microsoft.azure.engagement.adm.EngagementADMReceiver;
import com.microsoft.azure.engagement.gcm.EngagementGCMEnabler;
import com.microsoft.azure.engagement.gcm.EngagementGCMReceiver;

/**
 * <p>
 * This agent manages the life cycle of the registration identifier (called token in Engagement)
 * used in native push.
 * </p>
 * <p>
 * The token is associated to a Native Push service. A device may only have one token at any time
 * but you can safely integrate several services: at most 1 service is available at runtime.
 * <p>
 * Engagement currently supports GCM (Google Cloud Messaging) and ADM (Amazon Device Messaging).
 * Engagement cannot work with GCM if your own application code uses deprecated C2DM: the device can
 * only have one registration identifier at any time. Please migrate your application to GCM in that
 * case.
 * </p>
 * <p>
 * You don't need to use this class directly, you can instead integrate the following classes.
 * </p>
 * <ul>
 * <li>For GCM:
 * <ul>
 * <li>{@link EngagementGCMEnabler}</li>
 * <li>{@link EngagementGCMReceiver}</li>
 * </ul>
 * </li>
 * <li>For ADM:
 * <ul>
 * <li>{@link EngagementADMEnabler}</li>
 * <li>{@link EngagementADMReceiver}</li>
 * </ul>
 * </li>
 * </ul>
 */
public class EngagementNativePushAgent
{
  /**
   * Native push payload extra parameter to detect our namespace (value is irrelevant and is likely
   * empty string).
   */
  private static final String INTENT_EXTRA_AZME = "azme";

  /** Storage file name */
  private static final String STORAGE_FILE = "engagement.nativepush";

  /** Storage key: application identifier for which we sent the token. */
  private static final String APP_ID = "appId";

  /** Storage key: token value */
  private static final String TOKEN_VALUE = "val";

  /** Storage key: token type */
  private static final String TOKEN_TYPE = "type";

  /** Storage key: sent timestamp in ms since epoch */
  private static final String SENT = "sent";

  /** Storage key: new registration identifier value */
  private static final String NEW_TOKEN_VALUE = "newVal";

  /** Storage key: new registration identifier type */
  private static final String NEW_TOKEN_TYPE = "newType";

  /**
   * We re-send the same registration identifier only if some time has elapsed, specified by this
   * constant (in ms). Value: 1 day.
   */
  private static final long SENT_EXPIRY = 86400000L;

  /** Unique instance */
  private static EngagementNativePushAgent sInstance;

  /**
   * Get the unique instance.
   * @param context any valid context.
   */
  public static EngagementNativePushAgent getInstance(Context context)
  {
    /* Always check this even if we instantiate once to trigger null pointer in all cases */
    if (sInstance == null)
      sInstance = new EngagementNativePushAgent(context.getApplicationContext());
    return sInstance;
  }

  /** Context */
  private final Context mContext;

  /** Storage file */
  private final SharedPreferences mStorage;

  /** Engagement agent */
  private final EngagementAgent mEngagementAgent;

  /** Application identifier */
  private String mAppId;

  /**
   * Init.
   * @param context application context.
   */
  private EngagementNativePushAgent(Context context)
  {
    /* Init */
    mContext = context;
    mEngagementAgent = EngagementAgent.getInstance(context);
    mStorage = context.getSharedPreferences(STORAGE_FILE, 0);
  }

  /**
   * Calls {@link EngagementAgent#registerNativePush(EngagementNativePushToken)} only if the token
   * is a new one or some time has elapsed since the last time we sent it.
   * @param token token to register.
   */
  public void registerNativePush(EngagementNativePushToken token)
  {
    /* If application identifier is unknown */
    if (mAppId == null)
    {
      /*
       * Keep state until onAppIdGot is called. Possibly in the next application launch so it must
       * be persisted.
       */
      Editor edit = mStorage.edit();
      edit.putString(NEW_TOKEN_VALUE, token.getToken());
      edit.putInt(NEW_TOKEN_TYPE, token.getType().ordinal());
      edit.commit();
    }
    else
    {
      /* Get state */
      String oldAppId = mStorage.getString(APP_ID, null);
      String oldTokenValue = mStorage.getString(TOKEN_VALUE, null);
      EngagementNativePushToken.Type oldType = EngagementNativePushToken.typeFromInt(mStorage.getInt(
        TOKEN_TYPE, -1));
      long sent = mStorage.getLong(SENT, 0);
      long elapsedSinceSent = System.currentTimeMillis() - sent;

      /* If registrationId changed or enough time elapsed since we sent it */
      if (oldAppId == null || !oldAppId.equals(mAppId) || oldTokenValue == null
        || !oldTokenValue.equals(token.getToken()) || oldType == null
        || !oldType.equals(token.getType()) || elapsedSinceSent >= SENT_EXPIRY)
      {
        /* Send registration identifier */
        mEngagementAgent.registerNativePush(token);

        /* Update state */
        Editor edit = mStorage.edit();
        edit.clear();
        edit.putString(APP_ID, mAppId);
        edit.putString(TOKEN_VALUE, token.getToken());
        edit.putInt(TOKEN_TYPE, token.getType().ordinal());
        edit.putLong(SENT, System.currentTimeMillis());
        edit.commit();
      }
    }
  }

  /**
   * Notify when the application identifier is known.
   * @param appId application identifier.
   */
  public void onAppIdGot(String appId)
  {
    /* Keep identifier */
    mAppId = appId;

    /* Send pending token if any */
    String value = mStorage.getString(NEW_TOKEN_VALUE, null);
    EngagementNativePushToken.Type type = typeFromInt(mStorage.getInt(NEW_TOKEN_TYPE, -1));
    if (value != null && type != null)
      registerNativePush(new EngagementNativePushToken(value, type));
  }

  /**
   * Notify when a push is received.
   * @param extras push parameters.
   */
  public void onPushReceived(Bundle extras)
  {
    /* Relay push to listeners if its an azme push */
    if (extras.containsKey(INTENT_EXTRA_AZME))
    {
      Intent intent = new Intent(INTENT_ACTION_MESSAGE);
      intent.setPackage(mContext.getPackageName());
      extras.putString(INTENT_EXTRA_TYPE, INTENT_EXTRA_TYPE_PUSH);
      intent.putExtras(extras);
      mContext.sendBroadcast(intent);
    }
  }
}
