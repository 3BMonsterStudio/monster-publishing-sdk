
using Firebase.RemoteConfig;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;


[System.Flags]
public enum AdStageStatus : long
{
    None = 1 << 0,
    Pass = 1 << 1,
    Lose = 1 << 2,
    Both = Pass | Lose
}

public class RemoteConfigManager
{

    public static bool isFirebaseInitialized = false;
    public static bool firebaseFectchComplete = false;
    public static Dictionary<string, object> RemoteConfigParams = null;
    public static UnityEvent OnRemoteConfigDone = new UnityEvent();
    static Dictionary<string, object> defaults = new Dictionary<string, object>();
    
    public static void AddDefaultBeforeInitialize(string key, object value)
    {
        defaults.Add(key, value);
    }

    public static void InitializeRemoteConfig()
    {
        string jsonAdsConfig = JsonUtility.ToJson(new AdsNetworksConfig());
        defaults.Add(StringConstants.RC_NETWORK_CONFIG, jsonAdsConfig);
        defaults.Add(StringConstants.RC_CHECK_INTERNET_CONNECT, true);
        defaults.Add(StringConstants.RC_ENABLE_SERVER_ANALYTICS, false);
        defaults.Add(StringConstants.RC_ENABLE_SPLASH_ADS, true);
        defaults.Add(StringConstants.RC_ENABLE_FORCE_UPDATE, true);
        defaults.Add(StringConstants.RC_CURRENT_LASTEST_VERSION, "1.0.5");
        defaults.Add(StringConstants.RC_APP_UPDATE_TYPE, "Flexible");
        defaults.Add(StringConstants.RC_VERSION_UPDATE_MESSAGE, "Please update the latest version!");
        defaults.Add(StringConstants.RC_KPI_PER_USER, 0.00001f);
        defaults.Add(StringConstants.RC_CHECKING_LEVEL_AMOUNT, 50);

        if (RemoteConfigParams != null)
        {
            foreach (KeyValuePair<string, object> remoteParams in RemoteConfigParams)
            {
                if (!defaults.ContainsKey(remoteParams.Key))
                {
                    defaults.Add(remoteParams.Key, remoteParams.Value);
                }
            }
        }

        FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
        FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero).ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                firebaseFectchComplete = true;
                Debug.Log("FirebaseFetchComplete: " + DateTime.Now.ToString());
                OnRemoteConfigDone.Invoke();
            }
        });
    }

    public static long GetLong(string key, long defaultValue)
    {
        return !isFirebaseInitialized ? defaultValue : FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
    }


    public static bool GetBool(string key, bool defaultValue)
    {
        return !isFirebaseInitialized ? defaultValue : FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
    }

    public static double GetDouble(string key, double defaultValue)
    {
        return !isFirebaseInitialized ? defaultValue : FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
    }

    public static string GetString(string key, string defaultValue)
    {
        return !isFirebaseInitialized ? defaultValue : FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
    }


    public static long GetLong(string key)
    {
        return !isFirebaseInitialized ? (long)defaults[key] : FirebaseRemoteConfig.DefaultInstance.GetValue(key).LongValue;
    }

    public static bool GetBool(string key)
    {
        return !isFirebaseInitialized ? (bool)defaults[key] : FirebaseRemoteConfig.DefaultInstance.GetValue(key).BooleanValue;
    }


    public static double GetDouble(string key)
    {
        return !isFirebaseInitialized ? (double)defaults[key] : FirebaseRemoteConfig.DefaultInstance.GetValue(key).DoubleValue;
    }


    public static string GetString(string key)
    {
        return !isFirebaseInitialized ? (string)defaults[key] : FirebaseRemoteConfig.DefaultInstance.GetValue(key).StringValue;
    }


    #region FORCE_UPDATE
    public static string lastestVersion
    {
        get
        {
            return !isFirebaseInitialized ? "1.0.5" : 
                FirebaseRemoteConfig.DefaultInstance.GetValue(StringConstants.RC_CURRENT_LASTEST_VERSION).StringValue;
        }
    }

    public static string appUpdateType
    {
        get
        {
            return !isFirebaseInitialized ? "Flexible" : 
                FirebaseRemoteConfig.DefaultInstance.GetValue(StringConstants.RC_CURRENT_LASTEST_VERSION).StringValue;
        }
    }

    public static string updateMessage
    {
        get {
            return !isFirebaseInitialized ? "Please update the latest version!" : 
                FirebaseRemoteConfig.DefaultInstance.GetValue(StringConstants.RC_VERSION_UPDATE_MESSAGE).StringValue;
        }
    }

    public static bool enableForceUpdate
    {
        get
        {
            return !isFirebaseInitialized ? false :
                FirebaseRemoteConfig.DefaultInstance.GetValue(StringConstants.RC_ENABLE_FORCE_UPDATE).BooleanValue;
        }
    }



    #endregion
}