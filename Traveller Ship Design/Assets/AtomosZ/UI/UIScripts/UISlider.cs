using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	/// <summary>
	/// @TODO(Tristan): maxSigFigs
	/// </summary>
	[Serializable]
	public class SliderEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Slider; } }
		public string referenceName;

		public UISliderScriptableObject scriptableObj;
		public bool fillParentHorizontal = true;
		public Vector2 minDimensions = new Vector2(126, 64);

		public bool useCustomShowHandle = false;
		//public bool useCustomHandleSprite = false;
		//public bool useCustomHandleOffset = false;
		public bool useCustomShowUnits = false;
		public bool useCustomUnitSpan = false;
		//public bool useCustomUnitVerticalOffset = false;

		public bool showHandle = true;
		public Sprite handleSprite;
		public Vector2 handleOffset = new Vector2(16, 16);

		public bool showUnits = true;
		public float unitSpan = 1;
		public int unitVerticalOffset = 0;


		public float minValue = 0;
		public float maxValue = 4;
		public float value = 0;
		public bool wholeNumbers = true;

		public LabelEx labelEx;

		public SliderEx(UISliderScriptableObject sliderSO)
		{
			scriptableObj = sliderSO;
			if (sliderSO == null)
			{
				useCustomShowHandle = true;
				useCustomShowUnits = true;
				useCustomUnitSpan = true;
				labelEx = new LabelEx("")
				{
					referenceName = "_slider_labelEx",
					fontColor = Color.black,
					fontSize = 18,
					minLabelDimensions = new Vector2(8, 8),
				};
			}
		}
	}

	public class UISlider : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private SliderEx sliderEx;
		public string referenceName { get { return sliderEx.referenceName; } }

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

		public float GetValue()
		{
			return sliderEx.value;
		}

		public void SetValue(float value)
		{
			sliderEx.value = value;
		}

		public IUIDataEx GetBackingData()
		{
			return sliderEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			sliderEx = (SliderEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			if (string.IsNullOrEmpty(sliderEx.referenceName))
				sliderEx.referenceName = transform.name;
			//name = sliderEx.referenceName;

			var layout = GetComponent<LayoutElement>();
			if (sliderEx.fillParentHorizontal)
				layout.flexibleWidth = 1;
			else
				layout.flexibleWidth = 0;

			layout.minWidth = sliderEx.minDimensions.x;
			layout.minHeight = sliderEx.minDimensions.y;

			Canvas.ForceUpdateCanvases();
			var slider = GetComponent<SliderCustom>();
			slider.UpdateSlider(sliderEx);
#if DEBUG
			// this required or the handle will not report the correct value after becoming active.
			if (!Application.isPlaying)
				slider.UpdateSlider(sliderEx);
#endif
			//var sliderDim = slider.GetMinDimensions();
			//if (sliderDim.x < sliderEx.minDimensions.x)
			//	sliderDim.x = sliderEx.minDimensions.x;
			
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			//return GetComponent<SliderCustom>().GetMinDimensions();
			return GetComponent<RectTransform>().sizeDelta;
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput,
			ref UIDesignObject currentlySelectedObject)
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
}