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
		public PanelControlType dataType { get { return PanelControlType.InputField; } }

		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string placeholderText = "Placeholder text";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string defaultText;
		public Vector2 fieldDimensions = new Vector2(275, 44);
		public float fontSize = 18;
		public Color placeHolderFontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 128.0f / 256);
		public Color fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
		public TMP_FontAsset fontAsset;

		public InputFieldEx() { }

		public InputFieldEx(string placeholderText, string defaultText)
		{
			this.placeholderText = placeholderText;
			this.defaultText = defaultText;
		}

		public void ResetToDefaults()
		{
			placeholderText = "Placeholder text";
			defaultText = "";
			fieldDimensions = new Vector2(275, 44);
			fontSize = 18;
			placeHolderFontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 128.0f / 256);
			fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
			fontAsset = null;
		}

		public object Clone()
		{
			return this.MemberwiseClone();
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
			inputFieldTMP.fontAsset = inputFieldEx.fontAsset;
			inputFieldTMP.pointSize = inputFieldEx.fontSize;
			placeholderLabel.text = inputFieldEx.placeholderText;
			placeholderLabel.color = inputFieldEx.placeHolderFontColor;
			placeholderLabel.ForceMeshUpdate();

			textLabel.color = inputFieldEx.fontColor;

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

		public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
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