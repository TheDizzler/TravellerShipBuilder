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

	public bool isModal = false;
	public bool isMoveable = false;
	public bool isSelectable = false;
	public bool hasCustomDimensions = false;
	private IUIBehavior uiBehavior;


	public List<string> tooltip;

	void Awake()
	{
		SearchForDesignObjects();
	}

	public Vector2 GetMinDimensions()
	{
		if (hasCustomDimensions)
		{
			if (uiBehavior == null)
				SearchForDesignObjects();
			return uiBehavior.GetMinDimensions();
		}

		return GetComponent<RectTransform>().sizeDelta;
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

	public UIDesignObject Select()
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
