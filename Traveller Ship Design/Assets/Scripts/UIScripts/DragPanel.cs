using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;

public class DragPanel : MonoBehaviour, IUIBehavior
{
	Vector2 startDragPos;
	private bool isDragging = false;
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
		startDragPos = Input.mousePosition;
	}
	public bool IsDragging()
	{
		return isDragging;
	}

	public void MouseDrag(Vector2 screenPosition)
	{
		Vector3 diff = screenPosition - startDragPos;
		GetComponent<RectTransform>().position += diff;
		startDragPos = screenPosition;
	}

	public void EndDrag(Vector2 pos)
	{
		throw new System.NotImplementedException();
	}

	

	public void ResetToLastPosition()
	{
		throw new System.NotImplementedException();
	}

	public DesignObject Select()
	{
		throw new System.NotImplementedException();
	}

	public void Deselect()
	{
		throw new System.NotImplementedException();
	}

}
