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
	/// Engagement DataPush abstraction.
	/// </summary>
	public class EngagementDataPush : EngagementReachContent
	{
	  /// <summary>
	  /// Intent action used by the Reach SDK. </summary>
	  public const string INTENT_ACTION = "com.microsoft.azure.engagement.reach.intent.action.DATA_PUSH";

	  /// <summary>
	  /// Special parameters to inject in the body of the datapush. </summary>
	  private readonly IDictionary<string, string> @params;

	  /// <summary>
	  /// MIME type </summary>
	  private string mType;

	  /// <summary>
	  /// Parse a datapush. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> campaign data. </param>
	  /// <param name="params"> special parameters to inject in the body of the datapush. </param>
	  /// <exception cref="JSONException"> if payload parsing error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementDataPush(CampaignId campaignId, android.content.ContentValues values, java.util.Map<String, String> params) throws org.json.JSONException
	  internal EngagementDataPush(CampaignId campaignId, ContentValues values, IDictionary<string, string> @params) : base(campaignId, values)
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
		/*
		 * Unlike interactive contents whose content is either cached in RAM (current content) or
		 * retrieved from SQLite from its identifier (to handle application restart for system
		 * notifications), we drop the content as soon as the first broadcast receiver that handles
		 * datapush acknowledges or cancel the content. We need to put data in the intent to handle
		 * several broadcast receivers.
		 */
		Intent intent = new Intent(INTENT_ACTION);
		intent.putExtra("category", mCategory);
		intent.putExtra("body", mBody);
		intent.putExtra("type", mType);
		return intent;
	  }

	  /// <summary>
	  /// Get encoding type for the body. </summary>
	  /// <returns> "text/plain or "text/base64". </returns>
	  public virtual string Type
	  {
		  get
		  {
			return mType;
		  }
	  }
	}

}