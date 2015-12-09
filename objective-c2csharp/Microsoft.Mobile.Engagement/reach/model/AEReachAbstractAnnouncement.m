/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachAbstractAnnouncement.h"
#import "AEContentStorage.h"

@implementation AEReachAbstractAnnouncement

@synthesize actionURL;

- (id)initWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params
{
  self = [super initWithReachValues:reachValues];

  if (self != nil)
  {
    /* Action URL */
    self.actionURL = reachValues[kAEActionUrl];
    
    /* Replace parameters in the action url */
    if (self.actionURL)
    {
      NSEnumerator* keyIt = [params keyEnumerator];
      NSString* key;
      while ((key = [keyIt nextObject]))
      {
        self.actionURL = [self.actionURL stringByReplacingOccurrencesOfString:key withString:params[key]];
      }
    }
  }

  return self;
}

- (void)dealloc
{
  self.actionURL = nil;
  [super dealloc];
}

/** All announcement types subclassing this class are sharing the same kind */
- (NSString*)kind
{
  return kAEAnnouncementKind;
}

@end
