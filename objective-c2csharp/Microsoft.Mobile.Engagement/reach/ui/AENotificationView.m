/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AENotificationView.h"
#import "AEDefaultNotifier.h"

#define OFFSET_X 5.0

@implementation AENotificationView

- (void)dealloc
{
  [_titleView release];
  [_messageView release];
  [_iconView release];
  [_imageView release];
  [_notificationButton release];
  [_closeButton release];
  [super dealloc];
}

- (void)awakeFromNib
{
  _titleView = (UILabel*)[[self viewWithTag:NOTIFICATION_TITLE_TAG] retain];
  _messageView = (UILabel*)[[self viewWithTag:NOTIFICATION_MESSAGE_TAG] retain];
  _iconView = (UIImageView*)[[self viewWithTag:NOTIFICATION_ICON_TAG] retain];
  _imageView = (UIImageView*)[[self viewWithTag:NOTIFICATION_IMAGE_TAG] retain];
  _notificationButton = (UIButton*)[[self viewWithTag:NOTIFICATION_BUTTON_TAG] retain];
  _closeButton = (UIButton*)[[self viewWithTag:NOTIFICATION_CLOSE_TAG] retain];
}

- (void)layoutSubviews
{
  /* Icon view always on the left */
  if (!self.iconView.hidden)
  {
    CGRect iconFrame = self.iconView.frame;
    iconFrame.origin.x = OFFSET_X;
    self.iconView.frame = iconFrame;
  }

  /* Close button always on the right */
  if (!self.closeButton.hidden)
  {
    CGRect closeButtonFrame = self.closeButton.frame;
    closeButtonFrame.origin.x = self.frame.size.width - OFFSET_X - closeButtonFrame.size.width;
    self.closeButton.frame = closeButtonFrame;
  }

  /* Move and resize image view */
  if (!self.imageView.hidden)
  {
    CGRect imageViewFrame = self.imageView.frame;
    imageViewFrame.size.width =
      MIN(self.frame.size.width,
          floor(self.imageView.frame.size.height*self.imageView.image.size.width/self.imageView.image.size.height));

    /* If title and message labels are hidden, display image at the left */
    if (self.titleView.hidden && self.messageView.hidden)
    {
      imageViewFrame.origin.x = self.iconView.hidden ? 0.0 : CGRectGetMaxX(self.iconView.frame);
      self.imageView.autoresizingMask = UIViewAutoresizingFlexibleRightMargin;
    }
    /* Otherwise display image at the right */
    else
    {
      imageViewFrame.origin.x = self.frame.size.width - imageViewFrame.size.width -
                                (self.closeButton.hidden ? 0 : self.frame.size.width -
                                 CGRectGetMinX(self.closeButton.frame));
      self.imageView.autoresizingMask = UIViewAutoresizingFlexibleLeftMargin;
    }

    self.imageView.frame = imageViewFrame;
  }

  /* Horizontal alignement of labels */
  {
    /* By default, labels take the whole notification width minus an offset */
    CGRect titleViewFrame = CGRectMake(OFFSET_X, self.titleView.frame.origin.y, self.frame.size.width - 2*OFFSET_X,
                                       self.titleView.frame.size.height);
    CGRect messageViewFrame = CGRectMake(OFFSET_X, self.messageView.frame.origin.y, self.frame.size.width - 2*OFFSET_X,
                                         self.messageView.frame.size.height);

    /* Labels must be at the left of the icon view */
    if (!self.iconView.hidden)
    {
      titleViewFrame.origin.x += CGRectGetMaxX(self.iconView.frame);
      titleViewFrame.size.width -= CGRectGetMaxX(self.iconView.frame);
      messageViewFrame.origin.x += CGRectGetMaxX(self.iconView.frame);
      messageViewFrame.size.width -= CGRectGetMaxX(self.iconView.frame);
    }

    /* Shorten labels if image view is visible */
    if (!self.imageView.hidden)
    {
      titleViewFrame.size.width -= CGRectGetWidth(self.imageView.frame);
      messageViewFrame.size.width -= CGRectGetWidth(self.imageView.frame);
    }

    /* Shorten labels if close button is visible */
    if (!self.closeButton.hidden)
    {
      titleViewFrame.size.width -= CGRectGetWidth(self.closeButton.frame);
      messageViewFrame.size.width -= CGRectGetWidth(self.closeButton.frame);
    }

    self.titleView.frame = titleViewFrame;
    self.messageView.frame = messageViewFrame;
  }

  /* Vertical alignement of labels */
  {
    CGRect titleViewFrame = self.titleView.frame;
    CGRect messageViewFrame = self.messageView.frame;

    /* Vertically align message label if title label is hidden */
    if (self.titleView.hidden)
    {
      messageViewFrame.origin.y = floor(CGRectGetHeight(self.frame)/2 - messageViewFrame.size.height/2);
    }
    /* Vertically align title label if message label is hidden */
    else if (self.messageView.hidden)
    {
      titleViewFrame.origin.y = floor(CGRectGetHeight(self.frame)/2 - titleViewFrame.size.height/2);
    }
    /* Otherwise, display title first and the message label below */
    else
    {
      titleViewFrame.origin.y = 9.0;
      messageViewFrame.origin.y = 25.0;
    }

    self.titleView.frame = titleViewFrame;
    self.messageView.frame = messageViewFrame;
  }
}

@end
