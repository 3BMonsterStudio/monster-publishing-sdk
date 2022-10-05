
using UnityEngine;

namespace ImoSysSDK.GameSupport
{
    public class AndroidShareHelper
    {
        private static AndroidJavaClass nativeAndroidShareHelper;

        public static void ShareToFBMessenger(string url)
        {
#if !UNITY_EDITOR
        ensureAndroidShareHelperClassCreated();
        nativeAndroidShareHelper.CallStatic("shareLinkToMessenger", url);
#endif
        }

        public static void ShareToOthers(string url)
        {
#if !UNITY_EDITOR
        ensureAndroidShareHelperClassCreated();
        nativeAndroidShareHelper.CallStatic("shareLinkToOthers", url);
#endif
        }

        private static void ensureAndroidShareHelperClassCreated()
        {
            if (nativeAndroidShareHelper == null)
            {
                nativeAndroidShareHelper = new AndroidJavaClass("com.monster.unity.gamesupport.utils.AndroidShareHelper");
            }
        }
    }
}