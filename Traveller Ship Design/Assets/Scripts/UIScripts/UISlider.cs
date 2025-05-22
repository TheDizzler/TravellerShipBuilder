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


	public  void ResetToDefaults()
	{
		minValue = 0;
		maxValue = 4;
		value = 0;
		wholeNumbers = true;
	}
	
	public object Clone()
	{
		return this.MemberwiseClone();
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
