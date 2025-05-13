using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
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
	private DynamicPanelOperator adder;


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
			var newTitleText = EditorGUILayout.TextField(titleText);
			if (newTitleText != titleText)
			{
				titleText = newTitleText;
				adder.SetTitleText(titleText);
			}
			--EditorGUI.indentLevel;
		}

		var newPanelStyle = (BottomPanelStyle)EditorGUILayout.EnumPopup(panelStyle);
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
		if (EditorGUI.EndChangeCheck())
			serializedObject.ApplyModifiedProperties();

		if (GUILayout.Button("Clear"))
			adder.Clear();
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
			inputField.UpdateInputField();
		}
	}
}


[CustomEditor(typeof(UIExpandingLabel))]
public class UILabelEditor : Editor
{
	public string labelText = "Label Text";
	private SerializedProperty labelEx;
	private SerializedProperty minDims;
	private SerializedProperty maxDims;
	private SerializedProperty fontSize;
	private SerializedProperty fontColor;
	private SerializedProperty textLabel;
	private SerializedProperty image;
	private TextMeshProUGUI tmp = null;


	void OnEnable()
	{
		labelEx = serializedObject.FindProperty("label");
		minDims = serializedObject.FindProperty("minLabelDimensions");
		maxDims = serializedObject.FindProperty("maxLabelDimensions");
		fontSize = serializedObject.FindProperty("fontSize");
		fontColor = serializedObject.FindProperty("fontColor");

		textLabel = serializedObject.FindProperty("textLabel");
		image = serializedObject.FindProperty("image");

		var tmpSP = serializedObject.FindProperty("textLabel");
		var targetObjectClassType = EditorHelper.GetTargetObjectOfProperty(tmpSP);
		if (targetObjectClassType != null)
		{
			tmp = (TextMeshProUGUI)targetObjectClassType;
			labelText = tmp.text;
		}
	}

	public override void OnInspectorGUI()
	{
		var panel = (UIExpandingLabel)target;

		EditorGUILayout.BeginVertical();
		{
			var newLabelText = EditorGUILayout.TextField("Label Text", labelText);
			EditorGUILayout.PropertyField(labelEx);
			EditorGUILayout.PropertyField(fontSize);
			EditorGUILayout.PropertyField(fontColor);
			EditorGUILayout.PropertyField(minDims);
			EditorGUILayout.PropertyField(maxDims);
			if (serializedObject.ApplyModifiedProperties() || newLabelText != labelText)
			{
				labelText = newLabelText;
				panel.text = labelText;
			}
		}
		EditorGUILayout.EndVertical();

		if (tmp != null)
		{
			if (tmp.text != labelText)
			{
				tmp.text = labelText;
			}
		}

		GUILayout.Space(10);
		{
			EditorGUILayout.PropertyField(textLabel);
			EditorGUILayout.PropertyField(image);
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
