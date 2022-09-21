using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour {
    static LoginController _Instance;
    public static LoginController Instance { get { if (_Instance == null) _Instance = (LoginController)(Resources.FindObjectsOfTypeAll(typeof(LoginController))[0]); return _Instance; } }
    void Awake() { if (_Instance == null) gameObject.SetActive(false); }
    #if UNITY_ANDROID || UNITY_IOS
    public GameObject step1;
    public GameObject step2;
    public GameObject QRCode;
    public GameObject infiniteLoop;

    public InputField   email;
    public Text         checkEmailText;
    public CodeWidget   code;

    Elixir.BaseWS.callback onLogin;
    Elixir.BaseWS.callback onError;

    public void OnLogin(Elixir.BaseWS.callback onLogin, Elixir.BaseWS.callback onError=null) {
        this.onLogin = onLogin;
        this.onError = onError;
        email.text = "";
        step1.SetActive(true);
        step2.SetActive(false);
        QRCode.SetActive(false);
        gameObject.SetActive(true);
    }
    public void OnStep1() {
        StartCoroutine(SignupRequest(email.text));
    }

    public void OnCancelStep1() {
        gameObject.SetActive(false);
        onError?.Invoke();
    }

    IEnumerator SignupRequest(string email) {
        infiniteLoop.SetActive(true);
        yield return Elixir.Auth.InitMail(email);
        infiniteLoop.SetActive(false);

        if (!Elixir.Auth.lastError) {
            checkEmailText.text = string.Format("A verification code has been sent to {0}.\nPlease enter the code to continue.", email);
            if (string.IsNullOrEmpty(Elixir.Auth.loginRequestResponse.data.userCode)) Elixir.Auth.loginRequestResponse.data.userCode = "     ";
            code.text = Elixir.Auth.loginRequestResponse.data.userCode;
            // Show step 2
            step1.SetActive(false);
            step2.SetActive(true);
        }
    }

    public void OnStep2() {
        StartCoroutine(ValidateCode(code.text));
    }

    IEnumerator ValidateCode(string code) {
        infiniteLoop.SetActive(true);
        yield return Elixir.Auth.ConfirmMail(code);
        infiniteLoop.SetActive(false);

        if (!Elixir.Auth.lastError) OnStep3();
    }

    public void OnCancelStep2() {
        // Go to step 1
        step1.SetActive(true);
        step2.SetActive(false);
        QRCode.SetActive(false);
    }

    public void OnUseQRCode() {
        // Go to QRCode screen
        step1.SetActive(false);
        step2.SetActive(false);
        QRCode.SetActive(true);
    }

    public void OnCancelQRCode() {
        // Go to step 1
        step1.SetActive(true);
        step2.SetActive(false);
        QRCode.SetActive(false);
    }

    public void OnStep3() {
        Handheld.Vibrate(); // Vibrate
        gameObject.SetActive(false);
        onLogin?.Invoke();
    }
#endif
}
