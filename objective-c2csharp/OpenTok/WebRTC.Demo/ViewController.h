//
//  ViewController.h
//  webrtcDemoiOS
//
//  Created by Song Zheng on 8/14/13.
//  Copyright (c) 2013 Song Zheng. All rights reserved.
//

#import <UIKit/UIKit.h>
#import "RoomViewController.h"

@interface ViewController : UIViewController <UIGestureRecognizerDelegate, UITextFieldDelegate>

@property (strong, nonatomic) IBOutlet UILabel *appTitle;
@property (strong, nonatomic) IBOutlet UITextField *roomNameTxtField;
@property (retain, nonatomic) IBOutlet UITextField *userNameTxtField;
@property (strong, nonatomic) IBOutlet UILabel *roomNameLbl;
@property (strong, nonatomic) IBOutlet UILabel *userNameNameLbl;
@property (strong, nonatomic) IBOutlet UIView *roomNameLineView;
@property (strong, nonatomic) IBOutlet UIView *userNameLineView;
@property (strong, nonatomic) IBOutlet UIButton *joinButton;
@property (strong, nonatomic) IBOutlet UILabel *creditsLbl;

@end
