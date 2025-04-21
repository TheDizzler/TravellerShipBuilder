using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


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
			panel.UpdatePanel(panel.titleType);
		}
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



[CustomEditor(typeof(ExpandingLabel))]
public class LabelEditor : Editor
{
	private string lastText;
	ExpandingLabel panel;

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if (panel == null)
			panel = (ExpandingLabel)target;
		if (EditorGUI.EndChangeCheck())
		{
			panel.UpdateText();
			lastText = panel.text;
		}
	}
}


[CustomEditor(typeof(DialogPanel))]
public class DialogPanelEditor : Editor
{
	public void OnSceneGUI()
	{
		// lock the panel size it can't get changed except by it's parent 
		DialogPanel panel = (DialogPanel)target;
		var rect = panel.GetComponent<RectTransform>();
		rect.offsetMin = Vector2.zero;
		rect.offsetMax = Vector2.zero;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			DialogPanel panel = (DialogPanel)target;
			panel.SetButtons(panel.buttons);
			panel.SetText(panel.text);
		}
	}
}

