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
		private Texture2D _background;
		private string _consoleText = "";
		private bool _isConsoleOpen;
		private GUIStyle _label;
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

		public static bool UseConsole { get; set; }

		private void Update()
		{
			Auth.CheckToken(Time.deltaTime);
#if !ENABLE_INPUT_SYSTEM
			if (UseConsole && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape))
				_isConsoleOpen = !_isConsoleOpen;
#endif
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
			Auth.CloseRei();
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
				await Auth.CloseRei();
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

		private void OnGUI()
		{
			if (_isConsoleOpen)
			{
				if (_label == null)
				{
					_background = new Texture2D(1, 1);
					_background.SetPixel(0, 0, Color.white * 0.85f);
					_background.Apply();

					_label = new GUIStyle();
					_label.normal.textColor = Color.black;
					_label.fontSize = 24;
					_label.normal.background = _background;
				}

				GUI.Label(new Rect(0, 0, Screen.width, Screen.height), _consoleText, _label);
			}
		}

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
					Instance.Rei = args[i + 1];
		}

		public static void Log(string log)
		{
			if (UseConsole)
			{
				Debug.Log($"<color=#a0a000>[Elixir] {log}</color>");
				Instance._consoleText += log + "\n";
			}
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