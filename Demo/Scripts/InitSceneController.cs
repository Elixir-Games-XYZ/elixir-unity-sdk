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
        Elixir.ElixirController.Instance.PrepareElixir("publicKey_fa588dbca65b887cbfe2e183d939f0c5_a7fc8f5229645e21ce1ddac8a9477fcb09c973aa2811c908a7821d187b3039ea2389df5dfbfae6c372bf16e941f2b7ee5f9897c26f8af6e4bdc4b4f781d9d6fe2fbb558b8e7a7429d2c5ac9fa8e964768d0fdf2ce033062e810a6aef140236da847daba21739f1c1776ad49fc85b6dc5c228335e3c202401b101d680ed0e9e444ba061dd9a4c000350b3b2c0ac2a7478061d1985d673e5088b13782921d41978de2a7c75f1dcad0fdb56b400b1d826c684fa8e74b8db88cd6ebd79999770bd205cdb66b579019a40163766b9b2dba5ad6995101c6673068fbcc12db20a647cbc29abea540e1e15720201187e84cfd5ac1a691030f07a951a2832bee811341216df3f7c742d5298a8fc4cc6de7eca5868eddf400b899419ec24cdd37ad7647b58922497c7ceae188f11c78a531e5e24ae162e30ee0651706e764bdd51f0ea6a7b3a24cbd4a5465948e9542ce13ef7cd12ec9bcc23b9e61b14ea802a0cc3c0a4d76f5e14c62c78a4da0cb722dc6ea11e16ead97df07c600332431b1f09f53c448a0ffd7a656c5851e84b2cfdb737806971ad159b9beaa279a76a95c6b59a48784b9b7c390f39fb49d684e4935646ca8dc5bdc39b64646a9f6451fa508df83d0fc48f82dd2b0be78066267c3267ff473e10f4384546fec705b8758fb38f89b71aeeabb2e9f469240023c42293095aeaf87740056d1ed7cfc1f9fa99011ad4c6ea79d2ceb27dd608395f6c1f80f11b92db63598f9cbb6b0f482ec271f39522add9dd0bf9af1d3c03f70eb35e2b0b7628b4b7a149c4cd3c27a9fa751a780f31b93d2b");
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
