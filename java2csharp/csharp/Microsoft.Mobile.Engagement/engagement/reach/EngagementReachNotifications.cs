/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	/// <summary>
	/// Constants related to Reach notifications </summary>
	public class EngagementReachNotifications
	{
	  /// <summary>
	  /// Reach notification content icon meta-data </summary>
	  public const string METADATA_NOTIFICATION_ICON = "engagement:reach:notification:icon";

	  /// <summary>
	  /// Overlay enabled meta-data </summary>
	  public const string METADATA_NOTIFICATION_OVERLAY = "engagement:notification:overlay";

	  /// <summary>
	  /// Prefix for all notification layout constants </summary>
	  private const string LAYOUT_NOTIFICATION_PREFIX = "engagement_notification_";

	  /// <summary>
	  /// ID of the icon view </summary>
	  public static readonly string LAYOUT_NOTIFICATION_ICON = LAYOUT_NOTIFICATION_PREFIX + "icon";

	  /// <summary>
	  /// Layout name of the in-app notification area, this is also the view identifier of the area
	  /// itself.
	  /// </summary>
	  public static readonly string LAYOUT_NOTIFICATION_AREA = LAYOUT_NOTIFICATION_PREFIX + "area";

	  /// <summary>
	  /// Layout name and root view id for in-app notification overlay. </summary>
	  public static readonly string LAYOUT_NOTIFICATION_OVERLAY = LAYOUT_NOTIFICATION_PREFIX + "overlay";

	  /// <summary>
	  /// ID of the close button in in-app notifications </summary>
	  public static readonly string LAYOUT_NOTIFICATION_CLOSE = LAYOUT_NOTIFICATION_PREFIX + "close";

	  /// <summary>
	  /// ID of the optional area to fill space for the close button in the linear layout, this is a
	  /// layout trick.
	  /// </summary>
	  public static readonly string LAYOUT_NOTIFICATION_CLOSE_AREA = LAYOUT_NOTIFICATION_PREFIX + "close_area";

	  /// <summary>
	  /// ID of the view containing notification title and message </summary>
	  public static readonly string LAYOUT_NOTIFICATION_TEXT = LAYOUT_NOTIFICATION_PREFIX + "text";

	  /// <summary>
	  /// ID of the view containing notification message </summary>
	  public static readonly string LAYOUT_NOTIFICATION_MESSAGE = LAYOUT_NOTIFICATION_PREFIX + "message";

	  /// <summary>
	  /// ID of the view containing notification title </summary>
	  public static readonly string LAYOUT_NOTIFICATION_TITLE = LAYOUT_NOTIFICATION_PREFIX + "title";

	  /// <summary>
	  /// ID of the view containing notification image </summary>
	  public static readonly string LAYOUT_NOTIFICATION_IMAGE = LAYOUT_NOTIFICATION_PREFIX + "image";
	}

}