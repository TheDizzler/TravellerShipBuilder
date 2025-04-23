using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExpandingLabel : MonoBehaviour
{
	[SerializeField] private Vector2 minLabelDimensions = new Vector2(1, .5f);
	[SerializeField] private Vector2 maxLabelDimensions = new Vector2(5, 2.5f);
	[SerializeField] private TextMeshProUGUI textLabel;
	[SerializeField] private Image image;

	[SerializeField] private string _titleText;

	/// <summary>
	///  NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's "empty", 
	///  so the length will NEVER equal zero!
	/// </summary>
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
		var horzPadding = textLabel.margin.x + textLabel.margin.z;
		var textWidth = prefTextSize.x;
		var textHeight = prefTextSize.y; // this should be the preferred height of a single line, right?

		float vertPadding = 0;


		if (textWidth < minLabelDimensions.x)
			textWidth = minLabelDimensions.x;
		if (textHeight < minLabelDimensions.y)
			textHeight = minLabelDimensions.y;

		var tmpRect = textLabel.rectTransform;
		var labelSize = new Vector2(textWidth /*+ horzPadding*/, textHeight /*+ vertPadding*/);
		if (textWidth > maxLabelDimensions.x)
		{
			var rendTextSize = textLabel.GetRenderedValues();
			textWidth = maxLabelDimensions.x;
			textHeight = rendTextSize.y;
			labelSize.x = textWidth;

			tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
			textLabel.ForceMeshUpdate();
			rendTextSize = textLabel.GetRenderedValues();
			tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rendTextSize.y);

			labelSize.y = rendTextSize.y + textLabel.margin.y + textLabel.margin.w;
		}


		tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, labelSize.x);
		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, labelSize.y);
	}

	/// <summary>
	///  NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's "empty", 
	///  so the length will NEVER equal zero!
	/// </summary>
	public void UpdateText()
	{
		text = _titleText;
	}
}
