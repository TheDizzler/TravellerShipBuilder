using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class CheckBoxEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.CheckBox; } }

		public UICheckBoxScriptableObject scriptableObj;

		public bool useCustomCheckImage = false;
		public bool useCustomBoxImage = false;
		public Sprite checkSprite;
		public Sprite boxSprite;


		public bool isOnByDefault = false;
		public LabelEx labelEx;

		//[Tooltip("Default: 14.")]
		//public float fontSize = 14;
		//[Tooltip("Default: Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1).")]
		//public Color fontColor = Color.black;

		public UnityEvent<bool> action = null;


		public CheckBoxEx(UICheckBoxScriptableObject scriptObj)
		{
			this.scriptableObj = scriptObj;
			SetToScriptableObjectValues();
		}

		public void SetToScriptableObjectValues()
		{
			if (scriptableObj == null)
				ResetToDefaults();
			else
			{
				scriptableObj.labelEx.SetToScriptableObjectValues();
			}
		}

		private void ResetToDefaults()
		{
			if (labelEx != null)
				labelEx.ResetToDefaults();

			useCustomCheckImage = true;
			useCustomBoxImage = true;

			if (action != null)
				action.RemoveAllListeners();
			isOnByDefault = false;
		}
	}

	public class UICheckBox : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private CheckBoxEx checkBoxEx;
		[SerializeField] private RectTransform backgroundRect;
		[SerializeField] private Image boxImage;
		[SerializeField] private Image checkImage;

		public UIExpandingLabel textLabel;
		public Toggle toggle;

		private UIDesignObject _designObject;
		public DynamicPanel parentPanel;

		public UIDesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<UIDesignObject>();
				return _designObject;
			}
		}

		private Sprite boxSprite
		{
			get
			{
				if (checkBoxEx.useCustomBoxImage || checkBoxEx.scriptableObj == null)
					return checkBoxEx.boxSprite;
				return checkBoxEx.scriptableObj.boxSprite;
			}
		}

		private Sprite checkSprite
		{
			get
			{
				if (checkBoxEx.useCustomCheckImage || checkBoxEx.scriptableObj == null)
					return checkBoxEx.checkSprite;
				return checkBoxEx.scriptableObj.checkSprite;
			}
		}

		public IUIDataEx labelEx
		{
			get
			{
				if (checkBoxEx.scriptableObj == null)
				{
					if (checkBoxEx.labelEx == null)
						throw new Exception("A LabelEx is required");
					return checkBoxEx.labelEx;
				}

				return checkBoxEx.scriptableObj.labelEx;
			}
		}

		public IUIDataEx GetBackingData()
		{
			return checkBoxEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			checkBoxEx = (CheckBoxEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			toggle.isOn = checkBoxEx.isOnByDefault;
			toggle.onValueChanged.RemoveAllListeners();
			toggle.onValueChanged.AddListener(OnToggled);

			var sprite = boxSprite;
			if (sprite != null)
				boxImage.sprite = sprite;
			sprite = checkSprite;
			if (sprite != null)
				checkImage.sprite = sprite;

			textLabel.UpdateBackingData(labelEx);




			var minDim = textLabel.GetMinDimensions();
			var layout = GetComponent<HorizontalLayoutGroup>();
			var space = layout.spacing;
			var imageDim = backgroundRect.sizeDelta;
			minDim.x += imageDim.x + space;
			if (minDim.y < imageDim.y)
				minDim.y = imageDim.y;
			var rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minDim.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);
		}

		private void OnToggled(bool isToggled)
		{
			checkBoxEx.isOnByDefault = toggle.isOn;
			if (checkBoxEx.action != null)
				checkBoxEx.action.Invoke(toggle.isOn);
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
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
			//var imageDim = backgroundRect.sizeDelta;
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
}