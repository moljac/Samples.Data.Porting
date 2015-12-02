/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "EngagementViewController.h"
#import "EngagementAgent.h"
#import "EngagementUtils.h"

@implementation EngagementViewController

- (void)viewDidAppear:(BOOL)animated
{
  [super viewDidAppear:animated];
  [[EngagementAgent shared] startActivity:[self engagementActivityName] extras:[self engagementActivityExtra]];
}

- (NSString*)engagementActivityName
{
  return [EngagementUtils buildEngagementActivityName:[self class]];
}

- (NSDictionary*)engagementActivityExtra
{
  return nil;
}

@end
