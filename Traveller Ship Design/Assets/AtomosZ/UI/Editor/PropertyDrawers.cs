using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.EditorZ;
using TMPro;

using UnityEditor;

using UnityEngine;


namespace AtomosZ.UI.EditorZ
{
	[CustomPropertyDrawer(typeof(UIControl), true)]
	public class ControlPanelDrawer : PropertyDrawer
	{
		Dictionary<UIControlType, float> heights;
		public ControlPanelDrawer() : base()
		{
			heights = new();
			foreach (UIControlType controlType in Enum.GetValues(typeof(UIControlType)))
				heights.Add(controlType, 0);
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			float drawerHeight = 0;

			var controlType = (UIControlType)property.FindPropertyRelative("controlType").enumValueIndex;
			this.CreateLabel(controlType.ToString(), position, ref drawerHeight);

			this.SetProperty(property, UIControl.panelControlNames[controlType], position, ref drawerHeight);

			var removeButtonRect = new Rect(position.x + 100, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
			if (GUI.Button(removeButtonRect, "Remove"))
			{
				var dpo = (DynaPanelOp)property.serializedObject.targetObject;
				dpo.RemoveControl((UIControl)property.boxedValue);
			}

			heights[controlType] = drawerHeight + removeButtonRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			var controlType = (UIControlType)property.FindPropertyRelative("controlType").enumValueIndex;
			return heights[controlType];
		}
	}

	[Obsolete("Been replaced with ControlPanelDrawer")]
	[CustomPropertyDrawer(typeof(CreatePanelControl))]
	public class CreatePanelControlDrawer : PropertyDrawer
	{
		public static float drawerHeight;
		public static bool isValuesExpanded = false;
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.CreateLabel("Add UI Control to Panel", position, ref drawerHeight);
			//property.isExpanded = this.CreateFoldout("Add Control to Dialog", property.isExpanded, position, ref drawerHeight);

			//if (!property.isExpanded)
			//{
			//	EditorGUI.EndProperty();
			//	return;
			//}

			var controlTypeProp = property.FindPropertyRelative("controlType");
			this.SetProperty(controlTypeProp, position, ref drawerHeight);
			UIControlType panelControlType = (UIControlType)controlTypeProp.enumValueIndex;

			/// the following was needed before I could read and edit already present controls. Now it feels vestigial.
			/// Should we remove the whole concept of the CreatePanelControl being a type of PanelControl_dep?

			isValuesExpanded = this.CreateFoldout("Edit values", isValuesExpanded, position, ref drawerHeight);
			if (isValuesExpanded)
			{
				++EditorGUI.indentLevel;
				this.SetProperty(property,
					PanelControl_dep.panelControlNames[panelControlType], position, ref drawerHeight);

				var resetButtonRect = new Rect(position.x + 195, position.y + drawerHeight, 175, EditorGUIUtility.singleLineHeight);
				if (GUI.Button(resetButtonRect, "Reset to Defaults"))
				{
					var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
					dpo.ResetToLabelDefaults();
				}

				drawerHeight += resetButtonRect.height + EditorGUIUtility.standardVerticalSpacing;
				--EditorGUI.indentLevel;
			}


			var addButtonRect = new Rect(position.x, position.y + drawerHeight, 200, EditorGUIUtility.singleLineHeight);

			if (GUI.Button(addButtonRect, $"Add {panelControlType} to Panel"))
			{
				var dpo = (DynamicPanelOperator)property.serializedObject.targetObject;
				dpo.AddControl();
			}

			drawerHeight += addButtonRect.height + EditorGUIUtility.standardVerticalSpacing;


			EditorGUI.EndProperty();
		}

		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			return drawerHeight;
		}
	}






	[CustomPropertyDrawer(typeof(TabLookupDictionary))]
	public class TabLookupDictionaryDrawer : DrawerEx
	{
		private bool isExpanded = true;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			if (isExpanded = this.CreateFoldout("Panels", isExpanded, position, ref drawerHeight))
			{
				++EditorGUI.indentLevel;
				var tabPanelDict = (TabLookupDictionary)property.boxedValue;
				foreach (var tabPanel in tabPanelDict)
				{
					this.CreateLabel(tabPanel.Key.name, position, ref drawerHeight);

				}
				--EditorGUI.indentLevel;
			}

			EditorGUI.EndProperty();
		}
	}


	[CustomPropertyDrawer(typeof(DropdownEx))]
	public class DropdownExDrawer : DrawerEx
	{
		private bool isExpanded;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);
			this.SetProperty(property, "fillParentHorizontal", position, ref drawerHeight);
			this.SetProperty(property, "minDimensions", position, ref drawerHeight);

			var setOptionsProp = property.FindPropertyRelative("SetOptions");
			this.SetProperty(setOptionsProp, position, ref drawerHeight);

			var optionsProp = property.FindPropertyRelative("options");
			this.SetProperty(optionsProp, position, ref drawerHeight);



			var isMultiSelectProp = property.FindPropertyRelative("isMultiSelect");
			this.SetProperty(isMultiSelectProp, position, ref drawerHeight);

			var defaultProp = property.FindPropertyRelative("defaultSelection");
			if (isMultiSelectProp.boolValue)
			{
				var selected = defaultProp.intValue;
				var newSelection = 0;
				int bit = 1;
				this.CreateLabel("Default Selections", position, ref drawerHeight);
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

			this.SetProperty(property, "onValueChangedAction", position, ref drawerHeight);


			isExpanded = this.CreateFoldout("Overridable values", isExpanded, position, ref drawerHeight);
			if (isExpanded)
			{
				++EditorGUI.indentLevel;
				var textSOProp = property.FindPropertyRelative("scriptableObj");
				this.SetProperty(textSOProp, position, ref drawerHeight);

				var customFontSizeProp = property.FindPropertyRelative("fontSize");
				var customFontColorProp = property.FindPropertyRelative("fontColor");
				var customFontAssetProp = property.FindPropertyRelative("fontAsset");
				var textSo = (UIExpandingLabelScriptableObject)textSOProp.boxedValue;
				if (textSo != null)
				{
					var isCustomFontSizeProp = property.FindPropertyRelative("useCustomFontSize");
					this.SetOverridableProperty(isCustomFontSizeProp, customFontSizeProp, textSo.fontSize, position, ref drawerHeight);

					var isCustomFontColorProp = property.FindPropertyRelative("useCustomFontColor");
					this.SetOverridableProperty(isCustomFontColorProp, customFontColorProp, textSo.fontColor, position, ref drawerHeight);

					var isCustomFontAssetProp = property.FindPropertyRelative("useCustomFontAsset");
					this.SetOverridableProperty(isCustomFontAssetProp, customFontAssetProp, textSo.fontColor, position, ref drawerHeight);
				}
				--EditorGUI.indentLevel;
			}



			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ImageViewDataEx))]
	public class ImageViewDataExDrawer : DrawerEx
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			var imageViewProp = property.FindPropertyRelative("scriptableObj");
			this.SetProperty(imageViewProp, position, ref drawerHeight);

			var imageViewSO = (UIImageViewPanelScriptableObject)imageViewProp.boxedValue;

			var maxPanelSize = imageViewSO.maxPanelSize;
			var imageSize = imageViewSO.imageSize;

			//var maxPanelSizeProp = property.FindPropertyRelative("maxPanelSize");
			//this.SetProperty(maxPanelSizeProp, position, ref drawerHeight);
			this.SetProperty(property, "useAllAvailableHeight", position, ref drawerHeight);

			imageViewSO.maxPanelSize = this.CreateVector2Field(maxPanelSize, "Max Panel Size", position, ref drawerHeight);
			imageViewSO.imageSize = this.CreateVector2Field(imageSize, "Image Size", position, ref drawerHeight);


			//var imageSizeProp = property.FindPropertyRelative("imageSize");
			//var imagePerRowProp = property.FindPropertyRelative("imagesPerRow");
			//var imageSize = imageSizeProp.vector2Value;
			//var maxPanelSize = maxPanelSizeProp.vector2Value;

			//this.SetProperty(imageSizeProp, position, ref drawerHeight);
			//this.SetProperty(imagePerRowProp, position, ref drawerHeight);




			//this.SetProperty(property, "defaultSprite", position, ref drawerHeight);



			this.SetProperty(property, "showCaptions", position, ref drawerHeight);
			var showCaptionsProp = property.FindPropertyRelative("showCaptions");
			if (showCaptionsProp.boolValue)
			{
				var labelProp = property.FindPropertyRelative("labelEx");
				var maxLabelSize = labelProp.FindPropertyRelative("maxLabelDimensions").vector2Value;
				imageSize.x = Mathf.Max(imageSize.x, maxLabelSize.x);
				imageSize.y += maxLabelSize.y;
				this.SetProperty(labelProp, position, ref drawerHeight);
			}

			//EditorGUILayout.Vector2Field("Cell Size", imageSize);

			//int imagesOnRow = Mathf.FloorToInt(maxPanelSize.x / imageSize.x);
			//var minWidthReq = imageSize.x * imagesOnRow;

			//EditorGUILayout.FloatField("Internal Panel Dimensions", minWidthReq);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ImageEx))]
	public class ImageExDrawer : DrawerEx
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			this.SetProperty(property, "isVisible", position, ref drawerHeight);
			this.SetProperty(property, "sprite", position, ref drawerHeight);
			this.SetProperty(property, "forceSize", position, ref drawerHeight);
			var forceSizeProp = property.FindPropertyRelative("forceSize");
			if (forceSizeProp.boolValue)
				this.SetProperty(property, "size", position, ref drawerHeight);
			this.SetProperty(property, "showCaption", position, ref drawerHeight);
			var showCaptionProp = property.FindPropertyRelative("showCaption");
			if (showCaptionProp.boolValue)
				this.SetProperty(property, "labelEx", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	/// <summary>
	/// @TODO(Tristan): maxSigFigs<br/>
	/// </summary>
	[CustomPropertyDrawer(typeof(SliderEx))]
	public class SliderExDrawer : DrawerEx
	{
		private bool isTextLabelExpanded;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			this.SetProperty(property, "isEnabled", position, ref drawerHeight);


			this.SetProperty(property, "fillParentHorizontal", position, ref drawerHeight);
			this.SetProperty(property, "minDimensions", position, ref drawerHeight);

			var sliderSOProp = property.FindPropertyRelative("scriptableObj");
			this.SetProperty(sliderSOProp, position, ref drawerHeight);


			var boolProp = property.FindPropertyRelative("wholeNumbers");
			var minProp = property.FindPropertyRelative("minValue");
			var maxProp = property.FindPropertyRelative("maxValue");
			var valueProp = property.FindPropertyRelative("value");


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


			var sliderSO = (UISliderScriptableObject)sliderSOProp.boxedValue;


			var useCustomShowHandleProp = property.FindPropertyRelative("useCustomShowHandle");
			var customShowHandleProp = property.FindPropertyRelative("showHandle");
			this.SetOverridableProperty(sliderSO == null, sliderSO != null && sliderSO.showHandle,
				useCustomShowHandleProp, customShowHandleProp, position, ref drawerHeight);

			if (customShowHandleProp.boolValue || (sliderSO != null && sliderSO.showHandle && !useCustomShowHandleProp.boolValue))
			{
				this.SetProperty(property, "handleOffset", position, ref drawerHeight);
			}


			var useCustomShowUnitsProp = property.FindPropertyRelative("useCustomShowUnits");
			var customShowUnitsProp = property.FindPropertyRelative("showUnits");
			this.SetOverridableProperty(sliderSO == null, sliderSO != null && sliderSO.showUnits,
				useCustomShowUnitsProp, customShowUnitsProp, position, ref drawerHeight);

			if (sliderSO == null)
			{
				if (customShowUnitsProp.boolValue)
				{
					var useCustomUnitSpanProp = property.FindPropertyRelative("useCustomUnitSpan");
					useCustomUnitSpanProp.boolValue = true;

					var customUnitSpanProp = property.FindPropertyRelative("unitSpan");
					var spanValue = customUnitSpanProp.floatValue;

					float range = newMaxValue - newMinValue;
					if (newIsInt)
						customUnitSpanProp.floatValue = this.CreateSlider("Unit Span", (int)spanValue, 1, (int)range, position, ref drawerHeight);
					else
					{
						// @TODO(Tristan): maxSigFigs
						customUnitSpanProp.floatValue = this.CreateSlider("Unit Span", spanValue, range / 10.0f, range / 2.0f, position, ref drawerHeight);
					}
				}
			}
			else
			{
				if ((useCustomShowHandleProp.boolValue && customShowUnitsProp.boolValue)
					|| (!useCustomShowHandleProp.boolValue && sliderSO.showUnits))
				{
					var useCustomUnitSpanProp = property.FindPropertyRelative("useCustomUnitSpan");

					this.SetProperty(useCustomUnitSpanProp, position, ref drawerHeight);
					if (useCustomUnitSpanProp.boolValue)
					{
						var customUnitSpanProp = property.FindPropertyRelative("unitSpan");
						var spanValue = customUnitSpanProp.floatValue;

						float range = newMaxValue - newMinValue;
						if (newIsInt)
							customUnitSpanProp.floatValue = this.CreateSlider("Unit Span", (int)spanValue, 1, (int)range, position, ref drawerHeight);
						else
						{
							// @TODO(Tristan): maxSigFigs
							customUnitSpanProp.floatValue = this.CreateSlider("Unit Span", spanValue, range / 10.0f, range / 2.0f, position, ref drawerHeight);
						}
					}
					else
					{
						GUI.enabled = false;
						if (newIsInt)
							this.CreateIntField((int)sliderSO.unitSpan, "default unit span value", position, ref drawerHeight);
						else
							this.CreateFloatField(sliderSO.unitSpan, "default unit span value", position, ref drawerHeight);
						GUI.enabled = true;
					}
				}
				else
				{
					GUI.enabled = false;
					if (newIsInt)
						this.CreateIntField((int)sliderSO.unitSpan, "default unit span value", position, ref drawerHeight);
					else
						this.CreateFloatField(sliderSO.unitSpan, "default unit span value", position, ref drawerHeight);
					GUI.enabled = true;
				}

			}


			if (isTextLabelExpanded = this.CreateFoldout("Text settings:", isTextLabelExpanded, position, ref drawerHeight))
			{
				if (sliderSO == null)
				{
					++EditorGUI.indentLevel;
					SerializedProperty labelProp = property.FindPropertyRelative("labelEx");
					this.SetProperty(labelProp, position, ref drawerHeight);

					// draw a short label
					++EditorGUI.indentLevel;
					this.SetProperty(labelProp, "fontSize", position, ref drawerHeight);
					this.SetProperty(labelProp, "fontColor", position, ref drawerHeight);
					this.SetProperty(labelProp, "fontAsset", position, ref drawerHeight);
					--EditorGUI.indentLevel;

					--EditorGUI.indentLevel;
				}
				else
				{
					var labelEx = sliderSO.labelEx;

					++EditorGUI.indentLevel;
					// is this going to serialize?
					labelEx.useCustomFontSize = labelEx.scriptableObj == null || this.CreateToggle(labelEx.useCustomFontSize, "Use custom font size", position, ref drawerHeight);

					if (!labelEx.useCustomFontSize)
					{
						GUI.enabled = false;
						this.CreateFloatField(labelEx.scriptableObj.fontSize, "Font Size", position, ref drawerHeight);
						GUI.enabled = true;
					}
					else
					{
						labelEx.fontSize = this.CreateFloatField(labelEx.fontSize, "Font Size", position, ref drawerHeight);
					}



					labelEx.useCustomFontColor = labelEx.scriptableObj == null || this.CreateToggle(labelEx.useCustomFontColor, "Use custom font color", position, ref drawerHeight);
					if (!labelEx.useCustomFontColor)
					{
						GUI.enabled = false;
						this.CreateColorField(labelEx.scriptableObj.fontColor, "Font Color", position, ref drawerHeight);
						GUI.enabled = true;
					}
					else
					{
						labelEx.fontColor = this.CreateColorField(labelEx.fontColor, "fontColor", position, ref drawerHeight);
					}

					labelEx.useCustomFontAsset = labelEx.scriptableObj == null || this.CreateToggle(labelEx.useCustomFontAsset, "Use custom font asset", position, ref drawerHeight);
					if (!labelEx.useCustomFontAsset)
					{
						GUI.enabled = false;
						this.CreateObjectField<TMP_FontAsset>(labelEx.scriptableObj.fontAsset, "Font Asset", position, ref drawerHeight);
						GUI.enabled = true;
					}
					else
						labelEx.fontAsset = this.CreateObjectField<TMP_FontAsset>(labelEx.fontAsset, "fontAsset", position, ref drawerHeight);

					--EditorGUI.indentLevel;

					//PrefabUtility.RecordPrefabInstancePropertyModifications(pi.uiDesignObject.transform);
				}

			}

			--EditorGUI.indentLevel;
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

	[CustomPropertyDrawer(typeof(CheckBoxEx))]
	public class CheckBoxExDrawer : DrawerEx
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			this.SetProperty(property, "isOnByDefault", position, ref drawerHeight);
			this.SetProperty(property, "labelEx", position, ref drawerHeight);

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(PanelEx))]
	public class PanelExDrawer : DrawerEx
	{
		private bool isExpanded;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight, new GUIContent("Panel Reference Name"));

			isExpanded = this.CreateFoldout("Overridable values", isExpanded, position, ref drawerHeight);
			if (isExpanded)
			{
				++EditorGUI.indentLevel;
				var inputSOProp = property.FindPropertyRelative("scriptableObj");
				this.SetProperty(inputSOProp, position, ref drawerHeight);

				var customBackgroundSpriteProp = property.FindPropertyRelative("backgroundSprite");
				var customMinDimenProp = property.FindPropertyRelative("minDimensions");
				var customLayoutPaddingProp = property.FindPropertyRelative("layoutPadding");
				var customLayoutSpacingProp = property.FindPropertyRelative("layoutSpacing");
				var panelSO = (UIPanelScriptableObject)inputSOProp.boxedValue;

				var isCustomFontSizeProp = property.FindPropertyRelative("useCustomBackgroundSprite");
				this.SetOverridableProperty(isCustomFontSizeProp, customBackgroundSpriteProp, panelSO.backgroundSprite, position, ref drawerHeight);

				var isCustomDimenProp = property.FindPropertyRelative("useCustomMinDimensions");
				this.SetOverridableProperty(isCustomDimenProp, customMinDimenProp, panelSO.minDimensions, position, ref drawerHeight);

				var isCustomLayoutPaddingProp = property.FindPropertyRelative("useCustomLayoutPadding");
				this.SetOverridableProperty(isCustomLayoutPaddingProp, customLayoutPaddingProp, panelSO.layoutPadding, position, ref drawerHeight);

				var isCustomLayoutSpacingProp = property.FindPropertyRelative("useCustomLayoutSpacing");
				this.SetOverridableProperty(isCustomLayoutSpacingProp, customLayoutSpacingProp, panelSO.layoutSpacing, position, ref drawerHeight);
				--EditorGUI.indentLevel;
			}


			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(InputFieldEx))]
	public class InputFieldExDrawer : DrawerEx
	{
		private bool isExpanded;

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			this.SetProperty(property, "placeholderText", position, ref drawerHeight);
			this.SetProperty(property, "defaultText", position, ref drawerHeight);

			this.SetProperty(property, "fieldDimensions", position, ref drawerHeight);

			var inputSOProp = property.FindPropertyRelative("scriptableObj");
			if (inputSOProp == null || inputSOProp.boxedValue == null)
			{
				this.CreateLabel("Text Parameters", position, ref drawerHeight);
				++EditorGUI.indentLevel;
			
				this.SetProperty(inputSOProp, position, ref drawerHeight);

				var customFontSizeProp = property.FindPropertyRelative("fontSize");
				this.SetProperty(customFontSizeProp, position, ref drawerHeight);
				var customFontColorProp = property.FindPropertyRelative("fontColor");
				this.SetProperty(customFontColorProp, position, ref drawerHeight);
				var customPlaceholderFontColorProp = property.FindPropertyRelative("placeholderFontColor");
				this.SetProperty(customPlaceholderFontColorProp, position, ref drawerHeight);

				--EditorGUI.indentLevel;
			}
			else
			{
				isExpanded = this.CreateFoldout("Scriptable Object values", isExpanded, position, ref drawerHeight);
				if (isExpanded)
				{
					++EditorGUI.indentLevel;

					this.SetProperty(inputSOProp, position, ref drawerHeight);

					var customFontSizeProp = property.FindPropertyRelative("fontSize");
					var customFontColorProp = property.FindPropertyRelative("fontColor");
					var customPlaceholderFontColorProp = property.FindPropertyRelative("placeholderFontColor");
					var customFontAssetProp = property.FindPropertyRelative("fontAsset");
					var textSo = (UIExpandingInputFieldScriptableObject)inputSOProp.boxedValue;

					var isCustomFontSizeProp = property.FindPropertyRelative("useCustomFontSize");
					this.SetOverridableProperty(isCustomFontSizeProp, customFontSizeProp, textSo.fontSize, position, ref drawerHeight);

					var isCustomFontColorProp = property.FindPropertyRelative("useCustomFontColor");
					this.SetOverridableProperty(isCustomFontColorProp, customFontColorProp, textSo.fontColor, position, ref drawerHeight);

					var isCustomPlaceholderFontColorProp = property.FindPropertyRelative("useCustomPlaceholderFontColor");
					this.SetOverridableProperty(isCustomPlaceholderFontColorProp, customPlaceholderFontColorProp, textSo.placeholderFontColor, position, ref drawerHeight);

					var isCustomFontAssetProp = property.FindPropertyRelative("useCustomFontAsset");
					this.SetOverridableProperty(isCustomFontAssetProp, customFontAssetProp, textSo.fontColor, position, ref drawerHeight);

					--EditorGUI.indentLevel;
				}


				EditorGUI.EndProperty();
			}
		}
	}



	[CustomPropertyDrawer(typeof(LabelEx))]
	public class LabelExDrawer : DrawerEx
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight, new GUIContent("Label Reference Name"));

			var minDimOverrideProp = property.FindPropertyRelative("minLabelDimensions");
			var maxDimOverrideProp = property.FindPropertyRelative("maxLabelDimensions");

			this.CreateLabel("minDimOverrideProp", position, ref drawerHeight);
			this.SetProperty(minDimOverrideProp, position, ref drawerHeight);

			this.CreateLabel("maxDimOverrideProp", position, ref drawerHeight);
			this.SetProperty(maxDimOverrideProp, position, ref drawerHeight);

			{
				++EditorGUI.indentLevel;
				var textSOProp = property.FindPropertyRelative("scriptableObj");
				this.SetProperty(textSOProp, position, ref drawerHeight);

				var customFontSizeProp = property.FindPropertyRelative("fontSize");
				var customFontColorProp = property.FindPropertyRelative("fontColor");
				var customFontAssetProp = property.FindPropertyRelative("fontAsset");

				var textSo = (UIExpandingLabelScriptableObject)textSOProp.boxedValue;
				if (textSo != null)
				{
					var isCustomFontSizeProp = property.FindPropertyRelative("useCustomFontSize");
					this.SetOverridableProperty(isCustomFontSizeProp, customFontSizeProp, textSo.fontSize, position, ref drawerHeight);

					var isCustomFontColorProp = property.FindPropertyRelative("useCustomFontColor");
					this.SetOverridableProperty(isCustomFontColorProp, customFontColorProp, textSo.fontColor, position, ref drawerHeight);

					var isCustomFontAssetProp = property.FindPropertyRelative("useCustomFontAsset");
					this.SetOverridableProperty(isCustomFontAssetProp, customFontAssetProp, textSo.fontAsset, position, ref drawerHeight);
				}
				else
				{
					this.SetProperty(customFontSizeProp, position, ref drawerHeight);
					this.SetProperty(customFontColorProp, position, ref drawerHeight);
					this.SetProperty(customFontAssetProp, position, ref drawerHeight);
				}
				--EditorGUI.indentLevel;
			}

			EditorGUI.EndProperty();
		}
	}


	[CustomPropertyDrawer(typeof(ButtonEx))]
	public class ButtonExDrawer : DrawerEx
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			var buttonSOProp = property.FindPropertyRelative("scriptableObj");
			this.SetProperty(buttonSOProp, position, ref drawerHeight);

			var buttonSO = (UIButtonScriptableObject)buttonSOProp.boxedValue;
			if (buttonSO == null)
			{
				this.SetProperty(property, "labelEx", position, ref drawerHeight);
			}
			//else
			//{
			//	var text = buttonSO.labelEx.text;
			//	buttonSO.labelEx.text = this.CreateTextField("Button Text", text, position, ref drawerHeight);
			//}

			this.SetProperty(property, "fillParentHorizontal", position, ref drawerHeight);


			var useCustomSpriteProp = property.FindPropertyRelative("useCustomSprite");
			var customSpriteProp = property.FindPropertyRelative("sprite");

			if (buttonSO == null)
			{
				useCustomSpriteProp.boolValue = true;
				this.SetProperty(property, "sprite", position, ref drawerHeight);
			}
			else
			{
				this.SetOverridableProperty(useCustomSpriteProp, customSpriteProp, buttonSO.sprite, position, ref drawerHeight);
			}

			this.SetProperty(property, "action", position, ref drawerHeight);


			this.CreateLabel("Text Label Settings:", position, ref drawerHeight);
			if (buttonSO == null)
			{
				++EditorGUI.indentLevel;
				SerializedProperty labelProp = property.FindPropertyRelative("labelEx");
				this.SetProperty(labelProp, position, ref drawerHeight);

				// draw a short label
				++EditorGUI.indentLevel;
				this.SetProperty(labelProp, "fontSize", position, ref drawerHeight);
				this.SetProperty(labelProp, "fontColor", position, ref drawerHeight);
				this.SetProperty(labelProp, "fontAsset", position, ref drawerHeight);
				--EditorGUI.indentLevel;
				--EditorGUI.indentLevel;
			}
			else
			{
				var labelEx = buttonSO.labelEx;

				++EditorGUI.indentLevel;
				// is this going to serialize?
				labelEx.useCustomFontSize = labelEx.scriptableObj == null
					|| this.CreateToggle(labelEx.useCustomFontSize, "Use custom font size", position, ref drawerHeight);

				if (!labelEx.useCustomFontSize)
				{
					GUI.enabled = false;
					this.CreateFloatField(labelEx.scriptableObj.fontSize, "Font Size", position, ref drawerHeight);
					GUI.enabled = true;
				}
				else
				{
					labelEx.fontSize = this.CreateFloatField(labelEx.fontSize, "Font Size", position, ref drawerHeight);
				}



				labelEx.useCustomFontColor = labelEx.scriptableObj == null || this.CreateToggle(labelEx.useCustomFontColor, "Use custom font color", position, ref drawerHeight);
				if (!labelEx.useCustomFontColor)
				{
					GUI.enabled = false;
					this.CreateColorField(labelEx.scriptableObj.fontColor, "Font Color", position, ref drawerHeight);
					GUI.enabled = true;
				}
				else
				{
					labelEx.fontColor = this.CreateColorField(labelEx.fontColor, "fontColor", position, ref drawerHeight);
				}

				labelEx.useCustomFontAsset = labelEx.scriptableObj == null || this.CreateToggle(labelEx.useCustomFontAsset, "Use custom font asset", position, ref drawerHeight);
				if (!labelEx.useCustomFontAsset)
				{
					GUI.enabled = false;
					this.CreateObjectField<TMP_FontAsset>(labelEx.scriptableObj.fontAsset, "Font Asset", position, ref drawerHeight);
					GUI.enabled = true;
				}
				else
					labelEx.fontAsset = this.CreateObjectField<TMP_FontAsset>(labelEx.fontAsset, "fontAsset", position, ref drawerHeight);

				--EditorGUI.indentLevel;

				//PrefabUtility.RecordPrefabInstancePropertyModifications(pi.uiDesignObject.transform);
			}

			EditorGUI.EndProperty();
		}
	}

	[CustomPropertyDrawer(typeof(ButtonPanelEx))]
	public class ButtonDataDrawer : DrawerEx
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);
			drawerHeight = 0;

			this.SetProperty(property, "referenceName", position, ref drawerHeight);

			var enumRect = new Rect(position.x, position.y + drawerHeight, position.width, EditorGUIUtility.singleLineHeight);
			drawerHeight += enumRect.height + EditorGUIUtility.standardVerticalSpacing;
			EditorGUI.PropertyField(enumRect, property.FindPropertyRelative("buttons"));

			var panelScriptObjProp = property.FindPropertyRelative("scriptableObj");
			this.SetProperty(panelScriptObjProp, position, ref drawerHeight);
			var panelScriptObj = (UIButtonPanelScriptableObject)panelScriptObjProp.boxedValue;

			CreateButtonEditor(property, "OK Button Settings", "useCustomOKButton", "okButton", position);
			CreateButtonEditor(property, "Cancel Button Settings", "useCustomCancelButton", "cancelButton", position);
			CreateButtonEditor(property, "Yes Button Settings", "useCustomYesButton", "yesButton", position);
			CreateButtonEditor(property, "No Button Settings", "useCustomNoButton", "noButton", position);


			EditorGUI.EndProperty();
		}

		private void CreateButtonEditor(SerializedProperty property, string label, string useCustomPropName, string customButtonPropName, Rect position)
		{
			this.CreateLabel(label, position, ref drawerHeight);
			++EditorGUI.indentLevel;
			var useCustomOKButtonProp = property.FindPropertyRelative(useCustomPropName);
			var customOKButtonProp = property.FindPropertyRelative(customButtonPropName);

			this.SetProperty(useCustomOKButtonProp, position, ref drawerHeight);
			++EditorGUI.indentLevel;
			if (useCustomOKButtonProp.boolValue)
				this.SetProperty(customOKButtonProp, position, ref drawerHeight);
			else
			{
				//GUI.enabled = false;
				//var minRect = new Rect(position.x, position.y + drawerHeight,
				//	position.width, EditorGUI.GetPropertyHeight(customOKButtonProp, true));
				//EditorGUI.ObjectField(minRect, "default sprite", panelScriptObj.okButton, typeof(Sprite), false);
				//drawerHeight += minRect.height + EditorGUIUtility.standardVerticalSpacing;
				//GUI.enabled = true;
			}
			--EditorGUI.indentLevel;
			--EditorGUI.indentLevel;
		}
	}
}
