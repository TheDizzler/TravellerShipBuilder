using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using AtomosZ.DFDQ.Battle.Units;
using AtomosZ.DFDQ.Tiles;
using UnityEngine;

namespace AtomosZ.DFDQ.Battle
{
	public class UnitManager : MonoBehaviour
	{
		public static UnitManager instance;


		public BaseUnit selectedHero;
		public List<BaseUnit> unitsInField;
		[SerializeField] private GridManager gridManager;
		[SerializeField] private Transform unitHolder;
		[SerializeField] private BaseUnit baseUnitPrefab;
		[SerializeField] public List<ScriptableUnit> unitData;

		public int[] prefabIndices
		{
			get
			{
				var list = new int[unitData.Count];
				for (int i = 0; i < unitData.Count; ++i)
					list[i] = i;
				return list;
			}
		}

		public string[] prefabNames
		{
			get
			{
				var list = new string[unitData.Count];
				for (int i = 0; i < unitData.Count; ++i)
					list[i] = unitData[i].name;
				return list.ToArray();
			}
		}

		void Awake()
		{
			instance = this;

			unitData = Resources.LoadAll<ScriptableUnit>("Units").ToList();
		}


		[Conditional("DEBUG")]
		public void RefreshUnitList()
		{
			unitData = Resources.LoadAll<ScriptableUnit>("Units").ToList();
		}

		public void SpawnHeroes()
		{
			var heroCount = 1;
			for (int i = 0; i < heroCount; ++i)
			{
				var heroData = GetRandomUnit<ScriptableUnit>(Faction.Hero);
				SpawnUnit(heroData);
			}
		}

		public void SpawnUnit(ScriptableUnit newUnitData)
		{
			var unit = Instantiate(baseUnitPrefab, unitHolder);
			unit.Initialize(newUnitData);

			GridTile tile;
			if (unit.faction == Faction.Hero)
				tile = gridManager.GetHeroSpawnTile(unit.movementType);
			else
				tile = gridManager.GetMonsterSpawnTile(unit.movementType);
						
			tile.SetUnit(unit);
			unitsInField.Add(unit);
		}

		public void SpawnMonsters()
		{
			var heroCount = 1;
			for (int i = 0; i < heroCount; ++i)
			{
				var monsterData = GetRandomUnit<ScriptableUnit>(Faction.Monster);
				SpawnUnit(monsterData);
			}
		}

		public void SetSelectedHero(BaseUnit hero)
		{
			selectedHero = hero;
			MenuManager.instance.ShowSelectedHero(hero);
			hero.ShowPossibleMove();
		}


		private T GetRandomUnit<T>(Faction faction) where T : ScriptableUnit
		{
#if DEBUG
			if (unitData == null || unitData.Count == 0)
				unitData = Resources.LoadAll<ScriptableUnit>("Units").ToList();
#endif
			return (T)unitData.Where(u => u.faction == faction).OrderBy(o => Random.value).First();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void DeleteUnit(int unitIndex)
		{
			var delUnit = unitsInField[unitIndex];
			unitsInField.RemoveAt(unitIndex);
			if (!Application.isPlaying)
				DestroyImmediate(delUnit.gameObject);
			else
				Destroy(delUnit.gameObject);
		}
	}
}