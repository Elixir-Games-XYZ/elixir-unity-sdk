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
            Auth.token = "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJfaWQiOiI5YmM4MTgzOC00NTY0LTQ4M2UtYmE2MC1iY2JiZjE2YmM1MjQiLCJlbWFpbCI6ImZlcm5hbmRvdGVzdDFAc2F0b3NoaXMuZ2FtZXMiLCJlbnZpcm9ubWVudCI6ImVsaXhpciIsImlhdCI6MTY2ODg1OTI0MiwiZXhwIjoxNzI4ODU5MjQyfQ._S9OJMUaUzkKZRJdiVjeGSzHzqFZhMWIUY3BmZla5etMecSRGFDH6BwRj3zEGygof9pVVZlkrYMiHGpKGcZUxQ";
            yield return Get($"/dev/reikey/{GameID}", response);
            ElixirController.Instance.rei = response.reikey;
        }
    }
}

#endif
