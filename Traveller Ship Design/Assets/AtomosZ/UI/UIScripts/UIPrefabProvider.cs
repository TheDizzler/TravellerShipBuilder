using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using static AtomosZ.ObjectForge;
using static AtomosZ.UI.MagicWindowBase;

namespace AtomosZ.UI
{
	public class UIPrefabProvider : MonoBehaviour
	{
		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public static UIPrefabProvider instance
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<UIPrefabProvider>();
				return _instance;
			}
		}

		public static UIDataRow temp { get; private set; }

		private static UIPrefabProvider _instance;

		public enum UIPrefabType
		{
			MagicWindow,
			Button,
			MenuButton,
			MenuDivider,
			ExpandingLabel,
			InputField,
			ButtonPanel,
			CheckBox,
			Slider,
			ModalClickBlocker,
			ImageView,
			//ImageViewPanel,
			Dropdown,
			TabControl,
			Spinner,
			HorizontalPanel,
			[Tooltip("AKA a vertical panel")]
			Panel,
			DataRow,
			DataCell,
			Table,
			TabItem,
			MagicTabbedWindow,
			MagicContextMenu,
			SliderUnit,
			/// <summary>
			/// this is not a base UI prefab.
			/// </summary>
			//GeomorphDisplayPanel,
		}

		public ObjectForge objectForge;

		public Dictionary<UIPrefabType, IObjectPool> poolDict;

		[Obsolete]
		private Dictionary<UIControlType, UIPrefabType> typeLinkage = new()
		{
			[UIControlType.Button] = UIPrefabType.Button,
			[UIControlType.ButtonPanel] = UIPrefabType.ButtonPanel,
			[UIControlType.CheckBox] = UIPrefabType.CheckBox,
			[UIControlType.DataCell] = UIPrefabType.DataCell,
			[UIControlType.DataRow] = UIPrefabType.DataRow,
			[UIControlType.Dropdown] = UIPrefabType.Dropdown,
			[UIControlType.HorizontalPanel] = UIPrefabType.HorizontalPanel,
			[UIControlType.Image] = UIPrefabType.ImageView,
			//[UIControlType.ImagePanel ] = UIPrefabType.imagePanel,
			[UIControlType.InputField] = UIPrefabType.InputField,
			[UIControlType.MenuButton] = UIPrefabType.MenuButton,
			[UIControlType.MenuDivider] = UIPrefabType.MenuDivider,
			[UIControlType.ModalClickBlocker] = UIPrefabType.ModalClickBlocker,
			[UIControlType.Panel] = UIPrefabType.Panel,
			[UIControlType.Slider] = UIPrefabType.Slider,
			[UIControlType.Spinner] = UIPrefabType.Spinner,
			[UIControlType.TabControl] = UIPrefabType.TabControl,
			[UIControlType.Table] = UIPrefabType.Table,
			[UIControlType.Text] = UIPrefabType.ExpandingLabel,
			[UIControlType.Window] = UIPrefabType.MagicWindow,
			//[UIControlType. ] = UIPrefabType.,
		};


		[SerializeField] public UIPanelScriptableObject panelScriptObj;
		[SerializeField] public UIPanelScriptableObject horizontalPanelScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject textScriptObj;
		[SerializeField] public UIDropdownScriptableObject dropdownScriptObj;
		[SerializeField] public UICheckBoxScriptableObject checkBoxScriptObj;
		[SerializeField] public UIExpandingInputFieldScriptableObject inputFieldScriptObj;
		[SerializeField] public UISliderScriptableObject sliderScriptObj;
		[SerializeField] public UIButtonScriptableObject buttonScriptObj;
		[SerializeField] public UIButtonPanelScriptableObject buttonPanelScriptObj;
		[SerializeField] public UIImageViewScriptableObject imageViewScriptObj;
		//[SerializeField] public UIImageViewPanelScriptableObject imageViewPanelScriptObj;

		[SerializeField] public UITabControlScriptableObject tabControlScriptObj;
		[SerializeField] public MagicWindowScriptableObject magicWindowScriptObj;
		[SerializeField] public MagicUIScriptableObject contextMenuWindowScriptObj;


		[SerializeField] private TMP_FontAsset defaultFont;

		void Awake()
		{

		}

		[Obsolete]
		private void RecreatePoolDict()
		{
			//	poolDict = new()
			//	{
			//		//[UIPrefabType.MagicWindow] = magicWindowPool,
			//		[UIPrefabType.Button] = buttonPool,
			//		[UIPrefabType.ButtonPanel] = buttonPanelPool,
			//		[UIPrefabType.CheckBox] = checkBoxPool,
			//		[UIPrefabType.DataCell] = cellPool,
			//		[UIPrefabType.DataRow] = rowPool,
			//		[UIPrefabType.Dropdown] = dropdownPool,
			//		//[UIPrefabType.ExpandingLabel] = labelPool,
			//		//[UIPrefabType.HorizontalPanel] = panelPool,
			//		[UIPrefabType.ImageView] = imageViewPool,
			//		[UIPrefabType.InputField] = inputFieldPool,
			//		[UIPrefabType.MenuButton] = menuButtonPool,
			//		[UIPrefabType.MenuDivider] = menuDividerPool,
			//		[UIPrefabType.ModalClickBlocker] = clickBlockerPool,
			//		//[UIPrefabType.Panel] = panelPool,
			//		[UIPrefabType.Slider] = sliderPool,
			//		[UIPrefabType.Spinner] = spinnerPool,
			//		//[UIPrefabType.TabControl] = tabControlPool,
			//		//[UIPrefabType.TabItem] = tabItemPool,
			//		[UIPrefabType.Table] = tablePool,
			//	};
		}

		[Obsolete]
		internal static IObjectPool GetPoolOfType(UIControlType dataType)
		{
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
			{
				return null;
			}

			if (instance.poolDict == null || instance.poolDict.Count == 0)
				instance.RecreatePoolDict();
#endif
			if (instance.poolDict.TryGetValue(instance.typeLinkage[dataType], out var pool))
				return pool;
			return null;
		}

		[Obsolete]
		public void DestroyPools()
		{
#if UNITY_EDITOR
			RecreatePoolDict();
#endif
			foreach (var pool in poolDict)
			{
				pool.Value.Clear();
			}

			poolDict.Clear();
		}



		public static UIMonoBehaviour GetMagicUIControl(UIPrefabType prefabType, Transform parent)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				if (instance == null)
				{
					Log.Error("There must be a MagicCanvas in an open scene.");
					return null;
				}

				return instance.EditorInstantiate(prefabType, parent);
			}
			//if (instance.objectForge == null || instance.objectForge.pools == null)
			//{
			//	if (!Helpers.IsPrefabStage_EDITOR())
			//	{
			//		Log.Error("Please initiate objectForge on the MagicCanvas UIPrefabProvider");
			//		return null;
			//	}


			//}
#endif

			var pool = instance.objectForge.GetPool(prefabType.ToString());
			if (pool == null)
			{

				return null;
			}

#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				var nonPooledObject = Instantiate(pool.prefab, pool.sleepingPooledObjectsParentTransform);
				var uiMono = nonPooledObject.GetComponent<UIMonoBehaviour>();
				if (uiMono == null)
					Log.Error($"{prefabType} has no UIMonoBehaviour?");
				return uiMono;
			}
#endif

			var pooledObj = pool.GetNext();
			var uiObj = pooledObj.GetComponent<UIMonoBehaviour>();
			if (uiObj == null)
				Log.Error($"{prefabType} has no UIMonoBehaviour?");
			uiObj.transform.SetParent(parent);
			uiObj.gameObject.SetActive(true);
			uiObj.rect.anchoredPosition3D = Vector3.zero;
			return uiObj;
		}

#if UNITY_EDITOR
		private UIMonoBehaviour EditorInstantiate(UIPrefabType prefabType, Transform parent)
		{
			//if (Helpers.IsPrefabStage_EDITOR())
			//{
			//	Log.Warning("May not instantiate in the prefab stage");
			//	return null;
			//}

			PooledObject newObject = null;
			foreach (var prefabData in objectForge.pooledPrefabDatas)
			{
				if (prefabData.pooledObject.prefabID == prefabType.ToString())
				{
					newObject = Instantiate(prefabData.pooledObject, parent);
					newObject.transform.localScale = Vector3.one;
					break;
				}
			}

			if (newObject == null)
			{
				Log.Error($"Could not instantiate {prefabType}");
				return null;
			}

			switch (prefabType)
			{
				case UIPrefabType.Button:
					return newObject.GetComponent<UIButton>();

				case UIPrefabType.ButtonPanel:
					return newObject.GetComponent<UIButtonPanel>();

				case UIPrefabType.CheckBox:
					return newObject.GetComponent<UICheckBox>();

				case UIPrefabType.DataCell:
					return newObject.GetComponent<UIDataCell>();

				case UIPrefabType.DataRow:
					return newObject.GetComponent<UIDataRow>();

				case UIPrefabType.Dropdown:
					return newObject.GetComponent<UIDropdown>();

				case UIPrefabType.ImageView:
					return newObject.GetComponent<UIImageView>();

				//case UIPrefabType.ImagePanel:
				//return prefab.GetComponent<>();

				case UIPrefabType.InputField:
					return newObject.GetComponent<UIInputField>();

				case UIPrefabType.MenuButton:
					return newObject.GetComponent<UIMenuButton>();

				case UIPrefabType.MenuDivider:
					return newObject.GetComponent<UIMenuDivider>();

				case UIPrefabType.ModalClickBlocker:
					return newObject.GetComponent<UIModalClickBlocker>();

				case UIPrefabType.HorizontalPanel:
				case UIPrefabType.Panel:
					return newObject.GetComponent<UIPanel>();

				case UIPrefabType.Slider:
					return newObject.GetComponent<UISlider>();
				case UIPrefabType.SliderUnit:
					return newObject.GetComponent<UIExpandingLabel>();

				case UIPrefabType.Spinner:
					return newObject.GetComponent<UISpinner>();

				case UIPrefabType.TabControl:
					return newObject.GetComponent<UITabControl>();

				case UIPrefabType.Table:
					return newObject.GetComponent<UITable>();

				case UIPrefabType.TabItem:
					return newObject.GetComponent<UITabItem>();

				case UIPrefabType.ExpandingLabel:
					return newObject.GetComponent<UIExpandingLabel>();

				case UIPrefabType.MagicWindow:
					return newObject.GetComponent<MagicWindow>();

				case UIPrefabType.MagicTabbedWindow:
					return newObject.GetComponent<MagicTabbedWindow>();


				default:
					Debug.LogError($"{prefabType} is not yet implemented.");
					return null;
			}
		}
#endif


		/// <summary>
		/// Is this necessary? Nullifying the fontasset has the same effect.
		/// </summary>
		/// <returns></returns>
		public static TMP_FontAsset GetDefaultFont()
		{
			return instance.defaultFont;
		}
	}
}