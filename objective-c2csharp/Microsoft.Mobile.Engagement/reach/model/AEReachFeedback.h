/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <Foundation/Foundation.h>

/**
 * A `AEReachFeedback` object represents a the state of the reach feedback push message.
 *
 * This class is used to cache the state of the sent reach feedback in order to avoid duplicates.
 *
 */
@interface AEReachFeedback : NSObject<NSCoding>

/** Unique push identifier */
@property(nonatomic, retain) NSString* pushId;

/** Dictionary containing all the sent reach feedbacks */
@property(nonatomic, retain) NSMutableDictionary* reachFeedbacks;

/**
 * Parse payload data.
 * @param pushId The unique push id.
 * @return object of AEReachFeedback.
 */
- (id)initWithPushId:(NSString*)pushId;

/**
 * Return the value for key in the reach feedback dictionary.
 * @param key The key.
 * @return boolean value of the key.
 */
- (BOOL)getValue:(NSString*)key;

/**
 * Set the value for key in the reach feedback dictionary.
 * @param value The value for the key.
 * @param key The key.
 */
- (void)setValue:(BOOL)value forKey:(NSString*)key;

@end