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
		private readonly string[] _options = { "MTCheckoutResult", "MTOpenStateChange" };
		private MessageTreeView _messageTreeView;
		private MultiColumnHeaderState _multiColumnHeaderState;
		private Vector2 _scrollPosition;
		private MessageType _selectedMessageType;
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

		private int GetOptionKeyFromMessageType(MessageType elixirMessageType)
		{
			return Array.FindIndex(_options, value => GetOptionFromMessageType(elixirMessageType) == value);
		}

		private string GetOptionFromMessageType(MessageType elixirMessageType)
		{
			return Enum.GetName(typeof(MessageType), elixirMessageType);
		}

		private void ProcessMessage(IMessage message)
		{
			switch (message)
			{
				case MCheckout checkout:
					AddLog(new Message("Incoming", GetOptionFromMessageType(MessageType.MTCheckout),
						$"Sku: {checkout.Sku}"));
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