using System;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.MG2eTraveller.Starmap.Starmap;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class Fleet : MonoBehaviour, ISelectable
	{
		//public StarSystem currentSystem;
		[SerializeField] private Vector3 idleOffset;
		[SerializeField] private Vector3 activeOffset;
		[SerializeField] public BoxCollider2D boxCollider;
		[SerializeField] private SpriteOutliner outliner;

		private SectorTilemap _sectorTilemap;
		public Starmap.InteractionState interactionState;
		public int jDrive = 0;

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


		//public void SetSystem(StarSystem system)
		//{
		//	currentSystem = system;
		//	transform.localPosition = currentSystem.transform.localPosition + centerOffset;
		//}

		[System.Diagnostics.Conditional("DEBUG")]
		public void UpdatePosition_EDITOR()
		{
			var system = sectorTilemap.GetSystemAtWorldPos(transform.position);
			transform.localPosition = system.transform.position + idleOffset;
			Starmap.instance.FleetEnteredSystem(system, this);
		}
	}
}