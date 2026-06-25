using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;


namespace AtomosZ.UI
{
	public class UISpinner : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Spinner; } }

		[SerializeField] public RectTransform baseRect;
		[SerializeField] public Button leftButton;
		[SerializeField] public Button rightButton;
		[SerializeField] public TMP_InputField inputField;
		[SerializeField] public TextMeshProUGUI placeholderText;
		[SerializeField] public TextMeshProUGUI text;

		[SerializeField] private Vector2 _minInputFieldDimensions = new Vector2(64, 32);
		public Vector2 minInputFieldDimensions
		{
			get { return _minInputFieldDimensions; }
			set
			{
				_minInputFieldDimensions = value;
				this.SetDirty();
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
				var oldValue = _value;
				_value = value;
				if (_value > maxValue)
					_value = maxValue;
				else if (_value < minValue)
					_value = minValue;
				inputField.text = _value.ToString();
				if (_value != oldValue)
				{
					this.SetDirty();
					if (onValueChanged != null)
						onValueChanged.Invoke(this, _value);
				}
			}
		}

		[SerializeField] private float _fontSize = 36;
		public float fontSize
		{
			get { return _fontSize; }
			set
			{
				_fontSize = inputField.pointSize = value;
				this.SetDirty();
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
				this.SetDirty();
			}
		}

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

		public UnityEvent<UISpinner, int> onValueChanged = null;

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

		void Start()
		{
			this.SetDirty();
		}



		public void SetTextAlignment(HorizontalAlignmentOptions horzAlignment,
			VerticalAlignmentOptions vertAlignment)
		{
			text.verticalAlignment = vertAlignment;
			placeholderText.verticalAlignment = vertAlignment;
			text.horizontalAlignment = horzAlignment;
			placeholderText.horizontalAlignment = horzAlignment;
			this.SetDirty();
		}

		public ScriptableObject GetBackingData()
		{
			return new UISpinnerScriptableObject();
		}


		/// <summary>
		/// this is my first attempt and creating controls without the IUIDataEx stuff (please work).
		/// First step is to implement functionality using a dummy IUIDataEx,
		/// then remove all IUIDataEx and only pass the ScriptableObjects.
		/// </summary>
		/// <param name="backingData"></param>
		public void UpdateBackingData(ScriptableObject backingData)
		{
			RecalculateDimensions();
		}

		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public override void RecalculateDimensions()
		{
			// calculate min dimension for input field give max&min
			text.ForceMeshUpdate(false, true);
			var minPrefTextSize = placeholderText.GetPreferredValues(_minValue.ToString());
			var maxPrefTextSize = placeholderText.GetPreferredValues(_maxValue.ToString());
			Vector2 maxSize = new Vector2(
				Mathf.Max(minPrefTextSize.x, maxPrefTextSize.x), Mathf.Max(minPrefTextSize.y, maxPrefTextSize.y));

			var textAreaRect = text.transform.parent.GetComponent<RectTransform>();
			maxSize.x += -textAreaRect.offsetMax.x + textAreaRect.offsetMin.x;
			maxSize.y += -textAreaRect.offsetMax.y + textAreaRect.offsetMin.y;

			if (maxSize.x < _minInputFieldDimensions.x)
				maxSize.x = _minInputFieldDimensions.x;
			if (maxSize.y < _minInputFieldDimensions.y)
				maxSize.y = _minInputFieldDimensions.y;
			var inputRect = inputField.GetComponent<RectTransform>();
			inputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxSize.x);
			inputRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, maxSize.y);

			Vector2 totalDimens = Vector2.zero;
			var buttonRect = leftButton.GetComponent<RectTransform>();


			totalDimens.x += buttonRect.sizeDelta.x * 2 + inputRect.sizeDelta.x;
			totalDimens.y = Mathf.Max(buttonRect.sizeDelta.y, inputRect.sizeDelta.y);

			totalDimens.x = Mathf.Max(_minDimensions.x, totalDimens.x);
			totalDimens.y = Mathf.Max(_minDimensions.y, totalDimens.y);
			baseRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalDimens.x);
			baseRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalDimens.y);

			var rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalDimens.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalDimens.y);

			preferredSize = totalDimens;
			isDirty = false;
		}


		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return baseRect.sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}
	}
}