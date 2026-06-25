using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.ObjectForge;
using static AtomosZ.UI.MagicWindowBase;

namespace AtomosZ.UI
{
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
#if UNITY_EDITOR
				isDirty = true;
#endif
				hideText = buttonData.noText;
				if (!hideText)
					textLabel.UpdateBackingData(buttonData.labelData);
				sprite = buttonData.sprite;
				spriteIsBackground = buttonData.spriteIsBackground;
#if UNITY_EDITOR
				isDirty = false;
#endif
			}

			this.SetDirty();
		}

		private Vector2 preferredSize;
		public override void RecalculateDimensions()
		{
#if UNITY_EDITOR
			if (!Helpers.IsSceneValid(this))
			{   // probably dragging the button into the scene.
				isDirty = false;
				return;
			}
#endif
			var layout = GetComponent<VerticalLayoutGroup>();
			preferredSize = textLabel.GetPreferredSize();
			preferredSize.x += layout.padding.horizontal;
			preferredSize.y += layout.padding.vertical;

			//var currentButtonWidth = rect.sizeDelta.x;
			//if (prefLabelSize.x + layout.padding.horizontal > currentButtonWidth)
			{
				//currentButtonWidth = prefLabelSize.x + layout.padding.horizontal;
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);
			}

			isDirty = false;

			////var parentVertLayout = transform.parent.GetComponent<VerticalLayoutGroup>();
			//////if (parentVertLayout != null)
			////{   // the below layout stuff only works when in a Vertical Layout Group

			////	if (fillParentHorizontal)
			////		layoutElement.flexibleWidth = 1;
			////	else
			////		layoutElement.flexibleWidth = -1;

			////	float preferredWidth = minButtonSize.x;
			////	float preferredHeight = minButtonSize.y;
			////	if (!hideText)
			////	{
			////		var labelHorzMargins = textLabel.margin.x + textLabel.margin.z;
			////		var labelVertMargins = textLabel.margin.y + textLabel.margin.w;

			////		layoutElement.minWidth = this.textLabel.minDimensions.x + labelHorzMargins;
			////		var labelDim = this.textLabel.GetDrawnDimensions();
			////		preferredWidth = Mathf.Max(preferredWidth, labelDim.x + labelHorzMargins);
			////		preferredHeight = Mathf.Max(preferredHeight, labelDim.y + labelVertMargins);
			////	}

			////	if (!spriteIsBackground)
			////	{
			////		if (parentVertLayout != null)
			////		{
			////			var height = image.GetDesiredHeight(preferredWidth);
			////			preferredHeight = Mathf.Max(preferredHeight, height);
			////		}
			////		else
			////		{
			////			var width = image.GetDesiredWidth(preferredHeight);
			////			preferredWidth = Mathf.Max(preferredWidth, width);
			////		}
			////	}

			////	layoutElement.preferredWidth = preferredWidth;
			////	layoutElement.preferredHeight = preferredHeight;
			////}

			////if (parentVertLayout != null)
			////{
			////	rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, layoutElement.preferredHeight);
			////}
			////else
			////{
			////	rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, layoutElement.preferredWidth);
			////}

			//isDirty = false;
			//textLabel.RecalculateDimensions();
		}


		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}
	}
}