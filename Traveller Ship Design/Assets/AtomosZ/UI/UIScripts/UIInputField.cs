using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;


namespace AtomosZ.UI
{
	/// <summary>
	/// @NOTE(Tristan): The built-in TMP Input Field is shit. It will not respect manual resizing and the single/multiline handling
	/// is buggy at best. Like the Slider, a new one from scratch will need to be created if it is to be useful at all.
	/// For now a ContentSizeFitter seems to be doing the trick, despite the warning.<br/>
	/// @TODO(Tristan): Text still gets screwy when the max dimensions are exceeded.
	/// Should create scrollbar.
	/// </summary>
	[ExecuteInEditMode]
	public class UIInputField : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.InputField; } }

		[SerializeField] private UIExpandingInputFieldScriptableObject inputFieldData;

		[SerializeField] private TextMeshProUGUI placeholderLabel;
		[SerializeField] private UIExpandingLabel textLabel;

		[SerializeField] private TMP_InputField inputFieldTMP;
		[SerializeField] private RectTransform textAreaRect;
		[SerializeField] private Image image;

		public bool interactable
		{
			get { return _interactable = inputFieldTMP.interactable; }
			set
			{
				_interactable = inputFieldTMP.interactable = value;

			}
		}

		[SerializeField] private TMP_FontAsset _fontAsset;
		[Tooltip("A null value will set the font to the scriptable object value, if it exists, or the default game font.")]
		public TMP_FontAsset fontAsset
		{
			get { return _fontAsset = inputFieldTMP.fontAsset; }
			set
			{
				if (value == null)
				{
					if (inputFieldData != null)
						value = inputFieldData.fontAsset;
				}

				if (value == _fontAsset)
					return;

				_fontAsset = inputFieldTMP.fontAsset = value;
				this.SetDirty();
			}
		}

		[SerializeField] private float _fontSize;
		[Tooltip("A value of <= 0 will set the fontSize to the scriptable object value, if it exists")]
		public float fontSize
		{
			get { return _fontSize = inputFieldTMP.pointSize; }
			set
			{
				if (value < 1)
				{
					if (inputFieldData == null)
						value = 1;
					else
						value = inputFieldData.fontSize;
				}

				if (value == fontSize)
					return;

				_fontSize = inputFieldTMP.pointSize = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Color _placeholderFontColor;
		[Tooltip("A value of Color.clear will set the font color to the scriptable object value, if it exists.")]
		public Color placeholderFontColor
		{
			get { return _placeholderFontColor = placeholderLabel.color; }
			set
			{
				if (value == Color.clear)
				{
					if (inputFieldData != null)
						_placeholderFontColor = placeholderLabel.color = inputFieldData.placeholderFontColor;
				}
				else
					_placeholderFontColor = placeholderLabel.color = value;
			}
		}


		[SerializeField] private Color _fontColor;
		[Tooltip("A value of Color.clear will set the font color to the scriptable object value, if it exists.")]
		public Color fontColor
		{
			get { return _fontColor = textLabel.color; }
			set
			{
				if (value == Color.clear)
				{
					if (inputFieldData != null)
						_fontColor = textLabel.color = inputFieldData.fontColor;
				}
				else
					_fontColor = textLabel.color = value;
			}
		}

		[SerializeField] private TextAlignmentOptions _alignmentOptions;
		public TextAlignmentOptions alignmentOptions
		{
			get
			{
				var val = _alignmentOptions = (TextAlignmentOptions)((int)placeholderLabel.verticalAlignment | (int)placeholderLabel.horizontalAlignment);
				if (val == 0)
				{
					textLabel.alignmentOptions = (TextAlignmentOptions)((int)VerticalAlignmentOptions.Top | (int)HorizontalAlignmentOptions.Left);
					placeholderLabel.verticalAlignment = VerticalAlignmentOptions.Top;
					placeholderLabel.horizontalAlignment = HorizontalAlignmentOptions.Left;
					alignmentOptions = _alignmentOptions = TextAlignmentOptions.TopLeft;
				}

				return val;
			}
			set
			{
				if (alignmentOptions == value)
					return;
				_alignmentOptions = value;
				textLabel.alignmentOptions = value;
				var vert = (VerticalAlignmentOptions)(value
					& (TextAlignmentOptions)(VerticalAlignmentOptions.Baseline | VerticalAlignmentOptions.Bottom
					| VerticalAlignmentOptions.Capline | VerticalAlignmentOptions.Geometry
					| VerticalAlignmentOptions.Middle | VerticalAlignmentOptions.Top));
				placeholderLabel.verticalAlignment = vert;

				var horz = (HorizontalAlignmentOptions)(value ^ (TextAlignmentOptions)vert);
				placeholderLabel.horizontalAlignment = horz;
			}
		}



		[SerializeField] private string _text;
		public string text
		{
			get { return _text = inputFieldTMP.text; }
			set
			{
				if (inputFieldTMP.text == value)
					return;
				_text = textLabel.text = inputFieldTMP.text = value;
				this.SetDirty();
			}
		}

		[SerializeField] private string _placeholderText;
		public string placeholderText
		{
			get { return _placeholderText = placeholderLabel.text; }
			set
			{
				if (placeholderLabel.text == value)
					return;
				_placeholderText = placeholderLabel.text = value;
				this.SetDirty();
			}
		}

		public float horizontalTextAreaOffsets
		{
			get { return Mathf.Abs(textAreaRect.offsetMin.x) + Mathf.Abs(textAreaRect.offsetMax.x); }
		}

		public float verticalTextAreaOffsets
		{
			get { return Mathf.Abs(textAreaRect.offsetMin.y) + Mathf.Abs(textAreaRect.offsetMax.y); }
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}


		public void SetPlaceholderText(string placeholderText)
		{
			placeholderLabel.text = placeholderText;
		}

		public void SetText(string newText)
		{
			inputFieldTMP.text = newText;
		}

		public void SetText(string newText, string placeholderText)
		{
			inputFieldTMP.text = newText;
			placeholderLabel.text = placeholderText;
		}


		void Start()
		{
			if (textLabel == null)
				textLabel = GetComponentInChildren<UIExpandingLabel>();
			this.SetDirty();
		}


		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			referenceName = _referenceName;
			interactable = _interactable;
			placeholderText = _placeholderText;
			text = _text;
			fontColor = _fontColor;
			placeholderFontColor = _placeholderFontColor;
			fontSize = _fontSize;

			if (Helpers.IsPrefabStage_EDITOR() && transform.parent.name == "Canvas (Environment)")
				RecalculateDimensions();
			else
				this.SetDirty();
		}

		public ScriptableObject GetBackingData()
		{
			return inputFieldData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			inputFieldData = (UIExpandingInputFieldScriptableObject)backingData;
			if (backingData != null)
			{
				fontAsset = inputFieldData.fontAsset;
				fontColor = inputFieldData.fontColor;
				placeholderFontColor = inputFieldData.placeholderFontColor;
				fontSize = inputFieldData.fontSize;
			}

			this.SetDirty();
		}


		public override void RecalculateDimensions()
		{
			placeholderLabel.ForceMeshUpdate();
			textLabel.RecalculateDimensions();

			var childMax = _maxDimensions;
			childMax.x -= verticalTextAreaOffsets;
			textLabel.maxDimensions = childMax;

			var prefTextSize = placeholderLabel.GetPreferredValues("ABCDEFGHIJKLMNOPQRSTUVWXYZ"); // Text to measure font height
			var textHeight = prefTextSize.y + verticalTextAreaOffsets; // this should be the preferred height of a single line, right?

			var fieldDimensions = minDimensions;
			fieldDimensions.x = Mathf.Max(fieldDimensions.x, textHeight);
			fieldDimensions.y = Mathf.Max(fieldDimensions.y, textHeight);

			var labelSize = textLabel.rect.rect;
			//var label = textLabel.GetComponent<TextMeshProUGUI>();
			//var prefSize = label.GetPreferredValues();
			labelSize.width += horizontalTextAreaOffsets;
			labelSize.height += verticalTextAreaOffsets;

			fieldDimensions.x = Mathf.Max(fieldDimensions.x, labelSize.width);
			fieldDimensions.y = Mathf.Max(fieldDimensions.y, labelSize.height);

			var maxWidth = Mathf.Min(fieldDimensions.x, maxDimensions.x);
			//layoutElement.minWidth = maxWidth;
			layoutElement.minHeight = fieldDimensions.y;

			//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fieldDimensions.y);

			preferredSize = rect.sizeDelta;

			isDirty = false;
		}


		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}
	}
}