//#define DEBUG_PATHFINDING

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AtomosZ.DFDQ.Battle.Units;
using AtomosZ.DFDQ.Tiles;
using UnityEngine;
using static AtomosZ.DFDQ.AbilityProfile;
using static AtomosZ.DFDQ.Battle.Units.BaseUnit;
using static AtomosZ.Helpers;
using Random = UnityEngine.Random;



namespace AtomosZ.DFDQ.Battle
{
	public class GridManager : MonoBehaviour
	{
		public enum HighlightType
		{
			FactionHeroMovement,
			FactionMonsterMovement,
			AttackTemplate,

#if DEBUG
			DEBUG_PathShortest,
			DEBUG_PathVisited,
			DEBUG_PathFrontier,
#endif
		}

		private static GridManager _instance;
		public static GridManager instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<GridManager>();
				return _instance;
			}
		}

		public static LayerMask tileLayer { get { return LayerMask.GetMask("TerrainTiles"); } }

		public LayerMask terrainTilesLayerMask;
		[SerializeField] private List<GridTile> tilePrefabs;

		public int waterWeight = 24;
		public int mountainWeight = 18;
		[SerializeField] private int width, height;

		[SerializeField] private Transform camTrans;
		public Transform gridTrans;
		[SerializeField] private TerrainHighlighter highlighterPrefab;
		[SerializeField] private PathLineRenderer pathRenderer;
		[SerializeField] private LineOfSightRenderer losRenderer;

		[SerializeField] private CustomDictionary<Vector2Int, GridTile> tiles = new();


		public AttackTemplate attackTemplate;

		public GridTile testCenterTile;



		public Vector2Int testAttackSize = new Vector2Int(1, 1);
		[SerializeField] public ObjectForge.ObjectPool<TerrainHighlighter> highlightPool;

		void Awake()
		{
			_instance = this;
			tilePrefabs = Resources.LoadAll<GridTile>("Tiles").ToList();
		}



		public void GenerateGrid()
		{
			ClearGrid();

#if DEBUG
			if (tilePrefabs.Count == 0)
				tilePrefabs = Resources.LoadAll<GridTile>("Tiles").ToList();
#endif

			for (int x = 0; x < width; ++x)
			{
				for (int y = 0; y < height; ++y)
				{
					var rand = Random.Range(1, 31);
					int tileIndex = 0;      // grass 18/30
					if (rand > waterWeight)
						tileIndex = 2;      // Water = 8 /30,
					else if (rand > mountainWeight)
						tileIndex = 1;      // Mountain = 6/30,

					var randomTile = tilePrefabs[tileIndex];
					var tile = Instantiate(randomTile, new Vector3(x, y), Quaternion.identity, gridTrans);
					tile.name = $"Tile ({x},{y})";
					tile.Init(new Vector2Int(x, y));

					tiles.Add(new Vector2Int(x, y), tile);
				}
			}

			camTrans.position = new Vector3((float)width / 2 - .5f, (float)height / 2 - .5f, -10);
		}

		private void ClearGrid()
		{
			var existingTiles = gridTrans.GetComponentsInChildren<GridTile>();
			foreach (var tile in existingTiles)
			{
#if DEBUG
				if (Application.isPlaying)
					Destroy(tile.gameObject);
				else
					DestroyImmediate(tile.gameObject);
#else
				Destroy(tile.gameObject);
#endif
			}

			tiles.Clear();
		}

		public GridTile GetHeroSpawnTile(MovementType movementType)
		{
			var allowedTiles = tiles.Where(t => t.Key.x < width).ToList();
			for (int i = allowedTiles.Count - 1; i >= 0; --i)
			{
				var tile = allowedTiles[i];
				if (!tile.Value.IsMovementAllowed(movementType)
					|| tile.Value.IsOccupied())
					allowedTiles.Remove(tile);
			}

			if (allowedTiles.Count == 0)
				Debug.LogException(new System.Exception("No allowable tiles found!"));
			var rand = Random.Range(0, allowedTiles.Count - 1);
			return allowedTiles[rand].Value;
		}

		public GridTile GetMonsterSpawnTile(MovementType movementType)
		{
			var allowedTiles = tiles.Where(t => t.Key.x > width / 2 && t.Value.IsMovementAllowed(movementType)).ToList();
			if (allowedTiles.Count == 0)
				Debug.LogException(new System.Exception("No allowable tiles found!"));
			var rand = Random.Range(0, allowedTiles.Count - 1);
			return allowedTiles[rand].Value;
		}

		public static GridTile GetTileAtPosition(int x, int y)
		{
			return GetTileAtPosition(new Vector2Int(x, y));
		}

		public static GridTile GetTileAtPosition(Vector2Int pos)
		{
			if (instance.tiles.TryGetValue(pos, out GridTile tile))
				return tile;
			return null;
		}


		[System.Diagnostics.Conditional("DEBUG")]
		public void DestroyAllHighlights()
		{
			highlightPool.Clear();

			foreach (var high in gridTrans.GetComponentsInChildren<TerrainHighlighter>(true))
			{
				if (Application.isPlaying)
					Destroy(high.gameObject);
				else
					DestroyImmediate(high.gameObject);
			}
		}

		public void ClearAllHighlights()
		{
			foreach (HighlightType highlightType in Enum.GetValues(typeof(HighlightType)))
				ClearHighlights(highlightType);

			attackTemplate = null;
		}

		public void ClearHighlights(HighlightType highlightType)
		{
			if (highlightType == HighlightType.AttackTemplate && attackTemplate != null)
			{
				attackTemplate.Clear();
				attackTemplate = null;
			}

			foreach (var high in gridTrans.GetComponentsInChildren<TerrainHighlighter>(true))
			{
				if (high.highlightType == highlightType)
				{
					high.ReturnToPool();
				}
			}
		}


		public static void ShowAttack(Vector2 pos, AreaEffectType attack, Vector2Int size)
		{
			instance.ShowAttackHighlight(pos, attack, size);
		}

		public void HideAttack()
		{
			ClearHighlights(HighlightType.AttackTemplate);
			attackTemplate = null;
		}

		public void ClearPool()
		{
			if (highlightPool != null)
				highlightPool.Clear();
		}

		public void HidePool()
		{
			highlightPool.ReturnAll();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void WakePool()
		{
			highlightPool.WakeAll();
		}

		public void TestCreatePool()
		{
			if (highlightPool != null)
				highlightPool.Clear();
			highlightPool = new ObjectForge.ObjectPool<TerrainHighlighter>(highlighterPrefab);
			//highlightPool.OnDestroy += highlightPool.DestroyMonoBehaviour;
			//highlightPool.OnSleep += highlightPool.SleepMonoBehaviour;
			//highlightPool.OnAwake += highlightPool.WakeMonoBehaviour;

			//var high = highlightPool.GetNext();
			//high.SetTileHighlight(HighlightType.DEBUG_PathShortest, GetTileAtPosition(1, 1));
			//high = highlightPool.GetNext();

			//high.SetTileHighlight(HighlightType.DEBUG_PathShortest, GetTileAtPosition(1, 2));
			//high = highlightPool.GetNext();

			//high.SetTileHighlight(HighlightType.DEBUG_PathShortest, GetTileAtPosition(2, 2));
		}


		private TerrainHighlighter InstantiateTerrainHighlighter()
		{
			var result = Instantiate(highlighterPrefab);
			return result;
		}

		public void ShowAttackHighlight(Vector2 pos, AreaEffectType attack, Vector2Int size)
		{
			var tileCollider = Physics2D.OverlapPoint(new Vector2(pos.x, pos.y), terrainTilesLayerMask);
			if (tileCollider == null)
			{
				HideAttack();
				return;
			}

			size.x = Mathf.Max(size.x, 1);
			size.y = Mathf.Max(size.y, 1);

			var tile = tileCollider.GetComponent<GridTile>();
			if (attackTemplate != null)
			{
				if (attackTemplate.attackType == attack
					&& attackTemplate.size == size)
				{
					attackTemplate.SlideTemplate(tile);
					return;
				}

				ClearHighlights(HighlightType.AttackTemplate);
				attackTemplate = null;
			}

			attackTemplate = new AttackTemplate(tile, attack, size);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sourceUnit"></param>
		/// <param name="targetTile"></param>
		/// <param name="range">-1 is infinite range.</param>
		public bool HasLineOfSight(BaseUnit sourceUnit, GridTile targetTile, int range)
		{
			return losRenderer.HasLineOfSight(sourceUnit.position, targetTile.position, range);
		}

		public void HideLineOfSight()
		{
			losRenderer.Hide();
		}

		public void ShowPathTo(BaseUnit unit, Vector2Int destinationPos)
		{
			if (destinationPos == unit.occupiedTile.position)
				return;
			//Debug.LogWarning("Looking for new path");
			var startTile = unit.occupiedTile;
			var startNode = new AStarNode(0, Heuristic(startTile.position, destinationPos))
			{
				prev = null,
				tile = startTile,
			};

			var openList = new List<AStarNode>();
			openList.Add(startNode);

			var path = AStar(unit, destinationPos, openList);
			pathRenderer.SetPath(path, unit.movePoints);
		}

		private List<AStarNode> AStar(BaseUnit unit, Vector2Int destinationPos,
			List<AStarNode> openList)
		{
#if DEBUG_PATHFINDING
			ClearHighlights(HighlightType.DEBUG_PathShortest);
			ClearHighlights(HighlightType.DEBUG_PathVisited);
			ClearHighlights(HighlightType.DEBUG_PathFrontier);

			var debugHighlights = new Dictionary<AStarNode, TerrainHighlighter>();

			var highlight = CreateHighlight(openList[0].tile, HighlightType.DEBUG_PathFrontier);
			highlight.SetText("START");
			debugHighlights.Add(openList[0], highlight);
#endif

			var closedList = new List<AStarNode>();
			AStarNode targetNode = null;
			while (openList.Count > 0 && targetNode == null)
			{
				int lowestF = int.MaxValue;
				AStarNode currentNode = null;
				for (int i = 0; i < openList.Count; ++i)
				{
					if (openList[i].f < lowestF)
					{
						currentNode = openList[i];
						lowestF = currentNode.f;
					}
				}

				if (currentNode == null)
					continue;
				openList.Remove(currentNode);
				if (currentNode.tile == null)
					Debug.LogError("WTF");

#if DEBUG_PATHFINDING
				debugHighlights[currentNode].SetHighlight(HighlightType.DEBUG_PathVisited, currentNode.tile);
#endif

				foreach (var dir in currentNode.tile.directions)
				{
					if (!TryGetTile(dir, out GridTile nextTile) // out of bounds
						|| !nextTile.IsMovementAllowed(unit))
						continue;

					if (IsInList(closedList, nextTile))
						continue;


					int g = currentNode.g + nextTile.movementCost;
					int h = Heuristic(nextTile.position, destinationPos);
					var nextNode = new AStarNode(g, h)
					{
						prev = currentNode,
						tile = nextTile,
					};

					if (nextTile.position == destinationPos) // would h always == 0 here?
					{
						targetNode = nextNode;
#if DEBUG_PATHFINDING
						highlight = CreateHighlight(targetNode.tile, HighlightType.DEBUG_PathShortest);
						highlight.SetText(targetNode.g.ToString());
						debugHighlights.Add(targetNode, highlight);
#endif
						break;
					}

					closedList.Add(nextNode);
					bool exists = false;
					for (int i = 0; i < openList.Count; ++i)
					{
						if (openList[i].tile == nextTile)
						{
							var existingNode = openList[i];
							if (existingNode.g > g)
							{
								existingNode.prev = currentNode;
								existingNode.g = g;
								existingNode.h = h;
								existingNode.f = g + h;

#if DEBUG_PATHFINDING
								var high = debugHighlights[existingNode];
								high.SetText(existingNode.g.ToString());
#endif
							}

							exists = true;
							break;
						}
					}

					if (exists)
						continue;

					openList.Add(nextNode);
#if DEBUG_PATHFINDING
					highlight = CreateHighlight(nextNode.tile, HighlightType.DEBUG_PathFrontier);
					highlight.SetText(nextNode.g.ToString());
					debugHighlights.Add(nextNode, highlight);
#endif
				}
			}

			if (targetNode == null)
				return null;
			var path = new List<AStarNode>();
			path.Add(targetNode);
			var parent = targetNode.prev;
			while (parent != null)
			{
#if DEBUG_PATHFINDING
				debugHighlights[parent].SetHighlight(HighlightType.DEBUG_PathShortest, parent.tile);
#endif
				path.Add(parent);
				parent = parent.prev;
			}

			return path;
		}

		private bool IsInList(List<AStarNode> closedList, GridTile nextTile)
		{
			for (int i = 0; i < closedList.Count; ++i)
			{
				if (closedList[i].tile == nextTile)
				{
					return true;
				}
			}

			return false;
		}

		private int Heuristic(Vector2Int position, Vector2Int destinationPos)
		{
			Vector2Int vectorDistanceToGoal = destinationPos - position;
			int distanceToGoal = Mathf.Abs(vectorDistanceToGoal.x) + Mathf.Abs(vectorDistanceToGoal.y);
			return distanceToGoal;
		}

		public class AStarNode
		{
			public AStarNode prev;
			public GridTile tile;
			/// <summary>
			/// Total cost of node. (f + g)
			/// </summary>
			public int f;
			/// <summary>
			/// Distance from start node.
			/// </summary>
			public int g;
			/// <summary>
			/// Distance To Goal (heuristic)
			/// </summary>
			public int h;


			/// <summary>
			/// 
			/// </summary>
			/// /// <param name="distanceFromStart">g</param>
			/// <param name="distanceToGoal">h</param>
			public AStarNode(int distanceFromStart, int distanceToGoal)
			{
				g = distanceFromStart;
				h = distanceToGoal;
				f = g + h;
			}
		}

		public void HidePath()
		{
			pathRenderer.SetPath(null);
#if DEBUG
			ClearHighlights(HighlightType.DEBUG_PathShortest);
			ClearHighlights(HighlightType.DEBUG_PathVisited);
			ClearHighlights(HighlightType.DEBUG_PathFrontier);
#endif
		}


		public TerrainHighlighter CreateHighlight(Vector2Int tilePos,
			HighlightType highlightType, Dictionary<Vector2Int, TerrainHighlighter> visited)
		{
			TerrainHighlighter highlight;
			if (!GridManager.instance.TryGetTile(tilePos, out GridTile tile))
			{
				highlight = highlightPool.GetNext();
				highlight.name = $"Highlight_{visited.Count} ({tilePos.x},{tilePos.y})";
				highlight.SetTileHighlight(highlightType, tile);
			}
			else
			{
				highlight = CreateHighlight(tile, highlightType);
			}

			highlight.SetBorders(TerrainHighlighter.Borders.Top | TerrainHighlighter.Borders.Left
				| TerrainHighlighter.Borders.Bottom | TerrainHighlighter.Borders.Right);
			visited.Add(tilePos, highlight);
			foreach (CardinalDirection dir in Enum.GetValues(typeof(CardinalDirection)))
			{
				if (visited.TryGetValue(tilePos.GetNeighbourPos(dir), out var borderTile)
					&& borderTile != null)
				{
					borderTile.RemoveBorder(BorderOpposite(dir));
					highlight.RemoveBorder(Border(dir));
				}
			}
			return highlight;
		}

		public TerrainHighlighter CreateHighlight(GridTile tile, HighlightType highlightType)
		{
			if (tile == null)
				Debug.LogException(new Exception("GridTile tile may not be null"));

			//var highlight = Instantiate(highlighterPrefab);
			var highlight = highlightPool.GetNext();
			var tilePos = tile.position;
			highlight.name = $"Highlight_({tilePos.x},{tilePos.y})";
			highlight.SetTileHighlight(highlightType, tile);

			return highlight;
		}


		public static TerrainHighlighter.Borders Border(CardinalDirection dir)
		{
			switch (dir)
			{
				case CardinalDirection.Up:
					return TerrainHighlighter.Borders.Top;
				case CardinalDirection.Right:
					return TerrainHighlighter.Borders.Right;
				case CardinalDirection.Down:
					return TerrainHighlighter.Borders.Bottom;
				case CardinalDirection.Left:
					return TerrainHighlighter.Borders.Left;
				default:
					throw new Exception("Not possible direction: " + dir);
			}
		}

		public static TerrainHighlighter.Borders BorderOpposite(CardinalDirection dir)
		{
			switch (dir)
			{
				case CardinalDirection.Up:
					return TerrainHighlighter.Borders.Bottom;
				case CardinalDirection.Right:
					return TerrainHighlighter.Borders.Left;
				case CardinalDirection.Down:
					return TerrainHighlighter.Borders.Top;
				case CardinalDirection.Left:
					return TerrainHighlighter.Borders.Right;
				default:
					throw new Exception("Not possible direction: " + dir);
			}
		}

		public bool TryGetTile(Vector2Int pos, out GridTile tile)
		{
			return tiles.TryGetValue(pos, out tile);
		}
	}
}