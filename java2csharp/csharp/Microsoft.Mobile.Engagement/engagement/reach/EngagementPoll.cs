/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using JSONArray = org.json.JSONArray;
	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using ContentValues = android.content.ContentValues;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;

	/// <summary>
	/// Engagement Poll abstraction.
	/// </summary>
	public class EngagementPoll : EngagementReachInteractiveContent
	{
	  /// <summary>
	  /// Intent poll action used by the reach SDK. </summary>
	  public const string INTENT_ACTION = "com.microsoft.azure.engagement.reach.intent.action.POLL";

	  /// <summary>
	  /// Questions as a bundle </summary>
	  private JSONArray mQuestions;

	  /// <summary>
	  /// Answer form </summary>
	  private readonly Bundle mAnswers;

	  /// <summary>
	  /// Parse a poll. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> campaign data. </param>
	  /// <exception cref="JSONException"> if a parsing error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementPoll(CampaignId campaignId, android.content.ContentValues values) throws org.json.JSONException
	  internal EngagementPoll(CampaignId campaignId, ContentValues values) : base(campaignId, values)
	  {
		mAnswers = new Bundle();
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override void setPayload(org.json.JSONObject payload) throws org.json.JSONException
	  internal override JSONObject Payload
	  {
		  set
		  {
			base.Payload = value;
			mQuestions = value.getJSONArray("questions");
		  }
	  }

	  internal override Intent buildIntent()
	  {
		Intent intent = new Intent(INTENT_ACTION);
		string category = Category;
		if (category != null)
		{
		  intent.addCategory(category);
		}
		return intent;
	  }

	  /// <summary>
	  /// Get questions for this poll as a JSON array. Each question is a JSON object with the following
	  /// structure:
	  /// <ul>
	  /// <li>"id" -> String</li>
	  /// <li>"title" -> String</li>
	  /// <li>"choices" -> JSONArray
	  /// <ul>
	  /// <li>"id" -> String
	  /// <li>"title" -> String</li>
	  /// <li>"isDefault" -> boolean (optional, default is false)</li>
	  /// </ul>
	  /// </ul>
	  /// </li> </ul> </li> </ul> </summary>
	  /// <returns> questions definition. </returns>
	  public virtual JSONArray Questions
	  {
		  get
		  {
			return mQuestions;
		  }
	  }

	  /// <summary>
	  /// Fill answer for a given question. Answers are sent when calling <seealso cref="#actionContent(Context)"/>
	  /// . </summary>
	  /// <param name="questionId"> question id as specified in the Bundle returned by <seealso cref="#getQuestions()"/>. </param>
	  /// <param name="choiceId"> choice id as specified in the Bundle returned by <seealso cref="#getQuestions()"/>. </param>
	  public virtual void fillAnswer(string questionId, string choiceId)
	  {
		mAnswers.putString(questionId, choiceId);
	  }

	  public override void actionContent(Context context)
	  {
		process(context, "content-actioned", mAnswers);
	  }
	}

}