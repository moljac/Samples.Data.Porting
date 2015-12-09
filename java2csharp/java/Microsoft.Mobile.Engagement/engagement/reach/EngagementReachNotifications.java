/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

/** Constants related to Reach notifications */
public class EngagementReachNotifications
{
  /** Reach notification content icon meta-data */
  public static final String METADATA_NOTIFICATION_ICON = "engagement:reach:notification:icon";

  /** Overlay enabled meta-data */
  public static final String METADATA_NOTIFICATION_OVERLAY = "engagement:notification:overlay";

  /** Prefix for all notification layout constants */
  private static final String LAYOUT_NOTIFICATION_PREFIX = "engagement_notification_";

  /** ID of the icon view */
  public static final String LAYOUT_NOTIFICATION_ICON = LAYOUT_NOTIFICATION_PREFIX + "icon";

  /**
   * Layout name of the in-app notification area, this is also the view identifier of the area
   * itself.
   */
  public static final String LAYOUT_NOTIFICATION_AREA = LAYOUT_NOTIFICATION_PREFIX + "area";

  /** Layout name and root view id for in-app notification overlay. */
  public static final String LAYOUT_NOTIFICATION_OVERLAY = LAYOUT_NOTIFICATION_PREFIX + "overlay";

  /** ID of the close button in in-app notifications */
  public static final String LAYOUT_NOTIFICATION_CLOSE = LAYOUT_NOTIFICATION_PREFIX + "close";

  /**
   * ID of the optional area to fill space for the close button in the linear layout, this is a
   * layout trick.
   */
  public static final String LAYOUT_NOTIFICATION_CLOSE_AREA = LAYOUT_NOTIFICATION_PREFIX
    + "close_area";

  /** ID of the view containing notification title and message */
  public static final String LAYOUT_NOTIFICATION_TEXT = LAYOUT_NOTIFICATION_PREFIX + "text";

  /** ID of the view containing notification message */
  public static final String LAYOUT_NOTIFICATION_MESSAGE = LAYOUT_NOTIFICATION_PREFIX + "message";

  /** ID of the view containing notification title */
  public static final String LAYOUT_NOTIFICATION_TITLE = LAYOUT_NOTIFICATION_PREFIX + "title";

  /** ID of the view containing notification image */
  public static final String LAYOUT_NOTIFICATION_IMAGE = LAYOUT_NOTIFICATION_PREFIX + "image";
}
