using ImoSysSDK.Network;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "AudienceManager", menuName ="Audience/General Manager")]
public class AudienceManager : SingletonScriptableObject<AudienceManager>
{
    public void LoadAudienceDataFromServer()
    {
        string path = "/v4/games/players/audiencewelfare";
        Dictionary<string, string> queryParams = new Dictionary<string, string>();

        queryParams.Add("playerId", MonsterPlayerPrefs.PlayerId.ToString());

        RestClient.SendGetRequest(path, queryParams, (statusCode, message, responseStr) => {
            OnRequestAudienceDataFromServerFinish(statusCode, message, responseStr);
        });
    }

    private void OnRequestAudienceDataFromServerFinish(long statusCode, string message, string responseStr)
    {
        Debug.Log("<color=cyan>----Load Audience " + statusCode + " "
            + message + " "
            + responseStr + "</color>");
        if (statusCode == 200)
        {
            LoadAudienceData(responseStr);
        }
    }

    private void LoadAudienceData(string json)
    {
        JSONNode audienceJson = JSON.Parse(json);
        try
        {
            JSONNode insterstitialJson = audienceJson["interstitials"];
            Debug.Log("<color=cyan>" + insterstitialJson.ToString() + "</color>");
            AudienceAdsManager.Instance.LoadInterstitialData(insterstitialJson);
        }
        catch(Exception e)
        {
            Debug.Log(e);
        }
        
        
    }

}
