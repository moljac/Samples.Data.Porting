/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEWebAnnouncementJsBridge.h"

static NSString* const kBridgeScript = @""
                                       "var engagementReachContent = {"
                                       "  actionContent: function() {"
                                       "    window.location.href = 'engagement://reach/action';"
                                       "  },"
                                       "  exitContent: function() {"
                                       "    window.location.href = 'engagement://reach/exit';"
                                       "  }"
                                       "};";

@implementation AEWebAnnouncementJsBridge

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Memory management
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

+ (id)jsBridgeWithDelegate:(id<AEWebAnnouncementActionDelegate>)delegate
{
  return [[[self alloc] initWithDelegate:delegate] autorelease];
}

- (id)initWithDelegate:(id<AEWebAnnouncementActionDelegate>)delegate
{
  self = [super init];
  if (self)
  {
    _delegate = [delegate retain];
  }

  return self;
}

- (void)dealloc
{
  [_delegate release];
  [super dealloc];
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark UIWebViewDelegate methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (void)webViewDidFinishLoad:(UIWebView*)webView
{
  [webView stringByEvaluatingJavaScriptFromString:kBridgeScript];
}

- (BOOL)webView:(UIWebView*)webView shouldStartLoadWithRequest:(NSURLRequest*)request navigationType:(
    UIWebViewNavigationType)navigationType
{
  NSURL* url = [request URL];
  if ([[url scheme] isEqualToString:@"engagement"])
  {
    NSString* command = [url lastPathComponent];
    if ([[url host] isEqualToString:@"reach"])
    {
      SEL commandSel = NSSelectorFromString(command);
      if ([_delegate respondsToSelector:commandSel])
        [_delegate performSelector:commandSel];
    }
    return NO;
  } else
    return YES;
}

@end
