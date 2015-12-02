/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEContentLoadingViewController.h"
#import "AEViewControllerUtil.h"
#import "AEReachModule.h"
#import "EngagementAgent.h"

@implementation AEContentLoadingViewController

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Memory management
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//
- (void)viewDidLoad
{
  [super viewDidLoad];

  /* Set transparent background  */
  self.view.backgroundColor = [UIColor blackColor];
  self.view.alpha = 0.7;
  
  /* Add activity indicator with animation */
  UIActivityIndicatorView* activityIndicator = [[[UIActivityIndicatorView alloc] init] autorelease];
  activityIndicator.activityIndicatorViewStyle = UIActivityIndicatorViewStyleWhiteLarge;
  [activityIndicator startAnimating];
  activityIndicator.center = self.view.center;
  [self.view addSubview:activityIndicator];

  /* Set alignments */
  [activityIndicator setAutoresizingMask:UIViewAutoresizingFlexibleLeftMargin | UIViewAutoresizingFlexibleRightMargin |
   UIViewAutoresizingFlexibleTopMargin | UIViewAutoresizingFlexibleBottomMargin];

  /* Add tap gesture recognizer */
  UITapGestureRecognizer* singleTap =
    [[[UITapGestureRecognizer alloc] initWithTarget:self
                                             action:@selector(handleSingleTap:)] autorelease];
  [self.view addGestureRecognizer:singleTap];
}

- (void)handleSingleTap:(UITapGestureRecognizer*)recognizer
{
  /* Call Reach module in order to re-scan the contents */
  AEReachModule* module = (AEReachModule*)[[EngagementAgent shared] getModule:kAEReachModuleName];
  [module onLoadingViewDismissed];

  /* Dismiss self */
  [AEViewControllerUtil dismissViewController:self animated:NO];
}

- (void)dealloc
{
  self.dlcId = nil;
  [super dealloc];
}

@end
