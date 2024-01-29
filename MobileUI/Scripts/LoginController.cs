using System;
using System.Threading.Tasks;
using Elixir;
using UnityEngine;
using UnityEngine.UI;

public class LoginController : MonoBehaviour
{
	private static LoginController _Instance;

	public static LoginController Instance
	{
		get
		{
			if (_Instance == null) _Instance = (LoginController)Resources.FindObjectsOfTypeAll(typeof(LoginController))[0];
			return _Instance;
		}
	}

	private void Awake()
	{
		if (_Instance == null) gameObject.SetActive(false);
	}

#if UNITY_ANDROID || UNITY_IOS
	public GameObject step1;
	public GameObject step2;
	public GameObject QRCode;
	public GameObject infiniteLoop;

	public InputField email;
	public Text checkEmailText;
	public CodeWidget code;

	public delegate void OnLoginDelegate();

	public delegate void OnErrorDelegate();

	private OnLoginDelegate onLogin;
	private OnErrorDelegate onError;

	private string transactionId = "";

	public void OnLogin(OnLoginDelegate onLogin, OnErrorDelegate onError = null)
	{
		this.onLogin = onLogin;
		this.onError = onError;
		email.text = "";
		step1.SetActive(true);
		step2.SetActive(false);
		QRCode.SetActive(false);
		gameObject.SetActive(true);
	}

	public void OnStep1()
	{
		SignupRequest(email.text);
	}

	public void OnCancelStep1()
	{
		gameObject.SetActive(false);
		transactionId = "";
		onError?.Invoke();
	}

	private async Task SignupRequest(string email)
	{
		infiniteLoop.SetActive(true);
		try
		{
			var response = await Auth.OtpLogin(email);
			transactionId = response.transactionId;


			checkEmailText.text = $"A verification code has been sent to {email}.\nPlease enter the code to continue.";
			if (string.IsNullOrEmpty(response.userCode))
				response.userCode = "     ";
			code.text = response.userCode;

			// Show step 2
			step1.SetActive(false);
			step2.SetActive(true);
		}
		catch (Exception exception)
		{
			ElixirController.Log($"OTP login request failed: {exception.Message}");
		}

		infiniteLoop.SetActive(false);
	}

	public void OnStep2()
	{
		ValidateCode(code.text);
	}

	private async Task ValidateCode(string code)
	{
		infiniteLoop.SetActive(true);
		try
		{
			await Auth.OtpVerify(transactionId, code);
			OnStep3();
		}
		catch (Exception exception)
		{
			ElixirController.Log($"OTP validation failed: {exception.Message}");
		}

		infiniteLoop.SetActive(false);
	}

	public void OnCancelStep2()
	{
		transactionId = "";

		// Go to step 1
		step1.SetActive(true);
		step2.SetActive(false);
		QRCode.SetActive(false);
	}

	public void OnUseQRCode()
	{
		// Go to QRCode screen
		step1.SetActive(false);
		step2.SetActive(false);
		QRCode.SetActive(true);
	}

	public void OnCancelQRCode()
	{
		transactionId = "";

		// Go to step 1
		step1.SetActive(true);
		step2.SetActive(false);
		QRCode.SetActive(false);
	}

	public void OnStep3()
	{
		Handheld.Vibrate(); // Vibrate
		gameObject.SetActive(false);
		onLogin?.Invoke();
	}
#endif
}