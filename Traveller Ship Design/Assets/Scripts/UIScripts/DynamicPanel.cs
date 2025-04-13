using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static BottomPanel;
using UnityEngine.Events;

public class DynamicPanel : MonoBehaviour
{
	public enum TitleLabelType
	{
		SquareTab,
		BladedTab,
		Bladed,
	}

	/// <summary>
	/// Tab width, height, bottom panel y offset, bottom panel min height
	/// </summary>
	private readonly Dictionary<TitleLabelType, Vector4> minTabSize = new()
	{
		[TitleLabelType.SquareTab] = new Vector4(64, 76, 64, 64),
		[TitleLabelType.BladedTab] = new Vector4(97, 76, 64, 64),
		[TitleLabelType.Bladed] = new Vector4(80, 68, 0, 128),
	};

	private readonly Dictionary<TitleLabelType, Vector4> titleTextMarginSize = new()
	{
		[TitleLabelType.SquareTab] = new Vector4(12, 8, 12, 12),
		[TitleLabelType.BladedTab] = new Vector4(12, 8, 46, 12),
		[TitleLabelType.Bladed] = new Vector4(12, 8, 48, 4),
	};

	public readonly Dictionary<TitleLabelType, Vector4> bottomPanelPadding = new()
	{
		[TitleLabelType.SquareTab] = new Vector4(30, 30, 14, 24),
		[TitleLabelType.BladedTab] = new Vector4(30, 30, 14, 24),
		[TitleLabelType.Bladed] = new Vector4(30, 30, 72, 24),
	};


	/// <summary>
	/// Minimum dimensions for the entire panel, top and bottom.
	/// </summary>
	private readonly Dictionary<TitleLabelType, Vector2> minDimensions = new()
	{
		[TitleLabelType.SquareTab] = new Vector2(128, 64),
		[TitleLabelType.BladedTab] = new Vector2(128, 64),
		[TitleLabelType.Bladed] = new Vector2(128, 128),
	};

	public TitleLabelType titleType = TitleLabelType.Bladed;
	public DialogButton buttons;
	public DialogResult result;
	public UnityAction<DynamicPanel> OnClose;

	[SerializeField] private string _titleText;

	[SerializeField] private TextMeshProUGUI titleTMP;
	[SerializeField] private RectTransform topPanel;
	[SerializeField] private RectTransform bottomPanel;

	[SerializeField] private Image titleImage;
	[SerializeField] private Image panelImage;
	[SerializeField] private Sprite squareTabSprite;
	[SerializeField] private Sprite bladeTabSprite;
	[SerializeField] private Sprite bladeSprite;


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

	public void SetTitle(string newTitleText, TitleLabelType titleLabelType)
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

	public void RecalculateDimensions()
	{
		var rect = GetComponent<RectTransform>();
		// calculate child panel perfered sizes
		var dialog = bottomPanel.GetComponent<BottomPanel>();
		var minDim = dialog.GetMinDimensions();
		var titleWidth = CalculateAndSetTitleWidth();
		var minBottomDimensions = minDimensions[titleType];
		var minWidthWithTitle = titleWidth + (minBottomDimensions.x - minTabSize[titleType].x);
		if (minDim.x < minWidthWithTitle)
			minDim.x = minWidthWithTitle;
		if (minDim.x < minBottomDimensions.x)
			minDim.x = minBottomDimensions.x;
		if (minDim.y < minBottomDimensions.y)
			minDim.y = minBottomDimensions.y;
		var size = rect.sizeDelta;
		size.y = minDim.y;

		if (size.x < minDim.x)
			size.x = minDim.x;

		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
		// the bottom height can be adjusted through the bottom panel's layout.bottom
		bottomPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
	}

	private float CalculateAndSetTitleWidth()
	{
		var textWidth = titleTMP.GetPreferredValues(titleText).x;
		if (textWidth < minTabSize[titleType].x)
			textWidth = minTabSize[titleType].x;

		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

		return textWidth;
	}

	public void UpdateTitle()
	{
		titleText = _titleText;
	}

	public void UpdatePanel(TitleLabelType newTitleType)
	{
		titleType = newTitleType;

		switch (titleType)
		{
			case TitleLabelType.SquareTab:
			{
				titleImage.sprite = squareTabSprite;
			}
			break;

			case TitleLabelType.BladedTab:
			{
				titleImage.sprite = bladeTabSprite;
			}
			break;

			case TitleLabelType.Bladed:
			{
				titleImage.sprite = bladeSprite;
			}
			break;
		}

		var tabSize = minTabSize[titleType];
		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabSize.x);
		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.y);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, tabSize.x);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.y);
		titleTMP.margin = titleTextMarginSize[titleType];

		bottomPanel.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, tabSize.z, tabSize.w);
		//bottomPanel.SetInsetAndSizeFromParentEdge(
		//bottomPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, tabSize.z);

		bottomPanel.GetComponent<VerticalLayoutGroup>().padding.top = (int)bottomPanelPadding[titleType].z;
		RecalculateDimensions();
	}


	public void SetButtons(DialogButton buttons)
	{
		bottomPanel.GetComponent<BottomPanel>().SetButtons(buttons);
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


	public void Close()
	{
		if (OnClose != null)
			OnClose(this);
		gameObject.SetActive(false);
	}
}
