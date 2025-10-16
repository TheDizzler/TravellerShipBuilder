using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	public class MagicWindow : MonoBehaviour, IUIBehavior
	{
		public enum WindowStyle
		{
			/// <summary>
			/// No title bar. Modal. Not movable. Disappears when focus lost.
			/// </summary>
			ContextMenu,
			/// <summary>
			/// A tabbed title bar that can be multi or single tabbed. Modal or non-modal. Probably not movable? Optional Close control.
			/// </summary>
			Tabbed,
			/// <summary>
			/// A single title bar. Modal or non-modal. Movable optional. Minimize, Maximize (maybe?), and Close controls optional.
			/// </summary>
			TitleBar,
		}
		public WindowStyle windowStyle;

		[SerializeField]
		public CustomDictionary<WindowStyle, UITabControlScriptableObject> styleDatas;

		[SerializeField] private string _referenceName;
		public string referenceName { get { return _referenceName; } set { _referenceName = value; } }

		public UIDesignObject _designObject;

		public UIDesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<UIDesignObject>();
				return _designObject;
			}
		}

		public UITabControl rootTabControl;
		public UIPanel panel
		{
			get
			{
				if (rootTabControl == null)
				{
					Debug.Log("rootTabControl is null and this is fucked");
					rootTabControl = GetComponentInChildren<UITabControl>();
				}

				return rootTabControl.SelectedPanel();
			}
		}


		[SerializeField] public UIExpandingLabelScriptableObject textScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject dropdownScriptObj;
		[SerializeField] public UICheckBoxScriptableObject checkBoxScriptObj;
		[SerializeField] public UIExpandingInputFieldScriptableObject inputFieldScriptObj;
		[SerializeField] public UISliderScriptableObject sliderScriptObj;
		[SerializeField] public UIButtonScriptableObject buttonScriptObj;
		[SerializeField] public UIButtonPanelScriptableObject buttonPanelScriptObj;
		[SerializeField] public UIImageViewScriptableObject imageViewScriptObj;
		[SerializeField] public UIImageViewPanelScriptableObject imageViewPanelScriptObj;

#if UNITY_EDITOR
		[SerializeField] public UIControlType currentType;
		public List<UIControl> controlList = new();

		[Conditional("UNITY_EDITOR")]
		public void ClearControlsEditor()
		{
			rootTabControl.ClearControlsEditor();
			GetMinDimensions();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void RecordPrefabInstances()
		{
			rootTabControl.RecordPrefabInstances();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void CreateRootTabControl()
		{
			rootTabControl = Instantiate(UIPrefabProvider.GetUIPrefab(
				UIPrefabProvider.UIPrefabType.TabControl), this.transform).GetComponent<UITabControl>();
			rootTabControl.referenceName = "rootTabControl";
			rootTabControl.tabPanels[0].Value.tabLabel.referenceName = "panel_00";
			rootTabControl.tabPanels[0].Key.referenceName = "tab_00";
			rootTabControl.tabControlEx = new TabControlEx(GetStyleData(windowStyle));
		}
#endif


		void Start()
		{
			UIPrefabProvider uiProvider = GetComponentInParent<UIPrefabProvider>();
			if (textScriptObj == null)
				textScriptObj = uiProvider.textScriptObj;
			if (dropdownScriptObj == null)
				dropdownScriptObj = uiProvider.dropdownScriptObj;
			if (checkBoxScriptObj == null)
				checkBoxScriptObj = uiProvider.checkBoxScriptObj;
			if (inputFieldScriptObj == null)
				inputFieldScriptObj = uiProvider.inputFieldScriptObj;
			if (sliderScriptObj == null)
				sliderScriptObj = uiProvider.sliderScriptObj;
			if (buttonScriptObj == null)
				buttonScriptObj = uiProvider.buttonScriptObj;
			if (buttonPanelScriptObj == null)
				buttonPanelScriptObj = uiProvider.buttonPanelScriptObj;
			if (imageViewScriptObj == null)
				imageViewScriptObj = uiProvider.imageViewScriptObj;
			if (imageViewPanelScriptObj == null)
				imageViewPanelScriptObj = uiProvider.imageViewPanelScriptObj;

			FindStyleData();

		}

		private void FindStyleData()
		{
			UIPrefabProvider uiProvider = UIPrefabProvider.instance;

			if (uiProvider == null)
				return;

			if (!styleDatas.TryGetValue(WindowStyle.Tabbed, out var tabbedSO))
			{
				tabbedSO = uiProvider.tabbedWindowScriptObj;
				styleDatas.Add(WindowStyle.Tabbed, tabbedSO);
			}
			else if (tabbedSO == null)
			{
				tabbedSO = uiProvider.tabbedWindowScriptObj;
				styleDatas[WindowStyle.Tabbed] = tabbedSO;
			}

			if (!styleDatas.TryGetValue(WindowStyle.TitleBar, out var titleBarSO))
			{
				titleBarSO = uiProvider.titleBarWindowScriptObj;
				styleDatas.Add(WindowStyle.TitleBar, titleBarSO);
			}
			else if (titleBarSO == null)
			{
				titleBarSO = uiProvider.titleBarWindowScriptObj;
				styleDatas[WindowStyle.TitleBar] = titleBarSO;
			}

			if (!styleDatas.TryGetValue(WindowStyle.ContextMenu, out var contextMenuSO))
			{
				contextMenuSO = uiProvider.contextMenuWindowScriptObj;
				styleDatas.Add(WindowStyle.ContextMenu, contextMenuSO);
			}
			else if (contextMenuSO == null)
			{
				contextMenuSO = uiProvider.contextMenuWindowScriptObj;
				styleDatas[WindowStyle.ContextMenu] = contextMenuSO;
			}
		}

		private UITabControlScriptableObject GetStyleData(WindowStyle windowStyle)
		{
			if (!styleDatas.TryGetValue(windowStyle, out var styleSO)
				|| styleSO == null)
				FindStyleData();
			return styleDatas[windowStyle];
		}

		public void RemoveControl(UIControl uiControl)
		{
			var uiData = uiControl.GetData();
			panel.RemoveControl(uiData);
		}

		public IUIBehavior GetControl(string referenceName)
		{
			return panel.GetControl(referenceName);
		}

		public List<UIDesignObject> GetControls()
		{
			return panel.GetControls();
		}

		public List<UIDesignObject> GetControlsFromTransform()
		{
			if (panel != null)
				return panel.GetControlsFromTransform();
			Debug.LogException(new Exception("Why is this null?"));
			return null;
		}


		public void ChangeWindowStyle(WindowStyle windowStyle)
		{
			var styleData = GetStyleData(windowStyle);
			if (styleData == null)
				return;
			var newStyleData = new TabControlEx(styleData);
			rootTabControl.UpdateBackingData(newStyleData);
#if UNITY_EDITOR
			rootTabControl.UpdateBackingData(newStyleData); // tabs do not properly update unless this is called again
			RecordPrefabInstances(); // probably necessary?
#endif
		}



		public Vector2 GetMinDimensions()
		{
			var minDim = rootTabControl.GetMinDimensions();
			return minDim;
		}


		public IUIBehavior AddUIControl()
		{
			switch (currentType)
			{
				case UIControlType.Text:
					return panel.AddUIControl(new LabelEx(textScriptObj));

				case UIControlType.InputField:
					return panel.AddUIControl(new InputFieldEx(inputFieldScriptObj));

				case UIControlType.Dropdown:
					return panel.AddUIControl(new DropdownEx(dropdownScriptObj));

				case UIControlType.CheckBox:
					return panel.AddUIControl(new CheckBoxEx(checkBoxScriptObj));

				case UIControlType.Slider:
					return panel.AddUIControl(new SliderEx(sliderScriptObj));

				case UIControlType.Button:
					return panel.AddUIControl(new ButtonEx(buttonScriptObj));

				case UIControlType.ButtonPanel:
					return panel.AddUIControl(new ButtonPanelEx(buttonPanelScriptObj));

				case UIControlType.Image:
					return panel.AddUIControl(new ImageEx(imageViewScriptObj));

				case UIControlType.ImagePanel:
					return panel.AddUIControl(new ImageViewDataEx(imageViewPanelScriptObj));

				default:
					Debug.LogException(new Exception($"{currentType} not yet implemented"));
					return null;
			}
		}

		public TabPanel AddTab(string tabText, UIPanelScriptableObject overridePanelData = null)
		{
			var tabPanel = rootTabControl.AddTab(tabText, overridePanelData);
			return tabPanel;
		}

		public void ResetToLastPosition()
		{
			throw new System.NotImplementedException();
		}


		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput,
			ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}

		public void Deselect()
		{
			throw new System.NotImplementedException();
		}




		public UIDesignObject Select()
		{
			throw new System.NotImplementedException();
		}

		public void SetHover(bool isHover)
		{
			throw new System.NotImplementedException();
		}

		public IUIDataEx GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			throw new System.NotImplementedException();
		}


		public void UpdateHover(Vector3 posOfHover)
		{
			throw new System.NotImplementedException();
		}
	}
}