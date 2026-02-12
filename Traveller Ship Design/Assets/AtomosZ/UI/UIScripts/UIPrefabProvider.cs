using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static AtomosZ.ObjectForge;
using static AtomosZ.UI.MagicWindow;

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
			/// <summary>
			/// this is not a base UI prefab.
			/// </summary>
			//GeomorphDisplayPanel,
		}

		public ObjectPool<MagicWindow> magicWindowPool;
		public ObjectPool<UIButton> buttonPool;
		public ObjectPool<UIButtonPanel> buttonPanelPool;
		public ObjectPool<UICheckBox> checkBoxPool;
		public ObjectPool<UIDataCell> cellPool;
		public ObjectPool<UIDataRow> rowPool;
		public ObjectPool<UIDropdown> dropdownPool;
		public ObjectPool<UIExpandingLabel> labelPool;
		public ObjectPool<UIImageView> imageViewPool;
		public ObjectPool<UIInputField> inputFieldPool;
		public ObjectPool<UIMenuButton> menuButtonPool;
		public ObjectPool<UIMenuDivider> menuDividerPool;
		public ObjectPool<UIModalClickBlocker> clickBlockerPool;
		public ObjectPool<UIPanel> panelPool;
		public ObjectPool<UISlider> sliderPool;
		public ObjectPool<UISpinner> spinnerPool;
		public ObjectPool<UITabControl> tabControlPool;
		public ObjectPool<UITable> tablePool;
		public ObjectPool<UITabItem> tabItemPool;

		public CustomDictionary<UIPrefabType, IObjectPool> poolDict;

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

		[SerializeField] public UITabControlScriptableObject tabbedWindowScriptObj;
		[SerializeField] public UITabControlScriptableObject titleBarWindowScriptObj;
		[SerializeField] public UITabControlScriptableObject contextMenuWindowScriptObj;


		[SerializeField] private TMP_FontAsset defaultFont;

		void Awake()
		{
			instance.RecreatePoolDict();
		}

		private void RecreatePoolDict()
		{
			poolDict = new()
			{
				[UIPrefabType.MagicWindow] = magicWindowPool,
				[UIPrefabType.Button] = buttonPool,
				[UIPrefabType.ButtonPanel] = buttonPanelPool,
				[UIPrefabType.CheckBox] = checkBoxPool,
				[UIPrefabType.DataCell] = cellPool,
				[UIPrefabType.DataRow] = rowPool,
				[UIPrefabType.Dropdown] = dropdownPool,
				[UIPrefabType.ExpandingLabel] = labelPool,
				[UIPrefabType.HorizontalPanel] = panelPool,
				[UIPrefabType.ImageView] = imageViewPool,
				[UIPrefabType.InputField] = inputFieldPool,
				[UIPrefabType.MenuButton] = menuButtonPool,
				[UIPrefabType.MenuDivider] = menuDividerPool,
				[UIPrefabType.ModalClickBlocker] = clickBlockerPool,
				[UIPrefabType.Panel] = panelPool,
				[UIPrefabType.Slider] = sliderPool,
				[UIPrefabType.Spinner] = spinnerPool,
				[UIPrefabType.TabControl] = tabControlPool,
				[UIPrefabType.TabItem] = tabItemPool,
				[UIPrefabType.Table] = tablePool,
			};
		}

		internal static IObjectPool GetPoolOfType(UIControlType dataType)
		{
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
			{
				return null;
			}

			instance.RecreatePoolDict();
#endif
			if (instance.poolDict.TryGetValue(instance.typeLinkage[dataType], out var pool))
				return pool;
				return null;

		}

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
			var objPool = GetPool(prefabType);
			UIMonoBehaviour obj = null;
			switch (prefabType)
			{
				case UIPrefabType.Button:
					obj = instance.buttonPool.GetNext();
					break;

				case UIPrefabType.ButtonPanel:
					obj = instance.buttonPanelPool.GetNext();
					break;

				case UIPrefabType.CheckBox:
					obj = instance.checkBoxPool.GetNext();
					break;

				case UIPrefabType.DataCell:
					obj = instance.cellPool.GetNext();
					break;

				case UIPrefabType.DataRow:
					obj = instance.rowPool.GetNext();
					break;

				case UIPrefabType.Dropdown:
					obj = instance.dropdownPool.GetNext();
					break;

				case UIPrefabType.HorizontalPanel:
					obj = instance.panelPool.GetNext();
					break;

				case UIPrefabType.ImageView:
					obj = instance.imageViewPool.GetNext();
					break;

				//case UIPrefabType.ImagePanel:
				//obj = instance.Pool.GetNext();break;

				case UIPrefabType.InputField:
					obj = instance.inputFieldPool.GetNext();
					break;

				case UIPrefabType.MenuButton:
					obj = instance.menuButtonPool.GetNext();
					break;

				case UIPrefabType.MenuDivider:
					obj = instance.menuDividerPool.GetNext();
					break;

				case UIPrefabType.ModalClickBlocker:
					obj = instance.clickBlockerPool.GetNext();
					break;

				case UIPrefabType.Panel:
					obj = instance.panelPool.GetNext();
					break;

				case UIPrefabType.Slider:
					obj = instance.sliderPool.GetNext();
					break;

				case UIPrefabType.Spinner:
					obj = instance.spinnerPool.GetNext();
					break;

				case UIPrefabType.TabControl:
					obj = instance.tabControlPool.GetNext();
					break;

				case UIPrefabType.Table:
					obj = instance.tablePool.GetNext();
					break;

				case UIPrefabType.ExpandingLabel:
					obj = instance.labelPool.GetNext();
					break;

				case UIPrefabType.MagicWindow:
					obj = instance.magicWindowPool.GetNext();
					break;

				default:
					Debug.LogError($"{prefabType} does not a have a pool.");
					return null;
			}

			if (parent != null)
			{
				obj.transform.SetParent(parent, false);
				//obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y, 0);
				obj.transform.localPosition = Vector3.zero;
				obj.transform.localScale = new Vector3(1, 1, 1);
				obj.gameObject.SetActive(true);
			}

			return obj;
		}

		public static IObjectPool GetPool(UIPrefabType prefabType)
		{
			switch (prefabType)
			{
				case UIPrefabType.Button:
					return instance.buttonPool;

				case UIPrefabType.ButtonPanel:
					return instance.buttonPanelPool;

				case UIPrefabType.CheckBox:
					return instance.checkBoxPool;

				case UIPrefabType.DataCell:
					return instance.cellPool;

				case UIPrefabType.DataRow:
					return instance.rowPool;

				case UIPrefabType.Dropdown:
					return instance.dropdownPool;

				case UIPrefabType.HorizontalPanel:
					return instance.panelPool;

				case UIPrefabType.ImageView:
					return instance.imageViewPool;

				//case UIPrefabType.ImagePanel:
				//return instance.Pool;

				case UIPrefabType.InputField:
					return instance.inputFieldPool;

				case UIPrefabType.MenuButton:
					return instance.menuButtonPool;

				case UIPrefabType.MenuDivider:
					return instance.menuDividerPool;

				case UIPrefabType.ModalClickBlocker:
					return instance.clickBlockerPool;

				case UIPrefabType.Panel:
					return instance.panelPool;

				case UIPrefabType.Slider:
					return instance.sliderPool;

				case UIPrefabType.Spinner:
					return instance.spinnerPool;

				case UIPrefabType.TabControl:
					return instance.tabControlPool;

				case UIPrefabType.Table:
					return instance.tablePool;

				case UIPrefabType.ExpandingLabel:
					return instance.labelPool;

				case UIPrefabType.MagicWindow:
					return instance.magicWindowPool;

				//case UIPrefabType.:
				//return instance.Pool;

				default:
					return null;
			}
		}


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