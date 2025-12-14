using System;
using UnityEngine;

namespace AtomosZ.DFDQ.Tiles.Widgets
{
	public class TerrainWidget : MonoBehaviour
	{
		public BoxCollider2D boxCollider;
		public GridTile tile;

		[SerializeField] private bool blocksLineOfSight = true;

		public void RemoveFromTile()
		{
			if (tile != null)
				tile.RemoveWidget();
		}


		public bool BlocksLineOfSight()
		{
			return blocksLineOfSight;
		}
	}
}