using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IronSourceIntances : BaseAds
{
    public const string SOURCE = "ironsource";

#if HAS_IRONSOURCE

    int retryInterAttempt, retryRewardedAttemp, retryBannerAttemp;

    public override bool IsInterstitialAvailable()
    {
        isInterstitialLoaded = IronSource.Agent.isInterstitialReady();
        return isInterstitialLoaded;

    }

    public override bool IsRewardedVideoAvailable()
    {
        isRewardedVideoLoaded = IronSource.Agent.isRewardedVideoAvailable();
        return isRewardedVideoLoaded;

    }

    public override void InitAdsNetWork(IAdsListener adsListener)
    {
        base.InitAdsNetWork(adsListener);

        sdkInitialized = true;
        IronSourceEvents.onImpressionSuccessEvent += ImpressionSuccessEvent;
        IronSource.Agent.shouldTrackNetworkState(true);
        StartCoroutine(GenerateAdsType());
        adsListener.OnAllSdkInitalized(SOURCE);
        if (MonsterPlayerPrefs.PlayerId != -1)
        {
            IronSource.Agent.setUserId(MonsterPlayerPrefs.PlayerId.ToString());
        }
    }

    protected IEnumerator GenerateAdsType()
    {
        InitRewadedVideo();
        yield return new WaitForSeconds(0.2f);
        InitInterstitial();

    }

    private void ImpressionSuccessEvent(IronSourceImpressionData impressionData)
    {
        Debug.Log("ad_network " + impressionData.ToString());

        string adId = impressionData.instanceId;
        double? revenue = impressionData.revenue;
        string country = impressionData.country;
        string adNetwork = impressionData.adNetwork;
        string placement = impressionData.placement;
        
        AdType adType = AdType.inter;
        if (impressionData.adUnit.Contains("inter"))
        {
            adType = AdType.inter;
        }
        if (impressionData.adUnit.ToLower().Contains("rewarded"))
        {
            adType = AdType.videorewarded;
        }
        if (impressionData.adUnit.ToLower().Contains("banner"))
        {
            adType = AdType.banner;
        }
#if !NO_FIREBASE
        GameAnalytics.LogAdRevenue(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue != null ? (double)revenue : 0, placement, country);
#endif
    }



    #region Interstitial
    public override void InitInterstitial()
    {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            return;
        }
        if (isInterstitialInitialized) return;

        Debug.Log("Init Inter " + SOURCE);
        IronSource.Agent.init(GameSDKSettings.ironSourceKey, IronSourceAdUnits.INTERSTITIAL);
        IronSourceEvents.onInterstitialAdClosedEvent += IronSourceEvents_onInterstitialAdClosedEvent;
        IronSourceEvents.onInterstitialAdLoadFailedEvent += IronSourceEvents_onInterstitialAdLoadFailedEvent;
        IronSourceEvents.onInterstitialAdReadyEvent += IronSourceEvents_onInterstitialAdReadyEvent;
        IronSourceEvents.onInterstitialAdShowFailedEvent += IronSourceEvents_onInterstitialAdShowFailedEvent;
        IronSourceEvents.onInterstitialAdShowSucceededEvent += IronSourceEvents_onInterstitialAdShowSucceededEvent;
        IronSourceEvents.onInterstitialAdClickedEvent += IronSourceEvents_onInterstitialAdClickedEvent;

        isInterstitialInitialized = true;
        //interstitial
        LoadInterstitial();

    }
    private void IronSourceEvents_onInterstitialAdClickedEvent()
    {
        Debug.Log("IronSourceEvents_onInterstitialAdClickedEvent ");
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.clickad, SOURCE);
    }
    private void IronSourceEvents_onInterstitialAdShowFailedEvent(IronSourceError obj)
    {
        Debug.Log("IronSourceEvents_onInterstitialAdShowFailedEvent " + obj.getDescription());
        isShowInterstitial = false;
        adsListener.OnInterstitialFail($"{SOURCE} InterstitialAdShowFailed");

        //if (isUniqueBaseAds)
        //{
            retryInterAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryInterAttempt));
            StartCoroutine(AdsController.StartAction(LoadInterstitial, (float)retryDelay));
        //}
    }
    private void IronSourceEvents_onInterstitialAdClosedEvent()
    {
        Debug.Log("IronSourceEvents_onInterstitialAdClosedEvent ");
        isShowInterstitial = false;
        adsListener.OnInterstitialSuccess(SOURCE);

        StartCoroutine(AdsController.StartAction(LoadInterstitial, 0.2f));
    }
    private void IronSourceEvents_onInterstitialAdShowSucceededEvent()
    {
        Debug.Log("IronSourceEvents_onInterstitialAdShowSucceededEvent ");
        isShowInterstitial = true;
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.show, SOURCE);
    }

    private void IronSourceEvents_onInterstitialAdLoadFailedEvent(IronSourceError obj)
    {
        Debug.Log("IronSourceEvents_onInterstitialAdLoadFailedEvent " + obj.getDescription());
        isInterstitialLoading = false;
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.loadfail, SOURCE);

        //if (isUniqueBaseAds)
        //{
            retryInterAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryInterAttempt));
            StartCoroutine(AdsController.StartAction(LoadInterstitial, (float)retryDelay));
        //}
    }

    private void IronSourceEvents_onInterstitialAdReadyEvent()
    {
        Debug.Log("IronSourceEvents_onInterstitialAdReadyEvent ");
        isInterstitialLoading = false;
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.loadsuccess, SOURCE);

        retryInterAttempt = 0;
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
        isInterstitialLoading = true;
        IronSource.Agent.loadInterstitial();
        GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.load, SOURCE);

    }

    public override bool ShowInterstitial()
    {
        Debug.Log("ShowInterstitial " + SOURCE);
        //AdsController.LogAdEvent(AdType.inter, AdRequestType.request, SOURCE);
        if (IsInterstitialAvailable())
        {
            IronSource.Agent.showInterstitial();
            return true;
        }
        else
        {

            Debug.Log("Do Load Inter in Not Ready " + SOURCE);
            return false;
        }


    }
    #endregion

    #region RewardedVideo
    public override void InitRewadedVideo()
    {
        if (isRewardedVideoInitialized) return;

        Debug.Log("Init Rewards " + SOURCE);
        IronSource.Agent.init(GameSDKSettings.ironSourceKey, IronSourceAdUnits.REWARDED_VIDEO);
        IronSourceEvents.onRewardedVideoAdClosedEvent += IronSourceEvents_onRewardedVideoAdClosedEvent;
        IronSourceEvents.onRewardedVideoAdShowFailedEvent += IronSourceEvents_onRewardedVideoAdShowFailedEvent;
        IronSourceEvents.onRewardedVideoAdRewardedEvent += IronSourceEvents_onRewardedVideoAdRewardedEvent;
        IronSourceEvents.onRewardedVideoAvailabilityChangedEvent += IronSourceRewardedVideoAvailableChangeValue;
        IronSourceEvents.onRewardedVideoAdClickedEvent += IronSourceEvents_onRewardedVideoAdClickedEvent;
        IronSourceEvents.onRewardedVideoAdOpenedEvent += IronSourceEvents_onRewardedVideoAdOpenedEvent;

        isRewardedVideoInitialized = true;
#if !ENV_PROD
        IronSource.Agent.validateIntegration();
#endif
#if ENV_LOG
        Debug.Log("AdsController initialized");
#endif
    }

    private void IronSourceEvents_onRewardedVideoAdClickedEvent(IronSourcePlacement placement)
    {
        Debug.Log("IronSourceEvents_onRewardedVideoAdClickedEvent ");
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.clickad, SOURCE);
    }

    private void IronSourceEvents_onRewardedVideoAdClosedEvent()
    {
        Debug.Log("IronSourceEvents_onRewardedVideoAdClosedEvent ");
        isShowRewardedVideo = false;
        adsListener.OnRewardedAdsClose(SOURCE, gotRewarded);
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.canreachreward, SOURCE);
    }
    private void IronSourceEvents_onRewardedVideoAdRewardedEvent(IronSourcePlacement obj)
    {
        Debug.Log("IronSourceEvents_onRewardedVideoAdRewardedEvent ");
        gotRewarded = true;
    }
    private void IronSourceEvents_onRewardedVideoAdShowFailedEvent(IronSourceError obj)
    {
        Debug.Log("IronSourceEvents_onRewardedVideoAdRewardedEvent " + obj.getDescription());
        isShowRewardedVideo = false;
        adsListener.OnRewardedAdsFail($"{SOURCE}_RewardedVideoAdShowFailed");
    }
    private void IronSourceEvents_onRewardedVideoAdOpenedEvent()
    {
        Debug.Log("IronSourceEvents_onRewardedVideoAdOpenedEvent ");
        isShowRewardedVideo = true;
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.show, SOURCE);
    }

    private void IronSourceRewardedVideoAvailableChangeValue(bool isActive)
    {
        if (isActive)
        {
            //GameAnalytics.LogRewardedLoadSucess(PlayerPrefsManager.currentScene);
            GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.loadsuccess, SOURCE);
        }
        
        isRewardedVideoLoaded = isActive;
        Debug.Log("IronSourceRewardedVideoAvailableChangeValue: " + isActive);
    }



    public override bool ShowRewadedVideo()
    {
        Debug.Log("ShowRewadedVideo " + SOURCE);
        //AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.request, SOURCE);
        gotRewarded = false;
        if (IsRewardedVideoAvailable())
        {
            IronSource.Agent.showRewardedVideo();
            return true;
        }
        else
        {
            Debug.Log("RewadedVideo Not Ready " + SOURCE);
            return false;
        }

    }

    #endregion

    #region Banner
    public override void InitBanner(bool forceShow)
    {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            return;
        }
        Debug.Log("Init Banner " + SOURCE);
        IronSource.Agent.init(GameSDKSettings.ironSourceKey, IronSourceAdUnits.BANNER);
        IronSourceEvents.onBannerAdLoadedEvent += BannerAdLoadedEvent;
        IronSourceEvents.onBannerAdLoadFailedEvent += BannerAdLoadFailedEvent;
        IronSourceEvents.onBannerAdClickedEvent += BannerAdClickedEvent;
        IronSourceEvents.onBannerAdScreenPresentedEvent += BannerAdScreenPresentedEvent;
        IronSourceEvents.onBannerAdScreenDismissedEvent += BannerAdScreenDismissedEvent;
        IronSourceEvents.onBannerAdLeftApplicationEvent += BannerAdLeftApplicationEvent;

        LoadBannerInternal();
    }

    void BannerAdLoadedEvent()
    {
        Debug.Log("IS BannerAdLoadedEvent");
        isBannerLoaded = true;
        retryBannerAttemp = 0;
    }
    //Invoked when the banner loading process has failed.
    //@param description - string - contains information about the failure.
    void BannerAdLoadFailedEvent(IronSourceError error)
    {
        Debug.Log("IS BannerAdLoadFailedEvent " + error.ToString());
        adsListener.OnBannerLoadFail(SOURCE);
        if (isUniqueBaseAds)
        {
            retryBannerAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryBannerAttemp));
            Invoke("LoadBannerInternal", (float)retryDelay);
        }
    }
    // Invoked when end user clicks on the banner ad
    void BannerAdClickedEvent()
    {
        Debug.Log("IS BannerAdClickedEvent");
    }
    //Notifies the presentation of a full screen content following user click
    void BannerAdScreenPresentedEvent()
    {
        Debug.Log("IS BannerAdScreenPresentedEvent");
    }
    //Notifies the presented screen has been dismissed
    void BannerAdScreenDismissedEvent()
    {
        Debug.Log("IS BannerAdScreenDismissedEvent");
    }
    //Invoked when the user leaves the app
    void BannerAdLeftApplicationEvent()
    {
        Debug.Log("IS BannerAdLeftApplicationEvent");
    }

    public override void ShowBanner(bool visible)
    {
        isShownBanner = visible;
        if (isShownBanner)
        {
            if (!loadBannerCalled)
            {
                loadBannerCalled = true;
                IronSource.Agent.loadBanner(IronSourceBannerSize.SMART, GameSDKSettings.bannerPosition);
                GameAnalytics.LogAdEvent(AdType.banner, AdRequestType.show, SOURCE);
            }
            else
            {
                IronSource.Agent.displayBanner();
            }
        }
        else
        {
            IronSource.Agent.hideBanner();
        }
    }
    private void LoadBannerInternal()
    {
        loadBannerCalled = false;
        ShowBanner(true);
    }
    #endregion
    void OnApplicationPause(bool isPaused)
    {
        IronSource.Agent.onApplicationPause(isPaused);
    }
#endif
    public override bool ValidateNetwork(string sourceCode)
    {

        return sourceCode.ToLower().Equals(SOURCE.ToLower());
    }
    public override string GetSource()
    {
        return SOURCE;
    }
}
