using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImoSysSDK.Others {
    public class NotchUtil {
        public static int hasDeviceNotch() {
#if !UNITY_EDITOR
#if UNITY_ANDROID
        AndroidJavaClass javaNotchUtil = new AndroidJavaClass("com.monster.unity.gamesupport.utils.NotchUtil");
        AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        return javaNotchUtil.CallStatic<int>("hasDeviceNotch", activity);
#else
        return -1;
#endif
#else
            return -1;
#endif
        }
    }
}