using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AtomosZ.MG2eTraveller.Starmap.Starmap;
using Random = UnityEngine.Random;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class Fleet : MonoBehaviour, ISelectable
	{
		public enum FleetState
		{
			AwaitingOrders,
			MovingToSafeJumpPosition,
			InJumpSpace,
			Refueling,
		}
		public FleetState state;

		private List<StarSystem> waypoints = new List<StarSystem>();

		[SerializeField] private Vector3 idleOffset;
		[SerializeField] private Vector3 activeOffset;
		[SerializeField] public BoxCollider2D boxCollider;
		[SerializeField] private SpriteOutliner outliner;

		private SectorTilemap _sectorTilemap;
		public Starmap.InteractionState interactionState;
		public int jDrive = 0;
		public int fuelCapacity = 0;
		public int fuelCurrent = 0;

		[Tooltip("The system and the date of last update.")]
		private Dictionary<StarSystem, ImperialDate> fleetLogs = new();

		public SectorTilemap sectorTilemap
		{
			get
			{
				if (_sectorTilemap == null)
				{
					var sector = GameObject.FindFirstObjectByType<SectorTilemap>();
					_sectorTilemap = sector;
				}
				return _sectorTilemap;
			}
		}

		void Start()
		{
			interactionState = InteractionState.MouseOver;
			SetInteractionState(InteractionState.None);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newState"></param>
		/// <param name="forcedStateChange">If true will always changed to newState.</param>
		public void SetInteractionState(Starmap.InteractionState newState, bool forcedStateChange = false)
		{
			if (forcedStateChange)
				interactionState = newState;
			else if (!this.CheckState(newState, ref interactionState))
				return;

			var oultineData = Starmap.instance.fleetHighlightData[interactionState];

			outliner.SetPulseColor(oultineData.color, oultineData.pulseSpeed);


			var pos = transform.localPosition;
			pos.Set(pos.x, pos.y, oultineData.zPopOut);
			transform.localPosition = pos;
		}


		public void UpdatePosition(StarSystem system, bool isIdle)
		{
			// If exists, contact local authorities and retrieve SystemLogs
			RecordSystemLogs(system);
			if (isIdle)
				transform.localPosition = system.transform.position + idleOffset;
			else
				transform.localPosition = system.transform.position + activeOffset;
		}

		private void RecordSystemLogs(StarSystem system)
		{
			if (fleetLogs.ContainsKey(system))
				fleetLogs[system] = Starmap.instance.currentDate;
			else
				fleetLogs.Add(system, Starmap.instance.currentDate);
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void UpdatePosition_EDITOR()
		{
			var system = sectorTilemap.GetSystemAtWorldPos(transform.position);
			transform.localPosition = system.transform.position + idleOffset;
			Starmap.instance.FleetEnteredSystem(system, this);
		}

		public void SpoolUpJDrive(StarSystem destinationSystem)
		{
			// If exists, contact local authorities and retrieve SystemLogs
			var cell = FleetManager.instance.GetCellOf(this);
			RecordSystemLogs(Starmap.instance.GetSystemAt(cell));


			StartCoroutine(MovingOutOfJumpShadows(destinationSystem));
		}

		[Tooltip("This could change depending on system for a more \"real\" feel.")]
		public float secondsToExitJumpShadowAt1G = 1800; // 300 minutes
		public float timeUntilJump = 0;
		/// <summary>
		/// Changes depending on system?
		/// Preparing for jump requires 2Dx10 minutes, which is usually a shorted amount of time then to get out of a Jump shadow of an Earth sized planet.
		/// </summary>
		/// <returns></returns>
		private IEnumerator MovingOutOfJumpShadows(StarSystem destinationSystem)
		{
			state = FleetState.MovingToSafeJumpPosition;
			// timeRequired = 2 * squareRoot of (distance / accel)
			timeUntilJump = secondsToExitJumpShadowAt1G;
			while (timeUntilJump > 0)
			{
				yield return null;
				timeUntilJump -= Time.deltaTime * Starmap.instance.baseMinuteIncrement * 60 * Starmap.instance.secondInterval;
			}

			StartCoroutine(EnterJumpSpace(destinationSystem));
		}

		private const int jumpTimeInHours = 148;
		private const int jumpTimeInSeconds = jumpTimeInHours * 60 * 60;
		public float timeUntilReturnToRealSpace;

		private IEnumerator EnterJumpSpace(StarSystem destinationSystem)
		{
			state = FleetState.InJumpSpace;
			FleetManager.instance.FleetEnteredJumpSpace(this);
			boxCollider.enabled = false;
			var renderer = GetComponent<SpriteRenderer>();
			renderer.enabled = false;

			var randTimeInSec = Random.Range(6, 37) * 60 * 60;
			//var jumpSpaceEnd = Time.time + jumpTimeInSeconds + randTimeInSec;
			timeUntilReturnToRealSpace = jumpTimeInSeconds + randTimeInSec;
			while (timeUntilReturnToRealSpace > 0)
			{
				yield return null;
				timeUntilReturnToRealSpace -= Time.deltaTime * Starmap.instance.baseMinuteIncrement * 60 * Starmap.instance.secondInterval;
			}

			boxCollider.enabled = true;
			renderer.enabled = true;
			// setting the position should trigger the systems collider
			transform.localPosition = destinationSystem.transform.position;
			// let the next action take place in UpdatePosition().
		}
	}
}