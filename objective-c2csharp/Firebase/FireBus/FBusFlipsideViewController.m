//
//  FBusFlipsideViewController.m
//  Firebus
//
//  Created by Vikrum Nijjar on 3/5/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import "FBusFlipsideViewController.h"

@interface FBusFlipsideViewController ()

@end

@implementation FBusFlipsideViewController

- (id)initWithNibName:(NSString *)nibNameOrNil bundle:(NSBundle *)nibBundleOrNil
{
    self = [super initWithNibName:nibNameOrNil bundle:nibBundleOrNil];
    if (self) {
        self.contentSizeForViewInPopover = CGSizeMake(320.0, 480.0);
    }
    return self;
}
							
- (void)viewDidLoad
{
    [super viewDidLoad];
	// Do any additional setup after loading the view, typically from a nib.
}

- (void)didReceiveMemoryWarning
{
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

#pragma mark - Actions

- (IBAction)done:(id)sender
{
    [self.delegate flipsideViewControllerDidFinish:self];
}

- (IBAction)github:(id)sender {
    [[UIApplication sharedApplication] openURL:[NSURL URLWithString:@"https://github.com/firebase/firebus"]];
}

@end
