using System;
using System.Collections;

using TMPro;

using UnityEngine;

using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class LabelEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Text; } }
		/// <summary>
		/// This is the name we use to modify this UIControl.
		/// </summary>
		public string referenceName;

		[Tooltip("IMPORTANT(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text = "Text Label";

		public UIExpandingLabelScriptableObject scriptableObj;

		public bool useCustomFontSize = false;
		public bool useCustomFontColor = false;
		public bool useCustomFontAsset = false;
		[Tooltip("Default: 36")]
		[SerializeField] public float fontSize = 36;
		[SerializeField] public Color fontColor = Color.white;
		[SerializeField] public TMP_FontAsset fontAsset;

		[SerializeField] public Vector2 minLabelDimensions = new Vector2(64, 10);
		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		[SerializeField] public Vector2 maxLabelDimensions = new Vector2(1025, 256);


		public LabelEx(string text)
		{
			this.text = text;
			useCustomFontSize = true;
			useCustomFontColor = true;
			useCustomFontAsset = true;
		}

		public LabelEx(UIExpandingLabelScriptableObject textScriptObj)
		{
			this.scriptableObj = textScriptObj;
			if (scriptableObj == null)
			{
				useCustomFontSize = true;
				useCustomFontColor = true;
				useCustomFontAsset = true;
			}

		}


		/// <summary>
		/// @IMPORTANT(Tristan): This is required for UISlider & UIImageView!
		/// </summary>
		/// <returns></returns>
		public LabelEx Clone()
		{
			return (LabelEx)this.MemberwiseClone();
		}


	}

	public class UIExpandingLabel : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private LabelEx labelEx;

		[SerializeField] private TextMeshProUGUI textLabel;
		[SerializeField] private Image image;

		public string referenceName { get { return labelEx.referenceName; } }
		/// <summary>
		/// NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's "empty", 
		/// so the length will NEVER equal zero!
		/// </summary>
		public string text
		{
			get { return textLabel.text; }
		}

		public void SetText(string text, bool recalculateDimensions)
		{
			labelEx.text = text;
			if (!recalculateDimensions)
				return;
			this.Refresh(gameObject);
		}


		public Color color
		{
			get
			{
				if (labelEx.useCustomFontColor || labelEx.scriptableObj == null)
					return labelEx.fontColor;
				return labelEx.scriptableObj.fontColor;
			}
		}

		public void SetColor(Color newColor)
		{
			labelEx.fontColor = newColor;
			textLabel.color = newColor;
			labelEx.useCustomFontColor = true;
		}

		public float fontSize
		{
			get
			{
				if (labelEx.useCustomFontSize || labelEx.scriptableObj == null)
					return labelEx.fontSize;
				return labelEx.scriptableObj.fontSize;
			}
		}

		public TMP_FontAsset fontAsset
		{
			get
			{
				if (labelEx.useCustomFontAsset || labelEx.scriptableObj == null)
					return labelEx.fontAsset;
				return labelEx.scriptableObj.fontAsset;
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

		public IUIDataEx GetBackingData()
		{
			return labelEx;
		}



		public void UpdateBackingData(IUIDataEx dataEx)
		{
			labelEx = (LabelEx)dataEx;
			UpdateBackingData();
		}


		public void UpdateBackingData()
		{
			if (string.IsNullOrEmpty(labelEx.referenceName))
				labelEx.referenceName = transform.name;
			//name = labelEx.referenceName;

			textLabel.text = labelEx.text;
			textLabel.color = color;
			textLabel.font = fontAsset;
			textLabel.fontSize = fontSize;

			textLabel.ForceMeshUpdate(false, true);

			var prefTextSize = textLabel.GetPreferredValues(text);
			var singleLineTextHeight = prefTextSize.y; // this is the height of a single line
			var singleLineTextWidth = prefTextSize.x; // this is the width if the text was on a single line

			var prefTextWidth = textLabel.preferredWidth;


			//var renderedValues = textLabel.GetRenderedValues();

			var newWidth = prefTextSize.x;
			if (newWidth > prefTextWidth)
				newWidth = prefTextWidth;
			if (newWidth > singleLineTextWidth)
				newWidth = singleLineTextWidth;
			if (newWidth > labelEx.maxLabelDimensions.x)
				newWidth = labelEx.maxLabelDimensions.x;
			else if (newWidth < labelEx.minLabelDimensions.x)
				newWidth = labelEx.minLabelDimensions.x;

			var textLabelRect = textLabel.rectTransform;
			textLabelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			textLabel.ForceMeshUpdate(false, true);
			//Canvas.ForceUpdateCanvases();


			//var prefTextHeight = textLabel.preferredHeight;

			//var pixelAdjWidth = textLabel.GetPixelAdjustedRect().width; // this is only accurate when canvas is in Screen Space
			//var newLineCount = singleLineTextWidth / newWidth;
			//if (singleLineTextWidth - pixelAdjWidth < 1)
			//	newLineCount = 1;
			//float newHeight = Mathf.Ceil(newLineCount) * singleLineTextHeight;
			//var prefValues = textLabel.GetPreferredValues();
			//if (prefValues.y != newHeight)
			//	newHeight = prefValues.y;
			//var newHeight = textLabel.GetPreferredValues().y;
			var prefForcedWidth = textLabel.GetPreferredValues(textLabelRect.sizeDelta.x - (textLabel.margin.x + textLabel.margin.z), 0);
			var newHeight = prefForcedWidth.y;
			if (newHeight < labelEx.minLabelDimensions.y)
				newHeight = labelEx.minLabelDimensions.y;
			//if (newHeight > 50)
			//	newHeight = newHeight ;
			textLabelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			textLabel.ForceMeshUpdate(false, true);


			if (image != null)
			{
				var imageLabelSize = new Vector2(newWidth, newHeight);
				//imageLabelSize.x += (textLabel.margin.x + textLabel.margin.z);
				imageLabelSize.y = newHeight /*+ textLabel.margin.y*/ /*+ textLabel.margin.w*/;
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageLabelSize.x);
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageLabelSize.y);
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

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}
	}
}