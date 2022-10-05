
using ImoSysSDK.Network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;

public enum IAP_STATE
{
    CLICK, SUCCESS, FAIL, CANCEL
}
public enum GAMEPLAY_STATE
{
    WIN, LOSE, START_LEVEL, SKIP
}


public class ServerAnalytics : SingletonObject<ServerAnalytics>
{
    //public static int GAMEPLAY_EVENT_SEND_THRESHOLD = 5;
    //public static int GAMEPLAY_IAP_EVENT_SEND_THRESHOLD = 8;
    //public static int event_count = 0;
    //public static int event_iap_count = 0;

    //public static int VERSION = 1;
    //public const string EVENT_LIST = "events";
    //// Header
    ////public const string GAME_ID = "gameId";
    ////public const string APP_ID = "appId";
    //public const string USER_ID = "userId";
    //public const string SESSION_ID = "sessionId";
    //public const string PLATFORM = "platform";
    //public const string PACKAGE_BUNDLE = "packageBundle";
    //public const string APP_VERSION = "appVersion";
    //public const string EVENT_AT = "eventAt";

    ////public const string ENGAGE_DAY = "engage_day";
    ////public const string DAY_FROM_INSTALL = "day_from_install";
    ////public const string SOURCE = "camp_source";

    ////Log Ads
    //public const string AD_ID = "adId";
    //public const string AD_PLATFORM = "adPlatform";
    //public const string AD_NEWWORK = "adNetwork";
    //public const string AD_TYPE = "adType";
    //public const string STATE = "state";
    //public const string REVENUE_ADS = "revenue";
    //public const string CURRENCY_ADS = "currency";

    //// Log IAP
    //public const string PRODUCT_ID = "productId";
    //public const string REVENUE_IAP = "revenue";
    //public const string CURRENCY_IAP = "currency";
    //public const string STATE_PURCHASE = "state";

    //// Log Level
    //public const string DATA_GAMEPLAY = "data";

    //static string LOG_SERVICE_PATH = Application.persistentDataPath + "/gol";

    //static string adLogPath = "/ADgol.gol";
    //static string iapLogPath = "/IAPgol.gol";
    //static string gameDataLogPath = "/GDgol.gol";

    //static JSONArray AnalyticsAds = null, AnalyticsIAP = null, AnalyticsLevel = null;

    //static bool _isGameServicesInitialized = false;
    public static bool isGameServicesInitialized;
    //{
    //    get
    //    {
    //        return _isGameServicesInitialized;
    //    }
    //    set
    //    {
    //        _isGameServicesInitialized = value;
    //        if (value == true)
    //        {
    //            InitAnalyticsAds();
    //            foreach (JSONObject adLog in AnalyticsAds)
    //            {
    //                adLog[USER_ID] = MonsterPlayerPrefs.PlayerId;
    //            }

    //            InitAnalyticsIAP();
    //            foreach (JSONObject IAPLog in AnalyticsIAP)
    //            {
    //                IAPLog[USER_ID] = MonsterPlayerPrefs.PlayerId;
    //            }

    //            InitAnalyticsGameplay();
    //            foreach (JSONObject LevelLog in AnalyticsLevel)
    //            {
    //                LevelLog[USER_ID] = MonsterPlayerPrefs.PlayerId;
    //            }

    //        }
    //    }
    //}

    

    //public static void InitAnalyticsAds()
    //{
    //    if (AnalyticsAds == null)
    //    {
    //        string analyticsAds = ReadStringFromFile(adLogPath).Trim();
    //        if (analyticsAds == "" || analyticsAds == null)
    //        {
    //            AnalyticsAds = new JSONArray();
    //        }
    //        else
    //        {
    //            AnalyticsAds = JSON.Parse(analyticsAds).AsArray;
    //        }

    //    }
    //}
    //public static void InitAnalyticsIAP()
    //{
    //    if (AnalyticsIAP == null)
    //    {
    //        string analyticsIAP = ReadStringFromFile(iapLogPath).Trim();
    //        if (analyticsIAP == "" || analyticsIAP == null)
    //        {
    //            AnalyticsIAP = new JSONArray();
    //        }
    //        else
    //        {
    //            AnalyticsIAP = JSON.Parse(analyticsIAP).AsArray;
    //        }

    //    }
    //}
    //public static void InitAnalyticsGameplay()
    //{
    //    if (AnalyticsLevel == null)
    //    {
    //        string analyticsLevel = ReadStringFromFile(gameDataLogPath).Trim();
    //        if (analyticsLevel != null && !analyticsLevel.Equals(""))
    //        {
    //            AnalyticsLevel = JSON.Parse(analyticsLevel.Trim()).AsArray;
    //        }
    //        else
    //        {
    //            AnalyticsLevel = new JSONArray();
    //        }

    //    }
    //}
    //static JSONObject GetHeader()
    //{

    //    JSONObject eventContent = new JSONObject();
    //    eventContent[USER_ID] = MonsterPlayerPrefs.PlayerId;
    //    eventContent[SESSION_ID] = SDKLogsPrefs.SessionID;
    //    eventContent[APP_VERSION] = Application.version.ToString();
    //    eventContent[EVENT_AT] = GetCurrentMilisSec();

    //    return eventContent;
    //}
    //public static async UniTask LogAnalyticsAds(string adID, string adPlatform, string adNetwork, AdType adType, AdRequestType adRequestType, double revenue, string currency)
    //{
        
    //    InitAnalyticsAds();


    //    JSONObject eventContent = GetHeader();

    //    eventContent[AD_ID] = adID;
    //    eventContent[AD_PLATFORM] = adPlatform;
    //    eventContent[AD_NEWWORK] = adNetwork;
    //    eventContent[AD_TYPE] = adType.ToString();
    //    eventContent[STATE] = adRequestType.ToString();
    //    eventContent[REVENUE_ADS] = revenue;
    //    eventContent[CURRENCY_ADS] = currency;

    //    //Debug.Log("LogAnalyticsAds " + eventContent.ToString());
        
    //    AnalyticsAds.Add(eventContent);
        
    //    await WriteStringToFile(adLogPath, AnalyticsAds.ToString());

    //}


    //public static async UniTask LogAnalyticsIAP(string productID, decimal revenue, string currency, IAP_STATE statePurchase)
    //{
        
    //    InitAnalyticsIAP();

    //    JSONObject eventContent = GetHeader();
    //    eventContent[PRODUCT_ID] = productID;
    //    eventContent[STATE_PURCHASE] = statePurchase.ToString();
    //    eventContent[REVENUE_IAP] = revenue.ToString();
    //    eventContent[CURRENCY_IAP] = currency;



    //    //Debug.Log("LogAnalyticsIAP " + eventContent.ToString());
    //    AnalyticsIAP.Add(eventContent);

    //    await WriteStringToFile(iapLogPath, AnalyticsIAP.ToString());
    //    await UniTask.SwitchToMainThread();

    //    event_iap_count++;
    //    if (event_iap_count >= GAMEPLAY_IAP_EVENT_SEND_THRESHOLD)
    //    {
    //        event_iap_count = 0;
    //        await UniTask.WaitForEndOfFrame();
    //        SendAnalyticsIAPRequest();
    //    }


    //}


    //public static async UniTask LogAnalyticsGameplay(DataGameplay dataGameplay, bool forceSend = false)
    //{
        
    //    InitAnalyticsGameplay();

    //    JSONObject eventContent = GetHeader();
    //    eventContent[DATA_GAMEPLAY] = dataGameplay.toJSON();
    //    //Debug.Log("LogAnalyticsGameplay " + eventContent.ToString());

    //    AnalyticsLevel.Add(eventContent);

        

    //    await WriteStringToFile(gameDataLogPath, AnalyticsLevel.ToString());
    //    await UniTask.SwitchToMainThread();
        
    //    event_count++;
    //    if(event_count >= GAMEPLAY_EVENT_SEND_THRESHOLD || forceSend)
    //    {
    //        event_count = 0;
    //        await UniTask.WaitForEndOfFrame();
    //        SendAnalyticsGameplayRequest();
    //        await UniTask.WaitForEndOfFrame();
    //        SendAnalyticsAdsRequest();
    //    }
    //}

    //public static void SendAnalyticsAdsRequest()
    //{

    //    if (!isGameServicesInitialized)
    //    {
    //        return;
    //    }
    //    if (!RemoteConfigManager.GetBool(StringConstants.RC_ENABLE_SERVER_ANALYTICS, false))
    //    {
    //        return;
    //    }
    //    if (AnalyticsAds != null)
    //    {
    //        JSONObject body = new JSONObject();
    //        body[EVENT_LIST] = AnalyticsAds;
    //        //Debug.Log("SendAnalyticsAdsRequest " + body.ToString());
    //        RestClient.SendPostRequest("/v4/eventlog/ad", body.ToString(), (statusCode, message, data) =>
    //        {
    //            Debug.Log("SendAnalyticsAdsRequest responce " + statusCode);
    //            if (statusCode == 200)
    //            {
    //                Debug.Log("SendAnalyticsAdsRequest Succes " + message + " " + data);

    //                AnalyticsAds = null;
    //                WriteStringToFile(adLogPath, "");
    //            }
    //        });
    //    }

    //}

    //public static void SendAnalyticsIAPRequest()
    //{
    //    if (!isGameServicesInitialized)
    //    {
    //        return;
    //    }
    //    if(!RemoteConfigManager.GetBool(StringConstants.RC_ENABLE_SERVER_ANALYTICS, false))
    //    {
    //        return;
    //    }
    //    if (AnalyticsIAP != null)
    //    {
    //        JSONObject body = new JSONObject();
    //        body[EVENT_LIST] = AnalyticsIAP;
    //        //Debug.Log("SendAnalyticsIAPRequest " + body.ToString());
    //        RestClient.SendPostRequest("/v4/eventlog/iap", body.ToString(), (statusCode, message, data) =>
    //        {
    //            Debug.Log("SendAnalyticsIAPRequest responce " + statusCode); 
    //            if (statusCode == 200)
    //            {
    //                Debug.Log("SendAnalyticsIAPRequest Succes " + message + " " + data);

    //                AnalyticsIAP = null;
    //                WriteStringToFile(iapLogPath, "");
    //            }
    //        });
    //    }

    //}

    //public static void SendAnalyticsGameplayRequest()
    //{
    //    if (!isGameServicesInitialized)
    //    {
    //        return;
    //    }
    //    if (!RemoteConfigManager.GetBool(StringConstants.RC_ENABLE_SERVER_ANALYTICS, false))
    //    {
    //        return;
    //    }
    //    if (AnalyticsLevel != null)
    //    {
    //        JSONObject body = new JSONObject();
    //        body[EVENT_LIST] = AnalyticsLevel;
    //       //Debug.Log("SendAnalyticsGameplayRequest " + body.ToString());
    //        RestClient.SendPostRequest("/v4/eventlog/level", body.ToString(), (statusCode, message, data) =>
    //        {
    //            Debug.Log("SendAnalyticsGameplayRequest responce " + statusCode);
    //            if (statusCode == 200)
    //            {
    //                Debug.Log("SendAnalyticsGameplayRequest Succes " + message + " " + data);

    //                AnalyticsLevel = null;
    //                WriteStringToFile(gameDataLogPath, "");
    //            }
    //        });
    //    }
    //}


    //static async UniTask WriteStringToFile(string path, string content)
    //{
    //    await UniTask.SwitchToThreadPool();
    //    string filePath = LOG_SERVICE_PATH + path;
    //    try
    //    {
    //        if (!Directory.Exists(LOG_SERVICE_PATH))
    //        {
    //            Directory.CreateDirectory(LOG_SERVICE_PATH);
    //        }
    //        StreamWriter writer = new StreamWriter(filePath, false);
    //        writer.WriteLine(content);
    //        writer.Close();
    //        Debug.Log("WriteStringToFile " + filePath + " " + content);
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError("WriteStringToFile " + filePath + " " + ex);
    //    }
    //}

    //static string ReadStringFromFile(string path)
    //{
    //    string filePath = LOG_SERVICE_PATH + path;
    //    try
    //    {
    //        if (!Directory.Exists(LOG_SERVICE_PATH))
    //        {
    //            Directory.CreateDirectory(LOG_SERVICE_PATH);
    //        }
    //        string content = "";
    //        if (File.Exists(filePath))
    //        {
    //            StreamReader reader = new StreamReader(filePath);
    //            content = reader.ReadToEnd();
    //            reader.Close();
    //        }
    //        Debug.Log("ReadStringFromFile " + filePath + " " + content);
    //        return content;
    //    }
    //    catch (System.Exception ex)
    //    {
    //        Debug.LogError("ReadStringFromFile " + filePath + " " + ex);
    //        return "";
    //    }
    //}

    //public static double GetCurrentMilisSec()
    //{
    //    DateTime dt1970 = new DateTime(1970, 1, 1);
    //    DateTime current = DateTime.UtcNow;
    //    TimeSpan span = current - dt1970;
    //    return span.TotalMilliseconds;
    //}

}
