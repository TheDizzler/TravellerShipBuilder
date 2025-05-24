using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ButtonEx : UIDataEx
{
	public PanelItemType dataType { get { return PanelItemType.Button; } }

	public LabelEx labelEx = new LabelEx
	{
		text = "Button Text",
		fontColor = Color.black,
	};


	public void ResetToDefaults()
	{
		labelEx.ResetToDefaults();
		labelEx.text = "Button Text";
		labelEx.fontColor = Color.black;
	}

	public object Clone()
	{
		var clone = (ButtonEx)this.MemberwiseClone();
		clone.labelEx = (LabelEx)labelEx.Clone();
		return clone;
	}
}

public class UIButton : MonoBehaviour, IUIBehavior
{
	[SerializeField] private ButtonEx buttonEx;
	[SerializeField] private UIExpandingLabel label;



	public UIDesignObject _designObject;
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
		// if I'm not mistaken, we should not need to get the labelEx since
		// it SHOULD be the same reference.
		//buttonEx.labelEx = (LabelEx)label.GetBackingData();
		return buttonEx;
	}

	public void UpdateBackingData(UIDataEx backingData)
	{
		buttonEx = (ButtonEx)backingData;
		UpdateBackingData();
	}

	public void UpdateBackingData()
	{
		label.UpdateBackingData(buttonEx.labelEx);
	}


	public Vector2 GetMinDimensions()
	{
		UpdateBackingData();
		return label.GetMinDimensions();
	}


	public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
	{
		throw new NotImplementedException();
	}

	public void Deselect()
	{
		throw new NotImplementedException();
	}

	public void ResetToLastPosition()
	{
		throw new NotImplementedException();
	}

	public UIDesignObject Select()
	{
		throw new NotImplementedException();
	}

	public void SetHover(bool isHover)
	{
		throw new NotImplementedException();
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		throw new NotImplementedException();
	}
}
