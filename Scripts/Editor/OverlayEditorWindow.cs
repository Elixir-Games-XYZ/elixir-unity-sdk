using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Elixir.Overlay
{
	public class OverlayEditorWindow : EditorWindow
	{
		private readonly List<IEventParameter> _currentParameters = new List<IEventParameter>();
		private readonly List<Message> _messages = new List<Message>();

		private readonly string[] _options =
		{
			"MTCheckoutResult", "MTOpenStateChange", "MTMKGetWalletResult", "MTMKSignTypedDataResult",
			"MTMKSignMessageResult", "MTMKSignTransactionResult"
		};

		private readonly string[] _responseTypeOptions =
		{
			"MKResponseEVM", "MKResponseSolana", "MKResponseEOS"
		};

		private MessageTreeView _messageTreeView;
		private MultiColumnHeaderState _multiColumnHeaderState;
		private Vector2 _scrollPosition;
		private MessageType _selectedMessageType;
		private MKResponseType _selectedResponseType;
		private TreeViewState _treeViewState;

		public void Update()
		{
			OverlayMessage.Update();
			Repaint();
		}

		private void OnEnable()
		{
			EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
			_selectedMessageType = MessageType.MTCheckoutResult;
			_selectedResponseType = MKResponseType.MKResponseEVM;
			UpdateParameterControls();
			_treeViewState = new TreeViewState();
			var headerState = MessageTreeView.CreateDefaultMultiColumnHeaderState();
			if (MultiColumnHeaderState.CanOverwriteSerializedFields(_multiColumnHeaderState, headerState))
				MultiColumnHeaderState.OverwriteSerializedFields(_multiColumnHeaderState, headerState);
			_multiColumnHeaderState = headerState;

			var multiColumnHeader = new MultiColumnHeader(headerState);
			_messageTreeView = new MessageTreeView(_treeViewState, multiColumnHeader, _messages);
		}

		private void OnDisable()
		{
			EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		}

		private void OnGUI()
		{
			var paddingStyle = new GUIStyle
			{
				padding = new RectOffset(10, 10, 5, 5)
			};

			EditorGUILayout.BeginHorizontal(paddingStyle, GUILayout.Height(20));

			using (new EditorGUI.DisabledGroupScope(!EditorApplication.isPlaying))
			{
				var buttonText = OverlayMessage.IsSimulating ? "Stop" : "Simulate";
				var buttonIcon = OverlayMessage.IsSimulating ? "d_PauseButton" : "d_PlayButton";
				if (GUILayout.Button(new GUIContent(buttonText, EditorGUIUtility.IconContent(buttonIcon).image),
					    GUILayout.Width(100)))
				{
					if (OverlayMessage.IsSimulating)
						OverlayMessage.Simulator_StopSimulating();
					else
						OverlayMessage.Simulator_StartSimulating(ProcessMessage);
				}
			}

			if (GUILayout.Button("Clear", GUILayout.Width(60)))
			{
				_messages.Clear();
				_messageTreeView.Reload();
			}

			GUILayout.FlexibleSpace();

			GUILayout.Label(OverlayMessage.IsSimulating ? "Simulating" : "Stopped");

			EditorGUILayout.EndHorizontal();


			// Middle Section - TreeView
			GUILayout.BeginVertical();
			if (_messageTreeView != null)
			{
				var treeViewRect = GUILayoutUtility.GetRect(0, 10000, 0, 10000);
				_messageTreeView.OnGUI(treeViewRect);
			}

			GUILayout.EndVertical();

			// Bottom Section
			var bottomPaddingStyle = new GUIStyle();
			bottomPaddingStyle.padding = new RectOffset(10, 10, 5, 5);

			EditorGUILayout.BeginVertical(bottomPaddingStyle);

			var newSelectedIndex =
				EditorGUILayout.Popup("Event Type", GetOptionKeyFromMessageType(_selectedMessageType),
					_options);
			if (newSelectedIndex != GetOptionKeyFromMessageType(_selectedMessageType))
			{
				_selectedMessageType = GetMessageTypeFromOption(_options[newSelectedIndex]);
				UpdateParameterControls();
				GUIUtility.ExitGUI();
			}

			var responseTypeParam = _currentParameters.Find(item => item.GetLabel() == "Response Type");
			if (responseTypeParam is DropdownParameter responseTypeParamSpecific)
				if (responseTypeParamSpecific.GetValue() != GetOptionFromResponseType(_selectedResponseType))
				{
					_selectedResponseType = GetResponseTypeFromOption(responseTypeParamSpecific.GetValue());
					UpdateParameterControls();
					GUIUtility.ExitGUI();
				}

			foreach (var param in _currentParameters)
				param.DrawUI();

			using (new EditorGUI.DisabledGroupScope(!EditorApplication.isPlaying || !OverlayMessage.IsSimulating))
			{
				if (GUILayout.Button("Send")) SendMessage();
			}

			EditorGUILayout.EndVertical();
		}

		private void SendMessage()
		{
			var payload = string.Join(", ", _currentParameters.ConvertAll(p => p.GetLogString()));
			switch (_selectedMessageType)
			{
				case MessageType.MTCheckoutResult:
					var successParam = _currentParameters.Find(item => item.GetLabel() == "Success");
					var skuParam = _currentParameters.Find(item => item.GetLabel() == "Sku");
					if (successParam is BooleanParameter successParamSpecific && skuParam is StringParameter skuParamSpecific)
						OverlayMessage.Simulator_CheckoutResult(successParamSpecific.GetValue(), skuParamSpecific.GetValue());
					else
						throw new Exception("Invalid parameters");
					break;
				case MessageType.MTOpenStateChange:
					var openStateParam = _currentParameters.Find(item => item.GetLabel() == "IsOpen");
					if (openStateParam is BooleanParameter openStateParamSpecific)
						OverlayMessage.Simulator_OpenStateChange(openStateParamSpecific.GetValue());
					else
						throw new Exception("Invalid parameters");
					break;
				case MessageType.MTMKGetWalletResult:
					var getWalletResultStatusParam = _currentParameters.Find(item => item.GetLabel() == "Status");
					var getWalletResultEthAddressParam = _currentParameters.Find(item => item.GetLabel() == "EthAddress");
					var getWalletResultSolAddressParam = _currentParameters.Find(item => item.GetLabel() == "SolAddress");
					var getWalletResultEosAddressParam = _currentParameters.Find(item => item.GetLabel() == "EosAddress");
					if (
						getWalletResultStatusParam is StringParameter getWalletResultStatusParamSpecific &&
						getWalletResultEthAddressParam is StringParameter getWalletResultEthAddressParamSpecific &&
						getWalletResultSolAddressParam is StringParameter getWalletResultSolAddressParamSpecific &&
						getWalletResultEosAddressParam is StringParameter getWalletResultEosAddressParamSpecific
					)
						OverlayMessage.Simulator_GetWalletResult(getWalletResultStatusParamSpecific.GetValue(),
							getWalletResultEthAddressParamSpecific.GetValue(), getWalletResultSolAddressParamSpecific.GetValue(),
							getWalletResultEosAddressParamSpecific.GetValue());
					else
						throw new Exception("Invalid parameters");
					break;
				case MessageType.MTMKSignTypedDataResult:
					var signTypedDataResultStatusParam = _currentParameters.Find(item => item.GetLabel() == "Status");
					var signTypedDataResultSignatureParam = _currentParameters.Find(item => item.GetLabel() == "Signature");
					var signTypedDataResultRParam = _currentParameters.Find(item => item.GetLabel() == "R");
					var signTypedDataResultSParam = _currentParameters.Find(item => item.GetLabel() == "S");
					var signTypedDataResultVParam = _currentParameters.Find(item => item.GetLabel() == "V");
					if (
						signTypedDataResultStatusParam is StringParameter signTypedDataResultStatusParamSpecific &&
						signTypedDataResultSignatureParam is StringParameter signTypedDataResultSignatureParamSpecific &&
						signTypedDataResultRParam is StringParameter signTypedDataResultRParamSpecific &&
						signTypedDataResultSParam is StringParameter signTypedDataResultSParamSpecific &&
						signTypedDataResultVParam is StringParameter signTypedDataResultVParamSpecific
					)
						OverlayMessage.Simulator_SignTypedDataResult(signTypedDataResultStatusParamSpecific.GetValue(),
							signTypedDataResultSignatureParamSpecific.GetValue(), signTypedDataResultRParamSpecific.GetValue(),
							signTypedDataResultSParamSpecific.GetValue(), signTypedDataResultVParamSpecific.GetValue());
					else
						throw new Exception("Invalid parameters");
					break;
				case MessageType.MTMKSignMessageResult:
					var signMessageResultStatusParam = _currentParameters.Find(item => item.GetLabel() == "Status");
					var signMessageResultResponseTypeParam = _currentParameters.Find(item => item.GetLabel() == "Response Type");
					var signMessageResultSignatureParam = _currentParameters.Find(item => item.GetLabel() == "Signature");
					if (
						signMessageResultSignatureParam is StringParameter signMessageResultSignatureParamSpecific &&
						signMessageResultStatusParam is StringParameter signMessageResultStatusParamSpecific &&
						signMessageResultResponseTypeParam is DropdownParameter signMessageResultResponseTypeParamSpecific
					)
					{
						var signMessageResultRParam = _currentParameters.Find(item => item.GetLabel() == "R");
						var signMessageResultSParam = _currentParameters.Find(item => item.GetLabel() == "S");
						var signMessageResultVParam = _currentParameters.Find(item => item.GetLabel() == "V");
						switch (GetResponseTypeFromOption(signMessageResultResponseTypeParamSpecific.GetValue()))
						{
							case MKResponseType.MKResponseEVM:
								if (
									signMessageResultRParam is StringParameter signMessageResultRParamSpecific &&
									signMessageResultSParam is StringParameter signMessageResultSParamSpecific &&
									signMessageResultVParam is StringParameter signMessageResultVParamSpecific
								)
									OverlayMessage.Simulator_SignMessageResult(signMessageResultStatusParamSpecific.GetValue(),
										new MMKSignMessageResultResponse(new MMKSignMessageResultResponseEVMInterop(
											signMessageResultSignatureParamSpecific.GetValue(),
											signMessageResultRParamSpecific.GetValue(),
											signMessageResultSParamSpecific.GetValue(), signMessageResultVParamSpecific.GetValue()))
									);
								break;
							case MKResponseType.MKResponseSolana:
								OverlayMessage.Simulator_SignMessageResult(signMessageResultStatusParamSpecific.GetValue(),
									new MMKSignMessageResultResponse(new MMKSignMessageResultResponseSolanaInterop(
										signMessageResultSignatureParamSpecific.GetValue()))
								);
								break;
							case MKResponseType.MKResponseEOS:
								OverlayMessage.Simulator_SignMessageResult(signMessageResultStatusParamSpecific.GetValue(),
									new MMKSignMessageResultResponse(new MMKSignMessageResultResponseEOSInterop(
										signMessageResultSignatureParamSpecific.GetValue()))
								);
								break;
						}
					}
					else
					{
						throw new Exception("Invalid parameters");
					}

					break;
				case MessageType.MTMKSignTransactionResult:
					var signTransactionResultStatusParam = _currentParameters.Find(item => item.GetLabel() == "Status");
					var signTransactionResultResponseTypeParam =
						_currentParameters.Find(item => item.GetLabel() == "Response Type");
					var signTransactionResultSignatureParam = _currentParameters.Find(item => item.GetLabel() == "Signature");
					if (
						signTransactionResultSignatureParam is StringParameter signTransactionResultSignatureParamSpecific &&
						signTransactionResultStatusParam is StringParameter signTransactionResultStatusParamSpecific &&
						signTransactionResultResponseTypeParam is DropdownParameter signTransactionResultResponseTypeParamSpecific
					)
					{
						var signTransactionResultSignedRawTransactionParam =
							_currentParameters.Find(item => item.GetLabel() == "SignedRawTransaction");
						var signTransactionResultTransactionHashParam =
							_currentParameters.Find(item => item.GetLabel() == "TransactionHash");
						var signTransactionResultRParam = _currentParameters.Find(item => item.GetLabel() == "R");
						var signTransactionResultSParam = _currentParameters.Find(item => item.GetLabel() == "S");
						var signTransactionResultVParam = _currentParameters.Find(item => item.GetLabel() == "V");
						switch (GetResponseTypeFromOption(signTransactionResultResponseTypeParamSpecific.GetValue()))
						{
							case MKResponseType.MKResponseEVM:
								if (
									signTransactionResultSignedRawTransactionParam is StringParameter
										signTransactionResultSignedRawTransactionParamSpecific &&
									signTransactionResultTransactionHashParam is StringParameter
										signTransactionResultTransactionHashParamSpecific &&
									signTransactionResultRParam is StringParameter signTransactionResultRParamSpecific &&
									signTransactionResultSParam is StringParameter signTransactionResultSParamSpecific &&
									signTransactionResultVParam is StringParameter signTransactionResultVParamSpecific
								)
									OverlayMessage.Simulator_SignTransactionResult(signTransactionResultStatusParamSpecific.GetValue(),
										new MMKSignTransactionResultResponse(new MMKSignTransactionResultResponseEVMInterop(
											signTransactionResultSignedRawTransactionParamSpecific.GetValue(),
											signTransactionResultTransactionHashParamSpecific.GetValue(),
											signTransactionResultSignatureParamSpecific.GetValue(),
											signTransactionResultRParamSpecific.GetValue(),
											signTransactionResultSParamSpecific.GetValue(), signTransactionResultVParamSpecific.GetValue()))
									);
								break;
							case MKResponseType.MKResponseSolana:
								OverlayMessage.Simulator_SignTransactionResult(signTransactionResultStatusParamSpecific.GetValue(),
									new MMKSignTransactionResultResponse(new MMKSignTransactionResultResponseSolanaInterop(
										signTransactionResultSignatureParamSpecific.GetValue()))
								);
								break;
							case MKResponseType.MKResponseEOS:
								OverlayMessage.Simulator_SignTransactionResult(signTransactionResultStatusParamSpecific.GetValue(),
									new MMKSignTransactionResultResponse(new MMKSignTransactionResultResponseEOSInterop(
										signTransactionResultSignatureParamSpecific.GetValue()))
								);
								break;
						}
					}
					else
					{
						throw new Exception("Invalid parameters");
					}

					break;
			}

			AddLog(new Message("Outgoing", GetOptionFromMessageType(_selectedMessageType), payload));
		}

		private void OnPlayModeStateChanged(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingPlayMode) OverlayMessage.Simulator_StopSimulating();
		}

		private MessageType GetMessageTypeFromOption(string option)
		{
			return (MessageType)Enum.Parse(typeof(MessageType), option);
		}

		private MKResponseType GetResponseTypeFromOption(string option)
		{
			return (MKResponseType)Enum.Parse(typeof(MKResponseType), option);
		}

		private int GetOptionKeyFromMessageType(MessageType elixirMessageType)
		{
			return Array.FindIndex(_options, value => GetOptionFromMessageType(elixirMessageType) == value);
		}

		private int GetOptionKeyFromResponseType(MKResponseType responseType)
		{
			return Array.FindIndex(_responseTypeOptions, value => GetOptionFromResponseType(responseType) == value);
		}

		private string GetOptionFromMessageType(MessageType elixirMessageType)
		{
			return Enum.GetName(typeof(MessageType), elixirMessageType);
		}

		private string GetOptionFromResponseType(MKResponseType responseType)
		{
			return Enum.GetName(typeof(MKResponseType), responseType);
		}

		private void ProcessMessage(IMessage message)
		{
			switch (message)
			{
				case MCheckout checkout:
					AddLog(new Message("Incoming", GetOptionFromMessageType(MessageType.MTCheckout),
						$"Sku: {checkout.Sku}"));
					break;
				case MMKGetWallet getWallet:
					AddLog(new Message("Incoming", GetOptionFromMessageType(MessageType.MTMKGetWallet),
						""));
					break;
				case MMKSignTypedData signTypedData:
					AddLog(new Message("Incoming", GetOptionFromMessageType(MessageType.MTMKSignTypedData),
						$"Message: {signTypedData.Message}, Reason: {signTypedData.Reason}"));
					break;
				case MMKSignMessage signMessage:
					AddLog(new Message("Incoming", GetOptionFromMessageType(MessageType.MTMKSignMessage),
						$"Message: {signMessage.Message}, Reason: {signMessage.Reason}"));
					break;
				case MMKSignTransaction signTransaction:
					AddLog(new Message("Incoming", GetOptionFromMessageType(MessageType.MTMKSignTransaction),
						$"Message: {signTransaction.Message}, Reason: {signTransaction.Reason}"));
					break;
			}
		}

		[MenuItem("Elixir/Overlay Event Simulator")]
		public static void ShowWindow()
		{
			GetWindow<OverlayEditorWindow>("Overlay Event Simulator");
		}

		private void UpdateParameterControls()
		{
			_currentParameters.Clear();

			switch (_selectedMessageType)
			{
				case MessageType.MTCheckoutResult:
					_currentParameters.Add(new BooleanParameter("Success"));
					_currentParameters.Add(new StringParameter("Sku"));
					break;
				case MessageType.MTOpenStateChange:
					_currentParameters.Add(new BooleanParameter("IsOpen"));
					break;
				case MessageType.MTMKGetWalletResult:
					_currentParameters.Add(new StringParameter("Status"));
					_currentParameters.Add(new StringParameter("EthAddress"));
					_currentParameters.Add(new StringParameter("SolAddress"));
					_currentParameters.Add(new StringParameter("EosAddress"));
					break;
				case MessageType.MTMKSignTypedDataResult:
					_currentParameters.Add(new StringParameter("Status"));
					_currentParameters.Add(new StringParameter("Signature"));
					_currentParameters.Add(new StringParameter("R"));
					_currentParameters.Add(new StringParameter("S"));
					_currentParameters.Add(new StringParameter("V"));
					break;
				case MessageType.MTMKSignMessageResult:
					var index = GetOptionKeyFromResponseType(_selectedResponseType);
					_currentParameters.Add(new StringParameter("Status"));
					_currentParameters.Add(new DropdownParameter("Response Type",
						_responseTypeOptions,
						GetOptionKeyFromResponseType(_selectedResponseType)));

					switch (_selectedResponseType)
					{
						case MKResponseType.MKResponseEVM:
							_currentParameters.Add(new StringParameter("Signature"));
							_currentParameters.Add(new StringParameter("R"));
							_currentParameters.Add(new StringParameter("S"));
							_currentParameters.Add(new StringParameter("V"));
							break;
						case MKResponseType.MKResponseSolana:
						case MKResponseType.MKResponseEOS:
							_currentParameters.Add(new StringParameter("Signature"));
							break;
						case MKResponseType.MKResponseNone:
						default:
							break;
					}

					break;
				case MessageType.MTMKSignTransactionResult:
					_currentParameters.Add(new StringParameter("Status"));
					_currentParameters.Add(new DropdownParameter("Response Type",
						_responseTypeOptions,
						GetOptionKeyFromResponseType(_selectedResponseType)));

					switch (_selectedResponseType)
					{
						case MKResponseType.MKResponseEVM:
							_currentParameters.Add(new StringParameter("SignedRawTransaction"));
							_currentParameters.Add(new StringParameter("TransactionHash"));
							_currentParameters.Add(new StringParameter("Signature"));
							_currentParameters.Add(new StringParameter("R"));
							_currentParameters.Add(new StringParameter("S"));
							_currentParameters.Add(new StringParameter("V"));
							break;
						case MKResponseType.MKResponseSolana:
						case MKResponseType.MKResponseEOS:
							_currentParameters.Add(new StringParameter("Signature"));
							break;
						case MKResponseType.MKResponseNone:
						default:
							break;
					}

					break;
			}
		}

		private void AddLog(Message message)
		{
			_messages.Add(message);
			_messageTreeView.Reload();
		}

		private class Message
		{
			public readonly string Direction;
			public readonly string EventName;
			public readonly string Payload;

			public Message(string direction, string eventName, string payload)
			{
				Direction = direction;
				EventName = eventName;
				Payload = payload;
			}
		}

		private interface IEventParameter
		{
			void DrawUI();
			string GetLogString();
			string GetLabel();
		}

		private class BooleanParameter : IEventParameter
		{
			private readonly string _label;
			private bool _value;

			public BooleanParameter(string label)
			{
				_label = label;
			}

			public void DrawUI()
			{
				_value = EditorGUILayout.Toggle(_label, _value);
			}

			public string GetLogString()
			{
				return $"{_label}: {_value}";
			}

			public string GetLabel()
			{
				return _label;
			}

			public bool GetValue()
			{
				return _value;
			}
		}

		private class StringParameter : IEventParameter
		{
			private readonly string _label;
			private string _value = "";

			public StringParameter(string label)
			{
				_label = label;
			}

			public void DrawUI()
			{
				_value = EditorGUILayout.TextField(_label, _value);
			}

			public string GetLogString()
			{
				return $"{_label}: {_value}";
			}

			public string GetLabel()
			{
				return _label;
			}

			public string GetValue()
			{
				return _value;
			}
		}

		private class DropdownParameter : IEventParameter
		{
			private readonly string _label;
			private readonly string[] _options;
			private int _selectedIndex;

			public DropdownParameter(string label, string[] options, int? selectedIndex = 0)
			{
				_label = label;
				_options = options;
				_selectedIndex = selectedIndex.Value;
			}

			public void DrawUI()
			{
				_selectedIndex = EditorGUILayout.Popup(_label, _selectedIndex, _options);
			}

			public string GetLogString()
			{
				return $"{_label}: {_options[_selectedIndex]}";
			}

			public string GetLabel()
			{
				return _label;
			}

			public string GetValue()
			{
				return _options[_selectedIndex];
			}
		}

		private class MessageTreeView : TreeView
		{
			private readonly List<Message> _messages;

			public MessageTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader, List<Message> messages)
				: base(state, multiColumnHeader)
			{
				_messages = messages;
				Reload();
			}

			protected override TreeViewItem BuildRoot()
			{
				var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

				if (_messages != null)
				{
					var id = 1;
					foreach (var message in _messages)
					{
						root.AddChild(new TreeViewItem(id, 0, message.EventName));
						id++;
					}
				}

				if (!root.hasChildren)
					// Add a dummy item if there are no messages
					root.AddChild(new TreeViewItem { id = 1, depth = 0, displayName = "No messages" });

				SetupDepthsFromParentsAndChildren(root);
				return root;
			}

			protected override void RowGUI(RowGUIArgs args)
			{
				if (_messages != null && _messages.Count > 0)
				{
					var message = _messages[args.row];
					for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
						CellGUI(args.GetCellRect(i), message, args.GetColumn(i), ref args);
				}
				else
				{
					for (var i = 0; i < args.GetNumVisibleColumns(); ++i)
						CellGUI(args.GetCellRect(i), new Message("", "", ""), args.GetColumn(i), ref args);
				}
			}

			private void CellGUI(Rect cellRect, Message message, int column, ref RowGUIArgs args)
			{
				CenterRectUsingSingleLineHeight(ref cellRect);

				switch (column)
				{
					case 0:
						GUI.Label(cellRect, message.Direction);
						break;
					case 1:
						GUI.Label(cellRect, message.EventName);
						break;
					case 2:
						GUI.Label(cellRect, message.Payload);
						break;
				}
			}

			public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState()
			{
				var columns = new[]
				{
					new MultiColumnHeaderState.Column
					{
						headerContent = new GUIContent("Direction"),
						headerTextAlignment = TextAlignment.Left,
						canSort = false,
						width = 100,
						minWidth = 60,
						autoResize = true,
						allowToggleVisibility = true
					},
					new MultiColumnHeaderState.Column
					{
						headerContent = new GUIContent("Event Name"),
						headerTextAlignment = TextAlignment.Left,
						canSort = false,
						width = 150,
						minWidth = 100,
						autoResize = true,
						allowToggleVisibility = true
					},
					new MultiColumnHeaderState.Column
					{
						headerContent = new GUIContent("Payload"),
						headerTextAlignment = TextAlignment.Left,
						canSort = false,
						width = 150,
						minWidth = 100,
						autoResize = true,
						allowToggleVisibility = true
					}
				};

				var state = new MultiColumnHeaderState(columns);
				return state;
			}
		}
	}
}