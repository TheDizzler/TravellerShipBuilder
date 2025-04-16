using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExpandingLabel : MonoBehaviour
{
	[SerializeField] private Vector2 minLabelDimensions = new Vector2(1, .5f);
	[SerializeField] private Vector2 maxLabelDimensions = new Vector2(5, 2.5f);
	[SerializeField] private TextMeshPro textLabel;
	[SerializeField] private SpriteRenderer spriteLabel;

	[SerializeField] private string _titleText;

	public string text
	{
		get { return textLabel.text; }
		set
		{
			_titleText = value;
			textLabel.text = value;
			textLabel.ForceMeshUpdate();
			UpdateLabel();
		}
	}

	public Color color
	{
		get { return textLabel.color; }
		set { textLabel.color = value; }
	}

	private void UpdateLabel()
	{
		var prefTextSize = textLabel.GetPreferredValues(_titleText);
		var rendTextSize = textLabel.GetRenderedValues();
		var horzPadding = textLabel.margin.x + textLabel.margin.z;
		var textWidth = prefTextSize.x;
		var textHeight = prefTextSize.y; // this should be the preferred height of a single line, right?

		float vertPadding = 0;
		

		if (textWidth < minLabelDimensions.x)
			textWidth = minLabelDimensions.x;
		if (textHeight < minLabelDimensions.y)
			textHeight = minLabelDimensions.y;

		var labelSize = new Vector2(textWidth + horzPadding, textHeight + vertPadding);
		if (textWidth > maxLabelDimensions.x)
		{
			textWidth = maxLabelDimensions.x;
			textHeight = rendTextSize.y;
			labelSize.x = rendTextSize.x + horzPadding;
			labelSize.y = textHeight + textLabel.margin.y + textLabel.margin.w;
		}

		var rect = textLabel.rectTransform;
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
		spriteLabel.size = labelSize;
	}

	public void UpdateText()
	{
		text = _titleText;
	}
}
