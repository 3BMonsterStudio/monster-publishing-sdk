using UnityEngine;
using System.Collections;
using System;

namespace ImoSysSDK.Others {

    public class JsonHelper {
        public static T[] FromJson<T>(string json) {
            string wrapperJson = string.Format("{{ \"items\": {0}}}", json);
            Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrapperJson);
            return wrapper.items;
        }

        [Serializable]
        private class Wrapper<T> {
            public T[] items;
        }
    }
}