using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace ImoSysSDK.SocialPlatforms {

    [Serializable]
    public class LeaderboardItem {
        public int playerId;
        public string name;
        public string avatarUrl;
        [JsonProperty(PropertyName = "class")]
        public int? clazz;
        public long score;
        public int rank;
        public string countryCode;
        public JObject metadata;
    }
}