using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UIButton : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Button; } }

		[SerializeField] private UIButtonScriptableObject buttonData;
		[SerializeField] public UIExpandingLabel textLabel;
		[SerializeField] private Image image;

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return textLabel.GetControl(controlRefName);
		}

		public bool interactable
		{
			get { return _interactable = textLabel.interactable = GetComponent<Button>().interactable; }
			set { _interactable = textLabel.interactable = GetComponent<Button>().interactable = value; }
		}

		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				this.SetDirty();
			}
		}

		public Vector2 maxDimensions { get; set; }


		[SerializeField] private bool _hideText = false;
		public bool hideText
		{
			get { return _hideText = !textLabel.gameObject.activeSelf; }
			set
			{
				_hideText = value;
				textLabel.gameObject.SetActive(!value);
				this.SetDirty();
			}
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
			set
			{
				_fontStyles = textLabel.fontStyles = value;
				this.SetDirty();
			}
		}

		[SerializeField] private bool _spriteIsBackground = true;
		public bool spriteIsBackground
		{
			get { return _spriteIsBackground; }
			set
			{
				_spriteIsBackground = value;
				if (_spriteIsBackground)
				{
					image.type = Image.Type.Sliced;
				}
				else
				{
					image.type = Image.Type.Simple;
					image.preserveAspect = true;
				}

				this.SetDirty();
			}
		}

		[SerializeField] private Sprite _sprite;
		[Tooltip("A value of null will revert the sprite to scriptable object value, if it exists.")]
		public Sprite sprite
		{
			get { return _sprite = image.sprite; }
			set
			{
				if (value == null && buttonData != null)
				{
					_sprite = image.sprite = buttonData.sprite;
				}
				else
				{
					_sprite = image.sprite = value;
				}

				this.SetDirty();
			}
		}

		[SerializeField] private Color _spriteColor = Color.white;
		public Color spriteColor
		{
			get { return _spriteColor = image.color; }
			set
			{
				_spriteColor = image.color = value;
			}
		}

		[SerializeField] private Vector2 _minButtonSize = new Vector2(10, 10);
		public Vector2 minButtonSize
		{
			get { return _minButtonSize; }
			set
			{
				_minButtonSize = value;
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


		public ScriptableObject GetBackingData()
		{
			return buttonData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			buttonData = (UIButtonScriptableObject)backingData;
			if (buttonData != null)
			{
				hideText = buttonData.noText;
				if (!hideText)
					textLabel.UpdateBackingData(buttonData.labelData);
				sprite = buttonData.sprite;
				spriteIsBackground = buttonData.spriteIsBackground;
			}

			this.SetDirty();
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public void RecalculateDimensions()
		{
			var layout = GetComponent<LayoutElement>();
			var vertLayout = transform.parent.GetComponent<VerticalLayoutGroup>();
			//if (vertLayout != null)
			{   // the below layout stuff only works when in a Vertical Layout Group

				if (fillParentHorizontal)
					layout.flexibleWidth = 1;
				else
					layout.flexibleWidth = 0;

				float preferredWidth = minButtonSize.x;
				float preferredHeight = minButtonSize.y;
				if (!hideText)
				{
					var labelHorzMargins = textLabel.margin.x + textLabel.margin.z;
					var labelVertMargins = textLabel.margin.y + textLabel.margin.w;

					layout.minWidth = this.textLabel.minDimensions.x + labelHorzMargins;
					var labelDim = this.textLabel.GetDrawnDimensions();
					preferredWidth = Mathf.Max(preferredWidth, labelDim.x + labelHorzMargins);
					preferredHeight = Mathf.Max(preferredHeight, labelDim.y + labelVertMargins);
				}

				if (!spriteIsBackground)
				{
					if (vertLayout != null)
					{
						var height = image.GetDesiredHeight(preferredWidth);
						preferredHeight = Mathf.Max(preferredHeight, height);
					}
					else
					{
						var width = image.GetDesiredWidth(preferredHeight);
						preferredWidth = Mathf.Max(preferredWidth, width);
					}
				}

				layout.preferredWidth = preferredWidth;
				layout.preferredHeight = preferredHeight;
			}

			if (vertLayout != null)
			{
				GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layout.preferredHeight);
			}
			else
			{
				GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layout.preferredWidth);
			}

			isDirty = false;
		}


		public Vector2 GetDrawnDimensions()
		{
			if (isDirty)
				RecalculateDimensions();
			var layout = GetComponent<LayoutElement>();
			return new Vector2(layout.preferredWidth, layout.preferredHeight);
		}
	}
}