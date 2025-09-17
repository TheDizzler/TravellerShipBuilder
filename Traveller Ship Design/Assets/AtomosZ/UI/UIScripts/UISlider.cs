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

		public UISliderScriptableObject scriptableObj;

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
		// = new LabelEx
		//{
		//fontColor = Color.black,
		//fontSize = 18,
		//minLabelDimensions = new Vector2(8, 8),
		//};

		public SliderEx()
		{
			ResetToDefaults();
		}

		public SliderEx(UISliderScriptableObject sliderSO)
		{
			scriptableObj = sliderSO;
			SetToScriptableObjectValues();
		}

		public void SetToScriptableObjectValues()
		{
			if (scriptableObj == null)
				ResetToDefaults();
			else
			{
				if (scriptableObj.labelEx == null)
				{
					labelEx = new LabelEx()
					{
						fontColor = Color.black,
						fontSize = 18,
						minLabelDimensions = new Vector2(8, 8),
					};
				}
				else
				{
					scriptableObj.labelEx.SetToScriptableObjectValues();
					//scriptableObj.labelEx.fontColor = Color.black;
					//scriptableObj.labelEx.fontSize = 18;
					//scriptableObj.labelEx.minLabelDimensions = new Vector2(8, 8);
				}


				showUnits = scriptableObj.showUnits;
				showHandle = scriptableObj.showHandle;
				unitSpan = scriptableObj.unitSpan;
			}
		}

		public void ResetToDefaults()
		{
			useCustomShowHandle = true;
			useCustomShowUnits = true;
			useCustomUnitSpan = true;
			//useCustomHandleSprite = true;
			//useCustomHandleOffset = true;
			//useCustomUnitVerticalOffset = true;

			minValue = 0;
			maxValue = 4;
			value = 0;
			wholeNumbers = true;
			showUnits = false;
			showHandle = true;
			unitSpan = 1;
			unitVerticalOffset = 0;
			handleOffset = new Vector2(16, 16);

			// what do when labelEx null?
			if (labelEx == null)
			{
				labelEx = new LabelEx()
				{
					fontColor = Color.black,
					fontSize = 18,
					minLabelDimensions = new Vector2(8, 8),
				};
			}
			else
			{
				labelEx.ResetToDefaults();
				labelEx.fontColor = Color.black;
				labelEx.fontSize = 18;
				labelEx.minLabelDimensions = new Vector2(8, 8);
			}
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
			GetComponent<SliderCustom>().UpdateSlider(sliderEx);
#if DEBUG
			// this required or the handle will not report the correct value after becoming active.
			if (!Application.isPlaying)
				GetComponent<SliderCustom>().UpdateSlider(sliderEx);
#endif
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			return GetComponent<SliderCustom>().GetMinDimensions();
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