using AtomosZ.Interceptor;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.EditorZ.Interceptor
{
	[CustomEditor(typeof(Terrainizer))]
	public class TerrainizerEditor : EditorEx
	{
		private Terrainizer grid;

		void OnEnable()
		{
			grid = (Terrainizer)target;
		}

		public override void OnInspectorGUI()
		{
			BeginChangeCheck();

			base.OnInspectorGUI();

			if (Button("Fill map"))
			{
				grid.FillBasePlane();
			}

			bool changed = serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck() || changed)
			{

			}
		}
	}
}