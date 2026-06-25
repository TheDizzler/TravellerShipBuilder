using System;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace AtomosZ.EditorZ
{
	[CustomPropertyDrawer(typeof(CustomDictionary), true)]
	public class CustomDictionaryDrawer : DrawerEx
	{
		public const float ELEMENT_HEIGHT_PADDING = 6f;
		public const float ELEMENT_SPACING = 10f;
		public const float ELEMENT_FOLDOUT_PADDING = 20f;

		public const float TOP_PADDING = 5f;
		public const float BOTTOM_PADDING = 5f;

		private const float KEY_SPLIT = 30;
		private const float VALUE_SPLIT = 50;

		private bool isExpanded;
		private ReorderableList list = null;
		private SerializedProperty keys;
		private SerializedProperty values;
		private string label;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			//EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			if (list == null)
			{
				keys = property.FindPropertyRelative(nameof(keys));
				values = property.FindPropertyRelative("values");
				if (values == null)
				{
					Log.Error("The Values type is not serializable");
					//return;
				}

				list = new ReorderableList(property.serializedObject, keys, true, true, true, true);

				list.drawHeaderCallback = DrawHeader;
				list.onAddCallback = Add;
				list.onRemoveCallback = Remove;
				list.elementHeightCallback = GetElementHeight;
				list.drawElementCallback = DrawElement;
				list.onReorderCallbackWithDetails += Reorder;

				this.label = $" {label.text}";
			}

			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			//++EditorGUI.indentLevel;

			position.y += TOP_PADDING;
			position.height -= TOP_PADDING + BOTTOM_PADDING;
			drawerHeight += TOP_PADDING + BOTTOM_PADDING;

			if (isExpanded)
				DrawList(ref position);
			else
				DrawCompleteHeader(ref position);


			//--EditorGUI.indentLevel;

			drawerHeight += isExpanded ? list.GetHeight() : list.headerHeight;
			//EditorGUI.EndProperty();
		}

		void DrawList(ref Rect rect)
		{
			EditorGUIUtility.labelWidth = 80f;
			EditorGUIUtility.fieldWidth = 80f;

			list.DoList(rect);
		}
		void DrawCompleteHeader(ref Rect rect)
		{
			ReorderableList.defaultBehaviours.DrawHeaderBackground(rect);

			rect.x += 6;
			rect.y += 0;

			DrawHeader(rect);
		}

		void DrawHeader(Rect rect)
		{
			rect.x += 10f;
			isExpanded = EditorGUI.Foldout(rect, isExpanded, label, true);
		}

		void Reorder(ReorderableList list, int oldIndex, int newIndex)
		{
			values.MoveArrayElement(oldIndex, newIndex);
		}

		void Add(ReorderableList list)
		{
			values.InsertArrayElementAtIndex(values.arraySize);
			ReorderableList.defaultBehaviours.DoAddButton(list);
		}

		void Remove(ReorderableList list)
		{
			values.DeleteArrayElementAtIndex(list.index);
			ReorderableList.defaultBehaviours.DoRemoveButton(list);
		}

		float GetElementHeight(int index)
		{
			SerializedProperty key = keys.GetArrayElementAtIndex(index);
			SerializedProperty value = values.GetArrayElementAtIndex(index);

			var kHeight = GetChildSingleHeight(key);
			var vHeight = GetChildSingleHeight(value);

			var max = Math.Max(kHeight, vHeight);

			if (max < EditorGUIUtility.singleLineHeight)
				max = EditorGUIUtility.singleLineHeight;

			return max + ELEMENT_HEIGHT_PADDING;
		}

		private float GetChildSingleHeight(SerializedProperty property)
		{
			if (IsInline(property))
				return EditorGUIUtility.singleLineHeight;

			var height = 0f;

			foreach (var child in IterateChildern(property))
				height += EditorGUIUtility.singleLineHeight + 2f;

			return height;
		}

		void DrawElement(Rect rect, int index, bool isActive, bool isFocused)
		{
			rect.height -= ELEMENT_HEIGHT_PADDING;
			rect.y += ELEMENT_HEIGHT_PADDING / 2;

			var areas = Split(rect, KEY_SPLIT, VALUE_SPLIT);

			DrawKey(areas[0], index);
			DrawValue(areas[1], index);
		}

		void DrawKey(Rect rect, int index)
		{
			var property = keys.GetArrayElementAtIndex(index);

			rect.x += ELEMENT_SPACING / 2f;
			rect.width -= ELEMENT_SPACING;

			DrawField(rect, property);
		}

		void DrawValue(Rect rect, int index)
		{
			var property = values.GetArrayElementAtIndex(index);

			rect.x += ELEMENT_SPACING / 2f;
			rect.width -= ELEMENT_SPACING;

			DrawField(rect, property);
		}

		void DrawField(Rect rect, SerializedProperty property)
		{
			rect.height = EditorGUIUtility.singleLineHeight;

			if (IsInline(property))
			{
				EditorGUI.PropertyField(rect, property, GUIContent.none);
			}
			else
			{
				rect.x += ELEMENT_SPACING / 2f;
				rect.width -= ELEMENT_SPACING;

				foreach (var child in IterateChildern(property))
				{
					EditorGUI.PropertyField(rect, child, false);

					rect.y += EditorGUIUtility.singleLineHeight + +2f;
				}
			}
		}

		static IEnumerable<SerializedProperty> IterateChildern(SerializedProperty property)
		{
			var path = property.propertyPath;

			property.Next(true);

			while (true)
			{
				yield return property;

				if (property.NextVisible(false) == false)
					break;
				if (property.propertyPath.StartsWith(path) == false)
					break;
			}
		}

		static Rect[] Split(Rect source, params float[] cuts)
		{
			var rects = new Rect[cuts.Length];

			var x = 0f;

			for (int i = 0; i < cuts.Length; i++)
			{
				rects[i] = new Rect(source);

				rects[i].x += x;
				rects[i].width *= cuts[i] / 100;

				x += rects[i].width;
			}

			return rects;
		}

		static bool IsInline(SerializedProperty property)
		{
			switch (property.propertyType)
			{
				case SerializedPropertyType.Generic:
					return property.hasVisibleChildren == false;
			}

			return true;
		}
	}


	public class DrawerEx : PropertyDrawer
	{
		public float drawerHeight;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return drawerHeight;
		}
	}

	public static class PropertyDrawerEx
	{
		public static void SetProperty(this PropertyDrawer propertyDrawer,
			SerializedProperty subProperty, Rect position, ref float drawerHeight)
		{
			if (subProperty == null)
			{
				Debug.LogException(new Exception($"subProperty is null"));
				return;
			}

			var propRect = new Rect(position.x, position.y + drawerHeight,
				position.width, EditorGUI.GetPropertyHeight(subProperty, true));
			EditorGUI.PropertyField(propRect, subProperty);
			drawerHeight += propRect.height + EditorGUIUtility.standardVerticalSpacing;
		}

		public static void SetProperty(this PropertyDrawer propertyDrawer, SerializedProperty mainProperty,
			string propertyName, Rect position, ref float drawerHeight, GUIContent label = null)
		{
			var prop = mainProperty.FindPropertyRelative(propertyName);
			if (prop == null)
			{
				Debug.LogException(new Exception($"No property found by name {propertyName} on {mainProperty.name}"));
				return;
			}

			var propRect = new Rect(position.x, position.y + drawerHeight,
				position.width, EditorGUI.GetPropertyHeight(prop, true));
			if (label != null)
				EditorGUI.PropertyField(propRect, prop, label);
			else
				EditorGUI.PropertyField(propRect, prop);
			drawerHeight += propRect.height + EditorGUIUtility.standardVerticalSpacing;
		}


		public static void CreateLabel(this PropertyDrawer propertyDrawer, string labelText, Rect position, ref float drawerHeight)
		{
			var labelRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(labelRect, labelText);
			drawerHeight += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
		}


		public static string CreateTextField(this PropertyDrawer propertyDrawer,
			string labelText, string textInField, Rect position, ref float drawerHeight)
		{
			var labelRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
			return EditorGUI.TextField(labelRect, labelText, textInField);
		}


		public static int CreateIntField(this PropertyDrawer propertyDrawer, int value, string label, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.IntField(rect, label, value);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}

		public static float CreateFloatField(this PropertyDrawer propertyDrawer, float value, string label, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.FloatField(rect, label, value);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}

		public static Vector2 CreateVector2Field(this PropertyDrawer propertyDrawer, Vector2 value, string label, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.Vector2Field(rect, label, value);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}

		public static Color CreateColorField(this PropertyDrawer propertyDrawer, Color value, string label, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.ColorField(rect, label, value);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}

		public static bool CreateToggle(this PropertyDrawer propertyDrawer, bool value, string label, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.Toggle(rect, label, value);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}


		public static int CreateSlider(this PropertyDrawer propertyDrawer, string label, int value,
			int leftValue, int rightValue, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.IntSlider(rect, label, value, leftValue, rightValue);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}


		public static float CreateSlider(this PropertyDrawer propertyDrawer, string label, float value,
			float leftValue, float rightValue, Rect position, ref float drawerHeight)
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = EditorGUI.Slider(rect, label, value, leftValue, rightValue);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}

		public static T CreateObjectField<T>(this PropertyDrawer propertyDrawer,
			UnityEngine.Object obj, string label, Rect position, ref float drawerHeight) where T : UnityEngine.Object
		{
			var rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			var result = (T)EditorGUI.ObjectField(rect, label, obj, typeof(T), true);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return result;
		}

		public static bool CreateFoldout(this PropertyDrawer propertyDrawer, string labelText, bool isExpanded, Rect position, ref float drawerHeight)
		{
			Rect rect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			bool newIsExpanded = EditorGUI.Foldout(rect, isExpanded, new GUIContent(labelText), true);
			drawerHeight += rect.height + EditorGUIUtility.standardVerticalSpacing;
			return newIsExpanded;
		}

		/// <summary>
		/// Bool
		/// </summary>
		/// <param name="scriptabelObjectIsNull"></param>
		/// <param name="scriptableObjectBoolValue">Not used if scriptabelObjectIsNull.</param>
		/// <param name="useCustomBoolProp"></param>
		/// <param name="nonScriptableObjectBoolProp"></param>
		/// <param name="position"></param>
		/// <param name="drawerHeight"></param>
		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer, bool scriptabelObjectIsNull,
			bool scriptableObjectBoolValue, SerializedProperty useCustomBoolProp,
			SerializedProperty nonScriptableObjectBoolProp, Rect position, ref float drawerHeight)
		{
			if (scriptabelObjectIsNull)
			{
				useCustomBoolProp.boolValue = true;
				SetProperty(propertyDrawer, nonScriptableObjectBoolProp, position, ref drawerHeight);
				//GUI.enabled = false;
				//this.SetProperty(useCustomBoolProp, position, ref drawerHeight); // should always be true
				//GUI.enabled = true;
			}
			else
			{
				SetProperty(propertyDrawer, nonScriptableObjectBoolProp, position, ref drawerHeight);
				useCustomBoolProp.boolValue = nonScriptableObjectBoolProp.boolValue != scriptableObjectBoolValue;

				GUI.enabled = false;
				SetProperty(propertyDrawer, useCustomBoolProp, position, ref drawerHeight); // is true sliderSO.showHandle != customShowHandleProp.boolValue
				GUI.enabled = true;
			}
		}


		/// <summary>
		/// TMP_FontAsset
		/// </summary>
		/// <param name="propertyDrawer"></param>
		/// <param name="overrideBoolProp"></param>
		/// <param name="overrideValueProp"></param>
		/// <param name="value"></param>
		/// <param name="position"></param>
		/// <param name="drawerHeight"></param>
		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer, SerializedProperty overrideBoolProp,
			SerializedProperty overrideValueProp, TMP_FontAsset value, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.ObjectField(minRect, "default value", value, typeof(TMP_FontAsset), false);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}

		/// <summary>
		/// Color
		/// </summary>
		/// <param name="propertyDrawer"></param>
		/// <param name="overrideBoolProp"></param>
		/// <param name="overrideValueProp"></param>
		/// <param name="value"></param>
		/// <param name="position"></param>
		/// <param name="drawerHeight"></param>
		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
		SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Color value, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.ColorField(minRect, "default value", value);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}

		/// <summary>
		/// Float
		/// </summary>
		/// <param name="propertyDrawer"></param>
		/// <param name="overrideBoolProp"></param>
		/// <param name="overrideValueProp"></param>
		/// <param name="defaultValueFromScriptableObject"></param>
		/// <param name="position"></param>
		/// <param name="drawerHeight"></param>
		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
		SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			float defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.FloatField(minRect, "default value", defaultValueFromScriptableObject);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}

		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
			SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Vector2 defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.Vector2Field(minRect, "default value", defaultValueFromScriptableObject);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}


		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
			SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Vector3 defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.Vector3Field(minRect, "default value", defaultValueFromScriptableObject);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}

		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
			SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Vector4 defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.Vector4Field(minRect, "default value", defaultValueFromScriptableObject);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}

		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
			SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Vector2Int defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.Vector2IntField(minRect, "default value", defaultValueFromScriptableObject);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}


		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
			SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Vector3Int defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.Vector3IntField(minRect, "default value", defaultValueFromScriptableObject);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}


		public static void SetOverridableProperty(this PropertyDrawer propertyDrawer,
		SerializedProperty overrideBoolProp, SerializedProperty overrideValueProp,
			Sprite defaultValueFromScriptableObject, Rect position, ref float drawerHeight)
		{
			SetProperty(propertyDrawer, overrideBoolProp, position, ref drawerHeight);
			if (overrideBoolProp.boolValue)
				SetProperty(propertyDrawer, overrideValueProp, position, ref drawerHeight);
			else
			{
				GUI.enabled = false;
				var minRect = new Rect(position.x, position.y + drawerHeight,
					position.width, EditorGUI.GetPropertyHeight(overrideValueProp, true));
				EditorGUI.ObjectField(minRect, "default sprite", defaultValueFromScriptableObject, typeof(Sprite), false);
				drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				GUI.enabled = true;
			}
		}
	}
}
