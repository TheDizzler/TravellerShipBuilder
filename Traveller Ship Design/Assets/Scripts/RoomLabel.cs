using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static DesignManager;

public class RoomLabel : MonoBehaviour, IHoverable, IMoveable, ISelectable
{
	[SerializeField] private ExpandingLabel roomLabel;
	[SerializeField] private Room room;

	private DesignObject _designObject;
	public DesignObject designObject
	{
		get
		{
			if (_designObject == null)
				_designObject = GetComponent<DesignObject>();
			return _designObject;
		}
	}

	public string text
	{
		get { return roomLabel.text; }
		set { roomLabel.text = value; }
	}



	public DesignObject Select()
	{
		return room.Select();
	}

	public void Deselect()
	{
		room.Deselect();
	}

	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		room.Clicked(mouseWorldPos, keyInput, ref currentlySelectedObject, ref editMode);
	}

	public bool IsDragging()
	{
		return ((IMoveable)room).IsDragging();
	}

	public void MouseDrag(Vector2 worldPos)
	{
		((IMoveable)room).MouseDrag(worldPos);
	}


	public void ResetToLastPosition()
	{
		((IMoveable)room).ResetToLastPosition();
	}

	public void EndDrag(Vector2 pos)
	{
		((IMoveable)room).EndDrag(pos);
	}

	public Vector3 SnapToGrid(Vector3 pos)
	{
		return ((IMoveable)room).SnapToGrid(pos);
	}


	public Dictionary<string, DesignAction> GetContextMenuItems()
	{
		return room.GetContextMenuItems();
	}



	public void SetHover(bool isHovering)
	{
		room.SetHover(isHovering);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		room.UpdateHover(posOfHover);
	}

	public void SetHoverColor(bool isHovering)
	{
		if (isHovering)
			roomLabel.color = designObject.hoverColor;
		else
			roomLabel.color = designObject.normalColor;
	}
}
