//
//  FirefeedSpark.h
//  iFirefeed
//
//  Created by Greg Soltis on 4/10/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Firebase/Firebase.h>

@interface FirefeedSpark : NSObject

+ (FirefeedSpark *) loadFromRoot:(Firebase *)root withSparkId:(NSString *)sparkId block:(void (^)(FirefeedSpark* spark))block;

- (void) stopObserving;

@property (strong, nonatomic) NSString* authorId;
@property (strong, nonatomic) NSString* authorName;
@property (strong, nonatomic) NSString* content;
@property (nonatomic) double timestamp;
@property (readonly) NSURL* authorPicURL;

@end
