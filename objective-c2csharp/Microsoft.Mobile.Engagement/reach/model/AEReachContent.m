/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachContent.h"
#import "AEReachModule.h"
#import "EngagementAgent.h"
#import "AEContentStorage.h"

@implementation AEReachContent

- (id)initWithReachValues:(NSDictionary*)reachValues
{
  self = [super init];

  if (self != nil)
  {
    /* Content identifier */
    _contentId = [reachValues[kAEContentId] copy];
    
    /* Unique Push ID */
    _pushId = [reachValues[kAEPushId] copy];

    /* DLC */
    _dlc = [reachValues[kAEDlc] integerValue];
    _dlcId = [reachValues[kAEDlcId] copy];

    /* Category */
    NSDictionary* aps = reachValues[kAEAps];
    if (aps)
      _category = [aps[kAECategory] copy];

    /* Expiry */
    if (![reachValues[kAETtl] isEqualToString:@"-1"])
    {
      long long expiryTimestamp = [reachValues[kAETtl] longLongValue];
      _expiryDate = [[NSDate dateWithTimeIntervalSince1970:(NSTimeInterval)(expiryTimestamp)] retain];

      /* Check for user timezone, default value is "0" */
      if (![reachValues[kAEUserTimeZone] isEqualToString:@"0"])
      {
        _expiryLocaltz = YES;
      }
    }

    /* Set payload */
    if (reachValues[kAEPayload])
      [self setPayload:reachValues[kAEPayload]];
  }
  return self;
}

- (void)dealloc
{
  [_contentId release];
  [_category release];
  [_body release];
  [_expiryDate release];
  [_actionURL release];
  [_dlcId release];
  [_pushId release];
  [super dealloc];
}

- (BOOL)isExpired
{
  if (_expiryDate != nil)
  {
    /* Remove current timezone offset from expiry date to get the date in GMT */
    NSDate* expiryDate = [[_expiryDate copy] autorelease];
    if (_expiryLocaltz)
      expiryDate =
        [expiryDate dateByAddingTimeInterval:-[[NSTimeZone defaultTimeZone] secondsFromGMTForDate:expiryDate]];

    /* If expiry date has been reached, content is expired */
    if ([[expiryDate earlierDate:[NSDate date]] isEqualToDate:expiryDate])
      return YES;
  }
  return NO;
}

- (void)sendFeedback:(NSString*)status extras:(NSDictionary*)extras
{
  /* If feedback enabled and content is not expired*/
  if (_feedback  && ![self isExpired])
  {
    AEReachModule* module = (AEReachModule*)[[EngagementAgent shared] getModule:kAEReachModuleName];
    
    /* Make sure feedback hasn't been sent already */
    if ([module shouldSendFeedback:status forPushId:[self getPushId]])
    {
      /* Build feedback dictionary */
      NSMutableDictionary* feedback = [[NSMutableDictionary alloc] initWithCapacity:3];
      feedback[@"kind"] = [self kind];
      feedback[@"id"] = _contentId;
      feedback[@"status"] = status;
      if (extras)
        feedback[@"extras"] = extras;
      
      /* Reply */
      [[EngagementAgent shared] sendReachFeedback:feedback];
      
      /* Release payload */
      [feedback release];
    }
  }
}

- (void)process:(NSString*)status extras:(NSDictionary*)extras
{
  if (!_processed)
  {
    /* Send feedback if any */
    if (status != nil)
      [self sendFeedback:status extras:extras];

    /* Mark content as processed */
    AEReachModule* module = (AEReachModule*)[[EngagementAgent shared] getModule:kAEReachModuleName];
    [module markContentProcessed:self];
    _processed = YES;
  }
}

- (NSData*)decodeBase64:(NSString*)str
{
  NSData* data = [NSData alloc];
  if ([data respondsToSelector:@selector(initWithBase64EncodedString:options:)])
    return [data initWithBase64EncodedString:str options:0];
  else
    return [data initWithBase64Encoding:str];
  return [data autorelease];
}

- (NSString*)kind
{
  [NSException raise:NSInternalInconsistencyException format:@"You must override %@ in a subclass",
   NSStringFromSelector(@selector(getContentTag))];
  return nil;
}

- (void)drop
{
  [self process:kAEReachFeedbackDropped extras:nil];
}

- (void)actionContent
{
  [self process:kAEReachFeedbackContentActioned extras:nil];
}

- (void)exitContent
{
  [self process:kAEReachFeedbackContentExited extras:nil];
}

/** Set payload */
- (void)setPayload:(NSDictionary*)payload
{
  _dlcCompleted = YES;
  if (payload[kAEDlcBody])
  {
    [_body release];
    _body = [payload[kAEDlcBody] copy];
  }
}

- (BOOL)isDlcCompleted
{
  return _dlcCompleted;
}

- (BOOL)hasDLC
{
  return _dlc > 0;
}

- (BOOL)hasNotificationDLC
{
  return (_dlc & FLAG_DLC_NOTIFICATION) == FLAG_DLC_NOTIFICATION;
}

- (BOOL)hasContentDLC
{
  return (_dlc & FLAG_DLC_CONTENT) == FLAG_DLC_CONTENT;
}

- (BOOL)hasNotificationAndContentDLC
{
  return (_dlc & FLAG_DLC_NOTIF_AND_CONTENT) == FLAG_DLC_NOTIF_AND_CONTENT;
}

- (NSString*)getDlcId
{
  return _dlcId;
}

- (NSString*)getPushId
{
  return _pushId;
}

@end