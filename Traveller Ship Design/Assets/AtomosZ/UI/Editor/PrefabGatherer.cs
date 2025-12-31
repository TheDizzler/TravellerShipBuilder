using System;

using UnityEditor;

using UnityEngine;

using static AtomosZ.UI.UIPrefabProvider;
namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(UIPrefabProvider))]
	public class PrefabGatherer : Editor
	{
		private const string DEFAULT_SO_FOLDER_PATH = "Assets/AtomosZ/UI/BaseUIPrefabs/DefaultScriptableObjects/";

		UIPrefabProvider provider;

		void OnEnable()
		{
			provider = (UIPrefabProvider)target;
			provider.uiPrefabs.Clear();
			foreach (UIPrefabType prefabType in Enum.GetValues(typeof(UIPrefabType)))
			{
				string prefabName = prefabType.ToString();
				if (prefabType != UIPrefabType.MagicWindow)
					prefabName = "UI" + prefabName;
				var prefab = AssetDatabase.LoadAssetAtPath<UIDesignObject>($"Assets/AtomosZ/UI/BaseUIPrefabs/{prefabName}.prefab");
				if (prefab == null)
					Debug.LogWarning("No prefab found for " + prefabType);
				else
					provider.uiPrefabs.Add(prefabType, prefab);
			}

			if (provider.checkBoxScriptObj == null)
				provider.checkBoxScriptObj = AssetDatabase.LoadAssetAtPath<UICheckBoxScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UICheckBoxData.asset");
			if (provider.dropdownScriptObj == null)
				provider.dropdownScriptObj = AssetDatabase.LoadAssetAtPath<UIDropdownScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIDropdownData.asset");
			if (provider.inputFieldScriptObj == null)
				provider.inputFieldScriptObj = AssetDatabase.LoadAssetAtPath<UIExpandingInputFieldScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIExpandingInputFieldData.asset");
			if (provider.textScriptObj == null)
				provider.textScriptObj = AssetDatabase.LoadAssetAtPath<UIExpandingLabelScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIExpandingLabelData.asset");
			if (provider.sliderScriptObj == null)
				provider.sliderScriptObj = AssetDatabase.LoadAssetAtPath<UISliderScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UISliderData.asset");
			if (provider.buttonScriptObj == null)
				provider.buttonScriptObj = AssetDatabase.LoadAssetAtPath<UIButtonScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIButtonData.asset");
			if (provider.buttonPanelScriptObj == null)
				provider.buttonPanelScriptObj = AssetDatabase.LoadAssetAtPath<UIButtonPanelScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIButtonPanelData.asset");
			if (provider.imageViewScriptObj == null)
				provider.imageViewScriptObj = AssetDatabase.LoadAssetAtPath<UIImageViewScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIImageViewData.asset");
			//if (provider.imageViewPanelScriptObj == null)
			//	provider.imageViewPanelScriptObj = AssetDatabase.LoadAssetAtPath<UIImageViewPanelScriptableObject>(DEFAULT_SO_FOLDER_PATH + "UIImageViewPanelData.asset");

			if (provider.tabbedWindowScriptObj == null)
				provider.tabbedWindowScriptObj = AssetDatabase.LoadAssetAtPath<UITabControlScriptableObject>(DEFAULT_SO_FOLDER_PATH + "TabControlData_Tabbed.asset");
			if (provider.titleBarWindowScriptObj == null)
				provider.titleBarWindowScriptObj = AssetDatabase.LoadAssetAtPath<UITabControlScriptableObject>(DEFAULT_SO_FOLDER_PATH + "TabControlData_TitleBar.asset");
			if (provider.contextMenuWindowScriptObj == null)
				provider.contextMenuWindowScriptObj = AssetDatabase.LoadAssetAtPath<UITabControlScriptableObject>(DEFAULT_SO_FOLDER_PATH + "TabControlData_ContextMenu.asset");
		}
	}
}