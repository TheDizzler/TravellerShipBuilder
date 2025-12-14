using System;
using System.Collections.Generic;
using System.Reflection;
using AtomosZ.DFDQ.Battle;
using AtomosZ.DFDQ.Battle.Units;
using AtomosZ.DFDQ.Tiles;
using AtomosZ.DFDQ.Tiles.Widgets;
using AtomosZ.EditorZ;
using UnityEditor;
using UnityEngine;
using static AtomosZ.DFDQ.AbilityProfile;
using static AtomosZ.ObjectForge;
using Random = UnityEngine.Random;


namespace AtomosZ.DFDQ.EditorZ
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




	[CustomEditor(typeof(ScriptableUnit))]
	public class ScriptableUnitEditor : EditorEx
	{
		private ScriptableUnit unit;

		void OnEnable()
		{
			unit = (ScriptableUnit)target;
		}
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			if (Button("Get Color Data"))
			{
				unit.GetPaletteFromTexture();
			}

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}
	}

	[CustomEditor(typeof(LineOfSightRenderer))]
	public class LineOfSightRendererEditor : EditorEx
	{
		private LineOfSightRenderer line;
		private Vector2Int sourceTile;
		private Vector2Int targetTile;

		void OnEnable()
		{
			line = (LineOfSightRenderer)target;
		}
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			//sourceTile = Vector2IntField("Source", sourceTile);
			//targetTile = Vector2IntField("Target", targetTile);
			//if (Button("Show line"))
			//{
			//	line.HasLineOfSight(sourceTile, targetTile);
			//}

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}

		//public void OnSceneGUI()
		//{
		//	Handles.color = line.hasLOS ? Color.green : Color.red;
		//	Handles.DrawLine(new Vector3(sourceTile.x, sourceTile.y), new Vector3(targetTile.x, targetTile.y));
		//}
	}

	[CustomEditor(typeof(PathLineRenderer))]
	public class PathLineRendererEditor : EditorEx
	{
		private PathLineRenderer line;
		private Vector2Int waypoint;

		void OnEnable()
		{
			line = (PathLineRenderer)target;
		}
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			//waypoint = EditorGUILayout.Vector2IntField("Way point", waypoint);
			//if (Button("Set Waypoint"))
			//{
			//	line.SetWaypoint(waypoint);
			//	var rand = Random.Range(0, 2);
			//	if (rand == 0)
			//		waypoint.x += 1;
			//	else
			//		waypoint.y += 1;


			//}

			//if (Button("Clear"))
			//	line.ClearPath();

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}
	}


	[CustomEditor(typeof(TerrainWidget))]
	public class TerrainWidgetEditor : EditorEx
	{
		private TerrainWidget widget;
		private Vector3 lastPos;

		void OnEnable()
		{
			widget = (TerrainWidget)target;
			lastPos = widget.transform.position;
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
			var pos = widget.transform.position;
			if (pos != lastPos || widget.tile == null)
			{
				var size = widget.boxCollider.size;
				LayerMask layer = LayerMask.GetMask("TerrainTiles");
				var tileCollider = Physics2D.OverlapPoint(new Vector2(pos.x, pos.y), layer);
				if (tileCollider != null)
				{
					var tile = tileCollider.GetComponent<GridTile>();
					widget.RemoveFromTile();
					if (!tile.AttachWidget(widget))
						widget.GetComponent<SpriteRenderer>().color = Color.red;
					else
						widget.GetComponent<SpriteRenderer>().color = Color.white;
				}

				lastPos = pos;
			}
		}
	}

	[CustomEditor(typeof(DestructableObject))]
	public class DestructableObjectEditor : EditorEx
	{
		private DestructableObject destObj;

		void OnEnable()
		{
			destObj = (DestructableObject)target;
		}
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			GUILayout.BeginHorizontal();
			if (Button("Damage Wall"))
			{
				destObj.TakeDamage();
			}

			if (Button("Repair Wall"))
			{
				destObj.RepairDamage();
			}
			GUILayout.EndHorizontal();

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}
	}

	[CustomEditor(typeof(BaseUnit))]
	public class BaseUnitEditor : EditorEx
	{
		private BaseUnit unit;
		private Vector3 lastPos;
		private SerializedProperty movePointProp;
		private SerializedProperty isShowMoveProp;
		private SerializedProperty isShowAttackProp;
		private GridTile lastLastTile;
		private GridTile lastTile;

		void OnEnable()
		{
			unit = (BaseUnit)target;
			lastPos = unit.transform.position;
			movePointProp = FindProperty("movePoints");
			isShowMoveProp = FindProperty("isShowMove");
			isShowAttackProp = FindProperty("isShowAttack");
		}
		public override void OnInspectorGUI()
		{
			bool oldMove = isShowMoveProp.boolValue;
			bool oldAttack = isShowAttackProp.boolValue;
			BeginChangeCheck();
			base.OnInspectorGUI();


			if (isShowMoveProp.boolValue)
			{
				if (oldMove != isShowMoveProp.boolValue)
					unit.ShowPossibleMove();
			}
			else
			{
				if (oldMove != isShowMoveProp.boolValue)
					unit.HideMove();
			}

			if (isShowAttackProp.boolValue)
			{
				unit.ShowAbility(lastTile);
			}
			else
			{
				unit.HideAbility();
			}

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{
			}
		}

		void OnSceneGUI()
		{
			var pos = unit.transform.position;
			if (pos != lastPos || unit.occupiedTile == null)
			{
				var size = unit.boxCollider.size;
				LayerMask layer = LayerMask.GetMask("TerrainTiles");
				var tileCollider = Physics2D.OverlapPoint(new Vector2(pos.x, pos.y), layer);
				if (tileCollider != null)
				{
					var tile = tileCollider.GetComponent<GridTile>();
					unit.RemoveFromTile();
					if (!tile.SetUnit(unit))
						unit.GetComponent<SpriteRenderer>().color = Color.red;
					else
						unit.GetComponent<SpriteRenderer>().color = Color.white;
				}

				lastPos = pos;
			}

			if (isShowAttackProp.boolValue)
			{
				var mousePos = Helpers.GetSceneViewMousePosition();
				var tileCollider = Physics2D.OverlapPoint(new Vector2(mousePos.x, mousePos.y), GridManager.tileLayer);
				if (tileCollider != null)
				{
					var tile = tileCollider.GetComponent<GridTile>();
					if (tile != lastTile)
					{
						if (lastLastTile == tile)
							return; // mousePos is not consistent between frames. This hack prevents the path from reconstructing when on a tile edge.
									// BUT it feels sticky when back tracking between two tiles.
						lastLastTile = lastTile;
						lastTile = tile;
					}

				}
			}

			if (isShowMoveProp.boolValue)
			{
				var mousePos = Helpers.GetSceneViewMousePosition();
				LayerMask layer = LayerMask.GetMask("TerrainTiles");
				var tileCollider = Physics2D.OverlapPoint(new Vector2(mousePos.x, mousePos.y), layer);
				if (tileCollider == null)
					unit.ShowPossibleMove();
				else
				{
					var tile = tileCollider.GetComponent<GridTile>();
					if (tile != lastTile)
					{
						if (lastLastTile == tile)
							return; // mousePos is not consistent between frames. This hack prevents the path from reconstructing when on a tile edge.
									// BUT it feels sticky when back tracking between two tiles.
						unit.ShowPathTo(tile.position);
						lastLastTile = lastTile;
						lastTile = tile;
					}
				}
			}

		}
	}

	[CustomEditor(typeof(TerrainHighlighter))]
	public class TerrainHighlighterEditor : EditorEx
	{
		private TerrainHighlighter highlighter;
		private SerializedProperty borderProp;
		private SerializedProperty borderColorProp;
		private SerializedProperty transColorProp;

		void OnEnable()
		{
			highlighter = (TerrainHighlighter)target;
			borderProp = FindProperty("activeBorders");
			borderColorProp = FindProperty("borderColor");
			transColorProp = FindProperty("transparentColor");
		}

		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{
				//highlighter.SetBorders((Tiles.TerrainHighlighter.Borders)borderProp.enumValueFlag);
				//highlighter.SetBorderColor((Tiles.TerrainHighlighter.BorderColor)borderColorProp.enumValueIndex);
				//highlighter.SetTransparentColor((TerrainHighlighter.TransparentColor)transColorProp.enumValueIndex);
			}
		}
	}

	[CustomEditor(typeof(UnitManager))]
	public class UnitManagerEditor : EditorEx
	{
		private UnitManager unitManager;
		private SerializedProperty unitsProp;
		private SerializedProperty unitData;
		private int lastUnitIndex;
		private int unitDataIndex;

		void OnEnable()
		{
			unitManager = (UnitManager)target;

			unitsProp = FindProperty("unitsInField");
			unitData = FindProperty("unitData");
		}

		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();


			if (Button("Refresh Unit Data"))
				unitManager.RefreshUnitList();

			BeginHorizontal();
			{
				unitDataIndex = EditorGUILayout.IntPopup("Unit Data", unitDataIndex, unitManager.prefabNames, unitManager.prefabIndices);
				if (Button("Create " + unitManager.unitData[unitDataIndex].name))
				{
					unitManager.SpawnUnit(unitManager.unitData[unitDataIndex]);
				}
			}
			EndHorizontal();


			BeginHorizontal();
			{
				GUILayout.Label("Unit Index");
				GUI.enabled = unitsProp.arraySize > 0;
				lastUnitIndex = EditorGUILayout.IntSlider(lastUnitIndex, 0, unitsProp.arraySize - 1);


				if (unitManager.unitsInField.Count > 0 && Button("Delete Unit " + unitManager.unitsInField[lastUnitIndex].unitName))
				{
					unitManager.DeleteUnit(lastUnitIndex);
					if (lastUnitIndex > unitsProp.arraySize - 2)
						lastUnitIndex = 0;
				}

				GUI.enabled = true;
			}
			EndHorizontal();


			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}
	}



	[CustomEditor(typeof(GridManager))]
	public class GridManagerEditor : EditorEx
	{
		private GridManager grid;
		private static bool showAttackIcon = false;

		void OnEnable()
		{
			grid = (GridManager)target;
			SceneView.duringSceneGui += DuringSceneGUI;
		}

		private void OnDisable()
		{
			SceneView.duringSceneGui -= DuringSceneGUI;
			grid.HideAttack();
		}

		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			base.OnInspectorGUI();

			if (Button("Create grid"))
			{
				grid.GenerateGrid();
			}

			if (Button("Clear Highlights"))
				grid.DestroyAllHighlights();

			Toggle("Show Attack Highlight", ref showAttackIcon);

			if (Button("Clear Pool"))
				grid.ClearPool();

			if (Button("Sleep Pool"))
				grid.HidePool();
			if (Button("Wake Pool"))
				grid.WakePool();
			if (Button("Create Pool "))
			{
				grid.TestCreatePool();
			}

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{

			}
		}

		private void DuringSceneGUI(SceneView view)
		{
			if (showAttackIcon)
			{
				var pos = Helpers.GetSceneViewMousePosition();
				grid.ShowAttackHighlight(pos,
					(AreaEffectType)FindProperty("testAttack").enumValueIndex,
					FindProperty("testAttackSize").vector2IntValue);
			}
		}

	}
}