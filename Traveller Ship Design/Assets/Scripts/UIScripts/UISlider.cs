using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SliderEx : UIDataEx
{
	public PanelItemType dataType { get { return PanelItemType.Slider; } }

	public float minValue = 0;
	public float maxValue = 4;
	public float value = 0;
	public bool wholeNumbers = true;
	public bool showUnits = true;
	public int unitCount = 2;
	public int unitVerticalOffset = 16;
	public Vector2 handleOffset = new Vector2(16, 16);
	public LabelEx labelEx = new LabelEx
	{
		fontColor = Color.black,
		fontSize = 18,
		minLabelDimensions = new Vector2(8, 8),
	};


	public void ResetToDefaults()
	{
		minValue = 0;
		maxValue = 4;
		value = 0;
		wholeNumbers = true;
		showUnits = false;
		unitCount = 2;
		unitVerticalOffset = 16;
		handleOffset = new Vector2(16, 16);

		labelEx.ResetToDefaults();
		labelEx.fontColor = Color.black;
		labelEx.fontSize = 18;
		labelEx.minLabelDimensions = new Vector2(8, 8);
	}

	public object Clone()
	{
		var clone = (SliderEx)this.MemberwiseClone();
		clone.labelEx = (LabelEx)labelEx.Clone();
		return clone;
	}
}

public class UISlider : MonoBehaviour, IUIBehavior
{
	[SerializeField] private SliderEx sliderEx;


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
		return sliderEx;
	}

	public void UpdateBackingData(UIDataEx backingData)
	{
		sliderEx = (SliderEx)backingData;
		UpdateBackingData();
	}

	public void UpdateBackingData()
	{
		GetComponent<SliderCustom>().UpdateSlider(sliderEx);
	}

	public Vector2 GetMinDimensions()
	{
		UpdateBackingData();
		return GetComponent<SliderCustom>().GetMinDimensions();
	}

	public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
	{
		throw new System.NotImplementedException();
	}

	public void Deselect()
	{
		throw new System.NotImplementedException();
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
