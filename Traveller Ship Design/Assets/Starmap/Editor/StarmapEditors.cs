using AtomosZ.EditorZ;
using UnityEditor;
using UnityEngine;

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


	[CustomEditor(typeof(SubSectorMap))]
	public class _Editor : EditorEx
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


			if (Button("Create SubSector"))
			{
				map.FillSubSector();
			}

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


			if (Button("Highlight"))
			{
				star.SetHighlightTest();
			}

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
