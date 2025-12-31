using System;
using System.Collections.Generic;
using System.Linq;
using AtomosZ.MG2eTraveller.Starmap.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.Helpers;
using static AtomosZ.MG2eTraveller.Starmap.SectorTilemap;
using Random = UnityEngine.Random;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class SubSectorMap : MonoBehaviour
	{

		public Tilemap tilemap;
		public CustomDictionary<Vector3Int, StarSystem> systems;
		public LineRenderer lineRenderer;



		public void SetBorderRenderer()
		{
			var width = Starmap.subSectorWidthInWorldUnits;
			float height = Starmap.subSectorHeightInWorldUnits;
			var leftMost = -(tilemap.cellSize.y * 1.125f);
			var topMost = (tilemap.cellSize.x * 1f);
			lineRenderer.SetPosition(0, new Vector3(leftMost, topMost));
			lineRenderer.SetPosition(1, new Vector3(width + leftMost, topMost));
			lineRenderer.SetPosition(2, new Vector3(width + leftMost, -(height - topMost)));
			lineRenderer.SetPosition(3, new Vector3(leftMost, -(height - topMost)));
		}

		public CustomDictionary<Vector3Int, StarSystem> FillSubSector(Vector3Int sectorOffsetInHexes,
			List<string> worldNames, List<SystemTile> systemTileBases, StarSystem starPrefab)
		{
			ClearSubSector();
			for (int height = 0; height < Starmap.instance.subSectorHeightInHexes; ++height)
			{
				for (int width = 0; width < Starmap.instance.subSectorWidthInHexes; ++width)
				{
					var randomSystemType = Random.Range(0, 2);
					var systemData = systemTileBases[randomSystemType];
					string worldName = null;
					if (systemData.type != SystemType.Empty)
					{
						var randomWorldName = Random.Range(0, worldNames.Count - 1);
						worldName = worldNames[randomWorldName];
						worldNames.RemoveAt(randomWorldName);
					}

					var subsectorPos = new Vector3Int(-height, width - 1, 0);
					var sectorPos = new Vector3Int(subsectorPos.x + sectorOffsetInHexes.y, subsectorPos.y + sectorOffsetInHexes.x);

					var system = Instantiate(starPrefab, transform);
					system.tilemap = tilemap;
					system.subSector = this;
					system.SetSystemData(sectorPos, systemData, worldName);
					if (tilemap.GetTile(sectorPos) != null)
						tilemap.SetTile(sectorPos, null);
					tilemap.SetTile(sectorPos, systemTileBases[randomSystemType]);
					systems.Add(sectorPos, system);
				}
			}

			SetBorderRenderer();

			return systems;
		}

		public void ClearSubSector()
		{
			foreach (var system in systems)
			{
#if DEBUG
				if (!Application.isPlaying)
				{
					DestroyImmediate(system.Value.gameObject);
					continue;
				}
#endif

				Destroy(system.Value.gameObject);
			}

			systems.Clear();
			tilemap.ClearAllTiles();
		}
	}
}