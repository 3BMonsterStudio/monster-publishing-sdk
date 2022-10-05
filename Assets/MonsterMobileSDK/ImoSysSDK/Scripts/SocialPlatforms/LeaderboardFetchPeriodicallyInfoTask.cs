using ImoSysSDK.Network;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ImoSysSDK.SocialPlatforms {
    public class LeaderboardFetchPeriodicallyInfoTask : BaseLeaderboardTask{
        private const string PATH = "/v3/games/leaderboards/{0}/scopes/periodically/info";

        private Action<string> onLeaderboardFetchPeriodicallyInfoFailed;
        private Action<PeriodicallyLeaderboardInfo> onLeaderboardFetchPeriodicallyInfoSuccess;

        public LeaderboardFetchPeriodicallyInfoTask(int leaderboardId, Action<string> onLeaderboardFetchPeriodicallyInfoFailed, Action<PeriodicallyLeaderboardInfo> onLeaderboardFetchPeriodicallyInfoSuccess) : base(leaderboardId) {
            this.onLeaderboardFetchPeriodicallyInfoFailed = onLeaderboardFetchPeriodicallyInfoFailed;
            this.onLeaderboardFetchPeriodicallyInfoSuccess = onLeaderboardFetchPeriodicallyInfoSuccess;
        }

        public void FetchInfo() {
            string path = string.Format(PATH, leaderboardId);
            RestClient.SendGetRequest(path, null, OnRequestFinished);
        }

        private void OnRequestFinished(long statusCode, string message, string data) {
            if (statusCode == 200) {
                if (onLeaderboardFetchPeriodicallyInfoSuccess != null) {
                    onLeaderboardFetchPeriodicallyInfoSuccess(JsonConvert.DeserializeObject<PeriodicallyLeaderboardInfo>(data));
                    onLeaderboardFetchPeriodicallyInfoSuccess = null;
                }
            } else {
                if (onLeaderboardFetchPeriodicallyInfoFailed != null) {
                    onLeaderboardFetchPeriodicallyInfoFailed(message);
                    onLeaderboardFetchPeriodicallyInfoFailed = null;
                }
            }
        }
    }
}
