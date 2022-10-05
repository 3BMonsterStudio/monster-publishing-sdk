#if UNITY_IPHONE || UNITY_IOS

using System.IO;

using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;
public static class SDKPListProcessor
{
    private static string[] ORIENTATIONS = new string[] { "UIInterfaceOrientationLandscapeRight", "UIInterfaceOrientationLandscapeLeft", "UIInterfaceOrientationPortraitUpsideDown", "UIInterfaceOrientationPortrait" };

    [PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
    {
#if UNITY_IPHONE || UNITY_IOS
        string plistPath = Path.Combine(path, "Info.plist");
        PlistDocument plist = new PlistDocument();
        plist.ReadFromFile(plistPath);
        var supportedOrientations = plist.root["UISupportedInterfaceOrientations"].AsArray();
        for (int i = 0; i < ORIENTATIONS.Length; i++)
        {
            bool found = false;
            for(int j = 0; j < supportedOrientations.values.Count; j++)
            {
                if (supportedOrientations.values[j].AsString().Equals(ORIENTATIONS[i])) {
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                supportedOrientations.AddString(ORIENTATIONS[i]);
            }
        }
        plist.root.SetString("GADApplicationIdentifier", GameSDKSettings.admobAppId);
        PlistElementArray bgModes = plist.root.CreateArray("SKAdNetworkIdentifier");
            bgModes.AddString("v9wttpbfk9.skadnetwork");
            bgModes.AddString("n38lu8286q.skadnetwork");
        File.WriteAllText(plistPath, plist.WriteToString());
#endif
    }
}

#endif
