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
		public string referenceName;

		//public UISliderScriptableObject scriptableObj;

		//public bool isEnabled = true;
		//public bool fillParentHorizontal = true;
		//public Vector2 minDimensions = new Vector2(126, 64);

		////public bool useCustomShowHandle = false;
		////public bool useCustomHandleSprite = false;
		////public bool useCustomHandleOffset = false;
		//public bool useCustomShowUnits = false;
		//public bool useCustomUnitSpan = false;
		////public bool useCustomUnitVerticalOffset = false;

		////public bool showHandle = true;
		////public Sprite handleSprite;
		////public Vector2 handleOffset = new Vector2(16, 16);

		//public bool showUnits = true;
		//public float unitSpan = 1;
		//public int unitVerticalOffset = 0;


		//public float minValue = 0;
		//public float maxValue = 4;
		//public float value = 0;
		//public bool wholeNumbers = true;

		//public LabelEx labelEx;

		public SliderEx(UISliderScriptableObject sliderSO)
		{
			//scriptableObj = sliderSO;
			//	if (sliderSO == null)
			//	{
			//		//useCustomShowHandle = true;
			//		useCustomShowUnits = true;
			//		useCustomUnitSpan = true;
			//		labelEx = new LabelEx("")
			//		{
			//			referenceName = "_slider_labelEx",
			//			fontColor = Color.black,
			//			fontSize = 18,
			//			minLabelDimensions = new Vector2(8, 8),
			//		};
			//	}
		}
	}

	public class UISlider : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private SliderEx sliderEx;
		[SerializeField] private string _referenceName;
		public string referenceName { get { return _referenceName; } set { _referenceName = value; } }

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
			}
		}

		[SerializeField] private float _value;
		public float value
		{
			get { return _value; }
			set
			{
				float oldValue = _value; // this doesn't work when changing the value from the editor. This is good, I guess?
				if (_wholeNumbers)
					value = Mathf.RoundToInt(value);
				_value = value;
				if (_value > maxValue)
					_value = maxValue;
				else if (_value < minValue)
					_value = minValue;
				slider.SetValueFill();
				if (showUnits)
					slider.CreateUnitLabels();

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
			set { _unitVerticalOffset = value; }
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
			}
		}

		[SerializeField] private LabelEx _labelEx;
		public LabelEx labelEx
		{
			get { return _labelEx; }
			set { _labelEx = value; }
		}


		[SerializeField] private bool _showHandle = true;
		public bool showHandle
		{
			get { return _showHandle; }
			set
			{
				_showHandle = value;
				slider.ShowHandle(value);
			}
		}

		[SerializeField] private Sprite _handleSprite;
		public Sprite handleSprite
		{
			get
			{
				_handleSprite = GetComponent<SliderCustom>().handleSprite;
				return _handleSprite;
			}
			set
			{
				_handleSprite = value;
				var slider = GetComponent<SliderCustom>().handleSprite = value;
			}
		}

		[SerializeField] private Vector2 _handleOffset = new Vector2(16, 16);
		public Vector2 handleOffset
		{
			get { return _handleOffset; }
			set { _handleOffset = value; }
		}


		private SliderCustom slider
		{
			get { return GetComponent<SliderCustom>(); }
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
			}
		}

		[SerializeField] private Vector2 _maxDimensions;
		public Vector2 maxDimensions
		{
			get { return _maxDimensions; }
			set { _maxDimensions = value; }
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
			return sliderEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			sliderEx = (SliderEx)backingData;
			UpdateBackingData();
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

		public void UpdateBackingData()
		{
			this.SetNameToReferenceName(gameObject);

			Canvas.ForceUpdateCanvases();
			var slider = GetComponent<SliderCustom>();
			slider.UpdateSlider(sliderEx);
#if DEBUG
			// this required or the handle will not report the correct value after becoming active.
			//if (!Application.isPlaying)
			//slider.UpdateSlider(sliderEx);
#endif

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			//return GetComponent<SliderCustom>().GetMinDimensions();
			return GetComponent<RectTransform>().sizeDelta;
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