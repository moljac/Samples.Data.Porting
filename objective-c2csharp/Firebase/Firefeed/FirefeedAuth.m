//
//  FirefeedAuth.m
//  iFirefeed
//
//  Created by Greg Soltis on 4/8/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import "FirefeedAuth.h"


typedef void (^ffbt_void_nserror_user)(NSError* error, FAuthData* user);
typedef void (^ffbt_void_void)(void);

// This class manages multiple concurrent auth requests (login, logout, status) against the same Firebase
@interface FirefeedAuthData : NSObject {
    NSMutableDictionary* _blocks;
    Firebase* _ref;
    long _luid;
    FAuthData* _user;
    FirebaseHandle _authHandle;
}

- (id) initWithRef:(Firebase *)ref;
- (long) checkAuthStatus:(ffbt_void_nserror_user)block;
- (void) loginToAppWithId:(NSString *)appId;
- (void) logout;

@end

@implementation FirefeedAuthData

- (id) initWithRef:(Firebase *)ref {
    self = [super init];
    if (self) {
        // Start at 1 so it works with if (luid) {...}
        _luid = 1;
        _ref = ref;
        _user = nil;
        // Keep an eye on what Firebase says our authentication status is
        
        _authHandle = [_ref observeAuthEventWithBlock:^(FAuthData *user) {
            // This is the new style, but there doesn't appear to be any way to tell which way the user is going, online or offline?
            if ((user == nil) && (_user != nil)) {
                [self onAuthStatusError:nil user:nil];
            }
        }];
        _blocks = [[NSMutableDictionary alloc] init];
    }
    return self;
}

- (void) dealloc {
    if (_authHandle != NSNotFound) {
        [_ref removeAuthEventObserverWithHandle:_authHandle];
    }
}

- (void) loginToAppWithId:(NSString *)appId {
    [FBSettings setDefaultAppID: appId];    // Should not be necessary
    [FBSession openActiveSessionWithReadPermissions:@[@"public_profile"] allowLoginUI:YES completionHandler:^(FBSession *session, FBSessionState state, NSError *error) {
        if (error) {
            NSLog(@"Facebook login failed. Error: %@", error);
        } else if (state == FBSessionStateOpen) {
            NSString *accessToken = session.accessTokenData.accessToken;
            [_ref authWithOAuthProvider:@"facebook" token:accessToken withCompletionBlock:^(NSError *error, FAuthData *authData) {
                [self onAuthStatusError:error user:authData];
                if (error) {
                    NSLog(@"Login failed. %@", error);
                } else {
                    NSLog(@"Logged in! %@", authData);
                    [self populateSearchIndicesForUser:authData];
                }
            }];
        }
    }];
}

- (void) populateSearchIndicesForUser:(FAuthData *)user {
    // For each user, we list them in the search index twice. Once by first name and once by last name. We include the id at the end to guarantee uniqueness
    Firebase* firstNameRef = [_ref.root childByAppendingPath:@"search/firstName"];
    Firebase* lastNameRef = [_ref.root childByAppendingPath:@"search/lastName"];

    NSString* firstName = [user.providerData objectForKey:@"first_name"];
    NSString* lastName = [user.providerData objectForKey:@"last_name"];
    NSString* firstNameKey = [[NSString stringWithFormat:@"%@_%@_%@", firstName, lastName, user.uid] lowercaseString];
    NSString* lastNameKey = [[NSString stringWithFormat:@"%@_%@_%@", lastName, firstName, user.uid] lowercaseString];

    [[firstNameRef childByAppendingPath:firstNameKey] setValue:user.uid];
    [[lastNameRef childByAppendingPath:lastNameKey] setValue:user.uid];
}

- (void) logout {
    // Pass through to Firebase to unauth
    [_ref unauth];
}

// Assumes block is already on the heap
- (long) checkAuthStatus:(ffbt_void_nserror_user)block {
    long handle = _luid++;
    NSNumber* luid = [NSNumber numberWithLong:handle];

    [_blocks setObject:block forKey:luid];
    if (_user) {
        // we already have a user logged in
        // force async to be consistent
        ffbt_void_void cb = ^{
            block(nil, _user);
        };
        [self performSelector:@selector(executeCallback:) withObject:[cb copy] afterDelay:0];
    } else if (_blocks.count == 1) {
        // This is the first block for this firebase, kick off the login process
        [_ref observeAuthEventWithBlock:^(FAuthData *user) {
            if (user) {
                [self onAuthStatusError:nil user:user];
            } else {
                [self onAuthStatusError:nil user:nil];
            }
        }];
    }
    return handle;
}

- (void) stopWatchingAuthStatus:(long)handle {
    NSNumber* luid = [NSNumber numberWithLong:handle];

    [_blocks removeObjectForKey:luid];
}

- (void) onAuthStatusError:(NSError *)error user:(FAuthData *)user {
    if (user) {
        _user = user;
    } else {
        _user = nil;
    }
    
    for (NSNumber* handle in _blocks) {
        // tell everyone who's listening
        ffbt_void_nserror_user block = [_blocks objectForKey:handle];
        block(error, user);
    }
}

// Used w/ performSelector. Basically a hack to execute a block asynchronously
- (void) executeCallback:(ffbt_void_void)callback {
    callback();
}

@end

@interface FirefeedAuth ()

@property (strong, nonatomic) NSMutableDictionary* firebases;

@end

@implementation FirefeedAuth

+ (FirefeedAuth *) singleton {
    // We use a singleton here
    static dispatch_once_t pred;
    static FirefeedAuth* theSingleton;
    dispatch_once(&pred, ^{
        theSingleton = [[FirefeedAuth alloc] init];
    });
    return theSingleton;
}

// Pass-through methods to the singleton
+ (long) watchAuthForRef:(Firebase *)ref withBlock:(void (^)(NSError *, FAuthData *))block {
    return [[self singleton] checkAuthForRef:ref withBlock:block];
}

+ (void) stopWatchingAuthForRef:(Firebase *)ref withHandle:(long)handle {
    [[self singleton] stopWatchingAuthForRef:ref withHandle:handle];
}

+ (void) loginRef:(Firebase *)ref toFacebookAppWithId:(NSString *)appId {
    [[self singleton] loginRef:ref toFacebookAppWithId:appId];
}

+ (void) logoutRef:(Firebase *)ref {
    [[self singleton] logoutRef:ref];
}

- (id) init {
    self = [super init];
    if (self) {
        self.firebases = [[NSMutableDictionary alloc] init];
    }
    return self;
}

- (void) loginRef:(Firebase *)ref toFacebookAppWithId:(NSString *)appId {

    NSString* firebaseId = ref.root.description;

    // Pass to the FirefeedAuthData object, which manages multiple auth requests against the same Firebase
    FirefeedAuthData* authData = [self.firebases objectForKey:firebaseId];
    if (!authData) {
        authData = [[FirefeedAuthData alloc] initWithRef:ref.root];
        [self.firebases setObject:authData forKey:firebaseId];
    }
    [authData loginToAppWithId:appId];
}

- (void) logoutRef:(Firebase *)ref {
    NSString* firebaseId = ref.root.description;

    // Pass to the FirefeedAuthData object, which manages multiple auth requests against the same Firebase
    FirefeedAuthData* authData = [self.firebases objectForKey:firebaseId];
    if (!authData) {
        authData = [[FirefeedAuthData alloc] initWithRef:ref.root];
        [self.firebases setObject:authData forKey:firebaseId];
    }

    [authData logout];
}


- (void) stopWatchingAuthForRef:(Firebase *)ref withHandle:(long)handle {
    NSString* firebaseId = ref.root.description;

    // Pass to the FirefeedAuthData object, which manages multiple auth requests against the same Firebase
    FirefeedAuthData* authData = [self.firebases objectForKey:firebaseId];
    if (authData) {
        [authData stopWatchingAuthStatus:handle];
    }
}

- (long) checkAuthForRef:(Firebase *)ref withBlock:(ffbt_void_nserror_user)block {
    ffbt_void_nserror_user userBlock = [block copy];
    NSString* firebaseId = ref.root.description;

    // Pass to the FirefeedAuthData object, which manages multiple auth requests against the same Firebase
    FirefeedAuthData* authData = [self.firebases objectForKey:firebaseId];
    if (!authData) {
        authData = [[FirefeedAuthData alloc] initWithRef:ref.root];
        [self.firebases setObject:authData forKey:firebaseId];
    }

    return [authData checkAuthStatus:userBlock];
}

@end
