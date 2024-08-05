using System;
using UnityEngine;
using Debug = UnityEngine.Debug;
#if !UNITY_ANDROID && !UNITY_IOS && !(UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
using Elixir.Overlay;
using Event = Elixir.Overlay.Event;
#endif

namespace Elixir
{
	public class ElixirController : MonoBehaviour
	{
		private static ElixirController _instance;
#if !UNITY_EDITOR
		private static bool _readyToQuit;
#endif

		internal string Rei { get; set; }

		public static ElixirController Instance
		{
			get
			{
				if (_instance == null)
				{
					var gameObject = new GameObject("ElixirController");
					_instance = gameObject.AddComponent<ElixirController>();
					DontDestroyOnLoad(gameObject);
				}

				return _instance;
			}
		}

		public static bool DebugLog { get; set; }

		private void Update()
		{
			Auth.CheckToken(Time.deltaTime);
#if !UNITY_ANDROID && !UNITY_IOS && !(UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
			OverlayMessage.Update();
#endif
		}

		[RuntimeInitializeOnLoadMethod]
		private static void RunOnStart()
		{
			Application.wantsToQuit += WantsToQuit;
		}

		private static bool WantsToQuit()
		{
#if UNITY_EDITOR
			Auth.CloseRei(Instance.Rei);
			return true;
#else
			CloseReiAndQuit();
			return _readyToQuit;
#endif
		}

#if !UNITY_EDITOR
		private static async void CloseReiAndQuit()
		{
			try
			{
				await Auth.CloseRei(Instance.Rei);
			}
			catch (Exception)
			{
				Log("CloseRei failed");
			}
			finally
			{
				_readyToQuit = true;
				Application.Quit();
			}
		}
#endif

		public bool PrepareElixir(string apiKey)
		{
			BaseWebService.apiKey = apiKey;
			InitReiProgramArgument();
#if !UNITY_ANDROID && !UNITY_IOS && !(UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
			OverlayMessage.Init(ProcessMessage);
#endif
			return true;
		}

		// the rei argument comes from the Elixir Launcher on desktop platforms
		private void InitReiProgramArgument()
		{
			var args = Environment.GetCommandLineArgs();
			for (var i = 0; i < args.Length; i++)
				if (args[i] == "-rei")
					Rei = args[i + 1];
		}

		public static void Log(string log)
		{
			if (DebugLog) Debug.Log($"<color=#a0a000>[Elixir] {log}</color>");
		}

#if !UNITY_ANDROID && !UNITY_IOS && !(UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX)
		private void ProcessMessage(IMessage message)
		{
			Log("Event received");
			switch (message)
			{
				case MOpenStateChange openStateChange:
					Log($"MOpenStateChange: {openStateChange.IsOpen}");
					Event.OnOpenStateChange(openStateChange.IsOpen);
					break;
				case MCheckoutResult checkoutResult:
					Log($"MCheckoutResult: {checkoutResult.Success}, {checkoutResult.Sku}");
					Event.OnCheckoutResult(checkoutResult.Success, checkoutResult.Sku);
					break;
				case MMKGetWalletResult getWalletResult:
					Log(
						$"MMKGetWalletResult: {getWalletResult.Status}, {getWalletResult.EthAddress}, {getWalletResult.SolAddress}, {getWalletResult.EosAddress}");
					Event.OnGetWalletResult(getWalletResult.Status, getWalletResult.EthAddress, getWalletResult.SolAddress,
						getWalletResult.EosAddress);
					break;
				case MMKSignTypedDataResult signTypedDataResult:
					Log(
						$"MMKSignTypedDataResult: {signTypedDataResult.Status}, {signTypedDataResult.Signature}, {signTypedDataResult.R}, {signTypedDataResult.S}, {signTypedDataResult.V}");
					Event.OnSignTypedDataResult(signTypedDataResult.Status, signTypedDataResult.Signature, signTypedDataResult.R,
						signTypedDataResult.S, signTypedDataResult.V);
					break;
				case MMKSignMessageResult signMessageResult:
					switch (signMessageResult.Response.type)
					{
						case MKResponseType.MKResponseEVM:
							Log(
								$"SignMessageResultEVM: {signMessageResult.Status}, {signMessageResult.Response.responseEVM.Signature}, {signMessageResult.Response.responseEVM.R}, {signMessageResult.Response.responseEVM.S}, {signMessageResult.Response.responseEVM.V}");
							Event.OnSignMessageResult(signMessageResult.Status,
								new SignMessageResultEVM(
									signMessageResult.Response.responseEVM.Signature, signMessageResult.Response.responseEVM.R,
									signMessageResult.Response.responseEVM.S,
									signMessageResult.Response.responseEVM.V
								), null, null);
							break;
						case MKResponseType.MKResponseSolana:
							Log(
								$"SignMessageResultSolana: {signMessageResult.Status}, {signMessageResult.Response.responseSolana.Signature}");
							Event.OnSignMessageResult(signMessageResult.Status,
								null, new SignMessageResultSolana(signMessageResult.Response.responseSolana.Signature), null);
							break;
						case MKResponseType.MKResponseEOS:
							Log(
								$"SignMessageResultEOS: {signMessageResult.Status}, {signMessageResult.Response.responseEOS.Signature}");
							Event.OnSignMessageResult(signMessageResult.Status,
								null, null, new SignMessageResultEOS(signMessageResult.Response.responseEOS.Signature));
							break;
					}

					break;
				case MMKSignTransactionResult signTransactionResult:
					switch (signTransactionResult.Response.type)
					{
						case MKResponseType.MKResponseEVM:
							Log(
								$"SignTransactionResultEVM: {signTransactionResult.Status}, {signTransactionResult.Response.responseEVM.Signature}, {signTransactionResult.Response.responseEVM.SignedRawTransaction}, {signTransactionResult.Response.responseEVM.TransactionHash}, {signTransactionResult.Response.responseEVM.R}, {signTransactionResult.Response.responseEVM.S}, {signTransactionResult.Response.responseEVM.V}");
							Event.OnSignTransactionResult(signTransactionResult.Status,
								new SignTransactionResultEVM(
									signTransactionResult.Response.responseEVM.Signature,
									signTransactionResult.Response.responseEVM.SignedRawTransaction,
									signTransactionResult.Response.responseEVM.TransactionHash,
									signTransactionResult.Response.responseEVM.R,
									signTransactionResult.Response.responseEVM.S,
									signTransactionResult.Response.responseEVM.V
								), null, null);
							break;
						case MKResponseType.MKResponseSolana:
							Log(
								$"SignTransactionResultSolana: {signTransactionResult.Status}, {signTransactionResult.Response.responseSolana.Signature}");
							Event.OnSignTransactionResult(signTransactionResult.Status,
								null, new SignTransactionResultSolana(signTransactionResult.Response.responseSolana.Signature), null);
							break;
						case MKResponseType.MKResponseEOS:
							Log(
								$"SignTransactionResultEOS: {signTransactionResult.Status}, {signTransactionResult.Response.responseEOS.Signature}");
							Event.OnSignTransactionResult(signTransactionResult.Status,
								null, null, new SignTransactionResultEOS(signTransactionResult.Response.responseEOS.Signature));
							break;
					}

					break;
				default:
					Log("Event not processed");
					break;
			}
		}

		public void OnApplicationQuit()
		{
			OverlayMessage.StopListening();
		}
#endif
	}
}