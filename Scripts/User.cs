using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Elixir{
    public class User : BaseWS {
        [System.Serializable]       
        public class UserData {
            public string   sub; // ElixirId
            public string   iss;
            public string[] wallets;
            public string   nickname;
            public string   picture;
            public string   aud;
            public string   status;
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