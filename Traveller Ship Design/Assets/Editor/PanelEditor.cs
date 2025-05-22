using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static DynamicPanel;




[CustomEditor(typeof(DynamicPanel))]
public class DynamicPanelEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			DynamicPanel panel = (DynamicPanel)target;
			panel.Refresh();
		}
	}
}

[CustomEditor(typeof(DynamicPanelOperator))]
public class DynamicPanelOperatorEditor : Editor
{
	private TitleLabelStyle titleStyle;
	private string titleText;
	private BottomPanelStyle panelStyle;
	private Vector2 maxDims;
	private bool alwaysShrink;
	private SerializedProperty createPanelItem;
	private BottomPanel bottomPanel;
	private DynamicPanelOperator adder;
	private SerializedProperty panelItemsSO;

	void OnEnable()
	{
		adder = (DynamicPanelOperator)target;
		var panel = adder.GetComponent<DynamicPanel>();
		titleStyle = panel.titleType;
		titleText = panel.titleText;
		panelStyle = panel.panelType;
		maxDims = panel.maxSize;
		alwaysShrink = panel.alwaysShrinkToMinSize;

		createPanelItem = serializedObject.FindProperty("createPanelItem");

		BuildItemList();
	}

	private void BuildItemList()
	{
		bottomPanel = adder.GetComponentInChildren<BottomPanel>();
		var currentItems = bottomPanel.GetItems();
		var items = new List<PanelItem>();
		foreach (var item in currentItems)
		{
			var uiBehavior = item.GetComponent<IUIBehavior>();
			var dataEx = uiBehavior.GetBackingData();
			object panelItem = new PanelItem
			{
				uiDesignObject = uiBehavior.designObject,
				itemType = dataEx.dataType,
			};

			typeof(PanelItem).GetField(PanelItem.panelItemNames[dataEx.dataType]).SetValue(panelItem, dataEx);
			items.Add((PanelItem)panelItem);
		}

		adder.panelItems = items;
		panelItemsSO = serializedObject.FindProperty("panelItems");
	}

	public override void OnInspectorGUI()
	{
		var newTitleType = (TitleLabelStyle)EditorGUILayout.EnumPopup("Title Type", titleStyle);
		if (newTitleType != titleStyle)
		{
			titleStyle = newTitleType;
			adder.ChangeTitleStyle(titleStyle);
		}

		if (titleStyle != TitleLabelStyle.None)
		{
			++EditorGUI.indentLevel;
			var newTitleText = EditorGUILayout.TextField("Title Text?", titleText);
			if (newTitleText != titleText)
			{
				titleText = newTitleText;
				adder.SetTitleText(titleText);
			}
			--EditorGUI.indentLevel;
		}

		var newPanelStyle = (BottomPanelStyle)EditorGUILayout.EnumPopup("Dialog Style", panelStyle);
		if (newPanelStyle != panelStyle)
		{
			panelStyle = newPanelStyle;
			adder.SetPanelStyle(panelStyle);
		}

		var newMaxDims = EditorGUILayout.Vector2Field("Max Panel Size", maxDims);
		if (newMaxDims != maxDims)
		{
			maxDims = newMaxDims;
			adder.ChangeMaxDims(maxDims);
		}

		var newAlwaysShrink = EditorGUILayout.Toggle("Always shrink to min size", alwaysShrink);
		if (newAlwaysShrink != alwaysShrink)
		{
			alwaysShrink = newAlwaysShrink;
			adder.SetAlwaysShrink(alwaysShrink);
		}



		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(createPanelItem);
		EditorGUILayout.PropertyField(panelItemsSO);


		if (serializedObject.ApplyModifiedProperties())
			adder.RecalculateDimensions();

		if (GUILayout.Button("Clear All"))
			adder.Clear();

		if (EditorGUI.EndChangeCheck())
			BuildItemList();
	}
}



[CustomEditor(typeof(BottomPanel))]
public class BottomPanelEditor : Editor
{
	public void OnSceneGUI()
	{
		// lock the panel size so it can't get changed except by it's parent 
		BottomPanel panel = (BottomPanel)target;
		panel.SetToParentsSize();
	}
}


[CustomEditor(typeof(UIButtonPanel))]
public class UIButtonPanelEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			var buttonPanel = (UIButtonPanel)target;
			buttonPanel.UpdateBackingData();
		}
	}
}


[CustomEditor(typeof(UICheckBox))]
public class UICheckBoxEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			var checkBox = (UICheckBox)target;
			checkBox.UpdateBackingData();
		}
	}
}


[CustomEditor(typeof(UIExpandingInputField))]
public class UIExpandingInputFieldEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			var inputField = (UIExpandingInputField)target;
			inputField.UpdateBackingData();
		}
	}
}


[CustomEditor(typeof(UIExpandingLabel))]
public class UILabelEditor : Editor
{
	private SerializedProperty labelEx;
	private SerializedProperty textLabel;
	private SerializedProperty image;

	void OnEnable()
	{
		labelEx = serializedObject.FindProperty("labelEx");


		textLabel = serializedObject.FindProperty("textLabel");
		image = serializedObject.FindProperty("image");

		/// Keeping this here for future reference!
		//var tmpSP = serializedObject.FindProperty("textLabel");
		//var targetObjectClassType = EditorHelper.GetTargetObjectOfProperty(tmpSP);
		//if (targetObjectClassType != null)
		//{
		//	tmp = (TextMeshProUGUI)targetObjectClassType;
		//}
	}

	public override void OnInspectorGUI()
	{
		EditorGUILayout.PropertyField(labelEx);

		GUILayout.Space(10);
		{
			EditorGUILayout.PropertyField(textLabel);
			EditorGUILayout.PropertyField(image);
		}

		if (serializedObject.ApplyModifiedProperties())
		{
			var panel = (UIExpandingLabel)target;
			panel.UpdateBackingData();
		}
	}
}



[CustomEditor(typeof(ExpandingLabel))]
public class LabelEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			var panel = (ExpandingLabel)target;
			panel.UpdateText();
		}
	}
}


[CustomEditor(typeof(UISlider))]
public class SliderCustomEditor : Editor
{
	private UISlider slider;
	private SliderEx sliderEx;
	private RectTransform rect;
	private Vector2 lastSize;

	void OnEnable()
	{
		slider = (UISlider)target;
		sliderEx = (SliderEx)slider.GetBackingData();
		rect = slider.GetComponent<RectTransform>();
		lastSize = rect.sizeDelta;
	}


	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();

		bool newIsInt = EditorGUILayout.Toggle("Whole Numbers", sliderEx.wholeNumbers);
		if (newIsInt != sliderEx.wholeNumbers)
		{
			sliderEx.wholeNumbers = newIsInt;
			if (sliderEx.wholeNumbers)
			{
				sliderEx.minValue = Mathf.RoundToInt(sliderEx.minValue);
				sliderEx.maxValue = Mathf.RoundToInt(sliderEx.maxValue);
				sliderEx.value = Mathf.RoundToInt(sliderEx.value);
			}
		}

		float newMinValue = EditorGUILayout.FloatField("Min Value", sliderEx.minValue);
		if (newMinValue != sliderEx.minValue)
		{
			if (sliderEx.wholeNumbers)
				newMinValue = Mathf.RoundToInt(newMinValue);
			if (newMinValue > sliderEx.maxValue)
				newMinValue = sliderEx.maxValue;
			sliderEx.minValue = newMinValue;
		}

		float newMaxValue = EditorGUILayout.FloatField("Max Value", sliderEx.maxValue);
		if (newMaxValue != sliderEx.maxValue)
		{
			if (sliderEx.wholeNumbers)
				newMaxValue = Mathf.RoundToInt(newMaxValue);
			if (newMaxValue < sliderEx.minValue)
				newMaxValue = sliderEx.minValue;
			sliderEx.maxValue = newMaxValue;
		}

		float newValue = EditorGUILayout.Slider("Value", sliderEx.value, sliderEx.minValue, sliderEx.maxValue);
		if (sliderEx.wholeNumbers)
			newValue = Mathf.RoundToInt(newValue);
		sliderEx.value = newValue;

		if (EditorGUI.EndChangeCheck())
		{
			slider.UpdateBackingData();
		}
	}

	public void OnSceneGUI()
	{
		var size = rect.sizeDelta;
		if (size != lastSize)
		{
			slider.UpdateBackingData();
			lastSize = size;
		}
	}
}