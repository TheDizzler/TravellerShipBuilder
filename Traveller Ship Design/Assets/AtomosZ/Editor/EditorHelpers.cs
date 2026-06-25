using System;
using System.Diagnostics;
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
	///									// add a context menu to a field that calls a named method<br/>
	/// [NonReorderable]				// Disables reordering of an array or list in the Inspector window.
	/// [Space(10)]						// 10 pixels of spacing.
	/// [TextAreaAttribute()]			// Attribute to make a string be edited with a height-flexible and scrollable text area.
	/// </summary>
	public class EditorEx : Editor
	{
		public int indentLevel
		{
			get { return EditorGUI.indentLevel; }
			set { EditorGUI.indentLevel = value; }
		}
		[DebuggerStepThrough]
		public void BeginChangeCheck()
		{
			EditorGUI.BeginChangeCheck();
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool EndChangeCheck()
		{
			return EditorGUI.EndChangeCheck();
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public Rect BeginHorizontal()
		{
			return EditorGUILayout.BeginHorizontal();
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public void EndHorizontal()
		{
			EditorGUILayout.EndHorizontal();
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public void FlexibleSpace()
		{
			GUILayout.FlexibleSpace();
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool ApplyModifiedProperties()
		{
			return serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// <code>var prop = serializedObject.FindProperty(propertyName);
		/// EditorGUILayout.PropertyField(prop);
		/// return prop;</code>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		[HideInCallstack]
		public SerializedProperty Property(string propertyName)
		{
			var prop = serializedObject.FindProperty(propertyName);
			EditorGUILayout.PropertyField(prop);
			return prop;
		}

		/// <summary>
		/// <c>return serializedObject.FindProperty(propertyName);</c>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		[HideInCallstack]
		public SerializedProperty FindProperty(string propertyName)
		{
			return serializedObject.FindProperty(propertyName);
		}
		/// <summary>
		/// <c>return EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName));</c>
		/// </summary>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool PropertyField(string propertyName)
		{
			return EditorGUILayout.PropertyField(serializedObject.FindProperty(propertyName));
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool PropertyField(SerializedProperty labelDataProp)
		{
			return EditorGUILayout.PropertyField(labelDataProp);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool PropertyField(SerializedProperty labelDataProp, GUIContent label, params GUILayoutOption[] options)
		{
			return EditorGUILayout.PropertyField(labelDataProp, label, options);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool PropertyField(SerializedProperty labelDataProp, params GUILayoutOption[] options)
		{
			return EditorGUILayout.PropertyField(labelDataProp, options);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public void CreateLabel(string labelText)
		{
			EditorGUILayout.LabelField(labelText);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public int CreateIntSlider(string labelText, int value, int leftValue, int rightValue)
		{
			value = Mathf.Min(value, rightValue);
			value = Mathf.Max(value, leftValue);
			return EditorGUILayout.IntSlider(labelText, value, leftValue, rightValue);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public void CreateBorder(float height)
		{
			GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(height));
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool CreateFoldout(ref bool isFoldedOut, string text)
		{
			return isFoldedOut = EditorGUILayout.Foldout(isFoldedOut, text, true);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool Button(string buttonText)
		{
			return GUILayout.Button(buttonText);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public bool Toggle(string text, ref bool isToggled)
		{
			return isToggled = EditorGUILayout.Toggle(text, isToggled);
		}
		[DebuggerStepThrough]
		[HideInCallstack]
		public Vector2Int Vector2IntField(string text, Vector2Int vec)
		{
			return EditorGUILayout.Vector2IntField(text, vec);
		}

		public bool IsInScene(bool showEditorGUILayoutMessage)
		{
			if (((MonoBehaviour)target).gameObject.scene.IsValid())
				return true;

			if (showEditorGUILayoutMessage)
				EditorGUILayout.LabelField("Drag into scene or open in prefab editor to edit");
			return false;
		}

		/// <summary>
		/// Use this for editing/viewing another a scriptableobject within another scriptableobject.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <typeparam name="V"></typeparam>
		/// <param name="foldoutLabel"></param>
		/// <param name="prop"></param>
		/// <param name="editor"></param>
		/// <param name="isFoldout"></param>
		/// <param name="canEdit"></param>
		internal void SODataDisplay<T, V>(string foldoutLabel, SerializedProperty prop,
			ref Editor editor, ref bool isFoldout, bool canEdit) where T : ScriptableObject where V : Editor
		{
			BeginHorizontal();
			{
				if (prop.boxedValue != null)
					isFoldout = EditorGUILayout.Foldout(isFoldout, foldoutLabel, true);
				EditorGUILayout.PropertyField(prop, GUIContent.none);
			}
			EndHorizontal();

			if (isFoldout && prop.boxedValue != null)
			{
				CreateBorder(2);
				++EditorGUI.indentLevel;

				GUI.enabled = canEdit;
				Editor.CreateCachedEditor((T)prop.boxedValue, typeof(V), ref editor);
				editor.OnInspectorGUI();
				GUI.enabled = true;

				--EditorGUI.indentLevel;
				CreateBorder(2);
			}
		}

		public void CreateScriptObjectEditor<T_ScriptableObject, V_ScriptableObjectEditor>(
			string foldoutLabel, SerializedProperty prop, T_ScriptableObject oldValue, ref Editor scriptObjEditor,
			ref bool isFoldout, IUIBehavior dataOwner, Action<T_ScriptableObject> UpdateBackingData)
			where T_ScriptableObject : ScriptableObject where V_ScriptableObjectEditor : Editor
		{
			BeginHorizontal();
			{
				isFoldout = EditorGUILayout.Foldout(isFoldout, foldoutLabel, true);
				EditorGUILayout.PropertyField(prop, GUIContent.none);
			}
			EndHorizontal();

			T_ScriptableObject newValue = (T_ScriptableObject)prop.boxedValue;
			if (isFoldout && newValue != null)
			{
				++indentLevel;
				Editor.CreateCachedEditor(newValue, typeof(V_ScriptableObjectEditor), ref scriptObjEditor);
				scriptObjEditor.OnInspectorGUI();
				if (dataOwner != null) // this is null when the caller is a ScriptableObject editor.
				{
					EditorGUILayout.BeginHorizontal();
					GUILayout.FlexibleSpace();
					if (oldValue != newValue
						|| GUILayout.Button($"Reset To ScriptableObject data", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
					{
						UpdateBackingData(newValue);

						EditorUtility.SetDirty(dataOwner.uIMonoBehaviour);
						dataOwner.RecalculateDimensions();
						dataOwner.uIMonoBehaviour.RecordPrefabInstances();
					}
					EditorGUILayout.EndHorizontal();
				}

				--indentLevel;
			}
		}
	}

	//public static class EditorExtensions
	//{
	//	public static void CreateScriptObjectEditor<T>(this EditorEx editor, Type editorType,
	//		T oldValue, T newValue, ref Editor scriptObjEditor, IUIBehavior dataOwner,
	//		Action<T> updateBackingData) where T : ScriptableObject
	//	{
	//		++editor.indentLevel;
	//		Editor.CreateCachedEditor(newValue, editorType, ref scriptObjEditor);
	//		scriptObjEditor.OnInspectorGUI();
	//		if (dataOwner != null)
	//		{
	//			EditorGUILayout.BeginHorizontal();
	//			GUILayout.FlexibleSpace();
	//			if (oldValue != newValue
	//				|| GUILayout.Button($"Reset To ScriptableObject data", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
	//			{
	//				updateBackingData(newValue);

	//				EditorUtility.SetDirty(dataOwner.designObject);
	//				dataOwner.UpdateBackingData();
	//				dataOwner.RecordPrefabInstances();
	//			}
	//			EditorGUILayout.EndHorizontal();
	//		}

	//		--editor.indentLevel;
	//	}
	//}
}