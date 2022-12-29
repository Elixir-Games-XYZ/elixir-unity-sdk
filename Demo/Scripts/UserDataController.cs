using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserDataController : MonoBehaviour
{
    static UserDataController _Instance;
    public static UserDataController Instance { get { if (_Instance == null) _Instance = (UserDataController)(Resources.FindObjectsOfTypeAll(typeof(UserDataController))[0]); return _Instance; } }
    void Awake() { if (_Instance == null) gameObject.SetActive(false); }

    public RawImage image;
    public Text nick;
    public Transform content;
    public GameObject item;
    // Start is called before the first frame update
    public void Show() {
        nick.text = Elixir.User.userData.nickname;
        gameObject.SetActive(true);
        StartCoroutine(LoadImage(Elixir.User.userData.avatar, image));

        if (Elixir.NFTs.collections != null) {
            foreach (var collection in Elixir.NFTs.collections) {
                foreach (var nft in collection.nfts) {
                    var tmp = Instantiate<GameObject>(item).GetComponent<NFTItemController>();
                    tmp.transform.parent = content;
                    tmp.Show(collection.collectionName, nft.name, nft.image);
                }
            }
        }
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
    public static IEnumerator LoadImage(string url, RawImage image) {
        if (string.IsNullOrEmpty(url)) yield break;        
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url, true);
        yield return www.SendWebRequest();
        DownloadHandlerTexture dht = www.downloadHandler as DownloadHandlerTexture;
        image.texture = dht.texture;
    }
}
