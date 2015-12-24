using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.ACTION_URL;

	using JSONException = org.json.JSONException;

	using ContentValues = android.content.ContentValues;

	/// <summary>
	/// Base class for all kind of announcements. </summary>
	public abstract class EngagementAbstractAnnouncement : EngagementReachInteractiveContent
	{
	  /// <summary>
	  /// Action's URL </summary>
	  private readonly string mActionURL;

	  /// <summary>
	  /// Parse an announcement. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> campaign data. </param>
	  /// <param name="params"> special parameters to inject in the action URL of the announcement. </param>
	  /// <exception cref="JSONException"> if a parsing error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementAbstractAnnouncement(CampaignId campaignId, android.content.ContentValues values, java.util.Map<String, String> params) throws org.json.JSONException
	  internal EngagementAbstractAnnouncement(CampaignId campaignId, ContentValues values, IDictionary<string, string> @params) : base(campaignId, values)
	  {
		string actionURL = values.getAsString(ACTION_URL);
		foreach (KeyValuePair<string, string> param in @params.SetOfKeyValuePairs())
		{
		  if (actionURL != null)
		  {
			actionURL = actionURL.Replace(param.Key, param.Value);
		  }
		}
		mActionURL = actionURL;
	  }

	  /// <summary>
	  /// Get action's URL. </summary>
	  /// <returns> action's URL. </returns>
	  public virtual string ActionURL
	  {
		  get
		  {
			return mActionURL;
		  }
	  }
	}

}