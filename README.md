# Welcome to Elixir Unity SDK v2

This is the Unity version of [Elixir Games](https://elixir.app/) SDK. With the implmentation of it, your game will be synchronized with the [Elixir Launcher](launcher.elixir.app). 

Having this sdk integrated will ease the "click and play" for all users launching the Game from Elixir. As we facilitate the user profile, wallets...

See [Elixir Docs](https://docs.elixir.app/).

# Integration Example

You'll find a detailed [Integration Example](https://github.com/Elixir-Games-XYZ/elixir-unity-sdk/blob/master/Demo/Scripts/InitSceneController.cs)

# Kick-off Guide

### 1. Download and Install

 - Download the latest version of the SDK available [here](https://github.com/Elixir-Games-XYZ/elixir-unity-sdk/releases).
 - Import the package inside your workspace: *Assets* > *Import Package* > *Custom Package*
 
### 2.Obtain your credentials

These credentails can be obtained in the [Developer Dashboard](https://dashboard.elixir.app/), following the Elixir Docs.

Input your credentials on the Integration Example:

```c#

Elixir.ElixirController.Instance.PrepareElixir("put you public key here")

```

 
### 3. Launch the Integration Example

Now you should be able to run the Initial Scene and check that you obtained yoour Elixir Publisher User.

# Description

This SDK is intended to provide an `AUTH` layer, but also several features that can be included in your project:

## Auth 

### Desktop

The sdk validates that the game is being executed inside the Elixir Launcher Environment. If this step is completed successfully, the SDK will obtain the user session credentials, and from now on, the user will be logged into the game using his Elixir Launcher account.

If this step is not completed, the game wont be able to be executed, avoiding the game execution outside the Elixir Launcher or blocking Banned users.

> **_NOTE:_** In the latest version of the SDK this check can be bypassed by the owner responsability to allow executing outside the Launcher environment 

### Mobile

This SDK also provides an elixir user authentication for the mobile platform by two ways:

 - OTP login: The user will be able to provide his email, then we will email him a verification code. Once this code is redeemed and validated, the user will be logged into the SDK.

 - QR Scan: To avoid the change of context for the user, we also provide a QR verification inside Elixir Launcher. The user will be able to scan it using his phone camera.

In the Mobile SDK OAuth, the user will remain logged with elixir until he wants to log out. This way, we avoid having a login every time the game is executed.

## GetUserData()

This endpoint work as an OpenId. The games can relate to it to validate the JWT generated by the SDK. This endpoint will provide Elixir-related info:

 - ElixirID: Unique and immutable user identification inside the Elixir Platform.
 - Username: Elixir user nickname, that can be displayed inside the game
 - Wallet: User wallet associated with the game blockchain network.

> **_NOTE:_**  If your game already has a User entity, with account credentials, we recommend adding a "Link Account" button, that pairs up the ElixirID with your User Entity, so it's only needed to do it once. This way, each user will be able to use your native game account when launching from Elixir with  a one-time login.

## GetCollections()

After importing all NFT collections that the game involves using the Elixir Dashboard. You will be able to use this endpoint, as NFT gating to obtain all the NFTs that the user owns for these collections.

## GetTournaments()

A complete tournament tool is available with the integration of this SDK. This method will allow the game client to obtain all available tournaments in Elixir for the game. 
The tournaments feature should be coordinated with the game backend and Elixir configuration. See docs.
