//
//  FirefeedUser.m
//  iFirefeed
//
//  Created by Greg Soltis on 4/7/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import "FirefeedUser.h"

@interface FirefeedUser ()

@property (nonatomic) FirebaseHandle valueHandle;
@property (nonatomic) BOOL loaded;
@property (strong, nonatomic) Firebase* ref;

@end

@implementation FirefeedUser

typedef void (^ffbt_void_ffuser)(FirefeedUser* user);

+ (FirefeedUser *) loadFromRoot:(Firebase *)root withUserId:(NSString *)userId completionBlock:(ffbt_void_ffuser)block {
    // Create basic user data from what we already know and pass through
    return [self loadFromRoot:root withUserData:@{@"userId": userId} completionBlock:block];
}

+ (FirefeedUser *) loadFromRoot:(Firebase *)root withUserData:(NSDictionary *)userData completionBlock:(ffbt_void_ffuser)block {
    // Create a new FirefeedUser instance pointed at the given location, with the given initial data, and setup the callback for when it updates
    ffbt_void_ffuser userBlock = [block copy];
    NSString* userId = [userData objectForKey:@"userId"];
    Firebase* peopleRef = [[root childByAppendingPath:@"people"] childByAppendingPath:userId];

    return [[FirefeedUser alloc] initRef:peopleRef initialData:userData andBlock:userBlock];
}

- (id) initRef:(Firebase *)ref initialData:(NSDictionary *)userData andBlock:(ffbt_void_ffuser)userBlock {
    self = [super init];
    if (self) {
        self.loaded = NO;
        self.userId = ref.name;
        // Setup any initial data that we already have
        self.bio = [userData objectForKey:@"bio"];
        self.firstName = [userData objectForKey:@"firstName"];
        self.lastName = [userData objectForKey:@"lastName"];
        self.fullName = [userData objectForKey:@"fullName"];
        self.location = [userData objectForKey:@"location"];
        self.ref = ref;
        // Load the actual data from Firebase
        self.valueHandle = [ref observeEventType:FEventTypeValue withBlock:^(FDataSnapshot *snapshot) {

            id val = snapshot.value;
            if (val == [NSNull null]) {
                // First login

            } else {
                NSString* prop = [val objectForKey:@"bio"];
                if (prop) {
                    self.bio = prop;
                }
                prop = [val objectForKey:@"firstName"];
                if (prop) {
                    self.firstName = prop;
                }
                prop = [val objectForKey:@"lastName"];
                if (prop) {
                    self.lastName = prop;
                }
                prop = [val objectForKey:@"fullName"];
                if (prop) {
                    self.fullName = prop;
                }
                prop = [val objectForKey:@"location"];
                if (prop) {
                    self.location = prop;
                }
            }


            if (self.loaded) {
                // just call the delegate for updates
                [self.delegate userDidUpdate:self];
            } else {
                // Trigger the block for the initial load
                userBlock(self);
            }
            self.loaded = YES;
        }];
    }
    return self;
}

- (void) stopObserving {
    [_ref removeObserverWithHandle:_valueHandle];
    _valueHandle = NSNotFound;
}


- (void) updateFromRoot:(Firebase *)root {
    // We force lowercase for firstName and lastName so that we can check search index keys in the security rules
    // Those values aren't used for display anyways
    Firebase* peopleRef = [[root childByAppendingPath:@"people"] childByAppendingPath:_userId];
    [peopleRef updateChildValues:@{@"bio": _bio, @"firstName": [_firstName lowercaseString], @"lastName": [_lastName lowercaseString], @"fullName": _fullName, @"location": _location}];
}

- (void) setBio:(NSString *)bio {
    if (!bio) {
        _bio = @"";
    } else {
        _bio = bio;
    }
}

- (void) setFirstName:(NSString *)name {
    if (!name) {
        _firstName = @"";
    } else {
        _firstName = name;
    }
}

- (void) setLastName:(NSString *)lastName {
    if (!lastName) {
        _lastName = @"";
    } else {
        _lastName = lastName;
    }
}

- (void) setFullName:(NSString *)fullName {
    if (!fullName) {
        _fullName = @"";
    } else {
        _fullName = fullName;
    }
}

- (void) setLocation:(NSString *)location {
    if (!location) {
        _location = @"";
    } else {
        _location = location;
    }
}

- (NSURL *) picUrl {
    NSString *author;
    // Check for uid vs id, so we can know how to query the facebook API for the profile picture
    if ([self.userId containsString:@"facebook:"]) {
        NSArray *stringPieces = [self.userId componentsSeparatedByString:@":"];
        author = stringPieces[1];
    } else {
        author = self.userId;
    }
    return [NSURL URLWithString:[NSString stringWithFormat:@"https://graph.facebook.com/v2.1/%@/picture/?return_ssl_resources=1&width=96&height=96", author]];\
}

- (NSURL *) picURLSmall {
    NSString *author;
    // Check for uid vs id, so we can know how to query the facebook API for the profile picture
    if ([self.userId containsString:@"facebook:"]) {
        NSArray *stringPieces = [self.userId componentsSeparatedByString:@":"];
        author = stringPieces[1];
    } else {
        author = self.userId;
    }
    return [NSURL URLWithString:[NSString stringWithFormat:@"https://graph.facebook.com/v2.1/%@/picture/?return_ssl_resources=1&width=48&height=48", author]];
}

// Override so that we can find other objects pointed at the same user
- (BOOL) isEqual:(id)object {
    return [object isKindOfClass:[self class]] && [self.userId isEqualToString:[object userId]];
}

- (NSString *) description {
    return [NSString stringWithFormat:@"User %@", _userId];
}

@end
