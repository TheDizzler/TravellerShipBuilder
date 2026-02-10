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
			
			/// <summary>
			/// this is not a base UI prefab.
			/// </summary>
			//GeomorphDisplayPanel,
		}

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

		public CustomDictionary<UIPrefabType, ObjectForge.ObjectPool<UIMonoBehaviour>> poolDict = new();

		internal static ObjectForge.ObjectPool<UIMonoBehaviour> GetPoolOfType(
			MagicWindow.UIControlType dataType)
		{
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
			{
				return null;
			}
#endif
			if (!instance.poolDict.TryGetValue(instance.typeLinkage[dataType], out var pool))
			{
				return null;
			}

			return pool;
		}

		public void DestroyPools()
		{
			foreach (var pool in poolDict)
			{
				pool.Value.Clear();
			}

			poolDict.Clear();
		}


		public static UIMonoBehaviour GetMagicUIControl(UIPrefabType prefabType, Transform parent)
		{
			var obj = instance.poolDict[prefabType].GetNext();
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