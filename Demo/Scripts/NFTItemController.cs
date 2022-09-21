using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NFTItemController : MonoBehaviour
{
    public Text collectionName;
    new public Text name;
    public RawImage image;
    // Start is called before the first frame update
    public void Show(string collectionName, string name, string url) {
        gameObject.SetActive(true);
        this.collectionName.text = collectionName;
        this.name.text = name;
        StartCoroutine(UserDataController.LoadImage(url, image));
    }
}
