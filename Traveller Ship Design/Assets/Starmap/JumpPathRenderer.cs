using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class JumpPathRenderer : MonoBehaviour
	{
		[SerializeField] private Tilemap tilemap;
		[SerializeField] private LineRenderer lineRenderer;
		[SerializeField] private Transform head;
		[SerializeField] private Transform tail;
		[SerializeField] private GameObject cross;

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

			var secondPoint = lineRenderer.GetPosition(1);
			if (secondPoint.x < startPos.x)
				head.rotation = tail.rotation = Quaternion.Euler(0, 0, 90);
			else if (secondPoint.x > startPos.x)
				head.rotation = tail.rotation = Quaternion.Euler(0, 0, -90);
			else if (secondPoint.y < startPos.y)
				head.rotation = tail.rotation = Quaternion.Euler(0, 0, -180);
			else if (secondPoint.y > startPos.y)
				head.rotation = tail.rotation = Quaternion.Euler(0, 0, 0);
		}


		public void DrawLine(Vector3Int source, Vector3Int target, int range)
		{
			gameObject.SetActive(true);

			var srcWorld = tilemap.CellToWorld(source);
			var trgWorld = tilemap.CellToWorld(target);
			lineRenderer.SetPosition(0, srcWorld);
			lineRenderer.SetPosition(1, trgWorld);


			tail.position = srcWorld;
			head.position = trgWorld;

			Vector3 dir = trgWorld - srcWorld;
			var rot = Quaternion.LookRotation(Vector3.back, dir);
			head.rotation = tail.rotation = Quaternion.Inverse(rot);

			var distance = Helpers.Distance(source, target);
			if (distance > range)
			{
				cross.SetActive(true);
				cross.transform.position = head.position;
			}
			else
			{
				cross.SetActive(false);
			}
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}