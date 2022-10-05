using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
// This class is intended to be used the the AppsFlyerObject.prefab

public class AppsFlyersInitializer : MonoBehaviour, IAppsFlyerConversionData
{

    // These fields are set from the editor so do not modify!
    //******************************//
    public string devKey;
    public string appID;
    public string UWPAppID;
    public bool isDebug;
    public bool getConversionData;
    //******************************//


    void Start()
    {
        // These fields are set from the editor so do not modify!
        //******************************//
        Debug.Log("AppsFlyer Init" );
#if ENV_PROD
        AppsFlyer.setIsDebug(false);
#else
        AppsFlyer.setIsDebug(isDebug);
#endif
#if UNITY_WSA_10_0 && !UNITY_EDITOR
        AppsFlyer.initSDK(devKey, UWPAppID, getConversionData ? this : null);
#else
        AppsFlyer.initSDK(devKey, appID, getConversionData ? this : null);
#endif
        //******************************/

        AppsFlyer.startSDK();
    }


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

       
        if (PlayerPrefs.GetInt("has_sent_conversionData", 0) == 0)
        {
            PlayerPrefs.SetInt("has_sent_conversionData", 1);
            Debug.Log("AppsFlyers send_conversion_data ");
            GameAnalytics.LogEventFirebase("appsflyer_conversion_data", parameters.ToArray());
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

}
