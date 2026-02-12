using UnityEngine;

using static AtomosZ.UI.MagicWindow;


namespace AtomosZ.UI
{
	public class DragPanel : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get; }

		public Vector2 minDimensions { get; set; }
		public Vector2 maxDimensions { get; set; }

		private MagicWindow window;
		private RectTransform windowRect;
		private Vector2 startDragPos;

		public bool interactable { get; set; }

		void OnEnable()
		{
			window = GetComponentInParent<MagicWindow>();
			windowRect = window.GetComponent<RectTransform>();
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}


		public void BeginDrag()
		{
			startDragPos = UIInput.GetUICoordinates(Input.mousePosition);
			window.isDragging = true;
			window.cursors.SetCursor(UICursors.UICursorMode.Drag);
		}

		/// <summary>
		/// @TODO(Tristan): Check for windows bounds!
		/// </summary>
		public void MouseDrag()
		{
			var screenPosition = UIInput.GetUICoordinates(Input.mousePosition);
			Vector2 diff = screenPosition - startDragPos;
			windowRect.anchoredPosition += diff;
			startDragPos = screenPosition;
		}

		public void EndDrag()
		{
			var screenPosition = UIInput.GetUICoordinates(Input.mousePosition);
			Vector2 diff = screenPosition - startDragPos;
			windowRect.anchoredPosition += diff;
			window.isDragging = false;
			window.cursors.SetCursor(UICursors.UICursorMode.Default);
		}


		public void PointerUp()
		{
			window.SelectTab(transform.GetSiblingIndex());
		}

		public Vector2 GetDrawnDimensions()
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

		public void RecalculateDimensions()
		{
			throw new System.NotImplementedException();
		}
	}
}
