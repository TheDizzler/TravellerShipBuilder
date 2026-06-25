using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;

namespace AtomosZ.UI
{
	public class UITabItem : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get; }
		public bool interactable { get; set; }


		[SerializeField] private UIExpandingLabel _label;
		public UIExpandingLabel label
		{
			[DebuggerStepThrough]
			[HideInCallstack]
			get
			{
				if (_label == null)
					_label = GetComponentInChildren<UIExpandingLabel>();
				return _label;
			}
		}

		[SerializeField] public UIPanel panel;

		private Image _image;
		public Image image
		{
			[DebuggerStepThrough]
			[HideInCallstack]
			get
			{
				if (_image == null)
					_image = GetComponent<Image>();
				return _image;
			}
		}

		public Sprite sprite
		{
			[DebuggerStepThrough]
			[HideInCallstack]
			get
			{
				if (_image == null)
					_image = GetComponent<Image>();
				return _image.sprite;
			}
			set
			{
				if (_image == null)
					_image = GetComponent<Image>();
				_image.sprite = value;
			}
		}

		private MagicWindowBase window;
		private RectTransform windowRect;
		private Vector3 startDragPos;
		private Vector3 distanceToPointerFromTopLeft;
		[SerializeField] internal int tabIndex;

		void OnEnable()
		{
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
				return;
#endif
			window = GetComponentInParent<MagicWindowBase>();
			if (window == null)
			{
				UnityEngine.Debug.LogError("UITabItem must be on MagicWindow or MagicTabbedWindow to be draggable.");
				return;
			}

			windowRect = window.GetComponent<RectTransform>();
		}


		Vector3 screenPointOffset;
		public void BeginDrag()
		{
			Vector3 windowScreenPos = Helpers.camera.WorldToScreenPoint(windowRect.position);
			screenPointOffset = transform.position - UIInput.instance.GetMouseUIWorldPos(windowScreenPos.z);

			window.SetDragging(true);
		}


		/// <summary>
		/// @TODO(Tristan): Check for windows bounds!
		/// </summary>
		public void MouseDrag()
		{
			Vector3 objectScreenPos = Helpers.camera.WorldToScreenPoint(windowRect.position);
			windowRect.position = UIInput.instance.GetMouseUIWorldPos(objectScreenPos.z) + screenPointOffset;
		}

		public void EndDrag()
		{
			Vector3 objectScreenPos = Helpers.camera.WorldToScreenPoint(windowRect.position);
			windowRect.position = UIInput.instance.GetMouseUIWorldPos(objectScreenPos.z) + screenPointOffset;
			window.SetDragging(false);
		}


		public void PointerUp()
		{
			window.SelectTab(transform.GetSiblingIndex());
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return label.GetControl(controlRefName);
		}


		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		public override void RecalculateDimensions()
		{
			label.RecalculateDimensions();
			//var drawnDimensions = label.GetDrawnSize();
			preferredSize = label.GetPreferredSize();

			preferredSize.x = MathF.Max(preferredSize.x, minDimensions.x);
			preferredSize.y = MathF.Max(preferredSize.y, minDimensions.y);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);

			label.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
			label.RecalculateDimensions();
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

		/// <summary>
		/// This ignores minDimensions!
		/// </summary>
		/// <param name="titleWidth"></param>
		internal void SetWidth(float titleWidth)
		{
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, titleWidth);
		}
	}
}