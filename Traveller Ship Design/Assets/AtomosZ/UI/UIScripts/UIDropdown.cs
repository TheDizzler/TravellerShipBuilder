using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UIDropdown : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Dropdown; } }

		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private Image arrow;
		[SerializeField] private UIDropdownScriptableObject dropdownData;

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = dropdown.interactable = value;
			}
		}

		[SerializeField] private bool _fillParentHorizontal = false;
		public bool fillParentHorizontal
		{
			get { return _fillParentHorizontal; }
			set
			{
				_fillParentHorizontal = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Vector2 _minDimensions = new Vector2(64, 10);
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Sprite _arrowSprite;
		public Sprite arrowSprite
		{
			get { return _arrowSprite = arrow.sprite; }
			set
			{
				_arrowSprite = arrow.sprite = value;
				this.SetDirty();
			}
		}

		[SerializeField] private TMP_FontAsset _fontAsset;
		public TMP_FontAsset fontAsset
		{
			get { return _fontAsset = dropdown.captionText.font; }
			set
			{
				_fontAsset = dropdown.captionText.font = value;
				var allText = GetComponentsInChildren<TMP_Text>();
				foreach (var text in allText)
					text.font = value;
				this.SetDirty();
			}
		}

		[SerializeField] private float _fontSize = 18;
		public float fontSize
		{
			get { return _fontSize = dropdown.captionText.fontSize; }
			set
			{
				_fontSize = dropdown.captionText.fontSize = value;
				var allText = GetComponentsInChildren<TMP_Text>();
				foreach (var text in allText)
					text.fontSize = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Color _fontColor;
		public Color fontColor
		{
			get { return _fontColor = dropdown.captionText.color; }
			set
			{
				_fontColor = dropdown.captionText.color = value;
				var allText = GetComponentsInChildren<TMP_Text>();
				foreach (var text in allText)
					text.color = value;
			}
		}

		[SerializeField] private TextAlignmentOptions _alignmentOptions;
		public TextAlignmentOptions alignmentOptions
		{
			get
			{
				var val = _alignmentOptions = (TextAlignmentOptions)(
					(int)dropdown.captionText.verticalAlignment | (int)dropdown.captionText.horizontalAlignment);
				if (val == 0)
				{
					_alignmentOptions = TextAlignmentOptions.TopLeft;
					dropdown.captionText.verticalAlignment = VerticalAlignmentOptions.Top;
					dropdown.captionText.horizontalAlignment = HorizontalAlignmentOptions.Left;
					foreach (var text in GetComponentsInChildren<TMP_Text>())
					{
						text.verticalAlignment = VerticalAlignmentOptions.Top;
						text.horizontalAlignment = HorizontalAlignmentOptions.Left;
					}
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

				dropdown.captionText.verticalAlignment = vert;

				var horz = (HorizontalAlignmentOptions)(value ^ (TextAlignmentOptions)vert);
				dropdown.captionText.horizontalAlignment = horz;

				foreach (var text in GetComponentsInChildren<TMP_Text>())
				{
					text.verticalAlignment = vert;
					text.horizontalAlignment = horz;
				}

				this.SetDirty();
			}
		}

		//[SerializeField] private Vector4 _margin;
		//[Tooltip("An input of Vector4.positiveInfinity will set the margin to the scriptable object value, if it exists.")]
		//public Vector4 margin
		//{
		//	get { return _margin = textLabel.margin; }
		//	set
		//	{
		//		if (margin == value)
		//			return;
		//		if (value == Vector4.positiveInfinity)
		//		{
		//			if (labelData != null)
		//				_margin = textLabel.margin = labelData.textMargin;
		//		}
		//		else
		//			_margin = textLabel.margin = value;
		//		this.SetDirty();
		//	}
		//}


		public UnityEvent<UIDropdown, int> onValueChangedAction = null;

		[SerializeField] private int _value;
		public int value
		{
			get { return _value; }
			set
			{
				var oldValue = _value;
				dropdown.value = _value = value;
				if (oldValue != value)
				{
					this.SetDirty();
					if (onValueChangedAction != null)
						onValueChangedAction.Invoke(this, value);
				}
			}
		}

		[SerializeField] private bool _isMultiSelect = false;
		public bool isMultiSelect
		{
			get { return _isMultiSelect; }
			set
			{
				dropdown.MultiSelect = _isMultiSelect = value;
				// do we need to clamp a multiselect value to a single select value?
			}
		}

		[SerializeField]
		private List<TMP_Dropdown.OptionData> _options;

		public List<TMP_Dropdown.OptionData> options
		{
			get { return _options = dropdown.options; }
			set
			{
				_options = dropdown.options = value;
				this.SetDirty();
			}
		}

		[SerializeField] private UnityEvent<UIDropdown> _optionsDelegate = null;

		[Tooltip("A delegate to auto-populate the options list.")]
		public UnityEvent<UIDropdown> optionsDelegate
		{
			get { return _optionsDelegate; }
			set
			{
				_optionsDelegate = value;
				if (optionsDelegate != null)
					_optionsDelegate.Invoke(this);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void UpdateOptionsDelegate()
		{
			optionsDelegate = _optionsDelegate;
		}

		public void AddOption(TMP_Dropdown.OptionData option)
		{
			dropdown.AddOptions(new List<TMP_Dropdown.OptionData> { option });
		}

		public void AddOptions(List<TMP_Dropdown.OptionData> options)
		{
			dropdown.AddOptions(options);
		}

		public void ClearOption()
		{
			dropdown.ClearOptions();
		}



		public int SelectedIndex()
		{
			return value;
		}

		public string SelectedValue()
		{
			return _options[value].text;
		}


		void OnEnable()
		{
			//dropdown.onValueChanged.RemoveAllListeners();
			//dropdown.onValueChanged.AddListener(OnValueChanged);
			if (optionsDelegate != null)
				optionsDelegate.Invoke(this);
			//else
			//	dropdown.ClearOptions();

			this.SetDirty();
		}

		public void OnValueChanged(int newValue)
		{
			if (onValueChangedAction != null)
				onValueChangedAction.Invoke(this, value);
		}



		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public ScriptableObject GetBackingData()
		{
			return dropdownData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			dropdownData = (UIDropdownScriptableObject)backingData;
			if (dropdownData != null)
			{
				arrowSprite = dropdownData.arrowSprite;
				if (dropdownData.labelData != null)
				{
					fontAsset = dropdownData.labelData.fontAsset;
					fontColor = dropdownData.labelData.fontColor;
					fontSize = dropdownData.labelData.fontSize;
				}

				this.SetDirty();
			}
		}

		public void UpdateBackingData()
		{
			var layout = GetComponent<LayoutElement>();
			if (fillParentHorizontal)
				layout.flexibleWidth = 1;
			else
				layout.flexibleWidth = -1;

			float arrowWidth = 0;
			if (arrow != null)
				arrowWidth = arrow.rectTransform.sizeDelta.x;
			var minWidth = minDimensions.x;
			var textLabel = dropdown.captionText;
			textLabel.ForceMeshUpdate(false, true);
			var labelOffset = textLabel.margin.x + textLabel.margin.z + arrowWidth - textLabel.rectTransform.offsetMax.x;
			foreach (var option in options)
			{
				var dimens = dropdown.captionText.GetPreferredValues(option.text);
				minWidth = Mathf.Max(minWidth, dimens.x + labelOffset);
			}

			layout.minWidth = minWidth;
			layout.minHeight = minDimensions.y;

			isDirty = false;
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			var sDelta = GetComponent<RectTransform>().sizeDelta;
			return sDelta;
		}
	}
}