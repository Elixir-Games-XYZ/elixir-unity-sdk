using System.Collections;
using UnityEngine;

public class InitSceneController : MonoBehaviour {
    public GameObject loginButton;
    public GameObject logoutButton;
#if UNITY_ANDROID || UNITY_IOS
    IEnumerator Start() {
        logoutButton.SetActive(false);
        // Initialize the Elixir SDK.
        Elixir.ElixirController.Instance.PrepareElixir();
        // Try to refresh old session.
        yield return Elixir.Auth.Refresh();
        if (Elixir.BaseWS.lastError) {
            // Not valid session, start login process.
            loginButton.SetActive(true);
        } else {
            yield return GetData();
        }
    }

    public void OnMobileLogin() {
        LoginController.Instance.OnLogin(()=> {
            StartCoroutine( GetData()); 
        } );
    }
    public void OnMobileLogout() {
        StartCoroutine( Logout() );
    }

    IEnumerator Logout() {
        yield return Elixir.Auth.Logout();
        logoutButton.SetActive(false);
        loginButton.SetActive(true);
        UserDataController.Instance.Hide();
    }

#else
    // Standalone Version
    IEnumerator Start() {
        Elixir.ElixirController.useconsole = true;
        logoutButton.SetActive(false);
        // Initialize the Elixir SDK.
        Elixir.ElixirController.Instance.PrepareElixir("put you public key here");
        // Hide login button.
        loginButton.SetActive(false);
        // Login using REIKey
        yield return Elixir.Auth.InitREI();
        // If no error on login...
        if (!Elixir.BaseWS.lastError) {
            yield return GetData();
            // Get user data.
        } else {
            // ERROR login user.
        }
    }
#endif

    IEnumerator GetData() {
        // Hide login button.
#if UNITY_ANDROID || UNITY_IOS
        loginButton.SetActive(false);
        logoutButton.SetActive(true);
#endif
        // Ask for user data.
        yield return GetUserData();
        if (!Elixir.BaseWS.lastError) yield return GetCollections();
        if (!Elixir.BaseWS.lastError) yield return GetTournaments();
        if (!Elixir.BaseWS.lastError) UserDataController.Instance.Show();

        
    }
    IEnumerator GetUserData() {
        // Get user data.
        yield return Elixir.User.Get();
        if (!Elixir.BaseWS.lastError) {
            Elixir.ElixirController.Log($"Logedin user {Elixir.User.userData.nickname}");
        } else {
            Elixir.ElixirController.Log($"Error getting user data");
        }
    }
    IEnumerator GetCollections() {
        yield return Elixir.NFTs.Get();
        if (!Elixir.BaseWS.lastError) {
            foreach (var collection in Elixir.NFTs.collections) {
                Elixir.ElixirController.Log($"Collection({collection.collection}) Name: {collection.collectionName}");
                foreach (var nft in collection.nfts) {
                    Elixir.ElixirController.Log($"NFT({nft.tokenId}) Name: {nft.name}");
                }
            }
        } else
            Elixir.ElixirController.Log($"No Collections");

    }
    IEnumerator GetTournaments() {
        yield return Elixir.Tournaments.Get();
        if (!Elixir.BaseWS.lastError) {
            for(int i=0;i< Elixir.Tournaments.tournamentsData.Length;i++)
                Elixir.ElixirController.Log($"Tournament ({Elixir.Tournaments.tournamentsData[i].name}) Name: {Elixir.Tournaments.tournamentsData[i].prizePool}");
        } else
            Elixir.ElixirController.Log($"No Tournaments");

    }

}
