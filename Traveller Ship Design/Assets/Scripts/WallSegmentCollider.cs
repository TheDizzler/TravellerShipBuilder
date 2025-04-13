using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static DesignManager;


public class WallSegmentCollider : MonoBehaviour, IHoverable, IInteractable, ISelectable
{
	[SerializeField] private BoxCollider2D collider2d;
	public int index = -1;
	private Wall wall;
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

	public void Initialize(Wall wall, int index)
	{
		this.wall = wall;
		this.index = index;
	}

	public void UpdatePosition(Vector2 lowerIndexPoint, Vector2 higherIndexPoint)
	{
		var ab = lowerIndexPoint - higherIndexPoint;
		var length = ab.magnitude;

		Vector2 size = new Vector2(length, 0.5f);
		Vector2 offset = new Vector2((lowerIndexPoint.x + higherIndexPoint.x) / 2, (lowerIndexPoint.y + higherIndexPoint.y) / 2);
		float angle = Mathf.Atan2(ab.y, ab.x) * 180 / Mathf.PI;
		var quaternion = Quaternion.AngleAxis(angle, Vector3.forward);

		transform.position = offset;
		collider2d.size = size;
		transform.SetPositionAndRotation(offset, quaternion);
	}


	public void SetHover(bool isHovering)
	{
		wall.SetHover(isHovering);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		wall.UpdateHover(posOfHover);
	}

	public bool Interact(IInteractable otherObject)
	{
		if (otherObject is Door door)
		{
			// lock door to wall
		}

		return false;
	}

	public void EndInteraction()
	{

	}

	public void BindDoor(Door door, Vector2 pos)
	{
		wall.BindDoor(door, transform, pos);
	}

	public DesignObject Select()
	{
		return wall.Select();
	}

	public void Deselect()
	{

	}

	public void Clicked(Vector3 worldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		wall.Clicked(worldPos, keyInput, ref currentlySelectedObject, ref editMode);
	}

	public Dictionary<string, DesignAction> GetContextMenuItems()
	{
		var actionDict = new Dictionary<string, DesignAction>();
		DesignAction createRoomAction = new DesignAction(EditMode.None);
		createRoomAction += wall.ConvertToRoom;
		actionDict.Add("Create Room", createRoomAction);
		return actionDict;
	}
}
