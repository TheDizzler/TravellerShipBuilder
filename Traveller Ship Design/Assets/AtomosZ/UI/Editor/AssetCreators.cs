using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorZ
{
	public class AssetCreators
	{
		[MenuItem("GameObject/AtomosZUI/MagicCanvas", isValidateFunction: false, priority: 0)]
		public static MonoBehaviour CreateMagicCanvas(MenuCommand menuCommand)
		{
			MonoBehaviour prefab = AssetDatabase.LoadAssetAtPath<UIPrefabProvider>($"Assets/AtomosZ/UI/BaseUIPrefabs/MagicCanvas.prefab");
			var magicUIObj = Object.Instantiate(prefab);
			magicUIObj.name = magicUIObj.name.Replace("(Clone)", "");
			GameObjectUtility.SetParentAndAlign(magicUIObj.gameObject, menuCommand.context as GameObject);
			Undo.RegisterCreatedObjectUndo(magicUIObj.gameObject, "Create " + magicUIObj.name);
			Selection.activeObject = magicUIObj.gameObject;
			return magicUIObj;
		}

		[MenuItem("GameObject/AtomosZUI/MagicWindow", isValidateFunction: false, priority: -10)]
		public static void CreateMagicWindow(MenuCommand menuCommand)
		{
			CreatePrefab(AssetDatabase.LoadAssetAtPath<MagicWindow>($"Assets/AtomosZ/UI/BaseUIPrefabs/MagicWindow.prefab"), menuCommand);
		}

		[MenuItem("GameObject/AtomosZUI/MagicContextMenu", isValidateFunction: false, priority: -11)]
		public static void CreateMagicContextMenu(MenuCommand menuCommand)
		{
			CreatePrefab(AssetDatabase.LoadAssetAtPath<MagicContextMenu>($"Assets/AtomosZ/UI/BaseUIPrefabs/MagicContextMenu.prefab"), menuCommand);
		}

		private static void CreatePrefab(MonoBehaviour prefab, MenuCommand menuCommand)
		{
			var magicUIObj = Object.Instantiate(prefab);
			magicUIObj.name = magicUIObj.name.Replace("(Clone)", "");
			MonoBehaviour canvas = null;
			// Ensure it gets reparented if this was a context click (otherwise does nothing)
			if (menuCommand.context != null)
			{
				var prefabProvider = ((GameObject)menuCommand.context).GetComponent<UIPrefabProvider>();
				if (prefabProvider != null)
					canvas = prefabProvider;
			}
			else
			{
				canvas = Helpers.GetSingleton<UIPrefabProvider>();
			}

			if (canvas == null)
				canvas = CreateMagicCanvas(menuCommand);

			GameObjectUtility.SetParentAndAlign(magicUIObj.gameObject, canvas.gameObject);

			// Register the creation in the undo system
			Undo.RegisterCreatedObjectUndo(magicUIObj.gameObject, "Create " + magicUIObj.name);
			Selection.activeObject = magicUIObj.gameObject;
		}
	}
}