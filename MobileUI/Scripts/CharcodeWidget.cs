using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;


public class CharcodeWidget : Text, IPointerClickHandler {
    public delegate void FocusCallback(CharcodeWidget widget);
    public FocusCallback OnFocus;
    public GameObject selection;

    char _character;
    public char character { get { return _character; } }

    public void OnUpdate(char character) {
        _character = character;
        text = character.ToString();
    }
    public void OnPointerClick(PointerEventData eventData) {
        OnFocus?.Invoke(this);
    }
    public void Select(bool selected) {
#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
        selection?.SetActive(false);
#else
        selection?.SetActive(selected);
#endif
    }
}
