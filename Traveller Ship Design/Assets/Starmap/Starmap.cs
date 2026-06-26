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

	//[ExecuteInEditMode]
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

		[Serializable]
		public class ImperialDate
		{
			internal float second;
			public int minute;
			public int hour;
			public int day = 0;
			public int year = 1105;

			internal void Reset(int startDay, int startYear)
			{
				second = 0;
				minute = 0;
				day = startDay;
				year = startYear;
			}

			public string GetFormattedDateTime()
			{
				return $"{hour.ToString("00")}h{minute.ToString("00")} {day.ToString("000")}-{year}";
			}

			public ImperialDate LogDate()
			{
				return new ImperialDate
				{
					day = day,
					hour = hour,
					minute = minute,
					second = second,
					year = year,
				};
			}
		}

		/// <summary>
		/// This gets nuked on a rebuild.
		/// </summary>
		public ImperialDate currentDate;


		public MagicWindow selectedSystemWindow;
		public MagicWindow hoveredSystemWindow;
		public MagicWindow fleetWindow;
		public MagicWindow timerWindow;
		public MagicWindow systemHistoryWindow;
		public UIExpandingLabel timer;


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
			selectedSystemWindow.Hide();
			hoveredSystemWindow.Hide();
			fleetWindow.Hide();

			currentDate = new();
			currentDate.Reset(0, 1105);


			Time.timeScale = 0;
		}

		public Vector2Int sectorSize = new Vector2Int(2, 2);

		public void GenerateSector()
		{
			sector.GenerateSector(sectorSize);
		}

		public enum TimerSpeed
		{
			Paused,
			Normal,
			Fast,
		}
		public TimerSpeed timerSpeed = TimerSpeed.Paused;



		public void Pause(UIButton caller)
		{
			if (timerSpeed == TimerSpeed.Paused)
				return;
			Time.timeScale = 0;
			timer.text = currentDate.GetFormattedDateTime();
			timerSpeed = TimerSpeed.Paused;
			caller.spriteColor = Color.white;
			((UIButton)timerWindow.GetControl("play_button")).spriteColor = Color.black;
			((UIButton)timerWindow.GetControl("fastForward_button")).spriteColor = Color.black;
		}

		public void Play(UIButton caller)
		{
			if (timerSpeed == TimerSpeed.Normal)
				return;
			Time.timeScale = 1;
			timerSpeed = TimerSpeed.Normal;
			caller.spriteColor = Color.white;
			((UIButton)timerWindow.GetControl("pause_button")).spriteColor = Color.black;
			((UIButton)timerWindow.GetControl("fastForward_button")).spriteColor = Color.black;
		}

		public void FastForward(UIButton caller)
		{
			if (timerSpeed == TimerSpeed.Fast)
				return;
			Time.timeScale = 10;
			timerSpeed = TimerSpeed.Fast;
			caller.spriteColor = Color.white;
			((UIButton)timerWindow.GetControl("pause_button")).spriteColor = Color.black;
			((UIButton)timerWindow.GetControl("play_button")).spriteColor = Color.black;
		}

		public float secondInterval = 1;
		public int baseMinuteIncrement = 5;
		private void UpdateClock()
		{
			currentDate.second += Time.deltaTime;
			if (currentDate.second >= secondInterval)
			{
				currentDate.second -= secondInterval;
				currentDate.minute += baseMinuteIncrement;

				if (currentDate.minute >= 60)
				{
					currentDate.minute -= 60;
					++currentDate.hour;
					if (currentDate.hour >= 24)
					{
						currentDate.hour -= 24;
						++currentDate.day;
						if (currentDate.day >= 365)
						{
							currentDate.day -= 365;
							++currentDate.year;
						}
					}
				}

				timer.text = currentDate.GetFormattedDateTime();
			}
		}


		void Update()
		{
			UpdateClock();

			Vector3 mouseWorldPos = Helpers.GetMouseWorldPos();
			ModifierKey modifierKeys = GetModifierKeyInput();


			bool validDestination = false;

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

					if (systemHovered != null && systemHovered.cellCoordinates != fleetCell)
					{
						jumpPathRenderer.DrawLine(fleetCell, systemHovered.cellCoordinates, selectedFleet.jDrive);
						validDestination = DisplayHoveredSystem(systemHovered, fleetCell, selectedFleet.jDrive);
					}
					else
					{
						jumpPathRenderer.Hide();
						hoveredSystemWindow.Hide();
					}
				}
				else
				{
					HideJumpPath();
					hoveredSystemWindow.Hide();
				}



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


			if (Input.GetMouseButtonDown(0))
			{
				if (validDestination)
				{   // ship will enter jump space
					selectedFleet.SpoolUpJDrive((StarSystem)hoveredObject);
					DeselectFleet();
				}
				else if (hoveredObject != selectedObject)
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
						DisplayFleetData(selectedFleet);
					}
					else if (mouseSystem != null)
					{
						selectedSystem = (StarSystem)selectedObject;
						DisplaySystemData(selectedSystem);
					}
				}
			}
			else if (Input.GetMouseButtonUp(0))
			{
				//EndScroll();
			}

			if (Input.GetMouseButtonDown(1) || Input.GetKey(KeyCode.Escape))
			{
				if (selectedObject != null)
				{
					selectedObject = null;
				}

				if (selectedFleet != null)
				{
					DeselectFleet();
				}

				if (selectedSystem != null)
				{
					selectedSystem.SetInteractionState(InteractionState.None, true);
					selectedSystem = null;
					selectedSystemWindow.gameObject.SetActive(false);
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


			// map scrolling/zooming
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

		private void DeselectFleet()
		{
			selectedFleet.SetInteractionState(InteractionState.None, true);
			selectedFleet = null;
			fleetWindow.gameObject.SetActive(false);
		}

		private void DisplayFleetData(Fleet fleet)
		{
			fleetWindow.gameObject.SetActive(true);
			((UIExpandingLabel)fleetWindow.GetControl("shipName_label")).text = fleet.name;
			((UIExpandingLabel)fleetWindow.GetControl("jDrive_label")).text = fleet.jDrive + "";
			((UIExpandingLabel)fleetWindow.GetControl("fuelCapacity_label")).text = fleet.fuelCapacity + "";
			((UIExpandingLabel)fleetWindow.GetControl("fuelCurrent_label")).text = fleet.fuelCurrent + "";
		}

		private bool DisplayHoveredSystem(StarSystem hoveredSystem, Vector3Int fleetCell, int maxJump)
		{
			hoveredSystemWindow.Show();
			hoveredSystemWindow.SetTitle(hoveredSystem.worldName);
			//((UIExpandingLabel)hoveredSystemWindow.GetControl("uwp_label")).text = system.uwp;
			((UIExpandingLabel)hoveredSystemWindow.GetControl("coords_label")).text = hoveredSystem.GetStringCoordinates();

			var distanceLabel = (UIExpandingLabel)hoveredSystemWindow.GetControl("distance_label");
			var dist = Helpers.Distance(fleetCell, hoveredSystem.cellCoordinates);
			distanceLabel.text = "Jump " + dist;
			bool validDestination;
			if (dist > maxJump)
			{
				validDestination = false;
				distanceLabel.color = new Color(1, 0, 0);
			}
			else
			{
				validDestination = true;
				distanceLabel.color = new Color(1, 1, 1);
			}
			//((UIExpandingLabel)hoveredSystemWindow.GetControl("fuelCost_label")).text = ;

			return validDestination;
		}


		private void DisplaySystemData(StarSystem system)
		{
			selectedSystemWindow.gameObject.SetActive(true);
			if (system.systemData.type == SectorTilemap.SystemType.Empty)
			{
				selectedSystemWindow.SetTitle("Empty Space");
			}
			else
			{
				var title = (UIExpandingLabel)selectedSystemWindow.GetControl("titlebar");
				title.text = system.worldName;
			}

			var locationLabel = (UIExpandingLabel)selectedSystemWindow.GetControl("location_label");

			locationLabel.text = system.GetStringCoordinates();

			var fleetPanel = (UIPanel)selectedSystemWindow.GetControl("fleet_panel");
			fleetPanel.ClearControls();

			//var fleets = fleetManager.GetFleetsAt(system);
			//if (fleets.Count == 0)
			//{
			//	fleetPanel.AddText_("No known fleets in system");
			//}

			//foreach (var fleet in fleets)
			//{
			//	fleetPanel.AddText_("•" + fleet.name);
			//}

			systemHistoryWindow.ClearControls();
			var historyTable = systemHistoryWindow.AddTable();
			historyTable.Init(0, 0);
			historyTable.AddColumn("Ship Transponder ID", true);
			historyTable.AddColumn("Entered System Date", true);
			historyTable.AddColumn("Exit System Date", true);



			//system.fleetHistoryLog
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