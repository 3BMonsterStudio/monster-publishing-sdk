//
//  IOSGameSupportHelper.m
//  Unity-iPhone
//
//  Created by tuandigital on 8/30/20.
//

#import "IOSGameSupportHelper.h"
#import <FBSDKShareKit/FBSDKShareKit.h>
#import <DeviceCheck/DeviceCheck.h>

@implementation IOSGameSupportHelper

@end
static DelegateCallbackFunction dcDelegate = nil;

extern UIViewController *UnityGetGLViewController();

extern "C"{
    
    void NativeShareToOthers(const char *url){
        NSString *textToShare = [NSString stringWithCString:url encoding:NSUTF8StringEncoding];
        NSArray *items = @[textToShare];
        UIActivityViewController *activityVC = [[UIActivityViewController alloc] initWithActivityItems:items applicationActivities:nil];
        activityVC.modalPresentationStyle = UIModalPresentationPopover;
        UIViewController *parentController = UnityGetGLViewController();
        if(UI_USER_INTERFACE_IDIOM() == UIUserInterfaceIdiomPad){
            activityVC.modalPresentationStyle = UIModalPresentationPopover;
            UIPopoverPresentationController *popoverController = activityVC.popoverPresentationController;
            if(popoverController != nil){
                popoverController.sourceView = parentController.view;
                popoverController.sourceRect = CGRectMake(0,200,768,20);
                [parentController presentViewController:activityVC animated:YES completion:nil];
            }
        } else{
            [parentController presentViewController:activityVC animated:YES completion:nil];
        }
    }
    
    void NativeShareToFBMessenger(const char *urlCString){
        NSString *urlString = [NSString stringWithCString:urlCString encoding:NSUTF8StringEncoding];
//        NSString *encodedUrlString = [urlString stringByAddingPercentEncodingWithAllowedCharacters:[NSCharacterSet URLQueryAllowedCharacterSet]];
//        NSString *fullUrlString = [NSString stringWithFormat:@"fb-messenger://share?link=%@", encodedUrlString];
//        NSURL *urlToShare = [NSURL URLWithString:fullUrlString];
        FBSDKShareLinkContent *content = [[FBSDKShareLinkContent alloc] init];
        content.contentURL = [NSURL URLWithString:urlString];
        FBSDKMessageDialog *messageDialog = [[FBSDKMessageDialog alloc] init];
        messageDialog.delegate = nil;
        [messageDialog setShareContent:content];
        if([messageDialog canShow]){
            [messageDialog show];
            
//            [[UIApplication sharedApplication] openURL:urlToShare options:@{} completionHandler:nil];
        } else {
            UIAlertController *alertController = [UIAlertController alertControllerWithTitle:@"Alert" message:@"Please install Facebook Messenger to start sharing" preferredStyle:UIAlertControllerStyleAlert];
            [alertController addAction:[UIAlertAction actionWithTitle:@"OK" style:UIAlertActionStyleDefault handler:nil]];
            UIViewController *parentController = UnityGetGLViewController();
            [parentController presentViewController:alertController animated:true completion:nil];
        }
    }

    void SetDCTokenDelegate(DelegateCallbackFunction callback){
        dcDelegate = callback;
    }

    void RequestDCDeviceToken(){
        if(@available(iOS 11, *)){
            if(DCDevice.currentDevice.isSupported){
                [DCDevice.currentDevice generateTokenWithCompletionHandler:^(NSData * _Nullable token, NSError * _Nullable error) {
                    if(dcDelegate != nil){
                        if(error != nil){
                            dcDelegate(2, "");
                        } else {
                            NSString *strToken =[token base64EncodedStringWithOptions:NSDataBase64EncodingEndLineWithLineFeed];
                            dcDelegate(0, [strToken cStringUsingEncoding:NSASCIIStringEncoding]);
                        }
                    }
                }];
            } else {
                if(dcDelegate != nil){
                    dcDelegate(3, "");
                }
            }
        } else {
            if(dcDelegate != nil){
                dcDelegate(3, "");
            }
        }
    }
}