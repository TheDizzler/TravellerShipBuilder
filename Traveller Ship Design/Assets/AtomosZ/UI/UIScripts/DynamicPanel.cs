using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using AtomosZ.MG2eTraveller.Ship;
using TMPro;

using UnityEditor;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static AtomosZ.Keyboard;
using static AtomosZ.UI.BottomPanel;
using static AtomosZ.UI.UIButtonPanel;

using Debug = UnityEngine.Debug;



namespace AtomosZ.UI
{
	[Obsolete("Replaced with MagicWindow")]
	public class DynamicPanel : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get; }
		public enum TitleLabelStyle
		{
			SquareTab,
			BladedTab,
			BladedBar,
			Bar,
			None,
		}

		public enum BottomPanelStyle
		{
			Bolted,
			Square,
			RedSquare,
			Notched_BottomRight,
			Notched_BottomLeft,
			Notched_TopLeft,
			Notched_TopRight,
		}
		public bool isDirty { get; set; }

		[UDictionary.Split(50, 50)]
		[SerializeField] private UDictionary<TitleLabelStyle, Sprite> titleSprites;
		[UDictionary.Split(50, 50)]
		[SerializeField] private UDictionary<BottomPanelStyle, Sprite> panelSprites;

		/// <summary>
		/// Tab width, height, bottom panelRect y offset, bottom panelRect min height
		/// </summary>
		private readonly Dictionary<TitleLabelStyle, Vector4> minTabSize = new()
		{
			[TitleLabelStyle.SquareTab] = new Vector4(64, 76, 64, 64),
			[TitleLabelStyle.BladedTab] = new Vector4(97, 76, 64, 64),
			[TitleLabelStyle.BladedBar] = new Vector4(80, 68, 0, 128),
			[TitleLabelStyle.Bar] = new Vector4(80, 68, 0, 128),
			[TitleLabelStyle.None] = new Vector4(0, 0, 0, 128),
		};

		private readonly Dictionary<TitleLabelStyle, Vector4> titleTextMarginSize = new()
		{
			[TitleLabelStyle.SquareTab] = new Vector4(12, 8, 12, 12),
			[TitleLabelStyle.BladedTab] = new Vector4(12, 8, 46, 12),
			[TitleLabelStyle.BladedBar] = new Vector4(12, 8, 48, 4),
			[TitleLabelStyle.Bar] = new Vector4(12, 8, 12, 4),
			[TitleLabelStyle.None] = Vector4.zero,
		};

		public readonly Dictionary<TitleLabelStyle, Vector2> panelControlsPosition = new()
		{
			[TitleLabelStyle.SquareTab] = new Vector2(135, -20),
			[TitleLabelStyle.BladedTab] = new Vector2(135, -20),
			[TitleLabelStyle.BladedBar] = new Vector2(135, 0),
			[TitleLabelStyle.Bar] = new Vector2(135, 0),
			[TitleLabelStyle.None] = new Vector2(135, -20),
		};

		public readonly Dictionary<TitleLabelStyle, Vector4> bottomPanelPadding = new()
		{
			[TitleLabelStyle.SquareTab] = new Vector4(30, 30, 14, 24),
			[TitleLabelStyle.BladedTab] = new Vector4(30, 30, 14, 24),
			[TitleLabelStyle.BladedBar] = new Vector4(30, 30, 72, 24),
			[TitleLabelStyle.Bar] = new Vector4(30, 30, 72, 24),
			[TitleLabelStyle.None] = new Vector4(30, 30, 17, 24),
			[TitleLabelStyle.None] = new Vector4(30, 30, 17, 24),
		};


		/// <summary>
		/// Minimum dimensions for the entire panelRect, top and bottom.
		/// </summary>
		private readonly Dictionary<TitleLabelStyle, Vector2> minDimensions = new()
		{
			[TitleLabelStyle.SquareTab] = new Vector2(128, 64),
			[TitleLabelStyle.BladedTab] = new Vector2(128, 64),
			[TitleLabelStyle.BladedBar] = new Vector2(128, 128),
			[TitleLabelStyle.Bar] = new Vector2(128, 128),
			[TitleLabelStyle.None] = new Vector2(64, 64),
		};

		[SerializeField] public Vector2 minSize = new Vector2(128, 64);
		[Tooltip("Max height is not used.")]
		[SerializeField] public Vector2 maxSize = new Vector2(256, 0);
		[SerializeField] public bool alwaysShrinkToMinSize;

		[SerializeField] public TitleLabelStyle titleType;
		[SerializeField] public BottomPanelStyle panelType;
		[SerializeField] public List<BottomPanel> tabs;
		[Min(0)]
		public int selectedTabIndex;


		public DialogResult result;
		public UnityAction<DynamicPanel> OnClose;

		[SerializeField] private TextMeshProUGUI titleTMP;
		[SerializeField] private RectTransform topPanelRect;
		[SerializeField] private RectTransform bottomPanelRect;
		//[SerializeField] private BottomPanel bottomPanel;

		[SerializeField] private Image titleImage;
		[SerializeField] private Image panelImage;

		[SerializeField] private HorizontalLayoutGroup panelControlLayout;
		[SerializeField] private Button minimizeButton;
		[SerializeField] private Button maximizeButton;
		[SerializeField] private Button closeButton;
		[SerializeField] public bool showMinimizeButton;
		[SerializeField] public bool showMaximizeButton;
		[SerializeField] public bool showCloseButton;

		private UIPrefabProvider uiProvider;
		/// <summary>
		/// DynamicPanel should not need a reference name.
		/// </summary>
		public string referenceName { get; set; }

		[HideInInspector]
		public UIDesignObject modalClickBlocker;
		public bool isDragging;
		public bool isContextMenu;

		public bool centerTitleText = true;
		[HideInInspector]
		[SerializeField]
		private string _titleText;
		public string titleText
		{
			get { return titleTMP.text; }
			set
			{
				_titleText = value;
				titleTMP.text = value;
				if (titleType == TitleLabelStyle.Bar)
					titleTMP.alignment = centerTitleText ? TextAlignmentOptions.Center : TextAlignmentOptions.Left;
				titleTMP.ForceMeshUpdate();
				RecalculateDimensions();
			}
		}

		private UIDesignObject _designObject;
		private bool isMinimized = false;

		public UIDesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<UIDesignObject>();
				return _designObject;
			}
		}



		/// <summary>
		/// Reset is called when the script is attached and not in playmode.
		/// </summary>
		//public void Reset()
		//{

		//}

		public void Awake()
		{
			tabs[selectedTabIndex].GetControlsFromTransform();
			Refresh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyInput"></param>
		/// <returns>True if input consumed.</returns>
		/// <exception cref="Exception"></exception>
		public bool Input(ModifierKey keyInput)
		{
			if ((keyInput & ModifierKey.Esc) == ModifierKey.Esc
				&& (_designObject.isModal || isContextMenu))
			{
				SetDialogResultDefaultNegative();
				return true;
			}

			return false;
		}


		public void SetDialogResultDefaultNegative()
		{
			DialogButton buttons = tabs[selectedTabIndex].GetPanelButtons();

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

		public void SetTitle(string newTitleText, TitleLabelStyle titleLabelType)
		{
			UpdatePanel(titleLabelType);
			titleText = newTitleText;
		}

		public void SetTitleStyle(TitleLabelStyle titleLabelType)
		{
			UpdatePanel(titleLabelType);
		}

		public void SetTitleText(string newTitleText)
		{
			titleText = newTitleText;
		}

		public void SetPanelStyle(BottomPanelStyle panelStyle)
		{
			panelType = panelStyle;
			Refresh();
		}

		public void ToggleCloseButton(bool showCloseButton)
		{
			this.showCloseButton = showCloseButton;
			Refresh();
		}

		public void ToggleMinimizeButton(bool showMinButton)
		{
			this.showMinimizeButton = showMinButton;
			Refresh();
		}

		public void ToggleMaximizeButton(bool showMaxButton)
		{
			this.showMaximizeButton = showMaxButton;
			Refresh();
		}

		/// <summary>
		/// If would be better if this didn't have to run at all if !isDirty.
		/// </summary>
		void LateUpdate()
		{
			if (isDirty)
			{
				RecalculateDimensions();
				isDirty = false;
			}
		}

		public void SelectTab(int tabIndex)
		{
			selectedTabIndex = tabIndex;
			isDirty = true;
		}

		/// <summary>
		/// 
		/// </summary>
		public void RecalculateDimensions()
		{
			var rect = GetComponent<RectTransform>();
			Vector2 size;
			if (alwaysShrinkToMinSize)
				size = minSize;
			else
				size = rect.sizeDelta;
			float titleWidth = titleTMP.GetPreferredValues(titleText).x;

			if (isMinimized)
			{
				float buttonWidth = GetControlButtonWidth();
				if (centerTitleText)
				{
					var emptySpace = (size.x - titleWidth) / 2;
					if (emptySpace < buttonWidth)
					{
						emptySpace = Mathf.Max(0, emptySpace);
						titleWidth += (buttonWidth - emptySpace) * 2;
					}
				}
				else
				{
					var emptySpace = (size.x - titleWidth);
					if (emptySpace < buttonWidth)
					{
						emptySpace = Mathf.Max(0, emptySpace);
						titleWidth += (buttonWidth - emptySpace);
					}
				}

				if (titleWidth < size.x)
					titleWidth = size.x;
				if (titleWidth < minTabSize[titleType].x)
					titleWidth = minTabSize[titleType].x;
				if (titleWidth > maxSize.x)
					titleWidth = maxSize.x;

				size.x = titleWidth;
				size.y = 96.0f;
			}
			else
			{
				// calculate child panelRect perfered sizes
				var minDim = tabs[selectedTabIndex].GetMinDimensions();
				if (minDim.x < minSize.x)
					minDim.x = minSize.x;
				if (minDim.y < minSize.y)
					minDim.y = minSize.y;


				if (titleType == TitleLabelStyle.Bar
					|| titleType == TitleLabelStyle.None)
				{
					float buttonWidth = GetControlButtonWidth();

					// calculate space from end of title to edge of panelRect

					if (centerTitleText)
					{
						var emptySpace = (size.x - titleWidth) / 2;
						if (emptySpace < buttonWidth)
						{
							emptySpace = Mathf.Max(0, emptySpace);
							titleWidth += (buttonWidth - emptySpace) * 2;
						}
					}
					else
					{
						var emptySpace = (size.x - titleWidth);
						if (emptySpace < buttonWidth)
						{
							emptySpace = Mathf.Max(0, emptySpace);
							titleWidth += (buttonWidth - emptySpace);
						}
					}

					if (titleWidth < size.x)
						titleWidth = size.x;
					if (titleWidth < minDim.x)
						titleWidth = minDim.x;
					if (titleWidth < minTabSize[titleType].x)
						titleWidth = minTabSize[titleType].x;
					if (titleWidth > maxSize.x)
						titleWidth = maxSize.x;

					size.x = titleWidth;

					var minBottomDimensions = minDimensions[titleType];

					if (minDim.y < minBottomDimensions.y)
						minDim.y = minBottomDimensions.y;
					size.y = minDim.y;
				}
				else
				{
					if (titleWidth < minTabSize[titleType].x)
						titleWidth = minTabSize[titleType].x;

					var minBottomDimensions = minDimensions[titleType];
					var tabDialogDiff = minBottomDimensions.x - minTabSize[titleType].x;
					if (titleWidth > maxSize.x - tabDialogDiff)
						titleWidth = maxSize.x - tabDialogDiff;


					var minWidthWithTitle = titleWidth + tabDialogDiff;
					if (minDim.x < minWidthWithTitle)
						minDim.x = minWidthWithTitle;
					if (minDim.x < minBottomDimensions.x)
						minDim.x = minBottomDimensions.x;
					if (minDim.y < minBottomDimensions.y)
						minDim.y = minBottomDimensions.y;

					size.y = minDim.y;

					if (size.x < minDim.x)
						size.x = minDim.x;
				}
			}

			//Debug.Log("Min dim: " + minDim);
			topPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleWidth);
			titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleWidth);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
			// this needs to be set for UI raycasting
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
			// the bottom height can be adjusted through the bottom panelRect's layout.bottom
			bottomPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		}

		private float GetControlButtonWidth()
		{
			float buttonWidth = 0;
			int count = 0;
			if (showCloseButton)
			{
				++count;
				buttonWidth += closeButton.GetComponent<RectTransform>().sizeDelta.x;
			}

			if (showMinimizeButton)
			{
				++count;
				buttonWidth += minimizeButton.GetComponent<RectTransform>().sizeDelta.x;
			}

			if (showMaximizeButton)
			{
				++count;
				buttonWidth += maximizeButton.GetComponent<RectTransform>().sizeDelta.x;
			}

			if (count > 1)
				buttonWidth += panelControlLayout.spacing * (count - 1);

			return buttonWidth + panelControlLayout.padding.left;
		}

		public void UpdateTitle()
		{
			titleText = _titleText;
		}

		public void UpdatePanel(TitleLabelStyle newTitleType)
		{
			titleType = newTitleType;
			Refresh();
		}


		/// <summary>
		/// A total recalculation of all dimensions.
		/// </summary>
		public void Refresh()
		{
			Sprite tabSprite = titleSprites[titleType];
			Sprite panelSprite = panelSprites[panelType];

			if (tabSprite == null)
			{
				titleImage.gameObject.SetActive(false);
			}
			else
			{
				titleImage.gameObject.SetActive(true);
				titleImage.sprite = tabSprite;
			}

			panelImage.sprite = panelSprite;

			var tabSize = minTabSize[titleType];
			topPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabSize.x);
			topPanelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.y);
			titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabSize.x);
			titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.y);
			titleTMP.margin = titleTextMarginSize[titleType];

			bottomPanelRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, tabSize.z, tabSize.w);
			bottomPanelRect.GetComponent<VerticalLayoutGroup>().padding.top = (int)bottomPanelPadding[titleType].z;

			closeButton.gameObject.SetActive(showCloseButton);
			minimizeButton.gameObject.SetActive(showMinimizeButton);
			maximizeButton.gameObject.SetActive(showMaximizeButton);

			panelControlLayout.transform.localPosition = panelControlsPosition[titleType];

			UpdateTitle();

#if DEBUG
			PrefabUtility.RecordPrefabInstancePropertyModifications(titleImage);
			PrefabUtility.RecordPrefabInstancePropertyModifications(panelImage);
			PrefabUtility.RecordPrefabInstancePropertyModifications(panelControlLayout);
			PrefabUtility.RecordPrefabInstancePropertyModifications(titleTMP);
			tabs[selectedTabIndex].RecordPrefabInstances();
#endif
		}



		/// <summary>
		/// Just nukes existing controls and reconstructs them with the data.
		/// </summary>
		/// <param name="uiControls"></param>
		[System.Diagnostics.Conditional("DEBUG")]
		public void UpdateData(List<UIControl> uiControls)
		{
			ClearControlsEditor();
			for (int i = 0; i < uiControls.Count; ++i)
			{
				AddUIControl(uiControls[i].GetData());
			}
		}


		/// <summary>
		/// Searchs all tab for the first instance of controlRefName.
		/// @TODO(Tristan): prevent same ref name being used on any tab.
		/// </summary>
		/// <param name="controlRefName"></param>
		/// <returns></returns>
		public IUIBehavior GetControl(string controlRefName)
		{
			var allControls = new List<UIDesignObject>();
			foreach (var tab in tabs)
			{
				var control = tab.GetControl(controlRefName);
				if (control != null)
					return control.GetComponent<IUIBehavior>();
			}

			return null;
		}

		/// <summary>
		/// Creates a UI control using the default UI scriptable objects.<br/>
		/// Cast the returned reference to the UIControl you are creating.<br/>
		/// </summary>
		/// <param name="uiControlType"></param>
		/// <returns></returns>
		public IUIBehavior AddUIControl(UIControlType uiControlType)
		{
#if DEBUG
			if (uiProvider == null)
			{
				uiProvider = transform.GetComponentInParent<UIPrefabProvider>();
				if (uiProvider == null)
				{
					Debug.LogException(new Exception("what do in prefab edit more?"));
					return null;
				}
			}
#endif

			switch (uiControlType)
			{
				case UIControlType.Text:
					return AddUIControl(new LabelEx(uiProvider.textScriptObj));

				case UIControlType.InputField:
					return AddUIControl(new InputFieldEx(uiProvider.inputFieldScriptObj));

				case UIControlType.Dropdown:
					return AddUIControl(new DropdownEx(uiProvider.dropdownScriptObj));

				case UIControlType.CheckBox:
					return AddUIControl(new CheckBoxEx(uiProvider.checkBoxScriptObj));

				case UIControlType.Button:
					return AddUIControl(new ButtonEx(uiProvider.buttonScriptObj));

				default:
					Debug.LogException(new Exception($"{uiControlType} not yet implemented"));
					return null;
			}
		}


		public IUIBehavior AddUIControl(IUIDataEx uiDataEx)
		{
			var uiElement = tabs[selectedTabIndex].AddUIControl(uiDataEx);

#if DEBUG
			if (!Application.isPlaying)
				RecalculateDimensions();
			else
				isDirty = true;
#else
			isDirty = true;
#endif
			return uiElement;
		}

		public UIButton AddButton(ButtonEx buttonEx)
		{
			return (UIButton)AddUIControl(buttonEx);
		}

		/// <summary>
		/// For dialog boxes with a Result.
		/// </summary>
		/// <param name="buttons"></param>
		public UIButtonPanel AddButtonPanel(ButtonPanelEx buttons)
		{
			return (UIButtonPanel)AddUIControl(buttons);
		}

		public UIImageViewPanel AddImagePanel(ImageViewDataEx viewData)
		{
			return (UIImageViewPanel)AddUIControl(viewData);
		}

		public UIDropdown AddDropdown(DropdownEx dropdownData)
		{
			return (UIDropdown)AddUIControl(dropdownData);
		}

		public UIImageView AddImage(ImageEx imageEx)
		{
			return (UIImageView)AddUIControl(imageEx);
		}

		public UISlider AddSlider(SliderEx sliderEx)
		{
			return (UISlider)AddUIControl(sliderEx);
		}

		public UICheckBox AddCheckBox(CheckBoxEx checkBoxEx)
		{
			return (UICheckBox)AddUIControl(checkBoxEx);
		}

		public UIExpandingInputField AddInputField(InputFieldEx inputFieldEx)
		{
			return (UIExpandingInputField)AddUIControl(inputFieldEx);
		}

		public UIExpandingLabel AddText(LabelEx labelEx)
		{
			return (UIExpandingLabel)AddUIControl(labelEx);
		}

		/// <summary>
		/// _NoData tag added to prvent ambiguous call warning
		/// </summary>
		/// <param name="text"></param>
		public UIExpandingLabel AddText_NoData(string text)
		{
			var label = (UIExpandingLabel)AddUIControl(new LabelEx(null));
			label.text = text;
			return label;
		}

		/// <summary>
		/// _NoData tag added to prvent ambiguous call warning
		/// </summary>
		/// <param name="text"></param>
		public UIExpandingLabel AddText_NoData(string text, float fontSize, Color fontColor, Vector2 minLabelDimensions, Vector2 maxLabelDimensions)
		{
			var label = (UIExpandingLabel)AddUIControl(new LabelEx(null));

			label.minLabelDimensions = minLabelDimensions;
			label.maxLabelDimensions = maxLabelDimensions;
			label.text = text;
			return label;
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




		public void Show(Vector2 position)
		{
			GetComponent<RectTransform>().anchoredPosition = position;
			RecalculateDimensions();
			gameObject.SetActive(true);
			DesignManager.ShowDialog(this);
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
			DesignManager.CloseDialog(this);
		}

		/// <summary>
		/// Minimize to a titlebar only,
		/// then move to bottom of screen?
		/// </summary>
		public void Minimize()
		{
			isMinimized = !isMinimized;
			// is this necessary?
			// @TODO(Tristan): performance testing to see if controls are still being "drawn" even if this panel is hidden
			tabs[selectedTabIndex].ShowControls(!isMinimized);
			Refresh();
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
			isContextMenu = true;
			tabs[selectedTabIndex].SetContextMenuActions(clickActions);
			RecalculateDimensions();
			isDirty = true;
		}


		public void RemoveControl(IUIDataEx data)
		{
			tabs[selectedTabIndex].RemoveControl(data);
		}

		public void RemoveControl(UIDesignObject uiDO)
		{
			tabs[selectedTabIndex].RemoveControl(uiDO);
			isDirty = true;
		}

		public void ClearControls()
		{
			tabs[selectedTabIndex].ClearControls();
			isDirty = true;
		}

		[Conditional("DEBUG")]
		public void ClearControlsEditor()
		{
			tabs[selectedTabIndex].ClearControlsEditor();
			RecalculateDimensions();
		}

		[Conditional("DEBUG")]
		public void UpdateMaxDimensions(Vector2 maxDims)
		{
			maxSize = maxDims;
			RecalculateDimensions();
		}

		[Conditional("DEBUG")]
		public void SetAlwaysShrink(bool alwaysShrink)
		{
			alwaysShrinkToMinSize = alwaysShrink;
			RecalculateDimensions();
		}

		public void ResetToLastPosition()
		{
			throw new NotImplementedException();
		}

		public UIDesignObject Select()
		{
			return designObject;
		}

		/// <summary>
		/// If modal, this should NOT be able to be closed if "deselected".
		/// However, by definition, a modal window shoud not allowed to be deselected, so the "isModal" check should happen before a call to Deselect() is even possible.
		/// </summary>
		public void Deselect()
		{
			Close();
		}

		public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new NotImplementedException();
		}

		public Vector2 GetMinDimensions()
		{
			isDirty = true;
			return Vector2.zero;
		}

		public void SetHover(bool isHover)
		{
		}

		public void UpdateHover(Vector3 posOfHover)
		{
		}

		public IUIDataEx GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			throw new NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new NotImplementedException();
		}
	}
}