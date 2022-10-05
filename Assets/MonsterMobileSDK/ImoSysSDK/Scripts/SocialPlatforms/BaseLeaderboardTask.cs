using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImoSysSDK.SocialPlatforms {
    public class BaseLeaderboardTask {
        protected int leaderboardId;

        public BaseLeaderboardTask(int leaderboardId) {
            this.leaderboardId = leaderboardId;
        }
    }
}
