using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_IPHONE || UNITY_IOS
using Balaso;
#endif
using Firebase.Analytics;

public enum GettingConfigMethod
{
    SERVER_MONSTER,
    REMOTE_CONFIG
}

public enum AdsNetworkIndexType
{
    WATER_FALL = 0,
    RANDOM = 1
}

[System.Serializable]
public class AdsNetworksConfig
{
    public AdsNetworkIndexType indexType = AdsNetworkIndexType.RANDOM;
    public long delayToFirstInter = 60;
    public long interInterval = 25;
    public long interDelayAfterRewarded = 20;
    public string[] positions;
    public string[] adNetworks = new string[] { "ironsource", "maxapplovin" };
    public float[] adsNetworksShowRate = new float[] { 0.5f, 0.5f };
    public string[] logNetwork = new string[] { "firebase" };
}

public class AdsController : MonoBehaviour, IAdsListener
{
    public SDKDataAssets dataSettingId;
    public static string currentInterPosition;
    public static string currentRewardPosition;
    public static bool isShowing;


    public const string SOURCE = "AdsController";

    private UnityAction onInterstitialClosed;
    private UnityAction onInterstitialShowFailed;

    private UnityAction<bool> onRewardedClosed;
    private UnityAction onRewaredShowFailed;

    public GettingConfigMethod loadConfigMode;

    private AdsNetworksConfig networkConfig;
    public List<BaseAds> baseadsList;


    public List<BaseAds> adsNetworksList;

    public bool ShowBannerAfterInit;

    private bool lastAdsShowingWasRewarded = false;
    public bool justShowAReward = false;
    public bool returnFromPauseGame = false;
    private bool hasCallsdkInitialized = false;
    private bool recallBanner = false;


    private int _adsNetIndex = 0;

    public int adsNetworkIndex
    {
        set
        {
            if (_adsNetIndex != value)
            {
                _adsNetIndex = value;
            }
            else
            {
                _adsNetIndex++;
            }

            if (_adsNetIndex < 0 || _adsNetIndex >= adsNetworksList.Count)
            {
                _adsNetIndex = 0;
            }
        }
        get { return _adsNetIndex; }
    }

    public static AdsController Instances = null;

    private void Awake()
    {
        if (Instances == null)
        {
            Instances = this;
        }
        else
        {
            Destroy(gameObject);
        }
#if UNITY_IPHONE || UNITY_IOS
        AppTrackingTransparency.RegisterAppForAdNetworkAttribution();
        AppTrackingTransparency.UpdateConversionValue(3);
#endif

#if MKT_TEST
        SDKPlayerPrefs.SetBoolean(StringConstants.REMOVE_ADS, true);
#endif
    }



    public bool IsInitialized()
    {
        return adsNetworksList.All(ads => ads.sdkInitialized);
    }
    public void InitAllAdsNetwork() //at start
    {
        dataSettingId.ParseData();
        if (hasCallsdkInitialized) return;
        hasCallsdkInitialized = true;


#if UNITY_IPHONE || UNITY_IOS
        AudienceNetwork.AdSettings.SetAdvertiserTrackingEnabled(true);
        AppTrackingTransparency.OnAuthorizationRequestDone += OnAuthorizationRequestDone;

        AppTrackingTransparency.AuthorizationStatus currentStatus = AppTrackingTransparency.TrackingAuthorizationStatus;
        Debug.Log(string.Format("Current authorization status: {0}", currentStatus.ToString()));
        if (currentStatus != AppTrackingTransparency.AuthorizationStatus.AUTHORIZED)
        {
            Debug.Log("Requesting authorization...");
            AppTrackingTransparency.RequestTrackingAuthorization();
        }
        else
        {
            ValidateNetworkConfig();
            
        }
        return;
#endif


        ValidateNetworkConfig();
    }


    public bool CheckAllSdkInitalized()
    {
        bool allSDKCheckInit = true;
        for (int i = 0; i < adsNetworksList.Count; i++)
        {
            if (!adsNetworksList[i].sdkInitialized)
            {
                allSDKCheckInit = false;
                break;
            }
        }

        //Debug.Log("Check All SDK init " + allSDKCheckInit);
        return allSDKCheckInit;
    }


#if UNITY_IPHONE || UNITY_IOS
    private void OnAuthorizationRequestDone(AppTrackingTransparency.AuthorizationStatus obj)
    {
        ValidateNetworkConfig();
        

        switch (obj)
        {
            case AppTrackingTransparency.AuthorizationStatus.NOT_DETERMINED:
                Debug.Log("AuthorizationStatus: NOT_DETERMINED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.RESTRICTED:
                Debug.Log("AuthorizationStatus: RESTRICTED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.DENIED:
                Debug.Log("AuthorizationStatus: DENIED");
                break;
            case AppTrackingTransparency.AuthorizationStatus.AUTHORIZED:
                Debug.Log("AuthorizationStatus: AUTHORIZED");
                break;
        }

        // Obtain IDFA
        Debug.Log(string.Format("IDFA: {0}", AppTrackingTransparency.IdentifierForAdvertising()));
    }
#endif

    public void ValidateNetworkConfig()
    {
        switch (loadConfigMode)
        {
            case GettingConfigMethod.SERVER_MONSTER:
                AudienceAdsManager.Instance.InitMediation((string[] mediation) =>
                {
                    // use open ads in Magic merge
                    // mediation = new[]
                    // {
                    //     MaxInstances.SOURCE,
                    //     GoogleAdsInstances.SOURCE,
                    // };
                    GenerateAdsNetWorkOrders(mediation);
                });
                break;
            case GettingConfigMethod.REMOTE_CONFIG:
            default:
                try
                {
                    string jsonData = RemoteConfigManager.GetString(
                        StringConstants.RC_NETWORK_CONFIG,
                        JsonUtility.ToJson(new AdsNetworksConfig()));
                    if (jsonData.Trim() != "")
                    {
                        networkConfig = JsonUtility.FromJson<AdsNetworksConfig>(jsonData);
                        Debug.Log("Get networkConfig from remote: " + jsonData);
                    }
                    else
                    {
                        Debug.Log("Get networkConfig from remote null ");
                        networkConfig = new AdsNetworksConfig();
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("Get networkConfig from remote fail " + e);
                    networkConfig = new AdsNetworksConfig();
                }

                GenerateAdsNetWorkOrders(networkConfig.adNetworks);
                break;
        }
    }

    public void GenerateAdsNetWorkOrders(string[] adMediation)
    {
        for (int i = 0; i < baseadsList.Count; i++)
        {
            for (int j = 0; j < adMediation.Length; j++)
            {
                if (baseadsList[i].ValidateNetwork(adMediation[j]))
                {
                    baseadsList[i].validated = true;
                    break;
                }
            }
        }

        var tempAds = new List<BaseAds>();
        for (int i = baseadsList.Count; i > 0; i--)
        {
            var ads = baseadsList[i - 1];
            if (ads.validated)
            {
                if (ads.isMainMediation)
                {
                    adsNetworksList.Add(ads);
                    baseadsList.RemoveAt(i - 1);
                }
                else
                {
                    tempAds.Add(ads);
                    baseadsList.RemoveAt(i - 1);
                }
            }
            else
            {
                //Destroy(baseadsList[i].gameObject);
                //baseadsList[i] = null;
            }
        }

        if (tempAds.Count > 0)
        {
            foreach (var ads in tempAds)
            {
                adsNetworksList.Add(ads);
            }
        }

        if (adsNetworksList.Count == 0)
        {
            if (baseadsList.Count == 0)
            {
                GameObject emptyAds = new GameObject("emptyads");
                emptyAds.AddComponent<BaseAds>();
                adsNetworksList.Add(Instantiate<BaseAds>(emptyAds.GetComponent<BaseAds>(), transform));
            }
            else
            {
                if (!baseadsList[0].validated)
                {
                    baseadsList[0].validated = true;
                }

                adsNetworksList.Add(baseadsList[0]);
            }
        }
        else
        {
            if (baseadsList.Count > 0)
            {
                if (!baseadsList[0].validated)
                {
                    baseadsList[0].validated = true;
                }

                adsNetworksList.Add(baseadsList[0]);
            }
        }

        for (int i = 0; i < adsNetworksList.Count; i++)
        {
            adsNetworksList[i].InitAdsNetWork(this);
            AdLog.ActivateAdList(adsNetworksList[i].GetSource(), i);
        }

        if (adsNetworksList.Count == 1)
        {
            adsNetworksList[0].SetUniqueAds(true);
        }

        Debug.Log("InitAllAdsNetwork " + adsNetworksList.Count);
    }


    public void GetNewAdsIndex()
    {
        switch (networkConfig.indexType)
        {
            case AdsNetworkIndexType.WATER_FALL:
            default:
                adsNetworkIndex++;
                break;
            case AdsNetworkIndexType.RANDOM:
                adsNetworkIndex = UnityEngine.Random.Range(0, networkConfig.adNetworks.Length);
                break;
        }

        Debug.Log("GetNewAdsIndex " + adsNetworkIndex);
    }

    public bool CanShowInterFromFirstOpen
    {
        get
        {
            if (SDKPlayerPrefs.GetBoolean(StringConstants.PREF_CAN_SHOW_INTER_FROM_FIST_OPEN, false))
            {
                DateTime firstOpenTime = SDKLogsPrefs.firstOpenTime;
                double delta = (UnbiasedTime.Instance.Now() - firstOpenTime).TotalSeconds;
                if (delta < 0)
                {
                    delta = networkConfig.delayToFirstInter + 1;
                }

                if (delta > networkConfig.delayToFirstInter)
                {
                    SDKPlayerPrefs.SetBoolean(StringConstants.PREF_CAN_SHOW_INTER_FROM_FIST_OPEN, true);
                    return true;
                }
                else
                {
                    SDKPlayerPrefs.SetBoolean(StringConstants.PREF_CAN_SHOW_INTER_FROM_FIST_OPEN, false);
                    return false;
                }
            }

            return true;
        }
    }

    public bool ShouldShowInterstitial
    {
        get
        {
            long intervalSeconds;
            DateTime lastShown;
            double delta;

            if (lastAdsShowingWasRewarded)
            {
                lastAdsShowingWasRewarded = false;
                intervalSeconds = networkConfig.interDelayAfterRewarded;
                lastShown = SDKPlayerPrefs.GetDateTime(StringConstants.PREF_REWARDED_LAST_SHOWN,
                    UnbiasedTime.Instance.Now().Subtract(TimeSpan.FromDays(1)));
                delta = (UnbiasedTime.Instance.Now() - lastShown).TotalSeconds;
                if (delta < 0)
                {
                    SDKPlayerPrefs.SetDateTime(StringConstants.PREF_REWARDED_LAST_SHOWN,
                        UnbiasedTime.Instance.Now().AddDays(-1));
                    delta = intervalSeconds + 1;
                }
            }
            else
            {
                if (AudienceAdsManager.Instance.CheckCanShowPauseAds())
                {
                    if (returnFromPauseGame)
                    {
                        returnFromPauseGame = false;
                        return true;
                    }
                }

                intervalSeconds = networkConfig.interInterval;
                lastShown = SDKPlayerPrefs.GetDateTime(StringConstants.PREF_INTERSTITIAL_LAST_SHOWN,
                    UnbiasedTime.Instance.Now().Subtract(TimeSpan.FromDays(1)));
                delta = (UnbiasedTime.Instance.Now() - lastShown).TotalSeconds;
                if (delta < 0)
                {
                    SDKPlayerPrefs.SetDateTime(StringConstants.PREF_INTERSTITIAL_LAST_SHOWN,
                        UnbiasedTime.Instance.Now().AddDays(-1));
                    delta = intervalSeconds + 1;
                }
            }

            return (delta > intervalSeconds);
        }
    }


    public bool CanShowInterstitial(int id)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Check internet connection!");
            return false;
        }

        switch (loadConfigMode)
        {
            case GettingConfigMethod.SERVER_MONSTER:
                return AudienceAdsManager.Instance.CheckShowInterstitial(id);
            case GettingConfigMethod.REMOTE_CONFIG:
            default:
                return CanShowInterFromFirstOpen && ShouldShowInterstitial;
        }
    }


    public bool IsInterstitialAvailable
    {
        get
        {
            bool res = false;
            for (int i = 0; i < adsNetworksList.Count; i++)
            {
                if (adsNetworksList[i].IsInterstitialAvailable())
                {
                    res = true;
                    break;
                }
            }

            return res;
        }
    }

    public bool IsRewardedVideoAvailable
    {
        get
        {
            bool res = false;
            for (int i = 0; i < adsNetworksList.Count; i++)
            {
                if (adsNetworksList[i].IsRewardedVideoAvailable())
                {
                    res = true;
                    break;
                }
            }

            return res;
        }
    }


    public void ShowInterstitial(UnityAction interstitialClosedAction,
        int positionId)
    {
        ShowInterstitial(interstitialClosedAction,
            interstitialClosedAction,
            positionId);
    }


    public void ShowInterstitial(UnityAction interstitialShowFailedAction,
        UnityAction interstitialClosedAction,
        int positionType)
    {
        string positionName = AudienceAdsManager.Instance.GetPositionName(positionType);
#if AppBack_Checker
        if (AppBackChecker.State != AppBackgroundState.None && (positionType == InterstitialPositionType.AppBack || positionType == InterstitialPositionType.InGame ||positionType == InterstitialPositionType.Back))
        {
            Debug.Log($"Show inter at {positionType} but {AppBackChecker.State}");
            return;
        }
#endif

        if (isShowing)
        {
            return;
        }
        Debug.Log("Show Inter at " + positionType.ToString() + " adsList: " + adsNetworksList.Count);
        currentInterPosition = positionName;
        this.onRewardedClosed = null;
        this.onRewaredShowFailed = null;
        this.onInterstitialClosed = interstitialClosedAction;
        this.onInterstitialShowFailed = interstitialShowFailedAction;
        SetupBeforeAds();
        // if (adsNetworkslList[0].HasAdsShowing)
        // {
        //     Debug.Log($"inter ads showing");
        //     // SDKPlayerPrefs.SetBoolean(StringConstants.SHOWING_ADS, true);
        //     return;
        // }

        if (SDKPlayerPrefs.GetBoolean(StringConstants.REMOVE_ADS, false))
        {
            OnInterstitialSuccess(SOURCE);
            return;
        }


#if UNITY_EDITOR
        OnInterstitialSuccess(SOURCE);
        return;
#endif
        if (CanShowInterstitial(positionType))
        {
#if !NO_FIREBASE
            GameAnalytics.LogAdEvent(AdType.inter, AdRequestType.request, SOURCE);
#endif
            if (!adsNetworksList[0].ShowInterstitial())
            {
#if AppBack_Checker

                AppBackChecker.State = AppBackgroundState.AdsShowing;
#endif
                adsNetworksList[0].LoadInterstitial();

                if (adsNetworksList.Count > 1)
                {
                    if (!adsNetworksList[1].ShowInterstitial())
                    {
                        adsNetworksList[1].LoadInterstitial();
                        OnInterstitialFail(SOURCE + " Seconds Ads not available");
                    }
                    else
                    {
                        adsNetworkIndex = 1;
                    }
                }
                else
                {
                    OnInterstitialFail(SOURCE + " Has only one ads networks and it is not available");
                }
            }
            else
            {
                adsNetworkIndex = 0;
            }

            return;
        }
        else
        {
            OnInterstitialFail(SOURCE + " Not met audience requirement " + positionType);
        }
    }

    public void ShowRewardedVideo(UnityAction<bool> rewardedClosedAction,
        int rewardedPositionId,
        string rewaredPositionName)
    {
        ShowRewardedVideo(() => { },
        rewardedClosedAction,
        rewardedPositionId,
        rewaredPositionName);
    }


    public void ShowRewardedVideo(UnityAction rewardedShowFailedAction,
    UnityAction<bool> rewardedClosedAction,
    int rewardedPositionId,
    string rewardedPositionName)
    {
        if (isShowing)
        {
            return;
        }
#if AppBack_Checker
                AppBackChecker.State = AppBackgroundState.AdsShowing;
#endif
        Debug.Log("Show reward at " + rewardedPositionId + " adsList: " + adsNetworksList.Count);
        currentRewardPosition = rewardedPositionName;
        this.onInterstitialClosed = null;
        this.onInterstitialShowFailed = null;
        this.onRewaredShowFailed = rewardedShowFailedAction;
        this.onRewardedClosed = rewardedClosedAction;
        justShowAReward = true;
        SetupBeforeAds();

#if UNITY_EDITOR
        OnRewardedAdsClose(SOURCE, true);
        return;
#endif
#if !NO_FIREBASE
        GameAnalytics.LogAdEvent(AdType.videorewarded, AdRequestType.request, SOURCE);
#endif
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Check internet connection!");
            OnRewardedAdsFail($"{SOURCE}_Check internet connection!");
            return;
        }

        if (!adsNetworksList[0].ShowRewardedVideo())
        {

            adsNetworksList[0].LoadRewardedVideo();
            if (adsNetworksList.Count > 1)
            {
                if (!adsNetworksList[1].ShowRewardedVideo())
                {
                    adsNetworksList[1].LoadRewardedVideo();
                    OnRewardedAdsFail($"{SOURCE}_Second Network No Ad");
                }
                else
                {
                    adsNetworkIndex = 1;
                }
            }
            else
            {
                OnRewardedAdsFail($"{SOURCE}_First Network No Ad");
            }
        }
        else
        {
            adsNetworkIndex = 0;
        }
    }


    public void OnInterstitialSuccess(string source)
    {
        Debug.Log("OnInterstitialSuccess " + source);
        GameAnalytics.LogFirebaseUserProperty("last_ad_show", currentInterPosition);
        //adsNetworkslList[adsNetworkIndex].LoadInterstitial();
        InvokeEventLater(() =>
        {
            AdLog.DisplayToast($"{source} Interstitial Close Success");
            SetupAfterAds();
            onInterstitialClosed?.Invoke();
        });

        SetLastTimeWatchInters();
    }


    public void OnInterstitialFail(string error)
    {
        Debug.Log("OnInterstitialFail " + error);

        InvokeEventLater(() =>
        {
            AdLog.DisplayToast(error);
            SetupAfterAds();
            onInterstitialShowFailed?.Invoke();
        });
    }

    public void OnRewardedAdsClose(string source, bool gotRewarded)
    {
        Debug.Log("OnRewardedAdsClose " + source + " gotRewarded " + gotRewarded);
        GameAnalytics.LogFirebaseUserProperty("last_ad_show", currentRewardPosition);
        lastAdsShowingWasRewarded = true;
        //adsNetworkslList[adsNetworkIndex].LoadRewadedVideo();
        InvokeEventLater(() =>
        {
            AdLog.DisplayToast($"{source} gotRewarded {gotRewarded}");
            SetupAfterAds();
            onRewardedClosed?.Invoke(gotRewarded);
        });

        SetLastTimeWatchRewarded();
    }

    public void OnRewardedAdsFail(string error)
    {
        Debug.Log("OnRewardedAdsFail " + error);

        InvokeEventLater(() =>
        {
            AdLog.DisplayToast(error);
            SetupAfterAds();
            onRewaredShowFailed?.Invoke();
        });
    }

    public void OnInterstitialLoadFail(string source)
    {
        Debug.Log("OnInterstitialLoadFail " + source);
        //adsNetworkslList[adsNetworkIndex].LoadInterstitial();
    }

    public void OnRewardedAdsLoadFail(string source)
    {
        Debug.Log("OnRewardedAdsLoadFail " + source);
        //adsNetworkslList[adsNetworkIndex].LoadRewadedVideo();
    }

    private async void InvokeEventLater(UnityAction unityAction)
    {
        //StartCoroutine(DelayInvokeAction(unityAction, 0.05f));
        await Task.Delay(50);
        // UnityMainThreadDispatcher.Instance().Enqueue(() => { unityAction?.Invoke(); });
        unityAction?.Invoke();
    }

    IEnumerator DelayInvokeAction(UnityAction unityAction, float s)
    {
        yield return new WaitForSeconds(s);
        // UnityMainThreadDispatcher.Instance().Enqueue(() => { unityAction?.Invoke(); });
        unityAction?.Invoke();

    }

    public void OnBannerLoadFail(string source)
    {
        if (!recallBanner)
        {
            recallBanner = true;
            if (adsNetworksList.Count > 1)
                CallBanner(adsNetworksList[1].GetSource());
        }
    }

    public void OnAllSdkInitalized(string source)
    {
        InitBanner();
    }

    public void InitBanner()
    {
        if (CheckAllSdkInitalized())
        {
            if (adsNetworksList.Count > 0)
                CallBanner(adsNetworksList[0].GetSource());
        }
    }

    public void CallBanner(string source)
    {
        BaseAds instances = GetBaseAdsInstance(source);
        if (instances != null)
        {
            instances.InitBanner(ShowBannerAfterInit);
        }
    }

    public bool IsShowBanner
    {
        get
        {
            bool res = false;
            for (int i = 0; i < adsNetworksList.Count; i++)
            {
                if (adsNetworksList[i].isBannerLoaded)
                {
                    res = true;
                    break;
                }
            }

            return res;
        }
    }

    public void ShowBanner(bool visible)
    {
        for (int i = 0; i < adsNetworksList.Count; i++)
        {
            Debug.Log($"Hai isShow {adsNetworksList[i].isBannerLoaded}");
            adsNetworksList[i].ShowBanner(visible);
        }
    }

    public void TurnOffBanner()
    {
        if (IsShowBanner) ShowBanner(false);
    }


    public BaseAds GetBaseAdsInstance(string source)
    {
        BaseAds res = null;
        for (int i = 0; i < adsNetworksList.Count; i++)
        {
            if (adsNetworksList[i].ValidateNetwork(source))
            {
                res = adsNetworksList[i];
                break;
            }
        }

        return res;
    }


    public static string getLogPositon(AdType adType)
    {
        switch (adType)
        {
            case AdType.videorewarded:
                return currentRewardPosition.ToString();
            case AdType.inter:
                return currentInterPosition.ToString();
            case AdType.banner:
                break;
            case AdType.native:
                break;
        }

        return "others";
    }

    public void SetLastTimeWatchInters()
    {
        switch (loadConfigMode)
        {
            case GettingConfigMethod.SERVER_MONSTER:
                AudienceAdsManager.Instance.lastTimeWatchInterstitial = UnbiasedTime.Instance.Now().ToJavaTimeStamp();
                break;
            case GettingConfigMethod.REMOTE_CONFIG:
            default:
                SDKPlayerPrefs.SetDateTime(StringConstants.PREF_INTERSTITIAL_LAST_SHOWN, UnbiasedTime.Instance.Now());
                break;
        }
    }

    public void SetLastTimeWatchRewarded()
    {
        switch (loadConfigMode)
        {
            case GettingConfigMethod.SERVER_MONSTER:
                AudienceAdsManager.Instance.lastTimeWatchReward = UnbiasedTime.Instance.Now().ToJavaTimeStamp();
                break;
            case GettingConfigMethod.REMOTE_CONFIG:
            default:
                SDKPlayerPrefs.SetDateTime(StringConstants.PREF_REWARDED_LAST_SHOWN, UnbiasedTime.Instance.Now());
                break;
        }
    }

    private void SetupBeforeAds()
    {
        isShowing = true;
    }

    private void SetupAfterAds()
    {
        isShowing = false;
    }

#if HAS_ADS_MANAGER
    public void ShowOpenAds()
    {
        adsNetworksList[1].ShowFirstOpenAds();
    }
#endif

    public static IEnumerator StartAction(UnityAction action, float timeDelay)
    {
        yield return new WaitForSeconds(timeDelay);
        action.Invoke();
    }

    public static IEnumerator StartActionAtEndOfFrame(UnityAction action)
    {
        yield return new WaitForEndOfFrame();
        action.Invoke();
    }

    public static IEnumerator StartAction(UnityAction action, System.Func<bool> condition)
    {
        yield return new WaitUntil(condition);
        action.Invoke();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            returnFromPauseGame = true;
        }
    }
}