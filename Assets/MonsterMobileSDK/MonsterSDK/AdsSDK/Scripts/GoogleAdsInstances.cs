using System;
using System.Collections;

#if HAS_ADS_MANAGER
using GoogleMobileAds.Api;
using GoogleMobileAds.Common;
#endif
using UnityEngine;

public class GoogleAdsInstances : BaseAds
{
    public const string SOURCE = "googleadsmanager";


#if HAS_ADS_MANAGER

    private string AdsManagerKey = "ENTER_MAX_SDK_KEY_HERE";
    private string RewardedAdUnitId = "ENTER_REWARD_AD_UNIT_ID_HERE";
    private string InterstitialAdUnitId = "ENTER_INTERSTITIAL_AD_UNIT_ID_HERE";
    private string BannerAdUnitId = "ENTER_BANNER_AD_UNIT_ID_HERE";
    private string OpenAdUnitId = "ENTER_OPEN_AD_UNIT_ID_HERE";

    int retryInterAttempt, retryRewardedAttemp, retryBannerAttemp;

    public override bool IsRewardedVideoAvailable()
    {
        isRewardedVideoLoaded = rewardedAd.IsLoaded();
        return isRewardedVideoLoaded;
    }

    public override bool IsInterstitialAvailable()
    {
        // isInterstitialLoaded = MaxSdk.IsInterstitialReady(InterstitialAdUnitId);
        isInterstitialLoaded = interstitial.IsLoaded();
        return isInterstitialLoaded;
    }


    public override void InitAdsNetWork(IAdsListener adsListener)
    {
        base.InitAdsNetWork(adsListener);
        Debug.Log("InitAdsNetWork " + SOURCE);
        // AdsManagerKey = GameSDKSettings.GGAdsManagerKey;
        // RewardedAdUnitId = GameSDKSettings.GGAdsRewardID;
        // InterstitialAdUnitId = GameSDKSettings.GGAdsInterstitialID;
        // BannerAdUnitId = GameSDKSettings.GGAdsBannerID;
        OpenAdUnitId = GameSDKSettings.OpenAdUnitId;

        MobileAds.Initialize(initStatus =>
        {
            StartCoroutine(GenerateAdsType());
            sdkInitialized = true;
            adsListener.OnAllSdkInitalized(SOURCE);
            print($"MobileAds init {initStatus}, sdk init: {sdkInitialized}");

            var map = initStatus.getAdapterStatusMap();
            print("Adapter: number " + map.Count);
            foreach (var keyValuePair in map)
            {
                string className = keyValuePair.Key;
                AdapterStatus status = keyValuePair.Value;
                switch (status.InitializationState)
                {
                    case AdapterState.NotReady:
                        // The adapter initialization did not complete.
                        print("Adapter: " + className + " not ready.");
                        break;
                    case AdapterState.Ready:
                        // The adapter was successfully initialized.
                        print("Adapter: " + className + " is initialized.");
                        break;
                }
            }
        });

        // SDK initialization is complete
        Debug.Log("[SDK] Ads initialized");

        // https://developers.google.com/admob/unity/mediation/applovin#optimizations
        // AppLovin.Initialize();
    }

    protected IEnumerator GenerateAdsType()
    {
        InitOpenAds();
        yield return new WaitForSeconds(0.2f);
    }

    #region Banner

    private BannerView bannerView;

    public override void InitBanner(bool forceShow)
    {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            return;
        }

        // Clean up banner ad before creating a new one.
        bannerView?.Destroy();

        if (loadBannerCalled) return;
        loadBannerCalled = true;

        AdSize adaptiveSize =
            AdSize.GetCurrentOrientationAnchoredAdaptiveBannerAdSizeWithWidth(AdSize.FullWidth);
        this.bannerView = new BannerView(BannerAdUnitId, AdSize.Banner, AdPosition.Bottom);


        // Register for ad events.
        bannerView.OnAdLoaded += HandleAdLoaded;
        bannerView.OnAdFailedToLoad += HandleAdFailedToLoad;
        bannerView.OnAdOpening += HandleAdOpening;
        bannerView.OnAdClosed += HandleAdClosed;
        bannerView.OnPaidEvent += HandleAdPaid;

        // Attach Callbacks
        // MaxSdkCallbacks.Banner.OnAdLoadedEvent += MaxSdk_OnBannerAdLoadedEvent;
        // MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += MaxSdk_OnBannerAdFailedEvent;
        // MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += MaxSdk_OnBannerAdRevenuePaidEvent;

        if (forceShow)
            LoadBannerInternal();
    }


    private void HandleAdClosed(object sender, EventArgs e)
    {
        Debug.Log("OnBannerHandleAdClosed");
    }

    private void HandleAdOpening(object sender, EventArgs e)
    {
        Debug.Log("OnBannerHandleAdOpening");
    }

    private void HandleAdPaid(object sender, AdValueEventArgs e)
    {
        Debug.Log("OnBannerHandleAdPaid");
    }
    // private void MaxSdk_OnBannerAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     Debug.Log("MaxSdk_OnBannerAdRevenuePaidEvent");
    //
    //
    //     Debug.Log("ad_platform " + adInfo.ToString());
    //     string adId = adInfo.AdUnitIdentifier;
    //     double revenue = adInfo.Revenue;
    //     string adNetwork = adInfo.NetworkName;
    //     string adPlacement = adInfo.Placement;
    //     AdType adType = AdType.banner;
    //     AdsController.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");
    //
    //
    //     //ServerAnalytics.LogAnalyticsAds(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue, "");
    // }


    private void HandleAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        Debug.Log(
            $"OnBannerAdFailedEvent, {sender}, {e.LoadAdError}, {e.LoadAdError.GetResponseInfo()}, {e.LoadAdError.GetResponseInfo().GetMediationAdapterClassName()}");
        isBannerLoaded = false;
        adsListener.OnBannerLoadFail(SOURCE);
        if (isUniqueBaseAds)
        {
            retryBannerAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryBannerAttemp));
            Invoke("LoadBannerInternal", (float) retryDelay);
        }
    }
    // private void MaxSdk_OnBannerAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    // {
    //     Debug.Log("MaxSdk_OnBannerAdFailedEvent");
    //     isBannerLoaded = false;
    //     adsListener.OnBannerLoadFail(SOURCE);
    //     if (isUniqueBaseAds)
    //     {
    //         retryBannerAttemp++;
    //         double retryDelay = Math.Pow(2, Math.Min(6, retryBannerAttemp));
    //         Invoke("LoadBannerInternal", (float)retryDelay);
    //     }
    // }

    private void HandleAdLoaded(object sender, EventArgs e)
    {
        Debug.Log("OnBannerAdLoadedEvent");
        isBannerLoaded = true;
        retryBannerAttemp = 0;
    }
    // private void MaxSdk_OnBannerAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     Debug.Log("MaxSdk_OnBannerAdLoadedEvent");
    //     isBannerLoaded = true;
    //     retryBannerAttemp = 0;
    // }

    public override void ShowBanner(bool visible)
    {
        isShownBanner = visible;
        if (isShownBanner)
        {
            // MaxSdk.ShowBanner(BannerAdUnitId);
            // AdsController.LogAdEvent(AdType.banner, AdRequestType.show, SOURCE);

            bannerView.Show();
        }
        else
        {
            // MaxSdk.HideBanner(BannerAdUnitId);
            bannerView.Hide();
        }
    }

    private void LoadBannerInternal()
    {
        AdRequest adRequest = new AdRequest.Builder().Build();
        bannerView.LoadAd(adRequest);
        ShowBanner(true);
    }

    #endregion

    #region Interstitial

    private InterstitialAd interstitial;

    public override void InitInterstitial()
    {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            return;
        }

        if (isInterstitialInitialized) return;

        // Attach callbacks
        interstitial = new InterstitialAd(InterstitialAdUnitId);
        // Called when an ad request has successfully loaded.
        interstitial.OnAdLoaded += HandleOnAdLoaded;
        // Called when an ad request failed to load.
        interstitial.OnAdFailedToLoad += HandleOnAdFailedToLoad;
        // Called when an ad is shown.
        interstitial.OnAdFailedToShow += HandleOnAdFailedToShow;

        // Called when the ad is closed.
        interstitial.OnAdClosed += HandleOnAdClosed;
        interstitial.OnPaidEvent += HandleOnPaidEvent;
        interstitial.OnAdOpening += HandleOnAdOpening;
        interstitial.OnAdDidRecordImpression += (sender, args) => { };
        // MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += MaxSdk_OnInterstitialLoadedEvent;
        // MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += MaxSdk_OnInterstitialFailedEvent;
        // MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += MaxSdk_InterstitialFailedToDisplayEvent;
        // MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += MaxSdk_OnInterstitialDismissedEvent;
        // MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += MaxSdk_OnInterstitialRevenuePaidEvent;
        // MaxSdkCallbacks.Interstitial.OnAdDisplayedEvent += MaxSdk_OnInterstitialDisplayedEvent;


        isInterstitialInitialized = true;

        LoadInterstitial();
    }

    private void HandleOnAdOpening(object sender, EventArgs e)
    {
        isShowInterstitial = true;
    }
    // private void MaxSdk_OnInterstitialDisplayedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    // {
    //     isShowInterstitial = true;
    //     AdsController.LogAdEvent(AdType.inter, AdRequestType.show, SOURCE);
    // }

    private void HandleOnAdLoaded(object sender, EventArgs e)
    {
        Debug.Log($"inter loaded");

        isInterstitialLoading = false;
        // AdsController.LogAdEvent(AdType.inter, AdRequestType.loadsuccess, SOURCE);

        retryInterAttempt = 0;
    }

    // private void MaxSdk_OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'
    //
    //     Debug.Log("MaxSdk Interstitial loaded");
    //
    //     isInterstitialLoading = false;
    //     AdsController.LogAdEvent(AdType.inter, AdRequestType.loadsuccess, SOURCE);
    //
    //     retryInterAttempt = 0;
    //
    // }

    private void HandleOnAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        Debug.Log(
            $"Inter AdFailedEvent, {sender}, {e.LoadAdError}, {e.LoadAdError.GetResponseInfo()}, {e.LoadAdError.GetResponseInfo().GetMediationAdapterClassName()}");
        isInterstitialInitialized = false;
        retryInterAttempt++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryInterAttempt));

        StartCoroutine(AdsController.StartAction(LoadInterstitial, (float) retryDelay));
    }
    // private void MaxSdk_OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    // {
    //     // Interstitial ad failed to load. We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds).
    //     Debug.Log("MaxSdk Interstitial failed to load with error code: " + errorInfo.Code);
    //
    //     isInterstitialLoading = false;
    //     AdsController.LogAdEvent(AdType.inter, AdRequestType.loadfail, SOURCE);
    //
    //     //if (isUniqueBaseAds)
    //     //{
    //     retryInterAttempt++;
    //     double retryDelay = Math.Pow(2, Math.Min(6, retryInterAttempt));
    //
    //     StartCoroutine(AdsController.StartAction(LoadInterstitial, (float)retryDelay));
    //     //}
    //
    // }

    private void HandleOnAdFailedToShow(object sender, AdErrorEventArgs e)
    {
        isInterstitialInitialized = false;
        isShowInterstitial = false;
        adsListener.OnInterstitialFail($"{SOURCE} InterstitialAdShowFailed");
        StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
    }
    // private void MaxSdk_InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    // {
    //     // Interstitial ad failed to display. We recommend loading the next ad
    //     Debug.Log("MaxSdk Interstitial failed to display with error code: " + errorInfo.Code);
    //     isShowInterstitial = false;
    //     adsListener.OnInterstitialFail($"{SOURCE} InterstitialAdShowFailed");
    //     //if (isUniqueBaseAds)
    //     //{
    //     StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
    //     //}
    // }

    private void HandleOnAdClosed(object sender, EventArgs e)
    {
        isInterstitialInitialized = false;
        isShowInterstitial = false;
        adsListener.OnInterstitialSuccess(SOURCE);
        StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
    }
    // private void MaxSdk_OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     // Interstitial ad is hidden. Pre-load the next ad
    //     Debug.Log("MaxSdk Interstitial dismissed");
    //     isShowInterstitial = false;
    //     adsListener.OnInterstitialSuccess(SOURCE);
    //     StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
    // }

    private void HandleOnPaidEvent(object sender, EventArgs e)
    {
        Debug.Log($"HandleOnPaidEvent ");
        // Debug.Log("ad_platform " + adInfo.ToString());
        // string adId = adInfo.AdUnitIdentifier;
        // double revenue = adInfo.Revenue;
        // string adNetwork = adInfo.NetworkName;
        // string adPlacement = adInfo.Placement;
        // AdType adType = AdType.inter;
        //
        // AdsController.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");
    }
    // private void MaxSdk_OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     // Interstitial ad revenue paid. Use this callback to track user revenue.
    //     Debug.Log("MaxSdk Interstitial revenue paid");
    //
    //
    //     Debug.Log("ad_platform " + adInfo.ToString());
    //     string adId = adInfo.AdUnitIdentifier;
    //     double revenue = adInfo.Revenue;
    //     string adNetwork = adInfo.NetworkName;
    //     string adPlacement = adInfo.Placement;
    //     AdType adType = AdType.inter;
    //
    //     AdsController.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");
    //
    //     //ServerAnalytics.LogAnalyticsAds(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue, "");
    // }


    public override void LoadInterstitial()
    {
        if (!isInterstitialInitialized)
        {
            InitInterstitial();
            // return;
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
        // MaxSdk.LoadInterstitial(InterstitialAdUnitId);
        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the interstitial with the request.
        interstitial.LoadAd(request);

        isInterstitialLoading = true;

        // AdsController.LogAdEvent(AdType.inter, AdRequestType.load, SOURCE);
    }

    public override bool ShowInterstitial()
    {
        //AdsController.LogAdEvent(AdType.inter, AdRequestType.request, SOURCE);
        if (IsInterstitialAvailable())
        {
            // MaxSdk.ShowInterstitial(InterstitialAdUnitId);
            interstitial.Show();
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

    private RewardedAd rewardedAd;

    public override void InitRewardedVideo()
    {
        if (isRewardedVideoInitialized) return;

        rewardedAd = new RewardedAd(RewardedAdUnitId);
        // Attach callbacks
        // MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += MaxSdk_OnRewardedAdLoadedEvent;
        // MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += MaxSdk_OnRewardedAdFailedEvent;
        //
        // MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += MaxSdk_OnRewardedAdDisplayedEvent;
        // MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += MaxSdk_OnRewardedAdFailedToDisplayEvent;
        //
        // // MaxSdkCallbacks.Rewarded.OnAdClickedEvent += MaxSdk_OnRewardedAdClickedEvent; //NOT HAVE
        // MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += MaxSdk_OnRewardedAdDismissedEvent;
        // MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += MaxSdk_OnRewardedAdReceivedRewardEvent;
        // MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += MaxSdk_OnRewardedAdRevenuePaidEvent;

        // Called when an ad request has successfully loaded.
        this.rewardedAd.OnAdLoaded += HandleRewardedAdLoaded;
        // Called when an ad request failed to load.
        this.rewardedAd.OnAdFailedToLoad += HandleRewardedAdFailedToLoad;
        // Called when an ad is shown.
        this.rewardedAd.OnAdOpening += HandleRewardedAdOpening;
        // Called when an ad request failed to show.
        this.rewardedAd.OnAdFailedToShow += HandleRewardedAdFailedToShow;
        // Called when the ad is closed.
        this.rewardedAd.OnAdClosed += HandleRewardedAdClosed;
        // Called when the user should be rewarded for interacting with the ad.
        this.rewardedAd.OnUserEarnedReward += HandleUserEarnedReward;

        rewardedAd.OnPaidEvent += HandleRewardPaidEvent;
        rewardedAd.OnAdDidRecordImpression += HandleOnRewardRecodImpression;


        isRewardedVideoInitialized = true;
        LoadRewardedVideo();
    }


    private void HandleOnRewardRecodImpression(object sender, EventArgs e)
    {
        Debug.Log($"HandleOnRewardRecodImpression");
    }

    private void HandleRewardPaidEvent(object sender, AdValueEventArgs e)
    {
        Debug.Log("HandleRewardPaidEvent");

        // Debug.Log("ad_platform " + adInfo.ToString());
        // string adId = adInfo.AdUnitIdentifier;
        // double revenue = adInfo.Revenue;
        // string adNetwork = adInfo.NetworkName;
        // AdType adType = AdType.videorewarded;
        // string adPlacement = adInfo.Placement;
    }

    // private void MaxSdk_OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdRevenuePaidEvent");
    //
    //     Debug.Log("ad_platform " + adInfo.ToString());
    //     string adId = adInfo.AdUnitIdentifier;
    //     double revenue = adInfo.Revenue;
    //     string adNetwork = adInfo.NetworkName;
    //     AdType adType = AdType.videorewarded;
    //     string adPlacement = adInfo.Placement;
    //
    //     AdsController.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, adPlacement, "");
    // }

    private void HandleUserEarnedReward(object sender, Reward e)
    {
        gotRewarded = true;
        isRewardedVideoInitialized = false;
    }
    // private void MaxSdk_OnRewardedAdReceivedRewardEvent(string arg1, MaxSdkBase.Reward arg2, MaxSdkBase.AdInfo arg3)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdReceivedRewardEvent");
    //     gotRewarded = true;
    // }

    private void HandleRewardedAdClosed(object sender, EventArgs e)
    {
        isRewardedVideoInitialized = false;
        isShowRewardedVideo = false;
        adsListener.OnRewardedAdsClose(SOURCE, gotRewarded);
        StartCoroutine(AdsController.StartAction(LoadRewardedVideo, 0.2f));
    }
    // private void MaxSdk_OnRewardedAdDismissedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdDismissedEvent");
    //     isShowRewardedVideo = false;
    //     adsListener.OnRewardedAdsClose(SOURCE, gotRewarded);
    //     AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.canreachreward, SOURCE);
    //     StartCoroutine(AdsController.StartAction(LoadRewadedVideo, 0.2f));
    // }

    private void HandleRewardedAdOpening(object sender, EventArgs e)
    {
        isShowRewardedVideo = true;
        isRewardedVideoInitialized = false;
    }


    // private void MaxSdk_OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdDisplayedEvent");
    //     isShowRewardedVideo = true;
    //     // AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.show, SOURCE);
    // }

    // private void MaxSdk_OnRewardedAdClickedEvent(string arg1, MaxSdkBase.AdInfo arg2)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdClickedEvent");
    //     // AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.clickad, SOURCE);
    // }
    private void HandleRewardedAdFailedToShow(object sender, AdErrorEventArgs e)
    {
        isRewardedVideoInitialized = false;
        isShowRewardedVideo = false;
        isRewardedvideoLoading = false;
        adsListener.OnRewardedAdsFail($"{SOURCE}_RewardedVideoAdShowFailed");

        //if (isUniqueBaseAds)
        //{
        StartCoroutine(AdsController.StartAction(LoadRewardedVideo, 0.2f));
    }
    // private void MaxSdk_OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdFailedToDisplayEvent");
    //     isShowRewardedVideo = false;
    //     isRewardedvideoLoading = false;
    //     adsListener.OnRewardedAdsFail($"{SOURCE}_RewardedVideoAdShowFailed");
    //
    //     //if (isUniqueBaseAds)
    //     //{
    //     StartCoroutine(AdsController.StartAction(LoadRewadedVideo, 0.2f));
    //     //}
    // }

    private void HandleRewardedAdFailedToLoad(object sender, AdFailedToLoadEventArgs e)
    {
        isRewardedVideoInitialized = false;
        isRewardedvideoLoading = false;
        retryRewardedAttemp++;
        double retryDelay = Math.Pow(2, Math.Min(6, retryRewardedAttemp));
        StartCoroutine(AdsController.StartAction(LoadRewardedVideo, (float) retryDelay));
    }
    // private void MaxSdk_OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdFailedEvent");
    //     isRewardedvideoLoading = false;
    //     AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.loadfail, SOURCE);
    //     //if (isUniqueBaseAds)
    //     //{
    //     retryRewardedAttemp++;
    //     double retryDelay = Math.Pow(2, Math.Min(6, retryRewardedAttemp));
    //     StartCoroutine(AdsController.StartAction(LoadRewadedVideo, (float)retryDelay));
    // }

    private void HandleRewardedAdLoaded(object sender, EventArgs e)
    {
        Debug.Log($"GG ads reward loaded");

        isRewardedVideoInitialized = false;
        isRewardedvideoLoading = false;
        retryRewardedAttemp = 0;
    }
    // private void MaxSdk_OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     Debug.Log("MaxSdk_OnRewardedAdLoadedEvent");
    //     isRewardedvideoLoading = false;
    //     AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.loadsuccess, SOURCE);
    //
    //     retryRewardedAttemp = 0;
    // }


    public override void LoadRewardedVideo()
    {
        if (!isRewardedVideoInitialized)
        {
            InitRewardedVideo();
        }

        if (isShowRewardedVideo) return;
        if (isRewardedvideoLoading) return;
        if (IsRewardedVideoAvailable()) return;
        Debug.Log("LoadRewadedVideo " + SOURCE);

        // Create an empty ad request.
        AdRequest request = new AdRequest.Builder().Build();
        // Load the rewarded ad with the request.
        this.rewardedAd.LoadAd(request);

        // MaxSdk.LoadRewardedAd(RewardedAdUnitId);
        isRewardedvideoLoading = true;
        // AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.load, SOURCE);
    }

    public override bool ShowRewardedVideo()
    {
        //AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.request, SOURCE);
        gotRewarded = false;
        if (IsRewardedVideoAvailable())
        {
            Debug.Log("ShowRewardedAd " + SOURCE);
            // MaxSdk.ShowRewardedAd(RewardedAdUnitId);
            rewardedAd.Show();
            return true;
        }
        else
        {
            Debug.Log("RewadedVideo is Not Ready " + SOURCE);
            return false;
        }
    }

    #endregion

    #region OpenAds

    private AppOpenAd openAds;
    private bool isShowingAd = false;
    private DateTime loadTime;
    private bool firstOpenAds;

    private void InitOpenAds()
    {
        // Load an app open ad when the scene starts
        LoadOpenAd();

        // Listen to application foreground and background events.
        AppStateEventNotifier.AppStateChanged += OnAppStateChanged;
    }

    private void OnAppStateChanged(AppState state)
    {
        // Display the app open ad when the app is foregrounded.
        Debug.Log("App State is " + state + " Ads controller: init " + AdsController.isShowing);
        if (state == AppState.Foreground & !AdsController.isShowing)
        {
            ShowOpenAdIfAvailable();
        }
    }

    private bool IsAdAvailable => openAds != null && (System.DateTime.UtcNow - loadTime).TotalHours < 4;

    private void LoadOpenAd()
    {
        if (IsAdAvailable)
        {
            return;
        }

        AdRequest request = new AdRequest.Builder().Build();
        AppOpenAd.LoadAd(OpenAdUnitId, ScreenOrientation.Portrait, request, ((appOpenAd, error) =>
        {
            if (error != null)
            {
                Debug.LogFormat("Failed to load the ad. (reason: {0})", error.LoadAdError.GetMessage());
                return;
            }

            openAds = appOpenAd;
            loadTime = DateTime.UtcNow;
        }));
    }

    public override void ShowFirstOpenAds()
    {
#if !UNITY_EDITOR
        StartCoroutine(ShowFirstOpenAdsCO());
#endif
    }

    IEnumerator ShowFirstOpenAdsCO()
    {
        yield return new WaitUntil(() => IsAdAvailable);
        ShowOpenAdIfAvailable();
    }

    public override void ShowOpenAdIfAvailable()
    {
        Debug.Log($"Show open ads {IsAdAvailable} {isShowingAd} ");
        if (!IsAdAvailable || isShowingAd)
        {
            return;
        }

        openAds.OnAdDidDismissFullScreenContent += HandleOpenAdsDidDismissFullScreenContent;
        openAds.OnAdFailedToPresentFullScreenContent += HandleOpenAdsFailedToPresentFullScreenContent;
        openAds.OnAdDidPresentFullScreenContent += HandleOpenAdsDidPresentFullScreenContent;
        openAds.OnAdDidRecordImpression += HandleOpenAdsDidRecordImpression;
        openAds.OnPaidEvent += HandlePaidEvent;

        openAds.Show();
    }

    private void HandleOpenAdsDidDismissFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Closed app open ad");
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        openAds = null;
        isShowingAd = false;
        AdsController.isShowing = false;
        AppBackChecker.State = AppBackgroundState.None;
        LoadOpenAd();
    }

    private void HandleOpenAdsFailedToPresentFullScreenContent(object sender, AdErrorEventArgs args)
    {
        Debug.LogFormat("Failed to present the ad (reason: {0})", args.AdError.GetMessage());
        // Set the ad to null to indicate that AppOpenAdManager no longer has another ad to show.
        openAds = null;
        LoadOpenAd();
    }

    private void HandleOpenAdsDidPresentFullScreenContent(object sender, EventArgs args)
    {
        Debug.Log("Displayed app open ad");
        isShowingAd = true;
        AdsController.isShowing = true;
        AppBackChecker.State = AppBackgroundState.AdsShowing;
    }

    private void HandleOpenAdsDidRecordImpression(object sender, EventArgs args)
    {
        Debug.Log("Recorded ad impression");
    }

    private void HandlePaidEvent(object sender, AdValueEventArgs args)
    {
        Debug.LogFormat("Received paid event. (currency: {0}, value: {1}",
            args.AdValue.CurrencyCode, args.AdValue.Value);
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