using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using JSONException = org.json.JSONException;

	using ContentValues = android.content.ContentValues;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	/// <summary>
	/// Engagement Notification Announcement abstraction.
	/// </summary>
	public class EngagementNotifAnnouncement : EngagementAbstractAnnouncement
	{
	  /// <summary>
	  /// Parse a notification announcement. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> campaign data. </param>
	  /// <param name="params"> special parameters to inject in the action URL of the announcement. </param>
	  /// <exception cref="JSONException"> if a parsing error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementNotifAnnouncement(CampaignId campaignId, android.content.ContentValues values, java.util.Map<String, String> params) throws org.json.JSONException
	  internal EngagementNotifAnnouncement(CampaignId campaignId, ContentValues values, IDictionary<string, string> @params) : base(campaignId, values, @params)
	  {
	  }

	  internal override Intent buildIntent()
	  {
		return null;
	  }

	  public override void actionNotification(Context context, bool launchIntent)
	  {
		/* Normal behavior */
		base.actionNotification(context, launchIntent);

		/* This is the final step in this content kind */
		process(context, null, null);
	  }

	  public override void actionContent(Context context)
	  {
		/* Forbid this action on notification only announcements */
		forbidAction();
	  }

	  public override void exitContent(Context context)
	  {
		/* Forbid this action on notification only announcements */
		forbidAction();
	  }

	  /// <summary>
	  /// Throws an exception to indicate the caller that the call is forbidden. </summary>
	  /// <exception cref="UnsupportedOperationException"> always throws it. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void forbidAction() throws UnsupportedOperationException
	  private void forbidAction()
	  {
		throw new System.NotSupportedException("This is a notification only announcement.");
	  }
	}

}