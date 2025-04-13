using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using static DynamicPanel;

[CustomEditor(typeof(TabbedPanel))]
public class PanelEditor : Editor
{
	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			TabbedPanel panel = (TabbedPanel)target;
			panel.UpdatePanel();
		}
	}
}

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
			panel.UpdateTitle();
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
		//var rect = panel.GetComponent<RectTransform>();
		//rect.offsetMin = Vector2.zero;
		//rect.offsetMax = Vector2.zero;
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			BottomPanel panel = (BottomPanel)target;
			panel.SetButtons(panel.buttons);
			//panel.SetText(panel.text);
		}
	}
}


[CustomEditor(typeof(BladedPanel))]
public class BladededPanelEditor : Editor
{
	private Vector2 lastSizeDelta = new Vector2(float.MinValue, float.MinValue);

	public void OnSceneGUI()
	{
		BladedPanel panel = (BladedPanel)target;
		var rect = panel.GetComponent<RectTransform>();
		if (rect.sizeDelta != lastSizeDelta)
		{
			panel.RecalculateDimensions();
			lastSizeDelta = rect.sizeDelta;
		}
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();

		if (EditorGUI.EndChangeCheck())
		{
			BladedPanel panel = (BladedPanel)target;
			panel.UpdateTitle();
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
			ExpandingLabel panel = (ExpandingLabel)target;
			panel.UpdateText();
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

