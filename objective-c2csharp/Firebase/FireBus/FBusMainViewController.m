//
//  FBusMainViewController.m
//  Firebus
//
//  Created by Vikrum Nijjar on 3/5/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import "FBusMainViewController.h"
#import "FBusMetadata.h"
#import "MBProgressHUD.h"
#import "FBusImageUtils.h"
#import "FBusPointAnnotation.h"
#import <Firebase/Firebase.h>
#import <CoreLocation/CLLocation.h>
#import <QuartzCore/QuartzCore.h>

@interface FBusMainViewController ()

@end

@implementation FBusMainViewController

@synthesize busLocations;
@synthesize map;

- (void)viewDidLoad
{
    [super viewDidLoad];
    
    self.busLocations = [[NSMutableDictionary alloc] init];
    self.map.delegate = self;

    [self moveMapToSF];
    [self setupOpacityTimer];
    [self setupFirebaseCallbacks];
}

#pragma mark - Setup 

/*
 This is where all of the Firebase magic happens. We do some basic setup:
 
 1) Watch for when we're connected to display the progress HUD
 2) Observe for new and changing buses
 3) Observe for removed buses
 
 That's it! No more pull to refresh!
 
 */
- (void) setupFirebaseCallbacks {
    // Register for changes on connection state
    Firebase* firebusRoot = [[Firebase alloc] initWithUrl:@"https://publicdata-transit.firebaseio.com/"];
    [[firebusRoot childByAppendingPath:@"/.info/connected"] observeEventType:FEventTypeValue withBlock:^(FDataSnapshot *snapshot) {
        if(![snapshot.value boolValue]) {
            [MBProgressHUD showHUDAddedTo:self.view animated:YES];
        }
    }];
    
    // Observe for when buses are added
    Firebase* firebusSf = [firebusRoot childByAppendingPath:@"sf-muni/vehicles"];
    [firebusSf observeEventType:FEventTypeChildAdded withBlock:^(FDataSnapshot *snapshot) {
        [MBProgressHUD hideHUDForView:self.view animated:YES];
        [self addBusToMap:snapshot.value withId:snapshot.name];
    }];
    // ... and changed.
    [firebusSf observeEventType:FEventTypeChildChanged withBlock:^(FDataSnapshot *snapshot) {
        [MBProgressHUD hideHUDForView:self.view animated:YES];
        NSDictionary* bus = snapshot.value;
        FBusMetadata* busMetadata = [self.busLocations objectForKey:snapshot.name];
        [self animateBus:busMetadata toNewPosition:bus];
    }];
    
    // Observe for when buses are removed
    [firebusSf observeEventType:FEventTypeChildRemoved withBlock:^(FDataSnapshot *snapshot) {
        FBusMetadata* busMetadata = [self.busLocations objectForKey:snapshot.name];
        [self.busLocations removeObjectForKey:snapshot.name];
        [self.map removeAnnotation:busMetadata.pin];
    }];
}

- (void) moveMapToSF {
    CLLocationCoordinate2D sf = CLLocationCoordinate2DMake(37.778905, -122.391635);
    MKCoordinateSpan span = MKCoordinateSpanMake(0.025, 0.025);
    MKCoordinateRegion region = MKCoordinateRegionMake(sf, span);
    [self.map setRegion:region];
}

- (void) setupOpacityTimer {
    [NSTimer scheduledTimerWithTimeInterval:1.0
                                     target:self
                                   selector:@selector(adjustAgeOpacity)
                                   userInfo:nil
                                    repeats:YES];
}

- (void) adjustAgeOpacity {
    for(NSString *key in self.busLocations) {
        FBusMetadata* busMetadata = [self.busLocations objectForKey:key];
        MKPointAnnotation *busPin = busMetadata.pin;
        NSDictionary *metadata = busMetadata.metadata;
        
        double timestamp = [[metadata objectForKey:@"timestamp"] doubleValue] / 1000.0;
        double age = [[NSDate date] timeIntervalSince1970] - timestamp;
        double alpha = (age > 120) ? 0.01 : (1.0 - (age / 120.0)); // ghost bus if GPS is stale
        
        MKAnnotationView* busView = [self.map viewForAnnotation:busPin];
        
        double alphaDelta = fabs(busView.alpha - alpha);
        if(alphaDelta > 0.02) {
            CABasicAnimation *animation = [CABasicAnimation animationWithKeyPath:@"opacity"];
            animation.duration = 0.95;
            animation.fromValue = [NSNumber numberWithFloat:busView.alpha];
            animation.toValue = [NSNumber numberWithFloat:alpha];
            animation.delegate = busView;
            animation.removedOnCompletion = YES;
            [busView.layer addAnimation:animation forKey:@"alphaAnimation"];
        }
        [busView setAlpha:alpha];
    }
}

#pragma mark - Update views

- (void) addBusToMap:(NSDictionary *)bus withId:(NSString *)key {
        dispatch_async(dispatch_get_main_queue(), ^{
            if( bus && ![self.busLocations objectForKey:key]) {
                FBusPointAnnotation *busPin = [[FBusPointAnnotation alloc] init];
                
                [busPin setCoordinate:CLLocationCoordinate2DMake([[bus objectForKey:@"lat"] doubleValue], [[bus objectForKey:@"lon"] doubleValue])];
                [busPin setTitle:[[bus objectForKey:@"routeTag"] description]];
                busPin.key = key;
                busPin.route = [[bus objectForKey:@"routeTag"] description];
                busPin.outbound =  ! ([[bus objectForKey:@"dirTag"] rangeOfString:@"OB"].location == NSNotFound);
                busPin.vtype = [bus objectForKey:@"vtype"];
                
                FBusMetadata* busMetadata = [[FBusMetadata alloc] init];
                busMetadata.metadata = bus;
                busMetadata.pin = busPin;

                [self.busLocations setObject:busMetadata forKey:key];
                [self.map addAnnotation:busPin];
            }
        });
}

- (void) animateBus:(FBusMetadata *)busMetadata toNewPosition:(NSDictionary *)newMetadata {
    dispatch_async(dispatch_get_main_queue(), ^{
        MKPointAnnotation *busPin = busMetadata.pin;
        MKAnnotationView *busView = [self.map viewForAnnotation:busPin];
        if(busView) {
            CLLocationCoordinate2D newCoord = CLLocationCoordinate2DMake([[newMetadata objectForKey:@"lat"] doubleValue], [[newMetadata objectForKey:@"lon"] doubleValue]);
            MKMapPoint mapPoint = MKMapPointForCoordinate(newCoord);
            
            CGPoint toPos;
            if ([[[UIDevice currentDevice] systemVersion] floatValue] >= 7) {
                toPos = [self.map convertCoordinate:newCoord toPointToView:self.map];
            } else {
                CGFloat zoomFactor =  self.map.visibleMapRect.size.width / self.map.bounds.size.width;
                toPos.x = mapPoint.x/zoomFactor;
                toPos.y = mapPoint.y/zoomFactor;
            }

            if (MKMapRectContainsPoint(self.map.visibleMapRect, mapPoint)) {
                CABasicAnimation *animation = [CABasicAnimation animationWithKeyPath:@"position"];
                animation.fromValue = [NSValue valueWithCGPoint:busView.center];
                animation.toValue = [NSValue valueWithCGPoint:toPos];
                animation.duration = 5.5;
                animation.delegate = busView;
                animation.fillMode = kCAFillModeForwards;
                [busView.layer addAnimation:animation forKey:@"positionAnimation"];
            }
            
            busView.center = toPos;
            busMetadata.metadata = newMetadata;
            [busPin setCoordinate:newCoord];
        }
    });
}


#pragma mark - Flipside View Controller

- (void)flipsideViewControllerDidFinish:(FBusFlipsideViewController *)controller
{
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPhone) {
        [self dismissViewControllerAnimated:YES completion:nil];
    } else {
        [self.flipsidePopoverController dismissPopoverAnimated:YES];
    }
}

- (IBAction)showInfo:(id)sender
{
    if ([[UIDevice currentDevice] userInterfaceIdiom] == UIUserInterfaceIdiomPhone) {
        FBusFlipsideViewController *controller = [[FBusFlipsideViewController alloc] initWithNibName:@"FBusFlipsideViewController" bundle:nil];
        controller.delegate = self;
        controller.modalTransitionStyle = UIModalTransitionStyleFlipHorizontal;
        [self presentViewController:controller animated:YES completion:nil];
    } else {
        if (!self.flipsidePopoverController) {
            FBusFlipsideViewController *controller = [[FBusFlipsideViewController alloc] initWithNibName:@"FBusFlipsideViewController" bundle:nil];
            controller.delegate = self;
            
            self.flipsidePopoverController = [[UIPopoverController alloc] initWithContentViewController:controller];
        }
        if ([self.flipsidePopoverController isPopoverVisible]) {
            [self.flipsidePopoverController dismissPopoverAnimated:YES];
        } else {
            [self.flipsidePopoverController presentPopoverFromBarButtonItem:sender permittedArrowDirections:UIPopoverArrowDirectionAny animated:YES];
        }
    }
}

- (MKAnnotationView *)mapView:(MKMapView *)mapView viewForAnnotation:(id <MKAnnotation>)annotation {
    
    MKAnnotationView *pinView = nil;
    if(annotation != mapView.userLocation)
    {
        FBusPointAnnotation *busAnnotation = (FBusPointAnnotation *)annotation;
        
        pinView = (MKAnnotationView *)[mapView dequeueReusableAnnotationViewWithIdentifier:busAnnotation.key];

        if(pinView == nil)
            pinView = [[MKAnnotationView alloc] initWithAnnotation:annotation reuseIdentifier:busAnnotation.key];
    
        pinView.canShowCallout = NO;

        UIImage *image = [FBusImageUtils imageFromText:busAnnotation.route isOutbound:busAnnotation.outbound forVehicleType:busAnnotation.vtype];
        UIImageView *imageView = [[UIImageView alloc] initWithImage:image];
        imageView.layer.cornerRadius = 5.0;
        imageView.layer.masksToBounds = YES;
        imageView.layer.borderColor = [UIColor lightGrayColor].CGColor;
        imageView.layer.borderWidth = 1.0;
        
        [pinView addSubview:imageView];
    }
    return pinView;    
}

@end
