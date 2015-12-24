/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using EngagementStorage = com.microsoft.azure.engagement.storage.EngagementStorage;

	/// <summary>
	/// Constants used in SQLite </summary>
	internal sealed class ContentStorage
	{
	  /// <summary>
	  /// Content local identifier </summary>
	  internal const string OID = EngagementStorage.PRIMARY_KEY;

	  /// <summary>
	  /// Content reach campaign identifier </summary>
	  internal const string CAMPAIGN_ID = "ci";

	  /// <summary>
	  /// DLC flag </summary>
	  internal const string DLC = "dlc";

	  /// <summary>
	  /// Content DLC id </summary>
	  internal const string DLC_ID = "id";

	  /// <summary>
	  /// Category </summary>
	  internal const string CATEGORY = "cat";

	  /// <summary>
	  /// Delivery time </summary>
	  internal const string DELIVERY_TIME = "dt";

	  /// <summary>
	  /// Campaign end time </summary>
	  internal const string TTL = "ttl";

	  /// <summary>
	  /// Campaign end time user time zone flag </summary>
	  internal const string USER_TIME_ZONE = "utz";

	  /// <summary>
	  /// Notification type </summary>
	  internal const string NOTIFICATION_TYPE = "nt";

	  /// <summary>
	  /// Notification icon </summary>
	  internal const string NOTIFICATION_ICON = "ic";

	  /// <summary>
	  /// Notification closeable </summary>
	  internal const string NOTIFICATION_CLOSEABLE = "cl";

	  /// <summary>
	  /// Notification vibration </summary>
	  internal const string NOTIFICATION_VIBRATION = "v";

	  /// <summary>
	  /// Notification sound </summary>
	  internal const string NOTIFICATION_SOUND = "s";

	  /// <summary>
	  /// Notification title </summary>
	  internal const string NOTIFICATION_TITLE = "tle";

	  /// <summary>
	  /// Notification message </summary>
	  internal const string NOTIFICATION_MESSAGE = "msg";

	  /// <summary>
	  /// Notification big text </summary>
	  internal const string NOTIFICATION_BIG_TEXT = "bt";

	  /// <summary>
	  /// Notification big picture </summary>
	  internal const string NOTIFICATION_BIG_PICTURE = "bp";

	  /// <summary>
	  /// Action URL </summary>
	  internal const string ACTION_URL = "au";

	  /// <summary>
	  /// Campaign JSON payload (dlc) </summary>
	  internal const string PAYLOAD = "payload";

	  /// <summary>
	  /// Download identifier for attached file </summary>
	  internal const string DOWNLOAD_ID = "download_id";

	  /// <summary>
	  /// Notification first displayed date </summary>
	  internal const string NOTIFICATION_FIRST_DISPLAYED_DATE = "notification_first_displayed_date";

	  /// <summary>
	  /// Notification last displayed date </summary>
	  internal const string NOTIFICATION_LAST_DISPLAYED_DATE = "notification_last_displayed_date";

	  /// <summary>
	  /// Notification actioned flag </summary>
	  internal const string NOTIFICATION_ACTIONED = "notification_actioned";

	  /// <summary>
	  /// Content displayed flag </summary>
	  internal const string CONTENT_DISPLAYED = "content_displayed";
	}

}