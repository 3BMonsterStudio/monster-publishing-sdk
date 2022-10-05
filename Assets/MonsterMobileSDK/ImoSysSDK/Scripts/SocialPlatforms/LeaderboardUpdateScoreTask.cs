using UnityEngine;
using System.Collections;
using ImoSysSDK.Network;
using System;
using Newtonsoft.Json.Linq;

namespace ImoSysSDK.SocialPlatforms {

    public class LeaderboardUpdateScoreTask {

        private const string PATH = "/v3/games/leaderboards/{0}/update";

        private int leaderboardId;

        public delegate void OnUpdateScoreSuccess();

        public delegate void OnUpdateScoreFailed(string message);

        private event OnUpdateScoreFailed onUpdateScoreFailed;
        private event OnUpdateScoreSuccess onUpdateScoreSuccess;

        public LeaderboardUpdateScoreTask(int leaderboardId, OnUpdateScoreFailed onUpdateScoreFailed, OnUpdateScoreSuccess onUpdateScoreSuccess) {
            this.leaderboardId = leaderboardId;
            this.onUpdateScoreFailed = onUpdateScoreFailed;
            this.onUpdateScoreSuccess = onUpdateScoreSuccess;
        }

        public void UpdateScore(long score, int? clazz, string jsonMetadata) {
            JObject body = new JObject();
            body["playerId"] = MonsterPlayerPrefs.PlayerId;
            body["score"] = score;
            if (jsonMetadata != null) {
                body["metadata"] = JObject.Parse(jsonMetadata);
            }
            if (clazz != null) {
                body["class"] = clazz;
            }
            RestClient.SendPostRequest(string.Format(PATH, this.leaderboardId), body.ToString(), OnRequestFinished);
        }

        private void OnRequestFinished(long statusCode, string message, string data) {
            Debug.Log("status code: " + statusCode);
            if (statusCode == 200) {
                OnUpdateScoreSuccessCallback();
            } else {
                OnUpdateScoreFailedCallback(message);
            }
        }

        private void OnUpdateScoreFailedCallback(string message) {
            if (onUpdateScoreFailed != null) {
                onUpdateScoreFailed(message);
                onUpdateScoreFailed = null;
            }
        }

        private void OnUpdateScoreSuccessCallback() {
            if (onUpdateScoreSuccess != null) {
                onUpdateScoreSuccess();
                onUpdateScoreSuccess = null;
            }
        }

    }
}