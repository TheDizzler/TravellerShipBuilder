using UnityEngine;

namespace AtomosZ.DFDQ.Battle
{
	public class BattleManager : MonoBehaviour
	{
		public static BattleManager instance;

		public enum GameState
		{
			Initializing,
			HeroesTurn,
			MonsterTurn,
		}

		public GameState gameState;


		void Awake()
		{
			instance = this;
			gameState = GameState.Initializing;
		}

		void Start()
		{
			GridManager.instance.GenerateGrid();
			UnitManager.instance.SpawnHeroes();
			//UnitManager.instance.SpawnMonsters();
			gameState = GameState.HeroesTurn;
		}
	}
}