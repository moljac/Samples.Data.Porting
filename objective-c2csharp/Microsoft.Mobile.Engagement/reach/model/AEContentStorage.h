/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

/* Remote notification Reach key */
static NSString* kAERemoteNotifReachKey = @"azme";

/* Content local identifier */
static NSString* kAEOid = @"oid";

/* Content reach campaign identifier */
static NSString* kAECampaignId = @"ci";

/* DLC flag */
static NSString* kAEDlc = @"dlc";

/* Content DLC id */
static NSString* kAEDlcId = @"id";

/* Category id (used for sending interactive notification with custom actions) */
static NSString* kAECategory = @"category";

/* Delivery time */
static NSString* kAEDeliveryTime = @"dt";

/* Campaign end time */
static NSString* kAETtl = @"ttl";

/* Campaign end time user time zone flag */
static NSString* kAEUserTimeZone = @"utz";

/* Notification icon */
static NSString* kAENotificationIcon = @"ic";

/* Notification closeable */
static NSString* kAENotificationClosable = @"cl";

/* Notification title */
static NSString* kAENotificationTitle = @"tle";

/* Notification message */
static NSString* kAENotificationMessage = @"msg";

/* Action URL */
static NSString* kAEActionUrl = @"au";

/* Campaign JSON payload (dlc) */
static NSString* kAEPayload = @"payload";

/* Content ID */
static NSString* kAEContentId = @"cId";

/* Content type */
static NSString* kAEContentType = @"cType";

/* aps object sent by APNS in JSON format */
static NSString* kAEAps = @"aps";

/* Unique Push Id */
static NSString* kAEPushId = @"pid";

/* DLC notification image */
static NSString* kAEDlcNotificationImage = @"notificationImage";

/* DLC delivery activities */
static NSString* kAEDlcDeliveryActivities = @"deliveryActivities";

/* DLC title */
static NSString* kAEDlcTitle = @"title";

/* DLC body */
static NSString* kAEDlcBody = @"body";

/* DLC action button text */
static NSString* kAEDlcActionButtonText = @"actionButtonText";

/* DLC exit button text */
static NSString* kAEDlcExitButtonText = @"exitButtonText";

/* DLC questions */
static NSString* kAEDlcQuestions = @"questions";

/* DLC type */
static NSString* kAEDlcType = @"type";

/* content dropped */
static NSString* const kAEReachFeedbackDropped = @"dropped";

/* in-app-notification-displayed feedback */
static NSString* const kAEReachFeedbackInAppNotificationDisplayed = @"in-app-notification-displayed";

/* system-notification-displayed feedback */
static NSString* const kAEReachFeedbackSystemNotificationDisplayed = @"system-notification-displayed";

/* content-displayed feedback */
static NSString* const kAEReachFeedbackContentDisplayed = @"content-displayed";

/* in-app-notification-actioned feedback */
static NSString* const kAEReachFeedbackInAppNotificationActioned = @"in-app-notification-actioned";

/* system-notification-actioned feedback */
static NSString* const kAEReachFeedbackSystemNotificationActioned = @"system-notification-actioned";

/* in-app-notification-exited feedback */
static NSString* const kAEReachFeedbackInAppNotificationExited = @"in-app-notification-exited";

/* system-notification-exited feedback */
static NSString* const kAEReachFeedbackSystemNotificationExited = @"system-notification-exited";

/* content-actioned feedback */
static NSString* const kAEReachFeedbackContentActioned = @"content-actioned";

/* content-exited feedback */
static NSString* const kAEReachFeedbackContentExited = @"content-exited";

/* delivered feedback */
static NSString* const kAEReachFeedbackDelivered = @"delivered";