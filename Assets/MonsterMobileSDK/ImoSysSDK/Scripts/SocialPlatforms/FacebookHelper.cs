#if HAS_FB


using Facebook.Unity;

using ImoSysSDK.Network;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImoSysSDK.SocialPlatforms
{
    public enum AuthPlatform
    {
        google,
        facebook,
        apple
    }

    public class FacebookHelper
    {

        private static readonly object instanceLock = new object();
        private static FacebookHelper instance;

        private const string LOGIN_PATH = "/v3/games/facebook/login";
        private const string LOGOUT_PATH = "/v3/games/facebook/logout";

        public delegate void OnLoginSuccessEvent(bool needReloadData);

        public delegate void OnLoginFailedEvent(string message);

        public delegate void OnLogoutFinishedEvent(bool success);

        public event OnLoginFailedEvent onLoginFailed;

        public event OnLoginSuccessEvent onLoginSuccess;

        public static FacebookHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new FacebookHelper();
                        }
                    }
                }
                return instance;
            }
        }

        public FacebookHelper()
        {
        }

        public void Login(OnLoginFailedEvent onLoginFailed, OnLoginSuccessEvent onLoginSuccess)
        {
            this.onLoginFailed = onLoginFailed;
            this.onLoginSuccess = onLoginSuccess;

            if (FB.IsInitialized)
            {
                InternalLogin();
            }
            else
            {
                FB.Init(OnInitComplete, OnHideUnity);
            }

            //GameAnalytics.LogEventStartLogin();
        }

        public void Logout(OnLogoutFinishedEvent callback)
        {
            JSONObject sendBody = new JSONObject();
            sendBody["playerId"] = MonsterPlayerPrefs.PlayerId;
            sendBody["deviceId"] = MonsterPlayerPrefs.DeviceId;
            Network.RestClient.SendPostRequest(LOGOUT_PATH, sendBody.ToString(), (statusCode, message, data) =>
            {
                if (statusCode == 200)
                {
                    MonsterPlayerPrefs.FacebookLoggedIn = false;
                    MonsterPlayerPrefs.AvatarUrl = null;
                }
                callback?.Invoke(statusCode == 200);
            });
        }

        private void OnHideUnity(bool isGameShown)
        {
            if (!isGameShown)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }

        private void OnInitComplete()
        {
            if (FB.IsInitialized)
            {
                InternalLogin();
            }
            else
            {
                onLoginFailedCallback("Failed to initialize the Facebook SDK");
                Debug.Log("Failed to initialize the Facebook SDK");
            }
        }

        private void InternalLogin()
        {
            if (!FB.IsLoggedIn)
            {
                FB.LogInWithReadPermissions(new List<string>() { "public_profile", "email" }, AuthCallback);
            }
            else
            {
                LoginWithServer(AccessToken.CurrentAccessToken);
            }
        }

        private void AuthCallback(ILoginResult result)
        {
            if (FB.IsLoggedIn)
            {
                var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
                LoginWithServer(aToken);
            }
            else
            {
                Debug.Log("User cancelled login");
                onLoginFailedCallback("User cancelled login");
            }
        }

        private void LoginWithServer(AccessToken accessToken)
        {
            AuthInfoBody authInfoBody = new AuthInfoBody
            {
                //authSource = Constants.authSourceFacebook,
                authId = accessToken.UserId,
                authToken = accessToken.TokenString,
                name = string.Empty,
                playerId = MonsterPlayerPrefs.PlayerId,
                deviceId = MonsterPlayerPrefs.DeviceId,
                advertisingId = MonsterPlayerPrefs.AdvertisingId
            };
            BindAccountHelper.Instance.RequestBindAccount(authInfoBody, onLoginFailed, onLoginSuccess);

        }

        private void onLoginFailedCallback(string message)
        {
            if (onLoginFailed != null)
            {
                onLoginFailed(message);
                onLoginFailed = null;
            }
        }

    }


    [System.Serializable]
    public class FacebookInfoBody
    {
        public string fbId;
        public string fbToken;
        public int playerId;
        public string advertisingId;
        public string deviceId;
    }

    public class BindAccountHelper
    {
        private static readonly object instanceLock = new object();
        private static BindAccountHelper instance;

        private const string LOGIN_PATH = "/v4/games/services/login";
        private const string LOGOUT_PATH = "/v4/games/services/logout";

        public event FacebookHelper.OnLoginFailedEvent onLoginFailed;
        public event FacebookHelper.OnLoginSuccessEvent onLoginSuccess;

        public static BindAccountHelper Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (instanceLock)
                    {
                        if (instance == null)
                        {
                            instance = new BindAccountHelper();
                        }
                    }
                }
                return instance;
            }
        }

        public void Logout(FacebookHelper.OnLogoutFinishedEvent callback)
        {
            JSONObject sendBody = new JSONObject();
            sendBody["playerId"] = MonsterPlayerPrefs.PlayerId;
            sendBody["deviceId"] = MonsterPlayerPrefs.DeviceId;
            sendBody["loginToken"] = MonsterPlayerPrefs.LoginToken;
            Network.RestClient.SendPostRequest(LOGOUT_PATH, sendBody.ToString(), (statusCode, message, data) =>
            {
                Debug.Log("<color=cyan>Logout: " + statusCode + "</color>");
                if (statusCode == 200)
                {
                    MonsterPlayerPrefs.FacebookLoggedIn = false;
                    MonsterPlayerPrefs.AvatarUrl = null;
                }
                callback?.Invoke(statusCode == 200);
            });
        }

        public void RequestBindAccount(AuthInfoBody body,
            FacebookHelper.OnLoginFailedEvent onLoginFailed,
            FacebookHelper.OnLoginSuccessEvent onLoginSuccess) =>
            RequestBindAccount(JsonUtility.ToJson(body),
                onLoginFailed,
                onLoginSuccess);

        public void RequestBindAccount(string body,
            FacebookHelper.OnLoginFailedEvent onLoginFailed,
            FacebookHelper.OnLoginSuccessEvent onLoginSuccess)
        {
            this.onLoginFailed = onLoginFailed;
            this.onLoginSuccess = onLoginSuccess;
            RestClient.SendPostRequest(LOGIN_PATH, body, OnLoginRequestFinished);
        }

        private void OnLoginRequestFinished(long statusCode, string message, string data)
        {
            Debug.Log("<color=cyan>OnLoginRequestFinished: " + statusCode + "</color>");
            if (statusCode == 200)
            {
                //MonsterPlayerPrefs.PlayerId = "fb:" + AccessToken.CurrentAccessToken.UserId;
                JSONObject res = JSONNode.Parse(data).AsObject;
                int oldPlayerId = MonsterPlayerPrefs.PlayerId;
                MonsterPlayerPrefs.PlayerId = res["playerId"];
                MonsterPlayerPrefs.DeviceId = res["deviceId"];
                MonsterPlayerPrefs.AvatarUrl = res["avatarUrl"];
                MonsterPlayerPrefs.PlayerName = res["name"];
                MonsterPlayerPrefs.LoginToken = res["loginToken"];
                MonsterPlayerPrefs.FacebookLoggedIn = true;
                onBindSuccessCallback(oldPlayerId != MonsterPlayerPrefs.PlayerId);
            }
            else
            {
                onBindFailedCallback(message);
            }
        }

        private void onBindFailedCallback(string message)
        {
            if (onLoginFailed != null)
            {
                onLoginFailed(message);
                onLoginFailed = null;
            }

            //GameAnalytics.LogEventLoginStatus(false);
        }

        private void onBindSuccessCallback(bool needReloadData)
        {

            if (onLoginSuccess != null)
            {
                onLoginSuccess(needReloadData);
                onLoginSuccess = null;
            }
            //GameAnalytics.LogEventLoginStatus(true);

        }
    }

}
#endif