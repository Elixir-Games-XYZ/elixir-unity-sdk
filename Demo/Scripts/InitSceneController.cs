using System.Collections;
using UnityEngine;

public class InitSceneController : MonoBehaviour {
#if UNITY_ANDROID || UNITY_IOS
    // Mobile Version
    IEnumerator Start() {
        // Initialize the Elixir SDK.
        Elixir.ElixirController.Instance.PrepareElixir();
        // Try to use the old refresh token,
        yield return Elixir.Auth.Refresh();
        if (Elixir.BaseWS.lastError) {
            // Login using email
            yield return Elixir.Auth.InitMail("your@mail.here");
            // If no error on login...
            if (!Elixir.BaseWS.lastError) {
                // Send confirm mail using code typed by the user.
                yield return Elixir.Auth.ConfirmMail("<code>"); // In sandbox mode, code is readed from InitMail response.
            }
        }

        if (!Elixir.BaseWS.lastError) {
            Elixir.ElixirController.Log($"Try to get user data");
            // Get user data.
            yield return Elixir.User.Get();
            if (!Elixir.BaseWS.lastError) {
                Elixir.ElixirController.Log($"Logedin user {Elixir.User.userData.nickname}");
                // Get user NFTs collections
                yield return Elixir.NFTs.Get();
                if (!Elixir.BaseWS.lastError) {
                    Elixir.ElixirController.Log($"Collections {Elixir.NFTs.collections.Length}");
                    foreach (var collection in Elixir.NFTs.collections) {
                        Elixir.ElixirController.Log($"Collection({collection.collection}) Name: {collection.collectionName}");
                        foreach (var nft in collection.nfts) {
                            Elixir.ElixirController.Log($"NFT({nft.tokenId}) Name: {nft.name}");
                        }
                    }
                }
            }
        }

        // You can close session and invalidate tokens.
        // yield return Elixir.Auth.Logout();
    }
#else
    // Standalone Version
    IEnumerator Start() {
        // Initialize the Elixir SDK.
        Elixir.ElixirController.Instance.PrepareElixir();
        // Login using REIKey
        yield return Elixir.Auth.InitREI(Elixir.ElixirController.Instance.rei);
        // If no error on login...
        if (!Elixir.BaseWS.lastError) {
            Elixir.ElixirController.Log($"Try to get user data");
            // Get user data.
            yield return Elixir.User.Get();
            Elixir.ElixirController.Log($"Logedin user {Elixir.User.userData.nickname}");
            // Get user NFTs collections
            yield return Elixir.NFTs.Get();
            if (!Elixir.BaseWS.lastError) {
                Elixir.ElixirController.Log($"Collections {Elixir.NFTs.collections.Length}");
                foreach (var collection in Elixir.NFTs.collections) {
                    Elixir.ElixirController.Log($"Collection({collection.collection}) Name: {collection.collectionName}");
                    foreach (var nft in collection.nfts) {
                        Elixir.ElixirController.Log($"NFT({nft.tokenId}) Name: {nft.name}");
                    }
                }
            }
        } else {
            // ERROR login user.
        }
    }
#endif
}
