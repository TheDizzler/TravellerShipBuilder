using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GridManager : MonoBehaviour
{
	[SerializeField] private int width, height;
	[SerializeField] private GridTile tilePrefab;
	[SerializeField] private Transform camTrans;

	private Dictionary<Vector2, GridTile> tiles = new();

	void Start()
	{
		GenerateGrid();
	}

	void GenerateGrid()
	{
		for (int x = 0; x < width; ++x)
		{
			for (int y = 0; y < height; ++y)
			{
				var tile = Instantiate(tilePrefab, new Vector3(x, y), Quaternion.identity);
				tile.name = $"Tile ({x},{y})";

				var isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
				tile.Init(isOffset);

				tiles[new Vector2(x, y)] = tile;
			}
		}

		camTrans.position = new Vector3((float)width / 2 - .5f, (float)height / 2 - .5f, -10);
	}

	public GridTile GetTileAtPosition(Vector2 pos)
	{
		if (tiles.TryGetValue(pos, out GridTile tile))
			return tile;
		return null;
	}
}
