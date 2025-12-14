using System;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class ImageEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Image; } }

		public UIImageViewScriptableObject scriptableObject;


		public bool isVisible = true;
		public Sprite sprite;
		public bool forceSize = false;
		public Vector2 size = new Vector2(256, 256);
		public bool showCaption = true;


		public ImageEx(UIImageViewScriptableObject scriptObj)
		{
			scriptableObject = scriptObj;
		}
	}

	[ExecuteAlways]
	public class UIImageView : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Image; } }
		[SerializeField] private Image image;
		[SerializeField] private UIExpandingLabel captionLabel;
		[SerializeField] private ImageEx imageEx;

		[SerializeField] private string _referenceName;
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName= value;
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


		[SerializeField] private string _text = "Caption #00";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get { return _text; }
			set
			{
				if (_text == text)
					return;

				_text = captionLabel.text = value;
				this.SetDirty();
			}
		}

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return captionLabel.GetControl(controlRefName);
		}


		public void SetSize(Vector2 imageSize)
		{
			imageEx.size = imageSize;
			imageEx.forceSize = true;
			this.SetDirty();
		}

		public IUIDataEx GetBackingData()
		{
			return imageEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			imageEx = (ImageEx)backingData;
			UIExpandingLabelScriptableObject data = null;
			if (imageEx.scriptableObject != null)
				data = imageEx.scriptableObject.labelData;
			captionLabel.UpdateBackingData(new LabelEx(data));
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

			gameObject.SetActive(imageEx.isVisible);
			if (!imageEx.isVisible)
				return;
			image.sprite = imageEx.sprite;
			if (imageEx.forceSize)
			{
				if (imageEx.size.x <= 0)
					imageEx.size.x = 32;
				if (imageEx.size.y <= 0)
					imageEx.size.y = 32;
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, imageEx.size.x);
				image.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, imageEx.size.y);
			}
			else
			{
				image.SetNativeSize();
			}

			float height = image.rectTransform.sizeDelta.y;

			if (imageEx.showCaption)
			{
				captionLabel.gameObject.SetActive(true);
				captionLabel.UpdateBackingData();

				var layout = GetComponent<VerticalLayoutGroup>();
				var textSize = captionLabel.GetMinDimensions();
				height += textSize.y;
				height += layout.spacing;
			}
			else
			{
				captionLabel.gameObject.SetActive(false);
			}

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