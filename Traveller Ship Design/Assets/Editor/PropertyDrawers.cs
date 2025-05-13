using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;



[CustomPropertyDrawer(typeof(CreatePanelItem))]
public class CreatePanelItemDrawer : PropertyDrawer
{
	public static float totalHeight;
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);
		//EditorGUI.indentLevel = 0;
		float drawerHeight = 0;

		Rect fieldRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout(fieldRect, property.isExpanded, new GUIContent("Add Item to Dialog"), true);
		drawerHeight += fieldRect.height + EditorGUIUtility.standardVerticalSpacing;
		if (!property.isExpanded)
		{
			totalHeight = drawerHeight;
			return;
		}

		var itemTypeRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		SerializedProperty itemTypeProperty = property.FindPropertyRelative("dialogItemType");
		EditorGUI.PropertyField(itemTypeRect, itemTypeProperty);
		drawerHeight += itemTypeRect.height + EditorGUIUtility.standardVerticalSpacing;

		PanelItemType panelItemType = (PanelItemType)itemTypeProperty.enumValueIndex;
		++EditorGUI.indentLevel;
		switch (panelItemType)
		{
			case PanelItemType.Text:
			{
				var labelEx = property.FindPropertyRelative("labelEx");

				var textRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(labelEx, true));
				drawerHeight += textRect.height + EditorGUIUtility.standardVerticalSpacing;

				EditorGUI.PropertyField(textRect, labelEx);
			}
			break;

			case PanelItemType.InputField:
			{
				var inputField = property.FindPropertyRelative("inputFieldEx");
				var placeholderRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUI.GetPropertyHeight(inputField, true));
				drawerHeight += placeholderRect.height + EditorGUIUtility.standardVerticalSpacing;

				EditorGUI.PropertyField(placeholderRect, inputField);
			}
			break;

			case PanelItemType.Buttons:
			{
				var buttonsRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
				drawerHeight += buttonsRect.height + EditorGUIUtility.standardVerticalSpacing;
				EditorGUI.PropertyField(buttonsRect, property.FindPropertyRelative("buttons"));
			}
			break;
		}
		--EditorGUI.indentLevel;

		var buttonRect = new Rect((position.x + 175) / 2, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
		drawerHeight += buttonRect.height + EditorGUIUtility.standardVerticalSpacing;
		if (GUI.Button(buttonRect, "Add Item to Panel"))
		{
			var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
			dpo.AddItem();
		}

		totalHeight = drawerHeight;
		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		//var value = (PanelItemType)property.FindPropertyRelative("dialogItemType").enumValueIndex;
		return totalHeight;
	}
}




[CustomPropertyDrawer(typeof(InputFieldEx))]
public class InputFieldExDrawer : PropertyDrawer
{
	public static float inputFieldDrawerHeight;

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		float drawerHeight = 0;
		var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, "Placeholder Text");
		drawerHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		++EditorGUI.indentLevel;
		{
			var placeholder = property.FindPropertyRelative("placeholderText");
			var placeholderRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += placeholderRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(placeholderRect, placeholder);

			var placeholderColor = property.FindPropertyRelative("placeHolderFontColor");
			var placeholderColorRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += placeholderColorRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(placeholderColorRect, placeholderColor);
		}
		--EditorGUI.indentLevel;

		labelRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		EditorGUI.LabelField(labelRect, "Default Text");
		drawerHeight += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

		++EditorGUI.indentLevel;
		{
			var text = property.FindPropertyRelative("defaultText");
			var textRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += textRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(textRect, text);

			var placeholderColor = property.FindPropertyRelative("fontColor");
			var placeholderColorRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += placeholderColorRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(placeholderColorRect, placeholderColor);
		}
		--EditorGUI.indentLevel;


		var fontSize = property.FindPropertyRelative("fontSize");
		var fontSizeRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		drawerHeight += fontSizeRect.height + EditorGUIUtility.standardVerticalSpacing;
		EditorGUI.PropertyField(fontSizeRect, fontSize);

		var fieldDims = property.FindPropertyRelative("fieldDimensions");
		var fieldDimensionsRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
		drawerHeight += fieldDimensionsRect.height + EditorGUIUtility.standardVerticalSpacing;
		EditorGUI.PropertyField(fieldDimensionsRect, fieldDims);

		inputFieldDrawerHeight = drawerHeight;
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

	// TODO(Tristan): Investigate what CreatePropertyGUI is for.
	//public override VisualElement CreatePropertyGUI(SerializedProperty property)
	//{
	//}

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

		// Draw fields - pass GUIContent.none so they are drawn without labels
		EditorGUI.PropertyField(textRect, property.FindPropertyRelative("text"));
		EditorGUI.PropertyField(minRect, property.FindPropertyRelative("minLabelDimensions"));
		EditorGUI.PropertyField(maxRect, property.FindPropertyRelative("maxLabelDimensions"));
		EditorGUI.PropertyField(fontSizeRect, property.FindPropertyRelative("fontSize"));
		EditorGUI.PropertyField(fontColorRect, property.FindPropertyRelative("fontColor"));

		--EditorGUI.indentLevel;

		EditorGUI.EndProperty();
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		return drawerHeight;
	}
}
