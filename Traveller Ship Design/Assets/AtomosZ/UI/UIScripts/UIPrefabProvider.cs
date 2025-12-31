using System;
using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	public class UIPrefabProvider : MonoBehaviour
	{
		public static UIPrefabProvider instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<UIPrefabProvider>();
				return _instance;
			}
		}

		private static UIPrefabProvider _instance;

		public enum UIPrefabType
		{
			MagicWindow,
			Button,
			MenuControlButton,
			MenuDivider,
			ExpandingText,
			InputField,
			ButtonPanel,
			CheckBox,
			Slider,
			ModalClickBlocker,
			ImageView,
			ImageViewPanel,
			Dropdown,
			TabControl,
			Spinner,
			HorizontalPanel,
			[Tooltip("AKA a vertical panel")]
			Panel,
			/// <summary>
			/// this is not a base UI prefab.
			/// </summary>
			//GeomorphDisplayPanel,
		}


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


		[Tooltip("This is populated by an Editor script. Editing manually is futile.")]
		[UDictionary.Split(50, 50)]
		public UDictionary<UIPrefabType, UIDesignObject> uiPrefabs;

		[SerializeField] private TMP_FontAsset defaultFont;
		//[SerializeField] private Canvas uiCanvas;

		

		public static UIDesignObject GetPrefab(UIPrefabType prefabType)
		{
			return instance.uiPrefabs[prefabType];
		}

		public static UIDesignObject GetUIPrefab(UIPrefabType prefabType)
		{
			return instance.uiPrefabs[prefabType];
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