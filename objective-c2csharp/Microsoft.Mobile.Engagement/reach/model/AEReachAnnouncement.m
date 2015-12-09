/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachAnnouncement.h"
#import "AEReachContent.h"
#import "EngagementAgent.h"
#import "AEContentStorage.h"

@implementation AEReachAnnouncement

- (id)initWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params
{
  self = [super initWithReachValues:reachValues params:params];
  if (self != nil)
  {
    /* Init type, it can already be set if the payload has been set during super init */
    if (!_type)
      _type = AEAnnouncementTypeUnknown;
    
    /* Cache params */
    _cachedParams = [params copy];

    /* Set params in body. */
    [self replaceParamsInBody];
  }
  return self;
}

+ (id)announcementWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params
{
  return [[[self alloc] initWithReachValues:reachValues params:params] autorelease];
}

- (void)dealloc
{
  [_cachedParams release];
  [super dealloc];
}

- (void)replaceParamsInBody
{
  if (_cachedParams)
  {
    /* Replace parameters in the content payload */
    NSString* newBody = self.body;
    NSEnumerator* keyIt = [_cachedParams keyEnumerator];
    NSString* key;
    while ((key = [keyIt nextObject]))
    {
      newBody = [newBody stringByReplacingOccurrencesOfString:key withString:_cachedParams[key]];
    }
    self.body = newBody;
  }
}

- (void)setPayload:(NSDictionary*)payload
{
  [super setPayload:payload];

  /* Set type */
  _type = [self typeFromString:payload[kAEDlcType]];

  /* Replace the input param in body */
  [self replaceParamsInBody];
}

/** Get the type of this announcement from the given string */
- (AEAnnouncementType)typeFromString:(NSString*)type
{
  if ([type isEqualToString:@"text/plain"])
    return AEAnnouncementTypeText;
  else if ([type isEqualToString:@"text/html"])
    return AEAnnouncementTypeHtml;
  else
    return AEAnnouncementTypeUnknown;
}

@end
