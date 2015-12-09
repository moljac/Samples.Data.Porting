//
//  CRWViewController.m
//  CrowdWeather
//
//  Created by David on 8/18/14.
//  Copyright (c) 2014 Firebase. All rights reserved.
//

#import "CRWViewController.h"
#import <Firebase/Firebase.h>

@interface CRWViewController ()

@end

@implementation CRWViewController

- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view, typically from a nib.
    Firebase *fb = [[Firebase alloc] initWithUrl: @"https://ios-quickstart.firebaseio-demo.com/condition"];
    [fb observeEventType:FEventTypeValue withBlock:^(FDataSnapshot *snapshot) {
        self.labelCondition.text = snapshot.value;
    }];
}
- (IBAction)sendSunny:(UIButton *)sender {
    Firebase *fb = [[Firebase alloc] initWithUrl:@"https://ios-quickstart.firebaseio-demo.com/condition"];
    [fb setValue:@"Sunny"];
}
- (IBAction)sendFoggy:(UIButton *)sender {
    Firebase *fb = [[Firebase alloc] initWithUrl:@"https://ios-quickstart.firebaseio-demo.com/condition"];
    [fb setValue:@"Foggy"];
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

@end
