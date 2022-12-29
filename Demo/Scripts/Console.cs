using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
    public Text console;
    List<string> consoleLines = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        if(Elixir.ElixirController.useconsole)
            Application.logMessageReceived += LogCallback;
    }

    public void LogCallback(string condition, string stackTrace, LogType type) {
        if (consoleLines.Count > 50)
            consoleLines.RemoveAt(0);
        consoleLines.Add(condition);
        console.text = string.Join("\r\n", consoleLines);
    }
}
