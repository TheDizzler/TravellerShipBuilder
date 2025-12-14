using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class ButtonEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Button; } }
		public UIButtonScriptableObject scriptableObj;

		public ButtonEx(UIButtonScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
		}
	}

	[ExecuteAlways]
	public class UIButton : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Button; } }

		[SerializeField] private UIButtonScriptableObject buttonData;
		[SerializeField] public UIExpandingLabel textLabel;
		[SerializeField] private Image image;

		[SerializeField] private string _referenceName = "button";
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				this.SetGameObjectNameToReferenceName(gameObject);
			}
		}

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

		public bool isDirty { get; set; }

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return textLabel.GetControl(controlRefName);
		}

		[SerializeField] private bool _interactable = true;
		public bool interactable
		{
			get { return _interactable = textLabel.interactable = GetComponent<Button>().interactable; }
			set { _interactable = textLabel.interactable = GetComponent<Button>().interactable = value; }
		}

		[SerializeField] private string _text = "Button Text";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get { return _text = textLabel.text; }
			set { _text = textLabel.text = value; }
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

		[SerializeField] private Sprite _sprite;
		[Tooltip("A value of null will revert the sprite to scriptable object value, if it exists.")]
		public Sprite sprite
		{
			get { return _sprite = image.sprite; }
			set
			{
				if (value == null)
				{
					if (buttonData != null)
						image.sprite = buttonData.sprite;
				}
				else
					image.sprite = value;
				this.SetDirty();
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

		public UnityEvent<UIButton> onClickedEvent = null;


		/// <summary>
		/// Adds action to the listener.
		/// </summary>
		/// <param name="action"></param>
		public void AddListener(UnityAction<UIButton> action)
		{
			onClickedEvent.AddListener(action);
		}



		void OnEnable()
		{
			var button = GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			button.onClick.AddListener(OnClicked);

			this.SetDirty();
		}

		private void OnClicked()
		{
			if (onClickedEvent != null)
				onClickedEvent.Invoke(this);
		}


		public IUIDataEx GetBackingData()
		{
			return new ButtonEx(buttonData);
		}

		public void UpdateBackingData(UIButtonScriptableObject backingData)
		{
			buttonData = backingData;
			if (backingData != null)
			{
				textLabel.UpdateBackingData(backingData.labelData);
				image.sprite = backingData.sprite;
			}

			this.SetDirty();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			UpdateBackingData(((ButtonEx)backingData).scriptableObj);
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			var layout = GetComponent<LayoutElement>();
			if (fillParentHorizontal)
				layout.flexibleWidth = 1;
			else
				layout.flexibleWidth = 0;

			var labelHorzMargins = textLabel.margin.x + textLabel.margin.z;
			layout.minWidth = this.textLabel.minLabelDimensions.x + labelHorzMargins;
			var labelDim = this.textLabel.GetMinDimensions();
			layout.preferredWidth = labelDim.x + labelHorzMargins;

			isDirty = false;
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			return textLabel.GetMinDimensions();
		}


		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput,
			ref UIDesignObject currentlySelectedObject)
		{
			throw new NotImplementedException();
		}

		public void Deselect()
		{
			throw new NotImplementedException();
		}

		public void ResetToLastPosition()
		{
			throw new NotImplementedException();
		}

		public UIDesignObject Select()
		{
			throw new NotImplementedException();
		}

		public void SetHover(bool isHover)
		{
		}

		public void UpdateHover(Vector3 posOfHover)
		{
		}
	}
}