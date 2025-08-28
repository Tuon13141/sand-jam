//
//  TTPUnityBilling.m
//  Unity-iPhone
//
//  Created by Tabtale on 12/12/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIbilling.h>


@interface TTPUnityBilling : NSObject

+ (NSDictionary *) ttpBillingPostDataFromJsonStr: (const char*) json;

@end

@implementation TTPUnityBilling

+ (NSDictionary *) ttpBillingPostDataFromJsonStr: (const char*) json {
    // if json == null or json == "" subscriptionStarted event should not be sent
    if (json == nullptr) return nil;
    if([@(json) isEqualToString: @""]) return nil;
    NSData *data = [[[NSString alloc] initWithUTF8String:json] dataUsingEncoding:NSUnicodeStringEncoding];
    NSDictionary *dict = [NSJSONSerialization JSONObjectWithData:data options:kNilOptions error:nil];
    return dict;
}

extern "C" {
    
    void ttpNoAdsPurchased(bool purchased)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbilling> billing = [serviceManager get:@protocol(TTPIbilling)];
        if(billing != nil)
        {
            [billing setNoAdsItemPurchased:purchased];
        }
    }

    bool ttpWasUserDetectedInChina()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbilling> billing = [serviceManager get:@protocol(TTPIbilling)];
        if(billing != nil)
        {
            return [billing wasUserDetectedInChina];
        }
        return false;
    }

    extern void ttpReportPurchaseToConversion(const char * message)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbilling> billing = [serviceManager get:@protocol(TTPIbilling)];
        if(billing != nil && message != NULL)
        {
            NSString *json = [[NSString alloc] initWithUTF8String:message];
            NSData *data = [json dataUsingEncoding:NSUTF8StringEncoding];
            NSDictionary *dic = [NSJSONSerialization JSONObjectWithData:data options:0 error:nil];
            if(dic != nil){
                NSString *currency = [dic objectForKey:@"currency"];
                NSString *productId = [dic objectForKey:@"productId"];
                NSNumber *price = [dic objectForKey:@"price"];
                NSNumber *consumable = [dic objectForKey:@"consumable"];
                if(productId != nil && price != nil){
                    [billing reportPurchaseToConversion:currency price:[price floatValue] productId:productId consumable:[consumable boolValue]];
                }
            }
        }
    }

    extern char* ttpGetPurchaseValidationParams()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbilling> billing = [serviceManager get:@protocol(TTPIbilling)];
        if(billing != nil)
        {
            NSError *error;
            NSData *jsonData = 
                [NSJSONSerialization dataWithJSONObject:[billing getPurchaseValidationParams]
                                                options:NSJSONWritingPrettyPrinted
                                                  error:&error];

            if (!jsonData) {
                NSLog(@"ttpGetCurrentConfig error: %@", error.localizedDescription);
            } else {
                NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
                return strdup([jsonString UTF8String]);
            }
        }
        return NULL;
    }
    
    extern void ttpSetPurchaserKeywordForRequest()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIbilling> billing = [serviceManager get:@protocol(TTPIbilling)];
        if(billing != nil)
        {
            [billing setPurchaserKeywordForRequest];
        }
    }
}

@end
