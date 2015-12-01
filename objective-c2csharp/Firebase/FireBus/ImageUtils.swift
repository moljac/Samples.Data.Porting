//
//  ImageUtils.swift
//  Firebus
//
//  Created by Katherine Fang on 6/23/14.
//  Copyright (c) 2014 Firebase. All rights reserved.
//

import Foundation
import UIKit
import CoreGraphics

class ImageUtils {
    // Based on http://stackoverflow.com/a/2768081/1843780
    class func imageFromText(text: String, outbound:Bool, vtype: String) -> UIImage {
        var busText = " \(getEmojiForRoute(vtype)) \(text) "
        var font = UIFont.systemFontOfSize(17.0);
        var nsBusText = busText as NSString
        var size = nsBusText.sizeWithAttributes([NSFontAttributeName: font])
        
        UIGraphicsBeginImageContextWithOptions(size, true, 0.0)
        
        // optional: add a shadow, to avoid clipping the shadow, you should make the context size bigger
        var ctx = UIGraphicsGetCurrentContext();
        
        // Background; Outbound: 7094FF ; Inbound: FF6262
        outbound ? CGContextSetRGBFillColor(ctx, 0.439, 0.58, 1, 1.0) : CGContextSetRGBFillColor(ctx, 1, 0.384, 0.384, 1.0)
        
        CGContextSetRGBStrokeColor(ctx, 0.3, 0.3, 0.3, 0.8)
        CGContextFillRect(ctx, CGRect(origin: CGPointZero, size: size))
        
        // Foreground
        CGContextSetRGBStrokeColor(ctx, 0.9, 0.9, 0.9, 1.0)
        CGContextSetRGBFillColor(ctx, 1, 1, 1, 1.0)
        
        // ??? I can I do this with swift string
        nsBusText.drawAtPoint(CGPointMake(0.0, 0.0), withFont: font)
        
        var image = UIGraphicsGetImageFromCurrentImageContext()
        UIGraphicsEndImageContext()
        return image;
    }
    
    class func getEmojiForRoute(vtype: String) -> String {
        return (vtype == "bus") ? "🚌" : "🚃";
    }
}