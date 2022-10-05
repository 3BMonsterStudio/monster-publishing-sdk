
#if UNITY_IOS

using UnityEngine;
using System.Runtime.InteropServices;
using System;

namespace AudienceNetwork
{
    public static class AdSettings
    {
        [DllImport("__Internal")]
        private static extern void FBAdSettingsBridgeSetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled);

        public static void SetAdvertiserTrackingEnabled(bool advertiserTrackingEnabled)
        {
            try
            {
                FBAdSettingsBridgeSetAdvertiserTrackingEnabled(advertiserTrackingEnabled);
            }
            catch (Exception e)
            {

            }

            
        }
    }
}

#endif