using AtomosZ.EditorZ;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace AtomosZ.MG2eTraveller.Starmap.EditorZ
{

	//[CustomEditor(typeof(_))]
	//public class _Editor : EditorEx
	//{
	//	private _ +;

	//	void OnEnable()
	//	{
	//		+ = (_)target;
	//	}
	//	public override void OnInspectorGUI()
	//	{
	//		BeginChangeCheck();
	//		base.OnInspectorGUI();



	//		serializedObject.ApplyModifiedProperties();
	//		if (EndChangeCheck())
	//		{

	//		}
	//	}
	//}

	[CustomEditor(typeof(Starmap))]
	public class StarmapEditor : EditorEx
	{
		private Starmap starmap;
		private int jump = 1;
		private Vector2Int dest = new Vector2Int(4, 4);

		void OnEnable()
		{
			starmap = (Starmap)target;
		}
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			dest = EditorGUILayout.Vector2IntField("Destination", dest);
			jump = EditorGUILayout.IntSlider(jump, 1, 6);
			if (Button("ShowPath"))
			{
				starmap.DrawJumpPath(new SystemCoordinates(4, 4), new SystemCoordinates(dest.x, dest.y), jump);
			}

			if (Button("Show Jump " + jump))
			{
				starmap.ShowJumpRange(new SystemCoordinates(dest.x, dest.y), jump);
			}

			if (Button("Regenerate Sector"))
			{
				starmap.GenerateSector();
			}

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}
	}

	[CustomEditor(typeof(Fleet))]
	public class FleetEditor : EditorEx
	{
		private Fleet fleet;
		private Vector3 lastPos;

		void OnEnable()
		{
			fleet = (Fleet)target;
		}

		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();



			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{
				fleet.UpdatePosition_EDITOR();
			}
		}

		void OnSceneGUI()
		{
			var pos = fleet.transform.position;
			if (pos != lastPos || fleet.sectorTilemap != null)
			{
				var size = fleet.boxCollider.size;
				LayerMask layer = LayerMask.GetMask("TerrainTiles");
				var starCollider = Physics2D.OverlapPoint(new Vector2(pos.x, pos.y), layer);
				if (starCollider != null)
				{
					var star = starCollider.GetComponent<StarSystem>();
					if (star != null)
					{
						fleet.UpdatePosition_EDITOR();
					}
				}

				lastPos = pos;
			}
		}
	}


	[CustomEditor(typeof(SubSectorMap))]
	public class SubSectorMapEditor : EditorEx
	{
		private SubSectorMap map;

		void OnEnable()
		{
			map = (SubSectorMap)target;
		}
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}
	}

	[CustomEditor(typeof(StarSystem))]
	public class StarSystemEditor : EditorEx
	{
		private StarSystem star;
		private Vector3 lastPos;

		void OnEnable()
		{
			star = (StarSystem)target;
		}

		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();



			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}

		void OnSceneGUI()
		{
			var pos = star.transform.position;
			if (pos != lastPos || star.tilemap == null)
			{
				var size = star.starCollider.radius;
				LayerMask layer = LayerMask.GetMask("TerrainTiles");
				var tileCollider = Physics2D.OverlapPoint(new Vector2(pos.x, pos.y), layer);
				if (tileCollider != null)
				{
					//var tile = tileCollider.GetComponent<GridTile>();
					//widget.RemoveFromTile();
					//if (!tile.AttachWidget(widget))
					//	widget.GetComponent<SpriteRenderer>().color = Color.red;
					//else
					//	widget.GetComponent<SpriteRenderer>().color = Color.white;
				}

				lastPos = pos;
			}
		}
	}
}
