using System;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class FleetManager : MonoBehaviour
	{
		public static readonly Vector3Int jumpSpaceCell = new Vector3Int(int.MinValue, int.MinValue, int.MinValue);
		[Tooltip("A fleet in jump space has a Vector3Int == jumpSpaceCell")]
		public CustomDictionary<Fleet, Vector3Int> fleetLocations;

		public void FleetEnteredSystem(StarSystem starSystem, Fleet fleet)
		{
			fleetLocations.Add(fleet, starSystem.cellCoordinates);
		}

		public void FleetEnteredJumpSpace(Fleet fleet)
		{
			fleetLocations[fleet] = jumpSpaceCell;
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