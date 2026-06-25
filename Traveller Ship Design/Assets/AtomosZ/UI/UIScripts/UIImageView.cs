using System;

using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UIImageView : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Image; } }
		[SerializeField] private Image image;
		[SerializeField] private UIExpandingLabel captionLabel;
		[SerializeField] private UIImageViewScriptableObject imageData;

		public Button button { get { return GetComponent<Button>(); } }

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


		public UIMonoBehaviour GetControl(string controlRefName)
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

			RecalculateDimensions();
		}

		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public override void RecalculateDimensions()
		{
			float height = 0;
			float width = 0;
			if (showImage)
			{
				if (minDimensions.x <= 0 || minDimensions.y <= 0)
				{
					image.SetNativeSize();
				}
				else
				{
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minDimensions.x);
					image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDimensions.y);
				}

				width = image.rectTransform.sizeDelta.x;
				height = image.rectTransform.sizeDelta.y;
			}

			if (showCaption)
			{
				captionLabel.RecalculateDimensions();

				var layout = GetComponent<VerticalLayoutGroup>();
				var textSize = captionLabel.GetDrawnSize();
				height += textSize.y;
				height += layout.spacing;
				width = MathF.Max(textSize.x, width);
			}

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

			preferredSize.x = width;
			preferredSize.y = height;

			isDirty = false;
		}

		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}
	}
}