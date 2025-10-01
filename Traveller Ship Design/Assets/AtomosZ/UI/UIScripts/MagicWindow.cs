using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	public class MagicWindow : MonoBehaviour, IUIBehavior
	{

		[SerializeField] private string _referenceName;
		public string referenceName { get { return _referenceName; } }

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

		public UITabControl tabControl;
		public UIPanel panel { get { return tabControl.SelectedPanel(); } }

		//public ;
		[SerializeField] public UIExpandingLabelScriptableObject textScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject dropdownScriptObj;
		[SerializeField] public UICheckBoxScriptableObject checkBoxScriptObj;
		[SerializeField] public UIExpandingInputFieldScriptableObject inputFieldScriptObj;
		[SerializeField] public UISliderScriptableObject sliderScriptObj;
		[SerializeField] public UIButtonScriptableObject buttonScriptObj;
		[SerializeField] public UIButtonPanelScriptableObject buttonPanelScriptObj;
		[SerializeField] public UIImageViewScriptableObject imageViewScriptObj;
		[SerializeField] public UIImageViewPanelScriptableObject imageViewPanelScriptObj;

#if DEBUG
		[SerializeField] public UIControlType currentType;
		public List<UIControl> controlList = new();

		[Conditional("DEBUG")]
		public void ClearControlsEditor()
		{
			tabControl.ClearControlsEditor();
			GetMinDimensions();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void RecordPrefabInstances()
		{
			tabControl.RecordPrefabInstances();
		}
#endif

		void Start()
		{
			UIPrefabProvider uiProvider = GetComponentInParent<UIPrefabProvider>();
			if (textScriptObj == null)
				textScriptObj = uiProvider.textScriptObj;
			if (dropdownScriptObj == null)
				dropdownScriptObj = uiProvider.dropdownScriptObj;
			if (checkBoxScriptObj == null)
				checkBoxScriptObj = uiProvider.checkBoxScriptObj;
			if (inputFieldScriptObj == null)
				inputFieldScriptObj = uiProvider.inputFieldScriptObj;
			if (sliderScriptObj == null)
				sliderScriptObj = uiProvider.sliderScriptObj;
			if (buttonScriptObj == null)
				buttonScriptObj = uiProvider.buttonScriptObj;
			if (buttonPanelScriptObj == null)
				buttonPanelScriptObj = uiProvider.buttonPanelScriptObj;
			if (imageViewScriptObj == null)
				imageViewScriptObj = uiProvider.imageViewScriptObj;
			if (imageViewPanelScriptObj == null)
				imageViewPanelScriptObj = uiProvider.imageViewPanelScriptObj;

		}


		public void RemoveControl(UIControl uiControl)
		{
			var uiData = uiControl.GetData();
			panel.RemoveControl(uiData);
		}



		public UIControlLookup GetControls()
		{
			return panel.GetControls();
		}

		public UIControlLookup GetControlsFromTransform()
		{
			if (panel != null)
				return panel.GetControlsFromTransform();
			Debug.LogException(new Exception("Why is this null?"));
			return null;
		}

		public Vector2 GetMinDimensions()
		{
			var minDim = panel.GetMinDimensions();
			return minDim;
		}


		public IUIBehavior AddUIControl()
		{
			switch (currentType)
			{
				case UIControlType.Text:
					return panel.AddUIControl(new LabelEx(textScriptObj));

				case UIControlType.InputField:
					return panel.AddUIControl(new InputFieldEx(inputFieldScriptObj));

				case UIControlType.Dropdown:
					return panel.AddUIControl(new DropdownEx(dropdownScriptObj));

				case UIControlType.CheckBox:
					return panel.AddUIControl(new CheckBoxEx(checkBoxScriptObj));

				case UIControlType.Slider:
					return panel.AddUIControl(new SliderEx(sliderScriptObj));

				case UIControlType.Button:
					return panel.AddUIControl(new ButtonEx(buttonScriptObj));

				case UIControlType.ButtonPanel:
					return panel.AddUIControl(new ButtonPanelEx(buttonPanelScriptObj));

				case UIControlType.Image:
					return panel.AddUIControl(new ImageEx(imageViewScriptObj));

				case UIControlType.ImagePanel:
					return panel.AddUIControl(new ImageViewDataEx(imageViewPanelScriptObj));

				default:
					Debug.LogException(new Exception($"{currentType} not yet implemented"));
					return null;
			}
		}


		public void ResetToLastPosition()
		{
			throw new System.NotImplementedException();
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




		public UIDesignObject Select()
		{
			throw new System.NotImplementedException();
		}

		public void SetHover(bool isHover)
		{
			throw new System.NotImplementedException();
		}

		public IUIDataEx GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			throw new System.NotImplementedException();
		}


		public void UpdateHover(Vector3 posOfHover)
		{
			throw new System.NotImplementedException();
		}
	}
}