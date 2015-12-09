//
//  main.m
//  Firebus
//
//  Created by Vikrum Nijjar on 3/5/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <UIKit/UIKit.h>

// #import "Firebus-Swift.h"
#import "FBusAppDelegate.h"

int main(int argc, char *argv[])
{
    @autoreleasepool {
        // Use FBusAppDelegate line for iOS6 [works for iOS7, iOS8]
        // Use AppDelegate for Swift [works for iOS7, iOS8]
        
//        return UIApplicationMain(argc, argv, nil, NSStringFromClass([AppDelegate class]));
        return UIApplicationMain(argc, argv, nil, NSStringFromClass([FBusAppDelegate class]));
    }
}