using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;


[RequireComponent(typeof(LineRenderer))]
public class Wall : MonoBehaviour, /*IHoverable,*/ /*ISelectable,*/ IMoveable
{
	[SerializeField] private LineRenderer lineRenderer;
	[SerializeField] private List<WallSegmentCollider> colliders = new();

	private PointIndicator pointIndicator;
	/// <summary>
	/// Serialized for debugging
	/// </summary>
	[SerializeField] private List<Door> doors = new();
	private List<WallControlPoint> controlPoints = new();
	private bool dragging;
	/// <summary>
	/// This wall has definitely been finalized and is not in the first construction phase. <br/>
	/// Used when trying to determine if a dragging cntrlpnt should reset to last position or destroy this wall
	/// because the wall has not been finalized yet.
	/// </summary>
	public bool definitelyFinalized;

	private bool isDragging;
	private Vector2 startDragPosition;
	private Vector3[] startDragPoints;

	private DesignObject _designObject;
	private bool isRoom;

	public DesignObject designObject
	{
		get
		{
			if (_designObject == null)
				_designObject = GetComponent<DesignObject>();
			return _designObject;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="worldPos"></param>
	/// <returns>Returns controlPoints[1].</returns>
	public WallControlPoint CreateFirstPoint(Vector2 worldPos)
	{
		this.pointIndicator = GameObject.FindFirstObjectByType<PointIndicator>();
		dragging = true;
		lineRenderer.SetPosition(0, worldPos);
		lineRenderer.SetPosition(1, worldPos);

		var newCtrlPnt = CreateControlPoint(lineRenderer.GetPosition(0), controlPoints.Count);
		++controlPoints.Capacity;
		controlPoints.Add(newCtrlPnt);
		newCtrlPnt = CreateControlPoint(lineRenderer.GetPosition(1), controlPoints.Count);
		++controlPoints.Capacity;
		controlPoints.Add(newCtrlPnt);

		CreateCollider(lineRenderer.GetPosition(0), lineRenderer.GetPosition(1), 0);

		lineRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
		return controlPoints[1];
	}

	public DesignObject Select()
	{
		lineRenderer.startColor = designObject.selectColor;
		lineRenderer.endColor = designObject.selectColor;
		ShowControlPoints(true);
		return designObject;
	}

	public void MinorSelect()
	{
		lineRenderer.startColor = designObject.minorSelectColor;
		lineRenderer.endColor = designObject.minorSelectColor;
		ShowControlPoints(true);
	}

	public void Deselect()
	{
		lineRenderer.startColor = designObject.normalColor;
		lineRenderer.endColor = designObject.normalColor;
		ShowControlPoints(false);
		pointIndicator.ToggleIndicator(false);
	}

	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (currentlySelectedObject != null)
		{
			currentlySelectedObject.Deselect();
		}

		if ((keyInput & KeyInput.Ctrl) == KeyInput.Ctrl)
		{
			var ctrlPnt = AddControlPoint(mouseWorldPos);
			currentlySelectedObject = ctrlPnt.designObject;
			editMode = EditMode.None;
		}
		else
		{
			currentlySelectedObject = designObject;
			StartDrag(mouseWorldPos);
			editMode = EditMode.MoveObject;
		}

		currentlySelectedObject.Select();
	}

	public bool IsEndControlPoint(WallControlPoint ctrlPnt)
	{
		return ctrlPnt.index == 0 || ctrlPnt.index == controlPoints.Count - 1;
	}

	public WallControlPoint AddControlPointToEnd(Vector3 mouseWorldPos, int ctrlPntIndexClicked, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		WallControlPoint newCtrlPnt;
		Vector3 newPointPos;
		int insertIndex;
		if (ctrlPntIndexClicked == 0)
		{
			insertIndex = 0;
			newCtrlPnt = CreateControlPoint(mouseWorldPos, insertIndex);

			newPointPos = lineRenderer.GetPosition(0);
			CreateCollider(newPointPos, newPointPos, insertIndex);

			for (int i = 0; i < controlPoints.Count; ++i)
			{
				++controlPoints[i].index;
			}

			++lineRenderer.positionCount;
			var points = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(points);
			for (int i = 0; i < points.Length - 1; ++i)
			{
				lineRenderer.SetPosition(i + 1, points[i]);
			}

			foreach (var door in doors)
				++door.WallSegmentIndex;
		}
		else if (ctrlPntIndexClicked == controlPoints.Count - 1)
		{
			insertIndex = controlPoints.Count;
			newCtrlPnt = CreateControlPoint(mouseWorldPos, insertIndex);

			newPointPos = lineRenderer.GetPosition(ctrlPntIndexClicked);
			CreateCollider(newPointPos, newPointPos, ctrlPntIndexClicked);

			++lineRenderer.positionCount;
		}
		else
			throw new Exception("Must be an end control point!");

		lineRenderer.SetPosition(insertIndex, newPointPos);
		++controlPoints.Capacity;
		controlPoints.Insert(insertIndex, newCtrlPnt);

		VerifyIndexOrder(controlPoints);

		currentlySelectedObject = newCtrlPnt.designObject;

		editMode = EditMode.MoveObject;
		return newCtrlPnt;
	}

	public Vector3 SnapToGrid(Vector3 pos)
	{
		pos.x = Mathf.Round(pos.x);
		pos.y = Mathf.Round(pos.y);
		return pos;
	}

	public void StartDrag(Vector3 mouseWorldPos)
	{
		isDragging = true;
		if (DesignManager.snapToGrid)
			startDragPosition = SnapToGrid(mouseWorldPos);
		else
			startDragPosition = mouseWorldPos;
		startDragPoints = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(startDragPoints);
		CustomCursor.SetCursor(designObject.moveCursorMode);
	}

	public bool IsDragging()
	{
		return isDragging;
	}

	public void SetDragging(bool isDragging)
	{
		dragging = isDragging;
		ShowControlPoints(true);
		definitelyFinalized = true;
	}

	public void MouseDrag(Vector2 worldPos)
	{
		Vector3 diff = worldPos - startDragPosition;
		for (int cntrlPntIndex = 0; cntrlPntIndex < lineRenderer.positionCount; ++cntrlPntIndex)
		{
			controlPoints[cntrlPntIndex].MouseDrag(startDragPoints[cntrlPntIndex] + diff);
		}
	}

	public void EndDrag(Vector2 pos)
	{
		MouseDrag(pos);

		lineRenderer.startColor = designObject.normalColor;
		lineRenderer.endColor = designObject.normalColor;
		isDragging = false;

		startDragPoints = null;
	}

	public void ResetToLastPosition()
	{
		MouseDrag(startDragPosition);
		isDragging = false;

		startDragPoints = null;
	}

	public void MovePoint(Vector3 movingPointPos, int cntrlPntIndex)
	{
		if (cntrlPntIndex > 0)
		{
			var wallSegCollider = colliders[cntrlPntIndex - 1];
			var stationaryPoint = controlPoints[cntrlPntIndex - 1].transform.position;

			//var attachedDoors = new List<Door>();
			float minLength = 0;
			foreach (var door in doors)
			{
				if (door.WallSegmentIndex == cntrlPntIndex - 1)
					//attachedDoors.Add(door);
					minLength += door.GetLength() + Door.padding * 2;
			}

			if (minLength > 0)
			{
				var ab = stationaryPoint - movingPointPos;
				var length = ab.magnitude;
				if (length < minLength)
				{
					var diff = minLength - length;
					movingPointPos -= ab * diff;
				}
			}

			wallSegCollider.UpdatePosition(movingPointPos, stationaryPoint);
		}

		if (cntrlPntIndex < colliders.Count)
		{
			var wallSegCollider = colliders[cntrlPntIndex];
			var stationaryPoint = controlPoints[cntrlPntIndex + 1].transform.position;

			wallSegCollider.UpdatePosition(stationaryPoint, movingPointPos);
		}

		//Mouse.WarpCursorPosition();
		controlPoints[cntrlPntIndex].transform.position = movingPointPos;
		lineRenderer.SetPosition(cntrlPntIndex, movingPointPos);
	}



	public void SetHover(bool isHovering)
	{
		if (!dragging)
		{
			if (isHovering)
			{
				lineRenderer.startColor = designObject.hoverColor;
				lineRenderer.endColor = designObject.hoverColor;
				pointIndicator.ToggleIndicator(true);
				ShowControlPoints(true);
				//gradient.colorKeys = baseColorKeys;
				//lineRenderer.colorGradient = gradient;
			}
			else
			{
				lineRenderer.startColor = designObject.normalColor;
				lineRenderer.endColor = designObject.normalColor;
				pointIndicator.ToggleIndicator(false);
				ShowControlPoints(false);
				//var points = new Vector3[lineRenderer.positionCount];
				//lineRenderer.GetPositions(points);

				//var wallLength = GetTotalLength(points);
				//var segmentLengths = GetSegmentLengths(points);
				//var colorKeys = new GradientColorKey[controlPoints.Count - 1];
				//var color32s = new Color32[controlPoints.Count - 1];
				////var alphaKeys = new GradientAlphaKey[controlPoints.Count];
				//var testColor = new Color(1, 1, 1, 1);
				//float t = 0;
				//for (int i = 0; i < controlPoints.Count - 1; ++i)
				//{
				//	var length = segmentLengths[i];
				//	var relativeLength = length / wallLength;
				//	t += relativeLength;
				//	colorKeys[i] = new GradientColorKey(testColor, t);
				//	color32s[i] = new Color32((byte)(testColor.r * 255), (byte)(testColor.g * 255), (byte)(testColor.b * 255), 255);
				//	//alphaKeys[i] = new GradientAlphaKey(1, t);
				//	testColor.r -= .2f;
				//	testColor.b -= .15f;
				//	if (testColor.g == 1.0f)
				//		testColor.g = 0.0f;
				//	else
				//		testColor.g = 1.0f;

				//}

				//gradient.colorKeys = colorKeys;
				//lineRenderer.colorGradient = gradient;
			}
		}
	}

	public void UpdateHover(Vector3 worldPos)
	{
		if (dragging)
		{
			pointIndicator.ToggleIndicator(false);
			return;
		}

		var points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);
		if (FindClosestPoint(worldPos, points, out Vector3 closestPointOnLine, out int nearestPointAIndex))
			pointIndicator.UpdatePosition(closestPointOnLine);
	}

	private void ShowControlPoints(bool display)
	{
		if (isRoom)
			display = false;

		foreach (var controlPoint in controlPoints)
		{
			controlPoint.gameObject.SetActive(display);
		}
	}


	public void BindDoor(Door door, int wallSegmentIndex, Vector2 worldPos)
	{
		BindDoor(door, colliders[wallSegmentIndex].transform, worldPos);
	}

	public void BindDoor(Door door, Transform wallSegmentColliderTrans, Vector2 worldPos)
	{
		var wallRot = wallSegmentColliderTrans.rotation;
		var wallCollider = wallSegmentColliderTrans.GetComponent<WallSegmentCollider>();
		FindClosestPoint(worldPos, wallCollider.index, out Vector3 nearestPointOnLine, out float t, out Vector3 lineStart, out Vector3 lineEnd);
		//door.transform.SetPositionAndRotation(nearestPointOnLine, wallRot);

		var segmentCollider = colliders[wallCollider.index];
		door.transform.SetParent(segmentCollider.transform, true);
		doors.Add(door);
		door.transform.SetPositionAndRotation(nearestPointOnLine, wallRot);
		door.BindWall(this, wallCollider.index);
	}

	public void UnbindDoor(Door door)
	{
		doors.Remove(door);
		door.UnbindWall();
	}


	public void ConvertToRoom()
	{
		var roomDObj = Instantiate(DesignManager.GetPrefab(PrefabType.RoomPrefab), Vector3.zero, Quaternion.identity);
		transform.SetParent(roomDObj.transform);
		var room = roomDObj.GetComponent<Room>();
		var center = GetCenter();
		room.SetRoom(this, "New Room #", center);
		this.enabled = false; // this doesn't really do anything
		foreach (var collider in colliders)
			collider.gameObject.SetActive(false);
		ShowControlPoints(false);
		isRoom = true;
	}

	public void RevertFromRoom()
	{
		this.enabled = true;
		foreach (var collider in colliders)
			collider.gameObject.SetActive(true);
		isRoom = false;
	}

	public Vector2 GetCenter()
	{
		var points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);

		float leftmost = points[0].x;
		float rightmost = points[0].x;
		float topmost = points[0].y;
		float bottommost = points[0].y;

		for (int i = 1; i < points.Length; ++i)
		{
			var point = points[i];
			rightmost = Mathf.Max(point.x, rightmost);
			leftmost = Mathf.Min(point.x, leftmost);
			topmost = Mathf.Max(point.y, topmost);
			bottommost = Mathf.Min(point.y, bottommost);
		}

		var centerx = Mathf.Lerp(rightmost, leftmost, .5f);
		var centery = Mathf.Lerp(bottommost, topmost, .5f);
		var centerPoint = new Vector2(centerx, centery);

		return centerPoint;
	}

	/// <summary>
	/// Returns true if this wall is destroyed (not used).
	/// </summary>
	/// <param name="ctrlPntIndex"></param>
	/// <exception cref="Exception"></exception>
	public bool RemoveControlPoint(int ctrlPntIndex)
	{
		pointIndicator.ToggleIndicator(false);

		if (controlPoints.Count <= 2)
		{   // destroy this wall completely
			Destroy(this.gameObject);
			return true;
		}

		var removeCtrlPnt = controlPoints[ctrlPntIndex];
		if (ctrlPntIndex == 0)
		{
			var removeDoors = new List<Door>();
			foreach (var door in doors)
			{
				if (door.WallSegmentIndex == 0)
				{
					removeDoors.Add(door);
				}
				else
				{// move all door indices down one
					--door.WallSegmentIndex;
				}
			}

			foreach (var door in removeDoors)
			{
				UnbindDoor(door);
				Destroy(door.gameObject);
			}

			controlPoints.Remove(removeCtrlPnt);
			RemoveCollider(0);

			var points = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(points);

			for (int i = 0; i < points.Length - 1; ++i)
			{
				lineRenderer.SetPosition(i, points[i + 1]);
				--controlPoints[i].index;
			}

			--lineRenderer.positionCount;
			Destroy(removeCtrlPnt.gameObject);
		}
		else if (ctrlPntIndex == controlPoints.Count - 1)
		{
			var removeDoors = new List<Door>();
			foreach (var door in doors)
			{
				if (door.WallSegmentIndex == ctrlPntIndex - 1)
				{
					removeDoors.Add(door);
				}
			}

			foreach (var door in removeDoors)
			{
				UnbindDoor(door);
				Destroy(door.gameObject);
			}

			controlPoints.Remove(removeCtrlPnt);
			RemoveCollider(ctrlPntIndex - 1);
			--lineRenderer.positionCount;
			Destroy(removeCtrlPnt.gameObject);
		}
		else
		{
			var points = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(points);

			var saveDoors = new List<Door>();
			var doorsCopy = new List<Door>(doors);
			foreach (var door in doorsCopy)
			{
				if (door.WallSegmentIndex == ctrlPntIndex - 1 || door.WallSegmentIndex == ctrlPntIndex)
				{
					UnbindDoor(door);
					saveDoors.Add(door);
				}
			}

			controlPoints.Remove(removeCtrlPnt);
			RemoveCollider(ctrlPntIndex - 1);
			RemoveCollider(ctrlPntIndex - 1);


			for (int i = ctrlPntIndex; i < points.Length - 1; ++i)
			{
				lineRenderer.SetPosition(i, points[i + 1]);
				--controlPoints[i].index;
			}

			--lineRenderer.positionCount;

			CreateCollider(points[ctrlPntIndex - 1], points[ctrlPntIndex + 1], ctrlPntIndex - 1);
			Destroy(removeCtrlPnt.gameObject);

			foreach (var door in saveDoors)
			{
				BindDoor(door, colliders[ctrlPntIndex - 1].transform, door.transform.position);
			}

			// move all door indices down one if above removed
			foreach (var door in doors)
			{
				if (door.WallSegmentIndex > ctrlPntIndex)
					--door.WallSegmentIndex;
			}
		}

		VerifyIndexOrder(controlPoints);
		return false;
	}

	private WallControlPoint AddControlPoint(Vector3 mousePos)
	{
		var points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);
		if (!FindClosestPoint(mousePos, points, out Vector3 newPointPos, out int nearestPointAIndex))
			throw new Exception("This is trouble");
		++lineRenderer.positionCount;

		var newCtrlPnt = CreateControlPoint(newPointPos, nearestPointAIndex + 1);
		++controlPoints.Capacity;

		var insertIndex = nearestPointAIndex + 1;
		for (int i = insertIndex; i < points.Length; ++i)
		{
			lineRenderer.SetPosition(i + 1, points[i]);
			++controlPoints[i].index;
		}

		lineRenderer.SetPosition(insertIndex, newPointPos);
		controlPoints.Insert(insertIndex, newCtrlPnt);

		points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);

		var saveDoors = new List<Door>();
		var doorsCopy = new List<Door>(doors);
		foreach (var door in doorsCopy)
		{
			if (door.WallSegmentIndex > nearestPointAIndex)
				++door.WallSegmentIndex; // move all door indices down one if above inserted
			else if (door.WallSegmentIndex == nearestPointAIndex
				/*|| door.WallSegmentIndex == nearestPointAIndex + 1*/)
			{
				UnbindDoor(door);
				saveDoors.Add(door);
			}
		}

		RemoveCollider(nearestPointAIndex);

		CreateCollider(points[nearestPointAIndex + 1], points[nearestPointAIndex], nearestPointAIndex);
		CreateCollider(points[nearestPointAIndex + 2], points[nearestPointAIndex + 1], nearestPointAIndex + 1);

		foreach (var door in saveDoors)
		{
			FindClosestPoint(door.transform.position, points, out Vector3 nearestPoint, out int nearestLineIndex);
			BindDoor(door, colliders[nearestLineIndex].transform, nearestPoint);
		}

		return newCtrlPnt;
	}

	/// <summary>
	/// Creates a new control point at index in list but does NOT add to controlPoints[].
	/// </summary>
	/// <param name="cntrlPos"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	private WallControlPoint CreateControlPoint(Vector3 cntrlPos, int index)
	{
		var ctrlPntGameObject = Instantiate(DesignManager.GetPrefab(PrefabType.WallControlPointPrefab), transform, false);
		ctrlPntGameObject.transform.localPosition = cntrlPos;
		var ctrlPnt = ctrlPntGameObject.GetComponent<WallControlPoint>();
		ctrlPnt.index = index;
		ctrlPnt.wall = this;
		return ctrlPnt;
	}

	private void CreateCollider(Vector2 pointA, Vector2 pointB, int index)
	{
		var collider = Instantiate(DesignManager.GetPrefab(PrefabType.WallSegmentColliderPrefab), transform, false);
		var wallSegCollider = collider.GetComponent<WallSegmentCollider>();

		if (index < 0)
			throw new Exception("index under 0 is illogical!");
		wallSegCollider.Initialize(this, index);
		wallSegCollider.UpdatePosition(pointA, pointB);
		++colliders.Capacity;
		colliders.Insert(index, wallSegCollider);

		for (int i = index + 1; i < colliders.Count; ++i)
		{
			++colliders[i].index;
		}

		VerifyIndexOrder(colliders);
	}

	private static void VerifyIndexOrder(List<WallControlPoint> controlPoints)
	{
		for (int i = 0; i < controlPoints.Count; ++i)
			if (controlPoints[i].index != i)
				throw new Exception("wrong index!");
	}

	private static void VerifyIndexOrder(List<WallSegmentCollider> colliders)
	{
		for (int i = 0; i < colliders.Count; ++i)
		{
			if (colliders[i].index != i)
				throw new Exception("Index mismatch!");
		}
	}

	public void GetLineFromIndex(int segmentIndex,
		out Vector3 nearestLineStart, out Vector3 nearestLineEnd)
	{
		var points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);
		nearestLineStart = points[segmentIndex];
		nearestLineEnd = points[segmentIndex + 1];
	}

	public void FindClosestPoint(Vector3 worldPos, int segmentIndex, out Vector3 nearestPointOnLine,
		out float t, out Vector3 lineStart, out Vector3 lineEnd)
	{
		var points = new Vector3[lineRenderer.positionCount];
		lineRenderer.GetPositions(points);
		lineStart = points[segmentIndex];
		lineEnd = points[segmentIndex + 1];

		var line = lineEnd - lineStart;
		var length = line.magnitude;
		var dir = line.normalized;
		var lhs = worldPos - lineStart;
		float dotP = Vector2.Dot(lhs, dir);
		nearestPointOnLine = lineStart + dir * dotP;
		t = dotP / length;
	}

	private bool FindClosestPoint(Vector3 mousePos, Vector3[] lineRendererPoints,
		out Vector3 nearestPoint, out int nearestLineIndex)
	{
		nearestLineIndex = -1;
		nearestPoint = Vector3.zero;
		float shortestDistance = int.MaxValue;

		for (int i = 0; i < lineRendererPoints.Length - 1; ++i)
		{
			var origin = lineRendererPoints[i];
			var line = lineRendererPoints[i + 1] - origin;
			var length = line.magnitude;
			var dir = line.normalized;
			var lhs = mousePos - origin;
			float dotP = Vector2.Dot(lhs, dir);
			var nearest = origin + dir * dotP;
			var t = dotP / length;
			if (t < 0 || t > 1)
				continue;
			var dist = Vector2.Distance(nearest, mousePos);
			if (dist < shortestDistance)
			{
				shortestDistance = dist;
				nearestLineIndex = i;
				nearestPoint = nearest;
			}
		}

		return nearestLineIndex != -1;
	}

	private void RemoveCollider(int colliderIndex)
	{
		var oldCollider = colliders[colliderIndex].gameObject;
		if (colliders[colliderIndex].index != colliderIndex)
			throw new Exception("Index mismatch!");
		if (oldCollider.transform.childCount > 0)
			throw new Exception("Remove all doors attached to a collider before destroying it!");
		Destroy(oldCollider);
		colliders.RemoveAt(colliderIndex);
		--colliders.Capacity;

		for (int i = colliderIndex; i < colliders.Count; ++i)
		{
			--colliders[i].index;
			if (colliders[i].index != i)
				throw new Exception("Index mismatch!");
		}

		VerifyIndexOrder(colliders);
	}

	private float[] GetSegmentLengths(Vector3[] points)
	{
		var lengths = new float[points.Length - 1];
		for (int i = 0; i < points.Length - 1; ++i)
		{
			var dist = Vector3.Distance(points[i + 1], points[i]);
			lengths[i] = dist;
		}

		return lengths;
	}

	private float GetTotalLength(Vector3[] points)
	{
		float totalLength = 0;
		for (int i = 0; i < points.Length - 1; ++i)
		{
			var line = points[i + 1] - points[i];
			totalLength += line.magnitude;
		}

		return totalLength;
	}
}
