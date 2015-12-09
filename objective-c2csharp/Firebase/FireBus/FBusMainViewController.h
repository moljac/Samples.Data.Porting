//
//  FBusMainViewController.h
//  Firebus
//
//  Created by Vikrum Nijjar on 3/5/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import "FBusFlipsideViewController.h"
#import <MapKit/MapKit.h>

@interface FBusMainViewController : UIViewController <FBusFlipsideViewControllerDelegate, MKMapViewDelegate>

@property (strong, nonatomic) UIPopoverController *flipsidePopoverController;
@property (strong, nonatomic) NSMutableDictionary *busLocations;
@property (weak, nonatomic) IBOutlet MKMapView *map;

- (IBAction)showInfo:(id)sender;

@end
