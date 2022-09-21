using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CodeWidget : MonoBehaviour
{
    protected TouchScreenKeyboard m_SoftKeyboard;
    public CharcodeWidget[] codes;
    CharcodeWidget selected;
    int currentIdx = -1;
    public bool isFocused = true;

    public string text { 
        get {
            string ret = "";
            for (int i = 0; i < codes.Length; ++i) {
                if (codes[i].character != ' ')
                    ret += codes[i].character;
            }
            return ret; 
        }
        set {
            for (int i = 0; i < codes.Length; ++i)
                codes[i].OnUpdate(i < value.Length ? value[i] : ' ');
        }
    }
    // Start is called before the first frame update
    void Start() {
        for (int i = 0; i < codes.Length; ++i) {
            codes[i].OnFocus += OnFocus;
        }
    }
    private Event m_ProcessingEvent = new Event();
    protected virtual void LateUpdate() {
        if (isFocused) {
            if (m_SoftKeyboard != null) {
                var txt = m_SoftKeyboard.text;
                for (int i = 0; i < codes.Length; ++i) {
                    codes[i].OnUpdate(i < txt.Length?txt[i]:' ');
                }
                var status = m_SoftKeyboard.status;
                if (status == TouchScreenKeyboard.Status.LostFocus ||
                    status == TouchScreenKeyboard.Status.Canceled ||
                    status == TouchScreenKeyboard.Status.Done) {
                    m_SoftKeyboard.active = false;
                    m_SoftKeyboard = null;
                }
            } else {
                if (currentIdx != -1) {
                    while (Event.PopEvent(m_ProcessingEvent)) {
                        var currentEventModifiers = m_ProcessingEvent.modifiers;
                        bool ctrl = SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX ? (currentEventModifiers & EventModifiers.Command) != 0 : (currentEventModifiers & EventModifiers.Control) != 0;
                        KeyCode keyCode = m_ProcessingEvent.keyCode;
                        switch (m_ProcessingEvent.rawType) {
                            case EventType.KeyDown:
                                switch (keyCode) {
                                    case KeyCode.Backspace:
                                    case KeyCode.LeftArrow:
                                        MoveCarret(-1);
                                        break;
                                    case KeyCode.RightArrow:
                                        MoveCarret(1, false);
                                        break;
                                    case KeyCode.V:
                                        if (ctrl) {
                                            string clipboard = GUIUtility.systemCopyBuffer;
                                            foreach (var c in clipboard)
                                                AppendCharacter(c);
                                        }
                                        break;
                                    default:
                                        AppendCharacter(m_ProcessingEvent.character);
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
        }
    }

    void AppendCharacter(char c) {
        if (c >= '0' && c <= '9') {
            selected?.OnUpdate(c);
            MoveCarret();
        }
    }

    void MoveCarret(int idx=1, bool lostFocusAtEnd=true) {
        selected?.Select(false);
        currentIdx+=idx;
        if (currentIdx < 0) currentIdx = 0;
        if (currentIdx >= codes.Length) {
            isFocused = !lostFocusAtEnd;
            currentIdx = codes.Length - 1;
        } else {
            selected = codes[currentIdx];
            selected?.Select(true);
        }

    }
    void OnFocus(CharcodeWidget widget) {
        string code = "";
        for (int i = 0; i < codes.Length; ++i) {
            if(codes[i].character !=' ') code += codes[i].character;
            if (codes[i] == widget) {
                currentIdx = i;
                selected?.Select(false);
                selected = codes[currentIdx];
                selected?.Select(true);
                isFocused = true;
            }
        }
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        m_SoftKeyboard = TouchScreenKeyboard.Open(code, TouchScreenKeyboardType.NumberPad, false, false, true, false, "", 5);
#endif
    }
    void OnValueChanged(string value) {
        Debug.Log($"OnValueChanged {value}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
