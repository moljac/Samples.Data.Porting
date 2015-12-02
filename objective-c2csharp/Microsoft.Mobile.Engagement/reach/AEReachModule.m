/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import <objc/runtime.h>
#import "AEReachModule.h"
#import "EngagementAgent.h"
#import "AEReachDataPush.h"
#import "AEReachNotifAnnouncement.h"
#import "AEReachAnnouncement.h"
#import "AEReachPoll.h"
#import "AEDefaultAnnouncementViewController.h"
#import "AEAnnouncementViewController.h"
#import "AEDefaultPollViewController.h"
#import "AEViewControllerUtil.h"
#import "AEPushMessage.h"
#import "AEModule.h"
#import "AEStorage.h"
#import "AEContentViewController.h"
#import "AEDefaultNotifier.h"
#import "AEContentStorage.h"
#import "AEFileStorage.h"
#import "AEContentLoadingViewController.h"
#import "AEReachFeedback.h"

/**
 * Downloadable Content type.
 */
typedef NS_ENUM (NSInteger, AEDLCType)
{
  /** Unknown type */
  AEDLCUnknownType = -1,

  /** Content not available for download => 00000000 */
  AEDLCUnavailableType = (0 << 0),

  /** Notification has downloadable content => 00000001 */
  AEDLCAvailableType = (1 << 0),

  /** Clickable content exists => 00000010 */
  AEDLCAvailableClickableType = (1 << 1)
};

NSString* const kAEReachModuleName = @"ReachModule";
NSString* const kAEReachNamespace = @"urn:ubikod:ermin:reach:0";
NSString* const kAEReachDefaultCategory = @"engagement.category.default";

static NSString* const kAERemoteNotifReachTestIdKey = @"-1";
static NSString* const kAEReachDBName = @"engagement.reach";
static double const kAEReachDBVersion = 2.2;
static NSString* const kAEReachFeedbackDBName = @"engagement.reachFeedback";
static double const kAEReachFeedbackDBVersion = 1.0;
static NSUInteger const kAEReachFeedbackCapacity = 30;
static NSString* const kAELastPolledTime = @"AEReachModuleLastPolledTime";
static NSString* const kAELastPolledTimeBackup = @"AEReachModuleLastPolledTimeBackup";

/* Polling threshold time in milliseconds (15 Min) */
static long const kAEReachPollingTimeThreshold = 15 * 60000;

/* Max poll time in milliseconds (1 month) */
static long long const kAEReachMaxPollTime = 2629746000;

/**********************************************************************************************************************/
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark -
#pragma mark Reach module implementation
#pragma mark -
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/**********************************************************************************************************************/

/* Real implementation of the reach module */
@implementation AEReachModule

- (id)initWithNotificationIcon:(UIImage*)icon
{
  self = [super init];
  if (self != nil)
  {
    _state = AEReachModuleStateIdle;

    /* Storage holding reach contents */
    _db = [[AEStorage storageWithName:kAEReachDBName version:kAEReachDBVersion autoSync:YES] retain];
    [_db setDelegate:self];

    /* Reach feedback storage */
    _feedbackDb =
      [[AEStorage storageWithName:kAEReachFeedbackDBName version:kAEReachFeedbackDBVersion autoSync:YES] retain];
    [_feedbackDb setCapacity:kAEReachFeedbackCapacity];

    /* Set of messages pending download */
    _pendingDlcs = [[NSMutableIndexSet alloc] init];

    /* Default maximum number of contents */
    _maxContents = 64;

    /* Trash */
    _trash = [[NSMutableIndexSet alloc] init];

    /* Create dictionary holding announcement and poll controller classes for each category */
    _announcementControllers = [[NSMutableDictionary alloc] init];
    _pollControllers = [[NSMutableDictionary alloc] init];

    /* Create dictionary holding notification handlers for each category */
    _notifiers = [[NSMutableDictionary alloc] init];

    /* Default categories */
    _notifiers[kAEReachDefaultCategory] = [AEDefaultNotifier notifierWithIcon:icon];
    _announcementControllers[kAEReachDefaultCategory] = [AEDefaultAnnouncementViewController class];
    _pollControllers[kAEReachDefaultCategory] = [AEDefaultPollViewController class];

    /* Register for system call applicationDidBecomeActive */
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(applicationDidBecomeActive)
                                                 name:UIApplicationDidBecomeActiveNotification
                                               object:nil];

    /* Register for system call didFinishLaunchingNotification */
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(applicationDidFinishLaunching:)
                                                 name:UIApplicationDidFinishLaunchingNotification
                                               object:nil];

    /* Register for system call applicationDidEnterBackground */
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(applicationDidEnterBackground)
                                                 name:UIApplicationDidEnterBackgroundNotification
                                               object:nil];

    /* Register for system call applicationWillResignActive */
    [[NSNotificationCenter defaultCenter] addObserver:self
                                             selector:@selector(applicationWillResignActive)
                                                 name:UIApplicationWillResignActiveNotification
                                               object:nil];
  }
  return self;
}

- (void)dealloc
{
  /* Make sure to remove the observer */
  [[NSNotificationCenter defaultCenter] removeObserver:self name:UIApplicationDidBecomeActiveNotification object:nil];

  [_db release];
  [_feedbackDb release];
  [_trash release];
  [_currentActivity release];
  [_announcementControllers release];
  [_pollControllers release];
  [_notifiers release];
  [_params release];
  [_dataPushDelegate release];
  [_displayedController release];
  [_pendingDlcs release];
  [super dealloc];
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Public methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

+ (id)moduleWithNotificationIcon:(UIImage*)icon
{
  return [[[[self class] alloc] initWithNotificationIcon:icon] autorelease];
}

- (void)setAutoBadgeEnabled:(BOOL)enabled
{
  _autoBadgeEnabled = enabled;
}

- (void)setMaxInAppCampaigns:(NSUInteger)maxContents
{
  _maxContents = maxContents;
}

- (void)registerAnnouncementController:(Class)clazz forCategory:(NSString*)category
{
  /* Check is not started */
  if (_isStarted)
    [NSException raise:NSInternalInconsistencyException format:
     @"Reach agent already started. Please call this method before registering Engagement application."];

  /* Check given class is a subclass of AEAnnouncementViewController */
  if (![clazz isSubclassOfClass:[AEAnnouncementViewController class]])
  {
    [NSException raise:NSInvalidArgumentException
                format:
     @"Couldn't register controller to category '%@' : %@ should inherit from AEAnnouncementViewController", category,
     NSStringFromClass(clazz)];
    return;
  }

  /* Add controller class */
  _announcementControllers[category] = clazz;
}

- (void)registerPollController:(Class)clazz forCategory:(NSString*)category
{
  /* Check is not started */
  if (_isStarted)
    [NSException raise:NSInternalInconsistencyException format:
     @"Reach agent already started. Please call this method before registering Engagement application."];

  /* Check given class is a subclass of AEAnnouncementViewController */
  if (![clazz isSubclassOfClass:[AEPollViewController class]])
  {
    [NSException raise:NSInvalidArgumentException
                format:@"Couldn't register controller to category '%@' : %@ should inherit from AEPollViewController",
     category, NSStringFromClass(clazz)];
    return;
  }

  /* Add controller class */
  _pollControllers[category] = clazz;
}

- (void)applicationDidEnterBackground
{
  _didEnterBackground = YES;
}

- (void)applicationWillResignActive
{
  _willResignActive = YES;
}

- (void)registerNotifier:(id<AENotifier>)notifier forCategory:(NSString*)category
{
  /* Add notifier */
  _notifiers[category] = notifier;
}

- (void)applicationDidBecomeActive
{
  _willResignActive = NO;
  _didEnterBackground = NO;

  /* Check if polling is required */
  if ([self isPollingRequired])
  {
    /* Get time offset */
    long long lastPolledTime = [[[NSUserDefaults standardUserDefaults] valueForKey:kAELastPolledTime] longLongValue];
    long long currentTime = [[NSDate date] timeIntervalSince1970] * 1000;
    
    /* Handle invalid case: If last polled time is 0 or greater than the current time, used 1 month before as the last polled time */
    if (lastPolledTime == 0 || lastPolledTime > currentTime)
      lastPolledTime = currentTime - kAEReachMaxPollTime;

    /* Used a fixed delta time for the first launch (one month old campaigns) */
    long long timeOffset = currentTime - lastPolledTime;

    /* Get Campaigns */
    [[EngagementAgent shared] getCampaigns:[NSNumber numberWithLongLong:timeOffset]];

    /* Update the last polled time */
    [[NSUserDefaults standardUserDefaults] setObject:[NSNumber numberWithLongLong:currentTime] forKey:kAELastPolledTime];

    /* Make a backup of the last polled time to cover failure cases */
    [[NSUserDefaults standardUserDefaults] setObject:[NSNumber numberWithLongLong:lastPolledTime] forKey:kAELastPolledTimeBackup];
  }

  /* Process the cached remote notification */
  if (_cachedReachValues)
  {
    [self displayPushMessageNotification:_cachedReachValues];

    [_cachedReachValues release];
    _cachedReachValues = nil;
  }
  else
  {
    /* Re-scan the pending contents, hide existing notifications before scanning */
    [self hideNotification];
    [self scan];
  }

  /*
   * Restart applicationDidFinishLaunching flag.
   * This flag is used to identify if app is launched via system push or not
   */
  _applicationLaunchedViaSystemPush = NO;
}

/* Set the flag to indicate if app was launched via system push or not */
- (void)applicationDidFinishLaunching:(NSNotification*)notif
{
  if ([notif userInfo])
    _applicationLaunchedViaSystemPush = YES;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Module methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (void)start
{
  /* Agent now started */
  _isStarted = YES;

  /* Set storage capacity */
  [_db setCapacity:_maxContents];
  [_feedbackDb setCapacity:kAEReachFeedbackCapacity];

  /* Clear message received flag */
  _badgeNotificationReceived = NO;

  /* Action URL parameters */
  [_params release];
  _params = [@{@"{deviceid}": [[EngagementAgent shared] deviceId]} retain];

  /* Clear badge if needed */
  if (_autoBadgeEnabled && [[UIApplication sharedApplication] applicationIconBadgeNumber] > 0)
  {
    /* Clear badge */
    [self clearBadge];

    /* Clear badge appInfo */
    [self clearBadgeAppInfo];
  }
}

- (void)stop
{
  /* If auto badge enabled, clear badge value. This will also clear the notification center. */
  if (_autoBadgeEnabled)
  {
    /* Clear badge appInfo if a badge notification has been received during session */
    if (_badgeNotificationReceived)
      [self clearBadgeAppInfo];
    _badgeNotificationReceived = NO;
  }

  /* Exit current displayed controller */
  [_displayedController exit];

  /* Call loading view dismissed if the content was loading */
  if (_state == AEReachModuleStateLoading)
    [self onLoadingViewDismissed];
}

- (NSString*)name
{
  return kAEReachModuleName;
}

- (void)pushNotificationReceived:(NSDictionary*)notification
{
  /* No need to clear badge in the background */
  if ([UIApplication sharedApplication].applicationState == UIApplicationStateBackground)
    return;

  /* Clear badge */
  NSString* badge = notification[@"aps"][@"badge"];
  if (badge != nil && _autoBadgeEnabled)
  {
    /* Clear badge */
    [self clearBadge];

    /* Remember that a badge notification has been received */
    _badgeNotificationReceived = YES;
  }
}

- (void)displayPushMessageNotification:(NSDictionary*)reachValues
{
  __block bool isProcessingMsg = NO;

  /* Determine whether this message is the message being processed or not */
  [self enumerateMessagesWithLocalId:_processingId usingBlock:^(AEPushMessage* pushMessage)
  {
    if ([reachValues[kAEPushId] isEqualToString:pushMessage.reachValues[kAEPushId]])
      isProcessingMsg = YES;
  }];

  /* Parse content */
  AEPushMessage* pushMsg = [[[AEPushMessage alloc] init] autorelease];
  pushMsg.reachValues = reachValues;
  AEReachContent* content = [self parseContent:pushMsg];

  /* If content is invalid then discard it */
  if (!content)
  {
    [self markContentProcessed:content];
    return;
  }

  /* If this message is currently being processed, only send feedback */
  if (isProcessingMsg && (_state == AEReachModuleStateLoading || _state == AEReachModuleStateShowing))
  {
    [((AEInteractiveContent*)content) sendFeedback:kAEReachFeedbackSystemNotificationActioned extras:nil];
  }
  else
  {
    /* Dismiss current content if any */
    [_displayedController exit];

    /* Hide the in-app notification */
    if (_processingId == content.localId)
      [self hideNotification];

    /* This content becomes the processing one */
    _processingId = content.localId;

    /* Action this notification now */
    ((AEInteractiveContent*)content).notifiedFromNativePush = YES;
    [((AEInteractiveContent*)content) actionNotification];
  }
}

/** Process the remote notification by caching the notification and display it when app becomes active */
- (void)processRemoteNotification:(NSDictionary*)userInfo
{
  NSDictionary* reachPayload = userInfo[kAERemoteNotifReachKey];
  BOOL shouldStore = YES;

  /* If reach payload is NOT available, ignore */
  if (!reachPayload)
    return;

  /* Get reach values */
  NSMutableDictionary* reachValues = [self getReachPayloadValues:userInfo];

  /* Verify if send feedback is needed */
  BOOL shouldSendFeedback = ![reachValues[kAEContentId] isEqualToString:kAERemoteNotifReachTestIdKey];

  /* Get push message */
  AEPushMessage* pushMessage = [[[AEPushMessage alloc] init] autorelease];
  pushMessage.messageId = reachValues[kAEDlcId];
  if (reachValues[kAEPayload])
    pushMessage.payload = reachValues[kAEPayload];
  pushMessage.reachValues = reachValues;
  AEReachContent* content = [self parseContent:pushMessage];
  
  /* If content is invalid, send dropped feedback */
  if (!content && shouldSendFeedback)
  {
    [self sendFeedback:reachValues withStatus:@"dropped" andExtra:nil];
    return;
  }

  /* Ignore expired contents */
  if ([content isExpired])
    return;
  
  /* If it's a background only push */
  if ([reachValues[kAEDeliveryTime] isEqualToString:@"b"])
  {
    /* If app is in the foreground or inactive, ignore */
    if ([UIApplication sharedApplication].applicationState == UIApplicationStateActive)
      return;

    /* If app is in the background, there is no need to save data to storage */
    if ([UIApplication sharedApplication].applicationState == UIApplicationStateBackground)
      shouldStore = NO;
  }

  /* Send Delivered/Displayed feedbacks */
  if (shouldSendFeedback)
  {
    /* Delivered Feedback */
    [self sendFeedback:reachValues withStatus:kAEReachFeedbackDelivered andExtra:nil];
  }

  /* Displayed Feedback */
  if ([UIApplication sharedApplication].applicationState == UIApplicationStateBackground)
  {
    /* For Data push, ignore */
    if (![reachValues[kAEContentType] isEqualToString:@"d"])
    {
      /* System push is displayed by iOS, send displayed feedback once it's delivered
       * for the In-App push, wait till message is displayed */
      if (shouldSendFeedback &&
          ([reachValues[kAEDeliveryTime] isEqualToString:@"a"] ||
           [reachValues[kAEDeliveryTime] isEqualToString:@"b"]))
        [self sendFeedback:reachValues withStatus:kAEReachFeedbackSystemNotificationDisplayed andExtra:nil];
    }
  }

  /* Content Actioned */
  else if ([self isLaunchedViaSystemPush])
  {
    /* Cache reach values and process it when app becomes active */
    if (_cachedReachValues)
    {
      [_cachedReachValues release];
      _cachedReachValues = nil;
    }
    _cachedReachValues = reachValues;
    [_cachedReachValues retain];
  }

  /* Create push message and store it in db */
  if (shouldStore)
    [self pushMessageReceived:pushMessage];
}

- (void)pushMessageReceived:(AEPushMessage*)msg
{
  BOOL isNewMessage = YES;
  NSString* pushId = msg.reachValues[kAEPushId];

  /* Check if content is in db */
  for (AEStorageEntry* sdata in _db)
  {
    if ([sdata.data isKindOfClass:[AEPushMessage class]])
    {
      AEPushMessage* pushMsg = (AEPushMessage*)sdata.data;

      /* Check for duplicate */
      if ([pushMsg.reachValues[kAEPushId] isEqualToString:pushId])
      {
        isNewMessage = NO;
        break;
      }
    }
  }

  /* Ignore */
  if (!isNewMessage)
    return;

  /* If db is full, an entry will get purged in LIFO manner */
  [_db put:msg];

  /* Start displaying notifications(check if app is not in the background) */
  if ([UIApplication sharedApplication].applicationState == UIApplicationStateActive)
    [self scan];
}

/* Re-scan the list of contents when the current activity has changed */
- (void)activityChanged:(NSString*)newActivity
{
  /* Make sure that activity has changed when app is in active state */
  if (![_currentActivity isEqualToString:newActivity] &&
      [UIApplication sharedApplication].applicationState == UIApplicationStateActive)
  {
    /* Keep current activity */
    [_currentActivity release];
    _currentActivity = [newActivity copy];

    /* Ensure notification is hidden */
    [self hideNotification];

    /* Scan pending content */
    [self scan];
  }
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Private methods
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/* Indicate if app is launched via system push */
- (BOOL)isLaunchedViaSystemPush
{
  BOOL isComingFromBackground = [UIApplication sharedApplication].applicationState == UIApplicationStateInactive
    && _willResignActive && _didEnterBackground;
  return _applicationLaunchedViaSystemPush || isComingFromBackground;
}

/* Indicate if polling is required based on the elapsed time since last poll */
- (BOOL)isPollingRequired
{
  NSNumber* lastPolledTime = [[NSUserDefaults standardUserDefaults] valueForKey:kAELastPolledTime];
  long long currentTime = [[NSDate date] timeIntervalSince1970] * 1000;
  long long timeOffset = currentTime - [lastPolledTime longLongValue];

  /* Return true if time offset is greater than polling time threshold */
  return (kAEReachPollingTimeThreshold <= timeOffset);
}

/* Create dictionary of all reach values */
- (NSMutableDictionary*)getReachPayloadValues:(NSDictionary*)userInfo
{
  NSMutableDictionary* dic = [[[NSMutableDictionary alloc] init] autorelease];
  NSDictionary* reachPayload = userInfo[kAERemoteNotifReachKey];

  /* Set 'aps' node with Apple push data */
  if (userInfo[kAEAps])
    dic[kAEAps] = userInfo[kAEAps];

  /* Content ID and type(kind) */
  NSDictionary* contentIdAndType = [self parseContentIdAndType:reachPayload[kAECampaignId]];
  dic[kAEContentId] = contentIdAndType[@"id"];
  dic[kAEContentType] = contentIdAndType[kAEDlcType];

  /* Set reach values */
  dic[kAEDlcId] = reachPayload[kAEDlcId] ? reachPayload[kAEDlcId] : @"-1";
  dic[kAECampaignId] = reachPayload[kAECampaignId];
  dic[kAETtl] = reachPayload[kAETtl] ? [reachPayload[kAETtl] stringValue] : @"-1";
  dic[kAEUserTimeZone] = reachPayload[kAEUserTimeZone] ? [reachPayload[kAEUserTimeZone] stringValue] : @"0";
  dic[kAEDlc] = reachPayload[kAEDlc] ? [reachPayload[kAEDlc] stringValue] : @"0";
  dic[kAEDeliveryTime] = reachPayload[kAEDeliveryTime] ? reachPayload[kAEDeliveryTime] : @"a";
  dic[kAENotificationIcon] = reachPayload[kAENotificationIcon] ? [reachPayload[kAENotificationIcon] stringValue] : @"1";
  dic[kAENotificationClosable] =
    reachPayload[kAENotificationClosable] ? [reachPayload[kAENotificationClosable] stringValue] : @"1";
  if (reachPayload[kAENotificationTitle])
    dic[kAENotificationTitle] = reachPayload[kAENotificationTitle];
  if (reachPayload[kAENotificationMessage])
    dic[kAENotificationMessage] = reachPayload[kAENotificationMessage];
  if (reachPayload[kAEActionUrl])
    dic[kAEActionUrl] = reachPayload[kAEActionUrl];
  dic[kAEPushId] = reachPayload[kAEPushId] ? reachPayload[kAEPushId] : @"-1";

  /* Set payload */
  if (reachPayload[kAEPayload])
    dic[kAEPayload] = reachPayload[kAEPayload];

  return dic;
}

- (void)dlcDownloaded:(AEPushMessage*)message
{
  __block NSString* dlcId = message.messageId;
  __block BOOL updated = NO;
  __block AEReachContent* content = nil;
  __block NSMutableArray* datapushes = [[[NSMutableArray alloc] init] autorelease];

  /* Update all messages matching this dlc ID */
  [self enumerateMessagesWithDlcId:dlcId usingBlock:^(AEStorageEntry* sdata)
   {
     AEReachContent* cachedContent;
     AEPushMessage* pushMsg = nil;
     NSUInteger localId = sdata.uid;
     pushMsg = (AEPushMessage*)sdata.data;
     pushMsg.payload = message.payload;

     /* Remove from pending dlcs */
     [_pendingDlcs removeIndex:[dlcId integerValue]];

     /* Get Content */
     cachedContent = [self parseContent:pushMsg];

     /* Set content payload */
     [cachedContent setPayload:message.payload];

     /* Set the updated flag */
     updated = YES;

     /* It's a datapush, we will notify it directly at the end */
     if ([[cachedContent kind] isEqualToString:kAEDatapushKind])
     {
       [datapushes addObject:cachedContent];
     }

     /**
      * It's an interactive content, it will be processed asap if:
      * - It's the current content and it's waiting for this dlc
      * - The above statement is not satisfied but it's the first content that match this dlc
      */
     else if ((localId == _processingId &&
               _state == AEReachModuleStateLoading) ||
              content == nil)
     {
       content = cachedContent;
     }
   }];

  /* Save changes */
  if (updated)
    [_db synchronize];

  /* Process all pending datapushes */
  [datapushes enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL* stop)
   {
     [self notifyContent:((AEReachContent*)obj)];
   }];

  /* Resume content life-cycle */
  if (content)
  {
    /* Show the content, if it is the one we are waiting for */
    if (content.localId == _processingId &&
        _state == AEReachModuleStateLoading)
    {
      /* Exit loading page and show the content */
      [_displayedController exit];
      [self showContent:content];
    }
    /* Show the notification if there is nothing currently processing */
    else if (_state == AEReachModuleStateIdle && !_scanning)
      [self notifyContent:content];

    /* In other cases it will be processed on next scan */
  }
}

- (void)dlcDownloadFailed:(NSString*)messageId
{
  __block NSMutableArray* contents = [[[NSMutableArray alloc] init] autorelease];

  /* Process all messages matching this dlc ID */
  [self enumerateMessagesWithDlcId:messageId usingBlock:^(AEStorageEntry* sdata)
   {
     AEReachContent* content = nil;
     AEPushMessage* pushMsg = (AEPushMessage*)sdata.data;

     /* Remove from pending dlcs */
     [_pendingDlcs removeIndex:[messageId integerValue]];

     /* Get Content */
     content = [self parseContent:pushMsg];

     /* Dismiss related notification and loading screen if this it is currently notified */
     if (content && content.localId == _processingId)
     {
       [self hideNotification];
       [_displayedController exit];
     }

     /* Content will be dropped at this end of the iteration */
     if (content)
       [contents addObject:content];
   }];

  /* Drop any content related to this dlc */
  [contents enumerateObjectsUsingBlock:^(id obj, NSUInteger idx, BOOL* stop)
   {
     [((AEReachContent*)obj) drop];
   }];
}

/* Enumerate all the message entries related to the given DLC Id and process the given block on each of them */
- (void)enumerateMessagesWithDlcId:(NSString*)dlcId usingBlock:(void (^)(AEStorageEntry* sdata))block
{
  /* Invalid Id */
  if ([dlcId length] == 0)
    return;

  /* Get the storage data with the matching message ID */
  /* Important: a message ID can match multiple entries */
  for (AEStorageEntry* sdata in _db)
  {
    /* Catch push message with the given DLC id */
    if ([sdata.data isKindOfClass:[AEPushMessage class]] &&
        [((AEPushMessage*)sdata.data).messageId isEqualToString:dlcId])
    {
      block(sdata);
    }
  }
}

/* Enumerate all the message entries related to the given DLC Id and process the given block on each of them */
- (void)enumerateMessagesWithLocalId:(NSUInteger)localId usingBlock:(void (^)(AEPushMessage* pushMessage))block
{
  /* Invalid Id */
  if (localId <= 0)
    return;

  /* Get the storage data with the matching message ID */
  for (AEStorageEntry* sdata in _db)
  {
    /* Catch sotrage entery with same local id */
    if ([sdata.data isKindOfClass:[AEPushMessage class]] &&
        sdata.uid == localId)
    {
      block((AEPushMessage*)sdata.data);
      break;
    }
  }
}

/* Notify content */
- (void)notifyContent:(AEReachContent*)content
{
  /* Ignore notifying in background (in-app, datapush ...),
   * it will be notified when the App will be back */
  if ([UIApplication sharedApplication].applicationState == UIApplicationStateBackground)
    return;

  /* If it's a data push */
  if ([content class] == [AEReachDataPush class])
  {
    /* Cast */
    AEReachDataPush* datapush = (AEReachDataPush*)content;

    /* Inform delegate */
    NSNumber* result = nil;
    if (datapush.type == AEDatapushTypeText &&
        [_dataPushDelegate respondsToSelector:@selector(didReceiveStringDataPushWithCategory:body:)])
      result = @([_dataPushDelegate didReceiveStringDataPushWithCategory:datapush.category body:datapush.body]);
    else if (datapush.type == AEDatapushTypeBase64 &&
             [_dataPushDelegate respondsToSelector:@selector(didReceiveBase64DataPushWithCategory:decodedBody:
                                                             encodedBody:)])
      result =
        @([_dataPushDelegate didReceiveBase64DataPushWithCategory:datapush.category decodedBody:datapush.decodedBody
                                                      encodedBody:datapush.body]);

    /* Report action on the content if a delegate handled the data push */
    if (result)
    {
      _processingId = content.localId;
      if ([result boolValue])
        [datapush actionContent];
      else
        [datapush exitContent];
    }
    /* Otherwise drop content */
    else
    {
      /* Remove data push from db */
      [datapush drop];
    }
  }
  else if ([content isKindOfClass:[AEInteractiveContent class]] &&
           [((AEInteractiveContent*)content)canNotify:_currentActivity])
  {
    /* Cast */
    AEInteractiveContent* icontent = (AEInteractiveContent*)content;

    /* Never notify "out of app" messages in-app, discard it */
    if ([icontent behavior] == AEContentBehaviorBackground &&
        [UIApplication sharedApplication].applicationState != UIApplicationStateBackground)
    {
      [icontent drop];
      return;
    }

    /* Get a notifier for this category */
    id<AENotifier> notifier = _notifiers[icontent.category];
    if (!notifier)
      notifier = _notifiers[kAEReachDefaultCategory];

    /* Try to notify content */
    @try
    {
      if ([notifier handleNotification:icontent])
      {
        [icontent displayNotification];
        _state = AEReachModuleStateNotifying;
        _processingId = icontent.localId;
      }
    }@catch (NSException* exception)
    {
      NSLog(@"[Engagement][ERROR] %@", [exception description]);

      /* Cannot notify, drop content */
      [icontent drop];
    }
  }
}

/* Display the content */
- (void)showContent:(AEReachContent*)content
{
  /* Initialize the view controller based on the content type */
  AEContentViewController* controller = nil;

  /* If it's a notif announcement */
  if ([content class] == [AEReachNotifAnnouncement class])
  {
    /* Notif announcements */
    AEReachNotifAnnouncement* notifAnnouncement = ((AEReachNotifAnnouncement*)content);

    /* Launch action url */
    if (notifAnnouncement.actionURL)
      [[UIApplication sharedApplication] openURL:[NSURL URLWithString:notifAnnouncement.actionURL]];
  }
  /* If it's an announcement */
  else if ([content class] == [AEReachAnnouncement class])
  {
    /* Cast */
    AEReachAnnouncement* announcement = (AEReachAnnouncement*)content;

    /* Default controller class */
    Class clazz = _announcementControllers[kAEReachDefaultCategory];

    /* Check category */
    if ([announcement.category length] > 0)
    {
      /* If this category has been mapped to a custom controller class */
      Class controllerClass = _announcementControllers[announcement.category];
      if (controllerClass)
        clazz = controllerClass;
    }

    /* Allocate view controller */
    id announcementController = NSAllocateObject(clazz, 0, NULL);
    announcementController = [announcementController initWithAnnouncement:announcement];
    controller = announcementController;
  }
  /* If it's a poll */
  else if ([content class] == [AEReachPoll class])
  {
    /* Cast */
    AEReachPoll* poll = (AEReachPoll*)content;

    /* Default controller class */
    Class clazz = _pollControllers[kAEReachDefaultCategory];

    /* Check category */
    if ([poll.category length] > 0)
    {
      /* If this category has been mapped to a custom controller class */
      Class controllerClass = _pollControllers[poll.category];
      if (controllerClass)
        clazz = controllerClass;
    }

    /* Allocate view controller */
    id pollController = NSAllocateObject(clazz, 0, NULL);
    pollController = [pollController initWithPoll:poll];
    controller = pollController;
  }

  /* Present view controller */
  if (controller)
  {
    _state = AEReachModuleStateShowing;
    [AEViewControllerUtil presentViewController:controller animated:YES];

    /* Memory management */
    [_displayedController release];
    _displayedController = [controller retain];
    [controller release];

    /* Content is displayed */
    if ([content isKindOfClass:[AEInteractiveContent class]])
      [((AEInteractiveContent*)content)displayContent];
  }
}

/**
 * Add the dlc request to pending requests and start it without further check
 * @pre This dlc is needed by at least one reach content being processed and its request is not in progress
 * @param dlcId id of the dlc to request
 */
- (void)requestDlc:(NSString*)dlcId
{
  [_pendingDlcs addIndex:[dlcId integerValue]];
  [[EngagementAgent shared] getMessage:dlcId];
}

- (void)sendFeedback:(NSDictionary*)reachValues withStatus:(NSString*)status andExtra:(NSString*)extra
{
  /* Make sure feedback hasn't already been sent */
  if ([self shouldSendFeedback:status forPushId:reachValues[kAEPushId]])
  {
    /* Send feedback */
    NSMutableDictionary* feedback = [[[NSMutableDictionary alloc] init] autorelease];
    feedback[@"kind"] = reachValues[kAEContentType];
    feedback[@"id"] = reachValues[kAEContentId];
    feedback[@"status"] = status;
    if (extra)
    {
      feedback[@"extra"] = extra;
    }

    /* Send feedback */
    [[EngagementAgent shared] sendReachFeedback:feedback];
  }
}

/* Callback to process polled campaigns */
- (void)campaignsPolled:(NSArray*)campaigns
{
  for (NSDictionary* aCampaign in campaigns)
  {
    NSMutableDictionary* azmeObject = [[[NSMutableDictionary alloc] init] autorelease];
    NSMutableDictionary* payloadObject = [[[NSMutableDictionary alloc] init] autorelease];
    NSSet* payloadKeySet = [NSSet setWithObjects:kAEDlcNotificationImage, kAEDlcDeliveryActivities, kAEDlcTitle,
                            kAEDlcBody, kAEDlcActionButtonText, kAEDlcExitButtonText, kAEDlcQuestions, kAEDlcType, nil];
    for (NSString* key in aCampaign.allKeys)
    {
      if ([payloadKeySet containsObject:key])
        payloadObject[key] = aCampaign[key];
      else
        azmeObject[key] = aCampaign[key];
    }
    if ([[payloadObject allKeys] count] > 0)
    {
      azmeObject[kAEPayload] = payloadObject;
    }

    /* Create a dictionary and add the reach values for 'azme' key */
    NSDictionary* reachValues = [NSDictionary dictionaryWithObject:azmeObject forKey:kAERemoteNotifReachKey];

    /* Process notification(send feedback and store in db) */
    if (reachValues)
      [self processRemoteNotification:reachValues];
  }
}

/* Callback to handle polling failure */
- (void)campaignsPollFailed
{
  /* Restore the backup last polling time in order to retry */
  NSNumber* lastPolledTimeBackup = [[NSUserDefaults standardUserDefaults] valueForKey:kAELastPolledTimeBackup];
  [[NSUserDefaults standardUserDefaults] setObject:lastPolledTimeBackup forKey:kAELastPolledTime];
}

/* Trash given content id */
- (void)trash:(NSUInteger)uid
{
  if (uid > 0)
  {
    /* Bypass trash if not scanning */
    if (!_scanning)
      [_db remove:uid];
    else
      [_trash addIndex:uid];
  }
}

/* Empty trash */
- (void)emptyTrash
{
  for (NSUInteger i = [_trash firstIndex]; i != NSNotFound; i = [_trash indexGreaterThanIndex:i])
    [_db remove:i];
  [_trash removeAllIndexes];
}

- (AEReachContent*)parseContent:(AEPushMessage*)msg
{
  if (!msg)
    return nil;

  /* Create a mutable reach values with payload */
  NSDictionary* dlcPayload = msg.payload;
  NSMutableDictionary* mutableReachValues = nil;

  /* If dlc payload is available, update the reach value with payload */
  if ([[dlcPayload allKeys] count] > 0)
  {
    mutableReachValues = [NSMutableDictionary dictionaryWithDictionary:[msg reachValues]];
    mutableReachValues[kAEPayload] = dlcPayload;
  }

  /* Set reach value as NSDictionary */
  NSDictionary* reachValues = mutableReachValues ? (NSDictionary*)mutableReachValues : [msg reachValues];

  /* Verify if valid */
  if (!reachValues)
    return nil;

  /* Get content based on the type */
  AEReachContent* content = nil;
  NSString* type = reachValues[kAEContentType];

  /* Initialize content */
  if ([type isEqualToString:@"a"])
  {
    if (([reachValues[kAEDlc] integerValue] & FLAG_DLC_CONTENT) == FLAG_DLC_CONTENT)
      content = [AEReachAnnouncement announcementWithReachValues:reachValues params:_params];
    else
      content = [AEReachNotifAnnouncement notifAnnouncementWithReachValues:reachValues params:_params];
  }
  else if ([type isEqualToString:@"p"])
  {
    content = [AEReachPoll pollWithReachValues:reachValues];
  }
  else if ([type isEqualToString:@"d"])
  {
    content = [AEReachDataPush datapushWithReachValues:reachValues params:_params];
  }

  /* Should content send feedback? */
  [content setFeedback:![reachValues[kAEContentId] isEqualToString:kAERemoteNotifReachTestIdKey]];

  /* Set local Id from cached data */
  for (AEStorageEntry* sdata in _db)
  {
    if ([sdata.data isKindOfClass:[AEPushMessage class]])
    {
      AEPushMessage* pushMsg = (AEPushMessage*)sdata.data;
      if ([pushMsg.reachValues[kAEPushId] isEqualToString:content.pushId])
      {
        [content setLocalId:sdata.uid];
        break;
      }
    }
  }

  /* Content couldn't be parsed */
  return content;
}

/* Parse shortened content Id and return the ID and Type */
- (NSDictionary*)parseContentIdAndType:(NSString*)shortenedContentId
{
  NSMutableDictionary* dic = [[[NSMutableDictionary alloc] init] autorelease];

  if ([shortenedContentId length] > 0)
  {
    NSString* pattern = @"([apdn])([-]?\\d+)";
    NSRegularExpression* regex = [NSRegularExpression regularExpressionWithPattern:pattern
                                                                           options:NSRegularExpressionCaseInsensitive
                                                                             error:NULL];
    NSTextCheckingResult* match =
      [regex firstMatchInString:shortenedContentId options:0 range:NSMakeRange(0, [shortenedContentId length])];
    if (match != nil)
    {
      dic[@"type"] = [shortenedContentId substringWithRange:[match rangeAtIndex:1]];
      dic[@"id"] = [shortenedContentId substringWithRange:[match rangeAtIndex:2]];
    }
  }
  return dic;
}

/* Scan the list of contents and display the first one that can be displayed. */
- (void)scan
{
  NSInteger* localId;
  bool canBeNotified;

  /* If module is ready to display notifications and not already scanning */
  if ((_state == AEReachModuleStateIdle) && !_scanning)
  {
    _scanning = YES;
    for (AEStorageEntry* sdata in _db)
    {
      localId = nil;
      BOOL parsed = NO;
      canBeNotified = NO;
      if ([sdata.data isKindOfClass:[AEPushMessage class]])
      {
        AEReachContent* content = [self parseContent:(AEPushMessage*)sdata.data];
        if (content)
        {
          localId = sdata.uid;

          /* Successfully parsed */
          parsed = YES;

          /* Set local id */
          [content setLocalId:localId];

          /* Remove if expired */
          if ([content isExpired])
          {
            [self trash:localId];
            continue;
          }

          /**
           * A content needs to download its dlc before being notified when:
           * - The notification itself needs a dlc (image, activity list)
           * - It's a datapush, data needs to be downloaded before callbacks can be called
           */
          if (([content hasNotificationDLC] || [content hasNotificationAndContentDLC] ||
               [[content kind] isEqualToString:kAEDatapushKind]))
          {
            /* this content has no pending dlc request */
            if (![_pendingDlcs containsIndex:[[content getDlcId] integerValue]])
            {
              /* Start downloading the dlc if it's not completed */
              if (![content isDlcCompleted])
                [self requestDlc:[content getDlcId]];

              /* This content is ready to go, notify it */
              else
                canBeNotified = YES;
            }

            /* Otherwise ignore, it will be notified when its dlc will be ready */
          }
          /* Don't need any dlc before being notified, notify it right now */
          else
            canBeNotified = YES;

          /* Notify this content if it's ready */
          if (canBeNotified)
          {
            [self notifyContent:content];
            break;
          }
        }
      }

      /* Remove from storage if content could not be parsed */
      if (!parsed)
      {
        [self trash:sdata.uid];
      }
    }

    /* End of scan, empty trash */
    _scanning = NO;
    [self emptyTrash];
  }
}

/* Hide any displayed notification */
- (void)hideNotification
{
  if (_state == AEReachModuleStateNotifying)
  {
    _processingId = -1;
    _state = AEReachModuleStateIdle;
    for (NSString* category in[_notifiers allKeys])
      [_notifiers[category] clearNotification:category];
  }
}

/* Clear badge application icon */
- (void)clearBadge
{
  [[UIApplication sharedApplication] setApplicationIconBadgeNumber:1];
  [[UIApplication sharedApplication] setApplicationIconBadgeNumber:0];
}

/* Clear badge appInfo */
- (void)clearBadgeAppInfo
{
  [[EngagementAgent shared] sendAppInfo:@{@"badge": @"0"}];
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Reach actions
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

/* Mark content as processed */
- (void)markContentProcessed:(AEReachContent*)content
{
  /* Trash content */
  [self trash:content.localId];

  /* State is only for interactive reach content */
  if (_processingId == content.localId)
  {
    /* Idle now */
    _processingId = -1;
    _state = AEReachModuleStateIdle;

    /* No controller displayed anymore */
    [_displayedController release];
    _displayedController = nil;

    /* If there are still reach contents to process, continue */
    [self scan];
  }
}

- (void)onNotificationActioned:(AEReachContent*)content
{
  /* Check if this content needs to wait for its DLC */
  if ([content hasContentDLC] && ![content isDlcCompleted])
  {
    /* While content is downloaded, show progressing view */
    _state = AEReachModuleStateLoading;

    /* Save the processing content id */
    _processingId = content.localId;

    /* Show progressing UI with progressing animation */
    AEContentLoadingViewController* controller = [[AEContentLoadingViewController alloc] init];
    controller.dlcId = [content getDlcId];
    [AEViewControllerUtil presentViewController:controller animated:NO];
    [_displayedController release];
    _displayedController = [controller retain];
    [controller release];

    /* Start downloading the related dlc if there is no pending request */
    if (![_pendingDlcs containsIndex:[[content getDlcId] integerValue]])
      [self requestDlc:[content getDlcId]];
  }
  else
  {
    [self showContent:content];
  }
}

- (void)onLoadingViewDismissed
{
  /* Set the state to idle for further content processing */
  _state = AEReachModuleStateIdle;
  _processingId = -1;
  [self scan];
}

- (BOOL)shouldSendFeedback:(NSString*)status forPushId:(NSString*)pushId
{
  /* Check if item with matching push id is in db */
  for (AEStorageEntry* sdata in _feedbackDb)
  {
    if ([sdata.data isKindOfClass:[AEReachFeedback class]])
    {
      AEReachFeedback* reachfeedback = (AEReachFeedback*)sdata.data;

      /* Check for duplicate */
      if ([reachfeedback.pushId isEqualToString:pushId])
      {
        BOOL feedbackValue = [reachfeedback getValue:status];

        /* If value already exist, there is no need to send feedback again */
        if (feedbackValue)
          return NO;

        /* It's not allowed to send feedback for more than one action */
        if ([status isEqualToString:kAEReachFeedbackContentActioned] ||
            [status isEqualToString:kAEReachFeedbackContentExited])
        {
          if ([reachfeedback getValue:kAEReachFeedbackContentActioned] ||
              [reachfeedback getValue:kAEReachFeedbackContentExited])
            return NO;
        }

        /* Add a key for status and update the db */
        [reachfeedback setValue:YES forKey:status];
        [_feedbackDb synchronize];
        return YES;
      }
    }
  }

  /* If no matching item found */
  AEReachFeedback* aReachFeedBack = [[[AEReachFeedback alloc] initWithPushId:pushId] autorelease];
  [aReachFeedBack setValue:YES forKey:status];
  [_feedbackDb put:aReachFeedBack];

  /* Return YES found since it's a new item */
  return YES;
}

////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
#pragma mark Storage eviction delegate
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

- (void)storage:(AEStorage*)storage didEvictEntry:(AEStorageEntry*)entry
{
  /* Do nothing if this content is being displayed */
  if (_state == AEReachModuleStateShowing && entry.uid == _processingId)
    return;

  /* Parse content that is going to be evicted */
  AEReachContent* content = [self parseContent:(AEPushMessage*)entry.data];
  if (content)
  {
    /* Hide notification and display a new one if this content is being notified */
    if (_state == AEReachModuleStateNotifying && entry.uid == _processingId)
    {
      [self hideNotification];
      [self scan];
    }

    /* Drop content */
    [content drop];
  }
}

@end
