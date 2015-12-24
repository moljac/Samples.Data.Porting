/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{

	/// <summary>
	/// Intent constants </summary>
	public sealed class EngagementIntents
	{
	  private EngagementIntents()
	  {
	  }

	  /// <summary>
	  /// Push received action or message downloaded </summary>
	  public const string INTENT_ACTION_MESSAGE = "com.microsoft.azure.engagement.intent.action.MESSAGE";

	  /// <summary>
	  /// Intent extra type key </summary>
	  public const string INTENT_EXTRA_TYPE = "type";

	  /// <summary>
	  /// Intent extra type value for push received </summary>
	  public const string INTENT_EXTRA_TYPE_PUSH = "push";

	  /// <summary>
	  /// Intent extra type value for DLC received </summary>
	  public const string INTENT_EXTRA_TYPE_DLC = "dlc";

	  /// <summary>
	  /// Intent id parameter </summary>
	  public const string INTENT_EXTRA_ID = "id";

	  /// <summary>
	  /// Intent payload parameter </summary>
	  public const string INTENT_EXTRA_PAYLOAD = "payload";
	}

}