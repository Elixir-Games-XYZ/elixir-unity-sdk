using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Elixir;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;
using ZXing;
#if UNITY_ANDROID
#endif

public class QRCodeReader : MonoBehaviour
{
	public RawImage webcam;
#if UNITY_ANDROID || UNITY_IOS
	private WebCamTexture camTexture;
	private RectTransform webcamRT;
	private Thread qrThread;
	private bool DecodeQRRunning;

	private void Awake()
	{
		webcamRT = webcam.GetComponent<RectTransform>();
	}
	
	private IEnumerator Start()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
        bool wating = true;
        PermissionCallbacks callback = new PermissionCallbacks();
        callback.PermissionGranted += (string str) => { wating = false; };
        callback.PermissionDenied += (string str) => { wating = false; LoginController.Instance.OnCancelQRCode(); };
        callback.PermissionDeniedAndDontAskAgain += (string str) => { wating =
 false; LoginController.Instance.OnCancelQRCode(); };
        Permission.RequestUserPermission(Permission.Camera, callback);
        while(wating) yield return null;
#else
		yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
		// If doesn't get authorization, go back to first login window.
		if (!Application.HasUserAuthorization(UserAuthorization.WebCam))
			//            LoginController.Instance.OnCancelQRCode();
			yield break;
#endif
		if (camTexture == null) camTexture = new WebCamTexture(Screen.width, Screen.height);
		webcam.texture = camTexture;
		camTexture?.Play();
		oldResult = "";
		qrCode = "";

		qrThread = new Thread(DecodeQR);
		qrThread.Start();
	}

	private void OnEnable()
	{
		if (Application.HasUserAuthorization(UserAuthorization.WebCam))
		{
			if (camTexture == null) camTexture = new WebCamTexture(Screen.width, Screen.height);
			webcam.texture = camTexture;
		}

		camTexture?.Play();
		oldResult = "";
		qrCode = "";
	}

	private void OnDisable()
	{
		camTexture?.Pause();
	}

	private void OnDestroy()
	{
		DecodeQRRunning = false;
		//qrThread.Abort();
		camTexture?.Stop();
		camTexture = null;
	}

	private string qrCode = "";
	private string oldResult = "";
	private Color32[] qrData = new Color32[0];
	private int qrWidth;
	private int qrHeight;

	// Update is called once per frame
	private void FixedUpdate()
	{
		// Check if webcam is working
		if (camTexture != null && camTexture.isPlaying && camTexture.didUpdateThisFrame && camTexture.width > 16)
		{
			var localScale = new Vector3(1, 1, 1);
			if (camTexture.videoRotationAngle == 180)
			{
				localScale.x *= -1;
				localScale.y *= -1;
			}

			if (camTexture.videoVerticallyMirrored) localScale.y *= -1;

			webcamRT.localScale = localScale;
			lock (qrCode)
			{
				lock (qrData)
				{
					if (qrCode == "" && qrData.Length == 0)
					{
						// Get new data to process.
						qrData = camTexture.GetPixels32();
						qrWidth = camTexture.width;
						qrHeight = camTexture.height;
					}
				}

				if (qrCode != "" && qrCode != "WAIT")
				{
					// QRCode found, check if it is correct.
					GetUserCredentials(qrCode);
					qrCode = "WAIT";
				}
			}
		}
	}

	private void DecodeQR()
	{
		IBarcodeReader barcodeReader = new BarcodeReader { AutoRotate = true, Options = { TryHarder = false } };
		DecodeQRRunning = true;
		while (DecodeQRRunning)
		{
			Color32[] tmpData = null;
			lock (qrData)
			{
				if (qrData.Length > 0) tmpData = qrData;
			}

			if (tmpData != null)
			{
				var result = barcodeReader.Decode(tmpData, qrWidth, qrHeight);
				lock (qrData)
				{
					qrData = new Color32[0];
				}

				if (result != null && oldResult != result.Text)
				{
					oldResult = result.Text;
					if (result.Text.StartsWith("REIKEY:"))
						lock (qrCode)
						{
							qrCode = result.Text;
						}
				}
			}

			Thread.Sleep(200);
		}

		Debug.Log("DecodeQR thread Exit");
	}

	private async Task GetUserCredentials(string qrcode)
	{
		try
		{
			await Auth.QrVerify(qrcode);
			LoginController.Instance.OnStep3();
		}
		catch (Exception)
		{
			qrCode = "";
		}
	}
#endif
}