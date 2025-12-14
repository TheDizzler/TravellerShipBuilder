using System.Collections.Generic;
using AtomosZ.DFDQ.Battle;
using UnityEngine;

namespace AtomosZ.DFDQ.Tiles
{
	public class LineOfSightRenderer : MonoBehaviour
	{
		[SerializeField] private SpriteRenderer spriteRenderer;
		public bool hasLOS;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <param name="range">0 or less means infinite range.</param>
		/// <returns></returns>
		public bool HasLineOfSight(Vector2Int source, Vector2Int target, int range)
		{
			gameObject.SetActive(true);
			Vector3 sourceVec3 = new Vector3(source.x, source.y, 0);
			Vector2 direction = target - source;
			var length = direction.magnitude;
			transform.SetPositionAndRotation(sourceVec3,
				Quaternion.LookRotation(Vector3.forward, new Vector3(direction.x, direction.y)));


			RaycastHit2D[] rays = Physics2D.RaycastAll(source, direction, length, GridManager.tileLayer);
			hasLOS = true;
			GridTile blockingTile = null;
			foreach (var ray in rays)
			{
				GridTile tile = ray.transform.GetComponent<GridTile>();
				if (tile.BlocksLineOfSight()
					|| (tile.occupiedUnit != null && tile.occupiedUnit.BlocksLineOfSight())
					|| (tile.widget != null && tile.widget.BlocksLineOfSight()))
				{
					hasLOS = false;
					length = (tile.position - source).magnitude;
					blockingTile = tile;
					break;
				}
			}

			if (range > 0)
			{
				var manhattan = Mathf.Abs(direction.x) + Mathf.Abs(direction.y);
				if (manhattan > range)
				{
					hasLOS = false;
					length = range;
				}
			}

			spriteRenderer.size = new Vector2(spriteRenderer.size.x, length);

			if (hasLOS)
			{
				spriteRenderer.color = Color.white;
				Debug.DrawRay(sourceVec3, direction, Color.green);
			}
			else
			{
				spriteRenderer.color = Color.red;
				Debug.DrawRay(sourceVec3, direction, Color.red);
			}

			return hasLOS;
		}

		public void Hide()
		{
			gameObject.SetActive(false);
		}
	}
}