/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "EngagementUtils.h"

static NSString* const kViewControllerSuffix = @"ViewController";

@implementation EngagementUtils

+ (NSString*)buildEngagementActivityName:(Class)class
{
  NSString* name = NSStringFromClass(class);
  
  /* Remove module name on swift classes */
  name = [[name componentsSeparatedByString:@"."] lastObject];
  
  /* Remove suffix if any */
  if ([name hasSuffix:kViewControllerSuffix] && [name length] > [kViewControllerSuffix length])
    name = [name substringToIndex:[name length] - [kViewControllerSuffix length]];
  return name;
}

@end
