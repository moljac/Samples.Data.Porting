/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using Notification = android.app.Notification;
	using NotificationManager = android.app.NotificationManager;

	/// <summary>
	/// Custom notifier specification.<br/>
	/// You can define how a content notification is done for a set of categories by implementing this
	/// class and registering your instances by calling
	/// <seealso cref="EngagementReachAgent#registerNotifier(EngagementNotifier, String...)"/>.<br/>
	/// It is recommended to extend the default implementation: <seealso cref="EngagementDefaultNotifier"/> which
	/// performs most of the work and has convenient callbacks.
	/// </summary>
	public interface EngagementNotifier
	{
	  /// <summary>
	  /// Handle a notification for a content. </summary>
	  /// <param name="content"> content to be notified. </param>
	  /// <returns> true to accept the content, false to postpone the content (like overlay disabled in a
	  ///         specific activity).<br/>
	  ///         You can also return null to accept the content but not reporting the notification as
	  ///         displayed yet, this is generally used when a system notification needs some background
	  ///         task to be completed before it can be submitted (like downloading a big picture). null
	  ///         can also be returned for in app notifications, in that case, the Reach agent will stop
	  ///         trying to display that notification on activity changes. When returning null you are
	  ///         responsible for calling
	  ///         <seealso cref="EngagementReachAgent#notifyPendingContent(EngagementReachInteractiveContent)"/>
	  ///         once the notification is ready to be processed again. </returns>
	  /// <exception cref="RuntimeException"> on any error, the content is dropped. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public Boolean handleNotification(EngagementReachInteractiveContent content) throws RuntimeException;
	  bool? handleNotification(EngagementReachInteractiveContent content);

	  /// <summary>
	  /// The Reach SDK needs to control overlays visibility for in-app notifications. When notifiers
	  /// customize overlays, they must provide a view identifier for each category they manage by
	  /// implementing this function. The same identifier can be used for several categories but all
	  /// notifications of a specified category must use the same overlay identifier. </summary>
	  /// <param name="category"> category. </param>
	  /// <returns> overlay view identifier, can be null if overlays are not used in this notifier for the
	  ///         specified category (for example they use dialogs, toasts or widgets). </returns>
	  int? getOverlayViewId(string category);

	  /// <summary>
	  /// The Reach SDK needs to control notification area visibility for in-app notifications (an
	  /// overlay may not be used like an embedded notification area in a list activity). When notifiers
	  /// customize notification areas, they must provide a view identifier for each category they manage
	  /// by implementing this function. The same identifier can be used for several categories but all
	  /// notifications of a specified category must use the same notification area view identifier. </summary>
	  /// <param name="category"> category. </param>
	  /// <returns> area view identifier, can be null if notification areas are not used in this notifier
	  ///         for the specified category (for example they use dialogs, toasts or widgets). </returns>
	  int? getInAppAreaId(string category);

	  /// <summary>
	  /// This method is called while a system notification is being built or canceled. You can override
	  /// this method to specify the identifier that will be used when calling
	  /// <seealso cref="NotificationManager#notify(int, Notification)"/>. </summary>
	  /// <param name="content"> content to be notified. </param>
	  /// <returns> system notification identifier. </returns>
	  int getNotificationId(EngagementReachInteractiveContent content);

	  /// <summary>
	  /// Called when a notification only announcement is clicked. Implementor is supposed to execute
	  /// action URL if specified or provide a default action in some scenarii otherwise (like launching
	  /// application if the notification is a system one and application is in background). </summary>
	  /// <param name="notifAnnouncement"> notification only announcement. </param>
	  void executeNotifAnnouncementAction(EngagementNotifAnnouncement notifAnnouncement);
	}

}