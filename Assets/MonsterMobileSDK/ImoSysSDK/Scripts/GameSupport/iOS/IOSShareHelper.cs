using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using AOT;
using UnityEngine;
using UnityEngine.Events;

public class IOSShareHelper
{
#if UNITY_IOS

    [DllImport("__Internal")]
    public static extern void NativeShareToOthers(string url);

    [DllImport("__Internal")]
    public static extern void NativeShareToFBMessenger(string url);

#endif
}
