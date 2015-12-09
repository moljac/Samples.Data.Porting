/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEInteractiveContent.h"
#import "AEReachModule.h"
#import "EngagementAgent.h"
#import "AEContentStorage.h"

@implementation AEInteractiveContent

- (id)initWithReachValues:(NSDictionary*)reachValues
{
  self = [super initWithReachValues:reachValues];
  
  if (self != nil)
  {
    NSDictionary* aps = reachValues[kAEAps];
    NSObject* anAlert = aps[@"alert"];
    NSString* alertMessage = nil;

    /* Parse "alert" in the aps object and split it into title and message using '\n' as separator */
    /* "alert" object can be of type NSString or NSDictionary. therefore, a type check is required */
    if ([anAlert isKindOfClass:[NSString class]])
    {
      alertMessage = (NSString*)anAlert;
    }
    else if ([anAlert isKindOfClass:[NSDictionary class]])
    {
      alertMessage = ((NSDictionary*)anAlert)[kAEDlcBody];
    }
    
    /* If title and message are not available through the AzME object then try to get them from the alert message */
    if ([reachValues[kAENotificationTitle] length] == 0 && [reachValues[kAENotificationMessage] length] == 0
        && [alertMessage length] > 0)
    {
      NSRange newLineRange = [alertMessage rangeOfCharacterFromSet:[NSCharacterSet newlineCharacterSet]];
      if (newLineRange.location != NSNotFound)
      {
        _notificationTitle = [[alertMessage substringToIndex:newLineRange.location] copy];
        _notificationMessage = [[alertMessage substringFromIndex:newLineRange.location+newLineRange.length] copy];
      }
      
      /* The alert only contains the message */
      else
        _notificationMessage = [alertMessage copy];
    }
    
    /* Get title and message from the AzME object */
    else
    {
      _notificationTitle = [reachValues[kAENotificationTitle] copy];
      _notificationMessage = [reachValues[kAENotificationMessage] copy];
    }

    /* The notification can be closed unless closeable is set to "0", default value is 1(closable) */
    _notificationCloseable = [reachValues[kAENotificationClosable] isEqualToString:@"1"];
    
    /* The notification has a content icon unless icon attribute set "0", default value is 1 */
    _notificationIcon = [reachValues[kAENotificationIcon] isEqualToString:@"1"];

    /* Behavior */
    if ([reachValues[kAEDeliveryTime] isEqualToString:@"s"])
      _behavior = AEContentBehaviorSession;
    else if ([reachValues[kAEDeliveryTime] isEqualToString:@"b"])
      _behavior = AEContentBehaviorBackground ;
    else
      _behavior = AEContentBehaviorAnyTime;
  }
  
  return self;
}

- (void)setPayload:(NSDictionary *)payload
{
  /* Set payload on parent */
  [super setPayload:payload];
  
  /* Body related fields */
  _actionLabel = [payload[kAEDlcActionButtonText] copy];
  _title = [payload[kAEDlcTitle] copy];
  _exitLabel = [payload[kAEDlcExitButtonText] copy];
  
  /* Delivery activity */
  NSArray* deliveryActivities = payload[kAEDlcDeliveryActivities];
  if ([deliveryActivities count] > 0)
  {
    _allowedActivities = [deliveryActivities copy];
  }

  /* Image Data */
  _notificationImageString = [payload[kAEDlcNotificationImage] copy];
}

- (void)dealloc
{
  [_title release];
  [_actionLabel release];
  [_exitLabel release];
  [_notificationTitle release];
  [_notificationMessage release];
  [_notificationImage release];
  [_notificationImageString release];
  [_allowedActivities release];
  [super dealloc];
}

- (UIImage*)notificationImage
{
  /* Decode image */
  if (_notificationImageString && !_notificationImage)
  {
    /* Decode base 64 then decode as an image */
    _notificationImage = [[UIImage imageWithData:[self decodeBase64:_notificationImageString]] retain];

    /* Clear image data */
    [_notificationImageString release];
    _notificationImageString = nil;
  }

  return _notificationImage;
}

- (BOOL)canNotify:(NSString*)activity
{
  return (_allowedActivities == nil || [_allowedActivities containsObject:activity]);
}

- (void)displayNotification
{
  /* Avoid sending feedback multiple times */
  if (!_notificationDisplayed)
  {
    [self sendFeedback:kAEReachFeedbackInAppNotificationDisplayed extras:nil];
    _notificationDisplayed = YES;
  }
}

- (void)exitNotification
{
  [self process:kAEReachFeedbackInAppNotificationExited extras:nil];
}

- (void)actionNotification
{
  [self actionNotification:YES];
}

- (void)actionNotification:(BOOL)launchAction
{
  AEReachModule* module = (AEReachModule*)[[EngagementAgent shared] getModule:kAEReachModuleName];
  
  /* Report status */
  if (!_notificationActioned)
  {
    if (self.notifiedFromNativePush)
    {
      /* For case where app is killed, delivered and displayed feedbacks are missing
       * therefore, send a delivered and system-displayed feedback as a backup in case they are missed
       */
      [self sendFeedback:kAEReachFeedbackDelivered extras:nil];
      [self sendFeedback:kAEReachFeedbackSystemNotificationDisplayed extras:nil];
      [self sendFeedback:kAEReachFeedbackSystemNotificationActioned extras:nil];
    }
    else
    {
      [self sendFeedback:kAEReachFeedbackInAppNotificationActioned extras:nil];
    }
    _notificationActioned = YES;
  }

  /* Notify module */
  if (launchAction)
    [module onNotificationActioned:self];
}

- (void)displayContent
{
  /* Avoid sending feedback multiple times */
  if (!_contentDisplayed)
  {
    [self sendFeedback:kAEReachFeedbackContentDisplayed extras:nil];
    _contentDisplayed = YES;
  }
}

- (void)setDisplayed:(BOOL)displayed
{
  _notificationDisplayed = displayed;
}

- (void)setActioned:(BOOL)actioned
{
  _notificationActioned = actioned;
}

@end