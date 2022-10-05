//
//  IOSGameSupportHelper.h
//  Unity-iPhone
//

#import <Foundation/Foundation.h>
#import <UIKit/UIKit.h>
NS_ASSUME_NONNULL_BEGIN

@interface IOSGameSupportHelper : NSObject

@end
NS_ASSUME_NONNULL_END
extern "C"{
    typedef void (*DelegateCallbackFunction)(int status, const char *token);
}