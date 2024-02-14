using System;
using UnityEngine;
#if !UNITY_ANDROID && !UNITY_IOS
using Elixir.Overlay;
using Event = Elixir.Overlay.Event;
#endif

namespace Elixir
{
	public class ElixirController : MonoBehaviour
	{
		private static ElixirController _instance;
#if !UNITY_EDITOR
		private static bool _readyToQuit;
#endif

		internal string Rei { get; set; }

		public static ElixirController Instance
		{
			get
			{
				if (_instance == null)
				{
					var gameObject = new GameObject("ElixirController");
					_instance = gameObject.AddComponent<ElixirController>();
					DontDestroyOnLoad(gameObject);
				}

				return _instance;
			}
		}

		public static bool DebugLog { get; set; }

		private void Update()
		{
			Auth.CheckToken(Time.deltaTime);
#if !UNITY_ANDROID && !UNITY_IOS
			OverlayMessage.Update();
#endif
		}

		[RuntimeInitializeOnLoadMethod]
		private static void RunOnStart()
		{
			Application.wantsToQuit += WantsToQuit;
		}

		private static bool WantsToQuit()
		{
#if UNITY_EDITOR
			Auth.CloseRei(Instance.Rei);
			return true;
#else
			CloseReiAndQuit();
			return _readyToQuit;
#endif
		}

#if !UNITY_EDITOR
		private static async void CloseReiAndQuit()
		{
			try
			{
				await Auth.CloseRei(Instance.Rei);
			}
			catch (Exception)
			{
				Log("CloseRei failed");
			}
			finally
			{
				_readyToQuit = true;
				Application.Quit();
			}
		}
#endif

		public bool PrepareElixir(string apiKey)
		{
			BaseWebService.apiKey = apiKey;
			InitReiProgramArgument();
#if !UNITY_ANDROID && !UNITY_IOS
			OverlayMessage.Init(ProcessMessage);
#endif
			return true;
		}

		// the rei argument comes from the Elixir Launcher on desktop platforms
		private void InitReiProgramArgument()
		{
			var args = Environment.GetCommandLineArgs();
			for (var i = 0; i < args.Length; i++)
				if (args[i] == "-rei")
					Rei = args[i + 1];
		}

		public static void Log(string log)
		{
			if (DebugLog) Debug.Log($"<color=#a0a000>[Elixir] {log}</color>");
		}

#if !UNITY_ANDROID && !UNITY_IOS
		private void ProcessMessage(IMessage message)
		{
			Log("Event received");
			switch (message)
			{
				case MOpenStateChange openStateChange:
					Log($"MOpenStateChange: {openStateChange.IsOpen}");
					Event.OnOpenStateChange(openStateChange.IsOpen);
					break;
				case MCheckoutResult checkoutResult:
					Log($"MCheckoutResult: {checkoutResult.Success}, {checkoutResult.Sku}");
					Event.OnCheckoutResult(checkoutResult.Success, checkoutResult.Sku);
					break;
				default:
					Log("Event not processed");
					break;
			}
		}

		public void OnApplicationQuit()
		{
			OverlayMessage.StopListening();
		}
#endif
	}
}