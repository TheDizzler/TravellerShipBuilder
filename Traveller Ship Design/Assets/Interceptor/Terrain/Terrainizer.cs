using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.Interceptor
{
	public class Terrainizer : MonoBehaviour
	{
		public Vector2 mapSize = new Vector2(4, 10);
		public Tilemap tilemap;

		[Tooltip("Size of one cell in grid")]
		public float cellSize = 1f;
		public GameObject baseTerrainTile;
		public GameObject objectToPlace;
		public GameObject ghostObject;
		public GameObject replaceObject;
		private bool isValidPos;
		private Dictionary<Renderer, Material> orgMats;


		void Start()
		{
			//FillBasePlane();
			CreateGhostObject();
		}

		public void FillBasePlane()
		{
			tilemap.transform.DeleteChildren();
			for (int x = 0; x < mapSize.x * 10; ++x)
			{
				for (int y = 0; y < mapSize.y * 10; ++y)
				{
					var worldPos = tilemap.CellToWorld(new Vector3Int(x, y, 0));
					Instantiate(baseTerrainTile, worldPos, Quaternion.identity, tilemap.transform);
				}
			}
		}

		void Update()
		{
			UpdateGhostPosition();
			if (Mouse.GetMouseButtonDown(0))
				PlaceObject();
		}

		public void CreateGhostObject()
		{
			ghostObject = Instantiate(objectToPlace);
			ghostObject.GetComponent<Collider>().enabled = false;

			Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();

			orgMats = new Dictionary<Renderer, Material>();
			foreach (var renderer in renderers)
			{
				Material mat = renderer.material;
				orgMats.Add(renderer, new Material(mat));

				Color color = mat.color;
				color.a = 0.5f;
				mat.color = color;

				mat.SetFloat("_Mode", 2);
				mat.SetInt("_ScrBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
				mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
				mat.SetInt("_ZWrite", 0);
				mat.DisableKeyword("_ALPHATEST_ON");
				mat.EnableKeyword("_ALPHABLEND_ON");
				mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
				mat.renderQueue = 3000;
			}
		}

		public void Button()
		{
			Debug.Log("!");
		}
		public void UpdateGhostPosition()
		{
			Ray ray = Helpers.camera.ScreenPointToRay(Mouse.pos);
			if (Physics.Raycast(ray, out RaycastHit hit))
			{
				Vector3 snappedPosition = new Vector3(
					Mathf.Round(hit.point.x / cellSize) * cellSize,
					Mathf.Round(hit.point.y / cellSize) * cellSize,
					Mathf.Round(hit.point.z / cellSize) * cellSize);

				ghostObject.transform.position = snappedPosition;

				var objectHit = hit.transform.gameObject;
				var tile = objectHit.GetComponent<TileBase>();
				if (tile.tileType == TileBase.TileType.Terrain)
				{
					replaceObject = objectHit;
					Debug.Log(objectHit.name);
				}

				isValidPos = true;
			}
		}

		public void SetGhostColor(Color color)
		{
			Renderer[] renderers = ghostObject.GetComponentsInChildren<Renderer>();
			foreach (Renderer renderer in renderers)
			{
				Material mat = renderer.material;
				mat.color = color;
			}
		}

		public void PlaceObject()
		{
			if (isValidPos)
			{
				if (replaceObject != null)
				{
					Helpers.SafeDelete(replaceObject);
					replaceObject = null;
				}

				ghostObject.GetComponent<Collider>().enabled = true;
				Vector3 placePos = ghostObject.transform.position;
				ghostObject.transform.SetParent(tilemap.transform, true);
				foreach (var rendererMat in orgMats)
				{
					rendererMat.Key.material = rendererMat.Value;
				}

				CreateGhostObject();
			}
		}
	}
}