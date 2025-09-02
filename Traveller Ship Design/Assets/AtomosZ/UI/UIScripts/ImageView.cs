using System;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class ImageEx : IUIDataEx
	{
		public PanelControlType dataType { get { return PanelControlType.Image; } }

		public bool isVisible = true;
		public Sprite sprite;
		public bool forceSize = false;
		public Vector2 size = new Vector2(256, 256);
		public bool showCaption = true;
		public LabelEx labelEx = new LabelEx
		{
			fontColor = Color.black,
			fontSize = 36,
			text = "Caption #00",
		};

		public object Clone()
		{
			var clone = (ImageEx)this.MemberwiseClone();
			clone.labelEx = (LabelEx)labelEx.Clone();
			return clone;
		}

		public void ResetToDefaults()
		{
			forceSize = false;
			size = new Vector2(256, 256);
			labelEx = new LabelEx
			{
				fontColor = Color.black,
				fontSize = 36,
				text = "Caption #00",
			};
		}
	}

	public class ImageView : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private Image image;
		[SerializeField] private UIExpandingLabel caption;
		[SerializeField] private ImageEx imageEx;

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


		public void SetSize(Vector2 imageSize)
		{
			imageEx.size = imageSize;
			imageEx.forceSize = true;
		}

		public IUIDataEx GetBackingData()
		{
			return imageEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			imageEx = (ImageEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
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
				caption.gameObject.SetActive(true);
				caption.UpdateBackingData(imageEx.labelEx);

				var layout = GetComponent<VerticalLayoutGroup>();
				var textSize = caption.GetMinDimensions();
				height += textSize.y;
				height += layout.spacing;
			}
			else
			{
				caption.gameObject.SetActive(false);
			}

			GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();

			//var size = GetComponent<RectTransform>().sizeDelta;
			//size.y += image.rectTransform.sizeDelta.y;

			//var layout = GetComponent<VerticalLayoutGroup>();
			//if (imageEx.showCaption)
			//{
			//	var textSize = caption.GetMinDimensions();
			//	size.y += textSize.y;
			//	size.y += layout.spacing;
			//}

			//return size;

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

		public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
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