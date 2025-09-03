using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

namespace AtomosZ.UI.EditorZ
{
	public static class PropertyDrawerEx
	{
		public static void SetProperty(SerializedProperty subProperty, Rect position, ref float currentDrawerHeight)
		{
			var propRect = new Rect(position.x, position.y + currentDrawerHeight,
				position.width, EditorGUI.GetPropertyHeight(subProperty, true));
			EditorGUI.PropertyField(propRect, subProperty);
			currentDrawerHeight += propRect.height + EditorGUIUtility.standardVerticalSpacing;
		}

		public static void SetProperty(SerializedProperty mainProperty, string propertyName, Rect position, ref float currentDrawerHeight)
		{
			var prop = mainProperty.FindPropertyRelative(propertyName);
			if (prop == null)
				throw new Exception($"No property found by name {propertyName} on {mainProperty.name}");
			var propRect = new Rect(position.x, position.y + currentDrawerHeight,
				position.width, EditorGUI.GetPropertyHeight(prop, true));
			EditorGUI.PropertyField(propRect, prop);
			currentDrawerHeight += propRect.height + EditorGUIUtility.standardVerticalSpacing;
		}


		public static void CreateLabel(string labelText, Rect position, ref float drawerHeight)
		{
			var labelRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(labelRect, labelText);
			drawerHeight += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
		}
	}


	[CustomPropertyDrawer(typeof(PanelControl))]
	public class PanelControlDrawer : PropertyDrawer
	{
		Dictionary<PanelControlType, float> heights;

		public PanelControlDrawer() : base()
		{
			heights = new();
			foreach (PanelControlType controlType in Enum.GetValues(typeof(PanelControlType)))
				heights.Add(controlType, 0);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			float drawerHeight = 0;

			var controlType = (PanelControlType)property.FindPropertyRelative("controlType").enumValueIndex;
			PropertyDrawerEx.CreateLabel(controlType.ToString(), position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property,
				PanelControl.panelControlNames[controlType], position, ref drawerHeight);


			var resetButtonRect = new Rect(position.x, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
			var addButtonRect = new Rect(position.x + 195, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);

			if (GUI.Button(resetButtonRect, "Reset to Default"))
			{
				var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
				var pi = (PanelControl)property.boxedValue;
				dpo.ResetToDefaults(pi);
				PrefabUtility.RecordPrefabInstancePropertyModifications(pi.uiDesignObject.transform);
			}

			if (GUI.Button(addButtonRect, "Remove"))
			{
				var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
				dpo.RemoveControl((PanelControl)property.boxedValue);
			}

			heights[controlType] = drawerHeight + resetButtonRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var controlType = (PanelControlType)property.FindPropertyRelative("controlType").enumValueIndex;
			return heights[controlType];
		}
	}



	[CustomPropertyDrawer(typeof(CreatePanelControl))]
	public class CreatePanelControlDrawer : PropertyDrawer
	{
		public static float totalHeight;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			totalHeight = 0;

			Rect fieldRect = new Rect(position.x, position.y + totalHeight, position.width, EditorGUIUtility.singleLineHeight);
			property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, new GUIContent("Add Control to Dialog"), true);
			totalHeight += fieldRect.height + EditorGUIUtility.standardVerticalSpacing;
			if (!property.isExpanded)
			{
				return;
			}

			PropertyDrawerEx.SetProperty(property, "controlType", position, ref totalHeight);
			PanelControlType panelControlType = (PanelControlType)property.FindPropertyRelative("controlType").enumValueIndex;

			++EditorGUI.indentLevel;
			PropertyDrawerEx.SetProperty(property,
				PanelControl.panelControlNames[panelControlType], position, ref totalHeight);
			--EditorGUI.indentLevel;


			var resetButtonRect = new Rect(position.x, position.y + totalHeight, 175, EditorGUIUtility.singleLineHeight);
			GUILayout.BeginHorizontal();
			{
				var addButtonRect = new Rect(position.x + 195, position.y + totalHeight, 175, EditorGUIUtility.singleLineHeight);

				if (GUI.Button(resetButtonRect, "Reset to Defaults"))
				{
					var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
					dpo.ResetToLabelDefaults();
				}

				if (GUI.Button(addButtonRect, "Add Control to Panel"))
				{
					var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
					dpo.AddControl();
				}
			}
			GUILayout.EndHorizontal();

			totalHeight += resetButtonRect.height + EditorGUIUtility.standardVerticalSpacing;

			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return totalHeight;
		}
	}


	[CustomPropertyDrawer(typeof(DropdownEx))]
	public class DropdownExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			var optionsProp = property.FindPropertyRelative("options");
			PropertyDrawerEx.SetProperty(optionsProp, position, ref drawerHeight);

			var isMultiSelectProp = property.FindPropertyRelative("isMultiSelect");
			PropertyDrawerEx.SetProperty(isMultiSelectProp, position, ref drawerHeight);

			var defaultProp = property.FindPropertyRelative("defaultSelection");
			if (isMultiSelectProp.boolValue)
			{
				var selected = defaultProp.intValue;
				var newSelection = 0;
				int bit = 1;
				PropertyDrawerEx.CreateLabel("Default Selections", position, ref drawerHeight);
				++EditorGUI.indentLevel;
				for (int i = 0; i < optionsProp.arraySize; ++i)
				{
					var option = optionsProp.GetArrayElementAtIndex(i);
					var text = option.displayName;
					var toggleRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
					if (EditorGUI.Toggle(toggleRect, text, (selected & bit) == bit))
						newSelection |= bit;
					bit <<= 1;
					drawerHeight += toggleRect.height + EditorGUIUtility.standardVerticalSpacing;
				}
				--EditorGUI.indentLevel;

				defaultProp.intValue = newSelection;
			}
			else
			{
				var selected = defaultProp.intValue;
				var intRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
				defaultProp.intValue = EditorGUI.IntSlider(intRect, selected, 0, optionsProp.arraySize - 1);
				drawerHeight += intRect.height + EditorGUIUtility.standardVerticalSpacing;
			}

			PropertyDrawerEx.CreateLabel("Label Text", position, ref drawerHeight);

			PropertyDrawerEx.SetProperty(property, "fontSize", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "fontColor", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ImageViewDataEx))]
	public class ImageViewDataExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			var maxPanelSizeProp = property.FindPropertyRelative("maxPanelSize");
			PropertyDrawerEx.SetProperty(maxPanelSizeProp, position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "useAllAvailableHeight", position, ref drawerHeight);

			var imageSizeProp = property.FindPropertyRelative("imageSize");
			//var imagePerRowProp = property.FindPropertyRelative("imagesPerRow");
			var imageSize = imageSizeProp.vector2Value;
			var maxPanelSize = maxPanelSizeProp.vector2Value;

			PropertyDrawerEx.SetProperty(imageSizeProp, position, ref drawerHeight);
			//PropertyDrawerEx.SetProperty(imagePerRowProp, position, ref drawerHeight);



			PropertyDrawerEx.SetProperty(property, "defaultSprite", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "showCaptions", position, ref drawerHeight);
			var showCaptionsProp = property.FindPropertyRelative("showCaptions");
			if (showCaptionsProp.boolValue)
			{
				var labelProp = property.FindPropertyRelative("labelEx");
				var maxLabelSize = labelProp.FindPropertyRelative("maxLabelDimensions").vector2Value;
				imageSize.x = Mathf.Max(imageSize.x, maxLabelSize.x);
				imageSize.y += maxLabelSize.y;
				PropertyDrawerEx.SetProperty(labelProp, position, ref drawerHeight);
			}

			//EditorGUILayout.Vector2Field("Cell Size", imageSize);

			//int imagesOnRow = Mathf.FloorToInt(maxPanelSize.x / imageSize.x);
			//var minWidthReq = imageSize.x * imagesOnRow;

			//EditorGUILayout.FloatField("Internal Panel Dimensions", minWidthReq);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ImageEx))]
	public class ImageExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			drawerHeight = 0;
			PropertyDrawerEx.SetProperty(property, "isVisible", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "sprite", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "forceSize", position, ref drawerHeight);
			var forceSizeProp = property.FindPropertyRelative("forceSize");
			if (forceSizeProp.boolValue)
				PropertyDrawerEx.SetProperty(property, "size", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "showCaption", position, ref drawerHeight);
			var showCaptionProp = property.FindPropertyRelative("showCaption");
			if (showCaptionProp.boolValue)
				PropertyDrawerEx.SetProperty(property, "labelEx", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(SliderEx))]
	public class SliderExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			drawerHeight = 0;
			var boolProp = property.FindPropertyRelative("wholeNumbers");
			var minProp = property.FindPropertyRelative("minValue");
			var maxProp = property.FindPropertyRelative("maxValue");
			var valueProp = property.FindPropertyRelative("value");
			var showUnitsProp = property.FindPropertyRelative("showUnits");

			var boolRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(boolProp, true));
			drawerHeight += boolRect.height + EditorGUIUtility.standardVerticalSpacing;
			var minRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(minProp, true));
			drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
			var maxRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(maxProp, true));
			drawerHeight += maxRect.height + EditorGUIUtility.standardVerticalSpacing;
			var valueRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(valueProp, true));
			drawerHeight += valueRect.height + EditorGUIUtility.standardVerticalSpacing;


			bool newIsInt = EditorGUI.Toggle(boolRect, "Whole Numbers", boolProp.boolValue);
			if (newIsInt != boolProp.boolValue)
			{
				boolProp.boolValue = newIsInt;
				if (newIsInt)
				{
					minProp.floatValue = Mathf.RoundToInt(minProp.floatValue);
					maxProp.floatValue = Mathf.RoundToInt(maxProp.floatValue);
					valueProp.floatValue = Mathf.RoundToInt(valueProp.floatValue);
				}
			}

			float newMinValue = EditorGUI.FloatField(minRect, "Min Value", minProp.floatValue);
			if (newMinValue != minProp.floatValue)
			{
				if (newIsInt)
					newMinValue = Mathf.RoundToInt(newMinValue);
				if (newMinValue > maxProp.floatValue)
					newMinValue = maxProp.floatValue;
				minProp.floatValue = newMinValue;
			}

			float newMaxValue = EditorGUI.FloatField(maxRect, "Max Value", maxProp.floatValue);
			if (newMaxValue != maxProp.floatValue)
			{
				if (newIsInt)
					newMaxValue = Mathf.RoundToInt(newMaxValue);
				if (newMaxValue < minProp.floatValue)
					newMaxValue = minProp.floatValue;
				maxProp.floatValue = newMaxValue;
			}

			float newValue = EditorGUI.Slider(valueRect, "Value", valueProp.floatValue, minProp.floatValue, maxProp.floatValue);

			if (newIsInt)
				newValue = Mathf.RoundToInt(newValue);
			valueProp.floatValue = newValue;


			var showHandleProp = property.FindPropertyRelative("showHandle");
			var showHandleRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(showHandleProp, true));
			drawerHeight += showHandleRect.height + EditorGUIUtility.standardVerticalSpacing;
			showHandleProp.boolValue = EditorGUI.Toggle(showHandleRect, "Show Handle", showHandleProp.boolValue);
			//if (showHandleProp.boolValue)
			{
				PropertyDrawerEx.SetProperty(property, "handleOffset", position, ref drawerHeight);
			}


			PropertyDrawerEx.SetProperty(showUnitsProp, position, ref drawerHeight);
			if (showUnitsProp.boolValue)
			{
				var unitSpanProp = property.FindPropertyRelative("unitSpan");
				var unitSpanRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(unitSpanProp, true));
				drawerHeight += unitSpanRect.height + EditorGUIUtility.standardVerticalSpacing;

				float range = newMaxValue - newMinValue;
				float newSpan;
				if (newIsInt)
					newSpan = EditorGUI.IntSlider(unitSpanRect, "Unit Span", (int)unitSpanProp.floatValue, 0, (int)range);
				else
					newSpan = EditorGUI.Slider(unitSpanRect, "Unit Span", unitSpanProp.floatValue, 0, range);
				if (newSpan != unitSpanProp.floatValue)
				{

					unitSpanProp.floatValue = newSpan;

				}
				PropertyDrawerEx.SetProperty(property, "unitVerticalOffset", position, ref drawerHeight);
				// draw a short label
				var labelProp = property.FindPropertyRelative("labelEx");
				++EditorGUI.indentLevel;
				PropertyDrawerEx.SetProperty(labelProp, "fontSize", position, ref drawerHeight);
				PropertyDrawerEx.SetProperty(labelProp, "fontColor", position, ref drawerHeight);
				PropertyDrawerEx.SetProperty(labelProp, "fontAsset", position, ref drawerHeight);
				--EditorGUI.indentLevel;
			}

			EditorGUI.EndProperty();
		}

		private static List<int> GetValidValues(int minValue, int maxValue)
		{
			var results = new List<int>();
			int range = (maxValue - minValue) + 1;
			results.Add(2);
			for (int count = 3; count <= range / 2; ++count)
			{
				if (range % count == 0)
				{
					results.Add(count);
				}
			}

			results.Add(range);
			return results;
		}
	}

	[CustomPropertyDrawer(typeof(ButtonEx))]
	public class ButtonExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			drawerHeight = 0;
			PropertyDrawerEx.SetProperty(property, "labelEx", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "action", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(CheckBoxEx))]
	public class CheckBoxExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			drawerHeight = 0;
			PropertyDrawerEx.SetProperty(property, "isOn", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "labelEx", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(InputFieldEx))]
	public class InputFieldExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			drawerHeight = 0;
			var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(labelRect, "Placeholder Text");
			drawerHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			++EditorGUI.indentLevel;
			{
				PropertyDrawerEx.SetProperty(property, "placeholderText", position, ref drawerHeight);
				PropertyDrawerEx.SetProperty(property, "placeHolderFontColor", position, ref drawerHeight);
			}
			--EditorGUI.indentLevel;

			labelRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			EditorGUI.LabelField(labelRect, "Text");
			drawerHeight += labelRect.height + EditorGUIUtility.standardVerticalSpacing;

			++EditorGUI.indentLevel;
			{
				PropertyDrawerEx.SetProperty(property, "defaultText", position, ref drawerHeight);
				PropertyDrawerEx.SetProperty(property, "fontColor", position, ref drawerHeight);
			}
			--EditorGUI.indentLevel;

			PropertyDrawerEx.SetProperty(property, "fontSize", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "fontAsset", position, ref drawerHeight);
			PropertyDrawerEx.SetProperty(property, "fieldDimensions", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(LabelEx))]
	public class LabelExDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			++EditorGUI.indentLevel;

			var maxDimProp = property.FindPropertyRelative("maxLabelDimensions");
			var minDimProp = property.FindPropertyRelative("minLabelDimensions");

			if (minDimProp.vector2Value.x <= 0)
				minDimProp.vector2Value = new Vector2(1, minDimProp.vector2Value.y);
			if (minDimProp.vector2Value.y <= 0)
				minDimProp.vector2Value = new Vector2(minDimProp.vector2Value.x, 1);

			if (maxDimProp.vector2Value.x < minDimProp.vector2Value.x)
				maxDimProp.vector2Value = new Vector2(minDimProp.vector2Value.x, maxDimProp.vector2Value.y);
			if (maxDimProp.vector2Value.y < minDimProp.vector2Value.y)
				maxDimProp.vector2Value = new Vector2(maxDimProp.vector2Value.x, minDimProp.vector2Value.y);

			var textRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += textRect.height + EditorGUIUtility.standardVerticalSpacing;
			var minRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
			var maxRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += maxRect.height + EditorGUIUtility.standardVerticalSpacing;
			var fontSizeRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += fontSizeRect.height + EditorGUIUtility.standardVerticalSpacing;
			var fontColorRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += fontColorRect.height + EditorGUIUtility.standardVerticalSpacing;
			var fontRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += fontRect.height + EditorGUIUtility.standardVerticalSpacing;

			// Draw fields - pass GUIContent.none so they are drawn without labels
			EditorGUI.PropertyField(textRect, property.FindPropertyRelative("text"));
			EditorGUI.PropertyField(minRect, minDimProp);
			EditorGUI.PropertyField(maxRect, maxDimProp);
			EditorGUI.PropertyField(fontSizeRect, property.FindPropertyRelative("fontSize"));
			EditorGUI.PropertyField(fontColorRect, property.FindPropertyRelative("fontColor"));
			EditorGUI.PropertyField(fontRect, property.FindPropertyRelative("fontAsset"));

			--EditorGUI.indentLevel;


			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ButtonPanelEx))]
	public class ButtonDataDrawer : UIExDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;
			var enumRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += enumRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(enumRect, property.FindPropertyRelative("buttons"));

			EditorGUI.EndProperty();
		}
	}


	public class UIExDrawer : PropertyDrawer
	{
		public float drawerHeight;

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return drawerHeight;
		}
	}
}
