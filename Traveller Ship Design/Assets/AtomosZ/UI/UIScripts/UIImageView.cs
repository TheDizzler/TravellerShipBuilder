using System;

using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UIImageView : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Image; } }
		[SerializeField] private Image image;
		[SerializeField] private UIExpandingLabel captionLabel;
		[SerializeField] private UIImageViewScriptableObject imageData;

		public Button button { get { return GetComponent<Button>(); } }


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


		[SerializeField] private bool _interactable = true;
		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				button.interactable = value;
			}
		}

		[SerializeField] public Sprite _sprite;
		[Tooltip("null sprite will set to defaultSprite (if available)")]
		public Sprite sprite
		{
			get { return _sprite = image.sprite; }
			set
			{
				if (value == null && imageData != null)
					value = imageData.defaultSprite;
				_sprite = image.sprite = value;
				this.SetDirty();
			}
		}

		[SerializeField] private string _text = "Caption #00";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get { return _text = captionLabel.text; }
			set
			{
				if (_text == text)
					return;

				_text = captionLabel.text = value;
				this.SetDirty();
			}
		}


		[SerializeField] private Vector2 _minDimensions;
		/// <summary>
		/// If either x or y is <= 0, sets image to native size.
		/// </summary>
		public Vector2 minDimensions
		{
			get { return _minDimensions = image.rectTransform.sizeDelta; }
			set
			{
				if (value.x <= 0 || value.y <= 0)
				{
					image.SetNativeSize();
				}
				else
				{
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
				}
				this.SetDirty();
			}
		}

		/// <summary>
		/// If either x or y is <= 0, sets image to native size.
		/// </summary>
		/// <param name="imageSize"></param>
		public void SetSize(Vector2 imageSize)
		{
			minDimensions = imageSize;
		}

		[SerializeField] private bool _showImage = true;
		public bool showImage
		{
			get { return _showImage; }
			set
			{
				_showImage = value;
				image.gameObject.SetActive(value);
				this.SetDirty();
			}
		}
		[SerializeField] private bool _showCaption = true;
		public bool showCaption
		{
			get { return _showCaption; }
			set
			{
				_showCaption = value;
				captionLabel.gameObject.SetActive(value);
				this.SetDirty();
			}
		}


		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return captionLabel.GetControl(controlRefName);
		}

		public ScriptableObject GetBackingData()
		{
			return imageData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			imageData = (UIImageViewScriptableObject)backingData;
			if (imageData != null)
			{
				captionLabel.UpdateBackingData(imageData.labelData);
				showImage = imageData.isImageHidden;
				showCaption = imageData.isCaptionHidden;
			}

			UpdateBackingData();
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			this.SetGameObjectNameToReferenceName(gameObject);

			float height = 0;
			float width = 0;
			if (showImage)
			{
				width = image.rectTransform.sizeDelta.x;
				height = image.rectTransform.sizeDelta.y;
			}

			if (showCaption)
			{
				captionLabel.UpdateBackingData();

				var layout = GetComponent<VerticalLayoutGroup>();
				var textSize = captionLabel.GetMinDimensions();
				height += textSize.y;
				height += layout.spacing;
				width = MathF.Max(textSize.x, width);
			}

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

			isDirty = false;
		}

		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			return GetComponent<RectTransform>().sizeDelta;
		}

		public void SetHover(bool isHover)
		{
			//caption.SetHover "EDFF00"
		}

		public void UpdateHover(Vector3 posOfHover)
		{
			//throw new System.NotImplementedException();
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}

		public UIDesignObject Select()
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
	}
}