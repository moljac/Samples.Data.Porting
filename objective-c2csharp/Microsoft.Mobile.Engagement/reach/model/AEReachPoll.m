/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachPoll.h"
#import "AEReachPollQuestion.h"
#import  "AEContentStorage.h"

@implementation AEReachPoll

- (id)initWithReachValues:(NSDictionary*)reachValues
{
  self = [super initWithReachValues:reachValues];

  if (self != nil)
  {
    /* Initialize answer dictionary */
    _answers = [[NSMutableDictionary alloc] init];

    if (reachValues[kAEPayload])
    {
      [self setPayload:reachValues[kAEPayload]];
    }
  }
  return self;
}

- (void)setPayload:(NSDictionary*)payload
{
  [super setPayload:payload];

  if ([payload[kAEDlcQuestions] count] > 0)
  {
    NSMutableArray* questionArray = [[[NSMutableArray alloc] init] autorelease];
    NSMutableArray* choiceArray = [[[NSMutableArray alloc] init] autorelease];

    for (NSDictionary* aQuestion in payload[kAEDlcQuestions])
    {
      for (NSDictionary* aChoice in aQuestion[@"choices"])
      {
        /* Create choice */
        AEReachPollChoice* choice =
          [[[AEReachPollChoice alloc] initWithId:[aChoice[@"id"] stringValue] title:aChoice[@"title"] isDefault:[aChoice[@"isDefault"]
                                                                                                   boolValue]]
           autorelease];
        [choiceArray addObject:choice];
      }

      /* Create question */
      AEReachPollQuestion* question =
        [[[AEReachPollQuestion alloc] initWithId:[aQuestion[@"id"] stringValue] title:aQuestion[@"title"] choices:choiceArray] autorelease];
      [questionArray addObject:question];

      /* Clear array */
      [choiceArray removeAllObjects];
    }
    _questions = [questionArray copy];
  }
}

+ (id)pollWithReachValues:(NSDictionary*)reachValues;
{
  return [[[self alloc] initWithReachValues:reachValues] autorelease];
}

- (void)fillAnswerWithQuestionId:(NSString*)qid choiceId:(NSString*)cid
{
  _answers[qid] = cid;
}

- (void)actionContent
{
  [self process:@"content-actioned" extras:_answers];
}

- (NSString*)kind
{
  return kAEPollKind;
}

- (void)dealloc
{
  [_questions release];
  [_answers release];
  [super dealloc];
}

@end
