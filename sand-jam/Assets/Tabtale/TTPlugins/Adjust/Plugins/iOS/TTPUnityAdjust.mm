//
//  TTPUnityAdjust.m
//  Unity-iPhone
//
//  Created by PetarKarov on 27.12.23.
//  Copyright © 2023 TabTale. All rights reserved.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPILRDData.h>
#import <TT_Plugins_Core/TTPMediationType.h>

@interface TTPUnityAdjust : NSObject
+ (NSDictionary *) ttpAdjustDictionaryFromJsonStr: (const char*) json;
@end

@implementation TTPUnityAdjust

+ (NSDictionary *) ttpAdjustDictionaryFromJsonStr: (const char*) json {
    if (json == nullptr) return nil;
    NSData *data = [[[NSString alloc] initWithUTF8String:json] dataUsingEncoding:NSUnicodeStringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    return dict;
}

extern "C" {
    
    void ttpAdjustLogEvent(const char* eventToken, const char* eventParamsJsonStr, float revenue, const char* currency)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIadjust> adjustService = [serviceManager get:@protocol(TTPIadjust)];
        if(adjustService != nil){
            [adjustService logEvent:[[NSString alloc] initWithUTF8String:eventToken]
                             params:[TTPUnityAdjust ttpAdjustDictionaryFromJsonStr:eventParamsJsonStr]
                            revenue: revenue
                           currency: [[NSString alloc] initWithUTF8String:currency]];
        }
    }
    
    void ttpAdjustNonRevenueLogEvent(const char* eventToken, const char* eventParamsJsonStr)
        {
            TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
            id<TTPIadjust> adjustService = [serviceManager get:@protocol(TTPIadjust)];
            if(adjustService != nil){
                [adjustService logNonRevenueEvent:[[NSString alloc] initWithUTF8String:eventToken]
                                 params:[TTPUnityAdjust ttpAdjustDictionaryFromJsonStr:eventParamsJsonStr]];
            }
        }
    void ttpAdjustReportAdView(int mediationType, float revenue, const char* currency, const char* type, const char* network, const char* networkPlacement, const char* placement, const char* creativeIdentifier)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIadjust> adjustService = [serviceManager get:@protocol(TTPIadjust)];
        if(adjustService != nil){
            NSDecimalNumber *decimalRevenue = [NSDecimalNumber decimalNumberWithDecimal:[[NSNumber numberWithFloat:revenue] decimalValue]];
            TTPILRDData* data = [[TTPILRDData alloc] initWithRevenue: decimalRevenue currency:[[NSString alloc] initWithUTF8String:currency] creativeIdentifier:[[NSString alloc] initWithUTF8String:creativeIdentifier]];
            [data setAdditionalILRDData:[[NSString alloc] initWithUTF8String:networkPlacement]
                              placement:[[NSString alloc] initWithUTF8String:placement]];
    
            TTPMediationType mediationTypeEnum = (mediationType == 1) ? MAX : ADMOB;
            
            [adjustService reportAdView: mediationTypeEnum
                                   type: [[NSString alloc] initWithUTF8String:type]
                                network: [[NSString alloc] initWithUTF8String:network]
                               ilrdData: data];
        }
    }
    
    const char * ttpGetAdjustId()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIadjust> adjustService = [serviceManager get:@protocol(TTPIadjust)];
        NSString *adjustId = @"NA";
        if(adjustService != nil) {
            adjustId = [adjustService getAdjustId];
        }
        return strdup([adjustId UTF8String]);
    }
}

@end
