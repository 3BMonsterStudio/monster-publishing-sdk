using System.Collections;
using System.Collections.Generic;
using ImoSysSDK.Network;
using Newtonsoft.Json.Linq;
using UnityEngine;
namespace ImoSysSDK.SocialPlatforms {
    public class ReportPlayerTask
    {
        private const string PATH = "/v3/games/players";

        public delegate void OnReportPlayerFinished(bool success, int playerId, string deviceId, string name);

        private event OnReportPlayerFinished onReportPlayerFinished;

        public ReportPlayerTask(OnReportPlayerFinished onReportPlayerFinished) {
            this.onReportPlayerFinished = onReportPlayerFinished;
        }

        public void Report(string advertisingId) {
            JObject body = new JObject();
            body["advertisingId"] = advertisingId;
            body["name"] = MonsterPlayerPrefs.PlayerName;
            body["dModel"] = SystemInfo.deviceModel;
            body["dName"] = SystemInfo.deviceName;
            body["dIdentifier"] = SystemInfo.deviceUniqueIdentifier;
            body["dOs"] = SystemInfo.operatingSystem;
            body["dCpuCount"] = SystemInfo.processorCount;
            body["dMemory"] = SystemInfo.systemMemorySize;
            body["dLanguage"] = Application.systemLanguage.ToString();
            body["afid"] = MonsterPlayerPrefs.AppsFlyerId;
            RestClient.SendPostRequest(PATH, body.ToString(), OnRequestFinished);
        }

        private void OnRequestFinished(long statusCode, string message, string json)
        {
            Debug.Log("ReportPlayerTask " + statusCode + " " + message);
            if (statusCode == 200)
            {
                JObject data = JObject.Parse(json);
                int playerId = data["playerId"].Value<int>();
                string deviceId = data["deviceId"].Value<string>();
                string name = data["name"].Value<string>();
                onReportPlayerFinished(true, playerId, deviceId, name);
            }
            else {
                onReportPlayerFinished(false, -1, null, null);
            }
        }

    }
}
