using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.Keyboard;
using static AtomosZ.UI.UIButtonPanel;
using static AtomosZ.UI.UICursors;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	public abstract class MagicWindowBase : UIMonoBehaviour
	{
		public abstract UIControlType dataType { get; }
		public enum DialogResult
		{
			None,
			OK,
			Cancel,
			Yes,
			No,
		}
		public DialogResult result;
		public enum UIControlType
		{
			/// <summary>
			/// UIExpandingLabel
			/// </summary>
			Text,
			/// <summary>
			/// UIInputField
			/// </summary>
			InputField,
			/// <summary>
			/// UICheckBox
			/// </summary>
			CheckBox,
			/// <summary>
			/// UISlider
			/// </summary>
			Slider,
			/// <summary>
			/// UIButton
			/// </summary>
			Button,
			/// <summary>
			/// UIButtonPanel
			/// </summary>
			ButtonPanel,
			/// <summary>
			/// UIImageView
			/// </summary>
			Image,
			/// <summary>
			/// UIImagePanel (currently disabled)
			/// </summary>
			ImagePanel,
			/// <summary>
			/// UIDropdown
			/// </summary>
			Dropdown,
			/// <summary>
			/// UITabControl
			/// </summary>
			TabControl,
			/// <summary>
			/// UIPanel
			/// </summary>
			Panel,
			/// <summary>
			/// UIHorizontalPanel
			/// </summary>
			HorizontalPanel,
			/// <summary>
			/// UISpinner
			/// </summary>
			Spinner,
			/// <summary>
			/// UITable
			/// </summary>
			Table,

			/// <summary>
			/// UIMenuDivider
			/// </summary>
			MenuDivider,
			/// <summary>
			/// UIMenuButton
			/// </summary>
			MenuButton,

			/// <summary>
			/// UIDataRow
			/// </summary>
			DataRow,
			/// <summary>
			/// UIDataCell
			/// </summary>
			DataCell,

			/// <summary>
			/// UIModalClickBlocker
			/// </summary>
			ModalClickBlocker,

			/// <summary>
			/// MagicWindow
			/// </summary>
			Window,
			/// <summary>
			/// MagicTabbedWindow
			/// </summary>
			TabbedWindow,
			/// <summary>
			/// MagicContextMenu
			/// </summary>
			ContextMenu,
		}

		[Tooltip("Controls that can be added directly to a panel.")]
		public enum PanelControlType
		{
			Text,
			InputField,
			CheckBox,
			Slider,
			Button,
			ButtonPanel,
			Image,

			Dropdown,
			TabControl,
			Panel,
			HorizontalPanel,
			Spinner,
			Table,

			MenuDivider,
		}

		public abstract UIPanel panel { get; protected set; }
		public bool isDragging;
		public UICursors cursors;

		public bool shrinkToContents;

		public UnityAction<MagicWindowBase> OnClose;

		public bool isModal = false;
		[SerializeField] protected UIMonoBehaviour modalClickBlocker;


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
		[SerializeField] public UITabControlScriptableObject tabControlData;
		//[SerializeField] public UIImageViewPanelScriptableObject imageViewPanelScriptObj;


		public abstract TabPanel SelectTab(int tabIndex);
		public abstract bool Input(ModifierKey modifierKeys);


		public void Show()
		{
			gameObject.SetActive(true);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="uiScreenCoords">Using <c>uiInput.GetMainCameraUICoordinatesFromMousePos();</c></param>
		public void Show(Vector2 uiScreenCoords)
		{
			rect.localPosition = uiScreenCoords;
			gameObject.SetActive(true);
			//if (designObject.isModal)
			//{
			//	//modalClickBlocker.
			//	Debug.LogWarning("modal blocker?");
			//}
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


		internal void SetDragging(bool isDragging)
		{
			this.isDragging = isDragging;
			if (isDragging)
				cursors.SetCursor(UICursors.UICursorMode.Drag);
			else
				cursors.SetCursor(UICursors.UICursorMode.Default);
		}


		public void SetCursor(UICursorMode cursorMode)
		{
			cursors.SetCursor(cursorMode);
		}



		public UITable AddTable()
		{
			return panel.AddTable();
		}

		public UIExpandingLabel AddText()
		{
			return panel.AddText(null);
		}

		public UIExpandingLabel AddText(string text)
		{
			return panel.AddText_(text);
		}

		public UIInputField AddInputField()
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

				case UIControlType.Table:
					return panel.AddTable();

				case UIControlType.TabControl:
					return panel.AddTabControl(tabControlData);

				default:
					Debug.LogException(new Exception($"{ctrlType} not yet implemented"));
					return null;
			}
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


	}

	[ExecuteInEditMode]
	public class MagicWindow : MagicWindowBase, IUIBehavior
	{
		public override UIControlType dataType { get { return UIControlType.Window; } }

		public UITabItem titlebar;
		public override UIPanel panel
		{
			[DebuggerStepThrough]
			[HideInCallstack]
			get;
			[DebuggerStepThrough]
			[HideInCallstack]
			protected set;
		}

		[SerializeField] public bool _showCloseButton;
		public bool showCloseButton
		{
			get { return _showCloseButton; }
			set { Debug.LogWarning("Close button has not yet been implemented"); }
		}


		public bool interactable
		{
			[DebuggerStepThrough]
			[HideInCallstack]
			get { return _interactable; }
			[DebuggerStepThrough]
			[HideInCallstack]
			set { _interactable = value; }
		}



		[SerializeField] private MagicWindowScriptableObject magicWindowData;
		public Sprite titlebarSprite
		{
			get
			{
				if (magicWindowData == null)
				{
					return null;
				}

				return magicWindowData.titleBarSprite;
			}
		}


		[Conditional("UNITY_EDITOR")]
		public new void RecordPrefabInstances()
		{
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
			panel.RecordPrefabInstances();
		}

		[Conditional("UNITY_EDITOR")]
		public void CreateTitlebar()
		{
			foreach (UITabItem child in GetComponentsInChildren<UITabItem>())
			{
				if (child.referenceName == "titlebar")
				{
					titlebar = child;
					break;
				}
			}

			if (titlebar == null)
			{
				titlebar = (UITabItem)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.TabItem, transform);
				titlebar.referenceName = "titlebar";
			}

			titlebar.panel = panel;
			this.SetDirty();
		}


		[Conditional("UNITY_EDITOR")]
		public void CreateMainPanel()
		{
			foreach (UIPanel child in GetComponentsInChildren<UIPanel>())
			{
				if (child.referenceName == "mainPanel")
				{
					panel = child;
					break;
				}
			}

			if (panel == null)
			{
				panel = (UIPanel)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.Panel, transform);
				panel.referenceName = "mainPanel";
				panel.rect.anchorMin = new Vector2(0, 1);
				panel.rect.anchorMax = new Vector2(0, 1);
				panel.rect.pivot = new Vector2(0, 1);
				panel.rect.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				panel.rect.localScale = Vector3.one;
			}

			if (magicWindowData != null)
				panel.UpdateBackingData(magicWindowData.panelScriptableObj);
			panel.tabItem = titlebar;
			this.SetDirty();
		}


		public void ClearControls()
		{
			panel.ClearControls();
		}



		void Start()
		{
			cursors = GetComponentInParent<UICursors>();
			UIPrefabProvider uiProvider = GetComponentInParent<UIPrefabProvider>();
			if (uiProvider == null)
				uiProvider = UIPrefabProvider.instance;

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

			if (tabControlData == null)
				tabControlData = uiProvider.tabControlScriptObj;

			if (magicWindowData == null)
				magicWindowData = uiProvider.magicWindowScriptObj;
			UpdateBackingData(magicWindowData);

#if UNITY_EDITOR
			if (!Application.isPlaying)
				// this resets the window to how it should look. Putting [ExecuteInEditMode] back on to all controls might have the same effect?
				SetDirty_Editor(); 
#endif
		}


		public UIMonoBehaviour GetControl(string searchControlReferenceName)
		{
			if (referenceName == searchControlReferenceName)
				return this;
#if UNITY_EDITOR
			if (titlebar == null)
				CreateTitlebar();
			if (panel == null)
				CreateMainPanel();
#endif
			if (titlebar.referenceName == searchControlReferenceName)
				return titlebar;
			return panel.GetControl(searchControlReferenceName);
		}


		public List<UIMonoBehaviour> GetControls()
		{
			return panel.GetControls();
		}


#if DEBUG
		public List<UIMonoBehaviour> GetControlsFromTransform_DEBUG()
		{
			if (panel != null)
				return panel.GetControlsFromTransform_DEBUG();
			Debug.LogException(new Exception("Why is this null?"));
			return null;
		}
#endif



		public void SetTitle(string titleText)
		{
			titlebar.label.text = titleText;
		}

		public void SetTitle(string titleText, float textSize)
		{
			titlebar.label.text = titleText;
			titlebar.label.fontSize = textSize;
		}


		/// <summary>
		/// Input should get passed to currently open panel.
		/// </summary>
		/// <param name="modifierKeys"></param>
		/// <returns>True if input consumed.</returns>
		/// <exception cref="Exception"></exception>
		public override bool Input(ModifierKey modifierKeys)
		{
			//if (panel.Input(modifierKeys))
			//	return true;
			if ((modifierKeys & ModifierKey.Esc) == ModifierKey.Esc
				&& isModal)
			{
				SetDialogResultDefaultNegative();
				return true;
			}

			return false;
		}

		private void SetDialogResultDefaultNegative()
		{
			DialogButton buttons = panel.GetPanelButtons();

			switch (buttons)
			{
				case (DialogButton)(-1):
					Close();
					return;

				case DialogButton.OK:
					SetDialogResultOK();    // only one response option, so...
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



		public ScriptableObject GetBackingData()
		{
			return magicWindowData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			magicWindowData = (MagicWindowScriptableObject)backingData;
			if (magicWindowData != null)
				RecalculateDimensions();
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}


		public override void RecalculateDimensions()
		{
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
				isDirty = true;
			if (panel == null)
			{
				CreateMainPanel();
			}

			if (titlebar == null)
			{
				CreateTitlebar();
				panel.tabItem = titlebar;
			}
#endif
			//panel.UpdateBackingData(magicWindowData.panelScriptableObj);


			if (titlebarSprite != null)
				titlebar.sprite = titlebarSprite;
			titlebar.image.color = magicWindowData.titleBarColor;

			var titleLabel = titlebar.label;
			if (titleLabel.text.StartsWith("TabItem_"))
			{
				titleLabel.text = "Title";
				titleLabel.referenceName = "titlebar";
			}

			titleLabel.color = magicWindowData.titleBarFontColor;
			titleLabel.alignmentOptions = magicWindowData.titleTextAlignment;
			titleLabel.fontSize = magicWindowData.titleBarFontSize;

			titlebar.RecalculateDimensions();
			var minTitleDimensions = titlebar.GetDrawnSize();

			var minPanelWidth = minTitleDimensions.x + magicWindowData.panelWidthAdjust;
			minPanelWidth = Mathf.Max(minPanelWidth, minDimensions.x);

			float panelVerticalOffset = 0;
			//float orgLayoutPaddingTop = panel.layoutPadding.top;
			//panel.layoutPadding = null; // this resets it to the panel backing data or 0
			if (magicWindowData.offsetPanelByTitleHeight)
			{
				panelVerticalOffset -= titlebar.rect.sizeDelta.y;
			}
			else
			{
				var backingData = ((UIPanelScriptableObject)panel.GetBackingData());
				if (backingData != null)
					panel.layoutPadding.top = backingData.layoutPadding.top;
				else
					panel.layoutPadding.top = 0;
				panel.layoutPadding.top += Mathf.CeilToInt(titlebar.rect.sizeDelta.y);
			}

			Vector2 panelDimens;
			if (shrinkToContents)
			{
				panel.RecalculateAllChildren();
				panelDimens = panel.GetPreferredSize();

				if (panelDimens.x < minPanelWidth)
				{
					panel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minPanelWidth);
					panel.RecalculateDimensions();
					panelDimens = panel.GetPreferredSize();
					minPanelWidth = minTitleDimensions.x + magicWindowData.panelWidthAdjust;
				}
				else
				{
					panel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x);
					panelDimens = panel.GetPreferredSize();
				}
			}
			else
			{
				var recalc = panel.GetPreferredSize();
				panelDimens.x = Mathf.Max(recalc.x, rect.sizeDelta.x);
				//panelDimens.y = Mathf.Max(recalc.y, rect.sizeDelta.y);
				panelDimens.y = recalc.y;   // ?TODO(Tristan): should we have a vert and horz shrink?
				panel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x);
			}

			minTitleDimensions.x = Mathf.Max(minTitleDimensions.x, panelDimens.x - magicWindowData.panelWidthAdjust, minDimensions.x);
			titlebar.SetWidth(minTitleDimensions.x);


			var newUIControlHeight = panelDimens.y + magicWindowData.panelVerticalOffset;
			newUIControlHeight = Mathf.Max(newUIControlHeight, minDimensions.y);
			panel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight);

			var tmp = titleLabel.GetComponentInChildren<TextMeshProUGUI>();
			tmp.margin = magicWindowData.titleTextMargin;

			var panelnewPos = new Vector2(0, panelVerticalOffset + magicWindowData.panelVerticalOffset);
			panel.rect.anchoredPosition = panelnewPos;


			// not really necessary? But it makes what is shown equal to what is "inside" the rect
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panel.rect.sizeDelta.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight - (panelVerticalOffset + magicWindowData.panelVerticalOffset));

			panel.GetDrawnSize();
			isDirty = false;
		}

		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}




		public override TabPanel SelectTab(int tabIndex)
		{
			return new TabPanel(titlebar, panel);
		}
	}
}