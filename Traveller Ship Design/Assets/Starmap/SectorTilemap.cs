using System;
using System.Collections.Generic;
using System.Linq;
using AtomosZ.MG2eTraveller.Starmap.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.MG2eTraveller.Starmap
{
	/// <summary>
	/// This will be a service layer between our game and Tilemap.
	/// </summary>
	public class SectorTilemap : MonoBehaviour
	{
		public enum SystemType
		{
			Empty,

			// Stars
			Uniary,
			Binary,
			Trinary,

			// Structures. These are not necessarily visible on the map.
			FuelDump,
			Starbase,

			// Anomolies
			BlackHole,
			RoguePlanet,
		}
		public List<SystemTile> systemTileBases = new();
		public StarSystem starPrefab;
		public SubSectorMap subSectorPrefab;


		public Tilemap tilemap;

		public Transform sectorTransform;
		//[HideInInspector]
		public List<SubSectorMap> subsectors = new();
		public CustomDictionary<Vector3Int, StarSystem> systems;


		public void GenerateSector(Vector2Int sectorSize)
		{
			foreach (var subsector in subsectors)
			{
#if DEBUG
				if (!Application.isPlaying)
				{
					DestroyImmediate(subsector.gameObject);
					continue;
				}
#endif
				Destroy(subsector.gameObject);
			}

			subsectors.Clear();
			systems.Clear();
#if DEBUG
			foreach (var subsector in GetComponentsInChildren<SubSectorMap>())
			{
				if (!Application.isPlaying)
					DestroyImmediate(subsector.gameObject);
				else
					Destroy(subsector.gameObject);
			}
#endif

			var text = Resources.Load("WorldNames").ToString();
			var worldNameList = text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
			var worldNames = worldNameList.ToList();

			for (int j = 0; j < sectorSize.y; ++j)
			{
				for (int i = 0; i < sectorSize.x; ++i)
				{
					var sectorOffset = new Vector3Int(i * Starmap.instance.subSectorWidthInHexes, -j * Starmap.instance.subSectorHeightInHexes);
					SubSectorMap subsector = Instantiate(subSectorPrefab, sectorTransform);
					subsector.name = $"([{i}], [{j}]) SubSector";
					subsector.tilemap = tilemap;
					subsector.transform.localPosition = new Vector3(i * Starmap.subSectorWidthInWorldUnits, -j * Starmap.subSectorHeightInWorldUnits);
					var subSectorSystems = subsector.FillSubSector(sectorOffset, worldNames, systemTileBases, starPrefab);

					subsectors.Add(subsector);
					systems.AddRange(subSectorSystems);
				}
			}

			tilemap.CompressBounds();
		}

		public StarSystem GetSystemAtWorldPos(Vector3 worldPos)
		{
			var cell = GetCellAtWorldPosition(worldPos);
			return GetSystemAtCell(cell);
		}

		public StarSystem GetSystemAtCell(Vector3Int cell)
		{
			systems.TryGetValue(cell, out var system);
			return system;
		}

		public SystemTile GetTileAt(Vector3Int cell)
		{
			var tile = (SystemTile)tilemap.GetTile(cell);
			return tile;
		}


		public SystemTile GetTileAtWorldPosition(Vector2 worldPos)
		{
			var pos = GetCellAtWorldPosition(worldPos);
			var tile = (SystemTile)tilemap.GetTile(pos);
			return tile;
		}

		public SystemTile GetTileUnderMouse()
		{
			var pos = GetCellUnderMouse();
			var tile = (SystemTile)tilemap.GetTile(pos);
			return tile;
		}

		public Vector3Int GetCellAtWorldPosition(Vector2 worldPos)
		{
			Vector3Int cell = tilemap.WorldToCell(worldPos);
			return cell;
		}

		public Vector3Int GetCellUnderMouse()
		{
			Vector3 worldPos = Helpers.camera.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cell = GetCellAtWorldPosition(worldPos);
			return cell;
		}

		public StarSystem GetSystemUnderMouse()
		{
			var cell = GetCellUnderMouse();
			return GetSystemAtCell(cell);
		}
	}
}