using System;
using AtomosZ.EditorZ;
using UnityEditor;
using UnityEngine;
using static AtomosZ.UI.UIPrefabProvider;


namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(UIPrefabProvider))]
	public class PrefabGatherer : EditorEx
	{
		private const string DEFAULT_SO_FOLDER_PATH = "Assets/AtomosZ/UI/BaseUIPrefabs/DefaultScriptableObjects/";

		UIPrefabProvider provider;

		void OnEnable()
		{
			provider = (UIPrefabProvider)target;

			CreateNewPools();


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

		private void CreateNewPools()
		{
			foreach (UIPrefabType type in Enum.GetValues(typeof(UIPrefabType)))
			{
				CreatePool(type, provider.poolDict);
			}
		}

		private void CreatePool(UIPrefabType type, CustomDictionary<UIPrefabType, ObjectForge.ObjectPool<UIMonoBehaviour>> dict)
		{
			if (dict.TryGetValue(type, out var pool))
				return;
			var prefabName = type.ToString();
			if (type != UIPrefabType.MagicWindow)
				prefabName = "UI" + prefabName;
			var prefabFilepath = $"Assets/AtomosZ/UI/BaseUIPrefabs/{prefabName}.prefab";
			var asset = AssetDatabase.LoadAssetAtPath<UIMonoBehaviour>(prefabFilepath);
			if (asset == null)
				Debug.LogException(new Exception($"could not find prefab {prefabFilepath}"));
			pool = new ObjectForge.ObjectPool<UIMonoBehaviour>(asset, 0);

			dict.Add(type, pool);
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			if (Button("Empty Pools"))
			{
				provider.DestroyPools();
				CreateNewPools();
			}

			EndChangeCheck();
		}
	}
}