/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEDefaultAnnouncementViewController.h"
#import "AEReachAnnouncement.h"

@implementation AEDefaultAnnouncementViewController

- (instancetype)initWithAnnouncement:(AEReachAnnouncement*)anAnnouncement
{
  self = [super init];
  if (self != nil)
  {
    /* Keep announcement */
    self.announcement = anAnnouncement;

    /* Javascript bridge */
    self.jsBridge = [AEWebAnnouncementJsBridge jsBridgeWithDelegate:self];
  }
  return self;
}

- (void)dealloc
{
  [self.webView setDelegate:nil];
  self.jsBridge = nil;
  self.titleBar = nil;
  self.textView = nil;
  self.webView = nil;
  self.toolbar = nil;
  [super dealloc];
}

- (void)viewDidLoad
{

  [super viewDidLoad];

  /* Init toolbar */
  [self loadToolbar:self.toolbar];

  /* Init announcement title */
  self.titleBar.topItem.title = self.announcement.title;
  self.titleBar.hidden = [self.announcement.title length] == 0;

  /* Hide toolbar if both action and label buttons are empty */
  self.toolbar.hidden = [self.announcement.actionLabel length] == 0 && [self.announcement.exitLabel length] == 0;

  /* Move and resize other views accordingly */
  if (self.titleBar.hidden)
  {
    CGRect frame = self.textView.frame;
    frame.origin.y = 0;
    frame.size.height += self.titleBar.frame.size.height;
    self.textView.frame = frame;
    self.webView.frame = frame;
  }
  if (self.toolbar.hidden)
  {
    CGRect frame = self.textView.frame;
    frame.size.height += self.toolbar.frame.size.height;
    self.textView.frame = frame;
    self.webView.frame = frame;
  }

  /* Init announcement body based on type */
  if (self.announcement.type == AEAnnouncementTypeHtml)
  {
    self.textView.hidden = YES;
    self.webView.hidden = NO;
    [self.webView setDelegate:self.jsBridge];
    [self.webView loadHTMLString:self.announcement.body baseURL:[NSURL URLWithString:@"http://localhost/"]];
  } else
  {
    self.textView.hidden = NO;
    self.webView.hidden = YES;
    self.textView.text = self.announcement.body;
  }
}

- (BOOL)shouldAutorotateToInterfaceOrientation:(UIInterfaceOrientation)toInterfaceOrientation
{
  return YES;
}

- (void)actionButtonClicked:(id)sender
{
  [self action];
}

@end
