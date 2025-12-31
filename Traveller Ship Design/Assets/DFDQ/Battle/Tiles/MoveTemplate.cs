using System;
using System.Collections.Generic;
using System.Linq;
using AtomosZ.DFDQ.Battle;
using AtomosZ.DFDQ.Battle.Units;
using UnityEngine;
using static AtomosZ.DFDQ.Battle.GridManager;
using static AtomosZ.DFDQ.Battle.Units.BaseUnit;
using static AtomosZ.Helpers;
using Object = UnityEngine.Object;

namespace AtomosZ.DFDQ.Tiles
{
	[Serializable]
	public class MoveTemplate
	{
		public BaseUnit unit;
		public GridTile startTile;
		public int movePoints;
		public MovementType moveType;
		public List<TerrainHighlighter> highlights;

		public MoveTemplate(BaseUnit unit)
		{
			this.unit = unit;
			startTile = unit.occupiedTile;
			movePoints = unit.movePoints;
			moveType = unit.movementType;
			highlights = MoveFloodFill(startTile, movePoints, moveType,
				unit.faction == Faction.Hero ? HighlightType.FactionHeroMovement : HighlightType.FactionMonsterMovement);
		}

		/// <summary>
		/// Does nothing if no changes found.
		/// </summary>
		/// <param name="unit"></param>
		/// <returns>true if new move template constructed, false if no change.</returns>
		public bool Recalculate(BaseUnit unit)
		{
			if (this.unit == unit
				&& this.movePoints == unit.movePoints
				&& this.moveType == unit.movementType
				&& highlights.Count > 0)
			{
				foreach (var high in highlights)
				{
#if DEBUG
					if (high == null)
					{
						Clear();
						return Recalculate(unit);
					}
#endif
					high.gameObject.SetActive(true);
				}

				return false;
			}

			Clear();
			this.unit = unit;
			startTile = unit.occupiedTile;
			movePoints = unit.movePoints;
			moveType = unit.movementType;
			highlights = MoveFloodFill(startTile, movePoints, moveType,
				unit.faction == Faction.Hero ? HighlightType.FactionHeroMovement : HighlightType.FactionMonsterMovement);
			return true;
		}

		public void Hide()
		{
			foreach (var high in highlights)
			{
				high.gameObject.SetActive(false);
			}
		}

		public void Clear()
		{
#if DEBUG
			if (highlights == null)
			{
				highlights = new List<TerrainHighlighter>();
				return;
			}
#endif

			foreach (var high in highlights)
			{
#if DEBUG
				if (high == null)
					continue;
#endif
				if (!Application.isPlaying)
					Object.DestroyImmediate(high.gameObject);
				else
					Object.Destroy(high.gameObject);
			}

			highlights.Clear();
		}



		private static List<TerrainHighlighter> MoveFloodFill(GridTile startTile, int maxDistance, MovementType moveAllowed, HighlightType highlightType)
		{
			var visited = new Dictionary<Vector2Int, TerrainHighlighter>();
			var firstHighlight = GridManager.instance.CreateHighlight(startTile.position, highlightType, visited);
			if (moveAllowed == 0 || maxDistance == 0)
			{
				firstHighlight.SetText("Immobile");
				return visited.Values.ToList();
			}

			firstHighlight.SetText(maxDistance.ToString());

			var nextLevel = new Queue<(GridTile tile, int distanceRemaining)>();
			nextLevel.Enqueue((startTile, maxDistance));
			while (nextLevel.TryDequeue(out (GridTile tile, int distanceRemaining) checkTile))
			{
				if (checkTile.distanceRemaining <= 0)
					continue;
				foreach (CardinalDirection dir in Enum.GetValues(typeof(CardinalDirection)))
				{
					var nextTilePos = checkTile.tile[dir];

					if (GridManager.instance.TryGetTile(nextTilePos, out GridTile nextTile))
					{
						var moveCost = nextTile.movementCost;
						var adjDistance = checkTile.distanceRemaining - moveCost;
						if (adjDistance < 0)
							continue;
						if (nextTile.IsMovementAllowed(moveAllowed)
							&& !visited.ContainsKey(nextTilePos))
						{
							if (nextTile.IsOccupied())
							{
								if ((moveAllowed & MovementType.Fly) != MovementType.Fly
									|| adjDistance == 0)
									continue;
							}


							nextLevel.Enqueue((nextTile, adjDistance));
							var highlight = GridManager.instance.CreateHighlight(nextTile.position, highlightType, visited);
							highlight.SetText(adjDistance.ToString());
						}
					}
				}
			}

			return visited.Values.ToList();
		}
	}
}