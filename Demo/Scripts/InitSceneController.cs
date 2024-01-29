using System;
using System.Threading.Tasks;
using Elixir;
using UnityEngine;

public class InitSceneController : MonoBehaviour
{
	public GameObject loginButton;
	public GameObject logoutButton;

	private const string ElixirPublicAPIKey =
		"publicKey_c712ecdf47e4eb39b35c6fd868a70b38_9cf87ac0a890b18aceccbba63b64f22f1b707a5109a0d260673ef3470ed02fc56eec6ec039a31a6b6b1d8c58ab8d8b19a325e98f11b13c6bb3339eddb06d16ce638ed3513e1e1298861396dbe79821d6e2b2a421bfa94b5766056d9dbb297eb31cafa1a7b1f62a6e52aca21f29877260fa661c9b49760a822a4f9b7897e15fd57a85ef7c8ec81a1e4fcf8c6b355fe9d4c9f1a6c05f3c42fe7d68a54bb979b6c23f5cc783679c0e1ef070462e5e6c4fccaeb4e861951018ce97210e33af62e9166f2f460599ca1bb1987c1be224f48fe5501b88caec9d48c965df2b1aebd1b6198ff9b64e3d30444143a793bc6d5624669483f1d80072d03f9e1bc96965ded66b9d203610a658c2c88f01b7f60626a65f3855ddcd86950741cfcb15340766a277f2340cc2f9cf9f7f6e9c602a22adeca7e4a1b6a2bfd0d6c8047ecfe397530ce25180e1b6f0fef4271a48fed69a845fc79c6494b0edf04a71d6bfb3d25e93676f583348f052ed973282dd26d0129dd61dd6729629008816cb268fdbc354154e6b195efca028566eb18edbe9d95dbf63f270bf16dc177ff8892dd3f79da1477f2abffe4d5ecdeab4594cecd136de67916c6ae86106bb4f0721f920441558cab8de549ada751a1d47b997b91696e48cb08b404f81e2f7e6aaa2e16a31bb30279fba5a78ddea1a09fc8d490adb48505d47a2cde82469f69aef0398f7d783890f7343cd4cc1c52f68834c1394ce926d9961280653f2d24fd29875c42746cf1686202a7d28deff7120e9645df48430b29e499f0148a8398b627b4c784228c1506b99fe";

#if UNITY_ANDROID || UNITY_IOS
	private void Start()
	{
		InitialSetup();
	}

	private async Task InitialSetup()
	{
    Elixir.ElixirController.UseConsole = true;
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
		await Auth.Logout();
	}

#else
	// Standalone Version
	private void Start()
	{
		InitialSetup();
	}

	private async Task InitialSetup()
	{
		ElixirController.UseConsole = true;
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