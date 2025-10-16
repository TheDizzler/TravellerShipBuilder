using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.ShaderGraph;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using static DesignManager;

namespace AtomosZ.UI
{
	[Serializable]
	public class ImageViewDataEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.ImagePanel; } }
		public string referenceName;

		/// <summary>
		/// I have decided that this MAY NOT be NULL!<br/>
		/// And so it shall be so...
		/// </summary>
		public UIImageViewPanelScriptableObject scriptableObj;

		//public Vector2 imageSize = new Vector2(256, 256);
		//public Vector2 maxPanelSize = new Vector2(512, 512);
		public bool useAllAvailableHeight = false;
		//public Sprite defaultSprite;
		public bool showCaptions = true;

		/// <summary>
		/// This should share the same label data (except the text, of course) with all child images.
		/// </summary>
		public LabelEx labelEx;
		//= new LabelEx
		//{
		//	//fontColor = Color.black,
		//	//fontSize = 36,
		//	//maxLabelDimensions = new Vector2(125, 125),
		//	text = "Image #00",
		//};

		public ImageViewDataEx(UIImageViewPanelScriptableObject scriptObj)
		{
			if (scriptObj == null)
				Debug.LogException(new Exception("Invalid ImageViewPanel state: may not be null."));
			scriptableObj = scriptObj;
			labelEx = new LabelEx(scriptableObj.labelData);
		}

		//public void ResetToDefaults()
		//{
		//	imageSize = new Vector2(256, 256);
		//	useAllAvailableHeight = false;
		//	maxPanelSize = new Vector2(512, 512);
		//	labelEx = new LabelEx
		//	{
		//		//fontColor = Color.black,
		//		//fontSize = 36,
		//		//maxLabelDimensions = new Vector2(125, 125),
		//		text = "Image #00",
		//	};
		//}
	}


	public class UIImageViewPanel : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private ImageViewDataEx viewDataEx;
		[SerializeField] public GridLayoutGroup gridLayout;
		[SerializeField] public ScrollRect scrollRect;

		[SerializeField] public Dictionary<ImageEx, UIImageView> images = new();

		public string referenceName { get { return viewDataEx.referenceName; } set { viewDataEx.referenceName = value; }}
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

		void OnEnable()
		{
			var imageViews = GetComponentsInChildren<UIImageView>();
			images.Clear();
			foreach (var imageView in imageViews)
			{
				images.Add((ImageEx)imageView.GetBackingData(), imageView);
			}
		}

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			foreach (var image in images)
			{
				var ctrl = image.Value.GetControl(controlRefName);
				if (ctrl != null)
					return ctrl;
			}

			return null;
		}


		public IUIDataEx GetBackingData()
		{
			return viewDataEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			viewDataEx = (ImageViewDataEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			this.SetNameToReferenceName(gameObject);

			var rect = GetComponent<RectTransform>();
			var panelSize = rect.sizeDelta;

			var minSize = gridLayout.cellSize;
			minSize.x += gridLayout.padding.right + gridLayout.padding.left;
			minSize.y += gridLayout.padding.top + gridLayout.padding.bottom;

			if (viewDataEx.useAllAvailableHeight)
			{
				panelSize.y = viewDataEx.scriptableObj.maxPanelSize.y;
			}

			var newWidth = Mathf.Max(panelSize.x, minSize.x);
			var newHeight = Mathf.Max(panelSize.y, minSize.y);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newHeight);

			var contentWidthAvailable = rect.sizeDelta.x - (gridLayout.padding.right + gridLayout.padding.left);
			var imageWidth = minSize.x + gridLayout.spacing.x;
			var imagesPerRow = contentWidthAvailable / imageWidth;
			float imagesOnRow = Mathf.FloorToInt(imagesPerRow);
			if (imagesOnRow <= 0)
				imagesOnRow = 1;
			int rowsRequired = Mathf.CeilToInt(images.Count / imagesOnRow);

			var contentHeight = rowsRequired * minSize.y;
			scrollRect.content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);

			foreach (var image in images)
			{
				image.Key.showCaption = viewDataEx.showCaptions;
				image.Key.forceSize = true;
				image.Key.size = viewDataEx.scriptableObj.imageSize;
				//image.Key.labelEx.fontSize = viewData.labelEx.fontSize;
				//image.Key.labelEx.fontColor = viewData.labelEx.fontColor;
				//image.Key.labelEx.fontAsset = viewData.labelEx.fontAsset;
				//image.Key.labelEx.maxLabelDimensions = viewData.labelEx.maxLabelDimensions;
				//image.Key.labelEx.minLabelDimensions = viewData.labelEx.minLabelDimensions;
				image.Value.UpdateBackingData();
			}
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			//var baseRect = GetComponent<RectTransform>();
			//var panelSize = baseRect.sizeDelta;

			//var minSize = gridLayout.cellSize;
			//minSize.x += gridLayout.padding.right + gridLayout.padding.left;
			//minSize.y += gridLayout.padding.top + gridLayout.padding.bottom;

			//var newWidth = Mathf.Max(panelSize.x, minSize.x);
			//var newHeight = Mathf.Max(panelSize.y, minSize.y);

			//return new Vector2(newWidth, newHeight);
			return GetComponent<RectTransform>().sizeDelta;
		}


		public void ClearImages()
		{
			foreach (var image in images)
			{
#if UNITY_EDITOR
				if (Application.isEditor && !Application.isPlaying)
					DestroyImmediate(image.Value.gameObject);
				else
					Destroy(image.Value.gameObject);
#else
			Destroy(image.Value.gameObject);
#endif
			}

			images.Clear();
		}

		public void RemoveImage(ImageEx imageEx)
		{
			if (!images.TryGetValue(imageEx, out var imageView))
			{
				Debug.LogWarning("Image not found in view");
				return;
			}

			images.Remove(imageEx);

#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
				DestroyImmediate(imageView.gameObject);
			else
				Destroy(imageView.gameObject);
#else
		Destroy(imageView.gameObject);
#endif
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sprite">Null sprite will result in the view panels default sprite.</param>
		/// <param name="caption"></param>
		/// <returns></returns>
		public UIImageView AddImage(Sprite sprite, string caption, UnityAction value)
		{
			var imageEx = new ImageEx(viewDataEx.scriptableObj.imageViewData)
			{
				sprite = sprite == null ? viewDataEx.scriptableObj.defaultSprite : sprite,
				labelEx = viewDataEx.labelEx.Clone(),
			};

			imageEx.labelEx.text = caption;
			var imageView = AddImage(imageEx);
			imageView.GetComponent<Button>().onClick.AddListener(value);
			return imageView;
		}

		/// <summary>
		/// All data (except text) in LabelEx gets overwritten by viewPanel defaults.
		/// </summary>
		/// <param name="imageEx"></param>
		/// <returns></returns>
		public UIImageView AddImage(ImageEx imageEx)
		{
			if (images.ContainsKey(imageEx))
			{
				Debug.LogWarning("Image already in view");
				return images[imageEx];
			}

			imageEx.size = viewDataEx.scriptableObj.imageSize;
			if (imageEx.labelEx == null)
				imageEx.labelEx = viewDataEx.labelEx;
			imageEx.forceSize = true;
			var imageDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabProvider.UIPrefabType.ImageView), gridLayout.transform);
			var image = imageDO.GetComponent<UIImageView>();

			image.UpdateBackingData(imageEx);
			images.Add(imageEx, image);
			return image;
		}


		public void ResetToLastPosition()
		{
			throw new System.NotImplementedException();
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