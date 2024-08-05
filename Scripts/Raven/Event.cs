namespace Elixir.Overlay
{
	public static class Event
	{
		public delegate void OnCheckoutResultDelegate(bool result, string sku);

		public delegate void OnGetWalletResultDelegate(string status, string ethAddress, string solAddress,
			string eosAddress);

		public delegate void OnOpenStateChangeDelegate(bool isOpen);

		public delegate void OnSignMessageResultDelegate(string status, SignMessageResultEVM? responseEVM,
			SignMessageResultSolana? responseSolana, SignMessageResultEOS? responseEOS);

		public delegate void OnSignTransactionResultDelegate(string status, SignTransactionResultEVM? responseEVM,
			SignTransactionResultSolana? responseSolana, SignTransactionResultEOS? responseEOS);

		public delegate void OnSignTypedDataResultDelegate(string status, string signature, string r, string s, string v);

		public static OnCheckoutResultDelegate OnCheckoutResult { get; set; }
		public static OnOpenStateChangeDelegate OnOpenStateChange { get; set; }
		public static OnGetWalletResultDelegate OnGetWalletResult { get; set; }
		public static OnSignTypedDataResultDelegate OnSignTypedDataResult { get; set; }
		public static OnSignMessageResultDelegate OnSignMessageResult { get; set; }
		public static OnSignTransactionResultDelegate OnSignTransactionResult { get; set; }

		public static bool Checkout(string sku)
		{
			return OverlayMessage.Checkout(sku);
		}

		public static bool GetWallet()
		{
			return OverlayMessage.GetWallet();
		}

		public static bool SignTypedData(string message, string reason)
		{
			return OverlayMessage.SignTypedData(message, reason);
		}

		public static bool SignMessage(string message, string reason)
		{
			return OverlayMessage.SignMessage(message, reason);
		}

		public static bool SignTransaction(string message, string reason)
		{
			return OverlayMessage.SignTransaction(message, reason);
		}
	}
}