using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace ImoSysSDK.Network
{
    using Core;
    using System.Collections.Generic;

    public class RestClient
    {
#if ENV_PROD || ENV_LOG
        public const string DOMAIN = "https://api.gamesontop.com";
#else
        public const string DOMAIN = "https://api-staging.gamesontop.com";
#endif

        public delegate void OnRequestFinished(long statusCode, string message, string data);

        public static void SendPostRequest(string path, string sJson, OnRequestFinished onRequestFinished)
        {
            SendRequest(UnityWebRequest.kHttpVerbPOST, path, null, sJson, onRequestFinished);
        }

        public static void SendGetRequest(string path, Dictionary<string, string> queryParams, OnRequestFinished onRequestFinished)
        {
            SendRequest(UnityWebRequest.kHttpVerbGET, path, queryParams, null, onRequestFinished);
        }

        public static void SendRequest(string method, string path, Dictionary<string, string> queryParams, string sJson, OnRequestFinished onRequestFinished)
        {
            StringBuilder sb = new StringBuilder(DOMAIN);
            sb.Append(path);
            if (queryParams != null && queryParams.Count > 0)
            {
                sb.Append('?');
                //bool first = true;
                //foreach(KeyValuePair<string, string> entry in queryParams) {
                //    if (!first) {
                //        sb.Append('&');
                //    } else {
                //        first = false;
                //    }
                //    sb.Append(UnityWebRequest.EscapeURL(entry.Key));
                //    sb.Append('=');
                //    sb.Append(UnityWebRequest.EscapeURL(entry.Value));
                //}
                sb.Append(Encoding.UTF8.GetString(UnityWebRequest.SerializeSimpleForm(queryParams)));
            }
            string url = sb.ToString();
#if !ENV_PROD
            Debug.Log("rest request url: " + url);
#endif
            UnityWebRequest request = new UnityWebRequest(url, method);
            if ((method.Equals(UnityWebRequest.kHttpVerbPOST) || method.Equals(UnityWebRequest.kHttpVerbPUT)) && !string.IsNullOrEmpty(sJson))
            {
#if !ENV_PROD
                Debug.Log("Post body: " + sJson);
#endif
                request.SetRequestHeader("Content-Type", "application/json");
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(sJson));
            }

            request.SetRequestHeader("pn", Application.identifier);
#if UNITY_ANDROID
            request.SetRequestHeader("p", "1");
#else
            request.SetRequestHeader("p", "2");
#endif
            Debug.Log("Request Header " + request.GetRequestHeader("pn"));
            Debug.Log("Request Header " + request.GetRequestHeader("p"));
            request.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            operation.completed += (ops) =>
            {
                string message = request.isNetworkError ? request.error : string.Empty;
                string strResponse = request.downloadHandler.text;
#if !ENV_PROD

                Debug.Log("http response code: " + request.responseCode);
                Debug.Log("error message: " + message);
                Debug.Log("str response: " + strResponse);
#endif
                onRequestFinished(request.responseCode, message, strResponse);
            };
        }
    }
}