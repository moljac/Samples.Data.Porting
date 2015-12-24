using System;
using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using ContentValues = android.content.ContentValues;
	using Intent = android.content.Intent;

	/// <summary>
	/// Engagement Announcement abstraction.
	/// </summary>
	public class EngagementAnnouncement : EngagementAbstractAnnouncement
	{
	  /// <summary>
	  /// Intent action used by the Reach SDK. </summary>
	  public const string INTENT_ACTION = "com.microsoft.azure.engagement.reach.intent.action.ANNOUNCEMENT";

	  /// <summary>
	  /// Special parameters to inject in the body of the datapush. </summary>
	  private readonly IDictionary<string, string> @params;

	  /// <summary>
	  /// MIME type </summary>
	  private string mType;

	  /// <summary>
	  /// Parse an announcement. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> campaign data. </param>
	  /// <param name="params"> special parameters to inject in the action URL and body of the announcement. </param>
	  /// <exception cref="JSONException"> if a parsing error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementAnnouncement(CampaignId campaignId, android.content.ContentValues values, java.util.Map<String, String> params) throws org.json.JSONException
	  internal EngagementAnnouncement(CampaignId campaignId, ContentValues values, IDictionary<string, string> @params) : base(campaignId, values, @params)
	  {
		this.@params = @params;
		replacePayloadParams();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override void setPayload(org.json.JSONObject payload) throws org.json.JSONException
	  internal override JSONObject Payload
	  {
		  set
		  {
			base.Payload = value;
			replacePayloadParams();
			mType = value.getString("type");
		  }
	  }

	  /// <summary>
	  /// Replace parameters within payload fields that support them </summary>
	  private void replacePayloadParams()
	  {
		if (@params != null)
		{
		  foreach (KeyValuePair<string, string> param in @params.SetOfKeyValuePairs())
		  {
			if (mBody != null)
			{
			  mBody = mBody.Replace(param.Key, param.Value);
			}
		  }
		}
	  }

	  internal override Intent buildIntent()
	  {
		Intent intent = new Intent(INTENT_ACTION);
		intent.Type = Type;
		string category = Category;
		if (category != null)
		{
		  intent.addCategory(category);
		}
		return intent;
	  }

	  /// <summary>
	  /// Get the mime type for this announcement. This is useful to interpret the text returned by
	  /// <seealso cref="#getBody()"/>. This type will also be set in the intent that launches the viewing
	  /// activity. </summary>
	  /// <returns> mime type. </returns>
	  public virtual string Type
	  {
		  get
		  {
			return mType;
		  }
	  }
	}

}