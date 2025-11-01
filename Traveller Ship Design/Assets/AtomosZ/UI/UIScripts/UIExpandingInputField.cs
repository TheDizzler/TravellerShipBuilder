using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.UIExpandingLabel;

namespace AtomosZ.UI
{
	[Serializable]
	/// TODO(Tristan): Char limit.
	public class InputFieldEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.InputField; } }

		public UIExpandingInputFieldScriptableObject scriptableObj;


		public bool useCustomFontSize = false;
		public bool useCustomPlaceholderFontColor = false;
		public bool useCustomFontColor = false;
		public bool useCustomFontAsset = false;
		public float fontSize = 18;
		public Color placeholderFontColor = new Color(.2f, .2f, .2f, .2f);
		public Color fontColor = Color.black;
		public TMP_FontAsset fontAsset;


		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string placeholderText = "Placeholder text";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string defaultText = "";

		public Vector2 fieldDimensions = new Vector2(275, 44);


		public InputFieldEx(UIExpandingInputFieldScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
			if (scriptableObj == null)
			{
				useCustomFontSize = true;
				useCustomPlaceholderFontColor = true;
				useCustomFontColor = true;
				useCustomFontAsset = true;
			}

		}

		public InputFieldEx(UIExpandingInputFieldScriptableObject scriptObj, string placeholderText, string defaultText = "")
		{
			this.placeholderText = placeholderText;
			this.defaultText = defaultText;
		}

		public InputFieldEx(string placeholderText, string defaultText)
		{
			this.placeholderText = placeholderText;
			this.defaultText = defaultText;
			useCustomFontSize = true;
			useCustomPlaceholderFontColor = true;
			useCustomFontColor = true;
			useCustomFontAsset = true;
		}
	}

	[ExecuteAlways]
	public class UIExpandingInputField : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private InputFieldEx inputFieldEx;

		[SerializeField] private TextMeshProUGUI placeholderLabel;
		[SerializeField] private TextMeshProUGUI textLabel;

		[SerializeField] private TMP_InputField inputFieldTMP;
		[SerializeField] private RectTransform textAreaRect;
		[SerializeField] private Image image;


		[SerializeField] private string _referenceName = "inputField";
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				this.SetGameObjectNameToReferenceName(gameObject);
			}
		}

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

		public bool isDirty { get; set; } = true;

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}


		public void SetPlaceholderText(string newText)
		{
			placeholderLabel.text = newText;
		}

		public void SetText(string newText)
		{
			inputFieldTMP.text = newText;
		}

		public void SetTextAlignment(HorizontalAlignmentOptions horzAlignment, VerticalAlignmentOptions vertAlignment)
		{
			textLabel.verticalAlignment = vertAlignment;
			placeholderLabel.verticalAlignment = vertAlignment;
			textLabel.horizontalAlignment = horzAlignment;
			placeholderLabel.horizontalAlignment = horzAlignment;
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

		void OnEnable()
		{
			this.SetDirty();
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

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			this.SetGameObjectNameToReferenceName(gameObject);

			inputFieldTMP.fontAsset = fontAsset;
			inputFieldTMP.pointSize = fontSize;
			placeholderLabel.color = placeholderFontColor;
			placeholderLabel.text = inputFieldEx.placeholderText;

			placeholderLabel.ForceMeshUpdate();

			textLabel.color = fontColor;
			if (string.IsNullOrEmpty(inputFieldTMP.text))
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

			isDirty = false;
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
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