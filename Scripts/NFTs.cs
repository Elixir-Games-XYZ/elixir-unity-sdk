using System;
using System.Threading.Tasks;

namespace Elixir
{
	public class Nfts : BaseWebService
	{
		public static Collection[] Collections { get; }

		public static async Task<Collection[]> GetUserNfts()
		{
			var response = await GetAsync<NftsResponse>("/sdk/v2/nfts/user");
			return response.data;
		}

		[Serializable]
		public class Collection
		{
			public string collection;
			public string collectionName;
			public Nft[] nfts;

			[Serializable]
			public class Nft
			{
				public string tokenId;
				public string name;
				public string image;
				public Attribute[] attributes;

				[Serializable]
				public class Attribute
				{
					public string trait_type;
					public string value;
				}
			}
		}

		[Serializable]
		private class NftsResponse : ElixirResponse
		{
			public Collection[] data;
		}
	}
}