/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.CATEGORY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DLC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DLC_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.PAYLOAD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.TTL;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.USER_TIME_ZONE;

	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using ContentValues = android.content.ContentValues;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;

	/// <summary>
	/// Abstract class for reach content such as announcements and polls. </summary>
	public abstract class EngagementReachContent
	{
	  /// <summary>
	  /// If set, the notification has downloadble content </summary>
	  public const int FLAG_DLC_NOTIFICATION = 1;

	  /// <summary>
	  /// If set, the clickable content exists </summary>
	  public const int FLAG_DLC_CONTENT = 2;

	  /// <summary>
	  /// Local identifier (which is a SQLite auto-increment) </summary>
	  private long mLocalID;

	  /// <summary>
	  /// DLC flags </summary>
	  private readonly int mDlc;

	  /// <summary>
	  /// DLC id </summary>
	  private readonly string mDlcId;

	  /// <summary>
	  /// Campaign id </summary>
	  internal readonly CampaignId mCampaignId;

	  /// <summary>
	  /// Category, null for default </summary>
	  internal readonly string mCategory;

	  /// <summary>
	  /// Body </summary>
	  internal string mBody;

	  /// <summary>
	  /// Expiry timestamp (ms since epoch), if any </summary>
	  private readonly long? mExpiry;

	  /// <summary>
	  /// Intent that must be launched for viewing this content </summary>
	  private Intent mIntent;

	  /// <summary>
	  /// True if replied, thus processed entirely, to avoid making that several times </summary>
	  private bool mProcessed;

	  /// <summary>
	  /// Download identifier </summary>
	  private long? mDownloadId;

	  /// <summary>
	  /// Flag to remember DLC completes </summary>
	  private bool mDlcCompleted;

	  /// <summary>
	  /// Parse a campaign. </summary>
	  /// <param name="campaignId"> already parsed campaign id. </param>
	  /// <param name="values"> content data. </param>
	  /// <exception cref="JSONException"> if payload parsing failure. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementReachContent(CampaignId campaignId, android.content.ContentValues values) throws org.json.JSONException
	  internal EngagementReachContent(CampaignId campaignId, ContentValues values)
	  {
		/* Parse base fields */
		mCampaignId = campaignId;
		mDlc = values.getAsInteger(DLC);
		mDlcId = values.getAsString(DLC_ID);
		mCategory = values.getAsString(CATEGORY);
		long? expiry = values.getAsLong(TTL);
		if (expiry != null)
		{
		  expiry *= 1000L;
		  if (parseBoolean(values, USER_TIME_ZONE))
		  {
			expiry -= TimeZone.Default.getOffset(expiry);
		  }
		}
		mExpiry = expiry;
		if (values.containsKey(PAYLOAD))
		{
		  Payload = new JSONObject(values.getAsString(PAYLOAD));
		}
	  }

	  /// <summary>
	  /// Parse boolean from content values. </summary>
	  /// <param name="values"> content values. </param>
	  /// <param name="key"> key. </param>
	  /// <returns> boolean value. </returns>
	  internal static bool parseBoolean(ContentValues values, string key)
	  {
		int? val = values.getAsInteger(key);
		return val != null && val == 1;
	  }

	  /// <summary>
	  /// Get local identifier. </summary>
	  /// <returns> the local identifier. </returns>
	  public virtual long LocalId
	  {
		  get
		  {
			return mLocalID;
		  }
		  set
		  {
			/* Remember id */
			mLocalID = value;
		  }
	  }


	  /// <returns> campaign id </returns>
	  public virtual CampaignId CampaignId
	  {
		  get
		  {
			return mCampaignId;
		  }
	  }

	  /// <returns> true if a download is necessary </returns>
	  public virtual bool hasDLC()
	  {
		return mDlc > 0;
	  }

	  /// <returns> true if a download is necessary to display notification </returns>
	  public virtual bool hasNotificationDLC()
	  {
		return (mDlc & FLAG_DLC_NOTIFICATION) == FLAG_DLC_NOTIFICATION;
	  }

	  /// <returns> true if a download is necessary after notification click </returns>
	  public virtual bool hasContentDLC()
	  {
		return (mDlc & FLAG_DLC_CONTENT) == FLAG_DLC_CONTENT;
	  }

	  /// <returns> DLC id </returns>
	  public virtual string DlcId
	  {
		  get
		  {
			return mDlcId;
		  }
	  }

	  /// <summary>
	  /// Set payload </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: void setPayload(org.json.JSONObject payload) throws org.json.JSONException
	  internal virtual JSONObject Payload
	  {
		  set
		  {
			/* Read field */
			mDlcCompleted = true;
			mBody = value.optString("body", null);
		  }
	  }

	  /// <summary>
	  /// Check if DLC completed. </summary>
	  /// <returns> true if DLC completed. </returns>
	  public virtual bool DlcCompleted
	  {
		  get
		  {
			return mDlcCompleted;
		  }
	  }

	  /// <summary>
	  /// Get the category of this content. This category is also added in the intent that launches the
	  /// viewing activity if any. If this method returns <tt>null</tt>, the category list in the intent
	  /// will only contain <tt>android.intent.category.DEFAULT</tt>. </summary>
	  /// <returns> category. </returns>
	  public virtual string Category
	  {
		  get
		  {
			return mCategory;
		  }
	  }

	  /// <summary>
	  /// Get content's body. </summary>
	  /// <returns> content's body. </returns>
	  public virtual string Body
	  {
		  get
		  {
			return mBody;
		  }
	  }

	  /// <summary>
	  /// Get the base intent to launch to view this content. The intent will be filtered with a
	  /// component name. </summary>
	  /// <returns> the base intent to launch to view this content. </returns>
	  public virtual Intent Intent
	  {
		  get
		  {
			if (mIntent == null && mDlcCompleted)
			{
			  mIntent = buildIntent();
			  if (mIntent != null)
			  {
				EngagementReachAgent.setContentIdExtra(mIntent, this);
			  }
			}
			return mIntent;
		  }
	  }

	  /// <summary>
	  /// Build the intent to launch to view this content. </summary>
	  /// <returns> the intent to launch to view this content. </returns>
	  internal abstract Intent buildIntent();

	  /// <summary>
	  /// Get content expiration date (in ms since epoch). </summary>
	  /// <returns> content expiration date or null if not specified. </returns>
	  public virtual long? Expiry
	  {
		  get
		  {
			return mExpiry;
		  }
	  }

	  /// <summary>
	  /// Check if this content is now expired. </summary>
	  /// <returns> true iff this content is expired now. </returns>
	  public virtual bool hasExpired()
	  {
		return mExpiry != null && DateTimeHelperClass.CurrentUnixTimeMillis() >= mExpiry;
	  }

	  /// <summary>
	  /// Drop content. </summary>
	  /// <param name="context"> any application context. </param>
	  public virtual void dropContent(Context context)
	  {
		process(context, "dropped", null);
	  }

	  /// <summary>
	  /// Report content has been actioned. </summary>
	  /// <param name="context"> any application context. </param>
	  public virtual void actionContent(Context context)
	  {
		process(context, "content-actioned", null);
	  }

	  /// <summary>
	  /// Report content been exited. </summary>
	  /// <param name="context"> any application context. </param>
	  public virtual void exitContent(Context context)
	  {
		process(context, "content-exited", null);
	  }

	  /// <summary>
	  /// Check whether this content has a system notification. </summary>
	  /// <returns> true iff the content has a system notification. </returns>
	  public virtual bool SystemNotification
	  {
		  get
		  {
			/* By default */
			return false;
		  }
	  }

	  /// <summary>
	  /// Dispose of this content so that new content can be notified. Possibly send feedback to the
	  /// service that sent it. </summary>
	  /// <param name="context"> application context. </param>
	  /// <param name="status"> feedback status if any (null not to send anything). </param>
	  /// <param name="extras"> extra information like poll answers. </param>
	  internal virtual void process(Context context, string status, Bundle extras)
	  {
		/* Do it once */
		if (!mProcessed)
		{
		  /* Send feedback if any */
		  if (status != null)
		  {
			mCampaignId.sendFeedBack(context, status, extras);
		  }

		  /* Mark this announcement as processed */
		  mProcessed = true;

		  /* Tell the reach application manager that announcement has been processed */
		  EngagementReachAgent.getInstance(context).onContentProcessed(this);
		}
	  }

	  /// <summary>
	  /// Set content status. </summary>
	  /// <param name="values"> values from storage. </param>
	  internal virtual ContentValues State
	  {
		  set
		  {
			mDownloadId = value.getAsLong(ContentStorage.DOWNLOAD_ID);
		  }
	  }

	  /// <summary>
	  /// Get download identifier if a download has ever been scheduled for this content. </summary>
	  /// <returns> download identifier or null if download has never been scheduled. </returns>
	  public virtual long? DownloadId
	  {
		  get
		  {
			return mDownloadId;
		  }
	  }

	  /// <summary>
	  /// Set download identifier for this content. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="downloadId"> download identifier. </param>
	  public virtual void setDownloadId(Context context, long downloadId)
	  {
		mDownloadId = downloadId;
		EngagementReachAgent.getInstance(context).onDownloadScheduled(this, downloadId);
	  }
	}

}