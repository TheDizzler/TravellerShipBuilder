using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.EditorZ;
using TMPro;

using UnityEditor;

using UnityEngine;
using static AtomosZ.UI.MagicWindow;


namespace AtomosZ.UI.EditorZ
{
	//[CustomPropertyDrawer(typeof(UIControl), true)]
	//public class ControlPanelDrawer : PropertyDrawer
	//{
	//	Dictionary<UIControlType, float> heights;
	//	public ControlPanelDrawer() : base()
	//	{
	//		heights = new();
	//		foreach (UIControlType controlType in Enum.GetValues(typeof(UIControlType)))
	//			heights.Add(controlType, 0);
	//	}

	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		float drawerHeight = 0;

	//		var controlType = (UIControlType)property.FindPropertyRelative("controlType").enumValueIndex;
	//		this.CreateLabel(controlType.ToString(), position, ref drawerHeight);

	//		this.SetProperty(property, UIControl.panelControlNames[controlType], position, ref drawerHeight);

	//		var removeButtonRect = new Rect(position.x + 100, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
	//		if (GUI.Button(removeButtonRect, "Remove"))
	//		{
	//			var dpo = (DynaPanelOp)property.serializedObject.targetObject;
	//			dpo.RemoveControl((UIControl)property.boxedValue);
	//		}

	//		heights[controlType] = drawerHeight + removeButtonRect.height + EditorGUIUtility.standardVerticalSpacing;
	//		EditorGUI.EndProperty();
	//	}

	//	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	//	{
	//		var controlType = (UIControlType)property.FindPropertyRelative("controlType").enumValueIndex;
	//		return heights[controlType];
	//	}
	//}

	//[Obsolete("Been replaced with ControlPanelDrawer")]
	//[CustomPropertyDrawer(typeof(CreatePanelControl))]
	//public class CreatePanelControlDrawer : PropertyDrawer
	//{
	//	public static float drawerHeight;
	//	public static bool isValuesExpanded = false;
	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		drawerHeight = 0;

	//		this.CreateLabel("Add UI Control to Panel", position, ref drawerHeight);
	//		//property.isExpanded = this.CreateFoldout("Add Control to Dialog", property.isExpanded, position, ref drawerHeight);

	//		//if (!property.isExpanded)
	//		//{
	//		//	EditorGUI.EndProperty();
	//		//	return;
	//		//}

	//		var controlTypeProp = property.FindPropertyRelative("controlType");
	//		this.SetProperty(controlTypeProp, position, ref drawerHeight);
	//		UIControlType panelControlType = (UIControlType)controlTypeProp.enumValueIndex;

	//		/// the following was needed before I could read and edit already present controls. Now it feels vestigial.
	//		/// Should we remove the whole concept of the CreatePanelControl being a type of PanelControl_dep?

	//		isValuesExpanded = this.CreateFoldout("Edit values", isValuesExpanded, position, ref drawerHeight);
	//		if (isValuesExpanded)
	//		{
	//			++EditorGUI.indentLevel;
	//			this.SetProperty(property,
	//				PanelControl_dep.panelControlNames[panelControlType], position, ref drawerHeight);

	//			var resetButtonRect = new Rect(position.x + 195, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
	//			if (GUI.Button(resetButtonRect, "Reset to Defaults"))
	//			{
	//				var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
	//				dpo.ResetToLabelDefaults();
	//			}

	//			drawerHeight += resetButtonRect.height + EditorGUIUtility.standardVerticalSpacing;
	//			--EditorGUI.indentLevel;
	//		}


	//		var addButtonRect = new Rect(position.x, position.y + drawerHeight, 200, EditorGUIUtility.singleLineHeight);

	//		if (GUI.Button(addButtonRect, $"Add {panelControlType} to Panel"))
	//		{
	//			var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
	//			dpo.AddControl();
	//		}

	//		drawerHeight += addButtonRect.height + EditorGUIUtility.standardVerticalSpacing;


	//		EditorGUI.EndProperty();
	//	}

	//	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	//	{
	//		return drawerHeight;
	//	}
	//}






	//[CustomPropertyDrawer(typeof(TabLookupDictionary))]
	//public class TabLookupDictionaryDrawer : DrawerEx
	//{
	//	private bool isExpanded = true;

	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		drawerHeight = 0;

	//		if (isExpanded = this.CreateFoldout("Panels", isExpanded, position, ref drawerHeight))
	//		{
	//			++EditorGUI.indentLevel;
	//			var tabPanelDict = (TabLookupDictionary)property.boxedValue;
	//			foreach (var tabPanel in tabPanelDict)
	//			{
	//				this.CreateLabel(tabPanel.Key.name, position, ref drawerHeight);

	//			}
	//			--EditorGUI.indentLevel;
	//		}

	//		EditorGUI.EndProperty();
	//	}
	//}


	//[CustomPropertyDrawer(typeof(DropdownEx))]
	//public class DropdownExDrawer : DrawerEx
	//{
	//	private bool isExpanded;

	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		drawerHeight = 0;

	//		this.SetProperty(property, "fillParentHorizontal", position, ref drawerHeight);
	//		this.SetProperty(property, "minDimensions", position, ref drawerHeight);


	//		isExpanded = this.CreateFoldout("Overridable values", isExpanded, position, ref drawerHeight);
	//		if (isExpanded)
	//		{
	//			++EditorGUI.indentLevel;
	//			var textSOProp = property.FindPropertyRelative("scriptableObj");
	//			this.SetProperty(textSOProp, position, ref drawerHeight);

	//			var customFontSizeProp = property.FindPropertyRelative("fontSize");
	//			var customFontColorProp = property.FindPropertyRelative("fontColor");
	//			var customFontAssetProp = property.FindPropertyRelative("fontAsset");
	//			var textSo = (UIExpandingLabelScriptableObject)textSOProp.boxedValue;
	//			if (textSo != null)
	//			{
	//				var isCustomFontSizeProp = property.FindPropertyRelative("useCustomFontSize");
	//				this.SetOverridableProperty(isCustomFontSizeProp, customFontSizeProp, textSo.fontSize, position, ref drawerHeight);

	//				var isCustomFontColorProp = property.FindPropertyRelative("useCustomFontColor");
	//				this.SetOverridableProperty(isCustomFontColorProp, customFontColorProp, textSo.fontColor, position, ref drawerHeight);

	//				var isCustomFontAssetProp = property.FindPropertyRelative("useCustomFontAsset");
	//				this.SetOverridableProperty(isCustomFontAssetProp, customFontAssetProp, textSo.fontColor, position, ref drawerHeight);
	//			}
	//			--EditorGUI.indentLevel;
	//		}



	//		EditorGUI.EndProperty();
	//	}
	//}

	//[CustomPropertyDrawer(typeof(ImageViewDataEx))]
	//public class ImageViewDataExDrawer : DrawerEx
	//{
	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		drawerHeight = 0;

	//		this.SetProperty(property, "referenceName", position, ref drawerHeight);

	//		var imageViewProp = property.FindPropertyRelative("scriptableObj");
	//		this.SetProperty(imageViewProp, position, ref drawerHeight);

	//		var imageViewSO = (UIImageViewPanelScriptableObject)imageViewProp.boxedValue;

	//		var maxPanelSize = imageViewSO.maxPanelSize;
	//		var imageSize = imageViewSO.imageSize;

	//		//var maxPanelSizeProp = property.FindPropertyRelative("maxPanelSize");
	//		//this.SetProperty(maxPanelSizeProp, position, ref drawerHeight);
	//		this.SetProperty(property, "useAllAvailableHeight", position, ref drawerHeight);

	//		imageViewSO.maxPanelSize = this.CreateVector2Field(maxPanelSize, "Max Panel Size", position, ref drawerHeight);
	//		imageViewSO.imageSize = this.CreateVector2Field(imageSize, "Image Size", position, ref drawerHeight);


	//		//var imageSizeProp = property.FindPropertyRelative("imageSize");
	//		//var imagePerRowProp = property.FindPropertyRelative("imagesPerRow");
	//		//var imageSize = imageSizeProp.vector2Value;
	//		//var maxPanelSize = maxPanelSizeProp.vector2Value;

	//		//this.SetProperty(imageSizeProp, position, ref drawerHeight);
	//		//this.SetProperty(imagePerRowProp, position, ref drawerHeight);




	//		//this.SetProperty(property, "defaultSprite", position, ref drawerHeight);



	//		this.SetProperty(property, "showCaptions", position, ref drawerHeight);
	//		var showCaptionsProp = property.FindPropertyRelative("showCaptions");
	//		if (showCaptionsProp.boolValue)
	//		{
	//			var labelProp = property.FindPropertyRelative("labelEx");
	//			var maxLabelSize = labelProp.FindPropertyRelative("maxLabelDimensions").vector2Value;
	//			imageSize.x = Mathf.Max(imageSize.x, maxLabelSize.x);
	//			imageSize.y += maxLabelSize.y;
	//			this.SetProperty(labelProp, position, ref drawerHeight);
	//		}

	//		//EditorGUILayout.Vector2Field("Cell Size", imageSize);

	//		//int imagesOnRow = Mathf.FloorToInt(maxPanelSize.x / imageSize.x);
	//		//var minWidthReq = imageSize.x * imagesOnRow;

	//		//EditorGUILayout.FloatField("Internal Panel Dimensions", minWidthReq);

	//		EditorGUI.EndProperty();
	//	}
	//}

	//[CustomPropertyDrawer(typeof(ImageEx))]
	//public class ImageExDrawer : DrawerEx
	//{
	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		drawerHeight = 0;

	//		this.SetProperty(property, "isVisible", position, ref drawerHeight);
	//		this.SetProperty(property, "sprite", position, ref drawerHeight);
	//		this.SetProperty(property, "forceSize", position, ref drawerHeight);
	//		var forceSizeProp = property.FindPropertyRelative("forceSize");
	//		if (forceSizeProp.boolValue)
	//			this.SetProperty(property, "size", position, ref drawerHeight);
	//		this.SetProperty(property, "showCaption", position, ref drawerHeight);
	//		var showCaptionProp = property.FindPropertyRelative("showCaption");

	//		EditorGUI.EndProperty();
	//	}
	//}


	//[CustomPropertyDrawer(typeof(InputFieldEx))]
	//public class InputFieldExDrawer : DrawerEx
	//{
	//	private bool isExpanded;

	//	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	//	{
	//		EditorGUI.BeginProperty(position, label, property);
	//		drawerHeight = 0;

	//		this.SetProperty(property, "placeholderText", position, ref drawerHeight);
	//		this.SetProperty(property, "defaultText", position, ref drawerHeight);

	//		this.SetProperty(property, "fieldDimensions", position, ref drawerHeight);

	//		var inputSOProp = property.FindPropertyRelative("scriptableObj");
	//		if (inputSOProp == null || inputSOProp.boxedValue == null)
	//		{
	//			this.CreateLabel("Text Parameters", position, ref drawerHeight);
	//			++EditorGUI.indentLevel;

	//			this.SetProperty(inputSOProp, position, ref drawerHeight);

	//			var customFontSizeProp = property.FindPropertyRelative("fontSize");
	//			this.SetProperty(customFontSizeProp, position, ref drawerHeight);
	//			var customFontColorProp = property.FindPropertyRelative("fontColor");
	//			this.SetProperty(customFontColorProp, position, ref drawerHeight);
	//			var customPlaceholderFontColorProp = property.FindPropertyRelative("placeholderFontColor");
	//			this.SetProperty(customPlaceholderFontColorProp, position, ref drawerHeight);

	//			--EditorGUI.indentLevel;
	//		}
	//		else
	//		{
	//			isExpanded = this.CreateFoldout("Scriptable Object values", isExpanded, position, ref drawerHeight);
	//			if (isExpanded)
	//			{
	//				++EditorGUI.indentLevel;

	//				this.SetProperty(inputSOProp, position, ref drawerHeight);

	//				var customFontSizeProp = property.FindPropertyRelative("fontSize");
	//				var customFontColorProp = property.FindPropertyRelative("fontColor");
	//				var customPlaceholderFontColorProp = property.FindPropertyRelative("placeholderFontColor");
	//				var customFontAssetProp = property.FindPropertyRelative("fontAsset");
	//				var textSo = (UIExpandingInputFieldScriptableObject)inputSOProp.boxedValue;

	//				var isCustomFontSizeProp = property.FindPropertyRelative("useCustomFontSize");
	//				this.SetOverridableProperty(isCustomFontSizeProp, customFontSizeProp, textSo.fontSize, position, ref drawerHeight);

	//				var isCustomFontColorProp = property.FindPropertyRelative("useCustomFontColor");
	//				this.SetOverridableProperty(isCustomFontColorProp, customFontColorProp, textSo.fontColor, position, ref drawerHeight);

	//				var isCustomPlaceholderFontColorProp = property.FindPropertyRelative("useCustomPlaceholderFontColor");
	//				this.SetOverridableProperty(isCustomPlaceholderFontColorProp, customPlaceholderFontColorProp, textSo.placeholderFontColor, position, ref drawerHeight);

	//				var isCustomFontAssetProp = property.FindPropertyRelative("useCustomFontAsset");
	//				this.SetOverridableProperty(isCustomFontAssetProp, customFontAssetProp, textSo.fontColor, position, ref drawerHeight);

	//				--EditorGUI.indentLevel;
	//			}


	//			EditorGUI.EndProperty();
	//		}
	//	}
	//}


}
