using UnityEditor;
using UnityEngine;

namespace AtomosZ.EditorZ
{
	public class EditorEx : Editor
	{
		public int indentLevel
		{
			get { return EditorGUI.indentLevel; }
			set { EditorGUI.indentLevel = value; }
		}

		public void BeginChangeCheck()
		{
			EditorGUI.BeginChangeCheck();
		}

		public bool EndChangeCheck()
		{
			return EditorGUI.EndChangeCheck();
		}

		public Rect BeginHorizontal()
		{
			return EditorGUILayout.BeginHorizontal();
		}

		public void EndHorizontal()
		{
			EditorGUILayout.EndHorizontal();
		}

		public void FlexibleSpace()
		{
			GUILayout.FlexibleSpace();
		}

		public SerializedProperty FindProperty(string propertName)
		{
			return serializedObject.FindProperty(propertName);
		}

		public bool PropertyField(SerializedProperty labelDataProp)
		{
			return EditorGUILayout.PropertyField(labelDataProp);
		}

		public void CreateBorder(float height)
		{
			GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(height));
		}

		public bool CreateFoldout(ref bool isFoldedOut, string text)
		{
			return isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, text, true);
		}

		public bool Button(string buttonText)
		{
			return GUILayout.Button(buttonText);
		}

		public bool Toggle(string text, ref bool isToggled)
		{
			return isToggled = EditorGUILayout.Toggle(text, isToggled);
		}

		public Vector2Int Vector2IntField(string text, Vector2Int vec)
		{
			return EditorGUILayout.Vector2IntField(text, vec);
		}
	}
}