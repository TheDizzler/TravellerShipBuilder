using System;
using AtomosZ.EditorZ;
using Unity.VisualScripting;
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

			//CreateNewPools();


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

			if (provider.tabControlScriptObj == null)
				provider.tabControlScriptObj = AssetDatabase.LoadAssetAtPath<UITabControlScriptableObject>(DEFAULT_SO_FOLDER_PATH + "TabControlData_BladeTab.asset");
			if (provider.magicWindowScriptObj == null)
				provider.magicWindowScriptObj = AssetDatabase.LoadAssetAtPath<MagicWindowScriptableObject>(DEFAULT_SO_FOLDER_PATH + "MagicWindowData_Basic.asset");
			if (provider.contextMenuWindowScriptObj == null)
				provider.contextMenuWindowScriptObj = AssetDatabase.LoadAssetAtPath<MagicUIScriptableObject>(DEFAULT_SO_FOLDER_PATH + "MagicContextMenu.asset");
		}

		private void CreateNewPools()
		{
			//CreatePool("MagicWindow", ref provider.magicWindowPool);

			provider.objectForge.pooledPrefabDatas.Clear();

			string[] allAssetFilePaths = System.IO.Directory.GetFiles($"Assets/AtomosZ/UI/BaseUIPrefabs", "*.prefab");
			foreach (var assetFilepath in allAssetFilePaths)
			{
				PooledObject pooledObject = UnityEditor.AssetDatabase.LoadAssetAtPath<PooledObject>(assetFilepath);
				if (pooledObject == null)
					continue;

				provider.objectForge.pooledPrefabDatas.Add(new ObjectForge.PrefabData
				{
					pooledObject = pooledObject,
					initialPoolSize = 1,
				});
			}
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