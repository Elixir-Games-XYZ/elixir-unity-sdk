using System;
using System.Threading.Tasks;

namespace Elixir
{
	internal class Rei : BaseWebService
	{
		public static async Task<ReiKeyResponseData> GetDevelopmentReiKey()
		{
			var response = await GetAsync<ReiKeyResponse>("/sdk/auth/v2/dev/reikey");
			return response.data;
		}

		[Serializable]
		public class ReiKeyResponseData
		{
			public string reikey;
			public string playerId;
		}

		[Serializable]
		private class ReiKeyResponse : ElixirResponse
		{
			public ReiKeyResponseData data;
		}
	}
}