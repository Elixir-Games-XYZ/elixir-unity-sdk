using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Elixir
{
	public class BaseWebService
	{
		protected const string BaseURL = "https://kend.elixir.app";
		internal static string apiKey;

		internal static Task<T> GetAsync<T>(string uri) where T : ElixirResponse
		{
			return MakeRequestAsync<T>(HttpMethod.Get, uri, null);
		}

		internal static Task<T> PostAsync<T>(string uri, object body) where T : ElixirResponse
		{
			var bodyText = body != null ? JsonUtility.ToJson(body) : "{}";
			return MakeRequestAsync<T>(HttpMethod.Post, uri, bodyText);
		}

		private static async Task<T> MakeRequestAsync<T>(HttpMethod httpMethod, string uri, string body)
			where T : ElixirResponse
		{
			using var www = CreateWebRequest(httpMethod, uri, body);
			await SendWebRequestTask(www);

			if (www.result != UnityWebRequest.Result.Success)
				throw new Exception($"{www.error} -> {www.downloadHandler?.text}");

			var json = JsonUtility.FromJson<T>(www.downloadHandler.text);
			ElixirController.Log($"Elixir.Response({uri}) {www.downloadHandler.text}");
			if (www.responseCode != 200 && json.error != null)
				throw new ElixirApiException(www.responseCode, json.error.code, json.error.message);

			return json;
		}

		private static UnityWebRequest CreateWebRequest(HttpMethod httpMethod, string uri, string body)
		{
			if (string.IsNullOrEmpty(apiKey)) throw new Exception("Missing apikey");

			UnityWebRequest www;
			if (httpMethod == HttpMethod.Get)
			{
				ElixirController.Log($"GET {uri}");
				www = UnityWebRequest.Get($"{BaseURL}{uri}");
			}
			else
			{
				ElixirController.Log($"POST {uri}: {body}");
				www = new UnityWebRequest($"{BaseURL}{uri}", "POST")
				{
					uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body)),
					downloadHandler = new DownloadHandlerBuffer()
				};
				www.SetRequestHeader("Content-Type", "application/json");
			}

			www.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
			www.SetRequestHeader("Expires", "0");
			www.SetRequestHeader("x-api-key", apiKey);
			if (!string.IsNullOrEmpty(Auth.Token))
				www.SetRequestHeader("authorization", "Bearer " + Auth.Token);

			return www;
		}

		protected static Task<UnityWebRequest> SendWebRequestTask(UnityWebRequest webRequest)
		{
			var completionSource = new TaskCompletionSource<UnityWebRequest>();
			webRequest.SendWebRequest().completed += operation => { completionSource.SetResult(webRequest); };
			return completionSource.Task;
		}
	}

	[Serializable]
	internal class ElixirResponse
	{
		public int code;
		public bool success;
		public ElixirResponseError error;

		[Serializable]
		internal class ElixirResponseError
		{
			public int code;
			public string message;
			public int status;
		}
	}

	public class ElixirApiException : Exception
	{
		public ElixirApiException(long status, int code, string message)
			: base(message)
		{
			Status = status;
			Code = code;
		}

		public long Status { get; private set; }
		public int Code { get; private set; }
	}
}