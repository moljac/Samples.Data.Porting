/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachPollQuestion.h"

@implementation AEReachPollQuestion

- (id)initWithId:(NSString*)qId title:(NSString*)title choices:(NSArray*)choices
{
  self = [super init];
  if (self != nil)
  {
    _questionId = [qId copy];
    _title = [title copy];
    _choices = [choices copy];
  }
  return self;
}

- (void)dealloc
{
  [_questionId release];
  [_title release];
  [_choices release];
  [super dealloc];
}

@end

@implementation AEReachPollChoice

- (id)initWithId:(NSString*)cId title:(NSString*)title isDefault:(BOOL)isDefault
{
  self = [super init];
  if (self != nil)
  {
    _choiceId = [cId copy];
    _title = [title copy];
    _isDefault = isDefault;
  }
  return self;
}

- (void)dealloc
{
  [_choiceId release];
  [_title release];
  [super dealloc];
}

@end
