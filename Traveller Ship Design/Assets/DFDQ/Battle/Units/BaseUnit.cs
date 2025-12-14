using System;
using AtomosZ.DFDQ.Tiles;
using UnityEngine;
using static AtomosZ.DFDQ.AbilityProfile;
using static AtomosZ.DFDQ.Battle.GridManager;

namespace AtomosZ.DFDQ.Battle.Units
{
	public class BaseUnit : MonoBehaviour
	{
		/// <summary>
		/// A movement of 0x0 means it cannot move on its own.
		/// </summary>
		[Serializable, Flags]
		public enum MovementType
		{
			/// <summary>
			/// May move on solid ground.
			/// </summary>
			Walk = 0x2,
			/// <summary>
			/// May fly over unwalkable tiles.
			/// </summary>
			Fly = 0x4,
			/// <summary>
			/// May swim in liquid.
			/// </summary>
			Swim = 0x8,
		}

		public BoxCollider2D boxCollider;
		public GridTile occupiedTile;
		public Vector2Int position => occupiedTile.position;
		public Faction faction;
		public string unitName;

		public int movePoints;
		public int attackRange;

		public MovementType movementType = MovementType.Walk;


		public bool hasMoved;
		public bool hasAttacked;
		public MoveTemplate moveTemplate;
		public AbilityProfile ability;

		private bool blocksLineOfSight;
		[SerializeField] private bool isShowAttack;
		[SerializeField] private bool isShowMove;

		public void Initialize(ScriptableUnit unitData)
		{
			name = unitName = unitData.unitName;
			faction = unitData.faction;
			blocksLineOfSight = unitData.blocksLineOfSight;
			GetComponent<SpriteRenderer>().sprite = unitData.sprite;
			var palSwap = GetComponent<PaletteSwap>();
			palSwap.colorSwapDict = unitData.colorSwapDict;
			palSwap.CreatePaletteSwapTexture();
		}

		public void StartTurn()
		{
			hasMoved = false;
			hasAttacked = false;
		}

		public void ShowAbility(GridTile targetTile)
		{
			isShowAttack = true;
			ability.ShowAbility(this);
			ability.ShowEffectArea(targetTile,
				GridManager.instance.HasLineOfSight(this, targetTile, -1));
		}


		public void HideAbility()
		{
			isShowAttack = false;
			ability.Hide();
			GridManager.instance.HideLineOfSight();
		}

		public void ShowPossibleMove()
		{
			isShowMove = true;
			if (moveTemplate.Recalculate(this))
				Debug.LogWarning("New move template");
			GridManager.instance.HidePath();
		}

		public void HideMove()
		{
#if DEBUG
			if (moveTemplate == null)
			{
				var allHighlights = GridManager.instance.gridTrans.GetComponentsInChildren<TerrainHighlighter>(true);
				foreach (var high in allHighlights)
				{
					if (high.highlightType == (faction == Faction.Hero ?
						HighlightType.FactionHeroMovement : HighlightType.FactionMonsterMovement))
						if (!Application.isPlaying)
							DestroyImmediate(high.gameObject);
						else
							Destroy(high.gameObject);
				}

				return;
			}
#endif
			isShowMove = false;
			moveTemplate.Hide();
		}

		public void RemoveFromTile()
		{
			if (occupiedTile != null)
				occupiedTile.RemoveUnit();
		}

		public void ShowPathTo(Vector2Int destinationPos)
		{
#if DEBUG_PATHFINDING
			HideMove();
#endif
			GridManager.instance.ShowPathTo(this, destinationPos);
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool BlocksLineOfSight()
		{
			return blocksLineOfSight;
		}

	}
}