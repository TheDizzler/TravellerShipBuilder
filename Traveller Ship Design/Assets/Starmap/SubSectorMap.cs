using System;
using System.Collections.Generic;
using System.Linq;
using AtomosZ.MG2eTraveller.Starmap.Tiles;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.MG2eTraveller.Starmap.StarSystem;
using Random = UnityEngine.Random;

namespace AtomosZ.MG2eTraveller.Starmap
{
	[Serializable]
	public class SystemShaderData
	{
		public SystemHighlightState state;
		[ColorUsageAttribute(true, true)]
		public Color color;
		public float pulseSpeed;
		public float zPopOut;
		public float textGlowPower;
	}

	public class SubSectorMap : MonoBehaviour
	{
		public enum SystemType
		{
			Empty,

			// Stars
			Uniary,
			Binary,
			Trinary,

			// Structures. The are not necessarily visible on the map.
			FuelDump,
			Starbase,

			// Anomolies
			BlackHole,
		}

		public Tilemap tilemap;
		public List<SystemTile> systemTileBases = new();
		public StarSystem starPrefab;

		public CustomDictionary<SystemHighlightState, SystemShaderData> highlightData = new()
		{
			[SystemHighlightState.None] = new SystemShaderData
			{
				state = SystemHighlightState.None,
				color = Color.white,
				pulseSpeed = 0,
				zPopOut = 0,
				textGlowPower = 1,
			},
			[SystemHighlightState.MouseOver] = new SystemShaderData
			{
				state = SystemHighlightState.None,
				color = Color.lightBlue,
				pulseSpeed = 3,
				zPopOut = -0.03f,
				textGlowPower = 0.003f,
			},
			[SystemHighlightState.Selected] = new SystemShaderData
			{
				state = SystemHighlightState.None,
				color = Color.blueViolet,
				pulseSpeed = 1,
				zPopOut = -0.04f,
				textGlowPower = 0.005f,
			},
			[SystemHighlightState.SelectedMouseOver] = new SystemShaderData
			{
				state = SystemHighlightState.None,
				color = Color.blueViolet,
				pulseSpeed = 3,
				zPopOut = -0.04f,
				textGlowPower = 0.005f,
			},
		};

		public CustomDictionary<Vector3Int, StarSystem> systems;



		public string[] worldNameList;
		private List<string> worldNames;

		public void FillSubSector()
		{
			var text = Resources.Load("WorldNames").ToString();
			worldNameList = text.Split('\n');
			worldNames = worldNameList.ToList();

			ClearSubSector();
			for (int height = 0; height < 10; ++height)
			{
				for (int width = 0; width < 8; ++width)
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
					var translatedPos = new Vector3Int(-height, width - 1, 0);

					var system = Instantiate(starPrefab, transform);
					system.tilemap = tilemap;
					system.subSector = this;
					system.SetSystemData(translatedPos, systemData, worldName);
					if (tilemap.GetTile(translatedPos) != null)
						tilemap.SetTile(translatedPos, null);
					tilemap.SetTile(translatedPos, systemTileBases[randomSystemType]);
					systems.Add(translatedPos, system);
				}
			}

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

		public StarSystem GetSystemAt(Vector3Int pos)
		{
			systems.TryGetValue(pos, out var system);
			return system;
		}

		public StarSystem GetSystemAtWorldPosition(Vector2 pos)
		{
			var cell = GetCellAtWorldPosition(pos);
			return GetSystemAt(cell);
		}

		public StarSystem GetSystemUnderMouse()
		{
			var cell = GetCellUnderneathMouse();
			return GetSystemAt(cell);
		}

		public SystemTile GetTileAt(Vector3Int pos)
		{
			var tile = (SystemTile)tilemap.GetTile(pos);
			return tile;
		}

		public SystemTile GetTileAtWorldPosition(Vector2 worldPos)
		{
			var pos = GetCellAtWorldPosition(worldPos);
			var tile = (SystemTile)tilemap.GetTile(pos);
			return tile;
		}

		public SystemTile GetTileUnderneathMouse()
		{
			var pos = GetCellUnderneathMouse();
			var tile = (SystemTile)tilemap.GetTile(pos);
			return tile;
		}

		public Vector3Int GetCellAtWorldPosition(Vector2 worldPos)
		{
			Vector3Int cell = tilemap.WorldToCell(worldPos);
			return cell;
		}

		public Vector3Int GetCellUnderneathMouse()
		{
			Vector3 worldPos = Helpers.camera.ScreenToWorldPoint(Input.mousePosition);
			Vector3Int cell = GetCellAtWorldPosition(worldPos);
			return cell;
		}


	}
}