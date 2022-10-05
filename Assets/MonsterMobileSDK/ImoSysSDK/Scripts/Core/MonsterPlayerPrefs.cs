using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterPlayerPrefs
{
    private const string PREF_PLAYER_ID = "monster_player_id";
    private const string PREF_PLAYER_NAME = "monster_player_name";
    private const string PREF_DEVICE_ID = "monster_device_id";
    private const string PREF_ADVERTISING_ID = "monster_advertising_id";
    private const string PREF_FB_LOGGED_IN = "monster_fb_logged_in";
    private const string PREF_AVATAR_URL = "monster_avatar_url";
    private const string PREF_DYNAMIC_LINK = "monster_referral_dynamic_link";
    private const string PREF_REFERRAL_EVENT_CONFIGURED = "monster_referral_event_configured";
    private const string PREF_REFERRAL_REPORTED = "monster_referral_reported";
    private const string PREF_FIRST_OPEN = "monster_first_open";
    private const string PREF_FCM_TOKEN_REPORTED = "monster_fcm_token_reported";
    private const string PREF_FCM_TOKEN = "monster_fcm_token";
    private const string PREF_LOGIN_TOKEN = "login_token";
    private const string PREF_CAMPAIGN_SOURCE = "from_campaign_source";
    private const string PREF_NETWORK_SOURCE = "from_network_source";

    private static int playerId = -1;

    public static int PlayerId
    {
        get
        {
            if (playerId == -1)
            {
                playerId = PlayerPrefs.GetInt(PREF_PLAYER_ID, -1);
            }
            return playerId;
        }
        set
        {
            playerId = value;
            PlayerPrefs.SetInt(PREF_PLAYER_ID, value);
            AudienceManager.Instance.LoadAudienceDataFromServer();
        }
    }

    public static string DeviceId
    {
        get
        {
            return PlayerPrefs.GetString(PREF_DEVICE_ID);
        }
        set
        {
            PlayerPrefs.SetString(PREF_DEVICE_ID, value);
        }
    }

    public static string AdvertisingId
    {
        get
        {
            return PlayerPrefs.GetString(PREF_ADVERTISING_ID, null);
        }
        set
        {
            PlayerPrefs.SetString(PREF_ADVERTISING_ID, value);
        }
    }

    public static string LoginToken
    {
        get
        {
            return PlayerPrefs.GetString(PREF_LOGIN_TOKEN, string.Empty);
        }
        set
        {
            PlayerPrefs.SetString(PREF_LOGIN_TOKEN, value);
        }
    }

    public static bool FacebookLoggedIn {
        get {
            return PlayerPrefs.GetInt(PREF_FB_LOGGED_IN, 0) == 1;
        }
        set
        {
            PlayerPrefs.SetInt(PREF_FB_LOGGED_IN, value ? 1 : 0);
        }
    }

    public static string PlayerName {
        get {
            return PlayerPrefs.GetString(PREF_PLAYER_NAME, null);
        }
        set
        {
            PlayerPrefs.SetString(PREF_PLAYER_NAME, value);
        }
    }

    public static string AvatarUrl {
        get {
            return PlayerPrefs.GetString(PREF_AVATAR_URL);
        }
        set {
            PlayerPrefs.SetString(PREF_AVATAR_URL, value);
        }
    }

    public static string CampaignSource
    {
        set
        {
            SDKPlayerPrefs.SetString(PREF_CAMPAIGN_SOURCE, value);
        }
        get
        {
            return SDKPlayerPrefs.GetString(PREF_CAMPAIGN_SOURCE, "Default");
        }
    }

    public static string NetworkSource
    {
        set
        {
            SDKPlayerPrefs.SetString(PREF_NETWORK_SOURCE, value);
        }
        get
        {
            return SDKPlayerPrefs.GetString(PREF_NETWORK_SOURCE, "Default");
        }
    }

    public static string ReferralDynamicLink {
        get {
            return PlayerPrefs.GetString(PREF_DYNAMIC_LINK);
        }
        set
        {
            PlayerPrefs.SetString(PREF_DYNAMIC_LINK, value);
        }
    }

    public static bool ReferralEventConfigured { get {
            return PlayerPrefs.GetInt(PREF_REFERRAL_EVENT_CONFIGURED, 0) == 1;
        } set {
            PlayerPrefs.SetInt(PREF_REFERRAL_EVENT_CONFIGURED, value ? 1 : 0);
        }
    }

    public static bool ReferralReported {
        get {
            return PlayerPrefs.GetInt(PREF_REFERRAL_REPORTED, 0) == 1;
        }
        set {
            PlayerPrefs.SetInt(PREF_REFERRAL_REPORTED, value ? 1 : 0);
        }
    }

    public static DateTime FirstOpen {
        get {
            return GetDateTime(PREF_FIRST_OPEN, DateTime.MinValue);
        }
        set {
            SetDateTime(PREF_FIRST_OPEN, value);
        }
    }

    public static bool FcmTokenReported {
        get {
            return GetBool(PREF_FCM_TOKEN_REPORTED, false);
        }
        set {
            SetBool(PREF_FCM_TOKEN_REPORTED, value);
        }
    }

    public static string FcmToken {
        get {
            return PlayerPrefs.GetString(PREF_FCM_TOKEN);
        }
        set {
            PlayerPrefs.SetString(PREF_FCM_TOKEN, value);
        }
    }

    public static string AppsFlyerId
    {
        get
        {
            return AppsFlyerSDK.AppsFlyer.getAppsFlyerId() != null ?
                AppsFlyerSDK.AppsFlyer.getAppsFlyerId() :
                "default";
        }
    }

    private static bool GetBool(string key, bool def) {
        return PlayerPrefs.GetInt(key, def ? 1 : 0) == 1;
    }

    private static void SetBool(string key, bool value) {
        PlayerPrefs.SetInt(key, value ? 1 : 0);
    }

    public static void SetDateTime(string key, DateTime date)
    {
        PlayerPrefs.SetString(key, date.ToBinary().ToString());
    }

    public static DateTime GetDateTime(string key, DateTime defaultValue)
    {
        string @string = PlayerPrefs.GetString(key);
        DateTime result = defaultValue;
        if (!string.IsNullOrEmpty(@string))
        {
            long dateData = Convert.ToInt64(@string);
            result = DateTime.FromBinary(dateData);
        }
        return result;
    }
    
}
