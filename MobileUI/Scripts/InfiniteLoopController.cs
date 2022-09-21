using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteLoopController : MonoBehaviour {
    public RectTransform infiniteProgress;
    void Update() {
        infiniteProgress?.Rotate(Vector3.forward, Time.deltaTime * 180f);
    }
}
