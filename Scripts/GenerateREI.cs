using System.Collections;

namespace Elixir {
    internal class GenerateREI : BaseWS {
        [System.Serializable]
        class GenerateREIResponse {
            [System.Serializable]
            public class Data {
                public string reikey;
                public string playerId;
            }
            public Data data;
        }
        static GenerateREIResponse response = new GenerateREIResponse();
        
        public static IEnumerator Do() {
            yield return Get($"/sdk/auth/v2/dev/reikey", response);
            ElixirController.Instance.rei = response.data.reikey;
        }
    }
}