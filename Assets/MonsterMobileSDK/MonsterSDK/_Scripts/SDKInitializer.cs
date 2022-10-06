
#if HAS_FB
using Facebook.Unity;
#endif
using Firebase.DynamicLinks;

using ImoSysSDK.SocialPlatforms;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterUtils;
using AppsFlyerSDK;
using UnityEngine.Events;

public enum ThirdPartySDK
{
    FB_SDK,
    FIREBASE,
    APPSFLYER,
    IMO_SERVICES,
    MONSTER_ADS_SERVICES
}

public class SDKInitializer : MonoBehaviour, IAppsFlyerConversionData
{
    private static string MEDIA_SOURCE = "media_source";
    private static string CAMPAIGN_SOURCE = "campaign_id";
    private Dictionary<ThirdPartySDK, UnityAction> sdkInitDic;
    private int currentInitSdkIndex = -1;

    public static bool isAFinitialized = false;
    [Header("Firebase Setting")]
    [SerializeField] string gameNameForEvent = string.Empty;
    [SerializeField] UnityEvent beforeInitializeRemoteConfigEvent;

    [Header("AppsFlyerSetting")]
    [SerializeField] string UWPAppID;
    [SerializeField] bool isAppsflyerDebug;
    [SerializeField] bool getConversionData;


    private bool isFirebaseInitialized;
    bool tokenSent = false;
    public static bool isObtainedDeeplinkFB
    {
        set
        {
            PlayerPrefs.SetInt("has_sent_facebook_deeplink", value ? 1 : 0);
        }
        get
        {
            return PlayerPrefs.GetInt("has_sent_facebook_deeplink", 0) == 1;
        }
    }
    public static bool isObtainedDeeplinkAF
    {
        set
        {
            PlayerPrefs.SetInt("has_sent_conversionData", value ? 1 : 0);
        }
        get
        {
            return PlayerPrefs.GetInt("has_sent_conversionData", 0) == 1;
        }
    }

    public static bool checkGetSourceDone()
    {
        return isObtainedDeeplinkAF || isObtainedDeeplinkFB;
    }

    private void Awake()
    {
        // InitializeFacebook();
    }
    private void Start()
    {
        InitializeFirebase();
        sdkInitDic = new Dictionary<ThirdPartySDK, UnityAction>();
        sdkInitDic.Add(ThirdPartySDK.APPSFLYER, InitializeAppsFlyers);
        sdkInitDic.Add(ThirdPartySDK.IMO_SERVICES, GameServices.Instance.Initialize);
        sdkInitDic.Add(ThirdPartySDK.MONSTER_ADS_SERVICES, AdsController.Instances.InitAllAdsNetwork);


        StartCoroutine(NextSdkInit());
    }
    void Update()
    {
#if UNITY_IOS
        if (!tokenSent)
        {
            byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
            if (token != null)
            {
                AppsFlyeriOS.registerUninstall(token);
                tokenSent = true;
            }
        }
#endif
    }

    private IEnumerator NextSdkInit()
    {
        yield return new WaitForSeconds(0.1f);
        foreach (var sktInit in sdkInitDic)
        {
            sktInit.Value.Invoke();
            yield return new WaitForSeconds(0.2f);
        }
        SetupSessionTimeCount();
    }


    private void InitializeAppsFlyers()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        Debug.Log("AppsFlyer Init");
#if ENV_PROD
        AppsFlyer.setIsDebug(false);
#else
        AppsFlyer.setIsDebug(isAppsflyerDebug);
#endif

#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(GameSDKSettings.appsflyerKey, UWPAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(GameSDKSettings.appsflyerKey, GameSDKSettings.appleAppId, getConversionData ? this : null);
#endif

        //******************************/

        AppsFlyer.startSDK();
        isAFinitialized = true;

#if UNITY_IOS
        UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound);
#endif

    }

    private async void InitializeFirebase()
    {

        Debug.Log("Firebase: InitializeFirebase");
        Firebase.DependencyStatus dependencyStatus = await Firebase.FirebaseApp.CheckDependenciesAsync();
        if (dependencyStatus != Firebase.DependencyStatus.Available)
        {
            _ = Firebase.FirebaseApp.FixDependenciesAsync().ContinueWith(async task =>
            {
                dependencyStatus = await Firebase.FirebaseApp.CheckDependenciesAsync();
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {

                    InitializeFirebaseComponents();
                    Debug.Log("Firebase: Initialize Success");
                }
                else
                {
                    Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
                }
            });
        }
        else
        {
            InitializeFirebaseComponents();
        }
    }

    private void InitializeFirebaseComponents()
    {
        isFirebaseInitialized = true;
        GameAnalytics.isFirebaseInitialized = true;
        RemoteConfigManager.isFirebaseInitialized = true;
        Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

        RemoteConfigManager.AddDefaultBeforeInitialize(StringConstants.RC_GAME_NAME, gameNameForEvent);
        beforeInitializeRemoteConfigEvent?.Invoke();
        RemoteConfigManager.InitializeRemoteConfig();

        DynamicLinks.DynamicLinkReceived += DynamicLinks_DynamicLinkReceived;

        Firebase.Messaging.FirebaseMessaging.TokenReceived += FirebaseMessaging_TokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += FirebaseMessaging_MessageReceived;
    }


    private void DynamicLinks_DynamicLinkReceived(object sender, ReceivedDynamicLinkEventArgs e)
    {
        Debug.Log("DynamicLinks_DynamicLinkReceived " + e.ToString());
        string dynamicLinkUrl = e.ReceivedDynamicLink.Url.OriginalString;
        if (dynamicLinkUrl != null && dynamicLinkUrl.Contains("utm_source"))
        {
            Debug.Log("This user from source " + dynamicLinkUrl);
        }

    }

    private void FirebaseMessaging_MessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {

    }

    private void FirebaseMessaging_TokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs e)
    {
#if UNITY_ANDROID
        AppsFlyer.updateServerUninstallToken(e.Token);
#endif
#if ENV_LOG
        Debug.Log("FirebaseMessaging token: " + e.Token);
#endif
    }

    private void OnHideUnity(bool isUnityShown)
    {
        if (!isUnityShown)
        {
            Time.timeScale = 0;
        }
        else
        {
            Time.timeScale = 1;
        }
    }

#if HAS_FB
    
    private void InitializeFacebook()
    {

        if (!FB.IsInitialized)
        {
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            GameAnalytics.isFBInitialized = true;
        }


    }

    private void InitCallback()
    {

        if (FB.IsInitialized)
        {
            GameAnalytics.isFBInitialized = true;
            FB.ActivateApp();
            FB.GetAppLink(FaceBookDeepLinkCallBack);
        }
        else
        {
            Debug.LogError("Failed to Initialize the Facebook SDK");
        }

    }

    private void FaceBookDeepLinkCallBack(IAppLinkResult result)
    {
    }
#endif


    // Mark AppsFlyer CallBacks
    public void onConversionDataSuccess(string conversionData)
    {
        AppsFlyer.AFLog("didReceiveConversionData", conversionData);
        if (conversionData != null)
        {
            Dictionary<string, object> conversionDataDictionary = AppsFlyer.CallbackStringToDictionary(conversionData);
            // add deferred deeplink logic here
            HandleConversionData(conversionDataDictionary);
        }
    }

    public void onConversionDataFail(string error)
    {
        AppsFlyer.AFLog("didReceiveConversionDataWithError", error);
    }

    public void onAppOpenAttribution(string attributionData)
    {
        AppsFlyer.AFLog("onAppOpenAttribution", attributionData);
        if (attributionData != null)
        {
            Dictionary<string, object> attributionDataDictionary = AppsFlyer.CallbackStringToDictionary(attributionData);
            // add direct deeplink logic here
            HandleAttributionData(attributionDataDictionary);
        }
    }

    public void onAppOpenAttributionFailure(string error)
    {
        AppsFlyer.AFLog("onAppOpenAttributionFailure", error);
    }

    private void HandleConversionData(Dictionary<string, object> conversionDataDictionary)
    {
        List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();

        foreach (KeyValuePair<string, object> pair in conversionDataDictionary)
        {
            if (pair.Key != null && pair.Value != null)
            {
                Debug.Log("conversionData " + pair.Key + " " + pair.Value);
                parameters.Add(new Firebase.Analytics.Parameter(pair.Key, pair.Value.ToString()));
            }
        }



        Debug.Log("AppsFlyers send_conversion_data ");
        GameAnalytics.LogEventFirebase("appsflyer_conversion_data", parameters.ToArray());


        if (!isObtainedDeeplinkAF && !isObtainedDeeplinkFB)
        {
            if (conversionDataDictionary.ContainsKey(CAMPAIGN_SOURCE) && conversionDataDictionary[CAMPAIGN_SOURCE] != null)
            {
                isObtainedDeeplinkAF = true;
                MonsterPlayerPrefs.CampaignSource = conversionDataDictionary[CAMPAIGN_SOURCE].ToString();

                GameAnalytics.LogFirebaseUserProperty("trafic_source", MonsterPlayerPrefs.CampaignSource);

                if (conversionDataDictionary.ContainsKey(MEDIA_SOURCE) && conversionDataDictionary[MEDIA_SOURCE] != null)
                {
                    MonsterPlayerPrefs.NetworkSource = conversionDataDictionary[MEDIA_SOURCE].ToString();
                    GameAnalytics.LogFirebaseUserProperty("network_source", MonsterPlayerPrefs.NetworkSource);
                }
            }
        }


        //SDKLogsPrefs.Source = conversionDataDictionary["source"].ToString();


    }

    private void HandleAttributionData(Dictionary<string, object> conversionDataDictionary)
    {

        List<Firebase.Analytics.Parameter> parameters = new List<Firebase.Analytics.Parameter>();

        foreach (KeyValuePair<string, object> pair in conversionDataDictionary)
        {
            if (pair.Key != null && pair.Value != null)
            {
                Debug.Log("attributionData " + pair.Key + " " + pair.Value);
                parameters.Add(new Firebase.Analytics.Parameter(pair.Key, pair.Value.ToString()));
            }

        }

        GameAnalytics.LogEventFirebase("appsflyer_attribution_data", parameters.ToArray());
        //SDKLogsPrefs.Source = conversionDataDictionary["source"].ToString();


    }

    private void SetupSessionTimeCount()
    {
        SDKLogsPrefs.SessionID++;
        if (SDKLogsPrefs.firstOpen)
        {
            SDKLogsPrefs.firstOpen = false;
            SDKLogsPrefs.firstOpenTime = UnbiasedTime.Instance.Now();
            GameAnalytics.LogEventAppsFlyer(AFInAppEvents.COMPLETE_REGISTRATION, new Dictionary<string, string>());
        }
        else
        {
            Dictionary<string, string> param = new Dictionary<string, string>();
            param.Add("session_id", SDKLogsPrefs.SessionID.ToString());
            GameAnalytics.LogEventAppsFlyer(AFInAppEvents.LOGIN, param);
        }
        var deltaTime = UnbiasedTime.Instance.Now() - SDKLogsPrefs.firstOpenTime;
        if (SDKLogsPrefs.DayFromInstall != (int)deltaTime.TotalDays)
        {
            SDKLogsPrefs.EngageDay++;
        }
        SDKLogsPrefs.DayFromInstall = (int)deltaTime.TotalDays;
        GameAnalytics.LogFirebaseUserProperty("session_id", $"ss{SDKLogsPrefs.SessionID}:dfi{SDKLogsPrefs.DayFromInstall}:d{SDKLogsPrefs.EngageDay}");

    }


}


