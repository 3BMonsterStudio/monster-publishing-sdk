using UnityEngine;
using System.Collections;

namespace ImoSysSDK.GameSupport{
    public class AndroidHelper
    {
        private static AndroidJavaClass nativeAndroidHelper;

        public static bool IsFBMessengerInstalled() {
            ensureAndroidHelperClassCreated();
#if UNITY_EDITOR
            return true;
#else
            return nativeAndroidHelper.CallStatic<bool>("isFBMessengerInstalled");
#endif
        }

        public static bool IsFBInstalled()
        {
            ensureAndroidHelperClassCreated();
#if UNITY_EDITOR
            return true;
#elif UNITY_ANDROID
            return nativeAndroidHelper.CallStatic<bool>("isFBInstalled");
#else
            return false;
#endif
        }

        private static void ensureAndroidHelperClassCreated()
        {
#if !UNITY_EDITOR
            if (nativeAndroidHelper == null)
            {
                nativeAndroidHelper = new AndroidJavaClass("com.monster.unity.gamesupport.utils.AndroidHelper");
            }
#endif
        }

    }
}
