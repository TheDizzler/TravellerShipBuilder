using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PropertyDrawerEx
{
	public static void SetProperty(SerializedProperty subProperty, Rect position, ref float currentDrawerHeight)
	{
		var propRect = new Rect(position.x, position.y + currentDrawerHeight, position.width, EditorGUI.GetPropertyHeight(subProperty, true));
		EditorGUI.PropertyField(propRect, subProperty);
		currentDrawerHeight += propRect.height + EditorGUIUtility.standardVerticalSpacing;
	}

	public static void SetProperty(SerializedProperty mainProperty, string propertyName, Rect position, ref float currentDrawerHeight)
	{
		var prop = mainProperty.FindPropertyRelative(propertyName);
		var propRect = new Rect(position.x, position.y + currentDrawerHeight, position.width, EditorGUI.GetPropertyHeight(prop, true));
		EditorGUI.PropertyField(propRect, prop);
		currentDrawerHeight += propRect.height + EditorGUIUtility.standardVerticalSpacing;
	}

	public static void CreateLabel(string labelText, Rect position, ref float drawerHeight)
	{
		var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, labelText);
		drawerHeight += labelRect.height + EditorGUIUtility.standardVerticalSpacing;
	}
}


[CustomPropertyDrawer(typeof(PanelItem))]
public class PanelItemDrawer : PropertyDrawer
{
	Dictionary<PanelItemType, float> heights;

	public PanelItemDrawer() : base()
	{
		heights = new();
		foreach (PanelItemType itemType in Enum.GetValues(typeof(PanelItemType)))
			heights.Add(itemType, 0);
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		float drawerHeight = 0;

		var itemType = (PanelItemType)property.FindPropertyRelative("itemType").enumValueIndex;
		PropertyDrawerEx.CreateLabel(itemType.ToString(), position, ref drawerHeight);
		PropertyDrawerEx.SetProperty(property, PanelItem.panelItemNames[itemType], position, ref drawerHeight);


		var resetButtonRect = new Rect(position.x, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
		var addButtonRect = new Rect(position.x + 195, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);

		if (GUI.Button(resetButtonRect, "Reset to Default"))
		{
			var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
			var pi = (PanelItem)property.boxedValue;
			dpo.ResetToDefaults(pi);
		}

		if (GUI.Button(addButtonRect, "Remove"))
		{
			var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
			dpo.RemoveItem((PanelItem)property.boxedValue);
		}

		heights[itemType] = drawerHeight + resetButtonRect.height + EditorGUIUtility.standardVerticalSpacing;
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		var itemType = (PanelItemType)property.FindPropertyRelative("itemType").enumValueIndex;
		return heights[itemType];
	}
}



[CustomPropertyDrawer(typeof(CreatePanelItem))]
public class CreatePanelItemDrawer : PropertyDrawer
{
	public static float totalHeight;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		totalHeight = 0;

		Rect fieldRect = new Rect(position.x, position.y + totalHeight, position.width, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, new GUIContent("Add Item to Dialog"), true);
		totalHeight += fieldRect.height + EditorGUIUtility.standardVerticalSpacing;
		if (!property.isExpanded)
		{
			return;
		}

		PropertyDrawerEx.SetProperty(property, "itemType", position, ref totalHeight);
		PanelItemType panelItemType = (PanelItemType)property.FindPropertyRelative("itemType").enumValueIndex;

		++EditorGUI.indentLevel;
		PropertyDrawerEx.SetProperty(property, CreatePanelItem.panelItemNames[panelItemType], position, ref totalHeight);
		--EditorGUI.indentLevel;


		GUILayout.BeginHorizontal();

		var resetButtonRect = new Rect(position.x, position.y + totalHeight, 175, EditorGUIUtility.singleLineHeight);
		var addButtonRect = new Rect(position.x + 195, position.y + totalHeight, 175, EditorGUIUtility.singleLineHeight);

		if (GUI.Button(resetButtonRect, "Reset to Defaults"))
		{
			var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
			dpo.ResetToLabelDefaults();
		}

		if (GUI.Button(addButtonRect, "Add Item to Panel"))
		{
			var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
			dpo.AddItem();
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

		PropertyDrawerEx.SetProperty(property, "handleOffset", position, ref drawerHeight);

		PropertyDrawerEx.SetProperty(showUnitsProp, position, ref drawerHeight);
		if (showUnitsProp.boolValue)
		{
			var unitCountProp = property.FindPropertyRelative("unitCount");
			var unitCountRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(unitCountProp, true));
			drawerHeight += unitCountRect.height + EditorGUIUtility.standardVerticalSpacing;

			unitCountProp.intValue = EditorGUI.IntSlider(unitCountRect, "Unit Count", unitCountProp.intValue, 2, 12);

			PropertyDrawerEx.SetProperty(property, "unitVerticalOffset", position, ref drawerHeight);
			// draw a short label
			PropertyDrawerEx.SetProperty(property, "labelEx", position, ref drawerHeight);
		}


		// logic

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


		EditorGUI.EndProperty();
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
		EditorGUI.PropertyField(minRect, property.FindPropertyRelative("minLabelDimensions"));
		EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("maxLabelDimensions"));
		EditorGUI.PropertyField(fontSizeRect, property.FindPropertyRelative("fontSize"));
		EditorGUI.PropertyField(fontColorRect, property.FindPropertyRelative("fontColor"));
		EditorGUI.PropertyField(fontRect, property.FindPropertyRelative("fontAsset"));

		--EditorGUI.indentLevel;

		EditorGUI.EndProperty();
	}
}



[CustomPropertyDrawer(typeof(ButtonPanelDataEx))]
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

