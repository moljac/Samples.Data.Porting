/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * The `EngagementUtils` class provides utility methods used by Engagement view controllers.
 */
@interface EngagementUtils : NSObject

/**
 * Build a Engagement alias for an iOS ViewController class. This implementation takes the simple
 * name of the class and removes the "ViewController" suffix if any (e.g. "MainViewController"
 * becomes "Main").<br/>
 * This method is used by <EngagementViewController> and <EngagementTableViewController>.
 * @param clazz The class to parse.
 * @result An activity name suitable to be reported by the Engagement service.
 */
+ (NSString*)buildEngagementActivityName:(Class)clazz;

@end
