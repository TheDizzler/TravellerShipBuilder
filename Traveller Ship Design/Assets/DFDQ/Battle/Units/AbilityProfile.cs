using System;
using System.Collections.Generic;
using AtomosZ.DFDQ.Battle.Units;
using AtomosZ.DFDQ.Tiles;
using UnityEngine;

namespace AtomosZ.DFDQ
{
	/// <summary>
	/// 2 basic types of Abilities: Targeted and non-Targeted.<br/><br/>
	/// Targeted abilities will show the range that the ability can be used in 
	/// (<c>range, rangeAreaEffectType, rangeTemplate</c>, with the caster as the center)<br/>
	/// and the area that will be affected by the ability
	/// (<c>effectAreaType, effectAreaTemplate</c>, with the cursor as the center).<br/><br/>
	/// Non-Targeted abilities will only show the area affected (<c>range, rangeAreaEffectType, rangeTemplate</c>, with the caster as the center).
	/// </summary>
	[Serializable]
	public class AbilityProfile
	{
		public enum AreaEffectType
		{
			[Tooltip("1*1 tile")]
			Single,
			[Tooltip("X*Y tiles")]
			/// </summary>
			Rectangle,
			[Tooltip("A cross that is X*2 long and Y*2 tall, intersecting at the middle.")]
			Cross,
			[Tooltip("")]
			Diamond,
			[Tooltip("A triangle shape that originates from a tile behind the tip.")]
			Cone,
		}


		[Tooltip("Use dependant on rangeAreaEffectType. Typical attack uses Diamond.\n"
			+ "rangeAreaEffectType.Diamond:\n"
			+ "\trange.x == maximum range\n"
			+ "\t\trange.x = 0 : self only.\n"
			+ "\t\trange.x = 1 : typical melee attack (default).\n"
			+ "\t\trange.x = 2+ : range attack (bow, magic, etc.)\n"
			+ "\trange.y == minimum range. (@TODO(Tristan): NOT YET IMPLEMENTED)\n"
			+ "\t\trange.y = 0 : no minimum range (default)\n"
			+ "\t\trange.y = 1 : cannot use on adjacent tiles\n"
			+ "\t\trange.y = X+ : cannot use on tiles within X tiles of user.")]
		public Vector2Int range = new Vector2Int(1, 0);
		public Vector2Int effectSize = Vector2Int.one;
		[Tooltip("This tells you where you can use the ability.")]
		public AreaEffectType rangeAreaEffectType;
		[Tooltip("This template shows which tiles you can use the ability on.")]
		public AttackTemplate rangeTemplate;


		[Tooltip("This tells you which tiles will be affected by the ability.")]
		public AreaEffectType effectAreaType;
		[Tooltip("This template shows you which tiles will take the effect of the ability.")]
		public AttackTemplate effectAreaTemplate;


		public HashSet<Vector2Int> viableTargets;



		public void ShowAbility(BaseUnit unit)
		{
			if (rangeTemplate.Recalculate(unit.occupiedTile, rangeAreaEffectType, range))
				viableTargets = rangeTemplate.affectedTiles;
		}

		public void Hide()
		{
			rangeTemplate.Hide();
			effectAreaTemplate.Hide();

		}

		public void ShowEffectArea(GridTile targetTile, bool isValidTarget)
		{
			if (effectAreaTemplate.Recalculate(targetTile, effectAreaType, effectSize))
				Debug.LogWarning("New effect template");

			if (viableTargets.Contains(targetTile.position))
			{
				Debug.Log("Do something");
			}
		}
	}
}