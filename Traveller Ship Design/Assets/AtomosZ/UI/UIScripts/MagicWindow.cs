using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static AtomosZ.Keyboard;
using static AtomosZ.UI.MagicWindow;
using static AtomosZ.UI.UIButtonPanel;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class MagicWindow : MonoBehaviour, IUIBehavior
	{
		public enum UIControlType
		{
			Text,
			InputField,
			CheckBox,
			Slider,
			Button,
			ButtonPanel,
			Image,
			ImagePanel,
			Dropdown,
			TabControl,
			Panel,
			HorizontalPanel,
			Spinner,
		}

		public enum DialogResult
		{
			None,
			OK,
			Cancel,
			Yes,
			No,
		}

		public UIControlType dataType { get; }
		public enum WindowStyle
		{
			/// <summary>
			/// No title bar. Modal. Not movable. Disappears when focus lost.
			/// </summary>
			ContextMenu,
			/// <summary>
			/// A tabbed title bar that can be multi or single tabbed. Modal or non-modal. Probably not movable? Optional Close control per tab.
			/// </summary>
			Tabbed,
			/// <summary>
			/// A single title bar. Modal or non-modal. Movable optional. Minimize, Maximize (maybe?), and Close controls optional.
			/// </summary>
			TitleBar,
		}
		public WindowStyle windowStyle;

		[SerializeField]
		public CustomDictionary<WindowStyle, UITabControlScriptableObject> windowStyleDatas;

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

		public bool interactable { get; set; }

		public bool isDirty { get; set; }

		public UIExpandingLabel titlebar { get { return rootTabControl.tabPanels[0].tabLabel; } }

		public UITabControl rootTabControl;

		/// <summary>
		/// If this is a tabbed window, gets the currently selected panel, otherwise the main (root) panel.
		/// </summary>
		public UIPanel panel
		{
			get
			{
#if DEBUG
				if (rootTabControl == null)
				{
					Debug.Log("rootTabControl is null and this is fucked");
					rootTabControl = GetComponentInChildren<UITabControl>();
				}
#endif

				return rootTabControl.SelectedPanel();
			}
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

		public DialogResult result;
		public UnityAction<MagicWindow> OnClose;

		public bool isDragging;

		//[SerializeField] private Button minimizeButton;
		//[SerializeField] private Button maximizeButton;
		//[SerializeField] private Button closeButton;
		//[SerializeField] public bool showMinimizeButton;
		//[SerializeField] public bool showMaximizeButton;
		[SerializeField] public bool _showCloseButton;
		public bool showCloseButton
		{
			get { return _showCloseButton; }
			set { Debug.LogWarning("Close button has not yet been implemented"); }
		}


		[SerializeField] private UIDesignObject modalClickBlocker;
#if UNITY_EDITOR
		[SerializeField] public UIControlType currentType;

		[Conditional("UNITY_EDITOR")]
		public void RecordPrefabInstances()
		{
			rootTabControl.RecordPrefabInstances();
		}

		[Conditional("UNITY_EDITOR")]
		public void CreateRootTabControl()
		{
			rootTabControl = Instantiate(UIPrefabProvider.GetUIPrefab(
				UIPrefabProvider.UIPrefabType.TabControl), this.transform).GetComponent<UITabControl>();
			rootTabControl.referenceName = "rootTabControl";
			rootTabControl.tabPanels[0].panel.tabLabel.referenceName = "panel_00";
			rootTabControl.tabPanels[0].tabLabel.referenceName = "tab_00";
			rootTabControl.UpdateBackingData(GetStyleData(windowStyle));
		}
#endif

		public void ClearControls()
		{
			rootTabControl.ClearControls();
			this.SetDirty();
		}


		void Start()
		{
			UIPrefabProvider uiProvider = GetComponentInParent<UIPrefabProvider>();
			if (panelScriptObj == null)
				panelScriptObj = uiProvider.panelScriptObj;
			if (horizontalPanelScriptObj == null)
				horizontalPanelScriptObj = uiProvider.horizontalPanelScriptObj;
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
			//if (imageViewPanelScriptObj == null)
			//	imageViewPanelScriptObj = uiProvider.imageViewPanelScriptObj;

			FindStyleData();
		}

		private void FindStyleData()
		{
			UIPrefabProvider uiProvider = UIPrefabProvider.instance;

			if (uiProvider == null)
				return;

			if (!windowStyleDatas.TryGetValue(WindowStyle.Tabbed, out var tabbedSO))
			{
				tabbedSO = uiProvider.tabbedWindowScriptObj;
				windowStyleDatas.Add(WindowStyle.Tabbed, tabbedSO);
			}
			else if (tabbedSO == null)
			{
				tabbedSO = uiProvider.tabbedWindowScriptObj;
				windowStyleDatas[WindowStyle.Tabbed] = tabbedSO;
			}

			if (!windowStyleDatas.TryGetValue(WindowStyle.TitleBar, out var titleBarSO))
			{
				titleBarSO = uiProvider.titleBarWindowScriptObj;
				windowStyleDatas.Add(WindowStyle.TitleBar, titleBarSO);
			}
			else if (titleBarSO == null)
			{
				titleBarSO = uiProvider.titleBarWindowScriptObj;
				windowStyleDatas[WindowStyle.TitleBar] = titleBarSO;
			}

			if (!windowStyleDatas.TryGetValue(WindowStyle.ContextMenu, out var contextMenuSO))
			{
				contextMenuSO = uiProvider.contextMenuWindowScriptObj;
				windowStyleDatas.Add(WindowStyle.ContextMenu, contextMenuSO);
			}
			else if (contextMenuSO == null)
			{
				contextMenuSO = uiProvider.contextMenuWindowScriptObj;
				windowStyleDatas[WindowStyle.ContextMenu] = contextMenuSO;
			}
		}

		private UITabControlScriptableObject GetStyleData(WindowStyle windowStyle)
		{
			if (!windowStyleDatas.TryGetValue(windowStyle, out var styleSO)
				|| styleSO == null)
				FindStyleData();
			return windowStyleDatas[windowStyle];
		}

		public IUIBehavior GetControl(string referenceName)
		{
			return rootTabControl.GetControl(referenceName);
		}

		public bool EnableTab(string tabName, bool enable)
		{
			return rootTabControl.EnableTab(tabName, enable);
		}

		public List<UIDesignObject> GetControls()
		{
			return panel.GetControls();
		}

#if DEBUG
		public List<UIDesignObject> GetControlsFromTransform_DEBUG()
		{
			if (panel != null)
				return panel.GetControlsFromTransform();
			Debug.LogException(new Exception("Why is this null?"));
			return null;
		}
#endif

		public void ChangeWindowStyle(WindowStyle windowStyle)
		{
			var styleData = GetStyleData(windowStyle);
			if (styleData == null)
				return;
			var newStyleData = styleData;
			rootTabControl.UpdateBackingData(newStyleData);
#if UNITY_EDITOR
			rootTabControl.UpdateBackingData(newStyleData); // tabs do not properly update unless this is called again
			RecordPrefabInstances(); // probably necessary?
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="modiferKeys"></param>
		/// <returns>True if input consumed.</returns>
		/// <exception cref="Exception"></exception>
		public bool Input(ModifierKey modiferKeys)
		{
			if ((modiferKeys & ModifierKey.Esc) == ModifierKey.Esc
				&& (_designObject.isModal || windowStyle == WindowStyle.ContextMenu))
			{
				SetDialogResultDefaultNegative();
				return true;
			}

			return false;
		}

		public void SetDialogResultDefaultNegative()
		{
			DialogButton buttons = panel.GetPanelButtons();

			switch (buttons)
			{
				case (DialogButton)(-1):
					Close();
					return;

				case DialogButton.OK:
					SetDialogResultOK();
					return;

				case DialogButton.OKCancel:
					SetDialogResultCancel();
					return;

				case DialogButton.YesNo:
					SetDialogResultNo();
					return;

				case DialogButton.YesNoCancel:
					SetDialogResultCancel();
					return;

				default:
					throw new Exception("Unimplemented DialogButton option: " + buttons);
			}
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}


		public void Refresh()
		{
			GetMinDimensions();
		}

		public void RecalculateDimensions()
		{
			GetMinDimensions();
		}

		public Vector2 GetMinDimensions()
		{
			isDirty = false;
			return rootTabControl.GetMinDimensions();
		}

		public void SetTitle(string titleText)
		{
			titlebar.text = titleText;
		}

		public void SetTitle(string titleText, float textSize)
		{
			titlebar.text = titleText;
			titlebar.fontSize = textSize;
		}


		/// <summary>
		/// Can add multiple methods to a single UnityAction as below:<br/>
		/// <c>
		/// UnityAction action = null;<br/>
		/// action += () => FunctionWithParam("name");<br/>
		/// action += () => FunctionNoParam();<br/>
		/// action += delegate {// some code here};</c>
		/// </summary>
		/// <param name="clickActions"></param>
		public void SetContextMenuActions(List<DesignAction> clickActions)
		{
			windowStyle = WindowStyle.ContextMenu;
			//tabs[selectedTabIndex].SetContextMenuActions(clickActions);
			RecalculateDimensions();
			isDirty = true;
		}

		public void SetDialogResultOK()
		{
			this.result = DialogResult.OK;
			Close();
		}

		public void SetDialogResultCancel()
		{
			this.result = DialogResult.Cancel;
			Close();
		}

		public void SetDialogResultYes()
		{
			this.result = DialogResult.Yes;
			Close();
		}

		public void SetDialogResultNo()
		{
			this.result = DialogResult.No;
			Close();
		}




		public void Show(Vector2 pos)
		{
			GetComponent<RectTransform>().anchoredPosition = pos;
			gameObject.SetActive(true);
			if (designObject.isModal)
			{
				//modalClickBlocker.
				Debug.LogWarning("modal blocker?");
			}
		}


		public void Hide()
		{
			gameObject.SetActive(false);
		}

		/// <summary>
		/// DesignManager handles destruction of objects.
		/// That way we can create a pool if we so desire.
		/// </summary>
		public void Close()
		{
			if (OnClose != null)
				OnClose(this);
			OnClose = null;
			gameObject.SetActive(false);
		}

		/// <summary>
		/// Minimize to a titlebar only,
		/// then move to bottom of screen?
		/// </summary>
		//public void Minimize()
		//{
		//	isMinimized = !isMinimized;
		//	// is this necessary?
		//	// @TODO(Tristan): performance testing to see if controls are still being "drawn" even if this panel is hidden
		//	tabs[selectedTabIndex].ShowControls(!isMinimized);
		//	Refresh();
		//}



		public UIExpandingLabel AddText()
		{
			return panel.AddText(null);
		}

		public UIExpandingLabel AddText(string text)
		{
			return panel.AddText_(text);
		}

		public UIExpandingInputField AddInputField()
		{
			return panel.AddInputField(null);
		}

		public UIButton AddButton()
		{
			return panel.AddButton(null);
		}

		public UIButtonPanel AddButtonPanel()
		{
			return panel.AddButtonPanel(null);
		}

		/// <summary>
		/// Adds a control to the currently selected panel (root panel, if not multi-tabbed).
		/// </summary>
		/// <param name="ctrlType"></param>
		/// <returns></returns>
		public IUIBehavior AddUIControl(UIControlType ctrlType)
		{
			isDirty = true;
			switch (ctrlType)
			{
				case UIControlType.Text:
					return panel.AddText(textScriptObj);

				case UIControlType.InputField:
					return panel.AddInputField(inputFieldScriptObj);

				case UIControlType.Dropdown:
					return panel.AddDropdown(dropdownScriptObj);

				case UIControlType.CheckBox:
					return panel.AddCheckBox(checkBoxScriptObj);

				case UIControlType.Slider:
					return panel.AddSlider(sliderScriptObj);

				case UIControlType.Button:
					return panel.AddButton(buttonScriptObj);

				case UIControlType.ButtonPanel:
					return panel.AddButtonPanel(buttonPanelScriptObj);

				case UIControlType.Image:
					return panel.AddImage(imageViewScriptObj);

				//case UIControlType.ImagePanel:
				//return panel.AddImagePanel(new ImageViewDataEx(imageViewPanelScriptObj));

				case UIControlType.HorizontalPanel:
					return panel.AddHorizontalPanel(horizontalPanelScriptObj);

				default:
					Debug.LogException(new Exception($"{currentType} not yet implemented"));
					return null;
			}
		}

		public TabPanel AddTab(
			string tabText, UIPanelScriptableObject overridePanelData = null)
		{
			var tabPanel = rootTabControl.AddTab(tabText, overridePanelData);
			return tabPanel;
		}


		public TabPanel SelectTab(int tabIndex)
		{
			return rootTabControl.SelectTab(tabIndex);
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

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}


		public void UpdateHover(Vector3 posOfHover)
		{
			throw new System.NotImplementedException();
		}
	}
}