using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static DesignManager;

namespace AtomosZ.UI
{
	public class DragPanel : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private DynamicPanel panel;
		[SerializeField] private RectTransform parentRect;
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


		public void SetHover(bool isHover)
		{
		}

		public void UpdateHover(Vector3 posOfHover)
		{
		}

		public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput,
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
			startDragPos = GetUICoordinatesFromMousePos();
			panel.isDragging = true;
			CustomCursor.SetCursor(panel.designObject.moveCursorMode);
		}

		/// <summary>
		/// TODO(Tristan): Check for windows bounds!
		/// </summary>
		public void MouseDrag()
		{
			var screenPosition = GetUICoordinatesFromMousePos();
			Vector2 diff = screenPosition - startDragPos;
			parentRect.anchoredPosition += diff;
			startDragPos = screenPosition;
		}

		public void EndDrag()
		{
			var screenPosition = GetUICoordinatesFromMousePos();
			Vector2 diff = screenPosition - startDragPos;
			parentRect.anchoredPosition += diff;
			panel.isDragging = false;
			CustomCursor.SetCursor(panel.designObject.hoverCursorMode);
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
