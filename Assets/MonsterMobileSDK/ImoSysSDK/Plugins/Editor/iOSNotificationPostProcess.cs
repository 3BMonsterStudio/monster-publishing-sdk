using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using UnityEditor;
using System.IO;

public class iOSNotificationPostProcess
{
    [PostProcessBuild]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string path)
    {
#if PLATFORM_IOS
        if (buildTarget == BuildTarget.iOS)
        {

            //Get Preprocessor.h
            var preprocessorPath = path + "/Classes/Preprocessor.h";
            var preprocessor = File.ReadAllText(preprocessorPath);

            preprocessor = preprocessor.Replace("UNITY_USES_REMOTE_NOTIFICATIONS 0", "UNITY_USES_REMOTE_NOTIFICATIONS 1");
            File.WriteAllText(preprocessorPath, preprocessor);
        }
#endif

    }
}
