/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import android.app.Notification;
import android.app.NotificationManager;

/**
 * Custom notifier specification.<br/>
 * You can define how a content notification is done for a set of categories by implementing this
 * class and registering your instances by calling
 * {@link EngagementReachAgent#registerNotifier(EngagementNotifier, String...)}.<br/>
 * It is recommended to extend the default implementation: {@link EngagementDefaultNotifier} which
 * performs most of the work and has convenient callbacks.
 */
public interface EngagementNotifier
{
  /**
   * Handle a notification for a content.
   * @param content content to be notified.
   * @return true to accept the content, false to postpone the content (like overlay disabled in a
   *         specific activity).<br/>
   *         You can also return null to accept the content but not reporting the notification as
   *         displayed yet, this is generally used when a system notification needs some background
   *         task to be completed before it can be submitted (like downloading a big picture). null
   *         can also be returned for in app notifications, in that case, the Reach agent will stop
   *         trying to display that notification on activity changes. When returning null you are
   *         responsible for calling
   *         {@link EngagementReachAgent#notifyPendingContent(EngagementReachInteractiveContent)}
   *         once the notification is ready to be processed again.
   * @throws RuntimeException on any error, the content is dropped.
   */
  public Boolean handleNotification(EngagementReachInteractiveContent content)
    throws RuntimeException;

  /**
   * The Reach SDK needs to control overlays visibility for in-app notifications. When notifiers
   * customize overlays, they must provide a view identifier for each category they manage by
   * implementing this function. The same identifier can be used for several categories but all
   * notifications of a specified category must use the same overlay identifier.
   * @param category category.
   * @return overlay view identifier, can be null if overlays are not used in this notifier for the
   *         specified category (for example they use dialogs, toasts or widgets).
   */
  public Integer getOverlayViewId(String category);

  /**
   * The Reach SDK needs to control notification area visibility for in-app notifications (an
   * overlay may not be used like an embedded notification area in a list activity). When notifiers
   * customize notification areas, they must provide a view identifier for each category they manage
   * by implementing this function. The same identifier can be used for several categories but all
   * notifications of a specified category must use the same notification area view identifier.
   * @param category category.
   * @return area view identifier, can be null if notification areas are not used in this notifier
   *         for the specified category (for example they use dialogs, toasts or widgets).
   */
  public Integer getInAppAreaId(String category);

  /**
   * This method is called while a system notification is being built or canceled. You can override
   * this method to specify the identifier that will be used when calling
   * {@link NotificationManager#notify(int, Notification)}.
   * @param content content to be notified.
   * @return system notification identifier.
   */
  public int getNotificationId(EngagementReachInteractiveContent content);

  /**
   * Called when a notification only announcement is clicked. Implementor is supposed to execute
   * action URL if specified or provide a default action in some scenarii otherwise (like launching
   * application if the notification is a system one and application is in background).
   * @param notifAnnouncement notification only announcement.
   */
  public void executeNotifAnnouncementAction(EngagementNotifAnnouncement notifAnnouncement);
}
