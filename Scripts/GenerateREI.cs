#if UNITY_EDITOR

using System.Collections;

namespace Elixir {
    internal class GenerateREI : BaseWS {
        [System.Serializable]
        class GenerateREIResponse {
            public string reikey;
        }
        static GenerateREIResponse response = new GenerateREIResponse();
        
        public static IEnumerator Do() {
            Auth.token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJfaWQiOiI5YmM4MTgzOC00NTY0LTQ4M2UtYmE2MC1iY2JiZjE2YmM1MjQiLCJlbWFpbCI6ImZlcm5hbmRvdGVzdDFAc2F0b3NoaXMuZ2FtZXMiLCJpYXQiOjE2Mjc0ODUxODYsImV4cCI6MjE1NTQ4NTE4Nn0.SAkEr9-CWwnLSNup7X1d9E3vmSj3mlPQA_5Z2p54m__hPsL765xFxvUvjRpKwMhxrAE9QirAodP7868_yA0s9A";
            yield return Get($"/dev/reikey/{GameID}", response);
            ElixirController.Instance.rei = response.reikey;
        }
    }
}

#endif