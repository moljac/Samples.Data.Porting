//
//  FBusPointAnnotation.h
//  Firebus
//
//  Created by Vikrum Nijjar on 4/22/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <MapKit/MapKit.h>

@interface FBusPointAnnotation : MKPointAnnotation

@property (nonatomic, strong) NSString* key;
@property (nonatomic, strong) NSString* route;
@property (nonatomic, strong) NSString* vtype;
@property (nonatomic) BOOL outbound;

@end
