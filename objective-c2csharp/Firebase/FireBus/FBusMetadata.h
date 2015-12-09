//
//  FBusMetadata.h
//  Firebus
//
//  Created by Vikrum Nijjar on 3/6/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <MapKit/MapKit.h>

@interface FBusMetadata : NSObject

@property (nonatomic, strong) NSDictionary *metadata;
@property (nonatomic, strong) MKPointAnnotation *pin;

@end
