using System;
using UnityEngine;

namespace Elixir
{
	public class ElixirController : MonoBehaviour
	{
		private static ElixirController _instance;
		private Texture2D _background;
		private string _consoleText = "";
		private bool _isConsoleOpen;
		private GUIStyle _label;

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
		}

		private void OnDestroy()
		{
			Auth.CloseRei();
		}

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
	}
}