using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIDesignObject : MonoBehaviour
{

	private RectTransform _rect;
	public RectTransform rect
	{
		get
		{
			if (_rect == null)
				_rect = GetComponent<RectTransform>();
			return _rect;
		}
	}

	//public bool isHoverable = false;
	public bool isModal = false;
	public bool isMoveable = false;
	public bool isSelectable = false;
	private IUIBehavior uiBehavior;

	//public bool isCreateable = false;
	//public bool isInteractable = false;


	public List<string> tooltip;

	void Awake()
	{
		SearchForDesignObjects();
	}

	public void MouseDrag(Vector2 pos)
	{
		if (isMoveable)
		{
			if (uiBehavior == null)
				SearchForDesignObjects();
			uiBehavior.MouseDrag(pos);
		}
	}

	public void EndDrag(Vector2 pos)
	{
		if (isMoveable)
		{
			if (uiBehavior == null)
				SearchForDesignObjects();
			uiBehavior.EndDrag(pos);
		}
	}

	public void ResetToLastPosition()
	{
		if (isMoveable)
		{
			if (uiBehavior == null)
				SearchForDesignObjects();
			uiBehavior.ResetToLastPosition();
		}
	}

	public DesignObject Select()
	{
		if (isSelectable)
		{
			if (uiBehavior == null)
				SearchForDesignObjects();
			DesignManager.instance.toolTip.SetToolTip(tooltip);
			return uiBehavior.Select();
		}

		return null;
	}

	public void Deselect()
	{
		if (isSelectable)
		{
			if (uiBehavior == null)
				SearchForDesignObjects();

			DesignManager.instance.toolTip.SetToolTip(null);
			uiBehavior.Deselect();
		}
	}



	private void SearchForDesignObjects()
	{
		var components = GetComponents<MonoBehaviour>();
		foreach (var comp in components)
		{
			if (comp is IUIBehavior)
			{
				uiBehavior = (IUIBehavior)comp;
				return;
			}
		}
	}
}
