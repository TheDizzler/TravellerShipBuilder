using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	/// TODO(Tristan): Char limit.
	public class InputFieldEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.InputField; } }

		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string placeholderText = "Placeholder text";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string defaultText;

		public Vector2 fieldDimensions = new Vector2(275, 44);


		public UIExpandingInputFieldScriptableObject scriptableObj;


		public bool useCustomFontSize = true;
		public bool useCustomPlaceholderFontColor = true;
		public bool useCustomFontColor = true;
		public bool useCustomFontAsset = true;
		public float fontSize;
		public Color placeholderFontColor;
		public Color fontColor;
		public TMP_FontAsset fontAsset;

		public InputFieldEx(UIExpandingInputFieldScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
			useCustomFontSize = false;
			useCustomPlaceholderFontColor = false;
			useCustomFontColor = false;
			useCustomFontAsset = false;
		}

		public InputFieldEx()
		{
			ResetToDefaults();
		}

		public InputFieldEx(string placeholderText, string defaultText)
		{
			this.placeholderText = placeholderText;
			this.defaultText = defaultText;
			ResetToDefaults();
		}

		public void SetToScriptableObjectValues()
		{
			if (scriptableObj == null)
				ResetToDefaults();
			else
			{
				fontSize = scriptableObj.fontSize;
				fontColor = scriptableObj.fontColor;
				fontAsset = scriptableObj.fontAsset;
				placeholderFontColor = scriptableObj.placeholderFontColor;
			}
		}


		public void ResetToDefaults()
		{
			useCustomFontSize = true;
			useCustomPlaceholderFontColor = true;
			useCustomFontColor = true;
			useCustomFontAsset = true;

			placeholderText = "Placeholder text";
			defaultText = "";
			fieldDimensions = new Vector2(275, 44);

			placeholderFontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 128.0f / 256);
			fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
			fontSize = 18;
			fontAsset = null;
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

		public TMP_FontAsset fontAsset
		{
			get
			{
				if (inputFieldEx.useCustomFontAsset || inputFieldEx.scriptableObj == null)
					return inputFieldEx.fontAsset;
				return inputFieldEx.scriptableObj.fontAsset;
			}
		}

		public float fontSize
		{
			get
			{
				if (inputFieldEx.useCustomFontSize || inputFieldEx.scriptableObj == null)
					return inputFieldEx.fontSize;
				return inputFieldEx.scriptableObj.fontSize;
			}
		}

		public Color placeholderFontColor
		{
			get
			{
				if (inputFieldEx.useCustomPlaceholderFontColor || inputFieldEx.scriptableObj == null)
					return inputFieldEx.placeholderFontColor;
				return inputFieldEx.scriptableObj.placeholderFontColor;
			}
		}

		public Color fontColor
		{
			get
			{
				if (inputFieldEx.useCustomFontColor || inputFieldEx.scriptableObj == null)
					return inputFieldEx.fontColor;
				return inputFieldEx.scriptableObj.fontColor;
			}
		}

		public IUIDataEx GetBackingData()
		{
			return inputFieldEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			inputFieldEx = (InputFieldEx)backingData;
			UpdateBackingData();
		}


		public void UpdateBackingData()
		{
			inputFieldTMP.fontAsset = fontAsset;
			inputFieldTMP.pointSize = fontSize;
			placeholderLabel.color = placeholderFontColor;
			placeholderLabel.text = inputFieldEx.placeholderText;

			placeholderLabel.ForceMeshUpdate();

			textLabel.color = fontColor;
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


		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			return image.rectTransform.sizeDelta;
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}

		public void Deselect()
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
}