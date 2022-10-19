using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Elixir
{
    public class ElixirController : MonoBehaviour
    {
        static ElixirController _Instance;
        public static ElixirController Instance {
            get {
                if (_Instance == null) {
                    var gameObject = new GameObject("ElixirController");
                    _Instance = gameObject.AddComponent<ElixirController>();
                    DontDestroyOnLoad(gameObject);
                }
                return _Instance;
            }
        }

        public static bool useconsole { get; private set; }
 
        internal string rei;

        public delegate void analyticsEvent(string eventName, float value = 0);
        public static analyticsEvent AnalyticsEvent;
        bool isDevelop = false;
        public string PlayerPrefsKey { get; private set; }
        public bool PrepareElixir() {
            var elixirDescriptor = Resources.Load<ElixirDescriptor>("ElixirDescriptor");
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Instance.isDevelop = true;
#else
            Instance.isDevelop = false;
#endif
            PlayerPrefsKey = "Elixir.RefreshToken." + (Instance.isDevelop ? "Dev" : "Prod");

#if UNITY_EDITOR
            switch (elixirDescriptor.EditorEnv) {
                case ElixirDescriptor.Environments.Dev:
                    BaseWS.APIKEY = elixirDescriptor.DevAPIKey;
                    BaseWS.GameID = elixirDescriptor.GameID;
                    BaseWS.baseURL = "https://sandbox.elixir.app";
                    BaseWS.hmac = new HMACSHA256(Encoding.ASCII.GetBytes("nIhnQDqV6NYN5bYxhFOh4mpOU43fIj6f"));
                    useconsole = elixirDescriptor.useconsole;
                    break;
                case ElixirDescriptor.Environments.Prod:
                    BaseWS.APIKEY = elixirDescriptor.ProdAPIKey;
                    BaseWS.GameID = elixirDescriptor.GameID;
                    BaseWS.baseURL = "https://kend.elixir.app";
                    BaseWS.hmac = new HMACSHA256(Encoding.ASCII.GetBytes("kiu84SHMmIKGjDnIWxH7ICySrcDLB06b"));
                    useconsole = elixirDescriptor.useconsole;
                    break;
            }
#else
            switch (elixirDescriptor.BuildEnv) {
                case ElixirDescriptor.Environments.Dev:
                    BaseWS.APIKEY = elixirDescriptor.DevAPIKey;
                    BaseWS.GameID = elixirDescriptor.GameID;
                    BaseWS.baseURL = "https://sandbox.elixir.app";
                    BaseWS.hmac = new HMACSHA256(Encoding.ASCII.GetBytes("nIhnQDqV6NYN5bYxhFOh4mpOU43fIj6f"));
                    useconsole = elixirDescriptor.useconsole;
                    break;
                case ElixirDescriptor.Environments.Prod:
                    BaseWS.APIKEY = elixirDescriptor.ProdAPIKey;
                    BaseWS.GameID = elixirDescriptor.GameID;
                    BaseWS.baseURL = "https://kend.elixir.app";
                    BaseWS.hmac = new HMACSHA256(Encoding.ASCII.GetBytes("kiu84SHMmIKGjDnIWxH7ICySrcDLB06b"));
                    useconsole = elixirDescriptor.useconsole;
                    break;
            }
#endif
            // Check for REIKey parameter (-rei)
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
                if (args[i] == "-rei")
                    Instance.rei = args[i + 1];
            return true;
        }

        void Update() {
            Auth.CheckToken(Time.deltaTime);
#if !ENABLE_INPUT_SYSTEM
            if (useconsole && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Escape))
                showedConsole = !showedConsole;
#endif
        }

        void OnDestroy() {
            Auth.Close();
        }
        string consoleText = "";
        bool showedConsole = false;
        private void OnGUI() {
            if (showedConsole)
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), consoleText);
        }
        public static void Log(string log) {
            if (useconsole) {
                Debug.Log($"<color=#a0a000>[Elixir] {log}</color>");
                Instance.consoleText += log + "\n";
            }

        }
    }
}