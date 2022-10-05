using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.Collections;
using System.IO;

public class EntitlementsPostProcess : ScriptableObject
{
    private const string ENTITLEMENT_CONTENT = "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n<!DOCTYPE plist PUBLIC \"-//Apple//DTD PLIST 1.0//EN\" \"http://www.apple.com/DTDs/PropertyList-1.0.dtd\">\r\n<plist version=\"1.0\">\r\n<dict>\r\n\t<key>aps-environment</key>\r\n\t<string>production</string>\r\n</dict>\r\n</plist>";


#if UNITY_IOS
    [PostProcessBuild]
    public static void OnPostProcess(BuildTarget buildTarget, string buildPath) {
        if (buildTarget != BuildTarget.iOS) {
            return;
        }


        var proj_path = PBXProject.GetPBXProjectPath(buildPath);
        var proj = new PBXProject();
        proj.ReadFromFile(proj_path);

        // target_name = "Unity-iPhone"
#if UNITY_2019_3_OR_NEWER
        var target_guid = proj.GetUnityMainTargetGuid();
        var target_name = "Unity-iPhone";
        var frameworkGuid = proj.GetUnityFrameworkTargetGuid();
#else
        var target_name = PBXProject.GetUnityTargetName();
        var target_guid = proj.TargetGuidByName(target_name);
#endif
        string filename = "Notification.entitlements";
        var dst = buildPath + "/" + target_name + "/" + filename;
        StreamWriter writer = new StreamWriter(dst);
        writer.Write(ENTITLEMENT_CONTENT);
        writer.Close();
        string entitlementPath = target_name + "/" + filename;
        proj.AddFile(entitlementPath, filename);
        proj.AddBuildProperty(target_guid, "CODE_SIGN_ENTITLEMENTS", entitlementPath);        
        proj.AddFrameworkToProject(frameworkGuid, "DeviceCheck.framework", false);
        proj.WriteToFile(proj_path);

        var capacilities = new ProjectCapabilityManager(proj_path, entitlementPath, "Unity-iPhone");
        //capacilities.AddSignInWithApple();
        //capacilities.AddAssociatedDomains(new string[] { "applinks:skyraptor.page.link" });
        capacilities.AddBackgroundModes(BackgroundModesOptions.RemoteNotifications);
        capacilities.AddPushNotifications(Debug.isDebugBuild);
        capacilities.WriteToFile();

        // Edit plist
        // Get plist
        string plistPath = buildPath + "/Info.plist";
        PlistDocument plist = new PlistDocument();
        plist.ReadFromString(File.ReadAllText(plistPath));

        // Get root
        PlistElementDict rootDict = plist.root;

        // Set encryption usage boolean
        // remove exit on suspend if it exists.
        string exitsOnSuspendKey = "UIApplicationExitsOnSuspend";
        if (rootDict.values.ContainsKey(exitsOnSuspendKey)) {
            rootDict.values.Remove(exitsOnSuspendKey);
        }
        // Write to file
        File.WriteAllText(plistPath, plist.WriteToString());
    }
#endif
}
