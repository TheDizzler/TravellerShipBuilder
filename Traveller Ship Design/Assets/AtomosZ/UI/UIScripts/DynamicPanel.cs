using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using static DesignManager;
using static AtomosZ.UI.DynamicPanel;
using static AtomosZ.UI.BottomPanel;
using static AtomosZ.UI.UIButtonPanel;
using System.Diagnostics;


namespace AtomosZ.UI
{
	public class UIDynamicPanelEx
	{
		public TitleLabelStyle titleLabelStyle;
		public string titleText;
		public BottomPanelStyle panelStyle;
		public Vector2 maxPanelSize;
		public bool alwaysShrinkToMinSize;
	}

	public class DynamicPanel : MonoBehaviour, IUIBehavior
	{
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


		[UDictionary.Split(50, 50)]
		[SerializeField] private UDictionary<TitleLabelStyle, Sprite> titleSprites;
		[UDictionary.Split(50, 50)]
		[SerializeField] private UDictionary<BottomPanelStyle, Sprite> panelSprites;

		/// <summary>
		/// Tab width, height, bottom panel y offset, bottom panel min height
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
			[TitleLabelStyle.SquareTab] = new Vector2(165, -26),
			[TitleLabelStyle.BladedTab] = new Vector2(165, -26),
			[TitleLabelStyle.BladedBar] = new Vector2(165, -26),
			[TitleLabelStyle.Bar] = new Vector2(165, 0),
			[TitleLabelStyle.None] = new Vector2(165, -26),
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
		/// Minimum dimensions for the entire panel, top and bottom.
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

		public TitleLabelStyle titleType;
		public BottomPanelStyle panelType;

		public DialogResult result;
		public UnityAction<DynamicPanel> OnClose;

		[SerializeField] private TextMeshProUGUI titleTMP;
		[SerializeField] private RectTransform topPanelRect;
		[SerializeField] private RectTransform bottomPanelRect;
		[SerializeField] private BottomPanel bottomPanel;

		[SerializeField] private Image titleImage;
		[SerializeField] private Image panelImage;

		[SerializeField] private HorizontalLayoutGroup panelControlLayout;
		[SerializeField] private Button minimizeButton;
		[SerializeField] private Button maximizeButton;
		[SerializeField] private Button closeButton;
		[SerializeField] public bool showMinimizeButton;
		[SerializeField] public bool showMaximizeButton;
		[SerializeField] public bool showCloseButton;

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

		public void Awake()
		{
			bottomPanel.GetControlsFromTransform();
			Refresh();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="keyInput"></param>
		/// <returns>True if input consumed.</returns>
		/// <exception cref="Exception"></exception>
		public bool Input(KeyInput keyInput)
		{
			if ((keyInput & KeyInput.Esc) == KeyInput.Esc
				&& (_designObject.isModal || isContextMenu))
			{
				SetDialogResultDefaultNegative();
				return true;
			}

			return false;
		}


		public void SetDialogResultDefaultNegative()
		{
			DialogButton buttons = bottomPanel.GetPanelButtons();

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
		/// TODO(Tristan): Allow tall title bars?
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
				// calculate child panel perfered sizes
				var minDim = bottomPanel.GetMinDimensions();
				if (minDim.x < minSize.x)
					minDim.x = minSize.x;
				if (minDim.y < minSize.y)
					minDim.y = minSize.y;


				if (titleType == TitleLabelStyle.Bar
					|| titleType == TitleLabelStyle.None)
				{
					float buttonWidth = GetControlButtonWidth();

					// calculate space from end of title to edge of panel

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
			// the bottom height can be adjusted through the bottom panel's layout.bottom
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

			panelControlLayout.transform.position = panelControlsPosition[titleType];

			UpdateTitle();
		}

#if UNITY_EDITOR
		readonly Dictionary<PanelControlType, System.Reflection.MethodInfo> functions = new()
		{
			[PanelControlType.Text] = typeof(DynamicPanel).GetMethod("AddText"),
			[PanelControlType.InputField] = typeof(DynamicPanel).GetMethod("AddInputField"),
			[PanelControlType.CheckBox] = typeof(DynamicPanel).GetMethod("AddCheckBox"),
			[PanelControlType.Slider] = typeof(DynamicPanel).GetMethod("AddSlider"),
			[PanelControlType.Button] = typeof(DynamicPanel).GetMethod("AddButton"),
			[PanelControlType.ButtonPanel] = typeof(DynamicPanel).GetMethod("AddButtonPanel"),
			[PanelControlType.Image] = typeof(DynamicPanel).GetMethod("AddImage"),
			[PanelControlType.ImagePanel] = typeof(DynamicPanel).GetMethod("AddImagePanel"),
			[PanelControlType.Dropdown] = typeof(DynamicPanel).GetMethod("AddDropdown"),
		};

		public void AddControl(CreatePanelControl createPanelControl)
		{
			var controlDataEx = createPanelControl.GetAllControlsByType()[createPanelControl.controlType];
			functions[createPanelControl.controlType].Invoke(this, new object[] { controlDataEx.Clone() });
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
		}
#endif

		//		public T AddUIControl<T>(IUIDataEx uiDataEx) where T : IUIBehavior
		//		{
		//			var uiElement = bottomPanel.AddControl(uiDataEx);
		//#if UNITY_EDITOR
		//			if (!Application.isPlaying)
		//				RecalculateDimensions();
		//#endif
		//			return uiElement;
		//		}

		/// <summary>
		/// For dialog boxes with a Result.
		/// </summary>
		/// <param name="buttons"></param>
		public void AddButtonPanel(ButtonPanelEx buttons)
		{
			bottomPanel.AddButtonPanel(buttons);
			RecalculateDimensions();
		}

		public UIImageViewPanel AddImagePanel(ImageViewDataEx viewData)
		{
			var uiElement = bottomPanel.AddImagePanel(viewData);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return uiElement;
		}

		public UIDropdown AddDropdown(DropdownEx dropdownData)
		{
			var uiElement = bottomPanel.AddDropdown(dropdownData);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return uiElement;
		}

		public ImageView AddImage(ImageEx imageEx)
		{
			var uiElemet = bottomPanel.AddImage(imageEx);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return uiElemet;
		}

		public Button AddButton(ButtonEx buttonEx)
		{
			var button = bottomPanel.AddButton(buttonEx);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return button;
		}

		public UISlider AddSlider(SliderEx sliderEx)
		{
			var slider = bottomPanel.AddSlider(sliderEx);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return slider;
		}

		public UICheckBox AddCheckBox(CheckBoxEx checkBoxEx)
		{
			var checkBox = bottomPanel.AddCheckBox(checkBoxEx);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return checkBox;
		}

		public TMP_InputField AddInputField(InputFieldEx inputFieldEx)
		{
			var inputField = bottomPanel.AddInputField(inputFieldEx);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				RecalculateDimensions();
#endif
			return inputField;
		}

		public void AddText(LabelEx labelEx)
		{
			bottomPanel.AddText(labelEx);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				Refresh();
#endif
		}

		/// <summary>
		/// _NoData tag added to prvent ambiguous call warning
		/// </summary>
		/// <param name="text"></param>
		public void AddText_NoData(string text)
		{
			bottomPanel.AddText(new LabelEx
			{
				text = text
			});
		}

		/// <summary>
		/// _NoData tag added to prvent ambiguous call warning
		/// </summary>
		/// <param name="text"></param>
		public void AddText_NoData(string text, float fontSize, Color fontColor, Vector2 minLabelDimensions, Vector2 maxLabelDimensions)
		{
			bottomPanel.AddText(new LabelEx
			{
				text = text,
				fontSize = fontSize,
				fontColor = fontColor,
				minLabelDimensions = minLabelDimensions,
				maxLabelDimensions = maxLabelDimensions,
			});
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
			bottomPanel.ShowControls(!isMinimized);
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
			bottomPanel.SetContextMenuActions(clickActions);
			RecalculateDimensions();
		}


		public void RemoveControl(UIDesignObject uiDO)
		{
			bottomPanel.RemoveControl(uiDO);
		}

		public void ClearControls()
		{
			bottomPanel.ClearControls();
		}

		[Conditional("DEBUG")]
		public void ClearControlsEditor()
		{
			bottomPanel.ClearControlsEditor();
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

		public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new NotImplementedException();
		}

		public Vector2 GetMinDimensions()
		{
			throw new NotImplementedException();
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