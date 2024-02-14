using System;
using System.Threading.Tasks;
using Elixir;
using UnityEngine;

public class InitSceneController : MonoBehaviour
{
	public GameObject loginButton;
	public GameObject logoutButton;

	private const string ElixirPublicAPIKey =
		"replace this with your public api key";

#if UNITY_ANDROID || UNITY_IOS
	private void Start()
	{
		InitialSetup();
	}

	private async Task InitialSetup()
	{
		ElixirController.UseConsole = true;
		ElixirController.Log("Preparing Elixir...");
		try
		{
			logoutButton.SetActive(false);
			// Initialize the Elixir SDK.
			ElixirController.Instance.PrepareElixir(ElixirPublicAPIKey);
			// Try to refresh old session.
			await Auth.Refresh();
			// Not valid session, start login process.
			GetData();
		}
		catch (ElixirApiException apiException)
		{
			if (apiException.Status == 400) ElixirController.Log("Invalid refresh token, need to login");
			else ElixirController.Log($"Elixir API Error: {apiException.Message}");
			loginButton.SetActive(true);
		}
		catch (Exception exception)
		{
			ElixirController.Log($"Error: {exception.Message}");
			loginButton.SetActive(true);
		}
	}

	public void OnMobileLogin()
	{
		LoginController.Instance.OnLogin(() => { GetData(); });
	}

	public void OnMobileLogout()
	{
		Logout();
	}

	private async Task Logout()
	{
		logoutButton.SetActive(false);
		loginButton.SetActive(true);
		UserDataController.Instance.Hide();
		Auth.Logout();
	}

#else
	// Standalone Version
	private void Start()
	{
		InitialSetup();
	}

	private async Task InitialSetup()
	{
		ElixirController.DebugLog = true;
		try
		{
			logoutButton.SetActive(false);
			// Initialize the Elixir SDK.
			ElixirController.Instance.PrepareElixir(ElixirPublicAPIKey);
			// Hide login button.
			loginButton.SetActive(false);
			// Login using REIKey
			await Auth.InitRei();
			await GetData();
		}
		catch (Exception exception)
		{
			ElixirController.Log($"Error on login: {exception.Message}");
		}
	}
#endif

	private async Task GetData()
	{
		// Hide login button.
#if UNITY_ANDROID || UNITY_IOS
		loginButton.SetActive(false);
		logoutButton.SetActive(true);
#endif
		// Ask for user data.
		await GetUserData();
		await GetCollections();
		await GetTournaments();
		UserDataController.Instance.Show();
	}

	private async Task GetUserData()
	{
		try
		{
			var userData = await User.GetUserInfo();
			ElixirController.Log($"Logedin user {userData.nickname} wallets {userData.wallets.Length}");
		}
		catch (Exception exception)
		{
			ElixirController.Log($"Error getting user data: {exception.Message}");
		}
	}

	private async Task GetCollections()
	{
		try
		{
			var collections = await Nfts.GetUserNfts();

			foreach (var collection in collections)
			{
				ElixirController.Log($"Collection({collection.collection}) Name: {collection.collectionName}");
				foreach (var nft in collection.nfts) ElixirController.Log($"NFT({nft.tokenId}) Name: {nft.name}");
			}
		}
		catch (Exception exception)
		{
			ElixirController.Log($"Failed to get collections: {exception.Message}");
		}
	}

	private async Task GetTournaments()
	{
		try
		{
			var tournaments = await Tournaments.GetTournaments();
			for (var i = 0; i < tournaments.Length; i++)
				ElixirController.Log(
					$"Tournament ({tournaments[i].name}) Name: {tournaments[i].prizePool}");
		}
		catch (Exception exception)
		{
			ElixirController.Log($"Failed to get tournaments: {exception.Message}");
		}
	}
}