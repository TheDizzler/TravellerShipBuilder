using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;

using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	/// <summary>
	/// THis is ready to be removed completely
	/// </summary>
	[Serializable]
	public class LabelEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Text; } }

		public UIExpandingLabelScriptableObject scriptableObj;



		public LabelEx(UIExpandingLabelScriptableObject textScriptObj)
		{
			this.scriptableObj = textScriptObj;

		}
	}

	[ExecuteAlways]
	public class UIExpandingLabel : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Text; } }
		[SerializeField] private UIExpandingLabelScriptableObject labelData;

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

		[SerializeField] private bool _interactable = true;
		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;

				if (labelData != null)
					textLabel.color = value ? labelData.fontColor : labelData.disabledColor;
				else
					textLabel.color = value ? color : disabledColor;
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
				_text = textLabel.text = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Color _color;
		[Tooltip("A value of Color.clear will set the font color to the scriptable object value, if it exists.")]
		public Color color
		{
			get { return _color; }
			set
			{
				if (value == Color.clear)
				{
					if (labelData != null)
						_color = labelData.fontColor;
				}
				else
					_color = value;

				if (interactable)
					textLabel.color = _color;
			}
		}
		[SerializeField] private Color _disabledColor;
		[Tooltip("A value of Color.clear will set the font color to the scriptable object value, if it exists.")]
		public Color disabledColor
		{
			get { return _disabledColor; }
			set
			{
				if (value == Color.clear)
				{
					if (labelData != null)
						_disabledColor = labelData.disabledColor;
				}
				else
					_disabledColor = value;

				if (!interactable)
					textLabel.color = _disabledColor;
			}
		}


		[SerializeField] private float _fontSize;
		[Tooltip("A value of <= 0 will set the fontSize to the scriptable object value, if it exists")]
		public float fontSize
		{
			get { return _fontSize = textLabel.fontSize; }
			set
			{
				if (value < 1)
				{
					if (labelData == null)
						value = 1;
					else
						value = labelData.fontSize;
				}

				if (value == fontSize)
					return;

				_fontSize = textLabel.fontSize = value;
				this.SetDirty();
			}
		}

		[SerializeField] private TMP_FontAsset _fontAsset;
		[Tooltip("A null value will set the font to the scriptable object value, if it exists, or the default game font.")]
		public TMP_FontAsset fontAsset
		{
			get { return _fontAsset = textLabel.font; }
			set
			{
				if (value == null)
				{
					if (labelData != null)
						value = labelData.fontAsset;
				}

				if (value == textLabel.font)
					return;

				_fontAsset = textLabel.font = value;

				this.SetDirty();
			}
		}

		[SerializeField] private FontStyles _fontStyles;
		public FontStyles fontStyles
		{
			get { return _fontStyles = textLabel.fontStyle; }
			set
			{
				if (textLabel.fontStyle == value)
					return;
				_fontStyles = textLabel.fontStyle = value;

				this.SetDirty();
			}
		}

		[SerializeField] private TextAlignmentOptions _alignmentOptions;
		public TextAlignmentOptions alignmentOptions
		{
			get
			{
				var val = _alignmentOptions = (TextAlignmentOptions)((int)textLabel.verticalAlignment | (int)textLabel.horizontalAlignment);
				if (val == 0)
				{
					textLabel.verticalAlignment = VerticalAlignmentOptions.Top;
					textLabel.horizontalAlignment = HorizontalAlignmentOptions.Left;
					_alignmentOptions = TextAlignmentOptions.TopLeft;
				}

				return val;
			}
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
		[Tooltip("An input of Vector4.positiveInfinity will set the margin to the scriptable object value, if it exists.")]
		public Vector4 margin
		{
			get { return _margin = textLabel.margin; }
			set
			{
				if (margin == value)
					return;
				if (value == Vector4.positiveInfinity)
				{
					if (labelData != null)
						_margin = textLabel.margin = labelData.textMargin;
				}
				else
					_margin = textLabel.margin = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Vector2 _minLabelDimensions = new Vector2(64, 10);
		public Vector2 minLabelDimensions
		{
			get { return _minLabelDimensions; }
			set
			{
				if (value.x > maxLabelDimensions.x)
					value.x = maxLabelDimensions.x;
				if (value.y > maxLabelDimensions.y)
					value.y = maxLabelDimensions.y;
				if (value.x < 5)
					value.x = 5;
				if (value.y < 5)
					value.y = 5;
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
				if (value.x < minLabelDimensions.x)
					value.x = minLabelDimensions.x;
				if (value.y < minLabelDimensions.y)
					value.y = minLabelDimensions.y;
				if (value.x < 5)
					value.x = 5;
				if (value.y < 5)
					value.y = 5;
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

		[Conditional("UNITY_EDITOR")]
		public void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(textLabel);
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
			return new LabelEx(labelData);
		}



		public void UpdateBackingData(UIExpandingLabelScriptableObject dataEx)
		{
			labelData = dataEx;
			if (labelData != null)
			{
				color = labelData.fontColor;
				disabledColor = labelData.disabledColor;
				fontAsset = labelData.fontAsset;
				fontSize = labelData.fontSize;
				fontStyles = labelData.fontStyles;
				margin = labelData.textMargin;
				this.SetDirty();
			}
		}

		public void UpdateBackingData(IUIDataEx dataEx)
		{
			UpdateBackingData(((LabelEx)dataEx).scriptableObj);
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}


		public void UpdateBackingData()
		{
			textLabel.ForceMeshUpdate(false, true);

			//if (name == "UIExpandingText (TMP)")
			//	name = name;

			var rect = transform.GetComponent<RectTransform>();

			var prefTextSize = textLabel.GetPreferredValues(text);
			var singleLineTextHeight = prefTextSize.y; // this is the height of a single line, assuming no linefeed
			var singleLineTextWidth = prefTextSize.x; // this is the width if the text was on a single line

			var actualMaxWidth = maxLabelDimensions.x - (margin.x + margin.z);
			var actualMaxHeight = maxLabelDimensions.y - (margin.y + margin.w);
			var newWidth = Mathf.Max(5, minLabelDimensions.x - (margin.x + margin.z));
			var newHeight = Mathf.Max(5, minLabelDimensions.y - (margin.y + margin.w));
			if (singleLineTextWidth <= actualMaxWidth)
			{
				newWidth = Mathf.Max(newWidth, singleLineTextWidth);
				newHeight = Mathf.Max(newHeight, singleLineTextHeight);
				newHeight = Mathf.Min(newHeight, actualMaxHeight);
			}
			else
			{
				//var prefTextWidth = textLabel.preferredWidth;
				//var prefTextHeight = textLabel.preferredHeight;

				//var renderedValues = textLabel.GetRenderedValues();	// this is values WITHOUT margins
				var preferredValues = textLabel.GetPreferredValues();   // this is values INCLUDING margins

				newWidth = Mathf.Min(preferredValues.x, actualMaxWidth);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
				textLabel.ForceMeshUpdate();
				Canvas.ForceUpdateCanvases();


				var prefValues = textLabel.GetPreferredValues(textLabel.text, actualMaxWidth, 0);
				newHeight = Mathf.Max(preferredValues.y, prefValues.y);
				newHeight = Mathf.Min(newHeight, actualMaxHeight);

				if (newHeight <= singleLineTextHeight)
				{
					//int nextCharIndex = 0;
					//bool moreChars = true;
					//string growString = textLabel.text[nextCharIndex] + "";
					//while (moreChars)
					//{
					//	var growStringValues = textLabel.GetPreferredValues(growString, actualMaxWidth, 0);
					//	while (growStringValues.x < actualMaxWidth)
					//	{
					//		if (nextCharIndex >= textLabel.text.Length - 1)
					//		{
					//			break;
					//		}

					//		growString += textLabel.text[++nextCharIndex] + "";
					//		growStringValues = textLabel.GetPreferredValues(growString, actualMaxWidth, 0);
					//	}

					//	if (nextCharIndex >= textLabel.text.Length - 1)
					//		break;
					//	growString = growString.Insert(nextCharIndex, System.Environment.NewLine);

					//	if (nextCharIndex > 100000)
					//		break;

					//}

					//text = growString;


					Debug.LogWarning("what the hell???");
				}

				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			}


			if (image != null)
			{
				var imageLabelSize = new Vector2(newWidth, newHeight);
				imageLabelSize.y = newHeight;
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageLabelSize.x);
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageLabelSize.y);
			}

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			isDirty = false;
		}



		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();

			return transform.GetComponent<RectTransform>().sizeDelta;
			//if (image == null)
			//{
			//	var size = textLabel.rectTransform.sizeDelta;
			//	size.x += textLabel.margin.x + textLabel.margin.z;
			//	size.y += textLabel.margin.y + textLabel.margin.w;
			//	return size;
			//}
			//else
			//{
			//	return image.rectTransform.sizeDelta;
			//}
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