/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEViewControllerUtil.h"
#import "AEAutorotateView.h"

#define ANIMATION_DURATION 0.4f

@implementation AEViewControllerUtil

static NSMutableArray* currentDisplayedControllers;

+ (void)initialize
{
  static BOOL initialized = NO;
  if (!initialized)
  {
    currentDisplayedControllers = [[NSMutableArray alloc] init];
    initialized = YES;
  }
}

+ (void)presentViewController:(UIViewController*)controller animated:(BOOL)animated
{
  AEAutorotateView* container = [[AEAutorotateView alloc] initWithContent:controller.view];
  
  /* Add view to the current window */
  [[[self class] availableWindow] addSubview:container];
  [container release];
  
  /* Animate opening */
  container.transform = CGAffineTransformTranslate(container.transform, 0.0, container.frame.size.height);
  [UIView beginAnimations:nil context:UIGraphicsGetCurrentContext()];
  [UIView setAnimationDuration:animated ? ANIMATION_DURATION : 0.0];
  container.transform = CGAffineTransformTranslate(container.transform, 0.0, -container.frame.size.height);
  [UIView commitAnimations];
  
  /* Keep track of the controller */
  [currentDisplayedControllers addObject:controller];
}

+ (void)dismissViewController:(UIViewController*)controller
{
  [self dismissViewController:controller animated:YES];
}

+ (void)dismissViewController:(UIViewController*)controller animated:(BOOL)animated
{
  if ([controller.view.superview isKindOfClass:[AEAutorotateView class]])
  {
    UIView* view = controller.view.superview;
    [controller dismissModalViewControllerAnimated:NO];

    if (animated)
    {
      /* Animate hiding */
      [UIView beginAnimations:nil context:UIGraphicsGetCurrentContext()];
      [UIView setAnimationDuration:ANIMATION_DURATION];
      view.transform = CGAffineTransformTranslate(view.transform, 0.0, view.frame.size.height);
      [UIView commitAnimations];

      /* Remove from view hierarchy at the end of the animation */
      [view performSelector:@selector(removeFromSuperview) withObject:nil afterDelay:ANIMATION_DURATION];
    } else
    {
      /* Remove from view hierarchy */
      [view removeFromSuperview];
    }
  }

  /* Remove controller from dictionary */
  [currentDisplayedControllers removeObject:controller];
}

+ (UIWindow*)availableWindow
{
  /* Key window is the best choice */
  UIWindow* keyW = [[UIApplication sharedApplication] keyWindow];
  if ([[self class] canUseWindow:keyW])
    return keyW;

  /* Key window cannot be used, find another one */
  else
    for (UIWindow* w in[[UIApplication sharedApplication] windows])
      if ([[self class] canUseWindow:w])
        return w;

  /* Fallback on the first window */
  return ([UIApplication sharedApplication].windows)[0];
}

/* Return YES if the window can be used to display the notification */
+ (BOOL)canUseWindow:(UIWindow*)win
{
  return win != nil && win.windowLevel == UIWindowLevelNormal;
}

@end
