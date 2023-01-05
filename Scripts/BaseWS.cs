using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Security.Cryptography;

// Clase base para consumo de EndPoints de Elixir.
namespace Elixir
{
    public class BaseWS
    {
        internal static string baseURL = "https://kend.elixir.app";
        internal static string APIKEY;

        public delegate void callback();
        public delegate void errorCallback(int code, string message);
        [System.Serializable]
        public class Error
        {
            public bool success = true;
            public int code = 0;
            public string message = "-1";
        }
        public static Error error = new Error();

        public static bool lastError;
        // GET Call.
        protected static IEnumerator Get(string uri, object objectToOverride) {
            yield return MakeRequest(uri, null, objectToOverride);
        }
        // POST Call.
      
        protected static IEnumerator Post(string uri, object body, object objectToOverride) {
            string bodyText = JsonUtility.ToJson(body);
            ElixirController.Log($"Elixir.Post({baseURL}{uri}) <- {bodyText}");
            yield return MakeRequest(uri, bodyText, objectToOverride);
        }
        protected static IEnumerator MakeRequest(string uri, string body, object target) {
            lastError = true;
            error.success = false;
            error.code = 0;
            UnityWebRequest www;
            if (string.IsNullOrEmpty(body)) {
                www = UnityWebRequest.Get($"{baseURL}{uri}");
            } else {
                www = UnityWebRequest.Post($"{baseURL}{uri}", body);
                www.SetRequestHeader("content-type", "application/json");
                www.uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(body));
                www.uploadHandler.contentType = "application/json";
            }
            www.SetRequestHeader("Cache-Control", "no-cache, no-store, must-revalidate");
            www.SetRequestHeader("Expires", "0");
            www.SetRequestHeader("x-api-key", APIKEY);
            if (!string.IsNullOrEmpty(Auth.token))
                www.SetRequestHeader("Authorization", "Bearer " + Auth.token);
            yield return www.SendWebRequest();
            try {
                if (www.result != UnityWebRequest.Result.Success) {
                    throw new System.Exception($"{www.error} ->{www?.downloadHandler?.text}");
                } else {
                    ElixirController.Log($"Elixir.Response({uri}) {www.downloadHandler.text} ");
                    // Check if error.
                    string text = www.downloadHandler.text;
                    if (text[0] != '[') { // is not an array.
                        if(target!=null) JsonUtility.FromJsonOverwrite(text, target);
                        JsonUtility.FromJsonOverwrite(text, error);
                        if (!error.success) 
                                throw new System.Exception($"Error on request. errorCode: {error.code} msg: {error.message}");
                    }
                    lastError = false;
                }
            } catch (System.Exception e) {
                ElixirController.Log($"ERROR: Elixir.Get({uri}) {e.Message} token {Auth.token}");
                error.code = -1;
            }
        }
    }
}
