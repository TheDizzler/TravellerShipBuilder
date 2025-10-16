using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static AtomosZ.Keyboard;


namespace AtomosZ.UI
{
	public class DragPanel : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private DynamicPanel panel;
		[SerializeField] private RectTransform panelRect;
		private Vector2 startDragPos;

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

		/// <summary>
		/// DragPanel does not require a reference name.
		/// </summary>
		public string referenceName { get; set; }

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public void SetHover(bool isHover)
		{
		}

		public void UpdateHover(Vector3 posOfHover)
		{
		}

		public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput,
			ref UIDesignObject currentlySelectedObject)
		{
			if (currentlySelectedObject != null)
			{
				currentlySelectedObject.Deselect();
			}

			currentlySelectedObject = designObject;
			currentlySelectedObject.Select();
		}

		public void BeginDrag()
		{
			startDragPos = UIInput.GetUICoordinates(Input.mousePosition);
			panel.isDragging = true;
			CustomCursor.SetCursor(panel.designObject.moveCursorMode);
		}

		/// <summary>
		/// TODO(Tristan): Check for windows bounds!
		/// </summary>
		public void MouseDrag()
		{
			var screenPosition = UIInput.GetUICoordinates(Input.mousePosition);
			Vector2 diff = screenPosition - startDragPos;
			panelRect.anchoredPosition += diff;
			startDragPos = screenPosition;
		}

		public void EndDrag()
		{
			var screenPosition = UIInput.GetUICoordinates(Input.mousePosition);
			Vector2 diff = screenPosition - startDragPos;
			panelRect.anchoredPosition += diff;
			panel.isDragging = false;
			CustomCursor.SetCursor(panel.designObject.hoverCursorMode);
		}


		public void PointerUp()
		{
			panel.SelectTab(transform.GetSiblingIndex());
		}


		public void ResetToLastPosition()
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

		public Vector2 GetMinDimensions()
		{
			throw new System.NotImplementedException();
		}

		public IUIDataEx GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}
	}
}
