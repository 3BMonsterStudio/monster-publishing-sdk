using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAdsListener {

    void OnInterstitialLoadFail(string source);
    void OnInterstitialSuccess(string source);
    void OnInterstitialFail(string source);
    void OnRewardedAdsLoadFail(string source);
    void OnRewardedAdsClose(string source, bool gotRewarded);
    void OnRewardedAdsFail(string source);
    void OnBannerLoadFail(string source);
    void OnAllSdkInitalized(string source);
}
