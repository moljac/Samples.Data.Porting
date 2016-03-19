//
//  ViewController.h
//  OpenTokRTC
//
//  Copyright © 2016 TokBox, Inc. All rights reserved.
//

#import <UIKit/UIKit.h>

@interface ViewController : UIViewController <UIGestureRecognizerDelegate, UITextFieldDelegate>

@property (strong, nonatomic) IBOutlet UILabel *appTitle;
@property (strong, nonatomic) IBOutlet UITextField *roomNameTxtField;
@property (strong, nonatomic) IBOutlet UITextField *userNameTxtField;
@property (strong, nonatomic) IBOutlet UILabel *roomNameLbl;
@property (strong, nonatomic) IBOutlet UILabel *userNameNameLbl;
@property (strong, nonatomic) IBOutlet UIView *roomNameLineView;
@property (strong, nonatomic) IBOutlet UIView *userNameLineView;
@property (strong, nonatomic) IBOutlet UIButton *joinButton;
@property (strong, nonatomic) IBOutlet UILabel *creditsLbl;

@end

