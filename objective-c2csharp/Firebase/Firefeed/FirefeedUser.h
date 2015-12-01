//
//  FirefeedUser.h
//  iFirefeed
//
//  Created by Greg Soltis on 4/7/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Firebase/Firebase.h>

@protocol FirefeedUserDelegate;

@interface FirefeedUser : NSObject

+ (FirefeedUser *) loadFromRoot:(Firebase *)root withUserId:(NSString *)userId completionBlock:(void (^)(FirefeedUser* user))block;
+ (FirefeedUser *) loadFromRoot:(Firebase *)root withUserData:(NSDictionary *)userData completionBlock:(void (^)(FirefeedUser* user))block;

- (void) updateFromRoot:(Firebase *)root;
- (void) stopObserving;

@property (strong, nonatomic) NSString* userId;
@property (strong, nonatomic) NSString* firstName;
@property (strong, nonatomic) NSString* lastName;
@property (strong, nonatomic) NSString* fullName;
@property (strong, nonatomic) NSString* location;
@property (strong, nonatomic) NSString* bio;
@property (readonly) NSURL* picUrl;
@property (readonly) NSURL* picURLSmall;
@property (weak, nonatomic) id<FirefeedUserDelegate> delegate;

@end

@protocol FirefeedUserDelegate

- (void) userDidUpdate:(FirefeedUser *)user;

@end
