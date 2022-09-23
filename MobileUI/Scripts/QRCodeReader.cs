using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using ZXing;
using System.Threading.Tasks;

#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class QRCodeReader : MonoBehaviour {
    public RawImage webcam;
#if UNITY_ANDROID || UNITY_IOS
    WebCamTexture   camTexture;
    RectTransform webcamRT;
    System.Threading.Thread qrThread;
    void Awake() {
        webcamRT = webcam.GetComponent<RectTransform>();
    }
    IEnumerator Start() {
#if UNITY_ANDROID && !UNITY_EDITOR
        bool wating = true;
        PermissionCallbacks callback = new PermissionCallbacks();
        callback.PermissionGranted += (string str) => { wating = false; };
        callback.PermissionDenied += (string str) => { wating = false; LoginController.Instance.OnCancelQRCode(); };
        callback.PermissionDeniedAndDontAskAgain += (string str) => { wating = false; LoginController.Instance.OnCancelQRCode(); };
        Permission.RequestUserPermission(Permission.Camera, callback);
        while(wating) yield return null;
#else
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        // If doesn't get authorization, go back to first login window.
        if (!Application.HasUserAuthorization(UserAuthorization.WebCam)) {
//            LoginController.Instance.OnCancelQRCode();
            yield break;
        }
#endif
        if (camTexture == null) camTexture = new WebCamTexture(Screen.width, Screen.height);
        webcam.texture = camTexture;
        camTexture?.Play();
        oldResult = "";
        qrCode = "";

        qrThread = new System.Threading.Thread(DecodeQR);
        qrThread.Start();
    }

    private void OnEnable() {
        if (Application.HasUserAuthorization(UserAuthorization.WebCam)) {
            if (camTexture == null) camTexture = new WebCamTexture(Screen.width, Screen.height);
            webcam.texture = camTexture;
        }
        camTexture?.Play();
        oldResult = "";
        qrCode = "";
    }
    private void OnDisable() {
        camTexture?.Pause();
    }
    void OnDestroy() {
        qrThread.Abort();
        camTexture?.Stop();
        camTexture = null;
    }
    string qrCode="";
    string oldResult="";    
    Color32[] qrData = new Color32[0];
    int qrWidth;
    int qrHeight;

    // Update is called once per frame
    void FixedUpdate() {
        // Check if webcam is working
        if (camTexture != null && camTexture.isPlaying && camTexture.didUpdateThisFrame && camTexture.width > 16) {
            Vector3 localScale = new Vector3(1, 1, 1);
            if (camTexture.videoRotationAngle == 180) {
                localScale.x *= -1;
                localScale.y *= -1;
            }
            if (camTexture.videoVerticallyMirrored) {
                localScale.y *= -1;
            }
            /*
#if UNITY_IOS
            if(camTexture.videoRotationAngle == 0)
                webcamRT.localScale = new Vector3( 1,-1, 1);
            else
                webcamRT.localScale = new Vector3(-1, 1, 1);
#else
            if (camTexture.videoRotationAngle == 0)
                webcamRT.localScale = new Vector3(1, 1, 1);
            else
                webcamRT.localScale = new Vector3(-1, -1, 1);
            
#endif
            */
            webcamRT.localScale = localScale;
            lock (qrCode) {
                lock (qrData) {
                    if (qrCode == "" && qrData.Length==0) {
                        // Get new data to process.
                        qrData = camTexture.GetPixels32();
                        qrWidth = camTexture.width;
                        qrHeight = camTexture.height;
                    }
                }            
                if (qrCode != "" && qrCode!="WAIT") {                    
                    // QRCode found, check if it is correct.
                    StartCoroutine(GetUserCredentials(qrCode));
                    qrCode = "WAIT";
                }
            }
        }
    }

    void DecodeQR() {
        IBarcodeReader barcodeReader = new BarcodeReader { AutoRotate = true, Options = { TryHarder = false }  };
        while (true) {
            Color32[] tmpData = null;
            lock (qrData) if(qrData.Length > 0) tmpData = qrData;
            if (tmpData!=null) {
                var result = barcodeReader.Decode(tmpData, qrWidth, qrHeight);
                lock (qrData) qrData = new Color32[0];
                if (result != null && oldResult != result.Text) {
                    oldResult = result.Text;
                    if (result.Text.StartsWith("REIKEY:")) {                        
                        lock (qrCode)
                            qrCode = result.Text;
                    }
                }
            }
            System.Threading.Thread.Sleep(200);
        }
    }

    IEnumerator GetUserCredentials(string qrcode) {
        yield return Elixir.Auth.VerifyQR(qrcode);
        if (!Elixir.Auth.lastError) {
            LoginController.Instance.OnStep3();
            yield break;
        } else
            qrCode = "";
    }
#endif
}
