using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static CustomCursor;

public class UIDesignObject : MonoBehaviour
{
	public CursorSpriteMode hoverCursorMode = CursorSpriteMode.UI_Default;
	public CursorSpriteMode moveCursorMode = CursorSpriteMode.UI_Default;

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

	public bool isHoverable = false;
	public bool isMoveable = false;
	public bool isModal = false;
	public bool isSelectable = false;
	public bool hasCustomDimensions = false;
	public bool hasUpdatableBackingData = false;
	private IUIBehavior uiBehavior;


	public List<string> tooltip;

	void Awake()
	{
		SearchForDesignObject();
	}

	private void SearchForDesignObject()
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

		throw new Exception("UIDesignObject MUST have a IUIBehavior!");
	}

	public void SetHover(bool isHover)
	{
		if (!isHoverable)
			return;

		if (uiBehavior == null)
			SearchForDesignObject();

		uiBehavior.SetHover(isHover);
		if (isHover)
			CustomCursor.SetCursor(hoverCursorMode);
		else
			CustomCursor.SetCursor(CursorSpriteMode.Default);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		if (!isHoverable)
			return;

		if (uiBehavior == null)
			SearchForDesignObject();

		uiBehavior.UpdateHover(posOfHover);
	}

	public Vector2 GetMinDimensions()
	{
		if (hasCustomDimensions)
		{
			if (uiBehavior == null)
				SearchForDesignObject();
			return uiBehavior.GetMinDimensions();
		}

		return GetComponent<RectTransform>().sizeDelta;
	}


	public void ResetToLastPosition()
	{
		if (isMoveable)
		{
			if (uiBehavior == null)
				SearchForDesignObject();
			uiBehavior.ResetToLastPosition();
		}
	}

	public void UpdateBackingData()
	{
		if (hasUpdatableBackingData)
		{
			if (uiBehavior == null)
				SearchForDesignObject();
			uiBehavior.UpdateBackingData();
		}
	}

		public void UpdateBackingData(UIDataEx backingData)
	{
		if (hasUpdatableBackingData)
		{
			if (uiBehavior == null)
				SearchForDesignObject();
			uiBehavior.UpdateBackingData(backingData);
		}
	}

	public UIDesignObject Select()
	{
		if (isSelectable)
		{
			if (uiBehavior == null)
				SearchForDesignObject();
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
				SearchForDesignObject();

			DesignManager.instance.toolTip.SetToolTip(null);
			uiBehavior.Deselect();
		}
	}
}
