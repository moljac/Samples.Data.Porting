//
//  RoomViewController.h
//  OpenTokRTC
//
//  Copyright © 2016 TokBox, Inc. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <OpenTok/OpenTok.h>

@interface RoomViewController : UIViewController <OTSessionDelegate, OTPublisherDelegate,
    UITextFieldDelegate, UIGestureRecognizerDelegate, UIScrollViewDelegate>

@property (strong, nonatomic) IBOutlet UIScrollView *videoContainerView;
@property (strong, nonatomic) IBOutlet UIView *bottomOverlayView;
@property (strong, nonatomic) IBOutlet UIView *topOverlayView;
@property (strong, nonatomic) IBOutlet UIButton *cameraToggleButton;
@property (strong, nonatomic) IBOutlet UIButton *audioPubUnpubButton;
@property (strong, nonatomic) IBOutlet UILabel *userNameLabel;
@property (strong, nonatomic) IBOutlet UIButton *audioSubUnsubButton;
@property (strong, nonatomic) IBOutlet UIButton *endCallButton;
@property (strong, nonatomic) IBOutlet UIView *archiveOverlay;
@property (strong, nonatomic) IBOutlet UILabel *archiveStatusLbl;
@property (strong, nonatomic) IBOutlet UIImageView *archiveStatusImgView;
@property (strong, nonatomic) IBOutlet UIImageView *rightArrowImgView;
@property (strong, nonatomic) IBOutlet UIImageView *leftArrowImgView;
@property (strong, nonatomic) IBOutlet UIActivityIndicatorView *spinningWheel;

@property (strong, nonatomic) NSTimer *overlayTimer;

@property (strong, nonatomic) NSString *rid;
@property (strong, nonatomic) NSString *publisherName;

- (IBAction)toggleAudioSubscribe:(id)sender;
- (IBAction)toggleCameraPosition:(id)sender;
- (IBAction)toggleAudioPublish:(id)sender;
- (IBAction)endCallAction:(UIButton *)button;

@end
