using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AdRequestType
{
    request,
    load,
    loadsuccess,
    loadfail,
    show,
    canreachreward,
    impression,
    clickad
}
public enum AdType
{
    videorewarded, inter, banner, native
}

public class BaseAds : MonoBehaviour
{
    public IAdsListener adsListener;
    [HideInInspector]
    public bool sdkInitialized, validated;
    [HideInInspector]
    public bool isShownBanner, isBannerLoaded;
    [HideInInspector]
    public bool loadBannerCalled;
    [HideInInspector]
    public bool isInterstitialInitialized, isRewardedVideoInitialized;
    [HideInInspector]
    protected bool isInterstitialLoaded, isRewardedVideoLoaded;
    [HideInInspector]
    public bool isInterstitialLoading, isRewardedvideoLoading;
    [HideInInspector]
    public bool isShowInterstitial, isShowRewardedVideo;
    [HideInInspector]
    public bool gotRewarded;

    public bool isUniqueBaseAds;
    public bool isMainMediation;

    public virtual bool IsInterstitialAvailable()
    {
        return isInterstitialLoaded;
    }

    public virtual bool IsRewardedVideoAvailable()
    {
        return isRewardedVideoLoaded;
    }

    public bool HasAdsShowing
    {
        get
        {
            return isShowInterstitial || isShowRewardedVideo;
        }
    }

    public virtual void InitAdsNetWork(IAdsListener adsListener)
    {
        this.adsListener = adsListener;
        ResetParams();
        isUniqueBaseAds = false;
    }

    public void ResetParams()
    {
        sdkInitialized = false;
        isShownBanner = false;
        isBannerLoaded = false;
        loadBannerCalled = false;
        isInterstitialInitialized = false;
        isRewardedVideoInitialized = false;
        isInterstitialLoaded = false;
        isRewardedVideoLoaded = false;
        isInterstitialLoading = false;
        isRewardedvideoLoading = false;
        isShowInterstitial = false;
        isShowRewardedVideo = false;
        gotRewarded = false;
        
    }

    public virtual void InitBanner(bool forceShow)
    {

    }

    public virtual void InitInterstitial()
    {

    }

    public virtual void InitRewardedVideo()
    {

    }

    public virtual void LoadRewardedVideo()
    {

    }

    public virtual void LoadInterstitial()
    {

    }

    public virtual bool ShowRewardedVideo()
    {
        return false;
    }

    public virtual bool ShowInterstitial()
    {
        return false;
    }

    public virtual void ShowBanner(bool visible)
    {

    }
    
    public virtual void ShowOpenAdIfAvailable()
    {

    }

    public virtual void OnInterstitialLoaded()
    {

    }

    public virtual void OnInterstitialLoadFail()
    {

    }

    public virtual string GetSource()
    {
        return "BaseAds";
    }

    public virtual bool ValidateNetwork(string sourceCode)
    {
        return false;
    }
    public virtual void SetUniqueAds(bool unique)
    {
        Debug.Log($"{GetSource()} is unique ads {unique}");
        isUniqueBaseAds = unique;
    }

    public virtual void ShowFirstOpenAds()
    {
    }
}
