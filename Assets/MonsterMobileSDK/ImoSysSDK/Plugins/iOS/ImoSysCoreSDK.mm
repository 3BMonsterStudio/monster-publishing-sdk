#import <Foundation/Foundation.h>
#import <AdSupport/ASIdentifierManager.h>
#import <sys/utsname.h>
#include <sys/sysctl.h>

@interface ImoSysCoreSDK : NSObject
{
}
-(NSString *) getDeviceId;
@end

@implementation ImoSysCoreSDK{
    NSString *deviceId;
    long installedAt;
}

NSString * const KEY_DEVICE_ID = @"device_id";
NSString * const KEY_INSTALL_REPORTED = @"install_reported";
NSString * const KEY_INSTALLED_AT = @"installed_at";
NSString * const KEY_REPORT_TRY_COUNT = @"report_try_count";
int const TRY_COUNT_LIMIT = 10;

static ImoSysCoreSDK *_instance;

+(ImoSysCoreSDK*) instance{
    static dispatch_once_t onceToken;
    dispatch_once(&onceToken, ^{
        NSLog(@"Creating ImoSysCoreSDK instance");
        _instance = [[ImoSysCoreSDK alloc] init];
    });
    return _instance;
}

-(id)init{
    self = [super init];
    if(self)
        [self initHelper];
    return self;
}

-(void) initHelper{
    NSLog(@"Init helper called");
    [self reportInstall];
}

-(NSString *) getDeviceId{
    [self ensureDeviceIdReady];
    return deviceId;
}

-(void) ensureDeviceIdReady{
    if(deviceId == nil){
        NSString *savedDeviceId = [[NSUserDefaults standardUserDefaults] stringForKey:KEY_DEVICE_ID];
        NSNumber *savedInstalledAt = [[NSUserDefaults standardUserDefaults] objectForKey:KEY_INSTALLED_AT];
        if(savedDeviceId == nil){
            [self generateInstallInfo];
        } else {
            installedAt = [savedInstalledAt longValue];
            deviceId = savedDeviceId;
        }
    }
}

-(void) generateInstallInfo{
    NSUUID *adId = [[ASIdentifierManager sharedManager] advertisingIdentifier];
    if([[ASIdentifierManager sharedManager] isAdvertisingTrackingEnabled] && adId != nil){
        deviceId = [@"ifa:" stringByAppendingString:adId.UUIDString];
    } else {
        NSUUID *identifierForVendor = [[UIDevice currentDevice] identifierForVendor];
        if(identifierForVendor == nil){
            deviceId = [@"uid:" stringByAppendingString:[[NSUUID UUID] UUIDString]];
        } else {
            deviceId = [@"ida:" stringByAppendingString: [identifierForVendor UUIDString]];
        }
    }
    installedAt = (long)[[NSDate date] timeIntervalSince1970];
    [self saveInstallInfo];
}

-(void) saveInstallInfo{
    NSUserDefaults *standardUserDefaults =[NSUserDefaults standardUserDefaults];
    [standardUserDefaults setObject:deviceId forKey:KEY_DEVICE_ID];
    [standardUserDefaults setObject:[NSNumber numberWithLong:installedAt] forKey:KEY_INSTALLED_AT];
    [standardUserDefaults synchronize];
}

-(void) reportInstall{
    NSUserDefaults *standardUserDefaults = [NSUserDefaults standardUserDefaults];
    bool installReported = [standardUserDefaults boolForKey:KEY_INSTALL_REPORTED];
    if(!installReported){
        long reportTryCount = [standardUserDefaults integerForKey:KEY_REPORT_TRY_COUNT];
        if(reportTryCount < TRY_COUNT_LIMIT){
            [standardUserDefaults setInteger:reportTryCount + 1 forKey:KEY_REPORT_TRY_COUNT];
            [standardUserDefaults synchronize];
            [self ensureDeviceIdReady];
            NSBundle *mainBundle = [NSBundle mainBundle];
            NSString *versionName = [mainBundle objectForInfoDictionaryKey:@"CFBundleShortVersionString"];
            NSInteger build = [[mainBundle objectForInfoDictionaryKey: (NSString *)kCFBundleVersionKey] integerValue];
            NSString *bundleId = [mainBundle bundleIdentifier];
            NSString *languageCode = [[NSLocale currentLocale] languageCode];
            UIDevice *currentDevice = [UIDevice currentDevice];
            NSString *osVersion = [currentDevice systemVersion];
            NSString *brand = @"Apple";
            struct utsname systemInfo;
            uname(&systemInfo);
            NSString *deviceModel = [NSString stringWithCString:systemInfo.machine
                                                       encoding:NSUTF8StringEncoding];
            NSDictionary *body = [NSDictionary dictionaryWithObjectsAndKeys:
                                  [NSNumber numberWithInteger:build], @"version",
                                  versionName, @"versionName",
                                  languageCode, @"lang",
                                  osVersion, @"osVersion",
                                  brand,@"deviceBrand",
                                  deviceModel,@"deviceModel",
                                  [NSNumber numberWithLong: installedAt],@"installedAt",
                                  nil];
            NSError *error;
            NSData *jsonBody = [NSJSONSerialization dataWithJSONObject:body options:NSJSONWritingPrettyPrinted error:&error];
            
            NSURL *url = [NSURL URLWithString:@"https://api.gamesontop.com/v1/analytics/installs"];
            NSMutableURLRequest *req = [[NSMutableURLRequest alloc] initWithURL:url];
            [req setValue:bundleId forHTTPHeaderField:@"pn"];
            [req setValue:@"2" forHTTPHeaderField:@"p"];
            [req setValue:deviceId forHTTPHeaderField:@"d"];
            [req setHTTPMethod:@"POST"];
            [req setValue:[NSString stringWithFormat:@"%d", (int)[jsonBody length]] forHTTPHeaderField:@"Content-Length"];
            [req setValue:@"application/json" forHTTPHeaderField:@"Content-Type"];
            [req setHTTPBody:jsonBody];
            NSURLSession *session = [NSURLSession sharedSession];
            [[session dataTaskWithRequest:req completionHandler:^(NSData * _Nullable data, NSURLResponse * _Nullable response, NSError * _Nullable error) {
                if(error == nil){
                    NSHTTPURLResponse *httpResponse = (NSHTTPURLResponse *)response;
                    if([httpResponse statusCode] == 200){
                        NSLog(@"Report install successful");
                        [standardUserDefaults setBool:true forKey:KEY_INSTALL_REPORTED];
                        [standardUserDefaults synchronize];
                    }
                }
            }] resume];
        }
    }
}

static long usSinceBoot() {
    struct timeval boottime;
    int mib[2] = {CTL_KERN, KERN_BOOTTIME};
    size_t size = sizeof(boottime);
    int rc = sysctl(mib, 2, &boottime, &size, NULL, 0);
    if (rc != 0) {
        return 0;
    }
    return boottime.tv_sec;
}

- (long) usUpTime
{
    long before_now;
    long after_now;
    struct timeval now;
    
    after_now = usSinceBoot();
    do {
        before_now = after_now;
        gettimeofday(&now, NULL);
        after_now = usSinceBoot();
    } while (after_now != before_now);
    
    return now.tv_sec - before_now;
}

@end

extern "C"{
    void IOSInitialize(){
        [ImoSysCoreSDK instance];
    }
    
    const char* IOSGetDeviceId(){
        return [[[ImoSysCoreSDK instance] getDeviceId] cStringUsingEncoding:[NSString defaultCStringEncoding]];
    }
    
    long IOSGetUpTime(){
        return [[ImoSysCoreSDK instance] usUpTime];
    }
    
}