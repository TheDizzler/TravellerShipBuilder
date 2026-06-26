using UnityEngine;

namespace AtomosZ.Interceptor
{
	public class TileBase : MonoBehaviour
	{
		public enum TileType
		{
			Terrain,
			Structure,
		}

		public TileType tileType;
	}
}