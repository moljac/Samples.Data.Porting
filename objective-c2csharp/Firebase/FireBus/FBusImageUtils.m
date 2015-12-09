//
//  FBusImageUtils.m
//  Firebus
//
//  Created by Vikrum Nijjar on 4/22/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import "FBusImageUtils.h"

@implementation FBusImageUtils

// Based on http://stackoverflow.com/a/2768081/1843780
+ (UIImage *)imageFromText:(NSString *)text isOutbound:(BOOL) outbound forVehicleType:(NSString *)vtype
{    
    NSString* busText = [NSString stringWithFormat:@" %@ %@ ", [FBusImageUtils getEmojiForRoute:vtype], text];
    // set the font type and size
    UIFont *font = [UIFont systemFontOfSize:17.0];
    CGSize size  = [busText sizeWithFont:font];
    
    UIGraphicsBeginImageContextWithOptions(size,YES,0.0);
    
    
    // optional: add a shadow, to avoid clipping the shadow you should make the context size bigger
    //
    CGContextRef ctx = UIGraphicsGetCurrentContext();
    
    // Background; Outbound: 7094FF ; Inbound: FF6262
    outbound ? CGContextSetRGBFillColor(ctx, 0.439, 0.58, 1, 1.0) : CGContextSetRGBFillColor(ctx, 1, 0.384, 0.384, 1.0);

    CGContextSetRGBStrokeColor(ctx, 0.3, 0.3, 0.3, 0.8);
    CGContextFillRect(ctx, (CGRect){CGPointZero, size});

    // Foreground
    CGContextSetRGBStrokeColor(ctx, 0.9, 0.9, 0.9, 1.0);
    CGContextSetRGBFillColor(ctx, 1, 1, 1, 1.0);

    [busText drawAtPoint:CGPointMake(0.0, 0.0) withFont:font];
    
    UIImage *image = UIGraphicsGetImageFromCurrentImageContext();
    UIGraphicsEndImageContext();
    return image;
}

+ (NSString *) getEmojiForRoute:(NSString *) vtype {
    return [vtype isEqualToString:@"bus"] ? @"🚌" : @"🚃";
}

@end
