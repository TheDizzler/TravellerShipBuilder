using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public enum PanelItemType
{
	Text,
	InputField,
	Buttons
}

[Serializable]
public class CreatePanelItem
{
	public PanelItemType dialogItemType;

	public LabelEx labelEx;
	public InputFieldEx inputFieldEx;

	public ButtonPanel.DialogButton buttons;
}



[Serializable]
public class PanelItem
{
	public PanelItemType dialogItemType;
	public UIDesignObject uiDO;
}

[RequireComponent(typeof(DynamicPanel))]
public class DynamicPanelOperator : MonoBehaviour
{
	[SerializeField] private CreatePanelItem createPanelItem;
	//[SerializeField] private List<PanelItem> panelItems;

	void Start()
	{
#if !UNITY_EDITOR
		Destroy(this);
#endif
	}

	public void AddItem()
	{
		var panel = GetComponent<DynamicPanel>();
		panel.AddItem(createPanelItem);
	}


	public void Clear()
	{
		var panel = GetComponent<DynamicPanel>();
		panel.ClearItemsEditor();
	}

	public void ChangeMaxDims(Vector2 maxDims)
	{
		var panel = GetComponent<DynamicPanel>();
		panel.UpdateMaxDimensions(maxDims);
	}

	public void SetAlwaysShrink(bool alwaysShrink)
	{
		var panel = GetComponent<DynamicPanel>();
		panel.SetAlwaysShrink(alwaysShrink);
	}

	public void ChangeTitleStyle(DynamicPanel.TitleLabelStyle titleStyle)
	{
		var panel = GetComponent<DynamicPanel>();
		panel.SetTitleStyle(titleStyle);
	}

	public void SetTitleText(string titleText)
	{
		var panel = GetComponent<DynamicPanel>();
		panel.SetTitleText(titleText);
	}

	public void SetPanelStyle(DynamicPanel.BottomPanelStyle panelStyle)
	{
		var panel = GetComponent<DynamicPanel>();
		panel.SetPanelStyle(panelStyle);
	}
}
