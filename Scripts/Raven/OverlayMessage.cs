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
	internal enum MessageType
	{
		MTEmpty = 0,
		MTToken = 1,
		MTOpenStateChange = 2,
		MTCheckout = 3,
		MTCheckoutResult = 4,
		MTFeatureFlags = 5,
		MTLanguage = 6,
		MTSetVisibility = 7
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

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
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
	internal struct MEmpty : IMessage
	{
	}

	[StructLayout(LayoutKind.Explicit)]
	internal struct MessageUnion
	{
		[FieldOffset(4)] internal readonly MCheckout checkout;
		[FieldOffset(4)] internal readonly MCheckoutResult checkoutResult;
		[FieldOffset(4)] internal readonly MFeatureFlags featureFlags;
		[FieldOffset(4)] internal readonly MEmpty empty;
		[FieldOffset(4)] internal readonly MLanguage language;
		[FieldOffset(4)] internal readonly MOpenStateChange openStateChange;
		[FieldOffset(4)] internal readonly MToken token;
		[FieldOffset(0)] internal readonly MessageType type;

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

		internal MessageUnion(MEmpty empty) : this()
		{
			type = MessageType.MTEmpty;
			this.empty = empty;
		}
	}

	internal static class OverlayMessage
	{
		private const string DllName = "raven_shared.dll";

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
				MessageType.MTFeatureFlags => union.featureFlags,
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

		internal delegate void MessageCallback(IMessage message);
	}
}