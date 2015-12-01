/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

/** Intent constants */
public final class EngagementIntents
{
  private EngagementIntents()
  {
  }

  /** Push received action or message downloaded */
  public static final String INTENT_ACTION_MESSAGE = "com.microsoft.azure.engagement.intent.action.MESSAGE";

  /** Intent extra type key */
  public static final String INTENT_EXTRA_TYPE = "type";

  /** Intent extra type value for push received */
  public static final String INTENT_EXTRA_TYPE_PUSH = "push";

  /** Intent extra type value for DLC received */
  public static final String INTENT_EXTRA_TYPE_DLC = "dlc";

  /** Intent id parameter */
  public static final String INTENT_EXTRA_ID = "id";

  /** Intent payload parameter */
  public static final String INTENT_EXTRA_PAYLOAD = "payload";
}
