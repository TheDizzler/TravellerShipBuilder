using System;
using System.Collections.Generic;
using AtomosZ.DFDQ.Battle;
using UnityEngine;

namespace AtomosZ.DFDQ.Tiles
{
	public class PathLineRenderer : MonoBehaviour
	{
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField] private Transform head;
		[SerializeField] private Transform tail;

		private Vector2Int startPos;

		public void ClearPath(Vector2Int startPosition)
		{
			startPos = startPosition;
			lineRenderer.positionCount = 1;
			var startVec = new Vector3(startPos.x, startPos.y, 0);
			lineRenderer.SetPosition(0, startVec);
			head.position = startVec;
			tail.position = startVec;
		}

		private void SetWaypoint(Vector2Int point)
		{
			lineRenderer.positionCount += 1;
			var nextVec = new Vector3(point.x, point.y, 0);
			lineRenderer.SetPosition(lineRenderer.positionCount - 1, nextVec);
			head.position = nextVec;

			var lastPoint = lineRenderer.GetPosition(lineRenderer.positionCount - 2);
			if (lastPoint.x > point.x) // went left
				head.rotation = Quaternion.Euler(0, 0, 90);
			else if (lastPoint.x < point.x)
				head.rotation = Quaternion.Euler(0, 0, -90);
			else if (lastPoint.y > point.y)
				head.rotation = Quaternion.Euler(0, 0, -180);
			else if (lastPoint.y < point.y)
				head.rotation = Quaternion.Euler(0, 0, 0);

			var secondPoint = lineRenderer.GetPosition(1);
			if (secondPoint.x < startPos.x)
				tail.rotation = Quaternion.Euler(0, 0, 90);
			else if (secondPoint.x > startPos.x)
				tail.rotation = Quaternion.Euler(0, 0, -90);
			else if (secondPoint.y < startPos.y)
				tail.rotation = Quaternion.Euler(0, 0, -180);
			else if (secondPoint.y > startPos.y)
				tail.rotation = Quaternion.Euler(0, 0, 0);
		}

		/// <summary>
		///  IMPORTANT(Tristan): The path is in reverse order, from goal to start.
		/// </summary>
		/// <param name="path">The path, with the start pos at the <i>LAST</i> index and the goal at the <i>FIRST</i> index.
		/// If null, deactivates the path gameObject.</param>
		/// <param name="movePoints">0 or lower means infinite movement.</param>
		public void SetPath(List<GridManager.AStarNode> path, int movePoints = 0)
		{
			if (path == null)
			{
				gameObject.SetActive(false);
				return;
			}

			if (movePoints <= 0)
				movePoints = int.MaxValue;
			//Debug.LogWarning("New path");
			gameObject.SetActive(true);
			ClearPath(path[path.Count - 1].tile.position);
			for (int i = path.Count - 2; i >= 0; --i)
			{
				movePoints -= path[i].tile.movementCost;
				if (movePoints < 0)
					break;
				SetWaypoint(path[i].tile.position);
				if (movePoints == 0)
					break;
			}
		}
	}
}