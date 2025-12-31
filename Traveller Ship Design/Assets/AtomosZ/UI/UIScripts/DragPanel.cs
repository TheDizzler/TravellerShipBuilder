using System.Collections;
using System.Collections.Generic;
using AtomosZ.MG2eTraveller.Ship;	// @TODO(Tristan): Ship design stuff should not be referenced by UI tools
using UnityEngine;

using static AtomosZ.Keyboard;
using static AtomosZ.UI.MagicWindow;


namespace AtomosZ.UI
{
	public class DragPanel : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get; }

		[SerializeField] private MagicWindow panel;
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

		public bool isDirty { get; set; }
		public string referenceName { get; set; }
		
		public bool interactable { get; set; }

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

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}
	}
}
