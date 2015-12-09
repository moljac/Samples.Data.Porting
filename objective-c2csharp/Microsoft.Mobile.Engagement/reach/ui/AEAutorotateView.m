/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEAutorotateView.h"
#import "AEViewControllerUtil.h"

#define ANIMATION_DURATION 0.4

@implementation AEAutorotateView

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Memory management
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (id)initWithContent:(UIView*)view
{
  self = [self init];
  if (self != nil)
  {
    _view = [view retain];
    [self sizeToFitOrientation:[UIApplication sharedApplication].statusBarOrientation];
    view.frame = self.bounds;
    [self addSubview:_view];
  }
  return self;
}

- (id)init
{
  self = [super initWithFrame:CGRectZero];
  if (self != nil)
  {
    _orientation = [UIApplication sharedApplication].statusBarOrientation;
    self.opaque = YES;
  }
  return self;
}

- (void)dealloc
{
  /* Stop listening to orientation changes */
  [[NSNotificationCenter defaultCenter] removeObserver:self name:UIApplicationDidChangeStatusBarOrientationNotification
                                                object:nil];
  [[NSNotificationCenter defaultCenter] removeObserver:self name:UIApplicationDidChangeStatusBarFrameNotification
                                                object:nil];
  [_view release];
  [super dealloc];
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Orientation
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (void)sizeToFitOrientation:(UIInterfaceOrientation)orientation
{

  /* Get application frame */
  CGRect fixedAppFrame = [UIScreen mainScreen].applicationFrame;
  CGRect orientedAppFrame = [self orientedApplicationFrame];
  
  CGPoint center = CGPointMake(fixedAppFrame.origin.x + ceil(fixedAppFrame.size.width/2), fixedAppFrame.origin.y + ceil(fixedAppFrame.size.height/2));
  CGRect newFrame = CGRectMake(0, 0, orientedAppFrame.size.width, orientedAppFrame.size.height);
  
  /* Keep current orientation */
  _orientation = orientation;

  /* Adjust size depending on containing view */
  if (_view)
  {
    /* If content height is fixed, take whole screen height */
    if ((_view.autoresizingMask & UIViewAutoresizingFlexibleHeight) != UIViewAutoresizingFlexibleHeight)
      newFrame.size.height = _view.frame.size.height;

    /* If content width is fixed, take whole screen width */
    if ((_view.autoresizingMask & UIViewAutoresizingFlexibleWidth) != UIViewAutoresizingFlexibleWidth)
      newFrame.size.width = _view.frame.size.width;
  }

  /* Update frame */
  self.frame = newFrame;

  /* Center view on the screen */
  self.center = center;
}

-(void)resize
{
  /* Clear any transform before resizing */
  self.transform = CGAffineTransformIdentity;
  
  /* Resize view based on current orientation */
  [self sizeToFitOrientation:_orientation];
  
  /* Transform (rotate + translate) based on current orientation */
  self.transform = [self transformForOrientation:_orientation];
  
}

- (void)statusBarOrientationDidChange:(void*)object
{
  if (_orientation != [UIApplication sharedApplication].statusBarOrientation)
  {
    UIInterfaceOrientation newOrientation = [UIApplication sharedApplication].statusBarOrientation;
    float duration = [UIApplication sharedApplication].statusBarOrientationAnimationDuration;
    if ((UIInterfaceOrientationIsPortrait(_orientation) && UIInterfaceOrientationIsPortrait(newOrientation)) ||
        (UIInterfaceOrientationIsLandscape(_orientation) && UIInterfaceOrientationIsLandscape(newOrientation)))
      duration *= 2;

    [UIView beginAnimations:nil context:nil];
    [UIView setAnimationBeginsFromCurrentState:YES];
    [UIView setAnimationDuration:duration];

    self.transform = CGAffineTransformIdentity;
    [self sizeToFitOrientation:newOrientation];
    self.transform = [self transformForOrientation:newOrientation];

    [UIView commitAnimations];
  }
}

- (void)statusBarFrameDidChange:(void*)object
{
  /* Delay resize to avoid a bug in iOS 8 where the OS still return the old frame at this point of time */
  [self performSelector:@selector(resize) withObject:nil afterDelay:0.1];
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (CGAffineTransform)transformForOrientation:(UIInterfaceOrientation)orientation
{
  CGAffineTransform t = CGAffineTransformIdentity;
  CGRect appFrame = [self orientedApplicationFrame];
  
  /* If device doesn't support automatic rotation, we have to do it manually */
  if(![self hasAutomaticRotation])
  {
    if (orientation == UIInterfaceOrientationLandscapeLeft)
    {
      t = CGAffineTransformMakeRotation(-M_PI/2);
    } else if (orientation == UIInterfaceOrientationLandscapeRight)
    {
      t = CGAffineTransformMakeRotation(M_PI/2);
    } else if (orientation == UIInterfaceOrientationPortraitUpsideDown)
    {
      t = CGAffineTransformMakeRotation(M_PI);
    }
  }
  /* Translate view based on containing view resizing options */
  if (_view)
  {
    /* If content height is fixed */
    if ((_view.autoresizingMask & UIViewAutoresizingFlexibleHeight) != UIViewAutoresizingFlexibleHeight)
    {
      /* If content bottom margin is fixed and top margin is flexible, move container view at the bottom of the screen
      **/
      if (((_view.autoresizingMask & UIViewAutoresizingFlexibleBottomMargin) !=
           UIViewAutoresizingFlexibleBottomMargin) &&
          ((_view.autoresizingMask & UIViewAutoresizingFlexibleTopMargin) == UIViewAutoresizingFlexibleTopMargin))
      {
          t =
            CGAffineTransformTranslate(t, 0.0f, (CGRectGetHeight(appFrame) - _view.frame.size.height)/2);
      }

      /* If content top margin is fixed and bottom margin is flexible, move container view at the top of the screen */
      if (((_view.autoresizingMask & UIViewAutoresizingFlexibleTopMargin) != UIViewAutoresizingFlexibleTopMargin) &&
          ((_view.autoresizingMask & UIViewAutoresizingFlexibleBottomMargin) == UIViewAutoresizingFlexibleBottomMargin))
      {
          t =
            CGAffineTransformTranslate(t, 0.0f,-(CGRectGetHeight(appFrame) - _view.frame.size.height)/2);
      }
    }
  }

  return t;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIView methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (void)willMoveToSuperview:(UIView*)newSuperview
{
  [super willMoveToSuperview:newSuperview];

  /* When removeFromSuperView is called, this method is called with a nil argument, ignore in that case.  */
  if (newSuperview)
  {
    /* Resize view */
    [self sizeToFitOrientation:[UIApplication sharedApplication].statusBarOrientation];
  }
}

- (void)didMoveToSuperview
{
  [super didMoveToSuperview];

  /* This method is also called when removeFromSuperview is called, ignore if the superview is nil. */
  if (self.superview)
  {
    /* Apply transform based on current orientation */
    self.transform = [self transformForOrientation:[UIApplication sharedApplication].statusBarOrientation];

    /* Listen to orientation changes */
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(statusBarOrientationDidChange:)
                                                 name:UIApplicationDidChangeStatusBarOrientationNotification
                                               object:nil];
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(statusBarFrameDidChange:)
                                                 name:UIApplicationDidChangeStatusBarFrameNotification
                                               object:nil];
  }
}

- (void)removeFromSuperview
{
  /* Stop listening to orientation changes */
  [[NSNotificationCenter defaultCenter] removeObserver:self
                                                  name:UIApplicationDidChangeStatusBarOrientationNotification
                                                object:nil];
  [[NSNotificationCenter defaultCenter] removeObserver:self
                                                  name:UIApplicationDidChangeStatusBarFrameNotification
                                                object:nil];

  [super removeFromSuperview];
}

/**
 * Retrieve application frame considering that 0,0 point is at the top left of the screen.
 */
-(CGRect)orientedApplicationFrame
{
  CGRect frame = [UIScreen mainScreen].applicationFrame;
  if(![[UIScreen mainScreen] respondsToSelector:@selector(fixedCoordinateSpace)] &&
     UIInterfaceOrientationIsLandscape([UIApplication sharedApplication].statusBarOrientation))
  {
    CGRect temp = {frame.origin, frame.size};
    frame.origin.x = temp.origin.y;
    frame.origin.y = temp.origin.x;
    frame.size.width = temp.size.height;
    frame.size.height = temp.size.width;
  }
  return frame;
}

/**
 * @result YES, if device supports automatic rotation, NO otherwise.
 */
-(BOOL)hasAutomaticRotation
{
  /* iOS 8 will auto rotate, no need to make transform */
  return [[UIScreen mainScreen] respondsToSelector:@selector(fixedCoordinateSpace)];
}

@end
