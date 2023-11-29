using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Elixir.Overlay
{
	public enum MessageType
	{
		MTToken = 0,
		MTOpenStateChange = 1,
		MTCheckout = 2,
		MTCheckoutResult = 3,
		MTElixirPro = 4,
		MTLanguage = 5,
		MTSetVisibility = 6,
		MTEmpty = 7
	}

	public interface IMessage
	{
	}

	public class MainThreadQueue
	{
		// anything on this queue will be executed on the main thread in Update()
		private static readonly Queue<Action> _executionQueue = new Queue<Action>();

		public void Enqueue(Action action)
		{
			_executionQueue.Enqueue(action);
		}

		public void Update()
		{
			lock (_executionQueue)
			{
				while (_executionQueue.Count > 0) _executionQueue.Dequeue().Invoke();
			}
		}
	}


	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct MToken : IMessage, IDisposable
	{
		private IntPtr TokenPtr;

		public MToken(string token)
		{
			TokenPtr = Marshal.StringToHGlobalAnsi(token);
		}

		public string Token
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
	public struct MOpenStateChange : IMessage
	{
		[MarshalAs(UnmanagedType.I1)] public bool IsOpen;

		public MOpenStateChange(bool inIsOpen)
		{
			IsOpen = inIsOpen;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct MCheckout : IMessage, IDisposable
	{
		private IntPtr SkuPtr;

		public MCheckout(string inSku)
		{
			SkuPtr = Marshal.StringToHGlobalAnsi(inSku);
		}

		public string Sku
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
	public struct MCheckoutResult : IMessage, IDisposable
	{
		[MarshalAs(UnmanagedType.I1)] public bool Success;
		private IntPtr SkuPtr;

		public MCheckoutResult(bool inSuccess, string inSku)
		{
			Success = inSuccess;
			SkuPtr = Marshal.StringToHGlobalAnsi(inSku);
		}

		public string Sku
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

	[StructLayout(LayoutKind.Sequential)]
	public struct MElixirPro : IMessage
	{
		[MarshalAs(UnmanagedType.I1)] public bool Enabled;

		public MElixirPro(bool inEnabled)
		{
			Enabled = inEnabled;
		}
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public struct MLanguage : IMessage, IDisposable
	{
		private IntPtr LanguagePtr;

		public MLanguage(string language)
		{
			LanguagePtr = Marshal.StringToHGlobalAnsi(language);
		}

		public string Language
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
	public struct MEmpty : IMessage
	{
	}

	[StructLayout(LayoutKind.Explicit)]
	public struct MessageUnion
	{
		[FieldOffset(4)] public readonly MCheckout checkout;
		[FieldOffset(4)] public readonly MCheckoutResult checkoutResult;
		[FieldOffset(4)] public readonly MElixirPro elixirPro;
		[FieldOffset(4)] public readonly MEmpty empty;
		[FieldOffset(4)] public readonly MLanguage language;
		[FieldOffset(4)] public readonly MOpenStateChange openStateChange;
		[FieldOffset(4)] public readonly MToken token;
		[FieldOffset(0)] public readonly MessageType type;

		public MessageUnion(MToken token) : this()
		{
			type = MessageType.MTToken;
			this.token = token;
		}

		public MessageUnion(MOpenStateChange openStateChange) : this()
		{
			type = MessageType.MTOpenStateChange;
			this.openStateChange = openStateChange;
		}

		public MessageUnion(MCheckout checkout) : this()
		{
			type = MessageType.MTCheckout;
			this.checkout = checkout;
		}

		public MessageUnion(MCheckoutResult checkoutResult) : this()
		{
			type = MessageType.MTCheckoutResult;
			this.checkoutResult = checkoutResult;
		}

		public MessageUnion(MElixirPro elixirPro) : this()
		{
			type = MessageType.MTElixirPro;
			this.elixirPro = elixirPro;
		}

		public MessageUnion(MLanguage language) : this()
		{
			type = MessageType.MTLanguage;
			this.language = language;
		}

		public MessageUnion(MEmpty empty) : this()
		{
			type = MessageType.MTEmpty;
			this.empty = empty;
		}
	}

	public static class OverlayMessage
	{
		public delegate void MessageCallback(IMessage message);

		private const string DllName = "raven_shared.dll";

		private static string _eventBufferGameSdkId;
		private static IntPtr _eventBufferGameSdk;
		private static string _eventBufferOverlayUiId;
		private static IntPtr _eventBufferOverlayUi;
		private static CancellationTokenSource _gameCancellationTokenSource;
		private static CancellationTokenSource _simulatorCancellationTokenSource;
		private static MainThreadQueue _mainThreadQueue;
		public static bool _Dangerous_IsSimulating { get; private set; }

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
		private static extern long WriteToEventBufferCheckout(IntPtr eventBuffer, string sku);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferCheckoutResult(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.I1)] bool result, string sku);

		[DllImport(DllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
		private static extern long WriteToEventBufferOpenStateChange(IntPtr eventBuffer,
			[MarshalAs(UnmanagedType.I1)] bool openState);

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
				MessageType.MTElixirPro => union.elixirPro,
				MessageType.MTLanguage => union.language,
				MessageType.MTEmpty => union.empty,
				_ => null
			};
		}

		private static async Task Listen(CancellationTokenSource cancellationTokenSource, IntPtr eventBuffer,
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

		public static void Init(MessageCallback callback)
		{
			if (callback == null) throw new Exception("Missing callback");
			if (_eventBufferGameSdk != IntPtr.Zero)
				throw new Exception("Already initialized");

			_eventBufferGameSdkId = GetEventBufferGameSdkManaged();
			_eventBufferGameSdk = CreateEventBuffer(_eventBufferGameSdkId);
			ClearEventBuffer(_eventBufferGameSdk);

			var error = GetEventBufferError(_eventBufferGameSdk);
			if (!string.IsNullOrEmpty(error))
			{
				ClearEventBufferError(_eventBufferGameSdk);
				throw new ExternalException(error);
			}

			EnsureInitMainThreadQueue();
			EnsureInitEventBufferEventOverlay();

			_gameCancellationTokenSource = new CancellationTokenSource();
			Task.Run(() => Listen(_gameCancellationTokenSource, _eventBufferGameSdk, callback));
		}

		public static void StopListening()
		{
			_gameCancellationTokenSource.Cancel();
			_mainThreadQueue = null;
			if (_eventBufferGameSdk != IntPtr.Zero) DestroyEventBuffer(_eventBufferGameSdk);
			if (_eventBufferOverlayUi != IntPtr.Zero) DestroyEventBuffer(_eventBufferOverlayUi);
			_eventBufferGameSdk = IntPtr.Zero;
			_eventBufferOverlayUi = IntPtr.Zero;
		}

		public static void _Dangerous_StartSimulating(MessageCallback callback)
		{
			if (_Dangerous_IsSimulating) throw new Exception("Already simulating");
			EnsureInitMainThreadQueue();
			EnsureInitEventBufferEventOverlay();
			_simulatorCancellationTokenSource = new CancellationTokenSource();
			_Dangerous_IsSimulating = true;

			Task.Run(() =>
			{
				try
				{
					return Listen(_simulatorCancellationTokenSource, _eventBufferOverlayUi, callback);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			});
		}

		public static void _Dangerous_StopSimulating()
		{
			_simulatorCancellationTokenSource?.Cancel();
			_Dangerous_IsSimulating = false;
		}

		public static void Update()
		{
			if (_mainThreadQueue != null) _mainThreadQueue.Update();
		}

		public static bool Checkout(string sku)
		{
			if (sku.Length == 0) throw new Exception("Invalid empty Sku");
			var bytesWritten = WriteToEventBufferCheckout(_eventBufferOverlayUi, sku);
			return bytesWritten > 0;
		}

		public static bool _Dangerous_CheckoutResult(bool result, string sku)
		{
			if (sku.Length == 0) throw new Exception("Invalid empty Sku");
			var bytesWritten = WriteToEventBufferCheckoutResult(_eventBufferGameSdk, result, sku);
			return bytesWritten > 0;
		}

		public static bool _Dangerous_OpenStateChange(bool openState)
		{
			var bytesWritten = WriteToEventBufferOpenStateChange(_eventBufferGameSdk, openState);
			return bytesWritten > 0;
		}
	}
}