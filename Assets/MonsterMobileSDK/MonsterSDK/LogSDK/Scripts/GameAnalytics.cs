using AppsFlyerSDK;
#if HAS_FB
using Facebook.Unity;
#endif
using Firebase.Analytics;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
public enum CURRENCY
{
    COIN
}
public enum CURRENCY_REFERRENCE
{
    NONE,
    SPEND,
    WIN,
    SHOP,
    IAP
}

public class GameAnalytics
{
    private const string PARAM_STAGE = "stage";
    private const string PARAM_CHARACTER_LEVEL = "level";

    private const string PARAM_CHARACTER_LEVEL_UP = "level_up";

    private const string PARAM_IAP_PRODUCT_ID = "product_id";
    private const string PARAM_IAP_REVENUE = "revenue";
    private const string PARAM_IAP_CURRENCY = "currency";

    private const string PARAM_CURRENCY_CURRENCY = "currency";
    private const string PARAM_CURRENCY_VALUE = "value";
    private const string PARAM_CURRENCY_CURRENT_VALUE = "current_value";
    private const string PARAM_CURRENCY_EVENT_REF_ID = "event_ref_id";
    private const string PARAM_CURRENCY_DESCRIPTION = "description";




    public static bool isFBInitialized;

    static UnityEvent onFinishFirebaseInit = new UnityEvent();

    static bool _isFirebaseInitialized = false;
    public static bool isFirebaseInitialized
    {
        get
        {
            return _isFirebaseInitialized;
        }
        set
        {
            _isFirebaseInitialized = value;
            if (value == true)
            {
                if (onFinishFirebaseInit != null)
                {
                    onFinishFirebaseInit.Invoke();
                    onFinishFirebaseInit.RemoveAllListeners();
                }
            }
        }
    }

    public static void LogEventFirebase(string eventName, Parameter[] parameters)
    {

        var newParams = parameters.ToList();

#if !ENV_PROD
        newParams.Add(new Parameter("test", true.ToString()));
#endif
        if (isFirebaseInitialized)
        {

            FirebaseAnalytics.LogEvent(eventName, newParams.ToArray());
        }
        else
        {
            onFinishFirebaseInit.AddListener(() =>
            {
                FirebaseAnalytics.LogEvent(eventName, newParams.ToArray());
            });
        }
    }
    public static void LogEventAppsFlyer(string eventName, Dictionary<string, string> parameters)
    {
        AppsFlyer.sendEvent(eventName, parameters);
    }

#if HAS_FB
    
    public static void LogEventFacebook(string eventName, Dictionary<string, object> parameters)
    {
        if (isFBInitialized)
        {
#if !ENV_PROD
            parameters["test"] = true;
#endif

            FB.LogAppEvent(eventName, null, parameters);

        }
    }
#endif

    public static void LogFirebaseUserProperty(string userProperty, object value)
    {
#if ENV_PROD
        if (isFirebaseInitialized) {
            FirebaseAnalytics.SetUserProperty(userProperty, value.ToString());
        }
        else
        {
            onFinishFirebaseInit.AddListener(() => { FirebaseAnalytics.SetUserProperty(userProperty, value.ToString()); });
        }
#endif
    }


    public static void LogAdEvent(AdType adType, AdRequestType adRequestType, string source)
    {

        bool internetReachable = !(Application.internetReachability == NetworkReachability.NotReachable);
        var eventName = "ads_info";

        Parameter[] parameters = new Parameter[] {
        new Parameter("ad_request_type",$"{adType}_{adRequestType}"),
        new Parameter("position", AdsController.getLogPositon(adType)),
        new Parameter("internet_enable",internetReachable.ToString())
        };
        GameAnalytics.LogEventFirebase(eventName, parameters);

        Debug.Log("monster_log_ads " + source + eventName);

    }



    public static void LogAdRevenue(string adId, string sourcrMediation, string adNetwork, AdType adType, AdRequestType impression, double adRevenue, string placement, string country, string currency = "USD")
    {
        Parameter[] parameters = new Parameter[] {
        new Parameter("ad_id", adId),
        new Parameter("mediation", sourcrMediation),
        new Parameter("ad_network", adNetwork),
        new Parameter("ad_type", adType.ToString()),
        new Parameter("ad_revenue", adRevenue),
        new Parameter("placement", placement),
        new Parameter("country", country),
        new Parameter("currency", currency),
        new Parameter("position", AdsController.getLogPositon(adType))
        };

        LogEventFirebase("ads_impression", parameters);
        LogRoasAd(adRevenue, currency);
    }

    public static string FIREBASE_EVENT_REVENUE_P_USER = $"ad_impression_{RemoteConfigManager.GetString(StringConstants.RC_GAME_NAME)}";
    public const string PARAMS_REVENUE = "ad_revenue";
    public const string PREFS_REVENUE = "roas_ad_word_by_threshold";
    public static void LogRoasAd(double ad_revenue, string currency = "USD")
    {
        try
        {
            double defaultRev = 0;
            double revenue = double.Parse(PlayerPrefs.GetString(PREFS_REVENUE, defaultRev.ToString())) + ad_revenue;
            double kpi = RemoteConfigManager.GetDouble(StringConstants.RC_KPI_PER_USER);
            Debug.Log($"Total Revenue {revenue} kpi {kpi}");
            if (revenue > kpi)
            {
                List<Parameter> parameters = new List<Parameter>();
                parameters.Add(new Parameter(FirebaseAnalytics.ParameterValue, revenue));
                parameters.Add(new Parameter(FirebaseAnalytics.ParameterCurrency, currency));
                LogEventFirebase(FIREBASE_EVENT_REVENUE_P_USER, parameters.ToArray());

                Dictionary<string, string> paramDict = new Dictionary<string, string>();
                paramDict.Add("ad_revenue", revenue.ToString());
                paramDict.Add("ad_currency", currency);
                LogEventAppsFlyer(FIREBASE_EVENT_REVENUE_P_USER, paramDict);

                revenue = 0;
            }
            PlayerPrefs.SetString(PREFS_REVENUE, revenue.ToString());
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public static void LogFirebaseDirectly(string name)
    {
        if (isFirebaseInitialized)
        {
            FirebaseAnalytics.LogEvent(name);
        }
        else
        {
            onFinishFirebaseInit.AddListener(() =>
            {
                FirebaseAnalytics.LogEvent(name);
            });
        }
    }


    public static void SetUserID(string uid)
    {
        if (isFirebaseInitialized)
        {
            FirebaseAnalytics.SetUserId(MonsterPlayerPrefs.PlayerId.ToString());
            AppsFlyer.setCustomerUserId(MonsterPlayerPrefs.PlayerId.ToString());
            //SingularSDK.SetCustomUserId(MonsterPlayerPrefs.PlayerId.ToString());
        }
        else
        {
            onFinishFirebaseInit.AddListener(() =>
            {
                FirebaseAnalytics.SetUserId(MonsterPlayerPrefs.PlayerId.ToString());
                AppsFlyer.setCustomerUserId(MonsterPlayerPrefs.PlayerId.ToString());
                // SingularSDK.SetCustomUserId(MonsterPlayerPrefs.PlayerId.ToString());
            });
        }
    }
    private const string EVENT_BACK_TO_MAIN_MENU = "back_to_menu";
    public static void LogBackGamePlay(int currentLevel)
    {
        List<Parameter> parameters = new List<Parameter>();
        parameters.Add(new Parameter("level", currentLevel));
        LogEventFirebase(EVENT_BACK_TO_MAIN_MENU, parameters.ToArray());
    }

    private const string EVENT_LEVEL_UP = "level_up";
    public static void LogLevelUp(int Level)
    {
        List<Parameter> parameters = new List<Parameter>();
        parameters.Add(new Parameter(PARAM_CHARACTER_LEVEL, Level));
        LogEventFirebase(EVENT_LEVEL_UP, parameters.ToArray());
    }


    public static void LogGamePlayData(int level, GAMEPLAY_STATE gameState, object param = null, string timeProgress = null)
    {
        int levelAmount = (int)RemoteConfigManager.GetDouble(StringConstants.RC_CHECKING_LEVEL_AMOUNT);
        switch (gameState)
        {
            case GAMEPLAY_STATE.START_LEVEL:
                if (level < levelAmount && !checkLevelFirstStart(level))
                {
                    string eventname = $"stage_{level}_first_start";
                    setLevelFirstStart(level);
                    LogEventFirebase(eventname, new Parameter[]
                    {
                        // new Parameter("t1", t1)
                    });
                    Debug.Log(eventname);
                }
                LogEventFirebase("stage_start", new Parameter[] { new Parameter("level", level) });
                break;
            case GAMEPLAY_STATE.WIN:
                if (level < levelAmount && !checkLevelFirstPass(level))
                {
                    string eventname = $"stage_{level}_first_pass";
                    setLevelFirstPass(level);
                    LogEventFirebase(eventname, new Parameter[] { new Parameter("type_pass", "Win") });
                    Debug.Log(eventname + "WIN");
                }

                // if (param != null)
                LogEventFirebase("stage_end", new Parameter[]
                {
                        new Parameter("type_pass", "Win"),
                        new Parameter("level", level),
                        new Parameter("time_progress", timeProgress),



                });

                Dictionary<string, string> LevelAchievedEvent = new Dictionary<string, string>();
                LevelAchievedEvent.Add(AFInAppEvents.LEVEL, level.ToString());
                LogEventAppsFlyer(AFInAppEvents.LEVEL_ACHIEVED, LevelAchievedEvent);
                LogFirebaseUserProperty("last_level_play", level);
                break;
            case GAMEPLAY_STATE.SKIP:
                if (level < levelAmount && !checkLevelFirstPass(level))
                {
                    string eventname = $"stage_{level}_first_pass";
                    setLevelFirstPass(level);
                    LogEventFirebase(eventname, new Parameter[] { new Parameter("type_pass", "Skip") });
                    Debug.Log(eventname + "SKIP");
                }

                // if (param != null)
                LogEventFirebase("stage_end", new Parameter[]
                {
                        new Parameter("type_pass", "Skip"),
                        new Parameter("level", level),

                });
                break;
            case GAMEPLAY_STATE.LOSE:
                // if (param != null)
                LogEventFirebase("stage_end", new Parameter[]
                {
                        new Parameter("type_pass", "Lose"),
                        new Parameter("level", level),
                        new Parameter("time_progress", timeProgress),

                });
                break;
            default:
                break;
        }
    }

    private static bool checkLevelFirstStart(int level)
    {
        bool hasStart = PlayerPrefs.GetInt($"stage_{level}_first_start", 0) == 1;
        //Debug.Log($"stage_{level}_map_{currentChapter.ToLower()}_first_start_{hasStart}");
        return hasStart;
    }
    private static void setLevelFirstStart(int level)
    {
        PlayerPrefs.SetInt($"stage_{level}_first_start", 1);
    }
    private static bool checkLevelFirstPass(int level)
    {
        bool hasPass = PlayerPrefs.GetInt($"stage_{level}_first_pass", 0) == 1;
        //Debug.Log($"stage_{level}_map_{currentChapter.ToLower()}_first_pass_{hasPass}");
        return hasPass;
    }
    private static void setLevelFirstPass(int level)
    {
        PlayerPrefs.SetInt($"stage_{level}_first_pass", 1);
    }

    private const string EVENT_IAP_CLICK = "iap_package_click";
    private const string EVENT_IAP_BUY_FAIL = "iap_package_fail";
    private const string EVENT_IAP_BUY_CANCEL = "iap_package_cancel";
    private const string EVENT_IAP_BUY_SUCCESS = "iap_package_success";
    public static void LogPurchase(UnityEngine.Purchasing.Product product, IAP_STATE iapState)
    {
        try
        {
            Debug.Log("LogPurchase " + product.transactionID + " reason " + iapState.ToString());

            List<Parameter> parameters = new List<Parameter>();
            parameters.Add(new Parameter(PARAM_IAP_PRODUCT_ID, product.transactionID));
            parameters.Add(new Parameter(PARAM_IAP_REVENUE, product.metadata.localizedPrice.ToString()));
            parameters.Add(new Parameter(PARAM_IAP_CURRENCY, product.metadata.isoCurrencyCode));

            switch (iapState)
            {
                case IAP_STATE.CLICK:
                    LogEventFirebase(EVENT_IAP_CLICK, parameters.ToArray());
                    break;
                case IAP_STATE.SUCCESS:
                    LogEventFirebase(EVENT_IAP_BUY_SUCCESS, parameters.ToArray());

                    Dictionary<string, string> paramDict = new Dictionary<string, string>();
                    paramDict.Add(AFInAppEvents.REVENUE, product.metadata.localizedPrice.ToString());
                    paramDict.Add(AFInAppEvents.CURRENCY, product.metadata.isoCurrencyCode);
                    paramDict.Add(AFInAppEvents.CONTENT_ID, product.definition.id);
                    paramDict.Add(AFInAppEvents.ORDER_ID, product.transactionID);
                    paramDict.Add(AFInAppEvents.RECEIPT_ID, product.receipt);
                    LogEventAppsFlyer(AFInAppEvents.PURCHASE, paramDict);
                    break;
                case IAP_STATE.FAIL:
                    LogEventFirebase(EVENT_IAP_BUY_FAIL, parameters.ToArray());
                    break;
                case IAP_STATE.CANCEL:
                    LogEventFirebase(EVENT_IAP_BUY_CANCEL, parameters.ToArray());
                    break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }

    }
    private const string LOG_EVENT_IAP_VERIFY = "purchase_verify";
    private const string LOG_PARAM_TRANSACTION_ID = "transaction_id";
    private const string LOG_PARAM_STATUS_CODE = "error_code";
    private const string LOG_PARAM_CODE_MESSAGE = "error_message";
    public static void LogVerifyPurchase(UnityEngine.Purchasing.Product product, long statusCode, string errorMessage)
    {
        List<Parameter> firebaseParams = new List<Parameter>();
        firebaseParams.Add(new Parameter(LOG_PARAM_TRANSACTION_ID, product.transactionID));
        firebaseParams.Add(new Parameter(LOG_PARAM_STATUS_CODE, statusCode));
        firebaseParams.Add(new Parameter(LOG_PARAM_CODE_MESSAGE, errorMessage));
        LogEventFirebase(LOG_EVENT_IAP_VERIFY, firebaseParams.ToArray());
    }


    private const string RATE_US_5STARS = "rate_us_5star";
    public static void LogRateUs5Stars()
    {
        LogEventFirebase(RATE_US_5STARS, new Parameter[] { });
    }

    private const string RATE_US_SHOW = "rate_us_show";
    public static void LogRateUsShow()
    {
        LogEventFirebase(RATE_US_SHOW, new Parameter[] { });
    }

    private const string TUTORIAL_BEGIN = "tutorial_begin";
    public static void LogTutorialBegin()
    {
        LogEventFirebase(TUTORIAL_BEGIN, new Parameter[] { });
    }

    private const string TUTORIAL_COMPLETE = "tutorial_complete";
    public static void LogTutorialComplete()
    {
        LogEventFirebase(TUTORIAL_COMPLETE, new Parameter[] { });
        LogEventAppsFlyer(AFInAppEvents.TUTORIAL_COMPLETION, new Dictionary<string, string>() { });
    }

    public static void LogFail(int level, int loseCount)
    {
        List<Parameter> parameters = new List<Parameter>();
        parameters.Add(new Parameter("fail_time(s)", loseCount));
        GameAnalytics.LogEventFirebase($"stage_{level}_fail_before_first_pass", parameters.ToArray());
    }

    private const string BUTTON_CLICK = "button_click";
    public static void LogEventButton(string nameScreen, string nameButton)
    {
        List<Parameter> parameters = new List<Parameter>();
        parameters.Add(new Parameter("button_pos", $"{nameScreen}_{nameButton}"));
        GameAnalytics.LogEventFirebase(BUTTON_CLICK, parameters.ToArray());
    }
}
