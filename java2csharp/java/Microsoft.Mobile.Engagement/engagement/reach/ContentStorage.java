/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import com.microsoft.azure.engagement.storage.EngagementStorage;

/** Constants used in SQLite */
final class ContentStorage
{
  /** Content local identifier */
  static final String OID = EngagementStorage.PRIMARY_KEY;

  /** Content reach campaign identifier */
  static final String CAMPAIGN_ID = "ci";

  /** DLC flag */
  static final String DLC = "dlc";

  /** Content DLC id */
  static final String DLC_ID = "id";

  /** Category */
  static final String CATEGORY = "cat";

  /** Delivery time */
  static final String DELIVERY_TIME = "dt";

  /** Campaign end time */
  static final String TTL = "ttl";

  /** Campaign end time user time zone flag */
  static final String USER_TIME_ZONE = "utz";

  /** Notification type */
  static final String NOTIFICATION_TYPE = "nt";

  /** Notification icon */
  static final String NOTIFICATION_ICON = "ic";

  /** Notification closeable */
  static final String NOTIFICATION_CLOSEABLE = "cl";

  /** Notification vibration */
  static final String NOTIFICATION_VIBRATION = "v";

  /** Notification sound */
  static final String NOTIFICATION_SOUND = "s";

  /** Notification title */
  static final String NOTIFICATION_TITLE = "tle";

  /** Notification message */
  static final String NOTIFICATION_MESSAGE = "msg";

  /** Notification big text */
  static final String NOTIFICATION_BIG_TEXT = "bt";

  /** Notification big picture */
  static final String NOTIFICATION_BIG_PICTURE = "bp";

  /** Action URL */
  static final String ACTION_URL = "au";

  /** Campaign JSON payload (dlc) */
  static final String PAYLOAD = "payload";

  /** Download identifier for attached file */
  static final String DOWNLOAD_ID = "download_id";

  /** Notification first displayed date */
  static final String NOTIFICATION_FIRST_DISPLAYED_DATE = "notification_first_displayed_date";

  /** Notification last displayed date */
  static final String NOTIFICATION_LAST_DISPLAYED_DATE = "notification_last_displayed_date";

  /** Notification actioned flag */
  static final String NOTIFICATION_ACTIONED = "notification_actioned";

  /** Content displayed flag */
  static final String CONTENT_DISPLAYED = "content_displayed";
}
