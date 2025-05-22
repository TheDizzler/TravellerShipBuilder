using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CheckBoxEx : UIDataEx
{
	public PanelItemType dataType { get { return PanelItemType.CheckBox; } }

	public LabelEx labelEx = new LabelEx
	{
		fontColor = Color.black,
		fontSize = 18,
		text = "CheckBox",
	};
	public bool isOn = false;

	public void ResetToDefaults()
	{
		labelEx.ResetToDefaults();
		labelEx.fontSize = 18;
		labelEx.fontColor = Color.black;
		labelEx.text = "CheckBox";
		isOn = false;
	}

	public object Clone()
	{
		var clone = (CheckBoxEx)this.MemberwiseClone();
		clone.labelEx = (LabelEx)labelEx.Clone();
		return clone;
	}
}

public class UICheckBox : MonoBehaviour, IUIBehavior
{
	[SerializeField] private CheckBoxEx checkBoxEx;
	[SerializeField] private RectTransform background;
	public UIExpandingLabel textLabel;
	public Toggle toggle;

	private UIDesignObject _designObject;
	public UIDesignObject designObject
	{
		get
		{
			if (_designObject == null)
				_designObject = GetComponent<UIDesignObject>();
			return _designObject;
		}
	}

	public UIDataEx GetBackingData()
	{
		return checkBoxEx;
	}

	public void UpdateBackingData(UIDataEx backingData)
	{
		checkBoxEx = (CheckBoxEx)backingData;
		UpdateBackingData();
	}

	public void UpdateBackingData()
	{
		textLabel.UpdateBackingData(checkBoxEx.labelEx);
		toggle.isOn = checkBoxEx.isOn;

		var minDim = textLabel.GetMinDimensions();
		var layout = GetComponent<HorizontalLayoutGroup>();
		var space = layout.spacing;
		var imageDim = background.sizeDelta;
		minDim.x += imageDim.x + space;
		if (minDim.y < imageDim.y)
			minDim.y = imageDim.y;
		var rect = GetComponent<RectTransform>();
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minDim.x);
		rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);
	}

	public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
	{
		throw new System.NotImplementedException();
	}

	public void Deselect()
	{
		throw new System.NotImplementedException();
	}

	public Vector2 GetMinDimensions()
	{
		UpdateBackingData();
		//var minDim = textLabel.GetMinDimensions();
		//var layout = GetComponent<HorizontalLayoutGroup>();
		//var space = layout.spacing;
		//var imageDim = background.sizeDelta;
		//minDim.x += imageDim.x + space;
		//if (minDim.y < imageDim.y)
		//	minDim.y = imageDim.y;
		return GetComponent<RectTransform>().sizeDelta;
	}

	public void ResetToLastPosition()
	{
		throw new System.NotImplementedException();
	}

	public UIDesignObject Select()
	{
		throw new System.NotImplementedException();
	}

	public void SetHover(bool isHover)
	{
		throw new System.NotImplementedException();
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		throw new System.NotImplementedException();
	}
}
