using System;
using System.Collections.Generic;
using System.Linq;
using AtomosZ.ShaderTools;
using AtomosZ.UI;
using UnityEngine;

using static AtomosZ.Keyboard;

namespace AtomosZ.MG2eTraveller.Starmap
{
	[Serializable]
	public class SystemCoordinates
	{
		public int x, y;

		public SystemCoordinates(int x, int y)
		{
			this.x = y;
			this.y = x;
		}
		//public Vector3Int tilemapCoordinates;
		//public Vector2Int subSectorCoordinates;
		//public SubSectorMap subSector;
		//public Vector2Int sectorCoordinates;
		//public Sector

		public static SystemCoordinates operator +(SystemCoordinates left, Vector3Int vec)
		{
			SystemCoordinates sysCoods = new SystemCoordinates(
				left.x + vec.x, left.y + vec.y);
			return sysCoods;
		}


		public Vector3Int ConvertToTilemapCoordinates()
		{
			return new Vector3Int(-(x - 1), y - 2);
		}
	}

	[ExecuteInEditMode]
	public class Starmap : MonoBehaviour
	{
		private static Starmap _instance;
		public static Starmap instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<Starmap>();
				return _instance;
			}
		}


		//public SubSectorMap subSector;
		public MagicWindow systemWindow;


		private Vector3 scrollStartPos;


		public ISelectable hoveredObject;
		public ISelectable selectedObject;
		public StarSystem selectedSystem;
		public Fleet selectedFleet;

		public JumpPathRenderer jumpPathRenderer;

		public Color inJumpRangeColor = new Color(0, 1, 1, .5f);

		public FleetManager fleetManager;
		[SerializeField] private SectorTilemap sector;


		[SerializeField] private int minZoom = -2;
		[SerializeField] private int maxZoom = -30;
		[Range(0.2f, 3.0f)]
		[SerializeField] private float scrollMultiplier = 1.1f;
		[Range(0.1f, 3.0f)]
		[SerializeField] private float zoomMultiplier = 1.0f;

		public enum InteractionState
		{
			None,
			MouseOver,
			Selected,
			SelectedMouseOver,
		}

		public enum MouseMode
		{
			None,
			SelectDestination,
		}
		private MouseMode mouseMode;

		[Serializable]
		public class SelectableShaderData
		{
			public InteractionState state;
			[ColorUsageAttribute(true, true)]
			public Color color;
			public float pulseSpeed;
			public float zPopOut;
		}

		[Serializable]
		public class SystemShaderData : SelectableShaderData
		{
			public float textGlowPower;
			public float thickness;
		}

		public CustomDictionary<InteractionState, SystemShaderData> systemHighlightData = new()
		{
			[InteractionState.None] = new SystemShaderData
			{
				state = InteractionState.None,
				color = new Color(1, 0, 1, 0),
				pulseSpeed = 0,
				zPopOut = 0,
				textGlowPower = 1,
				thickness = .005f,
			},
			[InteractionState.MouseOver] = new SystemShaderData
			{
				state = InteractionState.None,
				color = Color.lightBlue,
				pulseSpeed = 3,
				zPopOut = -0.03f,
				textGlowPower = 0.003f,
				thickness = .01f,
			},
			[InteractionState.Selected] = new SystemShaderData
			{
				state = InteractionState.None,
				color = Color.blueViolet,
				pulseSpeed = 1,
				zPopOut = -0.04f,
				textGlowPower = 0.005f,
				thickness = .015f,
			},
			[InteractionState.SelectedMouseOver] = new SystemShaderData
			{
				state = InteractionState.None,
				color = Color.blueViolet,
				pulseSpeed = 3,
				zPopOut = -0.04f,
				textGlowPower = 0.005f,
				thickness = .02f,
			},
		};

		public CustomDictionary<InteractionState, SelectableShaderData> fleetHighlightData = new()
		{
			[InteractionState.None] = new SelectableShaderData
			{
				state = InteractionState.None,
				color = new Color(1, 0, 1, 0),
				pulseSpeed = 0,
				zPopOut = 0,
			},
			[InteractionState.MouseOver] = new SelectableShaderData
			{
				state = InteractionState.None,
				color = Color.lightBlue,
				pulseSpeed = 3,
				zPopOut = -0.03f,
			},
			[InteractionState.Selected] = new SelectableShaderData
			{
				state = InteractionState.None,
				color = Color.blueViolet,
				pulseSpeed = 1,
				zPopOut = -0.04f,
			},
			[InteractionState.SelectedMouseOver] = new SelectableShaderData
			{
				state = InteractionState.None,
				color = Color.blueViolet,
				pulseSpeed = 3,
				zPopOut = -0.04f,
			},

		};



		private LayerMask _tileLayer = int.MinValue;
		public static LayerMask tileLayer
		{
			get
			{
				if (instance._tileLayer == int.MinValue)
					instance._tileLayer = LayerMask.GetMask("TerrainTiles");
				return instance._tileLayer;
			}
		}
		public static LayerMask GetLayerMask(string layerName)
		{
			return LayerMask.GetMask(layerName);
		}

		public int subSectorWidthInHexes = 8;
		public int subSectorHeightInHexes = 10;

		public static float subSectorWidthInWorldUnits
		{
			get
			{
				if (instance.subSectorWidthInHexes % 2 == 0)
					return
						   (instance.sector.tilemap.cellSize.y * .75f * (instance.subSectorWidthInHexes));
				else
					return instance.sector.tilemap.cellSize.y * 1.5f
						   + (instance.sector.tilemap.cellSize.y * .75f * (instance.subSectorWidthInHexes - 2));
			}
		}

		public static float subSectorHeightInWorldUnits
		{
			get
			{
				return instance.sector.tilemap.cellSize.x * instance.subSectorHeightInHexes;
			}
		}


		void Start()
		{
			SpriteOutlineCreator.spriteDictionary.Clear();
		}

		public Vector2Int sectorSize = new Vector2Int(2, 2);

		public void GenerateSector()
		{
			sector.GenerateSector(sectorSize);
		}

		void Update()
		{
			Vector3 mouseWorldPos = Helpers.GetMouseWorldPos();
			ModifierKey modifierKeys = GetModifierKeyInput();



			// 
			// General Interaction flow
			//		One fleet and one system can be selected at the same time, but only one object is hoverable.
			//
			// check for fleet hover
			// if found fleet hover 
			//		check for select
			// else check for system hover
			// if system hover
			//		check for select
			var mouseCell = sector.GetCellAtWorldPosition(mouseWorldPos);
			var mouseSystem = GetSystemAt(mouseCell);
			LayerMask layer = LayerMask.GetMask("Fleet");
			var fleetCollider = Physics2D.OverlapPoint(mouseWorldPos, layer);
			{
				ISelectable newHoveredObject = null;
				StarSystem systemHovered = null;
				if (fleetCollider != null)
				{
					var fleetHovered = fleetCollider.GetComponent<Fleet>();
					newHoveredObject = fleetHovered;
				}
				else
				{
					systemHovered = GetSystemAt(mouseCell);
					newHoveredObject = systemHovered;
				}

				if (selectedFleet != null)
				{
					var fleetCell = fleetManager.GetCellOf(selectedFleet);
					ShowJumpRange(fleetCell, selectedFleet.jDrive);

					if (systemHovered != null)
						jumpPathRenderer.DrawLine(fleetCell,  systemHovered.cellCoordinates, selectedFleet.jDrive);
					else
						jumpPathRenderer.Hide();
				}
				else
					HideJumpPath();



				if (newHoveredObject != hoveredObject)
				{
					if (hoveredObject != null)
					{
						hoveredObject.SetInteractionState(InteractionState.None);
					}

					hoveredObject = newHoveredObject;
					if (newHoveredObject != null)
					{
						hoveredObject.SetInteractionState(InteractionState.MouseOver);
					}
				}
			}


			if (Input.GetMouseButtonDown(0)
				&& hoveredObject != selectedObject)
			{
				if (selectedObject != null)
				{
					selectedObject.SetInteractionState(InteractionState.None);
				}

				if (hoveredObject != null)
				{
					var newSelectedObject = hoveredObject;
					newSelectedObject.SetInteractionState(InteractionState.Selected);
				}

				selectedObject = hoveredObject;

				if (fleetCollider != null)
				{
					selectedFleet = (Fleet)selectedObject;
					mouseMode = MouseMode.SelectDestination;
				}
				else if (mouseSystem != null)
				{
					selectedSystem = (StarSystem)selectedObject;
					DisplaySystemData(selectedSystem);
					mouseMode = MouseMode.None;
				}
				else
				{
					mouseMode = MouseMode.None;
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				//EndScroll();
			}
			else if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.Escape))
			{
				mouseMode = MouseMode.None;
				if (selectedObject != null)
				{
					selectedObject = null;
				}

				if (selectedFleet != null)
				{
					selectedFleet.SetInteractionState(InteractionState.None, true);
					selectedFleet = null;
				}

				if (selectedSystem != null)
				{
					selectedSystem.SetInteractionState(InteractionState.None, true);
					selectedSystem = null;
				}
			}
			if (Input.GetMouseButtonDown(2))
			{
				scrollStartPos = mouseWorldPos;
			}
			else if (Input.GetMouseButton(2))
			{
				var diff = mouseWorldPos - scrollStartPos;
				var newX = Helpers.camera.transform.position.x - diff.x;
				var newY = Helpers.camera.transform.position.y - diff.y;
				Helpers.camera.transform.position = new Vector3(newX, newY, Helpers.camera.transform.position.z);
				mouseWorldPos = Helpers.GetMouseWorldPos();
				scrollStartPos = mouseWorldPos;
			}

			if (Input.mouseScrollDelta != Vector2.zero)
			{
				if ((modifierKeys & ModifierKey.Ctrl) == ModifierKey.Ctrl)
				{
					var newY = Helpers.camera.transform.position.y + Input.mouseScrollDelta.y * scrollMultiplier;
					Helpers.camera.transform.position = new Vector3(
						Helpers.camera.transform.position.x, newY, Helpers.camera.transform.position.z);
				}
				else if ((modifierKeys & ModifierKey.Shift) == ModifierKey.Shift)
				{
					var newX = Helpers.camera.transform.position.x + Input.mouseScrollDelta.y * scrollMultiplier;
					Helpers.camera.transform.position = new Vector3(
						newX, Helpers.camera.transform.position.y, Helpers.camera.transform.position.z);
				}
				else
				{
					float extraZoom = ((modifierKeys & ModifierKey.Alt) == ModifierKey.Alt) ? 5.0f : 1.0f;
					var newZ = Helpers.camera.transform.position.z + Input.mouseScrollDelta.y * zoomMultiplier * extraZoom;
					if (newZ >= minZoom)
						newZ = minZoom;
					else if (newZ < maxZoom)
						newZ = maxZoom;
					Helpers.camera.transform.position = new Vector3(
						Helpers.camera.transform.position.x, Helpers.camera.transform.position.y, newZ);
				}
			}
		}

		private void DisplaySystemData(StarSystem system)
		{
			var title = (UIExpandingLabel)systemWindow.GetControl("titlebar");
			title.text = system.worldName;
			var locationLabel = (UIExpandingLabel)systemWindow.GetControl("location_label");

			locationLabel.text = system.GetStringCoordinates();

			var fleetPanel = (UIPanel)systemWindow.GetControl("fleet_panel");
			fleetPanel.ClearControls();

			var fleets = fleetManager.GetFleetsAt(system);
			foreach (var fleet in fleets)
			{
				fleetPanel.AddText_("•" + fleet.name);
			}
		}


		public void ShowJumpRange(SystemCoordinates startPos, int jump)
		{
			var centerPos = startPos.ConvertToTilemapCoordinates();
			ShowJumpRange(centerPos, jump);
		}

		public void ShowJumpRange(Vector3Int startCell, int jump)
		{
			var centerSystem = GetSystemAt(startCell);
			centerSystem.SetBackground(new Color(1, 0, 0));

			for (int q = -jump; q <= jump; ++q)
			{
				int r = Mathf.CeilToInt(-jump + (Mathf.Abs(q) / 2.0f));
				int maxR = Mathf.CeilToInt(jump - (Mathf.Abs(q) / 2.0f));
				//Debug.Log($"{q}, ({r} => {maxR})");
				for (; r <= maxR; ++r)
				{
					//var nextSystemCoord = startPos + new Vector3Int(r, q);
					//var nextPos = nextSystemCoord.ConvertToTilemapCoordinates();
					var nextPos = new Vector3Int(startCell.x + r, startCell.y + q);
					var nextSystem = GetSystemAt(nextPos);
					if (nextSystem != null)
					{
						nextSystem.SetBackground(inJumpRangeColor);
					}
				}
			}
		}

		public void DrawJumpPath(SystemCoordinates startSystem, SystemCoordinates destSystem, int jump)
		{
			jumpPathRenderer.DrawLine(startSystem.ConvertToTilemapCoordinates(), destSystem.ConvertToTilemapCoordinates(), jump);
		}

		public void DrawJumpPath(Vector3Int startCell, Vector3Int destCell, int jump)
		{
			jumpPathRenderer.DrawLine(startCell, destCell, jump);
		}

		public void HideJumpPath()
		{
			jumpPathRenderer.Hide();
		}

		public static Vector3Int ConvertToTilemapCoordinates(SystemCoordinates systemCoords)
		{
			return systemCoords.ConvertToTilemapCoordinates();
		}


		public StarSystem GetSystemAt(Vector3Int pos)
		{
			return sector.GetSystemAtCell(pos);
		}

		public StarSystem GetSystemAtWorldPosition(Vector2 pos)
		{
			return sector.GetSystemAtWorldPos(pos);
		}

		public StarSystem GetSystemUnderMouse()
		{
			return sector.GetSystemUnderMouse();
		}


		public void FleetEnteredSystem(StarSystem starSystem, Fleet fleet)
		{
			fleetManager.FleetEnteredSystem(starSystem, fleet);
		}
	}
}