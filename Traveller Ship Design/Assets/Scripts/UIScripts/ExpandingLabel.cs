using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ExpandingLabel : MonoBehaviour
{
	[SerializeField] private float minLabelWidth = 1;
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
		set
		{
			textLabel.color = value;
		}
	}

	private void UpdateLabel()
	{
		var textWidth = textLabel.GetPreferredValues(_titleText).x;
		var padding = textLabel.margin.x + textLabel.margin.z;
		if (textWidth < minLabelWidth)
			textWidth = minLabelWidth;

		textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
		spriteLabel.size = new Vector2(textWidth + padding, spriteLabel.size.y);
	}

	public void UpdateText()
	{
		text = _titleText;
		UpdateLabel();
	}
}
