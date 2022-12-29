using System.Collections;

namespace Elixir{
    public class Tournaments : BaseWS {
        [System.Serializable]
        public class Data {
            public string _id;
            public string name;
            public string gameId;
            public string description;
            public string createdAt;
            public string modifiedAt;
            public string startsAt;
            public string endsAt;
            public string repeatEvery;
            public string location;
            public string eventUrl;
            public string userId;
            public string imageUrl;
            public string prizePool;
            public string type;
            public string visibility;
            public string rules;
            public string prizeDescription;
            public string settingsId;
            public string leaderboard;
        }
        [System.Serializable]
        class TournamentsDataResponse {
            public Data[] data;
        }
        static TournamentsDataResponse response = new TournamentsDataResponse();
        public static Data[] tournamentsData {  get { return response.data; } }
        public static IEnumerator Get() {
            yield return Get($"/sdk/v2/tournaments/", response);
        }
    }
}