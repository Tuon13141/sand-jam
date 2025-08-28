//
//  TTPUnitySetup.mm
//  Unity-iPhone
//
//  Created by Tabtale on 21/11/2018.
//

#import <Foundation/Foundation.h>



extern "C" {

    void ttpOpenUrl(const char * urlCharStr)
    {
        NSString *urlString = [[NSString alloc] initWithUTF8String:urlCharStr];
        NSURL *url = [NSURL URLWithString:urlString];
        [[UIApplication sharedApplication] openURL:url options:@{} completionHandler:^(BOOL success) {
                                if(!success){
                                    NSLog(@"ttpOpenUrl:: failed to show %@", urlString);
                                }
                            }];
    }

    
}
