using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class FleetManager : MonoBehaviour
	{
		public static readonly Vector3Int jumpSpaceCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		public static readonly Vector3 jumpSpace = new Vector3(-1000, -1000, -1000);
		[Tooltip("A fleet in jump space has a Vector3Int == jumpSpaceCell")]
		public CustomDictionary<Fleet, Vector3Int> fleetLocations;

		private static FleetManager _instance;
		public static FleetManager instance
		{
			get
			{
				if (_instance == null)
					_instance = Helpers.GetSingleton<FleetManager>();
				return _instance;
			}
		}

		void Awake()
		{
			fleetLocations.Clear();
		}

		public void FleetEnteredSystem(StarSystem starSystem, Fleet fleet)
		{
			fleetLocations.Add(fleet, starSystem.cellCoordinates);
		}

		public void FleetEnteredJumpSpace(Fleet fleet)
		{
			fleetLocations[fleet] = jumpSpaceCell;
			fleet.transform.localPosition = jumpSpace;
		}

		public List<Fleet> GetFleetsAt(StarSystem system)
		{
			return fleetLocations.GetKeysFromValue(system.cellCoordinates);
		}

		public Vector3Int GetCellOf(Fleet fleet)
		{
			return fleetLocations[fleet];
		}
	}
}