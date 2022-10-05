using System.Collections;
using System.Collections.Generic;
#if UNITY_IOS
using System.Runtime.InteropServices;
#endif
using AOT;
using UnityEngine;
using UnityEngine.Events;

public class IOSHelper
{
    private static UnityAction<int, string> dcCallback;

    private delegate void DCReceivedCallback(int status, string token);
#if UNITY_IOS
    [MonoPInvokeCallback(typeof(DCReceivedCallback))]
    private static void delegateDCReceived(int status, string token)
    {
#if !ENV_PROD
        Debug.Log($"dc status = {status} and token is: {token}");
#endif
        dcCallback?.Invoke(status, token);
    }

    public static void RequestDCToken(UnityAction<int, string> callback)
    {
        dcCallback = callback;
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            SetDCTokenDelegate(delegateDCReceived);
            RequestDCDeviceToken();
        }
    }

    [DllImport("__Internal")]
    private static extern void SetDCTokenDelegate(DCReceivedCallback callback);

    [DllImport("__Internal")]
    private static extern void RequestDCDeviceToken();

    public static bool IsFBMessengerInstalled(){
        return true;
    }
#endif
}
