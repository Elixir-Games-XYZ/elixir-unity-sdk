namespace Elixir.Overlay
{
	public static class Event
	{
		public delegate void OnCheckoutResultDelegate(bool result, string sku);

		public delegate void OnOpenStateChangeDelegate(bool isOpen);

		public static OnCheckoutResultDelegate OnCheckoutResult { get; set; }
		public static OnOpenStateChangeDelegate OnOpenStateChange { get; set; }

		public static bool Checkout(string sku)
		{
			return OverlayMessage.Checkout(sku);
		}
	}
}