using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using RotaryHeart.Lib.SerializableDictionary;

[System.Serializable]
public class IntStringDictionary : SerializableDictionaryBase<int, string> { }


[CreateAssetMenu(fileName = "AudienceAdsManager", menuName = "Audience/Audience Ads Manager")]
public class AudienceAdsManager : SingletonScriptableObject<AudienceAdsManager>
{
    private const string PREF_SHOW_SPLASH_ADS_FIRST = "askjvcvo_splash";


    [SerializeField] IntStringDictionary defaultInterstitialPositons;

    [System.NonSerialized] IntStringDictionary _cacheInterstitialPositons;
    IntStringDictionary cacheInterstitialPositons
    {
        get
        {
            if (_cacheInterstitialPositons == null)
            {
                _cacheInterstitialPositons = new IntStringDictionary ();
            }
            return _cacheInterstitialPositons;
        }
        set
        {
            if (_cacheInterstitialPositons == null)
            {
                _cacheInterstitialPositons = new IntStringDictionary();
            }
            _cacheInterstitialPositons = value;
        }
    }

    private bool canShowSplashAdsFromFirstOpen
    {
        set
        {
            PlayerPrefs.SetInt(PREF_SHOW_SPLASH_ADS_FIRST, value ? 1 : 0);
        }
        get
        {
            return PlayerPrefs.GetInt(PREF_SHOW_SPLASH_ADS_FIRST, 0) == 1;
        }
    }

    public int timeInterval = 25;
    public int timeIntervalInGame;
    public int firstDelay = 60;
    public int timeReward = 20;
    public double lastTimeWatchInterstitial = -1;
    public double lastTimeWatchReward = -1;
    public double timeInit = -1;
    public string defaultMediation = "ironsource";
    //"mediations":{"mopub":50,"ironsource":50,"maxapplovin":21}
    private JSONNode mediations;

    public bool loadInterstialDataDone = false;

    public void LoadInterstitialData(JSONNode listInsterstitialJson)
    {
        cacheInterstitialPositons = new IntStringDictionary();
        lastTimeWatchInterstitial = UnbiasedTime.Instance.Now().ToJavaTimeStamp();

        if (listInsterstitialJson == null || listInsterstitialJson.Count == 0)
        {
            Debug.Log("Check Null " + listInsterstitialJson == null);
            return;
        }

        for (int i = 0; i < listInsterstitialJson.Count; ++i)
        {
            try
            {
                int id = listInsterstitialJson[i]["positionId"];
                string name = listInsterstitialJson[i]["name"];
                timeInterval = listInsterstitialJson[i]["interval"];
                firstDelay = listInsterstitialJson[i]["firstDelay"];
                timeReward = listInsterstitialJson[i]["rewardVideoInterval"];
                mediations = listInsterstitialJson[i]["mediations"];

                timeIntervalInGame = listInsterstitialJson[i]["intervalInGame"] != null ?
                    listInsterstitialJson[i]["intervalInGame"].AsInt : 120;

                if (timeInit == -1)
                {
                    timeInit = UnbiasedTime.Instance.Now().ToJavaTimeStamp();
                }
                

                if (!cacheInterstitialPositons.ContainsKey(id))
                {
                    Debug.Log($"Add position {id}");
                    cacheInterstitialPositons.Add(id, name) ;
                }

            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
        loadInterstialDataDone = true;
    }

    public void InitMediation(Action<string[]> onCompleteCallBack)
    {
        string[] listMed = new string[] { IronSourceIntances.SOURCE, MopubInstances.SOURCE, MaxInstances.SOURCE };
        string[] res = new string[1];
        res[0] = defaultMediation;
        if (loadInterstialDataDone)
        {
            if (mediations != null)
            {
                int totalRate = 0;
                Debug.Log("InitMediation " + mediations.ToString());
                int[] mediationDict = new int[listMed.Length];
                for (int i = 0; i < listMed.Length; i++)
                {
                    totalRate += mediations[listMed[i]];
                    mediationDict[i] = totalRate;
                    Debug.Log("InitMediation " + totalRate);
                }

                if (totalRate != 0)
                {
                    int rate = UnityEngine.Random.Range(0, totalRate);
                    Debug.Log("InitMediation rate " + rate);

                    for (int i = 0; i < mediationDict.Length; i++)
                    {
                        if (rate < mediationDict[i])
                        {
                            res[0] = listMed[i];
                            break;
                        }
                    }

                }
                else
                {
                    res[0] = defaultMediation;
                }
            }
        }
        Debug.Log("InitMediation netw " + res[0]);
        onCompleteCallBack(res);
    }


    public bool CheckShowInterstitial(int positionId)
    {
        if (positionId == 0) return false;
        if (lastTimeWatchInterstitial == -1)
        {
            lastTimeWatchInterstitial = UnbiasedTime.Instance.Now().ToJavaTimeStamp();
        }

        if (timeInit == -1)
        {
            timeInit = UnbiasedTime.Instance.Now().ToJavaTimeStamp();
        }

        bool isContainPosistion = true;
        if (cacheInterstitialPositons != null && cacheInterstitialPositons.Count > 0)
        {
            Debug.Log("Khoatv: init cacheInterstitialPositons" + cacheInterstitialPositons.Count);
            isContainPosistion = cacheInterstitialPositons.ContainsKey(positionId);
        }
        else
        {
            Debug.Log("Khoatv: init defaultInterstitialPositons");
            isContainPosistion = defaultInterstitialPositons.ContainsKey(positionId);
        }
        var timeCount = UnbiasedTime.Instance.Now().ToJavaTimeStamp().DeltaSecond(lastTimeWatchInterstitial);
        var timeFromFirstInit = UnbiasedTime.Instance.Now().ToJavaTimeStamp().DeltaSecond(timeInit);
        bool isReachTimeInterval = timeCount >= (positionId == ValueConstants.INTER_IN_GAME_POSITION ? timeIntervalInGame : timeInterval);

        bool showReward = false;
        if (lastTimeWatchReward > -1)
        {
            double timecountReward = UnbiasedTime.Instance.Now().ToJavaTimeStamp().DeltaSecond(lastTimeWatchReward);
            Debug.Log($"TimecountReward = {timecountReward }, timeReward = {timeReward}");
            if (timecountReward < (positionId == ValueConstants.INTER_IN_GAME_POSITION ? timeIntervalInGame : timeReward))
            {
                showReward = true;
            }
            else
            {
                lastTimeWatchReward = -1;
            }
        }


        Debug.Log("Check Audience: " + isContainPosistion + " " + isReachTimeInterval + ", timeCount = " + timeCount + ", timeFromFirstInit = " + timeFromFirstInit);

        if (isContainPosistion &&
            isReachTimeInterval &&
            timeFromFirstInit > firstDelay &&
            !showReward)
        {
            Debug.Log("Check Audience: true");
            return true;
        }
        Debug.Log($"Check Audience: false, showreward {showReward}, firstDelay {firstDelay} ");
        return false;
    }

    public bool CheckCanShowPauseAds()
    {
        bool isDataContainPauseAd;
        if (cacheInterstitialPositons != null && cacheInterstitialPositons.Count > 0)
        {
            isDataContainPauseAd = cacheInterstitialPositons.ContainsKey(ValueConstants.INTER_PAUSE_GAME_POSITION);
            Debug.Log("check contain pause ad" + isDataContainPauseAd);

        }
        else
        {
            isDataContainPauseAd = defaultInterstitialPositons.ContainsKey(ValueConstants.INTER_PAUSE_GAME_POSITION);
        }

        return isDataContainPauseAd;
    }

    public string GetPositionName(int positionId)
    {
        if (cacheInterstitialPositons != null && cacheInterstitialPositons.Count > 0)
        {
            if (cacheInterstitialPositons.ContainsKey(positionId))
                return cacheInterstitialPositons[positionId];
        }
        else
        {

            if (defaultInterstitialPositons.ContainsKey(positionId))
                return defaultInterstitialPositons[positionId];
        }
        return "None";
    }

}
