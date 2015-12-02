/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

#import "AEReachDataPush.h"
#import "AEContentStorage.h"

@implementation AEReachDataPush

- (id)initWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params
{
  self = [super initWithReachValues:reachValues];

  if (self != nil)
  {
    /* Init type, it can already be set if the payload has been set during super init */
    if (!_type)
      _type = AEDatapushTypeUnknown;

    /* cache the params */
    _cachedParams = [params copy];

    /* Replace parameters in the content payload */
    [self replaceParamsInBody];
  }
  return self;
}

+ (id)datapushWithReachValues:(NSDictionary*)reachValues params:(NSDictionary*)params
{
  return [[[self alloc] initWithReachValues:reachValues params:params] autorelease];
}

- (void)dealloc
{
  [_cachedParams release];
  [super dealloc];
}

- (void)replaceParamsInBody
{
  if (_cachedParams)
  {
    /* Replace parameters in the content payload */
    NSString* newBody = self.body;
    NSEnumerator* keyIt = [_cachedParams keyEnumerator];
    NSString* key;
    while ((key = [keyIt nextObject]))
    {
      newBody = [newBody stringByReplacingOccurrencesOfString:key withString:_cachedParams[key]];
    }
    self.body = newBody;
  }
}

- (void)setPayload:(NSDictionary*)payload
{
  [super setPayload:payload];

  /* Set type */
  _type = [self typeFromString:payload[kAEDlcType]];

  /* Replace the input param in body */
  [self replaceParamsInBody];
}

- (NSString*)kind
{
  return kAEDatapushKind;
}

- (NSData*)decodedBody
{
  if (self.type == AEDatapushTypeUnknown)
    return nil;

  /* Decode from base 64 */
  return [self decodeBase64:self.body];
}

- (AEDatapushType)typeFromString:(NSString*)type
{
  if ([type isEqualToString:@"text/plain"])
    return AEDatapushTypeText;
  else if ([type isEqualToString:@"text/base64"])
    return AEDatapushTypeBase64;
  else
    return AEDatapushTypeUnknown;
}

@end
