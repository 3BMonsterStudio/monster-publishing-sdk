using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSDKSettings
{
    public static string policyUrl = "https://imosys.net/policy.html";
    public static string termOfUseUrl = "https://imosys.net/term-of-use.html";
    public static string appsflyerKey = "Gio8zFFVzAZJGr9sbHXoVb";
    public static string appleAppId = "1585931537";

#if UNITY_ANDROID

#if HAS_IRONSOURCE
    public static string ironSourceKey = "137701659";
    //public static string abmobOpenAdId = "ca-app-pub-6884037586522683/9310454908";
    public static string admobAppId = "ca-app-pub-6884037586522683~4479291165";
    public static IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.BOTTOM;
#endif
#if HAS_MOPUB
    public static string MopubRewardedID = "";
    public static string MopubInterstitialID = "";
    public static string MopubBannerID = "";
#endif
#if HAS_MAX_APPLOVIN
    public static string MaxSdkKey = "iVth1Xns1vXaWlDJGqmE995884znPLc6rUpt1ai2TFc--qHFe2ToBZwKeu53DiA2C-vDTcF0ss_rfH5JJZ5V80";
    public static string MAXRewardedAdUnitId = "524a3bd03f28b291";
    public static string MAXInterstitialAdUnitId = "4228a6cea7dd3ac3";
    public static string MAXBannerAdUnitId = "64cf2c3a2ea63ee4";
#endif
#if HAS_ADS_MANAGER
    public static string AdsManagerKey = "";
    public static string RewardedAdUnitId = "";
    public static string InterstitialAdUnitId = "";
    public static string BannerAdUnitId = "";
    public static string OpenAdUnitId = "ca-app-pub-9085952480111777/2997064703";
#endif

#elif UNITY_IOS

#if HAS_IRONSOURCE
    public static string ironSourceKey = "137701659";
    //public static string abmobOpenAdId = "ca-app-pub-6884037586522683/9310454908";
    public static string admobAppId = "ca-app-pub-6884037586522683~4479291165";
    public static IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.BOTTOM;
#endif
#if HAS_MOPUB
    public static string MopubRewardedID = "";
    public static string MopubInterstitialID = "";
    public static string MopubBannerID = "";
#endif
#if HAS_MAX_APPLOVIN
    public static string MaxSdkKey = "iVth1Xns1vXaWlDJGqmE995884znPLc6rUpt1ai2TFc--qHFe2ToBZwKeu53DiA2C-vDTcF0ss_rfH5JJZ5V80";
    public static string MAXRewardedAdUnitId = "524a3bd03f28b291";
    public static string MAXInterstitialAdUnitId = "4228a6cea7dd3ac3";
    public static string MAXBannerAdUnitId = "64cf2c3a2ea63ee4";
#endif

#endif
}
