using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorZ
{
	public class AssetCreators
	{
		[MenuItem("GameObject/AtomosZUI/MagicWindow", isValidateFunction: false, priority: -10)]
		public static void CreatePrefab(MenuCommand menuCommand)
		{
			Debug.LogWarning("Change this to use ObjectPool!");
			var magicWindowPrefab = AssetDatabase.LoadAssetAtPath<MagicWindow>($"Assets/AtomosZ/UI/BaseUIPrefabs/MagicWindow.prefab");
			var magicWindow = Object.Instantiate(magicWindowPrefab);
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			GameObjectUtility.SetParentAndAlign(magicWindow.gameObject, menuCommand.context as GameObject);
			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(magicWindow.gameObject, "Create " + magicWindow.name);
			Selection.activeObject = magicWindow.gameObject;
		}
	}
}