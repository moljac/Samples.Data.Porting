using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.CONTENT_DISPLAYED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.DELIVERY_TIME;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_ACTIONED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_BIG_PICTURE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_BIG_TEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_CLOSEABLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_FIRST_DISPLAYED_DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_ICON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_LAST_DISPLAYED_DATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_SOUND;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_TITLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.ContentStorage.NOTIFICATION_VIBRATION;


	using JSONArray = org.json.JSONArray;
	using JSONException = org.json.JSONException;

	using ContentValues = android.content.ContentValues;
	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Base64 = android.util.Base64;

	/// <summary>
	/// Common class to announcements and polls </summary>
	public abstract class EngagementReachInteractiveContent : EngagementReachContent
	{
	  /// <summary>
	  /// Title </summary>
	  private string mTitle;

	  /// <summary>
	  /// Action's label </summary>
	  private string mActionLabel;

	  /// <summary>
	  /// Exit label </summary>
	  private string mExitLabel;

	  /// <summary>
	  /// True if this content has a notification that must be set in status bar </summary>
	  private readonly bool mSystemNotification;

	  /// <summary>
	  /// Does the notification have a resource icon in notification content? </summary>
	  private readonly bool mNotificationIcon;

	  /// <summary>
	  /// Can the notification be closed ? </summary>
	  private readonly bool mNotiticationCloseable;

	  /// <summary>
	  /// Make the telephone ring ? </summary>
	  private readonly bool mNotificationSound;

	  /// <summary>
	  /// Make the telephone vibrate ? </summary>
	  private readonly bool mNotificationVibrate;

	  /// <summary>
	  /// Notification's title </summary>
	  private readonly string mNotificationTitle;

	  /// <summary>
	  /// Notification's message </summary>
	  private readonly string mNotificationMessage;

	  /// <summary>
	  /// Notification's big text </summary>
	  private readonly string mNotificationBigText;

	  /// <summary>
	  /// Notification's big picture </summary>
	  private readonly string mNotificationBigPicture;

	  /// <summary>
	  /// Notification image base64 string </summary>
	  private string mNotificationImageString;

	  /// <summary>
	  /// Notification image bitmap null until requested </summary>
	  private Bitmap mNotificationImage;

	  /// <summary>
	  /// Behavior types </summary>
	  private enum Behavior
	  {
		ANYTIME,
		SESSION
	  }

	  /// <summary>
	  /// Behavior </summary>
	  private readonly Behavior mBehavior;

	  /// <summary>
	  /// Activities to restrict this content </summary>
	  private ISet<string> mAllowedActivities;

	  /// <summary>
	  /// True if notification actioned </summary>
	  private bool mNotificationActioned;

	  /// <summary>
	  /// Get date at which the notification was displayed the first time </summary>
	  private long? mNotificationFirstDisplayedDate;

	  /// <summary>
	  /// Get date at which the notification was displayed the last time </summary>
	  private long? mNotificationLastDisplayedDate;

	  /// <summary>
	  /// True if content displayed </summary>
	  private bool mContentDisplayed;

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: EngagementReachInteractiveContent(CampaignId campaignId, android.content.ContentValues values) throws org.json.JSONException
	  internal EngagementReachInteractiveContent(CampaignId campaignId, ContentValues values) : base(campaignId, values)
	  {
		/* Base fields */

		/* Behavior */
		string deliveryTime = values.getAsString(DELIVERY_TIME);
		if (deliveryTime.Equals("s"))
		{
		  mBehavior = Behavior.SESSION;
		}
		else
		{
		  mBehavior = Behavior.ANYTIME;
		}

		/* Notification type */
		mSystemNotification = "s".Equals(values.getAsString(NOTIFICATION_TYPE));

		/* Is notification closeable? */
		mNotiticationCloseable = parseBoolean(values, NOTIFICATION_CLOSEABLE);

		/* Has notification icon? */
		mNotificationIcon = parseBoolean(values, NOTIFICATION_ICON);

		/* Sound and vibration */
		mNotificationSound = parseBoolean(values, NOTIFICATION_SOUND);
		mNotificationVibrate = parseBoolean(values, NOTIFICATION_VIBRATION);

		/* Parse texts */
		mNotificationTitle = values.getAsString(NOTIFICATION_TITLE);
		mNotificationMessage = values.getAsString(NOTIFICATION_MESSAGE);

		/* Big text */
		mNotificationBigText = values.getAsString(NOTIFICATION_BIG_TEXT);

		/* Big picture */
		mNotificationBigPicture = values.getAsString(NOTIFICATION_BIG_PICTURE);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override void setPayload(org.json.JSONObject payload) throws org.json.JSONException
	  internal override org.json.JSONObject Payload
	  {
		  set
		  {
			/* Get parent fields */
			base.Payload = value;
    
			/* Get image data, we decode the bitmap in a lazy way */
			mNotificationImageString = value.optString("notificationImage", null);
    
			/* Delivery activities */
			JSONArray deliveryActivities = value.optJSONArray("deliveryActivities");
			if (deliveryActivities != null)
			{
			  mAllowedActivities = new HashSet<string>();
			  for (int i = 0; i < deliveryActivities.length(); i++)
			  {
				mAllowedActivities.Add(deliveryActivities.getString(i));
			  }
			}
    
			/* Get body related fields */
			mTitle = value.optString("title", null);
			mActionLabel = value.optString("actionButtonText", null);
			mExitLabel = value.optString("exitButtonText", null);
		  }
	  }

	  /// <summary>
	  /// Get content's title. </summary>
	  /// <returns> content's title </returns>
	  public virtual string Title
	  {
		  get
		  {
			return mTitle;
		  }
	  }

	  /// <summary>
	  /// Get action's label. </summary>
	  /// <returns> action's label. </returns>
	  public virtual string ActionLabel
	  {
		  get
		  {
			return mActionLabel;
		  }
	  }

	  /// <summary>
	  /// Get exit label. </summary>
	  /// <returns> exit label. </returns>
	  public virtual string ExitLabel
	  {
		  get
		  {
			return mExitLabel;
		  }
	  }

	  /// <summary>
	  /// Check whether this content has a system notification. </summary>
	  /// <returns> true if this content has to be notified in the status bar, false to embed the
	  ///         notification in activity. </returns>
	  public override bool SystemNotification
	  {
		  get
		  {
			return mSystemNotification;
		  }
	  }

	  /// <summary>
	  /// Check whether the notification can be closed without looking at the content. </summary>
	  /// <returns> true if the notification can be closed without looking at the content, false otherwise. </returns>
	  public virtual bool NotificationCloseable
	  {
		  get
		  {
			return mNotiticationCloseable;
		  }
	  }

	  /// <summary>
	  /// Check whether the notification has a resource icon in notification content. </summary>
	  /// <returns> true if the notification has a resource icon in notification content, false otherwise. </returns>
	  public virtual bool hasNotificationIcon()
	  {
		return mNotificationIcon;
	  }

	  /// <summary>
	  /// Check whether the notification makes the telephone ring. </summary>
	  /// <returns> true iff the notification makes the telephone ring. </returns>
	  public virtual bool NotificationSound
	  {
		  get
		  {
			return mNotificationSound;
		  }
	  }

	  /// <summary>
	  /// Check whether the notification makes the telephone vibrate. </summary>
	  /// <returns> true iff the notification makes the telephone vibrate. </returns>
	  public virtual bool NotificationVibrate
	  {
		  get
		  {
			return mNotificationVibrate;
		  }
	  }

	  /// <summary>
	  /// Get notification's title. </summary>
	  /// <returns> notification's title </returns>
	  public virtual string NotificationTitle
	  {
		  get
		  {
			return mNotificationTitle;
		  }
	  }

	  /// <summary>
	  /// Get notification's message. </summary>
	  /// <returns> notification's message. </returns>
	  public virtual string NotificationMessage
	  {
		  get
		  {
			return mNotificationMessage;
		  }
	  }

	  /// <summary>
	  /// Get notification big text message (displayed only on Android 4.1+). </summary>
	  /// <returns> notification's big text message. </returns>
	  public virtual string NotificationBigText
	  {
		  get
		  {
			return mNotificationBigText;
		  }
	  }

	  /// <summary>
	  /// Get notification big picture URL (displayed only on Android 4.1+). </summary>
	  /// <returns> notification big picture URL. </returns>
	  public virtual string NotificationBigPicture
	  {
		  get
		  {
			return mNotificationBigPicture;
		  }
	  }

	  /// <summary>
	  /// Get notification image for in app notifications. For system notification this field corresponds
	  /// to the large icon (displayed only on Android 3+). </summary>
	  /// <returns> notification image. </returns>
	  public virtual Bitmap NotificationImage
	  {
		  get
		  {
			/* Decode as bitmap now if not already done */
			if (mNotificationImageString != null && mNotificationImage == null)
			{
			  /* Decode base 64 then decode as a bitmap */
			  sbyte[] data = Base64.decode(mNotificationImageString, Base64.DEFAULT);
			  if (data != null)
			  {
				try
				{
				  mNotificationImage = BitmapFactory.decodeByteArray(data, 0, data.Length);
				}
				catch (System.OutOfMemoryException)
				{
				  /* Abort */
				}
			  }
    
			  /* On any error, don't retry next time */
			  if (mNotificationImage == null)
			  {
				mNotificationImageString = null;
			  }
			}
			return mNotificationImage;
		  }
	  }

	  /// <summary>
	  /// Test if this content can be notified in the current UI context. </summary>
	  /// <param name="activity"> current activity name, null if no current activity. </param>
	  /// <returns> true if this content can be notified in the current UI context. </returns>
	  internal virtual bool canNotify(string activity)
	  {
		/*
		 * If the system notification has already been displayed, always allows to replay it (for
		 * example at boot) disregarding U.I. context. A system notification remains visible even if you
		 * leave U.I. context that triggered it so it makes sense to replay it (would be weird to replay
		 * it only when U.I context is triggered again which can happen very late or never).
		 */
		if (mSystemNotification && mNotificationFirstDisplayedDate != null)
		{
		  return true;
		}

		/* Otherwise it depends on current UI context and this campaign settings */
		switch (mBehavior)
		{
		  case com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent.Behavior.ANYTIME:
			return mSystemNotification || activity != null;

		  case com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent.Behavior.SESSION:
			return activity != null && (mAllowedActivities == null || mAllowedActivities.Contains(activity));
		}
		return false;
	  }

	  internal override ContentValues State
	  {
		  set
		  {
			base.State = value;
			this.mNotificationActioned = parseBoolean(value, NOTIFICATION_ACTIONED);
			this.mNotificationFirstDisplayedDate = value.getAsLong(NOTIFICATION_FIRST_DISPLAYED_DATE);
			this.mNotificationLastDisplayedDate = value.getAsLong(NOTIFICATION_LAST_DISPLAYED_DATE);
			this.mContentDisplayed = parseBoolean(value, CONTENT_DISPLAYED);
		  }
	  }

	  /// <summary>
	  /// Get a status prefix equals to "in-app-notification-" or "system-notification-", depending of
	  /// this notification type. </summary>
	  /// <returns> "in-app-notification-" or "system-notification-". </returns>
	  internal virtual string NotificationStatusPrefix
	  {
		  get
		  {
			string status;
			if (SystemNotification)
			{
			  status = "system";
			}
			else
			{
			  status = "in-app";
			}
			status += "-notification-";
			return status;
		  }
	  }

	  /// <summary>
	  /// Report notification has been displayed. </summary>
	  /// <param name="context"> any application context. </param>
	  public virtual void displayNotification(Context context)
	  {
		/* Update last displayed date */
		mNotificationLastDisplayedDate = DateTimeHelperClass.CurrentUnixTimeMillis();

		/* First date and reach feedback the first time */
		if (mNotificationFirstDisplayedDate == null)
		{
		  mNotificationFirstDisplayedDate = mNotificationLastDisplayedDate;
		  mCampaignId.sendFeedBack(context, NotificationStatusPrefix + "displayed", null);
		}

		/* Notify reach agent */
		EngagementReachAgent.getInstance(context).onNotificationDisplayed(this);
	  }

	  /// <summary>
	  /// Get time at which the notification was displayed the first time or null if not yet displayed. </summary>
	  /// <returns> time in ms since epoch or null. </returns>
	  public virtual long? NotificationFirstDisplayedDate
	  {
		  get
		  {
			return mNotificationFirstDisplayedDate;
		  }
	  }

	  /// <summary>
	  /// Get time at which the notification was displayed the last time or null if not yet displayed. </summary>
	  /// <returns> time in ms since epoch or null. </returns>
	  public virtual long? NotificationLastDisplayedDate
	  {
		  get
		  {
			return mNotificationLastDisplayedDate;
		  }
	  }

	  /// <summary>
	  /// Action the notification: this will display the announcement or poll, or will launch the action
	  /// URL associated to the notification, depending of the content kind. This will also report the
	  /// notification has been actioned. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="launchIntent"> true to launch intent, false to just report the notification action and
	  ///          change internal state. If you call this method passing false, be sure that the content
	  ///          is either a notification only announcement or that you properly manage the content
	  ///          display and its life cycle (by calling actionContent or exitContent when the user is
	  ///          done viewing the content). </param>
	  public virtual void actionNotification(Context context, bool launchIntent)
	  {
		/* Notify agent if intent must be launched */
		EngagementReachAgent.getInstance(context).onNotificationActioned(this, launchIntent);

		/* Send feedback */
		if (!mNotificationActioned)
		{
		  mCampaignId.sendFeedBack(context, NotificationStatusPrefix + "actioned", null);
		  mNotificationActioned = true;
		}
	  }

	  /// <summary>
	  /// Report notification has been exited. </summary>
	  /// <param name="context"> any application context. </param>
	  public virtual void exitNotification(Context context)
	  {
		process(context, NotificationStatusPrefix + "exited", null);
	  }

	  /// <summary>
	  /// Report content has been displayed. </summary>
	  /// <param name="context"> any application context. </param>
	  public virtual void displayContent(Context context)
	  {
		/* Notify reach agent */
		EngagementReachAgent.getInstance(context).onContentDisplayed(this);

		/* Guard against multiple calls for feedback */
		if (!mContentDisplayed)
		{
		  mCampaignId.sendFeedBack(context, "content-displayed", null);
		  mContentDisplayed = true;
		}
	  }
	}

}