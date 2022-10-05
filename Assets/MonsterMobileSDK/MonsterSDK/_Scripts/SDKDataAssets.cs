using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "SDKDataIdSetting", menuName = "Monster SDK Settings")]
public class SDKDataAssets : ScriptableObject
{
    public string policyUrl = "https://imosys.net/policy.html";
    public string termOfUseUrl = "https://imosys.net/term-of-use.html";
    public string appsflyerKey = "Gio8zFFVzAZJGr9sbHXoVb";
    public string appleAppId = "1585931537";


#if HAS_IRONSOURCE
    [Header("Android ID")]
    public string ironSourceKey = "137701659";
    //public static string abmobOpenAdId = "ca-app-pub-6884037586522683/9310454908";
    public string admobAppId = "ca-app-pub-6884037586522683~4479291165";
    public IronSourceBannerPosition bannerPosition = IronSourceBannerPosition.BOTTOM;
#endif
#if HAS_MOPUB
[Header("Android ID")]
    public string MopubRewardedID = "";
    public string MopubInterstitialID = "";
    public string MopubBannerID = "";
#endif
#if HAS_MAX_APPLOVIN
    [Header("Android ID")] public string MaxSdkKey =
        "iVth1Xns1vXaWlDJGqmE995884znPLc6rUpt1ai2TFc--qHFe2ToBZwKeu53DiA2C-vDTcF0ss_rfH5JJZ5V80";

    public string MAXRewardedAdUnitId = "524a3bd03f28b291";
    public string MAXInterstitialAdUnitId = "4228a6cea7dd3ac3";
    public string MAXBannerAdUnitId = "64cf2c3a2ea63ee4";
#endif
#if HAS_ADS_MANAGER
    public string AdsManagerKey = "";
    public string RewardedAdUnitId = "";
    public string InterstitialAdUnitId = "";
    public string BannerAdUnitId = "";
    public string OpenAdUnitId = "ca-app-pub-9085952480111777/2997064703";
#endif

#if HAS_IRONSOURCE
    [Header("iOS ID")]
    public string ironSourceKeyiOS = "137701659";
    //public string abmobOpenAdId = "ca-app-pub-6884037586522683/9310454908";
    public string admobAppIdiOS = "ca-app-pub-6884037586522683~4479291165";
    public IronSourceBannerPosition bannerPositioniOS = IronSourceBannerPosition.BOTTOM;
#endif
#if HAS_MOPUB
    [Header("iOS ID")]
    public string MopubRewardedIDiOS = "";
    public string MopubInterstitialIDiOS = "";
    public string MopubBannerIDiOS = "";
#endif
#if HAS_MAX_APPLOVIN
    [Header("iOS ID")] public string MaxSdkKeyiOS =
        "iVth1Xns1vXaWlDJGqmE995884znPLc6rUpt1ai2TFc--qHFe2ToBZwKeu53DiA2C-vDTcF0ss_rfH5JJZ5V80";

    public string MAXRewardedAdUnitIdiOS = "524a3bd03f28b291";
    public string MAXInterstitialAdUnitIdiOS = "4228a6cea7dd3ac3";
    public string MAXBannerAdUnitIdiOS = "64cf2c3a2ea63ee4";
#endif

    public void ParseData()
    {
        GameSDKSettings.termOfUseUrl = this.termOfUseUrl;
        GameSDKSettings.policyUrl = policyUrl;
        GameSDKSettings.appsflyerKey = appsflyerKey;
        GameSDKSettings.appleAppId = appleAppId;
#if UNITY_ANDROID
#if HAS_IRONSOURCE
        GameSDKSettings.ironSourceKey = ironSourceKey;
        GameSDKSettings.admobAppId = admobAppId;
        GameSDKSettings.bannerPosition = bannerPosition;
#endif
#if HAS_MAX_APPLOVIN
        GameSDKSettings.MaxSdkKey = MaxSdkKey;
        GameSDKSettings.MAXRewardedAdUnitId = MAXRewardedAdUnitId;
        GameSDKSettings.MAXInterstitialAdUnitId = MAXInterstitialAdUnitId;
        GameSDKSettings.MAXBannerAdUnitId = MAXBannerAdUnitId;
#endif
#if HAS_ADS_MANAGER
        GameSDKSettings.AdsManagerKey = AdsManagerKey;
        GameSDKSettings.RewardedAdUnitId = RewardedAdUnitId;
        GameSDKSettings.InterstitialAdUnitId = InterstitialAdUnitId;
        GameSDKSettings.BannerAdUnitId = BannerAdUnitId;
        GameSDKSettings.OpenAdUnitId = OpenAdUnitId;
#endif
#if HAS_MOPUB
        GameSDKSettings.MopubRewardedID = MopubRewardedID;
        GameSDKSettings.MopubInterstitialID = MopubInterstitialID;
        GameSDKSettings.MopubBannerID = MopubBannerID;
#endif
#endif
#if UNITY_IOS
#if HAS_IRONSOURCE
        GameSDKSettings.ironSourceKey = ironSourceKeyiOS;
        GameSDKSettings.admobAppId = admobAppIdiOS;
        GameSDKSettings.bannerPosition = bannerPositioniOS;
#endif
#if HAS_MOPUB
        GameSDKSettings.MaxSdkKey = MaxSdkKeyiOS;
        GameSDKSettings.MAXRewardedAdUnitId = MAXRewardedAdUnitIdiOS;
        GameSDKSettings.MAXInterstitialAdUnitId = MAXInterstitialAdUnitIdiOS;
        GameSDKSettings.MAXBannerAdUnitId = MAXBannerAdUnitIdiOS;
#endif
#if HAS_MAX_APPLOVIN
        GameSDKSettings.MopubRewardedID = MopubRewardedIDiOS;
        GameSDKSettings.MopubInterstitialID = MopubInterstitialIDiOS;
        GameSDKSettings.MopubBannerID = MopubBannerIDiOS;
#endif
#endif
    }
}