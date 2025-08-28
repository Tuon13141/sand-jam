//
//  TTPUnityInterstitials.m
//  Unity-iPhone
//
//  Created by Tabtale on 17/12/2018.
//

#import <Foundation/Foundation.h>
#import "TTPUnityServiceManager.h"
#import <TT_Plugins_Core/TTPIinterstitial.h>

extern "C" {

    bool ttpInterstitialsShow(const char * location)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIinterstitial> interstitialService = [serviceManager get:@protocol(TTPIinterstitial)];
        if(interstitialService != nil){
            return [interstitialService show:[[NSString alloc] initWithUTF8String:location]];
        }
        return false;
    }
    
    bool ttpInterstitialsIsReady()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIinterstitial> interstitialService = [serviceManager get:@protocol(TTPIinterstitial)];
        if(interstitialService != nil){
            return [interstitialService isReady];
        }
        return false;
    }
    
    bool ttpInterstitialsIsVisible()
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIinterstitial> interstitialService = [serviceManager get:@protocol(TTPIinterstitial)];
        if(interstitialService != nil){
            return [interstitialService isViewVisible];
        }
        return false;
    }

    void ttpInterstitialsSetBackgroundRevenueCallback(UnityBackgroundRevenueCallback backgroundCallback)
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIinterstitial> interstitialService = [serviceManager get:@protocol(TTPIinterstitial)];
        if (interstitialService != nil) {
            [interstitialService setBackgroundRevenueCallback:backgroundCallback];
        }
    }

    bool ttpInterstitialInstanceExists() 
    {
        TTPServiceManager *serviceManager = [TTPUnityServiceManager sharedInstance];
        id<TTPIinterstitial> interstitialService = [serviceManager get:@protocol(TTPIinterstitial)];
        return interstitialService != nil;
    }
}
