using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;

public class WallControlPoint : MonoBehaviour, IMoveable, IHoverable, ICreateable, IInteractable, ISelectable
{
	[SerializeField] private new SpriteRenderer renderer;

	public int index;
	public Wall wall;

	private bool isDragging;
	private Vector3 lastPosition;
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

	void Start()
	{
		if (!isDragging)
			renderer.color = designObject.normalColor;
		else
			renderer.color = designObject.selectColor;
	}

	public Vector3 SnapToGrid(Vector3 pos)
	{
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		return pos;
	}


	public void SetHover(bool isHovering)
	{
		if (!isDragging)
		{
			if (isHovering)
				renderer.color = designObject.hoverColor;
			else
				renderer.color = designObject.normalColor;
		}

		if (wall != null && isHovering)
			wall.MinorSelect();
	}

	public void UpdateHover(Vector3 _)
	{

	}

	public void StartDrag()
	{
		isDragging = true;
		if (wall != null)
			wall.SetDragging(true);
		lastPosition = transform.position;

		CustomCursor.SetCursor(designObject.moveCursorMode);
	}

	public void MouseDrag(Vector2 worldPos)
	{
		if (wall != null)
			wall.MovePoint(worldPos, index);
		else // just a wee starter wall
			transform.position = worldPos;
	}

	public bool IsDragging()
	{
		return isDragging;
	}

	public void EndDrag(Vector2 pos)
	{
		isDragging = false;
		renderer.color = designObject.normalColor;
		if (wall != null)
		{
			wall.MovePoint(pos, index);
			wall.SetDragging(false);
		}
	}

	public void ResetToLastPosition()
	{
		if (!wall.definitelyFinalized)
		{
			RemoveControlPoint();
		}
		else
		{
			wall.MovePoint(lastPosition, index);
			if (isDragging)
				EndDrag(lastPosition);
		}
	}


	/// <summary>
	/// 
	/// </summary>
	/// <param name="pos"></param>
	/// <param name="createdObject"></param>
	/// <returns>Returns the DesignObject of the 2nd newly created control point (controlPoints[1]).</returns>
	public EditMode Create(Vector3 pos, out DesignObject createdObject)
	{
		var newWall = Instantiate(DesignManager.GetPrefab(PrefabType.WallSegmentPrefab));
		var wallSeg = newWall.GetComponent<Wall>();
		createdObject = wallSeg.CreateFirstPoint(pos).designObject;
		createdObject.gameObject.SetActive(true);
		wallSeg.MovePoint(pos, 1); // is this needed?
		return EditMode.MoveObject;
	}

	public bool Interact(IInteractable otherObject)
	{
		//if (otherObject is WallSegmentControlPoint cntrlPnt)
		//{
		//	return true;
		//}

		return false;
	}

	public void EndInteraction()
	{

	}

	/// <summary>
	/// Returns true when removing this control point results in the destruction of the whole wall (not used).
	/// </summary>
	/// <returns>(not used)</returns>
	private bool RemoveControlPoint()
	{
		return wall.RemoveControlPoint(index);
	}

	public DesignObject Select()
	{
		renderer.color = designObject.selectColor;
		if (wall != null)
			wall.MinorSelect();
		return designObject;
	}

	public void Deselect()
	{
		renderer.color = designObject.normalColor;
		if (wall != null)
			wall.Deselect();
	}

	public void Clicked(Vector3 worldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (currentlySelectedObject != null)
		{
			if (currentlySelectedObject != this)
				currentlySelectedObject.Deselect();
		}

		if ((keyInput & KeyInput.Ctrl) == KeyInput.Ctrl && wall.IsEndControlPoint(this))
		{
			var newCtrlPnt = wall.AddControlPointToEnd(worldPos, 
				index, ref currentlySelectedObject, ref editMode);
			newCtrlPnt.StartDrag();
		}
		else
		{
			currentlySelectedObject = designObject;
			StartDrag();
			editMode = EditMode.MoveObject;
		}
	}

	public Dictionary<string, DesignAction> GetContextMenuItems()
	{
		var actionDict = new Dictionary<string, DesignAction>();

		DesignAction createRoomAction = new DesignAction(EditMode.None);
		createRoomAction += wall.ConvertToRoom;
		actionDict.Add("Create Room", createRoomAction);

		actionDict.Add("divider", null);

		DesignAction deleteAction = new DesignAction(EditMode.None);
		deleteAction += Delete;
		actionDict.Add("Delete Control Point", deleteAction);
		return actionDict;
	}

	public void Delete()
	{
		RemoveControlPoint();
	}
}
