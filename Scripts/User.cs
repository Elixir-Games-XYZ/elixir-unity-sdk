using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elixir{
    public class User : BaseWS {
        [System.Serializable]
        public class UserData {
            public string elixirId;
            public string nickname;
            public string avatar;
            public string wallet;
        }
        [System.Serializable]
        class UserDataResponse {
            public UserData data;
        }
        static UserDataResponse responseUserData = new UserDataResponse();
        public static UserData userData {  get { return responseUserData.data; } }

        public static IEnumerator Get() {
            yield return Get($"/sdk/v2/userinfo/", responseUserData);
        }
    }
}