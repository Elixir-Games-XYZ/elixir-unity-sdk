using UnityEditor;

namespace Elixir.Overlay
{
	public class ElixirMainMenu
	{
		[MenuItem("Elixir/Documentation")]
		private static void NewMenuOption()
		{
			OverlayEditorWindow.ShowWindow();
		}

		[MenuItem("Elixir/Documentation", true)]
		private static bool ValidateNewMenuOption()
		{
			return true;
		}
	}
}