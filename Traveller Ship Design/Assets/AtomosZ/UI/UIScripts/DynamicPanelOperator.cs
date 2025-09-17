using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

namespace AtomosZ.UI
{
#if DEBUG
	/// <summary>
	/// IMPORTANT(Tristan): Needed for two different PropertyDrawer views.
	/// </summary>
	[Serializable]	
	[Obsolete()]
	public class CreatePanelControl : PanelControl_dep { }

	
	[Obsolete()]
	[Serializable]
	public class PanelControl_dep
	{
		public UIControlType controlType;
		public static Dictionary<UIControlType, string> panelControlNames = new()
		{
			[UIControlType.Text] = "labelEx",
			[UIControlType.InputField] = "inputFieldEx",
			[UIControlType.CheckBox] = "checkBoxEx",
			[UIControlType.Slider] = "sliderEx",
			[UIControlType.ButtonPanel] = "buttonPanelEx",
			[UIControlType.Button] = "buttonEx",
			[UIControlType.Image] = "imageEx",
			[UIControlType.ImagePanel] = "imagePanelEx",
			[UIControlType.Dropdown] = "dropdownEx",
		};

		public LabelEx labelEx;
		public InputFieldEx inputFieldEx;
		public CheckBoxEx checkBoxEx;
		public SliderEx sliderEx;
		public ButtonEx buttonEx;
		public ImageEx imageEx;
		public ImageViewDataEx imagePanelEx;
		public ButtonPanelEx buttonPanelEx;
		public DropdownEx dropdownEx;

		public UIDesignObject uiDesignObject;

		public List<IUIDataEx> GetAllControls()
		{
			var list = new List<IUIDataEx>();
			foreach (var controlName in panelControlNames)
			{
				var field = typeof(PanelControl_dep).GetField(controlName.Value);
				var control = field.GetValue(this);
				list.Add((IUIDataEx)control);
			}

			return list;
		}

		public Dictionary<UIControlType, IUIDataEx> GetAllControlsByType()
		{
			var dict = new Dictionary<UIControlType, IUIDataEx>();
			var allControls = GetAllControls();
			foreach (var data in allControls)
			{
				dict.Add(data.dataType, data);
			}

			return dict;
		}

		public IUIDataEx GetData()
		{
			var field = typeof(PanelControl_dep).GetField(panelControlNames[controlType]);
			var control = field.GetValue(this);
			return (IUIDataEx)control;
		}
	}
#endif

	/// <summary>
	/// A monobehaviour only used in the editor for building dynamic dialog boxes.
	/// This monobehaviour self-destructs on Start().
	/// </summary>
	[RequireComponent(typeof(DynamicPanel))]
	
	[Obsolete("Been replaced with DyanPanelOp")]
	public class DynamicPanelOperator : MonoBehaviour
	{
		[SerializeField] private CreatePanelControl createPanelControl;
		[SerializeField] public List<PanelControl_dep> panelControls;


		void Start()
		{
#if !EDITOR
			Destroy(this);
#endif
		}

		[Obsolete("No need")]
		[Conditional("DEBUG")]
		public void ResetToLabelDefaults()
		{
			var allControls = createPanelControl.GetAllControls();
			//foreach (IUIDataEx controlEx in allControls)
			//	controlEx.ResetToDefaults();
		}

		[Conditional("DEBUG")]
		public void Refresh()
		{
			var panel = GetComponent<DynamicPanel>();
			panel.Refresh();
		}

		[Conditional("DEBUG")]
		public void RecalculateDimensions()
		{
			var panel = GetComponent<DynamicPanel>();
			panel.RecalculateDimensions();
		}

		[Obsolete("No need")]
		[Conditional("DEBUG")]
		public void AddControl()
		{
			var panel = GetComponent<DynamicPanel>();
			//var controlDataEx = (IUIDataEx)createPanelControl.GetAllControlsByType()[createPanelControl.controlType].Clone();
			//panelRect.AddUIControl(controlDataEx);
		}

		[Conditional("DEBUG")]
		public void Remove(UIDesignObject uiDO)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.RemoveControl(uiDO);
		}

		[Conditional("DEBUG")]
		public void Remove(IUIDataEx data)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.RemoveControl(data);
		}

		[Obsolete("No need")]
		[Conditional("DEBUG")]
		public void ResetToDefaults(PanelControl_dep panelControl)
		{
			var allControls = panelControl.GetAllControlsByType();
			//allControls[panelControl.controlType].ResetToDefaults();
			panelControl.uiDesignObject.UpdateBackingData(allControls[panelControl.controlType]);

			var panel = GetComponent<DynamicPanel>();
			panel.RecalculateDimensions();
		}

		[Conditional("DEBUG")]
		public void RemoveControl(PanelControl_dep control)
		{
			var panel = GetComponent<DynamicPanel>();
			foreach (var ctrl in panelControls)
			{
				if (ctrl.uiDesignObject == control.uiDesignObject)
				{
					panelControls.Remove(ctrl);
					panel.RemoveControl(ctrl.uiDesignObject);
					break;
				}
			}
		}

		[Conditional("DEBUG")]
		public void RemoveControl(IUIDataEx data)
		{
			//panelControls.Remove(data);
			var panel = GetComponent<DynamicPanel>();
			panel.RemoveControl(data);
		}

		[Conditional("DEBUG")]
		public void ClearAllUIControls()
		{
			var panel = GetComponent<DynamicPanel>();
			panel.ClearControlsEditor();
		}

		[Conditional("DEBUG")]
		public void ChangeMaxDims(Vector2 maxDims)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.UpdateMaxDimensions(maxDims);
		}

		[Conditional("DEBUG")]
		public void SetAlwaysShrink(bool alwaysShrink)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.SetAlwaysShrink(alwaysShrink);
		}

		[Conditional("DEBUG")]
		public void ChangeTitleStyle(DynamicPanel.TitleLabelStyle titleStyle)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.SetTitleStyle(titleStyle);
		}

		[Conditional("DEBUG")]
		public void SetTitleText(string titleText)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.SetTitleText(titleText);
		}

		[Conditional("DEBUG")]
		public void SetPanelStyle(DynamicPanel.BottomPanelStyle panelStyle)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.SetPanelStyle(panelStyle);
		}

		[Conditional("DEBUG")]
		public void ToggleCloseButton(bool showCloseButton)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.ToggleCloseButton(showCloseButton);
		}

		[Conditional("DEBUG")]
		public void ToggleMinButton(bool showMinButton)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.ToggleMinimizeButton(showMinButton);
		}

		[Conditional("DEBUG")]
		public void ToggleMaxButton(bool showMaxButton)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.ToggleMaximizeButton(showMaxButton);
		}
	}
}