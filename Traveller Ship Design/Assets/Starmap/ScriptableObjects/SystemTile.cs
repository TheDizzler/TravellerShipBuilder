using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.MG2eTraveller.Starmap.SubSectorMap;


namespace AtomosZ.MG2eTraveller.Starmap.Tiles
{
	[CreateAssetMenu(fileName = "system", menuName = "Traveller/SystemTile")]
	public class SystemTile : TileBase
	{
		public SystemType type;

		public Sprite sprite;
	}
}