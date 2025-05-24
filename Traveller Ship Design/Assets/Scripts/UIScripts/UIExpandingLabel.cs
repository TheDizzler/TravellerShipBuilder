using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class LabelEx : UIDataEx
{
	public PanelItemType dataType { get { return PanelItemType.Text; } }

	[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
	public string text = "Text Label";
	public Vector2 minLabelDimensions = new Vector2(64, 1);
	[Tooltip("Max height is not used.")]
	public Vector2 maxLabelDimensions = new Vector2(1025, 0);
	public float fontSize = 36;
	public Color fontColor = Color.white;
	[Tooltip("TODO(Tristan): implement this!")]
	public TMP_FontAsset fontAsset;

	public LabelEx() { }

	public LabelEx(string text)
	{
		this.text = text;
	}

	public void ResetToDefaults()
	{
		text = "Text Label";
		minLabelDimensions = new Vector2(64, 1);
		maxLabelDimensions = new Vector2(1025, 0);
		fontSize = 36;
		fontColor = Color.white;
		fontAsset = null;
	}

	public object Clone()
	{
		return this.MemberwiseClone();
	}
}

public class UIExpandingLabel : MonoBehaviour, IUIBehavior
{
	[SerializeField] private LabelEx labelEx;

	[SerializeField] private TextMeshProUGUI textLabel;
	[SerializeField] private Image image;

	/// <summary>
	/// TODO(Tristan): Now that using LabelEx use of this property should be strongly guarded.
	///  NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's "empty", 
	///  so the length will NEVER equal zero!
	/// </summary>
	public string text
	{
		get { return textLabel.text; }
		set
		{
			labelEx.text = value;
			UpdateBackingData();
		}
	}

	public Color color
	{
		get { return labelEx.fontColor; }
		set
		{
			labelEx.fontColor = value;
			UpdateBackingData();
		}
	}

	public UIDesignObject _designObject;
	public UIDesignObject designObject
	{
		get
		{
			if (_designObject == null)
				_designObject = GetComponent<UIDesignObject>();
			return _designObject;
		}
	}

	public UIDataEx GetBackingData()
	{
		return labelEx;
	}

	public void UpdateBackingData(UIDataEx backingData)
	{
		labelEx = (LabelEx)backingData;
		text = labelEx.text;
	}


	public void UpdateBackingData()
	{
		textLabel.text = labelEx.text;
		textLabel.fontSize = labelEx.fontSize;
		textLabel.color = labelEx.fontColor;
		textLabel.font = labelEx.fontAsset;
		textLabel.ForceMeshUpdate();

		var prefTextSize = textLabel.GetPreferredValues(text);
		var textWidth = prefTextSize.x;
		var textHeight = prefTextSize.y; // this should be the preferred height of a single line, right?

		if (textWidth < labelEx.minLabelDimensions.x)
			textWidth = labelEx.minLabelDimensions.x;
		if (textHeight < labelEx.minLabelDimensions.y)
			textHeight = labelEx.minLabelDimensions.y;

		var tmpRect = textLabel.rectTransform;
		var labelSize = new Vector2(textWidth, textHeight);
		if (textWidth > labelEx.maxLabelDimensions.x)
		{
			textWidth = labelEx.maxLabelDimensions.x;
			labelSize.x = textWidth;

			tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
			textLabel.ForceMeshUpdate();
			var rendTextSize = textLabel.GetRenderedValues();
			tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rendTextSize.y);

			labelSize.y = rendTextSize.y + textLabel.margin.y + textLabel.margin.w;
		}
		else
		{
			tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, textWidth);
			tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, textHeight);
		}

		if (image != null)
		{
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, labelSize.x);
			image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, labelSize.y);
		}
	}

	public Vector2 GetMinDimensions()
	{
		UpdateBackingData();
		if (image == null)
		{
			var size = textLabel.rectTransform.sizeDelta;
			size.x += textLabel.margin.x + textLabel.margin.z;
			size.y += textLabel.margin.y + textLabel.margin.w;
			return size;
		}
		else
		{
			return image.rectTransform.sizeDelta;
		}
	}

	public void SetHover(bool isHover)
	{
		throw new System.NotImplementedException();
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		throw new System.NotImplementedException();
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
}
