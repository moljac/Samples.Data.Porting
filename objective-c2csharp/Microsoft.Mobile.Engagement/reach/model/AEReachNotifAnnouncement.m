/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachNotifAnnouncement.h"
#import "AEReachModule.h"
#import "EngagementAgent.h"

#define forbidden() NSLog(@"[Engagement][ERROR] Unsupported operation '%@': This is a notification only announcement.", \
                          NSStringFromSelector(_cmd))

@implementation AEReachNotifAnnouncement

+ (id)notifAnnouncementWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params
{
  return [[[self alloc] initWithReachValues:reachValues params:params] autorelease];
}

- (void)actionNotification:(BOOL)launchAction
{
  [super actionNotification:launchAction];

  /* This is the final step in this content kind */
  [self process:nil extras:nil];
}

- (void)actionContent
{
  forbidden();
}

- (void)exitContent
{
  forbidden();
}

@end
