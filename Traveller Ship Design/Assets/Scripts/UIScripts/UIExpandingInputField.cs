using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// TODO(Tristan): Selection color, Content Type, Char limit
[Serializable]
public class InputFieldEx
{
	[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
	public string placeholderText = "Placeholder text";
	[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
	public string defaultText;
	public Vector2 fieldDimensions = new Vector2(275, 44);
	public float fontSize = 18;
	public Color placeHolderFontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 128.0f / 256);
	public Color fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
	[Tooltip("TODO(Tristan): implement this! First, find out how to set the default font in here.\nTODO(Tristan): different fonts for placeholder and default?")]
	public TMP_FontAsset fontAsset;

	public InputFieldEx() { }

	public InputFieldEx(string placeholderText, string defaultText)
	{
		this.placeholderText = placeholderText;
		this.defaultText = defaultText;
	}
}

public class UIExpandingInputField : MonoBehaviour, IUIBehavior
{
	[SerializeField] private InputFieldEx inputFieldEx;

	[SerializeField] private TextMeshProUGUI placeholderLabel;
	[SerializeField] private TextMeshProUGUI textLabel;

	[SerializeField] private TMP_InputField inputFieldTMP;
	[SerializeField] private RectTransform textAreaRect;
	[SerializeField] private Image image;

	private UIDesignObject _designObject;
	public UIDesignObject designObject
	{
		get
		{
			if (_designObject == null)
				_designObject = GetComponent<UIDesignObject>();
			return _designObject;
		}
	}


	public void UpdateInputField(InputFieldEx newInputFieldEx)
	{
		inputFieldEx = newInputFieldEx;
		UpdateInputField();
	}


	public void UpdateInputField()
	{
		//inputTMP.SetGlobalFontAsset();
		//inputFieldTMP.fontAsset = inputFieldEx.placeholderEx.fontAsset;
		inputFieldTMP.pointSize = inputFieldEx.fontSize;
		placeholderLabel.text = inputFieldEx.placeholderText;
		placeholderLabel.color = inputFieldEx.placeHolderFontColor;
		placeholderLabel.ForceMeshUpdate();


		inputFieldTMP.text = inputFieldEx.defaultText;

		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, inputFieldEx.fieldDimensions.x);
		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, inputFieldEx.fieldDimensions.y);


		var prefTextSize = placeholderLabel.GetPreferredValues("Text to measure font height");
		var textHeight = prefTextSize.y; // this should be the preferred height of a single line, right?

		if (textHeight < inputFieldEx.fieldDimensions.y)
			textHeight = inputFieldEx.fieldDimensions.y;

		var labelHeight = textHeight;

		labelHeight += Mathf.Abs(textAreaRect.offsetMin.y) + Mathf.Abs(textAreaRect.offsetMax.y);
		image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, labelHeight);
	}



	public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
	{
		throw new System.NotImplementedException();
	}

	public void Deselect()
	{
		throw new System.NotImplementedException();
	}

	public Vector2 GetMinDimensions()
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

	public void SetHover(bool isHover)
	{

	}

	public void UpdateHover(Vector3 posOfHover)
	{

	}
}
