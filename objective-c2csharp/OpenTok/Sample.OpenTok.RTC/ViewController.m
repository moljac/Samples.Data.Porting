//
//  ViewController.m
//  OpenTokRTC
//
//  Copyright © 2016 TokBox, Inc. All rights reserved.
//

#import "ViewController.h"
#import "RoomViewController.h"

#define OPENTOK_INFO @"OpenTokInfo"
#define OPENTOK_USER_NAME @"OpenTokUserName"
#define OPENTOK_ROOM_NAME @"OpenTokRoomName"

@interface ViewController () {
    UIFont *avantGarde;
}
@end

@implementation ViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    
    // Change fonts of all text on the screen
    [_roomNameTxtField setFont:[UIFont fontWithName:@"AvantGardeITCbyBT-Book" size:22.0]];
    [_appTitle setFont:[UIFont fontWithName:@"AvantGardeITCbyBT-Book" size:35.0]];
    [_roomNameLbl setFont:[UIFont fontWithName:@"AvantGardeITCbyBT-Book" size:13.0]];
    [_joinButton.titleLabel setFont:avantGarde];
    
    // listen to taps around the screen, and hide keyboard when necessary
    // FIXME: the action is empty
    UITapGestureRecognizer *tgr = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(viewTapped:)];
    tgr.delegate = self;
    [self.view addGestureRecognizer:tgr];
    
    // set up the look of the page
    [self.navigationController setNavigationBarHidden:NO];
    
    // FIXME: should the name caching concern be factored elsewhere? a model?
    NSDictionary *userInfo = [[NSUserDefaults standardUserDefaults]
                              valueForKey:OPENTOK_INFO];
    if(!userInfo)
    {
        self.userNameTxtField.text = [[UIDevice currentDevice] name];
    }
    else
    {
        self.roomNameTxtField.text = [userInfo valueForKey:OPENTOK_ROOM_NAME];
        self.userNameTxtField.text = [userInfo valueForKey:OPENTOK_USER_NAME];
    }
    
    self.title = @"OpenTokRTC";
    self.navigationController.navigationBar.barTintColor = [UIColor colorWithRed:40.0f/255.0f
                                                                           green:40.0f/255.0f
                                                                            blue:40.0f/255.0f
                                                                           alpha:1.0];
    
    self.navigationController.navigationBar.titleTextAttributes = @{
                                                                    NSForegroundColorAttributeName: [UIColor whiteColor]
                                                                    };
    self.navigationController.navigationBar.translucent = NO;
}

- (void)viewWillAppear:(BOOL)animated {
    // FIXME: is this redundant with the same line in viewDidLoad:?
    self.title = @"OpenTokRTC";
    // FIXME: why is this method being called?
    [self willAnimateRotationToInterfaceOrientation:[[UIApplication sharedApplication] statusBarOrientation] duration:1.0];
}

- (void)viewWillDisappear:(BOOL)animated {
    // FIXME: why?
    self.title = nil;
}

- (UIStatusBarStyle)preferredStatusBarStyle {
    return UIStatusBarStyleLightContent;
}


- (void)didReceiveMemoryWarning {
    [super didReceiveMemoryWarning];
    // Dispose of any resources that can be recreated.
}

- (void)willAnimateRotationToInterfaceOrientation:(UIInterfaceOrientation)toInterfaceOrientation duration:(NSTimeInterval)duration {
    // FIXME: the method returns a BOOL, why is it being compared with `==`?
    // FIXME: way too many hard-coded frames! use autolayout
    if (toInterfaceOrientation == UIInterfaceOrientationIsPortrait(toInterfaceOrientation))
    {
        self.appTitle.frame = CGRectMake(20,66,280,49);
        
        self.roomNameTxtField.frame = CGRectMake(65,121,190,43);
        self.roomNameLineView.frame = CGRectMake(65,159,190,1);
        self.roomNameLbl.frame  = CGRectMake(65,159,142,21);
        
        self.userNameTxtField.frame = CGRectMake(65,204,190,43);
        self.userNameLineView.frame = CGRectMake(65,240,190,1);
        self.userNameNameLbl.frame = CGRectMake(65,239,142,21);
        
        self.joinButton.frame =
        CGRectMake(119, 296, 82, 42);
        
        self.creditsLbl.frame = CGRectMake(93, 436, 134, 21);
    } else {
        self.appTitle.frame = CGRectMake(144,-5,280,49);
        
        self.roomNameTxtField.frame = CGRectMake(189,52,190,43);
        self.roomNameLineView.frame = CGRectMake(189,90,190,1);
        self.roomNameLbl.frame  = CGRectMake(189,90,142,21);
        
        self.userNameTxtField.frame = CGRectMake(189,127,190,43);
        self.userNameLineView.frame = CGRectMake(189,163,190,1);
        self.userNameNameLbl.frame = CGRectMake(189,162,142,21);
        
        self.joinButton.frame =
        CGRectMake(243, 200, 82, 42);
        
        self.creditsLbl.frame = CGRectMake(414, 226, 134, 21);
    }
}

#pragma mark - Gestures

- (BOOL)gestureRecognizer:(UIGestureRecognizer *)gestureRecognizer shouldReceiveTouch:(UITouch *)touch {
    // FIXME: this should use the wrapped in UIScrollView pattern to react to the keyboard being show and hidden
    if ([touch.view isKindOfClass:[UIControl class]]) {
        // user tapped on buttons or input fields
    } else {
        [self.roomNameTxtField resignFirstResponder];
        [self.userNameTxtField resignFirstResponder];
    }
    return YES;
}

- (void)viewTapped:(UITapGestureRecognizer *)tgr
{
    // user tapped on the view
}

#pragma mark - User Interaction

- (BOOL)shouldPerformSegueWithIdentifier:(NSString *)identifier sender:(id)sender {
    // FIXME: factor out validation into a model
    NSString* inputRoomName = [[_roomNameTxtField text] stringByReplacingOccurrencesOfString:@" " withString:@""];;
    return (inputRoomName.length >= 1) ? YES : NO;
}

- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender
{
    // user clicks button, prepares to join room
    if ([[segue identifier] isEqualToString:@"startChat"])
    {
        [_roomNameTxtField resignFirstResponder];
        // FIXME: why capture this reference?
        NSDictionary *tempUserInfo = [[NSUserDefaults standardUserDefaults]
                                      valueForKey:OPENTOK_INFO];
        // FIXME: why mutable?
        NSMutableDictionary *userInfo = [NSMutableDictionary
                                         dictionaryWithDictionary:tempUserInfo];
        [userInfo setValue:self.roomNameTxtField.text forKey:OPENTOK_ROOM_NAME];
        [userInfo setValue:self.userNameTxtField.text forKey:OPENTOK_USER_NAME];
        
        [[NSUserDefaults standardUserDefaults] setValue:userInfo
                                                 forKey:OPENTOK_INFO];
        // FIXME: async?
        [[NSUserDefaults standardUserDefaults] synchronize];
        
        RoomViewController *vc = [segue destinationViewController];
        // FIXME: this replacement happens twice, it should just be read from a model
        vc.rid = [[_roomNameTxtField text] stringByReplacingOccurrencesOfString:@" " withString:@""];
        vc.publisherName = [[self.userNameTxtField text] stringByReplacingOccurrencesOfString:@" " withString:@""];
    }
}

#pragma mark - Chat textfield

- (void)textFieldDidEndEditing:(UITextField *)textField
{
    // called after the text field resigns its first responder status
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField
{
    [textField resignFirstResponder];
    return NO;
}


@end
