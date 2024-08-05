using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Elixir")]
[assembly: InternalsVisibleTo("ElixirEditor")]

namespace Elixir.Overlay
{
	public struct SignMessageResultEVM
	{
		public readonly string Signature;
		public readonly string R;
		public readonly string S;
		public readonly string V;

		public SignMessageResultEVM(string signature, string r, string s, string v)
		{
			Signature = signature;
			R = r;
			S = s;
			V = v;
		}
	}

	public struct SignMessageResultSolana
	{
		public readonly string Signature;

		public SignMessageResultSolana(string signature)
		{
			Signature = signature;
		}
	}

	public struct SignMessageResultEOS
	{
		public readonly string Signature;

		public SignMessageResultEOS(string signature)
		{
			Signature = signature;
		}
	}

	public struct SignTransactionResultEVM
	{
		public readonly string Signature;
		public readonly string SignedRawTransaction;
		public readonly string TransactionHash;
		public readonly string R;
		public readonly string S;
		public readonly string V;

		public SignTransactionResultEVM(string signature, string signedRawTransaction, string transactionHash, string r,
			string s, string v)
		{
			Signature = signature;
			SignedRawTransaction = signedRawTransaction;
			TransactionHash = transactionHash;
			R = r;
			S = s;
			V = v;
		}
	}

	public struct SignTransactionResultSolana
	{
		public readonly string Signature;

		public SignTransactionResultSolana(string signature)
		{
			Signature = signature;
		}
	}

	public struct SignTransactionResultEOS
	{
		public readonly string Signature;

		public SignTransactionResultEOS(string signature)
		{
			Signature = signature;
		}
	}

	internal enum MessageType
	{
		MTEmpty = 0,
		MTToken = 1,
		MTOpenStateChange = 2,
		MTCheckout = 3,
		MTCheckoutResult = 4,
		MTFeatureFlags = 5,
		MTLanguage = 6,
		MTSetVisibility = 7,
		MTMKSignTransaction = 50,
		MTMKSignTransactionResult = 51,
		MTMKGetWallet = 52,
		MTMKGetWalletResult = 53,
		MTMKSignMessage = 54,
		MTMKSignMessageResult = 55,
		MTMKSignTypedData = 56,
		MTMKSignTypedDataResult = 57
	}

	internal enum MKResponseType
	{
		MKResponseNone = 0,
		MKResponseEVM = 1,
		MKResponseSolana = 2,
		MKResponseEOS = 3
	}

	internal interface IMessage
	{
	}

	internal class MainThreadQueue
	{
		// anything on this queue will be executed on the main thread in Update()
		private static readonly Queue<Action> ExecutionQueue = new Queue<Action>();

		internal void Enqueue(Action action)
		{
			ExecutionQueue.Enqueue(action);
		}

		internal void Update()
		{
			lock (ExecutionQueue)
			{
				while (ExecutionQueue.Count > 0) ExecutionQueue.Dequeue().Invoke();
			}
		}
	}


	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct MToken : IMessage, IDisposable
	{
		private IntPtr TokenPtr;

		internal MToken(string token)
		{
			TokenPtr = Marshal.StringToHGlobalAnsi(token);
		}

		internal string Token
		{
			get => Marshal.PtrToStringAnsi(TokenPtr);
			set
			{
				if (TokenPtr != IntPtr.Zero) Marshal.FreeHGlobal(TokenPtr);
				TokenPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (TokenPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(TokenPtr);
				TokenPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MOpenStateChange : IMessage
	{
		[MarshalAs(UnmanagedType.I1)] internal bool IsOpen;

		internal MOpenStateChange(bool inIsOpen)
		{
			IsOpen = inIsOpen;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct MCheckout : IMessage, IDisposable
	{
		private IntPtr SkuPtr;

		internal MCheckout(string inSku)
		{
			SkuPtr = Marshal.StringToHGlobalAnsi(inSku);
		}

		internal string Sku
		{
			get => Marshal.PtrToStringAnsi(SkuPtr);
			set
			{
				if (SkuPtr != IntPtr.Zero) Marshal.FreeHGlobal(SkuPtr);
				SkuPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SkuPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SkuPtr);
				SkuPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MCheckoutResult : IMessage, IDisposable
	{
		[MarshalAs(UnmanagedType.I1)] internal bool Success;
		private IntPtr SkuPtr;

		internal MCheckoutResult(bool inSuccess, string inSku)
		{
			Success = inSuccess;
			SkuPtr = Marshal.StringToHGlobalAnsi(inSku);
		}

		internal string Sku
		{
			get => Marshal.PtrToStringAnsi(SkuPtr);
			set
			{
				if (SkuPtr != IntPtr.Zero) Marshal.FreeHGlobal(SkuPtr);
				SkuPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SkuPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SkuPtr);
				SkuPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct MFeatureFlags : IMessage, IDisposable
	{
		private IntPtr FeatureFlagsPtr;

		internal MFeatureFlags(string featureFlags)
		{
			FeatureFlagsPtr = Marshal.StringToHGlobalAnsi(featureFlags);
		}

		internal string FeatureFlags
		{
			get => Marshal.PtrToStringAnsi(FeatureFlagsPtr);
			set
			{
				if (FeatureFlagsPtr != IntPtr.Zero) Marshal.FreeHGlobal(FeatureFlagsPtr);
				FeatureFlagsPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (FeatureFlagsPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(FeatureFlagsPtr);
				FeatureFlagsPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	internal struct MLanguage : IMessage, IDisposable
	{
		private IntPtr LanguagePtr;

		internal MLanguage(string language)
		{
			LanguagePtr = Marshal.StringToHGlobalAnsi(language);
		}

		internal string Language
		{
			get => Marshal.PtrToStringAnsi(LanguagePtr);
			set
			{
				if (LanguagePtr != IntPtr.Zero) Marshal.FreeHGlobal(LanguagePtr);
				LanguagePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (LanguagePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(LanguagePtr);
				LanguagePtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MMKGetWallet : IMessage
	{
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKGetWalletResult : IMessage, IDisposable
	{
		private IntPtr StatusPtr;
		private IntPtr EthAddressPtr;
		private IntPtr SolAddressPtr;
		private IntPtr EosAddressPtr;

		internal MMKGetWalletResult(string inStatus, string inEthAddress, string inSolAddress, string inEosAddress)
		{
			StatusPtr = Marshal.StringToHGlobalAnsi(inStatus);
			EthAddressPtr = Marshal.StringToHGlobalAnsi(inEthAddress);
			SolAddressPtr = Marshal.StringToHGlobalAnsi(inSolAddress);
			EosAddressPtr = Marshal.StringToHGlobalAnsi(inEosAddress);
		}

		internal string Status
		{
			get => Marshal.PtrToStringAnsi(StatusPtr);
			set
			{
				if (StatusPtr != IntPtr.Zero) Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string EthAddress
		{
			get => Marshal.PtrToStringAnsi(EthAddressPtr);
			set
			{
				if (EthAddressPtr != IntPtr.Zero) Marshal.FreeHGlobal(EthAddressPtr);
				EthAddressPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string SolAddress
		{
			get => Marshal.PtrToStringAnsi(SolAddressPtr);
			set
			{
				if (SolAddressPtr != IntPtr.Zero) Marshal.FreeHGlobal(SolAddressPtr);
				SolAddressPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string EosAddress
		{
			get => Marshal.PtrToStringAnsi(EosAddressPtr);
			set
			{
				if (EosAddressPtr != IntPtr.Zero) Marshal.FreeHGlobal(EosAddressPtr);
				EosAddressPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (StatusPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = IntPtr.Zero;
			}

			if (EthAddressPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(EthAddressPtr);
				EthAddressPtr = IntPtr.Zero;
			}

			if (SolAddressPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SolAddressPtr);
				SolAddressPtr = IntPtr.Zero;
			}

			if (EosAddressPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(EosAddressPtr);
				EosAddressPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTypedData : IMessage
	{
		private IntPtr MessagePtr;
		private IntPtr ReasonPtr;

		internal MMKSignTypedData(string inMessage, string inReason)
		{
			MessagePtr = Marshal.StringToHGlobalAnsi(inMessage);
			ReasonPtr = Marshal.StringToHGlobalAnsi(inReason);
		}

		internal string Message
		{
			get => Marshal.PtrToStringAnsi(MessagePtr);
			set
			{
				if (MessagePtr != IntPtr.Zero) Marshal.FreeHGlobal(MessagePtr);
				MessagePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string Reason
		{
			get => Marshal.PtrToStringAnsi(ReasonPtr);
			set
			{
				if (ReasonPtr != IntPtr.Zero) Marshal.FreeHGlobal(ReasonPtr);
				ReasonPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (MessagePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(MessagePtr);
				MessagePtr = IntPtr.Zero;
			}

			if (ReasonPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(ReasonPtr);
				ReasonPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTransactionResultResponseEVMInterop : IMessage, IDisposable
	{
		private IntPtr SignedRawTransactionPtr;
		private IntPtr TransactionHashPtr;
		private IntPtr SignaturePtr;
		private IntPtr RPtr;
		private IntPtr SPtr;
		private IntPtr VPtr;

		internal MMKSignTransactionResultResponseEVMInterop(string inSignedRawTransaction, string inTransactionHash,
			string inSignature, string inR, string inS, string inV)
		{
			SignedRawTransactionPtr = Marshal.StringToHGlobalAnsi(inSignedRawTransaction);
			TransactionHashPtr = Marshal.StringToHGlobalAnsi(inTransactionHash);
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
			RPtr = Marshal.StringToHGlobalAnsi(inR);
			SPtr = Marshal.StringToHGlobalAnsi(inS);
			VPtr = Marshal.StringToHGlobalAnsi(inV);
		}

		internal string SignedRawTransaction
		{
			get => Marshal.PtrToStringAnsi(SignedRawTransactionPtr);
			set
			{
				if (SignedRawTransactionPtr != IntPtr.Zero) Marshal.FreeHGlobal(SignedRawTransactionPtr);
				SignedRawTransactionPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string TransactionHash
		{
			get => Marshal.PtrToStringAnsi(TransactionHashPtr);
			set
			{
				if (TransactionHashPtr != IntPtr.Zero) Marshal.FreeHGlobal(TransactionHashPtr);
				TransactionHashPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string R
		{
			get => Marshal.PtrToStringAnsi(RPtr);
			set
			{
				if (RPtr != IntPtr.Zero) Marshal.FreeHGlobal(RPtr);
				RPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string S
		{
			get => Marshal.PtrToStringAnsi(SPtr);
			set
			{
				if (SPtr != IntPtr.Zero) Marshal.FreeHGlobal(SPtr);
				SPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string V
		{
			get => Marshal.PtrToStringAnsi(VPtr);
			set
			{
				if (VPtr != IntPtr.Zero) Marshal.FreeHGlobal(VPtr);
				VPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SignedRawTransactionPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignedRawTransactionPtr);
				SignedRawTransactionPtr = IntPtr.Zero;
			}

			if (TransactionHashPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(TransactionHashPtr);
				TransactionHashPtr = IntPtr.Zero;
			}

			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}

			if (RPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(RPtr);
				RPtr = IntPtr.Zero;
			}

			if (SPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SPtr);
				SPtr = IntPtr.Zero;
			}

			if (VPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(VPtr);
				VPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTransactionResultResponseSolanaInterop : IMessage, IDisposable
	{
		private IntPtr SignaturePtr;

		internal MMKSignTransactionResultResponseSolanaInterop(string inSignature)
		{
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTransactionResultResponseEOSInterop : IMessage, IDisposable
	{
		private IntPtr SignaturePtr;

		internal MMKSignTransactionResultResponseEOSInterop(string inSignature)
		{
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MMKSignTransactionResultResponse
	{
		[FieldOffset(4)] internal readonly MMKSignTransactionResultResponseEVMInterop responseEVM;
		[FieldOffset(4)] internal readonly MMKSignTransactionResultResponseSolanaInterop responseSolana;
		[FieldOffset(4)] internal readonly MMKSignTransactionResultResponseEOSInterop responseEOS;

		[FieldOffset(0)] [MarshalAs(UnmanagedType.U4)]
		internal readonly MKResponseType type;

		internal MMKSignTransactionResultResponse(MMKSignTransactionResultResponseEVMInterop responseEVM) : this()
		{
			type = MKResponseType.MKResponseEVM;
			this.responseEVM = responseEVM;
		}

		internal MMKSignTransactionResultResponse(MMKSignTransactionResultResponseSolanaInterop responseSolana) : this()
		{
			type = MKResponseType.MKResponseSolana;
			this.responseSolana = responseSolana;
		}

		internal MMKSignTransactionResultResponse(MMKSignTransactionResultResponseEOSInterop responseEOS) : this()
		{
			type = MKResponseType.MKResponseEOS;
			this.responseEOS = responseEOS;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignMessageResultResponseEVMInterop : IMessage, IDisposable
	{
		private IntPtr SignaturePtr;
		private IntPtr RPtr;
		private IntPtr SPtr;
		private IntPtr VPtr;

		internal MMKSignMessageResultResponseEVMInterop(string inSignature, string inR, string inS, string inV)
		{
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
			RPtr = Marshal.StringToHGlobalAnsi(inR);
			SPtr = Marshal.StringToHGlobalAnsi(inS);
			VPtr = Marshal.StringToHGlobalAnsi(inV);
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string R
		{
			get => Marshal.PtrToStringAnsi(RPtr);
			set
			{
				if (RPtr != IntPtr.Zero) Marshal.FreeHGlobal(RPtr);
				RPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string S
		{
			get => Marshal.PtrToStringAnsi(SPtr);
			set
			{
				if (SPtr != IntPtr.Zero) Marshal.FreeHGlobal(SPtr);
				SPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string V
		{
			get => Marshal.PtrToStringAnsi(VPtr);
			set
			{
				if (VPtr != IntPtr.Zero) Marshal.FreeHGlobal(VPtr);
				VPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}

			if (RPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(RPtr);
				RPtr = IntPtr.Zero;
			}

			if (SPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SPtr);
				SPtr = IntPtr.Zero;
			}

			if (VPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(VPtr);
				VPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignMessageResultResponseSolanaInterop : IMessage, IDisposable
	{
		private IntPtr SignaturePtr;

		internal MMKSignMessageResultResponseSolanaInterop(string inSignature)
		{
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignMessageResultResponseEOSInterop : IMessage, IDisposable
	{
		private IntPtr SignaturePtr;

		internal MMKSignMessageResultResponseEOSInterop(string inSignature)
		{
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MMKSignMessageResultResponse
	{
		[FieldOffset(4)] internal readonly MMKSignMessageResultResponseEVMInterop responseEVM;
		[FieldOffset(4)] internal readonly MMKSignMessageResultResponseSolanaInterop responseSolana;
		[FieldOffset(4)] internal readonly MMKSignMessageResultResponseEOSInterop responseEOS;

		[FieldOffset(0)] [MarshalAs(UnmanagedType.U4)]
		internal readonly MKResponseType type;

		internal MMKSignMessageResultResponse(MMKSignMessageResultResponseEVMInterop responseEVM) : this()
		{
			type = MKResponseType.MKResponseEVM;
			this.responseEVM = responseEVM;
		}

		internal MMKSignMessageResultResponse(MMKSignMessageResultResponseSolanaInterop responseSolana) : this()
		{
			type = MKResponseType.MKResponseSolana;
			this.responseSolana = responseSolana;
		}

		internal MMKSignMessageResultResponse(MMKSignMessageResultResponseEOSInterop responseEOS) : this()
		{
			type = MKResponseType.MKResponseEOS;
			this.responseEOS = responseEOS;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignMessage : IMessage
	{
		private IntPtr MessagePtr;
		private IntPtr ReasonPtr;

		internal MMKSignMessage(string inMessage, string inReason)
		{
			MessagePtr = Marshal.StringToHGlobalAnsi(inMessage);
			ReasonPtr = Marshal.StringToHGlobalAnsi(inReason);
		}

		internal string Message
		{
			get => Marshal.PtrToStringAnsi(MessagePtr);
			set
			{
				if (MessagePtr != IntPtr.Zero) Marshal.FreeHGlobal(MessagePtr);
				MessagePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string Reason
		{
			get => Marshal.PtrToStringAnsi(ReasonPtr);
			set
			{
				if (ReasonPtr != IntPtr.Zero) Marshal.FreeHGlobal(ReasonPtr);
				ReasonPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (MessagePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(MessagePtr);
				MessagePtr = IntPtr.Zero;
			}

			if (ReasonPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(ReasonPtr);
				ReasonPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTransaction : IMessage
	{
		private IntPtr MessagePtr;
		private IntPtr ReasonPtr;

		internal MMKSignTransaction(string inMessage, string inReason)
		{
			MessagePtr = Marshal.StringToHGlobalAnsi(inMessage);
			ReasonPtr = Marshal.StringToHGlobalAnsi(inReason);
		}

		internal string Message
		{
			get => Marshal.PtrToStringAnsi(MessagePtr);
			set
			{
				if (MessagePtr != IntPtr.Zero) Marshal.FreeHGlobal(MessagePtr);
				MessagePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string Reason
		{
			get => Marshal.PtrToStringAnsi(ReasonPtr);
			set
			{
				if (ReasonPtr != IntPtr.Zero) Marshal.FreeHGlobal(ReasonPtr);
				ReasonPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (MessagePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(MessagePtr);
				MessagePtr = IntPtr.Zero;
			}

			if (ReasonPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(ReasonPtr);
				ReasonPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTypedDataResult : IMessage, IDisposable
	{
		private IntPtr StatusPtr;
		private IntPtr SignaturePtr;
		private IntPtr RPtr;
		private IntPtr SPtr;
		private IntPtr VPtr;

		internal MMKSignTypedDataResult(string inStatus, string inSignature, string inR, string inS, string inV)
		{
			StatusPtr = Marshal.StringToHGlobalAnsi(inStatus);
			SignaturePtr = Marshal.StringToHGlobalAnsi(inSignature);
			RPtr = Marshal.StringToHGlobalAnsi(inR);
			SPtr = Marshal.StringToHGlobalAnsi(inS);
			VPtr = Marshal.StringToHGlobalAnsi(inV);
		}

		internal string Status
		{
			get => Marshal.PtrToStringAnsi(StatusPtr);
			set
			{
				if (StatusPtr != IntPtr.Zero) Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string Signature
		{
			get => Marshal.PtrToStringAnsi(SignaturePtr);
			set
			{
				if (SignaturePtr != IntPtr.Zero) Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string R
		{
			get => Marshal.PtrToStringAnsi(RPtr);
			set
			{
				if (RPtr != IntPtr.Zero) Marshal.FreeHGlobal(RPtr);
				RPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string S
		{
			get => Marshal.PtrToStringAnsi(SPtr);
			set
			{
				if (SPtr != IntPtr.Zero) Marshal.FreeHGlobal(SPtr);
				SPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		internal string V
		{
			get => Marshal.PtrToStringAnsi(VPtr);
			set
			{
				if (VPtr != IntPtr.Zero) Marshal.FreeHGlobal(VPtr);
				VPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (StatusPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = IntPtr.Zero;
			}

			if (SignaturePtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SignaturePtr);
				SignaturePtr = IntPtr.Zero;
			}

			if (RPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(RPtr);
				RPtr = IntPtr.Zero;
			}

			if (SPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(SPtr);
				SPtr = IntPtr.Zero;
			}

			if (VPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(VPtr);
				VPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignMessageResult : IMessage, IDisposable
	{
		private IntPtr StatusPtr;
		internal MMKSignMessageResultResponse Response { get; set; }

		internal MMKSignMessageResult(string inStatus, MMKSignMessageResultResponse inResponse)
		{
			StatusPtr = Marshal.StringToHGlobalAnsi(inStatus);
			Response = inResponse;
		}

		internal string Status
		{
			get => Marshal.PtrToStringAnsi(StatusPtr);
			set
			{
				if (StatusPtr != IntPtr.Zero) Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (StatusPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
	internal struct MMKSignTransactionResult : IMessage, IDisposable
	{
		private IntPtr StatusPtr;
		internal MMKSignTransactionResultResponse Response { get; set; }

		internal MMKSignTransactionResult(string inStatus, MMKSignTransactionResultResponse inResponse)
		{
			StatusPtr = Marshal.StringToHGlobalAnsi(inStatus);
			Response = inResponse;
		}

		internal string Status
		{
			get => Marshal.PtrToStringAnsi(StatusPtr);
			set
			{
				if (StatusPtr != IntPtr.Zero) Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = Marshal.StringToHGlobalAnsi(value);
			}
		}

		public void Dispose()
		{
			if (StatusPtr != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(StatusPtr);
				StatusPtr = IntPtr.Zero;
			}
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct MEmpty : IMessage
	{
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	internal struct MessageUnion
	{
		[FieldOffset(4)] internal readonly MCheckout checkout;
		[FieldOffset(4)] internal readonly MCheckoutResult checkoutResult;
		[FieldOffset(4)] internal readonly MFeatureFlags featureFlags;
		[FieldOffset(4)] internal readonly MEmpty empty;
		[FieldOffset(4)] internal readonly MLanguage language;
		[FieldOffset(4)] internal readonly MOpenStateChange openStateChange;
		[FieldOffset(4)] internal readonly MToken token;
		[FieldOffset(4)] internal readonly MMKGetWallet metaKeepGetWallet;
		[FieldOffset(4)] internal readonly MMKGetWalletResult metaKeepGetWalletResult;
		[FieldOffset(4)] internal readonly MMKSignTypedData metaKeepSignTypedData;
		[FieldOffset(4)] internal readonly MMKSignTypedDataResult metaKeepSignTypedDataResult;
		[FieldOffset(4)] internal readonly MMKSignMessage metaKeepSignMessage;
		[FieldOffset(4)] internal readonly MMKSignMessageResult metaKeepSignMessageResult;
		[FieldOffset(4)] internal readonly MMKSignTransaction metaKeepSignTransaction;
		[FieldOffset(4)] internal readonly MMKSignTransactionResult metaKeepSignTransactionResult;

		[FieldOffset(0)] [MarshalAs(UnmanagedType.U4)]
		internal readonly MessageType type;

		internal MessageUnion(MToken token) : this()
		{
			type = MessageType.MTToken;
			this.token = token;
		}

		internal MessageUnion(MOpenStateChange openStateChange) : this()
		{
			type = MessageType.MTOpenStateChange;
			this.openStateChange = openStateChange;
		}

		internal MessageUnion(MCheckout checkout) : this()
		{
			type = MessageType.MTCheckout;
			this.checkout = checkout;
		}

		internal MessageUnion(MCheckoutResult checkoutResult) : this()
		{
			type = MessageType.MTCheckoutResult;
			this.checkoutResult = checkoutResult;
		}

		internal MessageUnion(MFeatureFlags featureFlags) : this()
		{
			type = MessageType.MTFeatureFlags;
			this.featureFlags = featureFlags;
		}

		internal MessageUnion(MLanguage language) : this()
		{
			type = MessageType.MTLanguage;
			this.language = language;
		}

		internal MessageUnion(MMKGetWallet metaKeepGetWallet) : this()
		{
			type = MessageType.MTMKGetWallet;
			this.metaKeepGetWallet = metaKeepGetWallet;
		}

		internal MessageUnion(MMKGetWalletResult metaKeepGetWalletResult) : this()
		{
			type = MessageType.MTMKGetWalletResult;
			this.metaKeepGetWalletResult = metaKeepGetWalletResult;
		}

		internal MessageUnion(MMKSignTypedData metaKeepSignTypedData) : this()
		{
			type = MessageType.MTMKSignTypedData;
			this.metaKeepSignTypedData = metaKeepSignTypedData;
		}

		internal MessageUnion(MMKSignTypedDataResult metaKeepSignTypedDataResult) : this()
		{
			type = MessageType.MTMKSignTypedDataResult;
			this.metaKeepSignTypedDataResult = metaKeepSignTypedDataResult;
		}

		internal MessageUnion(MMKSignMessage metaKeepSignMessage) : this()
		{
			type = MessageType.MTMKSignMessage;
			this.metaKeepSignMessage = metaKeepSignMessage;
		}

		internal MessageUnion(MMKSignMessageResult metaKeepSignMessageResult) : this()
		{
			type = MessageType.MTMKSignMessageResult;
			this.metaKeepSignMessageResult = metaKeepSignMessageResult;
		}

		internal MessageUnion(MMKSignTransaction metaKeepSignTransaction) : this()
		{
			type = MessageType.MTMKSignTransaction;
			this.metaKeepSignTransaction = metaKeepSignTransaction;
		}

		internal MessageUnion(MMKSignTransactionResult metaKeepSignTransactionResult) : this()
		{
			type = MessageType.MTMKSignTransactionResult;
			this.metaKeepSignTransactionResult = metaKeepSignTransactionResult;
		}

		internal MessageUnion(MEmpty empty) : this()
		{
			type = MessageType.MTEmpty;
			this.empty = empty;
		}
	}

	internal static class OverlayMessage
	{
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        private const string DllName = "libraven_shared.1.0.0.dylib";
#elif UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
        private const string DllName = "libraven_shared.so.1.0.0";
#else
		private const string DllName = "raven_shared.dll";
#endif

		private static string _eventBufferGameSdkId;
		private static IntPtr _eventBufferGameSdk;
		private static string _eventBufferOverlayUiId;
		private static IntPtr _eventBufferOverlayUi;
		private static CancellationTokenSource _gameCancellationTokenSource;
		private static CancellationTokenSource _simulatorCancellationTokenSource;
		private static MainThreadQueue _mainThreadQueue;
		internal static bool IsSimulating { get; private set; }

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern IntPtr CreateEventBuffer(string bufferName);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void DestroyEventBuffer(IntPtr eventBuffer);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern MessageUnion ListenToEventBuffer(IntPtr eventBuffer);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern string GetEventBufferError(IntPtr eventBuffer);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void ClearEventBufferError(IntPtr eventBuffer);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern void ClearEventBuffer(IntPtr eventBuffer);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
		private static extern long WriteToEventBuffer(IntPtr eventBuffer, ref MessageUnion message);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferCheckout(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string sku);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferCheckoutResult(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.I1)] bool result, [MarshalAs(UnmanagedType.LPStr)] string sku);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferOpenStateChange(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.I1)] bool openState);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferGetWallet(IntPtr eventBuffer);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferGetWalletResult(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string status,
			[MarshalAs(UnmanagedType.LPStr)] string ethAddress,
			[MarshalAs(UnmanagedType.LPStr)] string solAddress,
			[MarshalAs(UnmanagedType.LPStr)] string eosAddress);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferSignTypedData(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string message,
			[MarshalAs(UnmanagedType.LPStr)] string reason);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferSignTypedDataResult(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string status,
			[MarshalAs(UnmanagedType.LPStr)] string signature,
			[MarshalAs(UnmanagedType.LPStr)] string r,
			[MarshalAs(UnmanagedType.LPStr)] string s,
			[MarshalAs(UnmanagedType.LPStr)] string v);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferSignMessage(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string message,
			[MarshalAs(UnmanagedType.LPStr)] string reason);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferSignMessageResult(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string status, MMKSignMessageResultResponse response);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferSignTransaction(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string message,
			[MarshalAs(UnmanagedType.LPStr)] string reason);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferSignTransactionResult(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.LPStr)] string status, MMKSignTransactionResultResponse response);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern IntPtr GetEventBufferOverlayUi();

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern IntPtr GetEventBufferGameSdk();

		private static string GetEventBufferOverlayUiManaged()
		{
			var ptr = GetEventBufferOverlayUi();
			return Marshal.PtrToStringAnsi(ptr);
		}

		private static string GetEventBufferGameSdkManaged()
		{
			var ptr = GetEventBufferGameSdk();
			return Marshal.PtrToStringAnsi(ptr);
		}

		private static IMessage ExtractMessage(MessageUnion union)
		{
			return union.type switch
			{
				MessageType.MTToken => union.token,
				MessageType.MTOpenStateChange => union.openStateChange,
				MessageType.MTCheckout => union.checkout,
				MessageType.MTCheckoutResult => union.checkoutResult,
				MessageType.MTFeatureFlags => union.featureFlags,
				MessageType.MTLanguage => union.language,
				MessageType.MTMKGetWallet => union.metaKeepGetWallet,
				MessageType.MTMKGetWalletResult => union.metaKeepGetWalletResult,
				MessageType.MTMKSignTypedData => union.metaKeepSignTypedData,
				MessageType.MTMKSignTypedDataResult => union.metaKeepSignTypedDataResult,
				MessageType.MTMKSignMessage => union.metaKeepSignMessage,
				MessageType.MTMKSignMessageResult => union.metaKeepSignMessageResult,
				MessageType.MTMKSignTransaction => union.metaKeepSignTransaction,
				MessageType.MTMKSignTransactionResult => union.metaKeepSignTransactionResult,
				MessageType.MTEmpty => union.empty,
				_ => null
			};
		}

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
		private static async Task Listen(CancellationTokenSource cancellationTokenSource, IntPtr eventBuffer,
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
			MessageCallback callback)
		{
			while (!cancellationTokenSource.Token.IsCancellationRequested)
			{
				var union = ListenToEventBuffer(eventBuffer);
				var message = ExtractMessage(union);
				if (!(message is MEmpty))
					_mainThreadQueue.Enqueue(() => { callback(message); });
			}
		}

		private static void EnsureInitMainThreadQueue()
		{
			_mainThreadQueue ??= new MainThreadQueue();
		}

		private static void EnsureInitEventBufferEventOverlay()
		{
			if (_eventBufferOverlayUi == IntPtr.Zero)
			{
				_eventBufferOverlayUiId = GetEventBufferOverlayUiManaged();
				_eventBufferOverlayUi = CreateEventBuffer(_eventBufferOverlayUiId);
			}

			ClearEventBuffer(_eventBufferOverlayUi);

			var error = GetEventBufferError(_eventBufferOverlayUi);
			if (string.IsNullOrEmpty(error)) return;
			ClearEventBufferError(_eventBufferOverlayUi);
			throw new ExternalException(error);
		}

		private static void EnsureInitEventBufferEventGameSdk()
		{
			if (_eventBufferGameSdk == IntPtr.Zero)
			{
				_eventBufferGameSdkId = GetEventBufferGameSdkManaged();
				_eventBufferGameSdk = CreateEventBuffer(_eventBufferGameSdkId);
			}

			ClearEventBuffer(_eventBufferGameSdk);

			var error = GetEventBufferError(_eventBufferGameSdk);
			if (string.IsNullOrEmpty(error)) return;
			ClearEventBufferError(_eventBufferGameSdk);
			throw new ExternalException(error);
		}


		internal static void Init(MessageCallback callback)
		{
			if (callback == null) throw new Exception("Missing callback");
			if (_eventBufferGameSdk != IntPtr.Zero)
				throw new Exception("Already initialized");

			EnsureInitMainThreadQueue();
			EnsureInitEventBufferEventGameSdk();
			EnsureInitEventBufferEventOverlay();

			_gameCancellationTokenSource = new CancellationTokenSource();
			Task.Run(() => Listen(_gameCancellationTokenSource, _eventBufferGameSdk, callback));
		}

		internal static void StopListening()
		{
			_gameCancellationTokenSource.Cancel();
			_mainThreadQueue = null;
			if (_eventBufferGameSdk != IntPtr.Zero) DestroyEventBuffer(_eventBufferGameSdk);
			if (_eventBufferOverlayUi != IntPtr.Zero) DestroyEventBuffer(_eventBufferOverlayUi);
			_eventBufferGameSdk = IntPtr.Zero;
			_eventBufferOverlayUi = IntPtr.Zero;
		}

		internal static void Simulator_StartSimulating(MessageCallback callback)
		{
			if (IsSimulating) throw new Exception("Already simulating");
			EnsureInitMainThreadQueue();
			EnsureInitEventBufferEventGameSdk();
			EnsureInitEventBufferEventOverlay();

			_simulatorCancellationTokenSource = new CancellationTokenSource();
			IsSimulating = true;

			Task.Run(() =>
			{
				try
				{
					return Listen(_simulatorCancellationTokenSource, _eventBufferOverlayUi, callback);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					throw;
				}
			});
		}

		internal static void Simulator_StopSimulating()
		{
			_simulatorCancellationTokenSource?.Cancel();
			IsSimulating = false;
		}

		internal static void Update()
		{
			if (_mainThreadQueue != null) _mainThreadQueue.Update();
		}

		internal static bool Checkout(string sku)
		{
			if (sku.Length == 0) throw new Exception("Invalid empty Sku");
			var bytesWritten = WriteToEventBufferCheckout(_eventBufferOverlayUi, sku);
			return bytesWritten > 0;
		}

		internal static bool Simulator_CheckoutResult(bool result, string sku)
		{
			if (sku.Length == 0) throw new Exception("Invalid empty Sku");
			var bytesWritten = WriteToEventBufferCheckoutResult(_eventBufferGameSdk, result, sku);
			return bytesWritten > 0;
		}

		internal static bool Simulator_OpenStateChange(bool openState)
		{
			var bytesWritten = WriteToEventBufferOpenStateChange(_eventBufferGameSdk, openState);
			return bytesWritten > 0;
		}

		internal static bool GetWallet()
		{
			var bytesWritten = WriteToEventBufferGetWallet(_eventBufferOverlayUi);
			return bytesWritten > 0;
		}

		internal static bool Simulator_GetWalletResult(string status, string ethAddress, string solAddress,
			string eosAddress)
		{
			if (status.Length == 0) throw new Exception("Invalid empty Status");
			var bytesWritten =
				WriteToEventBufferGetWalletResult(_eventBufferGameSdk, status, ethAddress, solAddress, eosAddress);
			return bytesWritten > 0;
		}

		internal static bool SignTypedData(string message, string reason)
		{
			if (message.Length == 0) throw new Exception("Invalid empty Message");
			if (reason.Length == 0) throw new Exception("Invalid empty Reason");
			var bytesWritten = WriteToEventBufferSignTypedData(_eventBufferOverlayUi, message, reason);
			return bytesWritten > 0;
		}

		internal static bool Simulator_SignTypedDataResult(string status, string signature, string r,
			string s,
			string v)
		{
			if (status.Length == 0) throw new Exception("Invalid empty Status");
			var bytesWritten =
				WriteToEventBufferSignTypedDataResult(_eventBufferGameSdk, status, signature, r, s,
					v);
			return bytesWritten > 0;
		}

		internal static bool SignMessage(string message, string reason)
		{
			if (message.Length == 0) throw new Exception("Invalid empty Message");
			if (reason.Length == 0) throw new Exception("Invalid empty Reason");
			var bytesWritten = WriteToEventBufferSignMessage(_eventBufferOverlayUi, message, reason);
			return bytesWritten > 0;
		}

		internal static bool Simulator_SignMessageResult(string status, MMKSignMessageResultResponse response)
		{
			if (status.Length == 0) throw new Exception("Invalid empty Status");
			var bytesWritten = WriteToEventBufferSignMessageResult(_eventBufferGameSdk, status, response);
			return bytesWritten > 0;
		}

		internal static bool SignTransaction(string message, string reason)
		{
			if (message.Length == 0) throw new Exception("Invalid empty Message");
			if (reason.Length == 0) throw new Exception("Invalid empty Reason");
			var bytesWritten = WriteToEventBufferSignTransaction(_eventBufferOverlayUi, message, reason);
			return bytesWritten > 0;
		}

		internal static bool Simulator_SignTransactionResult(string status, MMKSignTransactionResultResponse response)
		{
			if (status.Length == 0) throw new Exception("Invalid empty Status");
			var bytesWritten = WriteToEventBufferSignTransactionResult(_eventBufferGameSdk, status, response);
			return bytesWritten > 0;
		}

		internal delegate void MessageCallback(IMessage message);
	}
}