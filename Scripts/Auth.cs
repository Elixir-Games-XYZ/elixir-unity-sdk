using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace Elixir
{
	public class Auth : BaseWebService
	{
		private const string ElixirPrefsKey = "Elixir.RefreshToken";
		private static float _timeToRefreshToken;

		private static string RefreshToken
		{
			get => PlayerPrefs.GetString(ElixirPrefsKey);
			set => PlayerPrefs.SetString(ElixirPrefsKey, value);
		}

		public static string Token { get; private set; }

		public static async Task InitRei()
		{
#if UNITY_EDITOR
			// when running the game in the editor, we will not receive any rei from an external Elixir Launcher
			// in that case we will want to fetch a development rei key from the API
			if (string.IsNullOrEmpty(ElixirController.Instance.Rei))
			{
				var result = await Rei.GetDevelopmentReiKey();
				ElixirController.Log($"development reikey: {result.reikey}");
				ElixirController.Instance.Rei = result.reikey;
			}
#endif

			if (string.IsNullOrEmpty(ElixirController.Instance.Rei)) throw new Exception("rei is missing");

			try
			{
				var response = await GetAsync<TokenResponse>($"/sdk/auth/v2/session/reikey/{ElixirController.Instance.Rei}");
				Token = response.data.token;
				SaveRefreshToken(response.data.tokenLifeMS, response.data.refreshToken);
			}
			catch (Exception exception)
			{
				ElixirController.Log($"rei initialization failed: {exception.Message}");
				ClearRefreshToken();
				throw;
			}
		}

		[ItemCanBeNull]
		public static Task<TokenResponseData> Refresh()
		{
			return PerformTokenActionAsync(PostAsync<TokenResponse>("/sdk/auth/v2/session/refresh",
				new RefreshRequestBody { refreshToken = RefreshToken }));
		}

		private static void SaveRefreshToken(float tokenLifeMS, string refreshToken)
		{
			_timeToRefreshToken = tokenLifeMS / 1000 - 5;
			RefreshToken = refreshToken;
		}

		private static void ClearRefreshToken()
		{
			PlayerPrefs.DeleteKey("Elixir.RefreshToken");
		}

		public static async Task<CloseReiResponseData> CloseRei()
		{
			var response = await PostAsync<CloseReiResponse>($"/sdk/auth/v2/session/closerei/{ElixirController.Instance.Rei}",
				new CloseReiRequestBody { refreshToken = RefreshToken });
			ElixirController.Log("Ending session.");
			return response.data;
		}

		public static void CheckToken(float deltaTime)
		{
			if (_timeToRefreshToken > 0)
			{
				_timeToRefreshToken -= deltaTime;
				if (_timeToRefreshToken < 0)
				{
					_timeToRefreshToken = 0;
					Refresh();
				}
			}
		}

		private static async Task<TokenResponseData> PerformTokenActionAsync(Task<TokenResponse> tokenAction)
		{
			try
			{
				var response = await tokenAction;
				Token = response.data.token;
				SaveRefreshToken(response.data.tokenLifeMS, response.data.refreshToken);
				return response.data;
			}
			catch (Exception exception)
			{
				ElixirController.Log($"Failed to get token: {exception.Message}");
				ClearRefreshToken();
				throw;
			}
		}

		[Serializable]
		internal class CloseReiRequestBody
		{
			public string refreshToken;
		}

		[Serializable]
		internal class RefreshRequestBody
		{
			public string refreshToken;
		}

		[Serializable]
		public class CloseReiResponseData
		{
			public bool closed;
		}

		[Serializable]
		internal class CloseReiResponse : ElixirResponse
		{
			public CloseReiResponseData data = new CloseReiResponseData();
		}

		[Serializable]
		public class TokenResponseData
		{
			public string token;
			public ulong tokenExpiry;
			public ulong tokenLifeMS;
			public string refreshToken;
			public bool newAccount;
		}

		[Serializable]
		internal class TokenResponse : ElixirResponse
		{
			public TokenResponseData data = new TokenResponseData();
		}

#if UNITY_ANDROID || UNITY_IOS
		[Serializable]
		internal class QrVerifyRequestBody
		{
			public string deviceModel;

			public string deviceUniqueIdentifier;
			public string qrValue;
		}

		public static Task<TokenResponseData> QrVerify(string qrValue)
		{
			return PerformTokenActionAsync(PostAsync<TokenResponse>("/sdk/auth/v2/signin/qr-verify",
				new QrVerifyRequestBody
				{
					qrValue = qrValue,
					deviceModel = SystemInfo.deviceModel,
					deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier
				}));
		}

		[Serializable]
		internal class OtpVerifyRequestBody
		{
			public string code;
			public string deviceModel;
			public string deviceUniqueIdentifier;
			public string transactionId;
		}

		public static Task<TokenResponseData> OtpVerify(string transactionId, string code)
		{
			return PerformTokenActionAsync(PostAsync<TokenResponse>("/sdk/auth/v2/signin/otp-verify",
				new OtpVerifyRequestBody
				{
					transactionId = transactionId,
					code = code,
					deviceModel = SystemInfo.deviceModel,
					deviceUniqueIdentifier = SystemInfo.deviceUniqueIdentifier
				}));
		}

		[Serializable]
		public class OtpLoginResponseData
		{
			public string transactionId;
			public string userCode;
		}

		[Serializable]
		internal class OtpLoginResponse : ElixirResponse
		{
			public OtpLoginResponseData data = new OtpLoginResponseData();
		}

		[Serializable]
		internal class OtpLoginRequestBody
		{
			public string email;
		}

		public static async Task<OtpLoginResponseData> OtpLogin(string email)
		{
			if (string.IsNullOrEmpty(email)) throw new Exception("Missing email for OtpLogin");
			var response =
				await PostAsync<OtpLoginResponse>("/sdk/auth/v2/signin/otp-login", new OtpLoginRequestBody { email = email });
			return response.data;
		}

		public static bool IsLoggedIn()
		{
			return !string.IsNullOrEmpty(Token);
		}

		[Serializable]
		internal class LogoutResponse : ElixirResponse
		{
			public DataResponse data = new DataResponse();

			[Serializable]
			public class DataResponse
			{
				public string message;
			}
		}

		public static void Logout()
		{
			// We are very intentionally not waiting for logout to complete here
			if (!string.IsNullOrEmpty(Token)) PostAsync<LogoutResponse>("/sdk/auth/v2/session/signout", null);
			Token = null;
			ClearRefreshToken();
		}
#endif
	}
}