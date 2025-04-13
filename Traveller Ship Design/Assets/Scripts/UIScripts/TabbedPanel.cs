using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabbedPanel : MonoBehaviour
{
	[SerializeField] private Vector2 minDimensions;
	[SerializeField] private Vector2 maxDimensions;
	[SerializeField] private string _titleText;
	[SerializeField] private TabType tabType = TabType.Blade;

	[SerializeField] private RectTransform topPanel;
	[SerializeField] private TextMeshProUGUI titleTMP;
	[SerializeField] private Image tabImage;
	[SerializeField] private Sprite squareSprite;
	[SerializeField] private Sprite bladeSprite;
	[SerializeField] private GameObject bottomPanel;

	public string titleText
	{
		get { return titleTMP.text; }
		set
		{
			_titleText = value;
			titleTMP.text = value;
			titleTMP.ForceMeshUpdate();
			UpdateTitlePanel();
		}
	}

	public enum TabType
	{
		Square,
		Blade
	}


	private float minSquareTabWidth = 64;
	private float minBladeTabWidth = 97;
	private float minTabWidth;

	public void UpdateTitlePanel()
	{
		var textWidth = titleTMP.GetPreferredValues(_titleText).x;
		//var padding = titleTMP.margin.x + titleTMP.margin.z;
		if (textWidth < minTabWidth)
			textWidth = minTabWidth;

		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

		var panelRect = bottomPanel.GetComponent<RectTransform>();
		var minPanelWidth = textWidth + (minDimensions.x - minTabWidth);
		if (panelRect.sizeDelta.x < minPanelWidth)
			panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minPanelWidth);
	}

	public void UpdatePanel()
	{
		switch (tabType)
		{
			case TabType.Square:
			{
				tabImage.sprite = squareSprite;
				minTabWidth = minSquareTabWidth;
				var newMargin = titleTMP.margin;
				newMargin.z = 12;
				titleTMP.margin = newMargin;
			}
			break;

			case TabType.Blade:
			{
				tabImage.sprite = bladeSprite;
				minTabWidth = minBladeTabWidth;
				var newMargin = titleTMP.margin;
				newMargin.z = 46;
				titleTMP.margin = newMargin;
			}
			break;
		}

		UpdateTitlePanel();

		titleText = _titleText;
	}
}
