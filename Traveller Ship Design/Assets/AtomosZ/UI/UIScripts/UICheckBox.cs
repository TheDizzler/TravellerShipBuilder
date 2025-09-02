using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class CheckBoxEx : IUIDataEx
	{
		public PanelControlType dataType { get { return PanelControlType.CheckBox; } }

		public bool isOn = false;
		public LabelEx labelEx = new LabelEx
		{
			fontColor = Color.black,
			fontSize = 18,
			text = "CheckBox",
		};
		public UnityEvent<bool> action = null;


		public void ResetToDefaults()
		{
			labelEx.ResetToDefaults();
			labelEx.fontSize = 18;
			labelEx.fontColor = Color.black;
			labelEx.text = "CheckBox";
			action = null;
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
			textLabel.UpdateBackingData(checkBoxEx.labelEx);
			toggle.isOn = checkBoxEx.isOn;
			toggle.onValueChanged.RemoveAllListeners();
			toggle.onValueChanged.AddListener(OnToggled);

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

		private void OnToggled(bool isToggled)
		{
			checkBoxEx.isOn = toggle.isOn;
			if (checkBoxEx.action != null)
				checkBoxEx.action.Invoke(toggle.isOn);
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
}