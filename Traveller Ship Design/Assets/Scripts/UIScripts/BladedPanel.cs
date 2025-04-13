using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class BladedPanel : MonoBehaviour
{
	[SerializeField] private Vector2 minDimensions;
	[SerializeField] private string _titleText;
	[SerializeField] private float minTitleWidth;

	[SerializeField] private RectTransform topPanel;
	[SerializeField] private TextMeshProUGUI titleTMP;
	[SerializeField] private RectTransform bottomPanel;

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
		var dialog = bottomPanel.GetComponent<DialogPanel>();
		var minDim = dialog.GetMinDimensions();
		var titleWidth = CalculateAndSetTitleWidth();
		var minWidthWithTitle = titleWidth + (minDimensions.x - minTitleWidth);
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
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
	}

	private float CalculateAndSetTitleWidth()
	{
		var textWidth = titleTMP.GetPreferredValues(_titleText).x;
		if (textWidth < minTitleWidth)
			textWidth = minTitleWidth;

		topPanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		titleTMP.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);

		return textWidth;
	}

	public void UpdateTitle()
	{
		titleText = _titleText;
		RecalculateDimensions();
	}
}
