/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachFeedback.h"

static NSString* const kAEReachFeedbackPushId = @"pushId";
static NSString* const kAEReachFeedbackValues = @"reachFeedbacks";

@implementation AEReachFeedback

- (id)initWithPushId:(NSString*)pushId
{
  if ([pushId length] == 0 )
    return nil;

  /* Init object */
  self = [super init];
  if (self)
  {
    self.pushId = pushId;
    self.reachFeedbacks = [[[NSMutableDictionary alloc] init] autorelease];
  }
  return self;
}

- (void)dealloc
{
  self.pushId = nil;
  self.reachFeedbacks = nil;
  
  [super dealloc];
}

#pragma mark -
#pragma mark NSCoding

- (void)encodeWithCoder:(NSCoder*)encoder
{
  if (self.pushId)
    [encoder encodeObject:self.pushId forKey:kAEReachFeedbackPushId];
  if (self.reachFeedbacks)
    [encoder encodeObject:self.reachFeedbacks forKey:kAEReachFeedbackValues];
}

- (id)initWithCoder:(NSCoder*)decoder
{
  self = [super init];
  if (self != nil)
  {
    self.pushId = [decoder decodeObjectForKey:kAEReachFeedbackPushId];
    self.reachFeedbacks = [decoder decodeObjectForKey:kAEReachFeedbackValues];
  }
  return self;
}

- (BOOL)getValue:(NSString*)key
{
  if (self.reachFeedbacks[key])
    return [self.reachFeedbacks[key] boolValue];

  /* Return false, if key doesn't exist */
  return NO;
}

- (void)setValue:(BOOL)value forKey:(NSString*)key
{
  if ([key length] == 0)
    return;
  
  /* Update the reach feedbacks */
  [self.reachFeedbacks setObject:[NSNumber numberWithBool:value] forKey:key];
}

@end