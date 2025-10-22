using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace AtomosZ.UI
{
	public class SpinnerEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Spinner; } }
	}

	public class UISpinner : MonoBehaviour, IUIBehavior
	{
		/// <summary>
		/// Trying to deprecate this.
		/// </summary>
		//private SpinnerEx spinnerEx;

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

		[SerializeField] private string _referenceName;
		public string referenceName { get { return _referenceName; } set { _referenceName = value; } }

		[SerializeField] public RectTransform baseRect;
		[SerializeField] public Button leftButton;
		[SerializeField] public Button rightButton;
		[SerializeField] public TMP_InputField inputField;
		[SerializeField] public TextMeshProUGUI placeholderText;
		[SerializeField] public TextMeshProUGUI text;

		[SerializeField] private Vector2 _minDimen = new Vector2(64, 32);
		public Vector2 minInputFieldDimensions
		{
			get { return _minDimen; }
			set
			{
				_minDimen = value;
			}
		}

		[SerializeField] private int _minValue = int.MinValue;
		public int minValue
		{
			get { return _minValue; }
			set
			{
				if (value > maxValue)
					_minValue = maxValue;
				else
					_minValue = value;

				placeholderText.SetText(_minValue + " / " + _maxValue);
				this.value = this.value;
			}
		}

		[SerializeField] private int _maxValue = int.MaxValue;
		public int maxValue
		{
			get { return _maxValue; }
			set
			{
				if (value < minValue)
					_maxValue = minValue;
				else
					_maxValue = value;

				placeholderText.SetText(_minValue + " / " + _maxValue);
				this.value = this.value;
			}
		}

		[SerializeField] private int _value;
		public int value
		{
			get { return _value; }
			set
			{
				_value = value;
				if (_value > maxValue)
					_value = maxValue;
				else if (_value < minValue)
					_value = minValue;
				inputField.text = _value.ToString();
			}
		}

		[SerializeField] private float _fontSize = 36;
		public float fontSize
		{
			get { return _fontSize; }
			set
			{
				_fontSize = value;
				inputField.pointSize = value;
			}
		}


		[SerializeField] private TextAlignmentOptions _alignmentOptions;
		public TextAlignmentOptions alignmentOptions
		{
			get { return _alignmentOptions; }
			set
			{
				_alignmentOptions = value;
				var vert = (VerticalAlignmentOptions)(value
					& (TextAlignmentOptions)(VerticalAlignmentOptions.Baseline | VerticalAlignmentOptions.Bottom
					| VerticalAlignmentOptions.Capline | VerticalAlignmentOptions.Geometry
					| VerticalAlignmentOptions.Middle | VerticalAlignmentOptions.Top));

				text.verticalAlignment = vert;
				placeholderText.verticalAlignment = vert;

				var horz = (HorizontalAlignmentOptions)(value ^ (TextAlignmentOptions)vert);
				text.horizontalAlignment = horz;
				placeholderText.horizontalAlignment = horz;
			}
		}


		[SerializeField] private bool _interactable = true;
		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				leftButton.interactable = value;
				rightButton.interactable = value;
				inputField.interactable = value;
			}
		}

		public void OnRightButtonClick()
		{
			Increment();
		}

		public void OnLeftButtonClick()
		{
			Decrement();
		}

		public int Increment()
		{
			++value;
			return value;
		}


		public int Decrement()
		{
			--value;
			return value;
		}



		public void SetTextAlignment(HorizontalAlignmentOptions horzAlignment,
			VerticalAlignmentOptions vertAlignment)
		{
			text.verticalAlignment = vertAlignment;
			placeholderText.verticalAlignment = vertAlignment;
			text.horizontalAlignment = horzAlignment;
			placeholderText.horizontalAlignment = horzAlignment;
		}

		public IUIDataEx GetBackingData()
		{
			return new SpinnerEx();
		}

		/// <summary>
		/// this is my first attempt and creating controls without the IUIDataEx stuff (please work).
		/// First step is to implement functionality using a dummy IUIDataEx,
		/// then remove all IUIDataEx and only pass the ScriptableObjects.
		/// </summary>
		/// <param name="backingData"></param>
		public void UpdateBackingData(IUIDataEx backingData)
		{
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			this.SetNameToReferenceName(gameObject);

			// calculate min dimension for input field give max&min
			text.ForceMeshUpdate(false, true);
			var minPrefTextSize = placeholderText.GetPreferredValues(_minValue.ToString());
			var maxPrefTextSize = placeholderText.GetPreferredValues(_maxValue.ToString());
			Vector2 maxSize = new Vector2(
				Mathf.Max(minPrefTextSize.x, maxPrefTextSize.x), Mathf.Max(minPrefTextSize.y, maxPrefTextSize.y));

			var textAreaRect = text.transform.parent.GetComponent<RectTransform>();
			maxSize.x += -textAreaRect.offsetMax.x + textAreaRect.offsetMin.x;
			maxSize.y += -textAreaRect.offsetMax.y + textAreaRect.offsetMin.y;

			if (maxSize.x < _minDimen.x)
				maxSize.x = _minDimen.x;
			if (maxSize.y < _minDimen.y)
				maxSize.y = _minDimen.y;
			var inputRect = inputField.GetComponent<RectTransform>();
			inputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize.x);
			inputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize.y);

			Vector2 totalDimens = Vector2.zero;
			var buttonRect = leftButton.GetComponent<RectTransform>();


			totalDimens.x += buttonRect.sizeDelta.x * 2 + inputRect.sizeDelta.x;
			totalDimens.y = Mathf.Max(buttonRect.sizeDelta.y, inputRect.sizeDelta.y);


			baseRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalDimens.x);
			baseRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalDimens.y);

			var rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalDimens.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalDimens.y);
		}


		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			return baseRect.sizeDelta;
		}


		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
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