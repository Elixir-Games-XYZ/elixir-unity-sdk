using System;
using System.Threading.Tasks;

namespace Elixir
{
	public class User : BaseWebService
	{
		private UserInfoResponseData _userInfo;

		public static async Task<UserInfoResponseData> GetUserInfo()
		{
			var response = await GetAsync<UserInfoResponse>("/sdk/v2/userinfo");
			return response.data;
		}

		[Serializable]
		public class UserInfoResponseData
		{
			public string sub; // ElixirId
			public string iss;
			public string[] wallets;
			public string nickname;
			public string picture;
			public string aud;
			public string status;
		}

		[Serializable]
		private class UserInfoResponse : ElixirResponse
		{
			public UserInfoResponseData data;
		}
	}
}