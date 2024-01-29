using System.Collections;
using Elixir;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class UserDataController : MonoBehaviour
{
	private static UserDataController _Instance;

	public RawImage image;
	public Text nick;
	public Transform content;
	public GameObject item;

	public static UserDataController Instance
	{
		get
		{
			if (_Instance == null)
				_Instance = (UserDataController)Resources.FindObjectsOfTypeAll(typeof(UserDataController))[0];
			return _Instance;
		}
	}

	private void Awake()
	{
		if (_Instance == null) gameObject.SetActive(false);
	}

	// Start is called before the first frame update
	public async void Show()
	{
		var userData = await User.GetUserInfo();
		nick.text = userData.nickname;
		gameObject.SetActive(true);
		StartCoroutine(LoadImage(userData.picture, image));
		var collections = await Nfts.GetUserNfts();

		if (collections != null)
			foreach (var collection in collections)
			foreach (var nft in collection.nfts)
			{
				var tmp = Instantiate(item).GetComponent<NFTItemController>();
				tmp.transform.parent = content;
				tmp.Show(collection.collectionName, nft.name, nft.image);
			}
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}
 
	public static IEnumerator LoadImage(string url, RawImage image)
	{
		if (string.IsNullOrEmpty(url)) yield break;
		var www = UnityWebRequestTexture.GetTexture(url, true);
		yield return www.SendWebRequest();
		var dht = www.downloadHandler as DownloadHandlerTexture;
		image.texture = dht.texture;
	}
}