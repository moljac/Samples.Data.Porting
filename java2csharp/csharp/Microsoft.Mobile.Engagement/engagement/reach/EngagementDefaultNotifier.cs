using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.app.PendingIntent.FLAG_CANCEL_CURRENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.content.Context.NOTIFICATION_SERVICE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.view.ViewGroup.LayoutParams.MATCH_PARENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachAgent.INTENT_ACTION_ACTION_NOTIFICATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachAgent.INTENT_ACTION_EXIT_NOTIFICATION;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachAgent.INTENT_EXTRA_COMPONENT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachAgent.INTENT_EXTRA_NOTIFICATION_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_AREA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_CLOSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_CLOSE_AREA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_ICON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_IMAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_OVERLAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_TEXT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.LAYOUT_NOTIFICATION_TITLE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.METADATA_NOTIFICATION_ICON;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachNotifications.METADATA_NOTIFICATION_OVERLAY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.v11.EngagementNotificationUtilsV11.scaleBitmapForLargeIcon;
	using Activity = android.app.Activity;
	using Notification = android.app.Notification;
	using NotificationManager = android.app.NotificationManager;
	using PendingIntent = android.app.PendingIntent;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using PackageManager = android.content.pm.PackageManager;
	using Bitmap = android.graphics.Bitmap;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using BigPictureStyle = android.support.v4.app.NotificationCompat.BigPictureStyle;
	using BigTextStyle = android.support.v4.app.NotificationCompat.BigTextStyle;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using LayoutParams = android.view.ViewGroup.LayoutParams;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;

	using EngagementNotificationUtilsV11 = com.microsoft.azure.engagement.reach.v11.EngagementNotificationUtilsV11;
	using EngagementResourcesUtils = com.microsoft.azure.engagement.utils.EngagementResourcesUtils;
	using EngagementUtils = com.microsoft.azure.engagement.utils.EngagementUtils;

	/// <summary>
	/// Notifier handling the default category. </summary>
	public class EngagementDefaultNotifier : EngagementNotifier
	{
	  /// <summary>
	  /// Application context </summary>
	  protected internal readonly Context mContext;

	  /// <summary>
	  /// Notification manager </summary>
	  private readonly NotificationManager mNotificationManager;

	  /// <summary>
	  /// Icon used in notification content </summary>
	  private readonly int mNotificationIcon;

	  /// <summary>
	  /// Init default notifier. </summary>
	  /// <param name="context"> any application context. </param>
	  public EngagementDefaultNotifier(Context context)
	  {
		/* Init */
		mContext = context.ApplicationContext;
		mNotificationManager = (NotificationManager) context.getSystemService(NOTIFICATION_SERVICE);

		/* Get icon identifiers from AndroidManifest.xml */
		Bundle appMetaData = EngagementUtils.getMetaData(context);
		mNotificationIcon = getIcon(appMetaData, METADATA_NOTIFICATION_ICON);
	  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public Boolean handleNotification(EngagementReachInteractiveContent content) throws RuntimeException
	  public virtual bool? handleNotification(EngagementReachInteractiveContent content)
	  {
		/* System notification case */
		if (content.SystemNotification)
		{
		  /* Big picture handling */
		  Bitmap bigPicture = null;
		  string bigPictureURL = content.NotificationBigPicture;
		  if (bigPictureURL != null && Build.VERSION.SDK_INT >= 16)
		  {
			/* Schedule picture download if needed, or load picture if download completed. */
			long? downloadId = content.DownloadId;
			if (downloadId == null)
			{
			  EngagementNotificationUtilsV11.downloadBigPicture(mContext, content);
			  return null;
			}
			else
			{
			  bigPicture = EngagementNotificationUtilsV11.getBigPicture(mContext, downloadId.Value);
			}
		  }

		  /* Generate notification identifier */
		  int notificationId = getNotificationId(content);

		  /* Build notification using support lib to manage compatibility with old Android versions */
		  NotificationCompat.Builder builder = new NotificationCompat.Builder(mContext);

		  /* Icon for ticker and content icon */
		  builder.SmallIcon = mNotificationIcon;

		  /*
		   * Large icon, handled only since API Level 11 (needs down scaling if too large because it's
		   * cropped otherwise by the system).
		   */
		  Bitmap notificationImage = content.NotificationImage;
		  if (notificationImage != null && Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB)
		  {
			builder.LargeIcon = scaleBitmapForLargeIcon(mContext, notificationImage);
		  }

		  /* Texts */
		  string notificationTitle = content.NotificationTitle;
		  string notificationMessage = content.NotificationMessage;
		  string notificationBigText = content.NotificationBigText;
		  builder.ContentTitle = notificationTitle;
		  builder.ContentText = notificationMessage;

		  /*
		   * Replay: display original date and don't replay all the tickers (be as quiet as possible
		   * when replaying).
		   */
		  long? notificationFirstDisplayedDate = content.NotificationFirstDisplayedDate;
		  if (notificationFirstDisplayedDate != null)
		  {
			builder.When = notificationFirstDisplayedDate;
		  }
		  else
		  {
			builder.Ticker = notificationTitle;
		  }

		  /* Big picture */
		  if (bigPicture != null)
		  {
			builder.Style = (new NotificationCompat.BigPictureStyle()).bigPicture(bigPicture).setBigContentTitle(notificationTitle).setSummaryText(notificationMessage);
		  }

		  /* Big text */
		  else if (notificationBigText != null)
		  {
			builder.Style = (new NotificationCompat.BigTextStyle()).bigText(notificationBigText);
		  }

		  /* Vibration/sound if not a replay */
		  if (notificationFirstDisplayedDate == null)
		  {
			int defaults = 0;
			if (content.NotificationSound)
			{
			  defaults |= Notification.DEFAULT_SOUND;
			}
			if (content.NotificationVibrate)
			{
			  defaults |= Notification.DEFAULT_VIBRATE;
			}
			builder.Defaults = defaults;
		  }

		  /* Launch the receiver on action */
		  Intent actionIntent = new Intent(INTENT_ACTION_ACTION_NOTIFICATION);
		  EngagementReachAgent.setContentIdExtra(actionIntent, content);
		  actionIntent.putExtra(INTENT_EXTRA_NOTIFICATION_ID, notificationId);
		  Intent intent = content.Intent;
		  if (intent != null)
		  {
			actionIntent.putExtra(INTENT_EXTRA_COMPONENT, intent.Component);
		  }
		  actionIntent.Package = mContext.PackageName;
		  PendingIntent contentIntent = PendingIntent.getBroadcast(mContext, (int) content.LocalId, actionIntent, FLAG_CANCEL_CURRENT);
		  builder.ContentIntent = contentIntent;

		  /* Also launch receiver if the notification is exited (clear button) */
		  Intent exitIntent = new Intent(INTENT_ACTION_EXIT_NOTIFICATION);
		  exitIntent.putExtra(INTENT_EXTRA_NOTIFICATION_ID, notificationId);
		  EngagementReachAgent.setContentIdExtra(exitIntent, content);
		  exitIntent.Package = mContext.PackageName;
		  PendingIntent deleteIntent = PendingIntent.getBroadcast(mContext, (int) content.LocalId, exitIntent, FLAG_CANCEL_CURRENT);
		  builder.DeleteIntent = deleteIntent;

		  /* Can be dismissed ? */
		  Notification notification = builder.build();
		  if (!content.NotificationCloseable)
		  {
			notification.flags |= Notification.FLAG_NO_CLEAR;
		  }

		  /* Allow overriding */
		  if (onNotificationPrepared(notification, content))

			/*
			 * Submit notification, replacing the previous one if any (this should happen only if the
			 * application process is restarted).
			 */
		  {
			mNotificationManager.notify(notificationId, notification);
		  }
		}

		/* Activity embedded notification case */
		else
		{
		  /* Get activity */
		  Activity activity = EngagementActivityManager.Instance.CurrentActivity.get();

		  /* Cannot notify in app if no activity provided */
		  if (activity == null)
		  {
			return false;
		  }

		  /* Get notification area */
		  string category = content.Category;
		  int areaId = getInAppAreaId(category).Value;
		  View notificationAreaView = activity.findViewById(areaId);

		  /* No notification area, check if we can install overlay */
		  if (notificationAreaView == null)
		  {
			/* Check overlay is not disabled in this activity */
			Bundle activityConfig = EngagementUtils.getActivityMetaData(activity);
			if (!activityConfig.getBoolean(METADATA_NOTIFICATION_OVERLAY, true))
			{
			  return false;
			}

			/* Inflate overlay layout and get reference to notification area */
			View overlay = LayoutInflater.from(mContext).inflate(getOverlayLayoutId(category), null);
			activity.addContentView(overlay, new LayoutParams(MATCH_PARENT, MATCH_PARENT));
			notificationAreaView = activity.findViewById(areaId);
		  }

		  /* Otherwise check if there is an overlay containing the area to restore visibility */
		  else
		  {
			View overlay = activity.findViewById(getOverlayViewId(category));
			if (overlay != null)
			{
			  overlay.Visibility = View.VISIBLE;
			}
		  }

		  /* Make the notification area visible */
		  notificationAreaView.Visibility = View.VISIBLE;

		  /* Prepare area */
		  prepareInAppArea(content, notificationAreaView);
		}

		/* Success */
		return true;
	  }

	  public virtual int? getOverlayViewId(string category)
	  {
		return getId(LAYOUT_NOTIFICATION_OVERLAY);
	  }

	  public virtual int? getInAppAreaId(string category)
	  {
		return getId(LAYOUT_NOTIFICATION_AREA);
	  }

	  public virtual void executeNotifAnnouncementAction(EngagementNotifAnnouncement notifAnnouncement)
	  {
		/* Launch action intent (view activity in its own task) */
		try
		{
		  Intent intent = Intent.parseUri(notifAnnouncement.ActionURL, 0);
		  intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK;
		  doExecuteNotifAnnouncementAction(notifAnnouncement, intent);
		}
		catch (Exception)
		{
		  /*
		   * Invalid/Missing Action URL: launch/resume application instead if system notification and no
		   * session.
		   */
		  if (notifAnnouncement.SystemNotification && EngagementActivityManager.Instance.CurrentActivityAlias == null)
		  {
			PackageManager packageManager = mContext.PackageManager;
			Intent intent = packageManager.getLaunchIntentForPackage(mContext.PackageName);
			if (intent != null)
			{
			  /*
			   * Set package null is the magic enabling the same behavior than launching from Home
			   * Screen, e.g. perfect resume of the task. No idea why the setPackage messes the intent
			   * up...
			   */
			  if (intent.Component != null)
			  {
				intent.Package = null;
			  }
			  intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK;
			  doExecuteNotifAnnouncementAction(notifAnnouncement, intent);
			}
		  }
		}
	  }

	  /// <summary>
	  /// This function is called when an overlay is about to be inflated. It returns the overlay layout
	  /// resource identifier (R.layout... not the view identifier) for the specified category. </summary>
	  /// <param name="category"> content category. </param>
	  /// <returns> overlay layout resource identifier. </returns>
	  protected internal virtual int getOverlayLayoutId(string category)
	  {
		return EngagementResourcesUtils.getLayoutId(mContext, LAYOUT_NOTIFICATION_OVERLAY);
	  }

	  /// <summary>
	  /// This function is called when the notification area view must be prepared, e.g. change texts,
	  /// icon etc... based on the specified content. This is the responsibility of this method to
	  /// associate actions to the buttons. </summary>
	  /// <param name="content"> content. </param>
	  /// <param name="notifAreaView"> notification area view. </param>
	  /// <exception cref="RuntimeException"> on any error the content will be dropped. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected void prepareInAppArea(final EngagementReachInteractiveContent content, android.view.View notifAreaView) throws RuntimeException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
	  protected internal virtual void prepareInAppArea(EngagementReachInteractiveContent content, View notifAreaView)
	  {
		/* Set icon */
		ImageView iconView = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_ICON);
		if (content.hasNotificationIcon())
		{
		  iconView.Visibility = View.VISIBLE;
		  iconView.ImageResource = mNotificationIcon;
		}
		else
		{
		  iconView.Visibility = View.GONE;
		}

		/* Set title and message */
		View textArea = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_TEXT);
		if (content.NotificationTitle == null && content.NotificationMessage == null)
		{
		  textArea.Visibility = View.GONE;
		}
		else
		{
		  /* Show text area */
		  textArea.Visibility = View.VISIBLE;

		  /* Title */
		  TextView titleView = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_TITLE);
		  if (content.NotificationTitle == null)
		  {
			titleView.Visibility = View.GONE;
		  }
		  else
		  {
			titleView.Visibility = View.VISIBLE;
			titleView.Text = content.NotificationTitle;
		  }

		  /* Message */
		  TextView messageView = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_MESSAGE);
		  if (content.NotificationMessage == null)
		  {
			messageView.Visibility = View.GONE;
		  }
		  else
		  {
			messageView.Visibility = View.VISIBLE;
			messageView.Text = content.NotificationMessage;
		  }
		}

		/* Set image */
		ImageView imageView = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_IMAGE);
		Bitmap notificationImage = content.NotificationImage;
		if (notificationImage == null)
		{
		  imageView.Visibility = View.GONE;
		}
		else
		{
		  imageView.Visibility = View.VISIBLE;
		  imageView.ImageBitmap = notificationImage;
		}

		/* Set intent action */
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View notificationAreaViewFinal = notifAreaView;
		View notificationAreaViewFinal = notifAreaView;
		notifAreaView.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, content, notificationAreaViewFinal);

		/*
		 * Configure close button if not removed from layout (it was not mandatory in previous Reach SDK
		 * version).
		 */
		View closeButton = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_CLOSE);
		if (closeButton != null)
		{
		  /* Set close action if closeable */
		  if (content.NotificationCloseable)
		  {
			closeButton.Visibility = View.VISIBLE;
			closeButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this, content, notificationAreaViewFinal);
		  }

		  /* Otherwise hide the close button */
		  else
		  {
			closeButton.Visibility = View.GONE;
		  }
		}

		/*
		 * This optional view is used to ensure that the close button does not overlap the image or
		 * text, this is an invisible area. If we hide the text area in the provided layout, the close
		 * button area won't be right aligned, that's why we have both an invisible button and an actual
		 * button that is always right aligned. If you know a way to avoid that without breaking other
		 * possible layouts (every text or icon is optional), please contact us.
		 */
		View closeButtonArea = EngagementResourcesUtils.getView(notifAreaView, LAYOUT_NOTIFICATION_CLOSE_AREA);
		if (closeButtonArea != null)
		{
		  if (content.NotificationCloseable)
		  {
			closeButtonArea.Visibility = View.INVISIBLE;
		  }
		  else
		  {
			closeButtonArea.Visibility = View.GONE;
		  }
		}
	  }

	  private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
	  {
		  private readonly EngagementDefaultNotifier outerInstance;

		  private com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent content;
		  private View notificationAreaViewFinal;

		  public OnClickListenerAnonymousInnerClassHelper(EngagementDefaultNotifier outerInstance, com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent content, View notificationAreaViewFinal)
		  {
			  this.outerInstance = outerInstance;
			  this.content = content;
			  this.notificationAreaViewFinal = notificationAreaViewFinal;
		  }

		  public override void onClick(View v)
		  {
			notificationAreaViewFinal.Visibility = View.GONE;
			content.actionNotification(outerInstance.mContext, true);
		  }
	  }

	  private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
	  {
		  private readonly EngagementDefaultNotifier outerInstance;

		  private com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent content;
		  private View notificationAreaViewFinal;

		  public OnClickListenerAnonymousInnerClassHelper2(EngagementDefaultNotifier outerInstance, com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent content, View notificationAreaViewFinal)
		  {
			  this.outerInstance = outerInstance;
			  this.content = content;
			  this.notificationAreaViewFinal = notificationAreaViewFinal;
		  }

		  public override void onClick(View v)
		  {
			notificationAreaViewFinal.Visibility = View.GONE;
			content.exitNotification(outerInstance.mContext);
		  }
	  }

	  public virtual int getNotificationId(EngagementReachInteractiveContent content)
	  {
		return ("engagement:reach:" + content.LocalId).GetHashCode();
	  }

	  /// <summary>
	  /// This method is called just before the notification is submitted. You can override this method
	  /// to apply more customization like using the LED or a specific sound. </summary>
	  /// <param name="notification"> prepared notification that can be modified. </param>
	  /// <param name="content"> content to be notified. </param>
	  /// <exception cref="RuntimeException"> on any error, content will be silently dropped. </exception>
	  /// <returns> true to let the system notification being notified right after this call, false to
	  ///         manage it yourself. </returns>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected boolean onNotificationPrepared(android.app.Notification notification, EngagementReachInteractiveContent content) throws RuntimeException
	  protected internal virtual bool onNotificationPrepared(Notification notification, EngagementReachInteractiveContent content)
	  {
		return true;
	  }

	  /// <summary>
	  /// This method is called just before starting the intent associated to the click on a notification
	  /// only announcement: execute the action URL or launch application if no URL and clicked from a
	  /// system notification while application is in background. You can change flags, return a brand
	  /// new intent or return null to cancel the intent and manage it yourself. </summary>
	  /// <param name="notifAnnouncement"> notification only announcement. </param>
	  /// <param name="intent"> prepared intent. </param>
	  /// <returns> intent to launch, pass null to handle launching yourself. </returns>
	  protected internal virtual Intent onNotifAnnouncementIntentPrepared(EngagementNotifAnnouncement notifAnnouncement, Intent intent)
	  {
		return intent;
	  }

	  /// <summary>
	  /// Get resource identifier from its name. </summary>
	  /// <param name="name"> resource name. </param>
	  /// <returns> resource identifier. </returns>
	  private int getId(string name)
	  {
		return EngagementResourcesUtils.getId(mContext, name);
	  }

	  /// <summary>
	  /// Get drawable resource identifier from application meta-data. </summary>
	  /// <param name="appMetaData"> application meta-data. </param>
	  /// <param name="metaName"> meta-data key corresponding to the drawable resource name. </param>
	  /// <returns> drawable resource identifier or 0 if not found. </returns>
	  private int getIcon(Bundle appMetaData, string metaName)
	  {
		/* Get drawable resource identifier from its name */
		string iconName = appMetaData.getString(metaName);
		if (iconName != null)
		{
		  return EngagementResourcesUtils.getDrawableId(mContext, iconName);
		}
		return 0;
	  }

	  /// <summary>
	  /// Just common code used in <seealso cref="#executeNotifAnnouncementAction(EngagementNotifAnnouncement)"/> </summary>
	  private void doExecuteNotifAnnouncementAction(EngagementNotifAnnouncement notifAnnouncement, Intent intent)
	  {
		intent = onNotifAnnouncementIntentPrepared(notifAnnouncement, intent);
		if (intent != null)
		{
		  mContext.startActivity(intent);
		}
	  }
	}

}