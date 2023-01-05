using UnityEngine.Networking;
using System.Text;
using System.Collections;

namespace Elixir {
    public class Auth : BaseWS {
        [System.Serializable]
        public class TokenResponse {
            [System.Serializable]
            public class Data {
                public string   token;
                public ulong    tokenExpiry;
                public ulong    tokenLifeMS;
                public string   refreshToken;
                public bool     newAccount;
            }
            public Data data = new Data();
        }

        static TokenResponse tokenResponse = new TokenResponse();

        public static string token { get { return tokenResponse.data.token; }  set { tokenResponse.data.token = value; } }

        public ulong serverTimeMS;
        public static IEnumerator InitREI(errorCallback OnError = null) {
#if UNITY_EDITOR
            // If there is no REI, generate it.
            if (string.IsNullOrEmpty(ElixirController.Instance.rei)) {
                yield return Elixir.GenerateREI.Do();
                ElixirController.Instance.rei = Elixir.ElixirController.Instance.rei;
            }
#endif
            lastError = true;
            if ( !string.IsNullOrEmpty(ElixirController.Instance.rei) )
                yield return Get($"/sdk/auth/v2/session/reikey/{ElixirController.Instance.rei}", tokenResponse);
            if (lastError) {
                OnError?.Invoke(error.code, error.message);
                UnityEngine.PlayerPrefs.DeleteKey("Elixir.RefreshToken");
                yield break;
            }
            SaveRefreshToken();
        }
        public class SyncRefreshBody {
            public string refreshToken;
        }
        public static IEnumerator Refresh() {
            lastError = true;
            LoadRefreshToken();
            if (!string.IsNullOrEmpty(tokenResponse.data.refreshToken))
                yield return Post( $"/auth/refresh-session", new SyncRefreshBody { refreshToken = tokenResponse.data.refreshToken }, tokenResponse);
            if (!lastError) {                
                SaveRefreshToken();
            } else {
                ClearRefreshToken();
                yield break;
            }
        }
        static void LoadRefreshToken() {
            tokenResponse.data.refreshToken = UnityEngine.PlayerPrefs.GetString("Elixir.RefreshToken");
        }
        static void SaveRefreshToken() {
            timeToRefreshToken = (tokenResponse.data.tokenLifeMS / 1000) - 5;
            UnityEngine.PlayerPrefs.SetString("Elixir.RefreshToken", tokenResponse.data.refreshToken);
        }
        static void ClearRefreshToken() {
            UnityEngine.PlayerPrefs.DeleteKey("Elixir.RefreshToken");
        }
#if UNITY_ANDROID || UNITY_IOS
        public static LoginRequestResponse loginRequestResponse = new LoginRequestResponse();
        public static IEnumerator InitMail(string email, errorCallback OnError = null) {
            yield return LoginRequest(email, loginRequestResponse);
            if (lastError) OnError?.Invoke(error.code, error.message);
        }

        public static IEnumerator ConfirmMail(string userCode, errorCallback OnError = null) {
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(loginRequestResponse.data.userCode.TrimEnd()))
                userCode = loginRequestResponse.data.userCode;
#endif
            yield return LoginVerify(loginRequestResponse.data.transactionId, userCode);
            if (!lastError) {
                SaveRefreshToken();
            } else {
                OnError?.Invoke(error.code, error.message);
                UnityEngine.PlayerPrefs.DeleteKey(ElixirController.Instance.PlayerPrefsKey);
            }
        }
        // Verify QRCode.
        public class VerifyQRBody {
            public string deviceModel;
            public string deviceUniqueIdentifier;
            public string qrValue;
        }
        public static IEnumerator VerifyQR(string qrValue, errorCallback OnError = null) {
            var body = new VerifyQRBody() {
                qrValue = qrValue,
                deviceModel = UnityEngine.SystemInfo.deviceModel,
                deviceUniqueIdentifier = UnityEngine.SystemInfo.deviceUniqueIdentifier
            };
            yield return Post($"/auth/qr-verify", body, tokenResponse);
            if (!lastError) {
                SaveRefreshToken();
            } else { 
                OnError?.Invoke(error.code, error.message);
                ClearRefreshToken();
                yield break;
            }
        }
        public class LoginVerifyBody {
            public string transactionId;
            public string code;
            public string deviceModel;
            public string deviceUniqueIdentifier;
        }
        public static IEnumerator LoginVerify(string transactionId, string code) {
            LoginVerifyBody body = new LoginVerifyBody{
                transactionId = transactionId,
                code = code,
                deviceModel = UnityEngine.SystemInfo.deviceModel,
                deviceUniqueIdentifier = UnityEngine.SystemInfo.deviceUniqueIdentifier
            };
            yield return Post($"/auth/otp-verify", body, tokenResponse);
            if (lastError) {
                ClearRefreshToken();
                yield break;
            }
            SaveRefreshToken();
            // Wait for two seconds just in case new account has been created.
            if (tokenResponse.data.newAccount)
                yield return new UnityEngine.WaitForSeconds(2);
        }
        [System.Serializable]
        public class LoginRequestResponse {
            [System.Serializable]
            public class DataResponse {
                public string transactionId;
                public string userCode;
            }
            public DataResponse data = new DataResponse();
        }
        public class LoginRequestBody {
            public string email;
        }
        public delegate void LoginRequestCallback(LoginRequestResponse.DataResponse response);
        public static IEnumerator LoginRequest(string email, LoginRequestResponse response) {
            yield return Post($"/auth/otp-login", new LoginRequestBody { email = email }, response);
        }
        public static bool IsLoged() { return !string.IsNullOrEmpty(token); }
        public static IEnumerator Logout() {
            if(!string.IsNullOrEmpty(token))
                yield return Get($"/auth/{GameID}/closerei", null);
            token = null;
            tokenResponse.data.refreshToken = null;
            ClearRefreshToken();
        }
#endif
        public static void Close() {
            var uri = $"/sdk/auth/v2/session/closerei/{ElixirController.Instance.rei}";
            UnityWebRequest www = UnityWebRequest.Get($"{BaseWS.baseURL}{uri}");
            www.SetRequestHeader("x-api-key", APIKEY);
            if (!string.IsNullOrEmpty(token)) www.SetRequestHeader("Authorization", "Bearer " + token);
            www.SendWebRequest();
            ElixirController.Log($"Ending session.");
        }
        static float timeToRefreshToken = 0;
        public static void CheckToken(float deltaTime) {
            if (timeToRefreshToken > 0) {
                timeToRefreshToken -= deltaTime;
                if (timeToRefreshToken < 0) {
                    timeToRefreshToken = 0;
                    ElixirController.Instance.StartCoroutine( Refresh() ); // Refresh token
                }
            }
        }
    }
}