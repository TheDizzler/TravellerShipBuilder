using System;
using System.Collections.Generic;
using AtomosZ.DFDQ.Battle;
using UnityEngine;
using static AtomosZ.DFDQ.AbilityProfile;
using static AtomosZ.DFDQ.Battle.GridManager;
using static AtomosZ.Helpers;

namespace AtomosZ.DFDQ.Tiles
{
	[Serializable]
	public class AttackTemplate
	{
		public AreaEffectType attackType;
		public GridTile centerTile;
		public List<TerrainHighlighter> highlights = new List<TerrainHighlighter>();
		public Vector2Int size;

		public HashSet<Vector2Int> affectedTiles;

		public AttackTemplate(GridTile tile, AreaEffectType aoeType, Vector2Int sze)
		{
			Recalculate(tile, aoeType, sze);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="attackFrom"></param>
		/// <param name="atkType"></param>
		/// <param name="sze"></param>
		/// <returns>True if template changed.</returns>
		public bool Recalculate(GridTile attackFrom, AreaEffectType atkType, Vector2Int sze)
		{
			if (centerTile == attackFrom
				&& attackType == atkType
				&& size == sze)
			{
				foreach (var highlight in highlights)
				{
#if DEBUG
					if (highlight == null)
					{
						Clear();
						return Recalculate(attackFrom, atkType, sze);
					}
#endif
					if (highlight.transform.parent != ObjectForge.instance.sleepingPooledObjectsParentTransform) // need to get rid of this
						highlight.gameObject.SetActive(true);
				}

				return false;
			}

			Clear();


			centerTile = attackFrom;
			attackType = atkType;
			this.size = sze;

			switch (attackType)
			{
				case AreaEffectType.Single:
				{
					TerrainHighlighter highlight = GridManager.instance.CreateHighlight(centerTile, HighlightType.AttackTemplate);

					highlight.SetText($"{centerTile.position.x},{centerTile.position.y}");
					highlights.Add(highlight);
				}
				break;

				case AreaEffectType.Rectangle:
				{
					Vector2 sizeReal = new Vector2();
					sizeReal.x = Mathf.Max(size.x, 1);
					sizeReal.y = Mathf.Max(size.y, 1);

					var visited = new Dictionary<Vector2Int, TerrainHighlighter>();
					Vector2Int nextTilePos = centerTile.position;

					var lastHighs = new Dictionary<Vector2Int, TerrainHighlighter>();
					lastHighs.Add(nextTilePos, NextHighlight(nextTilePos, HighlightType.AttackTemplate, visited));

					var totalTiles = sizeReal.x * sizeReal.y;
					var currentDir = CardinalDirection.Up;
					var nextDir = (currentDir == CardinalDirection.Left) ? CardinalDirection.Up : currentDir + 1;
					var sizeToCheck = (currentDir == CardinalDirection.Up || currentDir == CardinalDirection.Down) ? sizeReal.y : sizeReal.x;
					int tilesOnThisLine = 1;
					for (int i = 1; i < totalTiles; ++i)
					{
						var emptyCheck = nextTilePos.GetNeighbourPos(nextDir);
						if (!visited.ContainsKey(emptyCheck))
						{
							currentDir = nextDir;
							nextDir = (currentDir == CardinalDirection.Left) ? CardinalDirection.Up : currentDir + 1;
							sizeToCheck = (currentDir == CardinalDirection.Up || currentDir == CardinalDirection.Down) ? sizeReal.y : sizeReal.x;
							tilesOnThisLine = 1;
						}

						if (tilesOnThisLine >= sizeToCheck)
						{
							currentDir = nextDir;
							nextDir = (currentDir == CardinalDirection.Left) ? CardinalDirection.Up : currentDir + 1;
							sizeToCheck = (currentDir == CardinalDirection.Up || currentDir == CardinalDirection.Down) ? sizeReal.y : sizeReal.x;
							tilesOnThisLine = 1;
							nextTilePos = nextTilePos.GetNeighbourPos(currentDir);
							while (visited.ContainsKey(nextTilePos))
							{
								nextTilePos = nextTilePos.GetNeighbourPos(currentDir);
								++tilesOnThisLine;
							}

							if (tilesOnThisLine >= sizeToCheck)
								break;

							tilesOnThisLine = 1;
						}
						else
						{
							nextTilePos = nextTilePos.GetNeighbourPos(currentDir);
						}

						TerrainHighlighter highlight = NextHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
						highlight.SetText(i.ToString());
						++tilesOnThisLine;
					}

					//var minX = -(Mathf.CeilToInt(sizeReal.x / 2));
					//var minY = -(Mathf.CeilToInt(sizeReal.y / 2));
					//var maxX = minX + sizeReal.x;
					//var maxY = minY + sizeReal.y;
					//for (int x = minX; x < maxX; ++x)
					//{
					//	for (int y = minY; y < maxY; ++y)
					//	{
					//		Vector2Int nextTilePos = centerTile.position;
					//		nextTilePos.x += x;
					//		nextTilePos.y += y;
					//		TerrainHighlighter highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);

					//		highlight.SetText($"{x},{y}");
					//		highlights.Add(highlight);
					//	}
					//}
				}
				break;

				case AreaEffectType.Cross:
				{
					Vector2 sizeReal = new Vector2();
					sizeReal.x = Mathf.Max(size.x, 1);
					sizeReal.y = Mathf.Max(size.y, 1);

					var visited = new Dictionary<Vector2Int, TerrainHighlighter>();
					Vector2Int nextTilePos = centerTile.position;
					TerrainHighlighter highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
					highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
					highlights.Add(highlight);
					for (int x = 1; x <= sizeReal.x; ++x)
					{
						nextTilePos.x = centerTile.position.x + x;
						highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
						highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
						highlights.Add(highlight);
						nextTilePos.x = centerTile.position.x - x;
						highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
						highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
						highlights.Add(highlight);
					}

					nextTilePos.x = centerTile.position.x;
					for (int y = 1; y <= sizeReal.y; ++y)
					{
						nextTilePos.y = centerTile.position.y + y;
						highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
						highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
						highlights.Add(highlight);
						nextTilePos.y = centerTile.position.y - y;
						highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
						highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
						highlights.Add(highlight);
					}
				}
				break;

				case AreaEffectType.Diamond:
				{
					int maxRange = Mathf.Max(size.x, 1);
					int minRange = Mathf.Max(size.y, 0);
					if (minRange > maxRange)
					{
						Debug.LogWarning("MinRange of an attack cannot be larger than MaxRange.");
						break;
					}

					var visited = new Dictionary<Vector2Int, TerrainHighlighter>();
					Vector2Int nextTilePos = centerTile.position;

					var lastHighs = new HashSet<Vector2Int>();
					lastHighs.Add(nextTilePos);

					TerrainHighlighter highlight;
					if (minRange == 0)
					{
						highlight = NextHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
						highlight.SetText("center");
					}
					else
						visited.Add(nextTilePos, null);
					for (int i = 0; i < maxRange; ++i)
					{
						var theseHighs = new HashSet<Vector2Int>();
						foreach (var high in lastHighs)
						{
							foreach (CardinalDirection dir in Enum.GetValues(typeof(CardinalDirection)))
							{
								nextTilePos = high.GetNeighbourPos(dir);
								if (visited.ContainsKey(nextTilePos))
									continue;

								theseHighs.Add(nextTilePos);
								// @NOTE(Tristan): clamping the magnitude with Mathf.CeilToInt creates a diamond shaped deadzone.
								// for now, I prefer the square deadzone. Perhaps this could be set to a toggle?
								if (/*Mathf.CeilToInt*/((centerTile.position - nextTilePos).magnitude) < minRange)
								{
									visited.Add(nextTilePos, null);
									continue;
								}

								highlight = NextHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
								highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
							}
						}

						lastHighs = theseHighs;
					}
				}
				break;

				case AreaEffectType.Cone:
				{
					Vector2 sizeReal = new Vector2();
					sizeReal.x = Mathf.Max(size.x, 1);
					var visited = new Dictionary<Vector2Int, TerrainHighlighter>();
					Vector2Int nextTilePos = centerTile.position;
					int minX = 0;
					int maxX = 0;
					for (int y = 1; y <= sizeReal.x; ++y)
					{
						for (int x = minX; x <= maxX; ++x)
						{
							nextTilePos.x = centerTile.position.x + x;
							nextTilePos.y = centerTile.position.y + y;
							var highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
							highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
							highlights.Add(highlight);
						}

						minX -= 1;
						maxX += 1;
					}

				}
				break;
			}

			foreach (var high in highlights)
				affectedTiles.Add(high.position);
			return true;
		}

		private TerrainHighlighter NextHighlight(Vector2Int nextTilePos, HighlightType attackTemplate, Dictionary<Vector2Int, TerrainHighlighter> visited)
		{
			var highlight = GridManager.instance.CreateHighlight(nextTilePos, HighlightType.AttackTemplate, visited);
			highlights.Add(highlight);
			return highlight;
		}

		public void Hide()
		{
			foreach (var high in highlights)
			{
#if DEBUG
				if (high == null)
					continue;
#endif
				high.gameObject.SetActive(false);
			}
		}
		public void SlideTemplate(GridTile newCenterTile)
		{
			Vector2Int diff = newCenterTile.position - centerTile.position;
			centerTile = newCenterTile;
			foreach (var highlight in highlights)
			{
				var nextTilePos = highlight.position + diff;
				highlight.position = nextTilePos;
				GridManager.instance.TryGetTile(nextTilePos, out GridTile nextTile);
				highlight.SetTileHighlight(HighlightType.AttackTemplate, nextTile);
				highlight.SetText($"{nextTilePos.x},{nextTilePos.y}");
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
				high.ReturnToPool();
			}

			highlights.Clear();
			affectedTiles.Clear();
		}
	}
}