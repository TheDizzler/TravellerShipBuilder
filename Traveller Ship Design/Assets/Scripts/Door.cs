using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEditor.U2D.ScriptablePacker;
using static DesignManager;
using UnityEngine.Events;
using UnityEditor.Timeline.Actions;

[RequireComponent(typeof(SpriteMask))]
public class Door : MonoBehaviour, IMoveable, IHoverable, ICreateable, IInteractable, ISelectable
{
	/// <summary>
	/// Minimum distance between other objects and door.
	/// </summary>
	public static float padding = .25f;

	[SerializeField] private TextMeshPro debugText;

	private new SpriteRenderer renderer;
	private SpriteMask spriteMask;
	private Wall wall;
	private Vector3 lastPosition;
	private Wall lastWall;
	private int lastWallSegmentIndex;


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

	public int WallSegmentIndex
	{
		get { return wallSegmentIndex; }
		set
		{
			wallSegmentIndex = value;
			SetDebugText(wallSegmentIndex.ToString());
		}
	}

	private int wallSegmentIndex = -1;
	private WallSegmentCollider wallSegCollider;
	private bool isDragging;

	void Awake()
	{
		spriteMask = GetComponent<SpriteMask>();
		renderer = GetComponent<SpriteRenderer>();
		renderer.color = designObject.normalColor;
	}


	/// <summary>
	/// <b>NOTE: This should be called from</b> <c>Wall</c> <b>only!</b>
	/// </summary>
	/// <param name="wallSegment"></param>
	/// <param name="wallSegmentIndex"></param>
	public void BindWall(Wall wallSegment, int wallSegmentIndex)
	{
		wall = wallSegment;
		WallSegmentIndex = wallSegmentIndex;
		spriteMask.enabled = true;
	}

	public void UnbindWall()
	{
		//lastWall = wall;//debug only!
		//lastWallSegmentIndex = wallSegmentIndex; // debug only!


		transform.SetParent(null, true);
		WallSegmentIndex = -1;
		wall = null;

		spriteMask.enabled = false;
	}

	public Vector3 SnapToGrid(Vector3 pos)
	{
		var xInt = Mathf.FloorToInt(pos.x);
		float xFloat = Mathf.Abs(pos.x - xInt);
		if (xFloat > .25f && xFloat < .75f)
		{
			pos.x = xInt + .5f;
		}
		else
		{
			pos.x = Mathf.Round(pos.x);
		}

		var yInt = Mathf.FloorToInt(pos.y);
		float yFloat = Math.Abs(pos.y - yInt);
		if (yFloat > .25f && yFloat < .75f)
		{
			pos.y = yInt + .5f;
		}
		else
		{
			pos.y = Mathf.Round(pos.y);
		}

		return pos;
	}

	public void SetHover(bool isHovering)
	{
		if (isHovering)
			renderer.color = designObject.hoverColor;
		else
			renderer.color = designObject.normalColor;
	}

	public void UpdateHover(Vector3 _) { }

	public float GetLength()
	{
		return renderer.size.x;
	}

	private void SetDebugText(string text)
	{
		debugText.text = text;
	}


	private void StartDrag()
	{
		isDragging = true;
		lastPosition = transform.position;
		if (wall != null)
		{
			lastWall = wall;
			lastWallSegmentIndex = wallSegmentIndex;
			wall.UnbindDoor(this);
		}

		CustomCursor.SetCursor(designObject.moveCursorMode);
	}

	public void MouseDrag(Vector2 worldPos)
	{
		transform.position = worldPos;
	}

	public bool IsDragging()
	{
		return isDragging;
	}

	public void EndDrag(Vector2 pos)
	{
		if (wallSegCollider != null)
		{
			wallSegCollider.BindDoor(this, pos);
			wallSegCollider = null;
		}
		else
		{
			transform.position = pos;
		}

		renderer.color = designObject.normalColor;
		lastWall = null;
		lastWallSegmentIndex = -1;
		isDragging = false;
	}

	public void ResetToLastPosition()
	{
		if (lastWall == null)
		{
			transform.position = lastPosition;
		}
		else
		{
			lastWall.BindDoor(this, lastWallSegmentIndex, lastPosition);
		}

		isDragging = false;
	}

	public DesignManager.EditMode Create(Vector3 pos, out DesignObject createdObject)
	{
		createdObject = designObject;
		//return EditMode.EditObject;
		return EditMode.None;
	}

	public bool Interact(IInteractable otherObject)
	{
		if (otherObject is WallSegmentCollider wallSeg)
		{
			spriteMask.enabled = true;
			wallSegCollider = wallSeg;
			LockDoorToWallSegment(this.gameObject, wallSeg, transform.position);
			return true;
		}

		return false;
	}

	/// <summary>
	/// This is the mouse over lock, not the true bind to wall lock.
	/// </summary>
	/// <param name="doorCursor"></param>
	/// <param name="wallCollider"></param>
	/// <param name="worldPos"></param>
	private void LockDoorToWallSegment(GameObject doorCursor, WallSegmentCollider wallCollider, Vector2 worldPos)
	{
		//Debug.Log("Locking");
		var wallRot = wallCollider.transform.rotation;
		var selectedWall = wallCollider.GetComponentInParent<Wall>();

		selectedWall.FindClosestPoint(worldPos, wallCollider.index, out Vector3 nearestPointOnLine, out float t, out Vector3 lineStart, out Vector3 lineEnd);
		var lineLength = (lineEnd - lineStart).magnitude;
		var doorLength = doorCursor.GetComponent<BoxCollider2D>().size.x;
		var halfDoorLength = doorLength / 2;
		var halfDoorT = halfDoorLength / lineLength;

		if (t - halfDoorT < 0)
		{
			var newT = halfDoorT;
			nearestPointOnLine = Vector2.Lerp(lineStart, lineEnd, newT);
		}
		else if (t + halfDoorT > 1)
		{
			var newT = 1 - halfDoorT;
			nearestPointOnLine = Vector2.Lerp(lineStart, lineEnd, newT);
		}

		doorCursor.transform.SetPositionAndRotation(nearestPointOnLine, wallRot);

		if (t > 0 && t < 1)
			selectedWall.UpdateHover(worldPos);
	}

	public void EndInteraction()
	{
		transform.rotation = Quaternion.AngleAxis(0, Vector3.forward);
		spriteMask.enabled = false;
		wallSegCollider = null;
	}

	public DesignObject Select()
	{
		renderer.color = designObject.selectColor;
		if (wall != null)
		{
			wall.MinorSelect();
		}

		return designObject;
	}

	public void Deselect()
	{
		renderer.color = designObject.normalColor;
		if (wall != null)
		{
			wall.Deselect();
		}
	}

	public void Clicked(Vector3 worldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (currentlySelectedObject != null)
		{
			if (currentlySelectedObject != this)
				currentlySelectedObject.Deselect();
		}

		currentlySelectedObject = designObject;

		this.Select();
		this.StartDrag();

		editMode = EditMode.MoveObject;
		//if ((keyInput & KeyInput.Ctrl) == KeyInput.Ctrl)
		//{
		//	CustomCursor.SetCursor(CustomCursor.CursorSpriteMode.ResizeHorizontal);
		//}
	}


	public Dictionary<string, DesignAction> GetContextMenuItems()
	{
		var actionDict = new Dictionary<string, DesignAction>();

		if (wall != null)
		{
			DesignAction createRoomAction = new DesignAction(EditMode.None);
			createRoomAction += wall.ConvertToRoom;
			actionDict.Add("Create Room", createRoomAction);

			actionDict.Add("divider", null);
		}


		var deleteAction = new DesignAction(EditMode.None);
		deleteAction += Delete;
		actionDict.Add("Delete Door", deleteAction);
		return actionDict;
	}

	public void Delete()
	{
		if (wall != null)
			wall.UnbindDoor(this);

		Destroy(gameObject);
	}
}