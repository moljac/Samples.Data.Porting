//
//  FBusImageUtils.h
//  Firebus
//
//  Created by Vikrum Nijjar on 4/22/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <Foundation/Foundation.h>

@interface FBusImageUtils : NSObject

+ (UIImage *)imageFromText:(NSString *)text isOutbound:(BOOL) outbound forVehicleType:(NSString *)vtype;
+ (NSString *) getEmojiForRoute:(NSString *) route;

@end
