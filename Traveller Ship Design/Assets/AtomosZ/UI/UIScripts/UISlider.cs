using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	/// <summary>
	/// @TODO(Tristan): maxSigFigs
	/// </summary>
	[Serializable]
	public class SliderEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Slider; } }
		public UISliderScriptableObject sliderData;

		public SliderEx(UISliderScriptableObject sliderData)
		{
			this.sliderData = sliderData;
		}
	}

	[ExecuteAlways]
	public class UISlider : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Slider; } }

		[SerializeField] private UISliderScriptableObject sliderData;
		[SerializeField] private RectTransform rectTransform;
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

		private SliderCustom slider
		{
			get { return GetComponent<SliderCustom>(); }
		}

		[SerializeField] private bool _interactable = true;
		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				var images = GetComponentsInChildren<Image>();
				foreach (var image in images)
				{
					if (value)
						image.color = Color.white;
					else
						image.color = new Color(.6f, .6f, .6f);
				}
			}
		}



		[SerializeField] private bool _showUnits = true;
		public bool showUnits
		{
			get { return _showUnits; }
			set
			{
				_showUnits = value;
				slider.ShowUnits(value);
			}
		}


		[SerializeField] private bool _wholeNumbers = true;
		public bool wholeNumbers
		{
			get { return _wholeNumbers; }
			set
			{
				if (_wholeNumbers == value)
					return;
				_wholeNumbers = value;
				if (_wholeNumbers)
				{
					minValue = Mathf.RoundToInt(_minValue);
					maxValue = Mathf.RoundToInt(_maxValue);
					unitSpan = Mathf.RoundToInt(_unitSpan);
					this.value = Mathf.RoundToInt(_value);
				}

				this.SetDirty();
			}
		}


		[SerializeField] private float _minValue = float.MinValue;
		public float minValue
		{
			get { return _minValue; }
			set
			{
				if (_wholeNumbers)
					value = Mathf.RoundToInt(value);
				if (value > maxValue)
					_minValue = maxValue;
				else
					_minValue = value;

				this.value = _value;
				if (showUnits)
					slider.CreateUnitLabels();

				this.SetDirty();
			}
		}

		[SerializeField] private float _maxValue = float.MaxValue;
		public float maxValue
		{
			get { return _maxValue; }
			set
			{
				if (_wholeNumbers)
					value = Mathf.RoundToInt(value);
				if (value < minValue)
					_maxValue = minValue;
				else
					_maxValue = value;

				this.value = this.value;
				if (showUnits)
					slider.CreateUnitLabels();

				this.SetDirty();
			}
		}

		[SerializeField] private float _value;
		public float value
		{
			get { return _value; }
			set
			{
				float oldValue = _value;
				if (_wholeNumbers)
					value = Mathf.RoundToInt(value);
				_value = value;
				if (_value > maxValue)
					_value = maxValue;
				else if (_value < minValue)
					_value = minValue;
				slider.SetValueFill();

				if (onValueChanged != null && oldValue != _value)
					onValueChanged.Invoke(this, _value);
			}
		}

		[SerializeField] private float _fontSize = 18;
		public float fontSize
		{
			get { return _fontSize; }
			set
			{
				_fontSize = value;
				slider.SetFontSize(value);
				this.SetDirty();
			}
		}

		[SerializeField] private Color _fontColor;
		public Color fontColor
		{
			get { return _fontColor; }
			set
			{
				_fontColor = value;
				slider.SetFontColor(value);
			}
		}

		[SerializeField] private float _unitVerticalOffset = 0;
		public float unitVerticalOffset
		{
			get { return _unitVerticalOffset; }
			set
			{
				_unitVerticalOffset = value;
				this.SetDirty();
			}
		}

		[Min(0)]
		[SerializeField] private float _unitSpan;
		public float unitSpan
		{
			get { return _unitSpan; }
			set
			{
				float range = _maxValue - minValue;
				if (_wholeNumbers)
				{
					value = Mathf.RoundToInt(value);
					if (value < 0)
						value = 0;
				}
				else if (value < 0 || value < range / 10)
				{
					value = range / 10;
				}

				if (value > range / 2)
					value = range / 2;

				if (_wholeNumbers)
					value = Mathf.RoundToInt(value);

				if (_unitSpan == value)
					return;
				_unitSpan = value;

				slider.CreateUnitLabels();

				this.SetDirty();
			}
		}

		[SerializeField] private UIExpandingLabelScriptableObject _labelData;
		public UIExpandingLabelScriptableObject labelData
		{
			get { return _labelData; }
			set
			{
				if (value == null)
				{
					if (sliderData != null)
						labelData = sliderData.labelData;
				}
				else
					_labelData = value;

				this.SetDirty();
			}
		}


		[SerializeField] private bool _showHandle = true;
		public bool showHandle
		{
			get { return _showHandle; }
			set
			{
				_showHandle = value;
				slider.ShowHandle(value);
				this.SetDirty();
			}
		}

		[SerializeField] private Sprite _handleSprite;
		[Tooltip("A value of null will set the sprite to the ScriptableObject value, if it exists")]
		public Sprite handleSprite
		{
			get { return _handleSprite = GetComponent<SliderCustom>().handleSprite; }
			set
			{
				if (value == null)
				{
					if (sliderData != null)
						value = sliderData.handleSprite;
				}

				_handleSprite = value;
				var slider = GetComponent<SliderCustom>().handleSprite = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Vector2 _handleOffset = new Vector2(16, 16);
		public Vector2 handleOffset
		{
			get { return _handleOffset; }
			set
			{
				_handleOffset = value;
				this.SetDirty();
			}
		}


		[SerializeField] private Vector2 _minDimensions = new Vector2(128, 32);
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				var layout = GetComponent<LayoutElement>();
				layout.minWidth = minDimensions.x;
				layout.minHeight = minDimensions.y;
				this.SetDirty();
			}
		}

		[SerializeField] private Vector2 _maxDimensions;
		public Vector2 maxDimensions
		{
			get { return _maxDimensions; }
			set
			{
				_maxDimensions = value;
				this.SetDirty();
			}
		}


		[SerializeField] private bool _fillParentHorizontal;
		public bool fillParentHorizontal
		{
			get { return _fillParentHorizontal; }
			set
			{
				_fillParentHorizontal = value;
				var layout = GetComponent<LayoutElement>();
				if (_fillParentHorizontal)
					layout.flexibleWidth = 1;
				else
					layout.flexibleWidth = 0;
				this.SetDirty();
			}
		}

		[Tooltip("Viewing for debugging")]
		[SerializeField] internal Vector2 size = Vector2.zero;

		public UnityEvent<UISlider, float> onValueChanged = null;

		public float GetValue()
		{
			return value;
		}

		public void SetValue(float newValue)
		{
			value = newValue;
		}


		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}


		public IUIDataEx GetBackingData()
		{
			return new SliderEx(sliderData);
		}

		public void UpdateBackingData(UISliderScriptableObject backingData)
		{
			sliderData = backingData;
			if (sliderData != null)
			{
				labelData = sliderData.labelData;
				unitSpan = sliderData.unitSpan;
				handleOffset = sliderData.handleOffset;
				showHandle = sliderData.showHandle;
				handleSprite = sliderData.handleSprite;
				showUnits = sliderData.showUnits;
			}

		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			UpdateBackingData(((SliderEx)backingData).sliderData);
		}

		private static bool isInPrefabStage()
		{
#if UNITY_EDITOR
			var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			return stage != null;
#else
    return false;
#endif
		}

		void Update()
		{
			if (isDirty || lastWidth != rectTransform.sizeDelta.x)
				UpdateBackingData();
		}

		private float lastWidth = -1;
		public void UpdateBackingData()
		{
			Canvas.ForceUpdateCanvases();
			var slider = GetComponent<SliderCustom>();
			slider.UpdateSlider();
#if DEBUG
			// this required or the handle will not report the correct value after becoming active.
			//if (!Application.isPlaying)
			//slider.UpdateSlider();
#endif

			rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);

			lastWidth = rectTransform.sizeDelta.x;

			isDirty = false;
		}

		public Vector2 GetMinDimensions()
		{
			if (isDirty || lastWidth != rectTransform.sizeDelta.x)
				UpdateBackingData();
			return rectTransform.sizeDelta;
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput,
			ref UIDesignObject currentlySelectedObject)
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