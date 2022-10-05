using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MaxInstances : BaseAds
{
    public const string SOURCE = "maxapplovin";


#if HAS_MAX_APPLOVIN
    private string MaxSdkKey = "ENTER_MAX_SDK_KEY_HERE";
    private string RewardedAdUnitId = "ENTER_REWARD_AD_UNIT_ID_HERE";
    private string InterstitialAdUnitId = "ENTER_INTERSTITIAL_AD_UNIT_ID_HERE";
    private string BannerAdUnitId = "ENTER_BANNER_AD_UNIT_ID_HERE";

    int retryInterAttempt, retryRewardedAttemp, retryBannerAttemp;

    public override bool IsRewardedVideoAvailable()
    {

        isRewardedVideoLoaded = MaxSdk.IsRewardedAdReady(RewardedAdUnitId);
        return isRewardedVideoLoaded;

    }

    public override bool IsInterstitialAvailable()
    {
        isInterstitialLoaded = MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
        return isInterstitialLoaded;

    }


    public override void InitAdsNetWork(IAdsListener adsListener)
    {
        base.InitAdsNetWork(adsListener);
        Debug.Log("InitAdsNetWork " + SOURCE);
        MaxSdkKey = GameSDKSettings.MaxSdkKey;
        RewardedAdUnitId = GameSDKSettings.MAXRewardedAdUnitId;
        InterstitialAdUnitId = GameSDKSettings.MAXInterstitialAdUnitId;
        BannerAdUnitId = GameSDKSettings.MAXBannerAdUnitId;

        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            // AppLovin SDK is initialized, configure and start loading ads.
            Debug.Log("MAX SDK Initialized");
            StartCoroutine(GenerateAdsType());
            sdkInitialized = true;
            adsListener.OnAllSdkInitalized(SOURCE);
        };
        MaxSdk.SetSdkKey(MaxSdkKey);
        if (MonsterPlayerPrefs.PlayerId != -1)
        {
            MaxSdk.SetUserId(MonsterPlayerPrefs.PlayerId.ToString());
        }

        MaxSdk.InitializeSdk();

    }

    protected IEnumerator GenerateAdsType()
    {
        InitRewardedVideo();
        yield return new WaitForSeconds(0.2f);
        InitInterstitial();
    }

    #region Banner
    public override void InitBanner(bool forceShow)
    {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            return;
        }
        if (loadBannerCalled) return;
        loadBannerCalled = true;
        // Attach Callbacks
        MaxSdkCallbacks.Banner.OnAdLoadedEvent += MaxSdk_OnBannerAdLoadedEvent;
        MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += MaxSdk_OnBannerAdFailedEvent;
        MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += MaxSdk_OnBannerAdRevenuePaidEvent;
        MaxSdk.CreateBanner(BannerAdUnitId, MaxSdkBase.BannerPosition.BottomCenter);
        if (forceShow)
            LoadBannerInternal();
    }

    private void MaxSdk_OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MaxSdk_OnBannerAdRevenuePaidEvent");


        Debug.Log("ad_platform " + adInfo.ToString());
        string adId = adInfo.AdUnitIdentifier;
        double revenue = adInfo.Revenue;
        string adNetwork = adInfo.NetworkName;
        string adPlacement = adInfo.Placement;
        AdType adType = AdType.banner;
        GameAnalytics.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");


        //ServerAnalytics.LogAnalyticsAds(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue, "");
    }

    private void MaxSdk_OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.Log("MaxSdk_OnBannerAdFailedEvent");
        isBannerLoaded = false;
        adsListener.OnBannerLoadFail(SOURCE);
        if (isUniqueBaseAds)
        {
            retryBannerAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryBannerAttemp));
            Invoke("LoadBannerInternal", (float)retryDelay);
        }
    }

    private void MaxSdk_OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MaxSdk_OnBannerAdLoadedEvent");
        isBannerLoaded = true;
        retryBannerAttemp = 0;
    }

    public override void ShowBanner(bool visible)
    {
        isShownBanner = visible;
        if (isShownBanner)
        {
            MaxSdk.ShowBanner(BannerAdUnitId);
            GameAnalytics.LogAdEvent(AdType.banner, AdRequestType.show, SOURCE);
        }
        else
        {
            MaxSdk.HideBanner(BannerAdUnitId);
        }
    }

    private void LoadBannerInternal()
    {
        ShowBanner(true);
    }
    #endregion

    #region Interstitial
    public override void InitInterstitial()
    {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            return;
        }
        if (isInterstitialInitialized) return;

        // Attach callbacks
        MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += MaxSdk_OnInterstitialLoadedEvent;
        MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += MaxSdk_OnInterstitialFailedEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += MaxSdk_InterstitialFailedToDisplayEvent;
        MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += MaxSdk_OnInterstitialDismissedEvent;
        MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += MaxSdk_OnInterstitialRevenuePaidEvent;
        MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += MaxSdk_OnInterstitialDisplayedEvent;


        isInterstitialInitialized = true;

        LoadInterstitial();
    }

    private void MaxSdk_OnInterstitialDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        isShowInterstitial = true;
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.show, SOURCE);
    }

    private void MaxSdk_OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'

        Debug.Log("MaxSdk Interstitial loaded");

        isInterstitialLoading = false;
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.loadsuccess, SOURCE);

        retryInterAttempt = 0;

    }

    private void MaxSdk_OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
        Debug.Log("MaxSdk Interstitial failed to load with error code: " + errorInfo.Code);

        isInterstitialLoading = false;
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.loadfail, SOURCE);

        //if (isUniqueBaseAds)
        //{
        retryInterAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryInterAttempt));

        StartCoroutine(AdsController.StartAction(LoadInterstitial, (float)retryDelay));
        //}

    }

    private void MaxSdk_InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        Debug.Log("MaxSdk Interstitial failed to display with error code: " + errorInfo.Code);
        isShowInterstitial = false;
        adsListener.OnInterstitialFail($"{SOURCE} InterstitialAdShowFailed");
        //if (isUniqueBaseAds)
        //{
        StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
        //}
    }

    private void MaxSdk_OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        Debug.Log("MaxSdk Interstitial dismissed");
        isShowInterstitial = false;
        adsListener.OnInterstitialSuccess(SOURCE);
        StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
    }

    private void MaxSdk_OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad revenue paid. Use this callback to track user revenue.
        Debug.Log("MaxSdk Interstitial revenue paid");


        Debug.Log("ad_platform " + adInfo.ToString());
        string adId = adInfo.AdUnitIdentifier;
        double revenue = adInfo.Revenue;
        string adNetwork = adInfo.NetworkName;
        string adPlacement = adInfo.Placement;
        AdType adType = AdType.inter;

        GameAnalytics.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");

        //ServerAnalytics.LogAnalyticsAds(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue, "");
    }


    public override void LoadInterstitial()
    {
        if (!isInterstitialInitialized)
        {
            InitInterstitial();
            return;
        }
        if (isShowInterstitial)
        {
            Debug.Log("LoadInterstitial Fail isShowInterstitial");
            return;
        }
        if (isInterstitialLoading)
        {
            Debug.Log("LoadInterstitial Fail isInterstitialLoading");
            return;
        }
        if (IsInterstitialAvailable())
        {
            Debug.Log("LoadInterstitial Fail IsInterstitialAvailable");
            return;
        }
        Debug.Log("LoadInterstitial " + SOURCE);
        MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        isInterstitialLoading = true;

        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.load, SOURCE);

    }

    public override bool ShowInterstitial()
    {
        //AdsController.LogAdEvent(AdType.inter, AdRequestType.request, SOURCE);
        if (IsInterstitialAvailable())
        {
            MaxSdk.ShowInterstitial(InterstitialAdUnitId);

            return true;
        }
        else
        {
            Debug.Log("Inter is Not Ready " + SOURCE);
            return false;
        }
    }

    #endregion

    #region Rewarded
    public override void InitRewardedVideo()
    {
        if (isRewardedVideoInitialized) return;

        // Attach callbacks
        MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += MaxSdk_OnRewardedAdLoadedEvent;
        MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += MaxSdk_OnRewardedAdFailedEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += MaxSdk_OnRewardedAdFailedToDisplayEvent;
        MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += MaxSdk_OnRewardedAdDisplayedEvent;
        MaxSdkCallbacks.Rewarded.OnAdClickedEvent += MaxSdk_OnRewardedAdClickedEvent;
        MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += MaxSdk_OnRewardedAdDismissedEvent;
        MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += MaxSdk_OnRewardedAdReceivedRewardEvent;
        MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += MaxSdk_OnRewardedAdRevenuePaidEvent;
        isRewardedVideoInitialized = true;
        LoadRewardedVideo();
    }

    private void MaxSdk_OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MaxSdk_OnRewardedAdRevenuePaidEvent");

        Debug.Log("ad_platform " + adInfo.ToString());
        string adId = adInfo.AdUnitIdentifier;
        double revenue = adInfo.Revenue;
        string adNetwork = adInfo.NetworkName;
        AdType adType = AdType.videorewarded;
        string adPlacement = adInfo.Placement;

        GameAnalytics.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");
    }

    private void MaxSdk_OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3)
    {
        Debug.Log("MaxSdk_OnRewardedAdReceivedRewardEvent");
        gotRewarded = true;
    }

    private void MaxSdk_OnRewardedAdDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        Debug.Log("MaxSdk_OnRewardedAdDismissedEvent");
        isShowRewardedVideo = false;
        adsListener.OnRewardedAdsClose(SOURCE, gotRewarded);
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.canreachreward, SOURCE);
        StartCoroutine(AdsController.StartAction(LoadRewardedVideo, 0.2f));
    }

    private void MaxSdk_OnRewardedAdClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    {
        Debug.Log("MaxSdk_OnRewardedAdClickedEvent");
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.clickad, SOURCE);
    }

    private void MaxSdk_OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MaxSdk_OnRewardedAdDisplayedEvent");
        isShowRewardedVideo = true;
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.show, SOURCE);
    }

    private void MaxSdk_OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MaxSdk_OnRewardedAdFailedToDisplayEvent");
        isShowRewardedVideo = false;
        isRewardedvideoLoading = false;
        adsListener.OnRewardedAdsFail($"{SOURCE}_RewardedVideoAdShowFailed");

        //if (isUniqueBaseAds)
        //{
        StartCoroutine(AdsController.StartAction(LoadRewardedVideo, 0.2f));
        //}
    }

    private void MaxSdk_OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        Debug.Log("MaxSdk_OnRewardedAdFailedEvent");
        isRewardedvideoLoading = false;
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.loadfail, SOURCE);
        //if (isUniqueBaseAds)
        //{
        retryRewardedAttemp++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryRewardedAttemp));
        StartCoroutine(AdsController.StartAction(LoadRewardedVideo, (float)retryDelay));

        //}


    }

    private void MaxSdk_OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        Debug.Log("MaxSdk_OnRewardedAdLoadedEvent");
        isRewardedvideoLoading = false;
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.loadsuccess, SOURCE);

        retryRewardedAttemp = 0;
    }


    public override void LoadRewardedVideo()
    {
        if (!isRewardedVideoInitialized) return;
        if (isShowRewardedVideo) return;
        if (isRewardedvideoLoading) return;
        if (IsRewardedVideoAvailable()) return;
        Debug.Log("LoadRewadedVideo " + SOURCE);
        MaxSdk.LoadRewardedAd(RewardedAdUnitId);
        isRewardedvideoLoading = true;
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.load, SOURCE);
    }

    public override bool ShowRewardedVideo()
    {
        //AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.request, SOURCE);
        gotRewarded = false;
        if (IsRewardedVideoAvailable())
        {
            Debug.Log("ShowRewardedAd " + SOURCE);
            MaxSdk.ShowRewardedAd(RewardedAdUnitId);
            return true;
        }
        else
        {
            Debug.Log("RewadedVideo is Not Ready " + SOURCE);
            return false;
        }
    }

    #endregion

#endif

    public override bool ValidateNetwork(string sourceCode)
    {
        Debug.Log(" ValidateNetwork " + sourceCode + " " + sourceCode.ToLower().Equals(SOURCE.ToLower()));
        return sourceCode.ToLower().Equals(SOURCE.ToLower());
    }

    public override string GetSource()
    {
        return SOURCE;
    }
}
