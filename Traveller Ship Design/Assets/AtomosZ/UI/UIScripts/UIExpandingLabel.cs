using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;

using static AtomosZ.UI.MagicWindowBase;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	[ExecuteInEditMode]
	public class UIExpandingLabel : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Text; } }
		[SerializeField] private UIExpandingLabelScriptableObject labelData;

		[SerializeField] private TextMeshProUGUI textLabel;

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				textLabel.color = value ? color : disabledColor;
			}
		}

		[SerializeField] private string _text = "Text Label";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get
			{
				if (textLabel == null)
					textLabel = GetComponent<TextMeshProUGUI>();
				return _text = textLabel.text;
			}
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

		public bool autoSizeFont
		{
			get { return textLabel.enableAutoSizing; }
			set
			{
				if (value == textLabel.enableAutoSizing)
					return;
				textLabel.enableAutoSizing = value;
				this.SetDirty();
			}
		}

		public float fontSizeMin
		{
			get { return textLabel.fontSizeMin; }
			set
			{
				if (textLabel.fontSizeMin == value)
					return;
				textLabel.fontSizeMin = value;
				this.SetDirty();
			}
		}

		public float fontSizeMax
		{
			get { return textLabel.fontSizeMax; }
			set
			{
				if (textLabel.fontSizeMax == value)
					return;
				textLabel.fontSizeMax = value;
				this.SetDirty();
			}
		}


		[Tooltip("A value of <= 0 will set the fontSize to the scriptable object value, if it exists")]
		public float fontSize
		{
			get { return textLabel.fontSize; }
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

				textLabel.fontSize = value;
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



		/// <summary>
		/// This may modify the min and max lable dimensions to accomodate the new width.
		/// Force the size of the label.
		/// Used on tab/titlebar.
		/// </summary>
		/// <param name="newWidth"></param>
		public void SetWidth(float newWidth)
		{
			var labelDims = minDimensions;
			if (labelDims.x > newWidth)
			{
				labelDims.x = newWidth;
				minDimensions = labelDims;
			}

			labelDims = maxDimensions;
			if (labelDims.x < newWidth)
			{
				labelDims.x = newWidth;
				maxDimensions = labelDims;
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
			var newDims = minDimensions;
			if (newDims.y > newHeight)
			{
				newDims.y = newHeight;
				minDimensions = newDims;
			}

			newDims = maxDimensions;
			if (newDims.y < newHeight)
			{
				newDims.y = newHeight;
				minDimensions = newDims;
			}

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
		}




		[Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			isDirty = true;
			referenceName = _referenceName;
			interactable = _interactable;
			text = _text;
			//color = _color;
			disabledColor = _disabledColor;
			//fontSize = _fontSize;
			fontAsset = _fontAsset;
			fontStyles = _fontStyles;
			alignmentOptions = _alignmentOptions;
			margin = _margin;
			//fillParentHorizontal = _fillParentHorizontal;
			//fillParentVertical = _fillParentVertical;
			minDimensions = _minDimensions;
			//maxDimensions = _maxDimensions;
			isDirty = false;
			this.SetDirty();
			//RecalculateDimensions();
		}

		[Conditional("UNITY_EDITOR")]
		public new void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(textLabel);
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}


		void Start()
		{
			this.SetDirty();
		}

		public void ReturnToPool()
		{
			autoSizeFont = false;
			fillParentHorizontal = false;
			fillParentVertical = false;
			color = Color.black;
			fontAsset = null;
			fontStyles = FontStyles.Normal;
			alignmentOptions = (TextAlignmentOptions)((int)VerticalAlignmentOptions.Top | (int)HorizontalAlignmentOptions.Left);

			pooledObject.ReturnToPool();
		}

		public ScriptableObject GetBackingData()
		{
			return labelData;
		}



		public void UpdateBackingData(ScriptableObject dataEx)
		{
			labelData = (UIExpandingLabelScriptableObject)dataEx;
			if (labelData != null)
			{
				color = labelData.fontColor;
				disabledColor = labelData.disabledColor;
				fontAsset = labelData.fontAsset;
				fontSize = labelData.fontSize;
				fontStyles = labelData.fontStyles;
				margin = labelData.textMargin;
			}

			this.SetDirty();
		}


		public override void RecalculateDimensions()
		{
			textLabel.ForceMeshUpdate(false, true);


			//if (fillParentHorizontal && fillParentVertical)
			//{
			//	var parent = transform.parent.GetComponent<UIMonoBehaviour>();
			//	rect.sizeDelta = parent.rect.sizeDelta;
			//	isDirty = false;
			//	return;
			//}

#if UNITY_EDITOR
			var prefTextSize = textLabel.GetPreferredValues(text);
#endif
			//var singleLineTextHeight = prefTextSize.y; // this is the height of a single line, assuming no linefeed
			//var singleLineTextWidth = prefTextSize.x; // this is the width if the text was on a single line

			preferredSize = textLabel.GetPreferredValues();   // this is values INCLUDING margins

			if (layoutElement.flexibleHeight > 0)
			{   // this is a horizontal panel so let's give some room to other controls if possible
				//if (preferredSize.x < rect.sizeDelta.x)
				{
					preferredSize.x = Mathf.Max(preferredSize.x, layoutElement.minWidth);
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
					textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
				}
			}
			else
			{
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
				textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
			}


			// there are occasions where the textlabel.rect is not the same gameobject as this.rect, so we need to set this "twice".
			// something to test is see if checking rect == textLabel.rectTransform is faster
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
			textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);

			isDirty = false;
			return;
			//var actualMaxWidth = maxDimensions.x - (margin.x + margin.z);
			//var actualMaxHeight = maxDimensions.y - (margin.y + margin.w);
			//var newWidth = Mathf.Max(5, minDimensions.x - (margin.x + margin.z));
			//var newHeight = Mathf.Max(5, minDimensions.y - (margin.y + margin.w));
			//if (singleLineTextWidth <= actualMaxWidth)
			//{
			//	newWidth = Mathf.Max(newWidth, singleLineTextWidth);
			//	newHeight = Mathf.Max(newHeight, singleLineTextHeight);
			//	newHeight = Mathf.Min(newHeight, actualMaxHeight);
			//}
			//else
			//{
			//	//var prefTextWidth = textLabel.preferredWidth;
			//	//var prefTextHeight = textLabel.preferredHeight;

			//	//var renderedValues = textLabel.GetRenderedValues();	// this is values WITHOUT margins
			//	var preferredValues = textLabel.GetPreferredValues();   // this is values INCLUDING margins

			//	newWidth = Mathf.Min(preferredValues.x, actualMaxWidth);

			//	textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

			//	textLabel.ForceMeshUpdate();
			//	Canvas.ForceUpdateCanvases();


			//	var prefValues = textLabel.GetPreferredValues(textLabel.text, actualMaxWidth, minDimensions.y);
			//	newHeight = Mathf.Max(preferredValues.y, prefValues.y);
			//	newHeight = Mathf.Min(newHeight, actualMaxHeight);

			//	if (newHeight <= singleLineTextHeight)
			//	{
			//		Debug.LogWarning("If this happening, then container is probably too small for the text. If that's not the case....PANIC!");
			//	}
			//}

			//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			//textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);


			//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);
			//textLabel.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);


			//isDirty = false;
		}



		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();

			return textLabel.rectTransform.sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		/// <summary>
		/// Size of text INCLUDING margins.
		/// </summary>
		/// <returns></returns>
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}
	}
}