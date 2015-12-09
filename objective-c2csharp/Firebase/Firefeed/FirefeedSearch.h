//
//  FirefeedSearch.h
//  iFirefeed
//
//  Created by Greg Soltis on 4/23/13.
//  Copyright (c) 2013 Firebase. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Firebase/Firebase.h>

@protocol FirefeedSearchDelegate;

@interface FirefeedSearch : NSObject

- (id) initWithRef:(Firebase *)ref;

- (BOOL) searchTextDidUpdate:(NSString *)text;

@property (strong, nonatomic) UITableView* resultsTable;
@property (weak, nonatomic) id<FirefeedSearchDelegate> delegate;

@end

@protocol FirefeedSearchDelegate <NSObject>

- (void) userIdWasSelected:(NSString *)userId;

@end