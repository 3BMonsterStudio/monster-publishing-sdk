using UnityEngine;
using System.Collections;
using System;
using UnityEngine.Events;
using ImoSysSDK.Core;
using SimpleJSON;
using ImoSysSDK.Network;
using System.Collections.Generic;
using System.Text;

using Firebase.DynamicLinks;
using System.Security.Cryptography;
using Newtonsoft.Json.Linq;

namespace ImoSysSDK.SocialPlatforms {

    [System.Serializable]
    public class DynamicLinkInfo {
        public int referralPlayerId;
        public int referralEventId;
    }

    [System.Serializable]
    public class AuthInfoBody {
        public string authSource;
        public string authId;
        public string authToken;
        public string name;
        public int playerId;
        public string deviceId;
        public string advertisingId;
    }

    public class GameServices : Singleton<GameServices> {

        private static readonly object instanceLock = new object();

        private const string SALT = "2few393f";

        private bool isReportingPlayer;
        private bool isReferralInfoFetched;
        private int unclaimedReferralCount;

        #region Referral event

        private int referralEventId;
        private string appleStoreId;
        private string referralDomain;
        private string baseDomain;
        private string iosMinimumVersion;
        private int androidMinimumVersion;
        #endregion

        private bool isFirstOpen;

        public UnityEvent UnclaimedReferralCountChanged = new UnityEvent();
        public UnityEvent PendingDynamicLinkRewardChanged = new UnityEvent();
        private bool pendingDynamicLinkReward;
        private bool claimingReferralReward;
        private bool reportingReferral;

        public static bool initialized;
        private bool initializing;

        public static class LeaderboardTypes {
            public const string LifeTime = "lifetime";
            public const string Periodically = "periodically";
            public const string Monthly = "monthly";
        }

        private GameServices() {

        }

        public void Initialize() {
            if (initialized || initializing) {
                return;
            }
            if (MonsterPlayerPrefs.FirstOpen == DateTime.MinValue) {
                isFirstOpen = true;
                MonsterPlayerPrefs.FirstOpen = UnbiasedTime.Instance.Now();
            }
            initializing = true;
            if (MonsterPlayerPrefs.PlayerId == -1) {
                ReportNewPlayer();
            } else {
                AudienceManager.Instance.LoadAudienceDataFromServer();
                //DoAfterInitialization();
                ServerAnalytics.isGameServicesInitialized = true;
                initializing = false;
                initialized = true;
            }
        }

        public bool IsInitialized {
            get {
                return initialized;
            }
        }

        public bool IsInitializing {
            get {
                return initializing;
            }
        }


        public void ReportNewPlayer()
        {
            if (isReportingPlayer)
            {
                return;
            }
            isReportingPlayer = true;
            bool advertisingSupported = Application.RequestAdvertisingIdentifierAsync((adId, trackingEnabled, errorMsg) => {
                MonsterPlayerPrefs.AdvertisingId = adId;
#if !ENV_PROD
                Debug.Log("advertising id = " + adId);
#endif

                ReportNewPlayerToServer();
            });
            if (!advertisingSupported)
            {
                ReportNewPlayerToServer();
            }
        }
          
        private void ReportNewPlayerToServer()
        {
            ReportPlayerTask task = new ReportPlayerTask((success, playerId, deviceId, name) => {
                Debug.Log("success " + success + "name " + name);
                if (success)
                {
                    Debug.Log("PlayerID " + playerId);
                    MonsterPlayerPrefs.PlayerId = playerId;
                    MonsterPlayerPrefs.DeviceId = deviceId;
                    //Debug.Log("Name received: " + name);
                    MonsterPlayerPrefs.PlayerName = name;
                    ServerAnalytics.isGameServicesInitialized = true;
                    //DoAfterInitialization();
                    initialized = true;
                    if (MonsterPlayerPrefs.PlayerId != -1)
                    {
                        GameAnalytics.SetUserID(MonsterPlayerPrefs.PlayerId.ToString());
                    }
                }
                initializing = false;
                isReportingPlayer = false;
            });
            task.Report(MonsterPlayerPrefs.AdvertisingId);
        }

#if USE_MONSTER_SERVICES
        public void SetReferralSetupInfo(int referralEventId, string baseDomain, string referralDomain, int androidMinimumVersion, string appleStoreId, string iosMinimumVersion) {
            this.referralEventId = referralEventId;
            this.baseDomain = baseDomain;
            this.referralDomain = referralDomain;
            this.androidMinimumVersion = androidMinimumVersion;
            this.appleStoreId = appleStoreId;
            this.iosMinimumVersion = iosMinimumVersion;
        }

        public int UnclaimedReferralCount {
            get {
                return unclaimedReferralCount;
            }
            set {
                int oldValue = unclaimedReferralCount;
                unclaimedReferralCount = value;
                if (oldValue != value) {
                    UnclaimedReferralCountChanged?.Invoke();
                }
            }
        }

        public DynamicLinkInfo DynamicLinkInfo {
            get; set;
        }

        public bool PendingDynamicLinkReward {
            get {
                return pendingDynamicLinkReward;
            }
            internal set {
                bool oldvalue = pendingDynamicLinkReward;
                pendingDynamicLinkReward = value;
                if (oldvalue != value) {
                    PendingDynamicLinkRewardChanged?.Invoke();
                }
            }
        }
        private void DoAfterInitialization() {
            ReportFCMToken();
            ReportReferral();
            FetchReferralInfo();
        }

        public void UpdateScore(int leaderboardId, long score, Action<bool> callback) {
            UpdateScore(leaderboardId, score, null, null, callback);
        }

        public void UpdateScore(int leaderboardId, long score, string jsonMetadata, Action<bool> callback) {
            UpdateScore(leaderboardId, score, null, jsonMetadata, callback);
        }

        public void UpdateScore(int leaderboardId, long score, int? clazz, string jsonMetadata, Action<bool> callback) {
            LeaderboardUpdateScoreTask updateScore = new LeaderboardUpdateScoreTask(leaderboardId, (message) => {
                callback?.Invoke(false);
            },
            () => {
                callback?.Invoke(true);
            });
            updateScore.UpdateScore(score, clazz, jsonMetadata);
        }

        public void AddScore(int leaderboardId, long score, Action<bool> callback) {
            AddScore(leaderboardId, score, null, null, callback);
        }

        public void AddScore(int leaderboardId, long score, string jsonMetadata, Action<bool> callback) {
            AddScore(leaderboardId, score, null, jsonMetadata, callback);
        }

        public void AddScore(int leaderboardId, long score, int? clazz, string jsonMetadata, Action<bool> callback) {
            LeaderboardAddScoreTask task = new LeaderboardAddScoreTask(leaderboardId, (message) => {
                callback(false);
            },
            () => {
                callback(true);
            });
            task.AddScore(clazz, score, jsonMetadata);
        }

        public void FetchLeaderboard(int leaderboardId, string scope, int limit, int aboveCount, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboard(leaderboardId, scope, null, LeaderboardType.Basic, null, limit, aboveCount, callback);
        }
        public void FetchCountryLeaderboard(int leaderboardId, string scope, int limit, int aboveCount, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboard(leaderboardId, scope, null, LeaderboardType.Country, null, limit, aboveCount, callback);
        }

        //public void FetchLeaderboard(int leaderboardId, string scope, int limit, int aboveCount, string countryCode, Action<bool, LeaderboardResponse> callback) {
        //    FetchLeaderboard(leaderboardId, scope, null, false, countryCode, limit, aboveCount, callback);
        //}

        public void FetchFriendLeaderboard(int leaderboardId, string scope, int limit, int aboveCount, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboard(leaderboardId, scope, null, LeaderboardType.Friend, null, limit, aboveCount, callback);
        }

        public void FetchFriendLeaderboard(int leaderboardId, string scope, int limit, int aboveCount, string countryCode, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboard(leaderboardId, scope, null, LeaderboardType.Friend, countryCode, limit, aboveCount, callback);
        }

        public void FetchLeaderboard(int leaderboardId, string scope, int clazz, int limit, int aboveCount, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboard(leaderboardId, scope, clazz, LeaderboardType.Basic, null, limit, aboveCount, callback);
        }

        public void FetchLeaderboard(int leaderboardId, string scope, int clazz, int limit, int aboveCount, string countryCode, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboard(leaderboardId, scope, clazz, LeaderboardType.Basic, countryCode, limit, aboveCount, callback);
        }

        public void RenamePlayer(string newName, Action<bool> callback) {
            RenamePlayerTask task = new RenamePlayerTask((message) => {
                callback(false);
            },
            () => {
                MonsterPlayerPrefs.PlayerName = newName;
                callback(true);
            });
            task.Rename(newName);
        }

        public void GetPeriodicallyLeaderboadInfo(int leaderboardId, Action<bool, PeriodicallyLeaderboardInfo> callback) {
            LeaderboardFetchPeriodicallyInfoTask task = new LeaderboardFetchPeriodicallyInfoTask(leaderboardId, (message) => {
                callback(false, null);
            }, (leaderboardInfo) => {
                callback(true, leaderboardInfo);
            });
            task.FetchInfo();
        }

        private void FetchLeaderboard(int leaderboardId, string scope, int? clazz, LeaderboardType type, string countryCode, int limit, int aboveCount, Action<bool, LeaderboardResponse> callback) {
            FetchLeaderboardTask fetchLeaderboardTask = new FetchLeaderboardTask(leaderboardId, (message) => {
                callback(false, null);
            },
            (items) => {
                callback(true, items);
            });
            fetchLeaderboardTask.Fetch(scope, clazz, type, countryCode, limit, aboveCount);
        }



        

        public void ReportFCMToken()
        {
            if (MonsterPlayerPrefs.PlayerId == -1 || string.IsNullOrEmpty(MonsterPlayerPrefs.DeviceId) || MonsterPlayerPrefs.FcmTokenReported || string.IsNullOrEmpty(MonsterPlayerPrefs.FcmToken))
            {
                return;
            }
            JSONObject sendBody = new JSONObject();
            sendBody["playerId"] = MonsterPlayerPrefs.PlayerId;
            sendBody["fcmToken"] = MonsterPlayerPrefs.FcmToken;
            sendBody["deviceId"] = MonsterPlayerPrefs.DeviceId;
            RestClient.SendPostRequest("/v3/games/players/fcm", sendBody.ToString(), (statusCode, message, data) =>
            {
                if (statusCode == 200)
                {
                    MonsterPlayerPrefs.FcmTokenReported = true;
                }
            });
        }

        public void SaveGameData(long clientTimestamp, JObject data, UnityAction<bool> callback)
        {
            JObject body = new JObject();
            StringBuilder strToHash = new StringBuilder();
            string dataJson = data.ToString();
            strToHash.Append(SALT);
            strToHash.Append(MonsterPlayerPrefs.PlayerId);
            strToHash.Append(dataJson);
            strToHash.Append(Application.identifier);
#if UNITY_ANDROID
            strToHash.Append("1");
#else
            strToHash.Append("2");
#endif
            strToHash.Append(SALT);
            body["playerId"] = MonsterPlayerPrefs.PlayerId;
            body["deviceId"] = MonsterPlayerPrefs.DeviceId;
            body["loginToken"] = MonsterPlayerPrefs.LoginToken;
            body["hash"] = strToHash.ToString().sha256();
            body["clientTimestamp"] = clientTimestamp;
            body["data"] = dataJson;

            RestClient.SendPostRequest("/v4/games/players/data", body.ToString(), (statusCode, message, resData) =>
            {
                callback?.Invoke(statusCode == 200);
            });


        }

        public void LoadGameData(UnityAction<int, JSONObject> callback) {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams["playerId"] = MonsterPlayerPrefs.PlayerId.ToString();
            queryParams["loginToken"] = MonsterPlayerPrefs.LoginToken;
            RestClient.SendGetRequest("/v4/games/players/data", queryParams, (statusCode, message, responseStr) => {
                if (statusCode == 200) {
                    JSONObject response = JSONNode.Parse(responseStr).AsObject;
                    string serverHash = response["hash"];
                    string dataJson = response["data"];
                    StringBuilder strToHash = new StringBuilder();
                    strToHash.Append(SALT);
                    strToHash.Append(MonsterPlayerPrefs.PlayerId);
                    strToHash.Append(dataJson);
                    strToHash.Append(Application.identifier);
#if UNITY_ANDROID
                    strToHash.Append(1);
#else
                    strToHash.Append(2);
#endif
                    strToHash.Append(SALT);
                    string clientHash = strToHash.ToString().sha256();
                    //Debug.Log($"client hash: {clientHash} and serverHash: {serverHash}");
                    if (clientHash != serverHash) {
                        callback(500, null);
                    } else {
                        callback((int)statusCode, JSONNode.Parse(dataJson).AsObject);
                    }
                } else {
                    callback((int)statusCode, null);
                }
            });
        }

        public void FetchReferralInfo(bool force = false) {
            Debug.Log("FetchReferralInfo");
            if (MonsterPlayerPrefs.PlayerId == -1 || (isReferralInfoFetched && !force)) {
                return;
            }
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams["playerId"] = MonsterPlayerPrefs.PlayerId.ToString();
            queryParams["referralEventId"] = referralEventId.ToString();
            RestClient.SendGetRequest("/v3/games/referral", queryParams, (statusCode, message, data) => {
                isReferralInfoFetched = true;
                if (statusCode == 200) {
                    JSONObject referralPlayerInfo = JSON.Parse(data).AsObject;
                    int unclaimedCoint = referralPlayerInfo["unclaimedCount"];
                    UnclaimedReferralCount = unclaimedCoint;
                    bool isConfigured = referralPlayerInfo["isConfigured"];
                    if (!isConfigured) {
                        string title = referralPlayerInfo["title"];
                        string description = referralPlayerInfo["description"];
                        string socialImageUrl = referralPlayerInfo["socialImageUrl"];
                        SetupReferralEvent(title, description, socialImageUrl);
                    }
                    string dynamicLink = referralPlayerInfo["dynamicLink"];
                    if (!string.IsNullOrEmpty(dynamicLink)) {
                        MonsterPlayerPrefs.ReferralDynamicLink = dynamicLink;
                    }
                }
            });
        }

        public void SetupReferralEvent(string title, string description, string previewUrl) {



            Debug.Log($"Referral dynamic link: {MonsterPlayerPrefs.ReferralDynamicLink}");
            if (!MonsterPlayerPrefs.ReferralEventConfigured)
            {
                if (string.IsNullOrEmpty(MonsterPlayerPrefs.ReferralDynamicLink))
                {

                    var components = new DynamicLinkComponents(new System.Uri($"https://{baseDomain}/?referralPlayerId={MonsterPlayerPrefs.PlayerId}&referralEventId={referralEventId}"), $"https://{referralDomain}");
                    IOSParameters iosParams = new IOSParameters(Application.identifier);
                    iosParams.AppStoreId = appleStoreId;
                    iosParams.MinimumVersion = iosMinimumVersion;
                    components.IOSParameters = iosParams;
                    AndroidParameters androidParams = new AndroidParameters(Application.identifier);
                    androidParams.MinimumVersion = androidMinimumVersion;
                    components.AndroidParameters = androidParams;
                    GoogleAnalyticsParameters gaParams = new GoogleAnalyticsParameters();
                    gaParams.Source = "referral_event";
                    gaParams.Campaign = "referral-" + MonsterPlayerPrefs.PlayerId;
                    components.GoogleAnalyticsParameters = gaParams;
                    ITunesConnectAnalyticsParameters ituneParams = new ITunesConnectAnalyticsParameters();
                    ituneParams.CampaignToken = "referral-" + MonsterPlayerPrefs.PlayerId;
                    ituneParams.AffiliateToken = MonsterPlayerPrefs.PlayerId.ToString();
                    ituneParams.ProviderToken = "referral_event";
                    components.ITunesConnectAnalyticsParameters = ituneParams;
                    SocialMetaTagParameters socialParams = new SocialMetaTagParameters();
                    socialParams.Title = title;
                    socialParams.Description = description;
                    socialParams.ImageUrl = new System.Uri(previewUrl);
                    components.SocialMetaTagParameters = socialParams;
                    DynamicLinks.GetShortLinkAsync(components).ContinueWith(task =>
                    {
                        Debug.Log("get short link finished");
                        if (task.IsCanceled)
                        {
                            Debug.LogError("GetShortLinkAsync was canceled.");
                            return;
                        }
                        if (task.IsFaulted)
                        {
                            Debug.LogError("GetShortLinkAsync encountered an error: " + task.Exception);
                            return;
                        }

                        // Short Link has been created.
                        ShortDynamicLink link = task.Result;
                        Debug.LogFormat("Generated short link {0}", link.Url);
                        MonsterPlayerPrefs.ReferralDynamicLink = link.Url.ToString();
                        SendDynamicLinkToServer();
                        var warnings = new List<string>(link.Warnings);
                        if (warnings.Count > 0)
                        {
                            for (int i = 0; i < warnings.Count; i++)
                            {
                                Debug.LogWarning(warnings[i]);
                            }
                        }
                    });
                }
                else
                {
                    SendDynamicLinkToServer();
                }
            }

        }

        public void ClaimReferralReward(int claimCount, UnityAction<bool, int> callback) {
            if (claimingReferralReward) {
                return;
            }
            claimingReferralReward = true;
            JSONObject sendBody = new JSONObject();
            sendBody["playerId"] = MonsterPlayerPrefs.PlayerId;
            sendBody["referralEventId"] = referralEventId;
            sendBody["claimCount"] = claimCount;
            RestClient.SendPostRequest("/v3/games/referral/reward", sendBody.ToString(), (statusCode, message, data) => {
                if (statusCode == 200) {
                    JSONObject ret = JSON.Parse(data).AsObject;
                    int claimedCount = ret["claimedCount"];
                    callback(true, claimedCount);
                } else {
                    callback(false, 0);
                }
            });
        }

        public void ReportReferral() {
            Debug.Log("ReportReferral FB DynamicLink");
            if (DynamicLinkInfo == null || MonsterPlayerPrefs.ReferralReported || reportingReferral || MonsterPlayerPrefs.PlayerId == -1) {
                Debug.Log("ReportReferral requirement not met ");
                Debug.Log("ReportReferral DynamicLinkInfo == null " + (DynamicLinkInfo == null));
                Debug.Log("ReportReferral MonsterPlayerPrefs.ReferralReported " + (MonsterPlayerPrefs.ReferralReported));
                Debug.Log("ReportReferral reportingReferral " + (reportingReferral));
                Debug.Log("ReportReferral MonsterPlayerPrefs.PlayerId == -1 " + (MonsterPlayerPrefs.PlayerId == -1));

                return;
            }
            reportingReferral = true;
#if UNITY_IOS
        IOSHelper.RequestDCToken((status, token) =>
        {
            SendReferralInfoToServer(token);
        });
#else
            SendReferralInfoToServer(null);
#endif

        }

        private void SendReferralInfoToServer(string dcToken) {
            int referralEventId = DynamicLinkInfo.referralEventId;
            int referralPlayerId = DynamicLinkInfo.referralPlayerId;
            string advertisingId = MonsterPlayerPrefs.AdvertisingId;
            string deviceId = MonsterPlayerPrefs.DeviceId;
            string strToHash = $"{SALT}{advertisingId}{referralEventId}{SALT}{referralPlayerId}{deviceId}";
            string hash = ComputeSha256Hash(strToHash);
            JSONObject sendBody = new JSONObject();
            sendBody["referralEventId"] = referralEventId;
            sendBody["targetPlayerId"] = MonsterPlayerPrefs.PlayerId;
            sendBody["referralPlayerId"] = referralPlayerId;
            sendBody["advertisingId"] = advertisingId;
            sendBody["deviceId"] = deviceId;
            sendBody["isFirstTime"] = isFirstOpen;
            if (dcToken != null) {
                sendBody["dcToken"] = dcToken;
            }
            sendBody["hash"] = hash;
            RestClient.SendPostRequest("/v3/games/referral", sendBody.ToString(), (statusCode, message, data) => {
                if (statusCode == 200) {
                    MonsterPlayerPrefs.ReferralReported = true;
                    PendingDynamicLinkReward = true;

                    Debug.Log("ReportReferral Succes ");
                }
                reportingReferral = false;
            });
        }

        private static string ComputeSha256Hash(string rawData) {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create()) {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void SendDynamicLinkToServer() {
            JSONObject sendBody = new JSONObject();
            sendBody["dynamicLink"] = MonsterPlayerPrefs.ReferralDynamicLink;
            sendBody["playerId"] = MonsterPlayerPrefs.PlayerId;
            sendBody["referralEventId"] = referralEventId;
            RestClient.SendPostRequest("/v3/games/referral/setup", sendBody.ToString(), (statusCode, message, data) => {
                if (statusCode == 200) {
                    MonsterPlayerPrefs.ReferralEventConfigured = true;
                }
            });
        }

#endif
    }
}
