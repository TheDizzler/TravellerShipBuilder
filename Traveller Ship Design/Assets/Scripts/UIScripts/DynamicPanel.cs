using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static BottomPanel;
using UnityEngine.Events;
using System;
using static ButtonPanel;

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

	public readonly Dictionary<TitleLabelStyle, Vector4> bottomPanelPadding = new()
	{
		[TitleLabelStyle.SquareTab] = new Vector4(30, 30, 14, 24),
		[TitleLabelStyle.BladedTab] = new Vector4(30, 30, 14, 24),
		[TitleLabelStyle.BladedBar] = new Vector4(30, 30, 72, 24),
		[TitleLabelStyle.Bar] = new Vector4(30, 30, 72, 24),
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

	public TitleLabelStyle titleType = TitleLabelStyle.BladedBar;
	public BottomPanelStyle panelType = BottomPanelStyle.Bolted;


	public DialogResult result;
	public UnityAction<DynamicPanel> OnClose;

	[SerializeField] private string _titleText;

	[SerializeField] private TextMeshProUGUI titleTMP;
	[SerializeField] private RectTransform topPanel;
	[SerializeField] private RectTransform bottomPanel;

	[SerializeField] private Image titleImage;
	[SerializeField] private Image panelImage;


	public string titleText
	{
		get { return titleTMP.text; }
		set
		{
			_titleText = value;
			titleTMP.text = value;
			titleTMP.ForceMeshUpdate();
			RecalculateDimensions();
		}
	}

	public UIDesignObject designObject { get; }

	public void SetTitle(string newTitleText, TitleLabelStyle titleLabelType)
	{
		UpdatePanel(titleLabelType);
		titleText = newTitleText;
	}

	public TMP_InputField AddInputField(string placeholderText, string defaultText = null)
	{
		var inputField = bottomPanel.GetComponent<BottomPanel>().AddInputField(placeholderText, defaultText);
		RecalculateDimensions();
		return inputField;
	}

	/// <summary>
	/// TODO(Tristan): Enable designer toggle to auto-shrink width of panel to min size.
	/// </summary>
	public void RecalculateDimensions()
	{
		var rect = GetComponent<RectTransform>();
		// calculate child panel perfered sizes
		var dialog = bottomPanel.GetComponent<BottomPanel>();
		var minDim = dialog.GetMinDimensions();
		float titleWidth = titleTMP.GetPreferredValues(titleText).x;
		Vector2 size = rect.sizeDelta;
		if (titleType == TitleLabelStyle.Bar
			|| titleType == TitleLabelStyle.None)
		{
			if (titleWidth < minDim.x)
				titleWidth = minDim.x;
			if (titleWidth < minTabSize[titleType].x)
				titleWidth = minTabSize[titleType].x;

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
			var minWidthWithTitle = titleWidth + (minBottomDimensions.x - minTabSize[titleType].x);
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

		//Debug.Log("Min dim: " + minDim);
		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleWidth);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleWidth);
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		// the bottom height can be adjusted through the bottom panel's layout.bottom
		bottomPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
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
		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabSize.x);
		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.y);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabSize.x);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.y);
		titleTMP.margin = titleTextMarginSize[titleType];

		bottomPanel.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, tabSize.z, tabSize.w);
		bottomPanel.GetComponent<VerticalLayoutGroup>().padding.top = (int)bottomPanelPadding[titleType].z;

		UpdateTitle();
	}


	public void AddButtons(DialogButton buttons, bool recalculateDimensions)
	{
		bottomPanel.GetComponent<BottomPanel>().AddButtons(buttons);
		if (recalculateDimensions)
			RecalculateDimensions();
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
		transform.position = position;
		gameObject.SetActive(true);
	}

	public void Close()
	{
		if (OnClose != null)
			OnClose(this);
		Destroy(gameObject);
	}


	/// <summary>
	/// Each UnityAction becomes one context menu item and the string becomes the text for the button.<br/>
	/// Can add multiple methods to a single UnityAction as below:<br/>
	/// <c>
	/// UnityAction action = null;<br/>
	/// action += () => FunctionWithParam("name");<br/>
	/// action += () => FunctionNoParam();<br/>
	/// action += delegate {// some code here};</c>
	/// </summary>
	/// <param name="clickActions"></param>
	public void SetContextMenuActions(Dictionary<string, DesignAction> clickActions)
	{
		bottomPanel.GetComponent<BottomPanel>().SetContextMenuActions(clickActions);
		RecalculateDimensions();

	}

	public void ClearItems()
	{
		bottomPanel.GetComponent<BottomPanel>().ClearItems();
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
	/// By definition (?), all DynamicPanels are modal, so Deselecting it show mean that needs to close.
	/// </summary>
	/// <exception cref="NotImplementedException"></exception>
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
}
