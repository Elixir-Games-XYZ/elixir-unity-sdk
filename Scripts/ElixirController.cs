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

        public static bool useconsole { get; set; }
 
        internal string rei;

        public delegate void analyticsEvent(string eventName, float value = 0);
        public static analyticsEvent AnalyticsEvent;
        bool isDevelop = false;
        public bool PrepareElixir(string APIKey) {
            BaseWS.APIKEY = APIKey;
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
        GUIStyle label;
        Texture2D background;
        private void OnGUI() {
            if (showedConsole) {
                if (label == null) {
                    background = new Texture2D(1, 1);
                    background.SetPixel(0, 0, Color.white * 0.85f);
                    background.Apply();

                    label = new GUIStyle();
                    label.normal.textColor = Color.black;
                    label.fontSize = 24;
                    label.normal.background = background;
                }
                GUI.Label(new Rect(0, 0, Screen.width, Screen.height), consoleText, label);
            }
        }
        public static void Log(string log) {
            if (useconsole) {
                Debug.Log($"<color=#a0a000>[Elixir] {log}</color>");
                Instance.consoleText += log + "\n";
            }

        }
    }
}