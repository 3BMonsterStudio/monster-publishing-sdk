using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;


public class MopubInstances : BaseAds {
    public const string SOURCE = "mopub";


#if HAS_MOPUB
    int retryInterAttempt, retryRewardedAttemp, retryBannerAttemp;
    [Serializable] public class InitializedEvent : UnityEvent<string> { }
    [Tooltip("Set the logging verbosity level for the MoPub SDK.")]
    public bool UseMopubManagerInstances;

    [Header("Initialization")]

    [Tooltip("(iOS Only) The app id on the App Store.  Used to track conversions.")]
    public string itunesAppId;

    [Tooltip("Enables or disables location support for banners and interstitials.")]
    public bool LocationAware;

    [Tooltip("Indicate that this app has Legitimate Interest for GDPR data tracking.")]
    public bool AllowLegitimateInterest;

    [Tooltip("Set the logging verbosity level for the MoPub SDK.")]
    public MoPub.LogLevel LogLevel = MoPub.LogLevel.Info;



    [Header("Callback")]

    // Add any callbacks to this event that must execute once the SDK has initialized.
    public InitializedEvent OnInitialized;

    private string RewardedID = "5452c702db3a4e958468cef29fb4aa28";
    private string InterstitialID = "2c27580895144defb38803a68988624b";
    private string BannerID = "a9ba2e8976b94ca89edd26f64cf23e1e";


    public override bool IsRewardedVideoAvailable() {

        isRewardedVideoLoaded = MoPub.HasRewardedVideo(RewardedID);
        return isRewardedVideoLoaded;

    }

    public override bool IsInterstitialAvailable() {
        isInterstitialLoaded = MoPub.IsInterstitialReady(InterstitialID);
        return isInterstitialLoaded;

    }

    public MoPub.SdkConfiguration SdkConfiguration {
        get {
            var config = new MoPub.SdkConfiguration {
                AdUnitId = RewardedID,
                AllowLegitimateInterest = AllowLegitimateInterest,
                LogLevel = LogLevel,
                MediatedNetworks = GetComponents<MoPubNetworkConfig>().Where(nc => nc.isActiveAndEnabled).Select(nc => nc.NetworkOptions).ToArray()
            };
            SendMessage("OnSdkConfiguration", config, SendMessageOptions.DontRequireReceiver);
            return config;
        }
    }

    public override void InitAdsNetWork(IAdsListener adsListener) {
        base.InitAdsNetWork(adsListener);
        RewardedID = GameSDKSettings.MopubRewardedID;
        InterstitialID = GameSDKSettings.MopubInterstitialID;
        BannerID = GameSDKSettings.MopubBannerID;
        Debug.Log("InitAdsNetWork " + SOURCE);
        MoPub.InitializeSdk(UseMopubManagerInstances ? MoPubManager.Instance.SdkConfiguration : SdkConfiguration);
        MoPub.ReportApplicationOpen(itunesAppId);
        MoPub.EnableLocationSupport(LocationAware);

        MoPubManager.OnSdkInitializedEvent += fwdSdkInitialized;
        MoPubManager.OnImpressionTrackedEvent += OnImpressionTrackedEvent;
    }

    private void OnImpressionTrackedEvent(string adUnitId, MoPub.ImpressionData impressionData) {
        // Feed impression data into internal tools or send to third-party analytics

        //var myImpressionObject = JsonUtility.FromJson<MoPub.ImpressionData>(impressionData.JsonRepresentation);
        Debug.Log("ad_platform " + impressionData.JsonRepresentation);
        string adId = impressionData.AdUnitId;
        double? revenue = impressionData.PublisherRevenue;
        string currency = impressionData.Currency;
        string adNetwork = impressionData.AdUnitName;
        AdType adType = AdType.inter;
        if (impressionData.AdUnitFormat.Contains("inter"))
        {
            adType = AdType.inter;
        }
        if (impressionData.AdUnitFormat.ToLower().Contains("rewarded"))
        {
            adType = AdType.videorewarded;
        }
        if (impressionData.AdUnitFormat.ToLower().Contains("banner"))
        {
            adType = AdType.banner;
        }
        

        ServerAnalytics.LogAnalyticsAds(adId, SOURCE, adNetwork, adType, AdRequestType.impression, revenue!=null ? (double)revenue :0, currency);

    }

    protected void fwdSdkInitialized(string adunitid) {
        Debug.Log("InitAdsNetWork Done " + SOURCE);
        sdkInitialized = true;
        var _bannerAdUnits = new string[] { BannerID };
        var _interstitialAdUnits = new string[] { InterstitialID };
        var _rewardedVideoAdUnits = new string[] { RewardedID };
        MoPub.LoadBannerPluginsForAdUnits(_bannerAdUnits);
        MoPub.LoadInterstitialPluginsForAdUnits(_interstitialAdUnits);
        MoPub.LoadRewardedVideoPluginsForAdUnits(_rewardedVideoAdUnits);

        StartCoroutine(GenerateAdsType());
        adsListener.OnAllSdkInitalized(SOURCE);
        if (isActiveAndEnabled && OnInitialized != null)
            OnInitialized.Invoke(adunitid);
    }




    protected IEnumerator GenerateAdsType() {
        InitRewadedVideo();
        yield return new WaitForSeconds(0.2f);
        InitInterstitial();
    }

    #region Banner
    public override void InitBanner() {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false)) {
            return;
        }
        MoPub.RequestBanner(BannerID, MoPub.AdPosition.BottomCenter);
        AdsController.LogAdEvent(AdType.banner, AdRequestType.load, SOURCE);
        loadBannerCalled = true;
    }
    public override void ShowBanner(bool visible) {
        isShownBanner = visible;
        isBannerLoaded = true;
        MoPub.ShowBanner(BannerID, visible);
        AdsController.LogAdEvent(AdType.banner, AdRequestType.show, SOURCE);
    }
    #endregion

    #region Interstitial
    public override void InitInterstitial() {
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false)) {
            return;
        }
        if (isInterstitialInitialized) return;

        MoPubManager.OnInterstitialLoadedEvent += MoPubManager_OnInterstitialLoadedEvent;
        MoPubManager.OnInterstitialFailedEvent += MoPubManager_OnInterstitialFailedEvent;
        MoPubManager.OnInterstitialDismissedEvent += MoPubManager_OnInterstitialDismissedEvent;
        MoPubManager.OnInterstitialExpiredEvent += MoPubManager_OnInterstitialExpiredEvent;
        MoPubManager.OnInterstitialShownEvent += MoPubManager_OnInterstitialShownEvent;
        MoPubManager.OnInterstitialClickedEvent += MoPubManager_OnInterstitialClickedEvent;


        isInterstitialInitialized = true;

        LoadInterstitial();
    }
    private void MoPubManager_OnInterstitialClickedEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnInterstitialClickedEvent ");
    }
    private void MoPubManager_OnInterstitialExpiredEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnInterstitialExpiredEvent ");
    }

    private void MoPubManager_OnInterstitialDismissedEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnInterstitialDismissedEvent ");
        isShowInterstitial = false;
        adsListener.OnInterstitialSuccess(SOURCE);
    }
    private void MoPubManager_OnInterstitialShownEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnInterstitialShownEvent ");
        isShowInterstitial = true;
        AdsController.LogAdEvent(AdType.inter, AdRequestType.show, SOURCE);
    }
    private void MoPubManager_OnInterstitialFailedEvent(string adUnitId, string errorCode) {
        Debug.Log("MoPubManager_OnInterstitialFailedEvent " + errorCode);
        isInterstitialLoading = false;
        AdsController.LogAdEvent(AdType.inter, AdRequestType.loadfail, SOURCE);
        if (isUniqueBaseAds)
        {
            retryInterAttempt++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryInterAttempt));

            Invoke("LoadInterstitial", (float)retryDelay);
        }
    }
    private void MoPubManager_OnInterstitialLoadedEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnInterstitialLoadedEvent ");
        isInterstitialLoading = false;
        AdsController.LogAdEvent(AdType.inter, AdRequestType.loadsuccess, SOURCE);
        retryInterAttempt = 0;
    }

    public override void LoadInterstitial() {
        if (!isInterstitialInitialized) return;
        if (isShowInterstitial) return;
        if (isInterstitialLoading) return;
        if (IsInterstitialAvailable()) return;
        MoPub.RequestInterstitialAd(InterstitialID);
        isInterstitialLoading = true;

        AdsController.LogAdEvent(AdType.inter, AdRequestType.load, SOURCE);

    }

    public override bool ShowInterstitial() {
        //AdsController.LogAdEvent(AdType.inter, AdRequestType.request, SOURCE);
        if (IsInterstitialAvailable()) {
            MoPub.ShowInterstitialAd(InterstitialID);
            return true;
        } else {
            Debug.Log("Inter is Not Ready " + SOURCE);
            return false;
        }
    }

    #endregion

    #region Rewarded
    public override void InitRewadedVideo() {
        if (isRewardedVideoInitialized) return;

        MoPubManager.OnRewardedVideoLoadedEvent += MoPubManager_OnRewardedVideoLoadedEvent;
        MoPubManager.OnRewardedVideoFailedEvent += MoPubManager_OnRewardedVideoFailedEvent;
        MoPubManager.OnRewardedVideoExpiredEvent += MoPubManager_OnRewardedVideoExpiredEvent;
        MoPubManager.OnRewardedVideoShownEvent += MoPubManager_OnRewardedVideoShownEvent;
        MoPubManager.OnRewardedVideoClickedEvent += MoPubManager_OnRewardedVideoClickedEvent;
        MoPubManager.OnRewardedVideoFailedToPlayEvent += MoPubManager_OnRewardedVideoFailedToPlayEvent;
        MoPubManager.OnRewardedVideoReceivedRewardEvent += MoPubManager_OnRewardedVideoReceivedRewardEvent;
        MoPubManager.OnRewardedVideoClosedEvent += MoPubManager_OnRewardedVideoClosedEvent;
        MoPubManager.OnRewardedVideoLeavingApplicationEvent += MoPubManager_OnRewardedVideoLeavingApplicationEvent;
        isRewardedVideoInitialized = true;
        LoadRewadedVideo();
    }

    private void MoPubManager_OnRewardedVideoLeavingApplicationEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnRewardedVideoLeavingApplicationEvent ");
    }

    private void MoPubManager_OnRewardedVideoClosedEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnRewardedVideoClosedEvent ");
        isShowRewardedVideo = false;
        adsListener.OnRewardedAdsClose(SOURCE, gotRewarded);
        AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.canreachreward, SOURCE);
    }

    private void MoPubManager_OnRewardedVideoReceivedRewardEvent(string adUnitId, string label, float amount) {
        Debug.Log("MoPubManager_OnRewardedVideoReceivedRewardEvent ");
        gotRewarded = true;
    }

    private void MoPubManager_OnRewardedVideoFailedToPlayEvent(string adUnitId, string errorMsg) {
        Debug.Log("MoPubManager_OnRewardedVideoFailedToPlayEvent " + errorMsg);
        isShowRewardedVideo = false;
        isRewardedvideoLoading = false;
        adsListener.OnRewardedAdsFail(SOURCE);

        if (isUniqueBaseAds)
        {
            LoadRewadedVideo();
        }

    }

    private void MoPubManager_OnRewardedVideoClickedEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnRewardedVideoClickedEvent ");
    }

    private void MoPubManager_OnRewardedVideoShownEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnRewardedVideoShownEvent ");
        isShowRewardedVideo = true;
        AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.show, SOURCE);
    }

    private void MoPubManager_OnRewardedVideoExpiredEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnRewardedVideoExpiredEvent ");
    }

    private void MoPubManager_OnRewardedVideoFailedEvent(string adUnitId, string errorMsg) {
        Debug.Log("MoPubManager_OnRewardedVideoFailedEvent ");
        isRewardedvideoLoading = false;
        AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.loadfail, SOURCE);

        if (isUniqueBaseAds)
        {
            retryRewardedAttemp++;
            double retryDelay = Math.Pow(2, Math.Min(6, retryRewardedAttemp));
            Invoke("LoadRewadedVideo", (float)retryDelay);
        }
    }

    private void MoPubManager_OnRewardedVideoLoadedEvent(string adUnitId) {
        Debug.Log("MoPubManager_OnRewardedVideoLoadedEvent ");
        isRewardedvideoLoading = false;
        AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.loadsuccess, SOURCE);
        retryRewardedAttemp = 0;
    }

    public override void LoadRewadedVideo() {
        if (!isRewardedVideoInitialized) return;
        if (isShowRewardedVideo) return;
        if (isRewardedvideoLoading) return;
        if (IsRewardedVideoAvailable()) return;
        MoPub.RequestRewardedVideo(RewardedID);
        isRewardedvideoLoading = true;
        AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.load, SOURCE);
    }

    public override bool ShowRewadedVideo() {
        //AdsController.LogAdEvent(AdType.videorewarded, AdRequestType.request, SOURCE);
        gotRewarded = false;
        if (IsRewardedVideoAvailable()) {
            MoPub.ShowRewardedVideo(RewardedID);
            return true;
        } else {
            Debug.Log("RewadedVideo is Not Ready " + SOURCE);
            return false;
        }


    }

    #endregion

    void OnDestroy() {
        MoPubManager.OnSdkInitializedEvent -= fwdSdkInitialized;
        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false)) {
            return;
        }
        if (loadBannerCalled)
            MoPub.DestroyBanner(BannerID);
    }

#endif

    public override bool ValidateNetwork(string sourceCode) {

        return sourceCode.ToLower().Equals(SOURCE.ToLower());
    }

    public override string GetSource()
    {
        return SOURCE;
    }
}
