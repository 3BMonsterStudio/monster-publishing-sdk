using ImoSysSDK.Network;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ImoSysSDK.SocialPlatforms {
    class RenamePlayerTask {
        private const string PATH = "/v3/games/players/name";

        public delegate void OnRenamePlayerSuccess();

        public delegate void OnRenamePlayerFailed(string message);

        private event OnRenamePlayerFailed onRenamePlayerFailed;

        private event OnRenamePlayerSuccess onRenamePlayerSuccess;

        public RenamePlayerTask(OnRenamePlayerFailed onRenamePlayerFailed, OnRenamePlayerSuccess onRenamePlayerSuccess) {
            this.onRenamePlayerFailed = onRenamePlayerFailed;
            this.onRenamePlayerSuccess = onRenamePlayerSuccess;
        }

        public void Rename(string newName) {
            string path = string.Format(PATH);
            JObject body = new JObject();
            body["playerId"] = MonsterPlayerPrefs.PlayerId;
            body["name"] = newName;
            RestClient.SendPostRequest(path, body.ToString(), OnRequestFinished);
        }

        private void OnRequestFinished(long statusCode, string message, string data) {
            if (statusCode == 200) {
                OnRenamePlayerSuccessCallback();
            } else {
                OnRenamePlayerFailedCallback(message);
            }
        }

        private void OnRenamePlayerFailedCallback(string message) {
            if (onRenamePlayerFailed != null) {
                onRenamePlayerFailed(message);
                onRenamePlayerFailed = null;
            }
        }

        private void OnRenamePlayerSuccessCallback() {
            if (onRenamePlayerSuccess != null) {
                onRenamePlayerSuccess();
                onRenamePlayerSuccess = null;
            }
        }
    }
}
