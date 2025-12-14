using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using FontStyles = TMPro.FontStyles;

namespace AtomosZ.UI
{
	[Serializable]
	public class CheckBoxEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.CheckBox; } }

		public UICheckBoxScriptableObject scriptableObj;


		public CheckBoxEx(UICheckBoxScriptableObject scriptObj)
		{
			this.scriptableObj = scriptObj;
		}
	}

	[ExecuteAlways]
	public class UICheckBox : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.CheckBox; } }

		[SerializeField] private UICheckBoxScriptableObject checkBoxData;
		[SerializeField] private RectTransform backgroundRect;
		[SerializeField] private Image boxImage;
		[SerializeField] private Image checkImage;

		public UIExpandingLabel textLabel;
		public Toggle toggle;

		[SerializeField] private string _referenceName = "checkbox";
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

		[SerializeField] private bool _interactable = true;
		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = toggle.interactable = value;
				textLabel.interactable = value;
				if (value)
				{
					checkImage.color = toggle.colors.normalColor;
				}
				else
				{
					checkImage.color = toggle.colors.disabledColor;
				}
			}
		}


		[SerializeField] private string _text = "CheckBox";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get { return _text; }
			set
			{
				_text = textLabel.text = value;
				// textLabel will set this to dirty if needed.
			}
		}

		[SerializeField] private Color _fontColor;
		[Tooltip("A value of Color.clear will set the font color to the scriptable object value, if it exists.")]
		public Color fontColor
		{
			get { return _fontColor = textLabel.color; }
			set { _fontColor = textLabel.color = value; }
		}

		[SerializeField] private Color _disabledFontColor;
		[Tooltip("A value of Color.clear will set the font color to the scriptable object value, if it exists.")]
		public Color disabledFontColor
		{
			get { return _disabledFontColor = textLabel.disabledColor; }
			set { _disabledFontColor = textLabel.disabledColor = value; }
		}

		[SerializeField] private FontStyles _fontStyles;
		public FontStyles fontStyles
		{
			get { return _fontStyles = textLabel.fontStyles; }
			set { _fontStyles = textLabel.fontStyles = value; }
		}

		[SerializeField] private TextAlignmentOptions _alignmentOptions;
		public TextAlignmentOptions alignmentOptions
		{
			get { return _alignmentOptions = textLabel.alignmentOptions; }
			set { _alignmentOptions = textLabel.alignmentOptions = value; }
		}


		[SerializeField] private Vector4 _margin;
		public Vector4 margin
		{
			get { return _margin = textLabel.margin; }
			set
			{
				_margin = textLabel.margin = value;
				// textLabel will set this to dirty if needed.
			}
		}


		[SerializeField] private Vector2 _minLabelDimensions = new Vector2(64, 10);
		public Vector2 minLabelDimensions
		{
			get { return _minLabelDimensions = textLabel.minLabelDimensions; }
			set
			{
				_minLabelDimensions = textLabel.minLabelDimensions = value;
				// textLabel will set this to dirty if needed.
			}
		}


		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		[SerializeField] private Vector2 _maxLabelDimensions = new Vector2(1025, 256);
		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		public Vector2 maxLabelDimensions
		{
			get { return _maxLabelDimensions = textLabel.maxLabelDimensions; }
			set
			{
				_maxLabelDimensions = textLabel.maxLabelDimensions = value;
				// textLabel will set this to dirty if needed.
			}
		}

		public UnityEvent<UICheckBox, bool> onCheckChangedEvent;

		public void AddListener(UnityAction<UICheckBox, bool> onChangedEvent)
		{
			onCheckChangedEvent.AddListener(onChangedEvent);
		}


		private void OnToggled(bool isToggled)
		{
			if (onCheckChangedEvent != null)
				onCheckChangedEvent.Invoke(this, toggle.isOn);
		}

		void OnEnable()
		{
			toggle.onValueChanged.RemoveAllListeners();
			toggle.onValueChanged.AddListener(OnToggled);

			this.SetDirty();
		}

		[SerializeField] private Sprite _boxSprite;
		[Tooltip("A null value will set the sprite to the scriptable object value, if it exists.")]
		public Sprite boxSprite
		{
			get { return _boxSprite = boxImage.sprite; }
			set
			{
				if (value == null)
				{
					if (checkBoxData != null)
						value = checkBoxData.boxSprite;
				}

				_boxSprite = boxImage.sprite = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Sprite _checkSprite;
		[Tooltip("A null value will set the sprite to the scriptable object value, if it exists.")]
		public Sprite checkSprite
		{
			get { return _checkSprite = checkImage.sprite; }
			set
			{
				if (value == null)
				{
					if (checkBoxData != null)
						value = checkBoxData.checkSprite;
				}

				_checkSprite = checkImage.sprite = value;
				this.SetDirty();
			}
		}


		public IUIDataEx GetBackingData()
		{
			return new CheckBoxEx(checkBoxData);
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void UpdateBackingData(UICheckBoxScriptableObject backingData)
		{
			checkBoxData = backingData;
			if (backingData != null)
			{
				textLabel.UpdateBackingData(backingData.labelData);
				if (backingData.labelData != null)
				{
					fontColor = backingData.labelData.fontColor;
					disabledFontColor = backingData.labelData.disabledColor;
					margin = backingData.labelData.textMargin;
				}

				boxSprite = backingData.boxSprite;
				checkSprite = backingData.checkSprite;
				this.SetDirty();
			}
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			UpdateBackingData(((CheckBoxEx)backingData).scriptableObj);
		}

		public void UpdateBackingData()
		{
			var minDim = textLabel.GetMinDimensions();
			var layout = GetComponent<HorizontalLayoutGroup>();
			var space = layout.spacing;
			var imageDim = backgroundRect.sizeDelta;
			minDim.x += imageDim.x + space;
			if (minDim.y < imageDim.y)
				minDim.y = imageDim.y;
			var rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minDim.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);

			isDirty = false;
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			return GetComponent<RectTransform>().sizeDelta;
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
			throw new System.NotImplementedException();
		}

		public void UpdateHover(Vector3 posOfHover)
		{
			throw new System.NotImplementedException();
		}
	}
}