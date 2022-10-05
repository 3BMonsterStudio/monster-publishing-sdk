using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImoSysSDK.SocialPlatforms {

    [Serializable]
    public class LeaderboardResponse {
        public const string STATUS_NOT_STARTED = "not_started";
        public const string STATUS_FINISHED = "finished";
        public const string STATUS_ON_GOING = "ongoing";
        public const string STATUS_EXPIRED = "expired";
        public string status;
        public LeaderboardItem[] items;
    }
}
