using System;
using AtomosZ.UI;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.EditorZ
{
	/// <summary>
	/// Method Attributes that will come in handy:<br/>
	/// [InitializeOnLoadMethod]		// mark a static method so that it is called when the Unity Editor is first opened<br/>
	/// [ContextMenu("Update Mesh")]	// adds method to a script's Context Menu<br/>
	/// [Header("Health Settings")]		// add a header above some fields in the Inspector.<br/>
	/// [HideInCallstack]				// hide from the Console window callstack. When you hide these methods they are removed from the detail area of the selected message in the Console window. <br/>
	/// [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType)]
	///									// Use this attribute to get a callback when the runtime is starting up and loading the first scene.
	///	[DebuggerStepThrough]
	/// 
	///	Enum Attributes: <br/>
	/// [InspectorName("16 bits")]		// attribute on enum value declarations to change the display name shown in the Inspector.
	/// 
	/// Field Attributes: <br/>
	/// [MultilineAttribute]			// attribute to make a string be edited with a multi-line textfield.
	/// [ContextMenuItem("{function}", "menu item}")]
	///									//  add a context menu to a field that calls a named method<br/>
	/// [NonReorderable]				// Disables reordering of an array or list in the Inspector window.
	/// [Space(10)]						// 10 pixels of spacing here.
	/// [TextAreaAttribute()]				// Attribute to make a string be edited with a height-flexible and scrollable text area.
	/// </summary>
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


		public SerializedProperty Property(string propertyName)
		{
			var prop = FindProperty(propertyName);
			PropertyField(prop);
			return prop;
		}

		public SerializedProperty FindProperty(string propertyName)
		{
			return serializedObject.FindProperty(propertyName);
		}

		public bool PropertyField(SerializedProperty labelDataProp)
		{
			return EditorGUILayout.PropertyField(labelDataProp);
		}

		public bool PropertyField(SerializedProperty labelDataProp, GUIContent label, params GUILayoutOption[] options)
		{
			return EditorGUILayout.PropertyField(labelDataProp, label, options);
		}

		public bool PropertyField(SerializedProperty labelDataProp, params GUILayoutOption[] options)
		{
			return EditorGUILayout.PropertyField(labelDataProp, options);
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

		internal void SODataDisplay<T, V>(string foldoutLabel, SerializedProperty prop,
			ref Editor editor, ref bool isFoldout) where T : ScriptableObject where V : Editor
		{
			BeginHorizontal();
			{
				isFoldout = EditorGUILayout.Foldout(isFoldout, foldoutLabel, true);
				EditorGUILayout.PropertyField(prop, GUIContent.none);
			}
			EndHorizontal();

			if (isFoldout && prop.boxedValue != null)
			{
				++EditorGUI.indentLevel;

				if (prop.boxedValue != null)
				{
					GUI.enabled = false;
					Editor.CreateCachedEditor((T)prop.boxedValue, typeof(V), ref editor);
					editor.OnInspectorGUI();
					GUI.enabled = true;
				}
				--EditorGUI.indentLevel;
			}
		}

		public void CreateScriptObjectEditor<T,V>(string foldoutLabel, SerializedProperty prop,
			T oldValue, ref Editor scriptObjEditor, ref bool isFoldout, IUIBehavior dataOwner,
			Action<T> updateBackingData) where T : ScriptableObject where V : Editor
		{
			BeginHorizontal();
			{
				isFoldout = EditorGUILayout.Foldout(isFoldout, foldoutLabel, true);
				EditorGUILayout.PropertyField(prop, GUIContent.none);
			}
			EndHorizontal();

			T newValue = (T)prop.boxedValue;
			if (isFoldout && newValue != null)
			{
				++indentLevel;
				Editor.CreateCachedEditor(newValue, typeof(V), ref scriptObjEditor);
				scriptObjEditor.OnInspectorGUI();
				if (dataOwner != null) // this is null when the caller is a ScriptableObject editor.
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (oldValue != newValue
						|| GUILayout.Button($"Reset To ScriptableObject data", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
					{
						updateBackingData(newValue);

						EditorUtility.SetDirty(dataOwner.uIMonoBehaviour);
						dataOwner.UpdateBackingData();
						dataOwner.uIMonoBehaviour.RecordPrefabInstances();
					}
					EditorGUILayout.EndHorizontal();
	}

				--indentLevel;
			}
		}
	}
}