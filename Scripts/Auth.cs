using UnityEngine.Networking;
using System.Text;
using System.Collections;

namespace Elixir {
    public class Auth : BaseWS {
        [System.Serializable]
        public class TokenResponse {
            [System.Serializable]
            public class TokenData {
                public string   token;
                public ulong    tokenExpiry;
                public ulong    tokenLifeMS;
                public string   refreshToken;
                public bool     newAccount;
            }
            public TokenData data = new TokenData();
        }

        static TokenResponse tokenResponse = new TokenResponse();

        public static string token { get { return tokenResponse.data.token; }  set { tokenResponse.data.token = value; } }

        public ulong serverTimeMS;
        public static IEnumerator InitREI(string rei, errorCallback OnError = null) {
#if UNITY_EDITOR
            // If there is no REI, generate it.
            if (string.IsNullOrEmpty(rei)) {
                yield return Elixir.GenerateREI.Do();
                rei = Elixir.ElixirController.Instance.rei;
            }
#endif
            lastError = true;
            if ( !string.IsNullOrEmpty(rei) )
                yield return Get($"/auth/{GameID}/reikey/{rei}", tokenResponse);
            if (lastError) {
                OnError?.Invoke(error.code, error.message);
                UnityEngine.PlayerPrefs.DeleteKey(ElixirController.Instance.PlayerPrefsKey);
                yield break;
            }
            timeToRefreshToken = (tokenResponse.data.tokenLifeMS / 1000) - 5;
            UnityEngine.PlayerPrefs.SetString(ElixirController.Instance.PlayerPrefsKey, tokenResponse.data.refreshToken);
        }
        public class SyncRefreshBody {
            public string refreshToken;
        }
        public static IEnumerator Refresh() {
            lastError = true;
            tokenResponse.data.refreshToken = UnityEngine.PlayerPrefs.GetString(ElixirController.Instance.PlayerPrefsKey);
            if (!string.IsNullOrEmpty(tokenResponse.data.refreshToken))
                yield return Post( $"/auth/refresh-session", new SyncRefreshBody { refreshToken = tokenResponse.data.refreshToken }, tokenResponse);
            if (lastError) {
//                ElixirController.Instance.dialog.Show($"Error [{error.message}]", $"There is a problem with your account.", "Accept", () => { UnityEngine.Application.Quit(); });
                UnityEngine.PlayerPrefs.DeleteKey(ElixirController.Instance.PlayerPrefsKey);
                yield break;
            }
            timeToRefreshToken = (tokenResponse.data.tokenLifeMS / 1000) - 5;
            UnityEngine.PlayerPrefs.SetString(ElixirController.Instance.PlayerPrefsKey, tokenResponse.data.refreshToken);
        }
#if UNITY_ANDROID || UNITY_IOS
        static LoginRequestResponse loginRequestResponse = new LoginRequestResponse();
        public static IEnumerator InitMail(string email, errorCallback OnError = null) {
            yield return LoginRequest(email, loginRequestResponse);
            if (lastError) OnError?.Invoke(error.code, error.message);
        }

        public static IEnumerator ConfirmMail(string userCode, errorCallback OnError = null) {
#if UNITY_EDITOR
            if(!string.IsNullOrEmpty(loginRequestResponse.data.userCode))
                userCode = loginRequestResponse.data.userCode;
#endif
            yield return LoginVerify(loginRequestResponse.data.transactionId, userCode);
            if (!lastError) {
                timeToRefreshToken = (tokenResponse.data.tokenLifeMS / 1000) - 5;
                UnityEngine.PlayerPrefs.SetString(ElixirController.Instance.PlayerPrefsKey, tokenResponse.data.refreshToken);
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
        public IEnumerator VerifyQR(string qrValue, callback OnOk = null, errorCallback OnError = null) {
            var body = new VerifyQRBody() {
                qrValue = qrValue,
                deviceModel = UnityEngine.SystemInfo.deviceModel,
                deviceUniqueIdentifier = UnityEngine.SystemInfo.deviceUniqueIdentifier
            };
            yield return Post($"/auth/qr-verify", body, tokenResponse);
            if (lastError) {
                OnError?.Invoke(error.code, error.message);
                UnityEngine.PlayerPrefs.DeleteKey(ElixirController.Instance.PlayerPrefsKey);
                yield break;
            }
            timeToRefreshToken = (tokenResponse.data.tokenLifeMS / 1000) - 5;
            UnityEngine.PlayerPrefs.SetString(ElixirController.Instance.PlayerPrefsKey, tokenResponse.data.refreshToken);
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
                UnityEngine.PlayerPrefs.DeleteKey(ElixirController.Instance.PlayerPrefsKey);
                yield break;
            }
            timeToRefreshToken = (tokenResponse.data.tokenLifeMS / 1000) - 5;
            UnityEngine.PlayerPrefs.SetString(ElixirController.Instance.PlayerPrefsKey, tokenResponse.data.refreshToken);
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
            yield return Get($"/auth/{GameID}/closerei", null);
            token = null;
            tokenResponse.data.refreshToken = null;
            UnityEngine.PlayerPrefs.DeleteKey(ElixirController.Instance.PlayerPrefsKey);
        }
#endif
        public static void Close() {
            var uri = $"/auth/{GameID}/closerei";
            ulong epoch = BaseWS.GetEpoch();
            string signature = BaseWS.ByteArrayToString(BaseWS.hmac.ComputeHash(Encoding.ASCII.GetBytes($"{epoch}.\"{uri}\"")));
            UnityWebRequest www = UnityWebRequest.Get($"{BaseWS.baseURL}{uri}");
            www.SetRequestHeader("x-api-key", APIKEY);
            www.SetRequestHeader("x-api-time", epoch.ToString());
            www.SetRequestHeader("x-api-signature", signature);
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