using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DynamicPanel;
using static TabbedPanel;
using UnityEngine.UI;
using TMPro;

public class DynamicPanel : MonoBehaviour
{
	public enum TitleType
	{
		SquareTab,
		BladedTab,
		Bladed,
	}

	/// <summary>
	/// Tab width, height, bottom panel y offset, bottom panel min height
	/// </summary>
	private readonly Dictionary<TitleType, Vector4> minTabSize = new()
	{
		[TitleType.SquareTab] = new Vector4(64, 76, 64, 64),
		[TitleType.BladedTab] = new Vector4(97, 76, 64, 64),
		[TitleType.Bladed] = new Vector4(80, 68, 0, 128),
	};

	private readonly Dictionary<TitleType, Vector4> titleTextMarginSize = new()
	{
		[TitleType.SquareTab] = new Vector4(12, 8, 12, 12),
		[TitleType.BladedTab] = new Vector4(12, 8, 46, 12),
		[TitleType.Bladed] = new Vector4(12, 8, 48, 4),
	};

	public readonly Dictionary<TitleType, Vector4> bottomPanelPadding = new()
	{
		[TitleType.SquareTab] = new Vector4(30, 30, 14, 24),
		[TitleType.BladedTab] = new Vector4(30, 30, 14, 24),
		[TitleType.Bladed] = new Vector4(30, 30, 72, 24),
	};

	//public readonly Dictionary<TitleType, float> heightDiff = new()
	//{
	//	[TitleType.SquareTab] = 0,
	//	[TitleType.BladedTab] = 0,
	//	[TitleType.Bladed] = 0,
	//};

	/// <summary>
	/// Minimum dimensions for the entire panel, top and bottom.
	/// </summary>
	private readonly Vector2 minDimensions = new Vector2(128, 128);

	public TitleType titleType = TitleType.Bladed;
	public bool isDialog = false;

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

	public void RecalculateDimensions()
	{
		var rect = GetComponent<RectTransform>();
		// calculate child panel perfered sizes
		var dialog = bottomPanel.GetComponent<BottomPanel>();
		var minDim = dialog.GetMinDimensions();
		var titleWidth = CalculateAndSetTitleWidth();
		var minWidthWithTitle = titleWidth + (minDimensions.x - minTabSize[titleType].x);
		if (minDim.x < minWidthWithTitle)
			minDim.x = minWidthWithTitle;
		if (minDim.x < minDimensions.x)
			minDim.x = minDimensions.x;
		if (minDim.y < minDimensions.y)
			minDim.y = minDimensions.y;
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

	public void UpdatePanel(TitleType newTitleType)
	{
		titleType = newTitleType;

		switch (titleType)
		{
			case TitleType.SquareTab:
			{
				titleImage.sprite = squareTabSprite;
			}
			break;

			case TitleType.BladedTab:
			{
				titleImage.sprite = bladeTabSprite;
			}
			break;

			case TitleType.Bladed:
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
}
