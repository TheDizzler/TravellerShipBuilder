using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIExpandingLabel : MonoBehaviour, IUIBehavior
{
	[SerializeField] private Vector2 minLabelDimensions = new Vector2(64, 72);
	[SerializeField] private Vector2 maxLabelDimensions = new Vector2(1025, 0);
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

	public UIDesignObject designObject { get; }

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
		if (image != null)
		{
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, labelSize.x);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, labelSize.y);
		}
	}

	/// <summary>
	///  NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's "empty", 
	///  so the length will NEVER equal zero!
	/// </summary>
	public void UpdateText()
	{
		text = _titleText;
	}

	public void ResetToLastPosition()
	{
		throw new System.NotImplementedException();
	}

	public UIDesignObject Select()
	{
		throw new System.NotImplementedException();
	}

	public void Deselect()
	{
		throw new System.NotImplementedException();
	}

	public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
	{
		throw new System.NotImplementedException();
	}

	public Vector2 GetMinDimensions()
	{
		var size = textLabel.rectTransform.sizeDelta;
		size.x += textLabel.margin.x + textLabel.margin.z;
		size.y += textLabel.margin.y + textLabel.margin.w;
		return size;
	}
}
