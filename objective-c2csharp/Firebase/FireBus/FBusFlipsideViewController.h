//
//  FBusFlipsideViewController.h
//  Firebus
//
//  Created by Vikrum Nijjar on 3/5/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <UIKit/UIKit.h>

@class FBusFlipsideViewController;

@protocol FBusFlipsideViewControllerDelegate
- (void)flipsideViewControllerDidFinish:(FBusFlipsideViewController *)controller;
@end

@interface FBusFlipsideViewController : UIViewController

@property (weak, nonatomic) id <FBusFlipsideViewControllerDelegate> delegate;

- (IBAction)done:(id)sender;
- (IBAction)github:(id)sender;

@end
