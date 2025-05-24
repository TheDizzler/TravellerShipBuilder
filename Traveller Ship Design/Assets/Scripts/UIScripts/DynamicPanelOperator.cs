using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface UIDataEx : ICloneable
{
	public PanelItemType dataType { get; }
	public void ResetToDefaults();
}

public enum PanelItemType
{
	Text,
	InputField,
	CheckBox,
	Slider,
	Button,
	ButtonPanel,
}

[Serializable]
/// Needed for two different PropertyDrawer views.
public class CreatePanelItem : PanelItem
{

}


[Serializable]
public class PanelItem
{
	public PanelItemType itemType;
	public static Dictionary<PanelItemType, string> panelItemNames = new()
	{
		[PanelItemType.Text] = "labelEx",
		[PanelItemType.InputField] = "inputFieldEx",
		[PanelItemType.CheckBox] = "checkBoxEx",
		[PanelItemType.Slider] = "sliderEx",
		[PanelItemType.ButtonPanel] = "buttonPanelEx",
		[PanelItemType.Button] = "buttonEx",
	};

	public LabelEx labelEx;
	public InputFieldEx inputFieldEx;
	public CheckBoxEx checkBoxEx;
	public SliderEx sliderEx;
	public ButtonEx buttonEx;
	public ButtonPanelDataEx buttonPanelEx;

	public UIDesignObject uiDesignObject;

	public List<UIDataEx> GetAllItems()
	{
		return new List<UIDataEx>
		{
			labelEx,
			inputFieldEx,
			checkBoxEx,
			sliderEx,
			buttonPanelEx,
			buttonEx,
		};
	}

	public Dictionary<PanelItemType, UIDataEx> GetAllItemsByType()
	{
		var dict = new Dictionary<PanelItemType, UIDataEx>();
		foreach (var data in GetAllItems())
		{
			dict.Add(data.dataType, data);
		}

		return dict;
	}
}

[RequireComponent(typeof(DynamicPanel))]
public class DynamicPanelOperator : MonoBehaviour
{
	[SerializeField] private CreatePanelItem createPanelItem;
	[SerializeField] public List<PanelItem> panelItems;

	void Start()
	{
#if !UNITY_EDITOR
		Destroy(this);
#endif
	}

#if UNITY_EDITOR
	public void ResetToLabelDefaults()
	{
		foreach (UIDataEx itemEx in createPanelItem.GetAllItems())
			itemEx.ResetToDefaults();
	}

	public void RecalculateDimensions()
	{
		var panel = GetComponent<DynamicPanel>();
		panel.RecalculateDimensions();
	}

	public void AddItem()
	{
		var panel = GetComponent<DynamicPanel>();
		panel.AddItem(createPanelItem);
	}

	public void Remove(UIDesignObject uiDO)
	{
		var panel = GetComponent<DynamicPanel>();
		panel.RemoveItem(uiDO);
	}

	public void ResetToDefaults(PanelItem panelItem)
	{
		var allItems = panelItem.GetAllItemsByType();
		allItems[panelItem.itemType].ResetToDefaults();
		panelItem.uiDesignObject.UpdateBackingData(allItems[panelItem.itemType]);

		var panel = GetComponent<DynamicPanel>();
		panel.RecalculateDimensions();
	}

	public void RemoveItem(PanelItem item)
	{
		panelItems.Remove(item);
		var panel = GetComponent<DynamicPanel>();
		panel.RemoveItem(item.uiDesignObject);
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
#endif
}
