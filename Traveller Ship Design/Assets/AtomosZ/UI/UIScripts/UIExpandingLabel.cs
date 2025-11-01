using System;
using System.Collections;
using System.Data.Common;
using TMPro;

using UnityEngine;

using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class LabelEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Text; } }

		public UIExpandingLabelScriptableObject scriptableObj;

		public bool useCustomFontSize = false;
		public bool useCustomFontColor = false;
		public bool useCustomFontAsset = false;
		[Tooltip("Default: 36")]
		[SerializeField] public float fontSize = 36;
		[SerializeField] public Color fontColor = Color.white;
		[SerializeField] public TMP_FontAsset fontAsset;




		public LabelEx(/*string text*/)
		{
			//this.text = text;
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

	[ExecuteAlways]
	public class UIExpandingLabel : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private LabelEx labelEx;

		[SerializeField] private TextMeshProUGUI textLabel;
		[SerializeField] private Image image;

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

		public bool isDirty { get; set; } = true;

		[SerializeField] private string _referenceName;
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				this.SetGameObjectNameToReferenceName(gameObject);
			}
		}

		[SerializeField] private string _text = "Text Label";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get { return _text = textLabel.text; }
			set
			{
				if (text == value)
					return;
				textLabel.text = _text = value;
				this.SetDirty();
			}
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
			this.SetDirty();
		}

		public float fontSize
		{
			get
			{
				if (labelEx.useCustomFontSize || labelEx.scriptableObj == null)
					return labelEx.fontSize;
				return labelEx.scriptableObj.fontSize;
			}
			set
			{
				labelEx.fontSize = value;
				labelEx.useCustomFontSize = true;
				this.SetDirty();
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

		[SerializeField] private FontStyles _fontStyles;
		public FontStyles fontStyles
		{
			get { return _fontStyles = textLabel.fontStyle; }
			set
			{
				if (fontStyles == value)
					return;
				_fontStyles = value;
				textLabel.fontStyle = value;
				this.SetDirty();
			}
		}

		[SerializeField] private TextAlignmentOptions _alignmentOptions;
		public TextAlignmentOptions alignmentOptions
		{
			get { return _alignmentOptions = (TextAlignmentOptions)((int)textLabel.verticalAlignment | (int)textLabel.horizontalAlignment); }
			set
			{
				if (alignmentOptions == value)
					return;
				_alignmentOptions = value;
				var vert = (VerticalAlignmentOptions)(value
					& (TextAlignmentOptions)(VerticalAlignmentOptions.Baseline | VerticalAlignmentOptions.Bottom
					| VerticalAlignmentOptions.Capline | VerticalAlignmentOptions.Geometry
					| VerticalAlignmentOptions.Middle | VerticalAlignmentOptions.Top));

				textLabel.verticalAlignment = vert;

				var horz = (HorizontalAlignmentOptions)(value ^ (TextAlignmentOptions)vert);
				textLabel.horizontalAlignment = horz;
				this.SetDirty();
			}
		}

		[SerializeField] private Vector4 _margin;
		public Vector4 margin
		{
			get { return _margin = textLabel.margin; }
			set
			{
				if (margin == value)
					return;
				_margin = value;
				textLabel.margin = _margin;
				this.SetDirty();
			}
		}

		[SerializeField] private Vector2 _minLabelDimensions = new Vector2(64, 10);
		public Vector2 minLabelDimensions
		{
			get { return _minLabelDimensions; }
			set
			{
				_minLabelDimensions = value;
				this.SetDirty();
			}
		}


		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		[SerializeField] private Vector2 _maxLabelDimensions = new Vector2(1025, 256);
		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		public Vector2 maxLabelDimensions
		{
			get { return _maxLabelDimensions; }
			set
			{
				_maxLabelDimensions = value;
				this.SetDirty();
			}
		}


		/// <summary>
		/// This may modify the min and max lable dimensions to accomodate the new width.
		/// Force the size of the label.
		/// Used on tab/titlebar.
		/// </summary>
		/// <param name="newWidth"></param>
		public void SetWidth(float newWidth)
		{
			var labelDims = minLabelDimensions;
			if (labelDims.x > newWidth)
			{
				labelDims.x = newWidth;
				minLabelDimensions = labelDims;
			}

			labelDims = maxLabelDimensions;
			if (labelDims.x < newWidth)
			{
				labelDims.x = newWidth;
				minLabelDimensions = labelDims;
			}

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			// need to this.SetDirty();?
		}

		/// <summary>
		/// This may modify the min and max dimensions to accomodate the new height.
		/// </summary>
		/// <param name="newHeight"></param>
		public void SetHeight(float newHeight)
		{
			var newDims = minLabelDimensions;
			if (newDims.y > newHeight)
			{
				newDims.y = newHeight;
				minLabelDimensions = newDims;
			}

			newDims = maxLabelDimensions;
			if (newDims.y < newHeight)
			{
				newDims.y = newHeight;
				minLabelDimensions = newDims;
			}

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
		}

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		void OnEnable()
		{
			this.SetDirty();
		}

		public IUIDataEx GetBackingData()
		{
			return labelEx;
		}



		public void UpdateBackingData(IUIDataEx dataEx)
		{
			labelEx = (LabelEx)dataEx;
			this.SetDirty();
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}


		public void UpdateBackingData()
		{
			this.SetGameObjectNameToReferenceName(gameObject);

			textLabel.color = color;
			textLabel.font = fontAsset;
			textLabel.fontSize = fontSize;

			textLabel.ForceMeshUpdate(false, true);

			var prefTextSize = textLabel.GetPreferredValues(text);
			var singleLineTextHeight = prefTextSize.y; // this is the height of a single line, assuming no linefeed
			var singleLineTextWidth = prefTextSize.x; // this is the width if the text was on a single line

			var prefTextWidth = textLabel.preferredWidth;


			//var renderedValues = textLabel.GetRenderedValues();

			var newWidth = prefTextSize.x;
			if (newWidth > prefTextWidth)
				newWidth = prefTextWidth;
			if (newWidth > singleLineTextWidth)
				newWidth = singleLineTextWidth;
			if (newWidth > maxLabelDimensions.x && maxLabelDimensions.x > 0)
				newWidth = maxLabelDimensions.x;
			else if (newWidth < minLabelDimensions.x)
				newWidth = minLabelDimensions.x;

			var prefTextHeight = textLabel.preferredHeight;
			var textLabelRect = textLabel.rectTransform;
			textLabelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			textLabel.ForceMeshUpdate(false, true);
			var bounds = textLabel.bounds;
			var textBounds = textLabel.textBounds;

			var newHeight = textLabel.preferredHeight;
			newHeight = Mathf.Min(newHeight, prefTextHeight); // is this right?

			//var prefForcedSize = textLabel.GetPreferredValues(textLabelRect.sizeDelta.x - (textLabel.margin.x + textLabel.margin.z), 0);
			//var newHeight = prefForcedSize.y;
			//var newHeight = prefTextHeight;
			if (newHeight < minLabelDimensions.y)
				newHeight = minLabelDimensions.y;
			textLabelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			textLabel.ForceMeshUpdate(false, true);


			if (image != null)
			{
				var imageLabelSize = new Vector2(newWidth, newHeight);
				imageLabelSize.y = newHeight;
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageLabelSize.x);
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageLabelSize.y);
			}

			isDirty = false;
		}



		public Vector2 GetMinDimensions()
		{
			if (isDirty)
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