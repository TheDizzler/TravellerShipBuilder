using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.DFDQ.Battle;
using AtomosZ.DFDQ.Battle.Units;
using AtomosZ.DFDQ.Tiles.Widgets;
using UnityEngine;
using static AtomosZ.DFDQ.Battle.BattleManager;
using static AtomosZ.DFDQ.Battle.GridManager;
using static AtomosZ.DFDQ.Battle.Units.BaseUnit;

namespace AtomosZ.DFDQ.Tiles
{
	public class GridTile : MonoBehaviour
	{
		public string tileName;
		public Vector2Int position;

		[SerializeField] private new SpriteRenderer renderer;
		[SerializeField] private SpecialTile specialTile;

		[SerializeField] private MovementType _movementAllowed;
		[SerializeField] private bool blocksLineOfSight;

		public TerrainWidget widget;
		public BaseUnit occupiedUnit;
		public int movementCost = 1;

		public MovementType movementAllowed { get { return _movementAllowed; } }

		public bool IsMovementAllowed(BaseUnit unit)
		{
			return IsMovementAllowed(unit.movementType);
		}

		public bool IsMovementAllowed(MovementType type)
		{
			foreach (MovementType moveType in Enum.GetValues(typeof(MovementType)))
				if ((movementAllowed & moveType) == moveType
					&& (type & moveType) == moveType)
					return true;
			return false;
		}


		public bool IsOccupied()
		{
			return occupiedUnit != null;
		}


		public Vector2Int this[CardinalDirection dir]
		{
			get
			{
				switch (dir)
				{
					case CardinalDirection.Up:
						return up;
					case CardinalDirection.Right:
						return right;
					case CardinalDirection.Down:
						return down;
					case CardinalDirection.Left:
						return left;
				}

				throw new Exception("This can't happen");
			}
		}

		public List<Vector2Int> directions => new()
		{
			up, right, down, left,
		};

		public Vector2Int up => new Vector2Int(position.x, position.y + 1);
		public Vector2Int right => new Vector2Int(position.x + 1, position.y);
		public Vector2Int down => new Vector2Int(position.x, position.y - 1);
		public Vector2Int left => new Vector2Int(position.x - 1, position.y);

		public void Init(Vector2Int pos)
		{
			position = pos;

			//Highlight(HighlightType.None);
		}

		public bool SetUnit(BaseUnit unit)
		{
			if (occupiedUnit != null)
				return false;

			if (unit.occupiedTile != null)
			{
				if (unit.occupiedTile == this)
					return true;
				unit.RemoveFromTile();
			}

			unit.transform.position = transform.position;
			occupiedUnit = unit;
			unit.occupiedTile = this;
			return true;
		}

		public void RemoveUnit()
		{
			if (occupiedUnit == null)
				return;
			occupiedUnit.occupiedTile = null;
			occupiedUnit = null;
		}

		public bool AttachWidget(TerrainWidget newWidget)
		{
			if (widget != null)
			{
				return false;
			}

			newWidget.transform.SetParent(this.transform);
			newWidget.tile = this;
			widget = newWidget;
			return true;
		}

		public void RemoveWidget()
		{
			if (widget == null)
				return;
			widget.transform.SetParent(GameObject.Find("OrphanedObjects").transform);
			widget.tile = null;
			widget = null;
		}




		void OnMouseEnter()
		{
			//Highlight(HighlightType.MouseOver);
			MenuManager.instance.ShowTileInfo(this);
		}

		void OnMouseExit()
		{
			//Highlight(HighlightType.None);
			MenuManager.instance.ShowTileInfo(null);
		}

		void OnMouseDown()
		{
			if (BattleManager.instance.gameState != GameState.HeroesTurn)
				return;

			if (occupiedUnit != null)
			{
				if (occupiedUnit.faction == Faction.Hero)
				{
					UnitManager.instance.SetSelectedHero((BaseUnit)occupiedUnit);
				}
				else
				{
					if (UnitManager.instance.selectedHero != null)
					{
						var monster = (BaseUnit)occupiedUnit;
						Destroy(monster.gameObject);
						UnitManager.instance.SetSelectedHero(null);
					}
				}
			}
			else
			{
				var selected = UnitManager.instance.selectedHero;
				if (selected != null)
				{
					if ((movementAllowed & selected.movementType) == selected.movementType)
					{
						SetUnit(selected);
						UnitManager.instance.SetSelectedHero(null);
					}
				}
			}
		}

		public bool BlocksLineOfSight()
		{
			if (occupiedUnit != null && occupiedUnit.BlocksLineOfSight())
				return true;
			if (widget != null && widget.BlocksLineOfSight())
				return true;
			return blocksLineOfSight;
		}
	}
}