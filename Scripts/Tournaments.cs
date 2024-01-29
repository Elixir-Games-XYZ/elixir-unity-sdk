using System;
using System.Threading.Tasks;

namespace Elixir
{
	public class Tournaments : BaseWebService
	{
		public static async Task<Tournament[]> GetTournaments()
		{
			var response = await GetAsync<TournamentsResponse>("/sdk/v2/tournaments/");
			return response.data;
		}

		[Serializable]
		public class Tournament
		{
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

		[Serializable]
		private class TournamentsResponse : ElixirResponse
		{
			public Tournament[] data;
		}
	}
}