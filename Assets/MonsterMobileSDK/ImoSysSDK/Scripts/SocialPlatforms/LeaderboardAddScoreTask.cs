using ImoSysSDK.Network;
using ImoSysSDK.SocialPlatforms;
using Newtonsoft.Json.Linq;

namespace ImoSysSDK.SocialPlatforms {
    public class LeaderboardAddScoreTask : BaseLeaderboardTask {
        private const string PATH = "/v3/games/leaderboards/{0}/add";

        public delegate void OnLeaderboardAddScoreFailed(string message);
        public delegate void OnLeaderboardAddScoreSuccess();

        public event OnLeaderboardAddScoreFailed onLeaderboardAddScoreFailed;
        public event OnLeaderboardAddScoreSuccess onLeaderboardAddScoreSuccess;

        public LeaderboardAddScoreTask(int leaderboardId, OnLeaderboardAddScoreFailed onLeaderboardAddScoreFailed, OnLeaderboardAddScoreSuccess onLeaderboardAddScoreSuccess) : base(leaderboardId){
            this.onLeaderboardAddScoreFailed = onLeaderboardAddScoreFailed;
            this.onLeaderboardAddScoreSuccess = onLeaderboardAddScoreSuccess;
        }

        public void AddScore(int? clazz, long score, string jsonMetadata) {
            string path = string.Format(PATH, leaderboardId);
            JObject body = new JObject();
            if (clazz != null) {
                body["class"] = clazz;
            }
            if (jsonMetadata != null) {
                body["metadata"] = JObject.Parse(jsonMetadata);
            }
            body["score"] = score;
            body["playerId"] = MonsterPlayerPrefs.PlayerId;
            RestClient.SendPostRequest(path, body.ToString(), OnRequestFinished);
        }

        private void OnRequestFinished(long statusCode, string message, string data) {
            if (statusCode == 200) {
                OnLeaderboardAddScoreSuccessCallback();
            } else {
                OnLeaderboardAddScoreFailedCallback(message);
            }
        }

        private void OnLeaderboardAddScoreFailedCallback(string message) {
            if (onLeaderboardAddScoreFailed != null) {
                onLeaderboardAddScoreFailed(message);
                onLeaderboardAddScoreFailed = null;
            }
        }

        private void OnLeaderboardAddScoreSuccessCallback() {
            if (onLeaderboardAddScoreSuccess != null) {
                onLeaderboardAddScoreSuccess();
                onLeaderboardAddScoreSuccess = null;
            }
        }
    }
}
