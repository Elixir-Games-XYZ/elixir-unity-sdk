using UnityEditor;
using UnityEngine;

namespace Elixir.Overlay
{
	public class ElixirMainMenu
	{
		[MenuItem("Elixir/Documentation")]
		private static void NewMenuOption()
		{
			Application.OpenURL("https://docs.elixir.app/");
		}

		[MenuItem("Elixir/Documentation", true)]
		private static bool ValidateNewMenuOption()
		{
			return true;
		}
	}
}