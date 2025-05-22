using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PropertyDrawerEx
{
	public static void SetProperty(SerializedProperty property, string propertyName, Rect position, ref float currentDrawerHeight)
	{
		var prop = property.FindPropertyRelative(propertyName);
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
		{
			heights.Add(itemType, 0);
		}
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
public class SliderExDrawer : PropertyDrawer
{
	public static float drawerHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		drawerHeight = 0;
		PropertyDrawerEx.SetProperty(property, "wholeNumbers", position, ref drawerHeight);
		PropertyDrawerEx.SetProperty(property, "minValue", position, ref drawerHeight);
		PropertyDrawerEx.SetProperty(property, "maxValue", position, ref drawerHeight);
		PropertyDrawerEx.SetProperty(property, "value", position, ref drawerHeight);

		EditorGUI.EndProperty();
	}


	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return drawerHeight;
	}
}

[CustomPropertyDrawer(typeof(CheckBoxEx))]
public class CheckBoxExDrawer : PropertyDrawer
{
	public static float drawerHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		drawerHeight = 0;
		PropertyDrawerEx.SetProperty(property, "isOn", position, ref drawerHeight);
		PropertyDrawerEx.SetProperty(property, "labelEx", position, ref drawerHeight);

		EditorGUI.EndProperty();
	}


	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return drawerHeight;
	}
}

[CustomPropertyDrawer(typeof(InputFieldEx))]
public class InputFieldExDrawer : PropertyDrawer
{
	public static float inputFieldDrawerHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		inputFieldDrawerHeight = 0;
		var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, "Placeholder Text");
		inputFieldDrawerHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		++EditorGUI.indentLevel;
		{
			PropertyDrawerEx.SetProperty(property, "placeholderText", position, ref inputFieldDrawerHeight);
			PropertyDrawerEx.SetProperty(property, "placeHolderFontColor", position, ref inputFieldDrawerHeight);
		}
		--EditorGUI.indentLevel;

		labelRect = new Rect(position.x, position.y + inputFieldDrawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, "Text");
		inputFieldDrawerHeight += labelRect.height + EditorGUIUtility.standardVerticalSpacing;

		++EditorGUI.indentLevel;
		{
			PropertyDrawerEx.SetProperty(property, "defaultText", position, ref inputFieldDrawerHeight);
			PropertyDrawerEx.SetProperty(property, "fontColor", position, ref inputFieldDrawerHeight);
		}
		--EditorGUI.indentLevel;

		PropertyDrawerEx.SetProperty(property, "fontSize", position, ref inputFieldDrawerHeight);
		PropertyDrawerEx.SetProperty(property, "fontAsset", position, ref inputFieldDrawerHeight);
		PropertyDrawerEx.SetProperty(property, "fieldDimensions", position, ref inputFieldDrawerHeight);

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return inputFieldDrawerHeight;
	}
}


[CustomPropertyDrawer(typeof(LabelEx))]
public class LabelExDrawer : PropertyDrawer
{
	public static float drawerHeight;

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

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return drawerHeight;
	}
}



[CustomPropertyDrawer(typeof(ButtonDataEx))]
public class ButtonDataDrawer : PropertyDrawer
{
	public static float drawerHeight;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		drawerHeight = 0;
		var enumRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		drawerHeight += enumRect.height + EditorGUIUtility.standardVerticalSpacing;
		EditorGUI.PropertyField(enumRect, property.FindPropertyRelative("buttons"));

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return drawerHeight;
	}
}
