using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// Make sure your implementing class has [Serializable] !
	/// </summary>
	public interface IUIDataEx : ICloneable
	{
		public PanelControlType dataType { get; }
		public void ResetToDefaults();
	}

	public enum PanelControlType
	{
		Text,
		InputField,
		CheckBox,
		Slider,
		Button,
		ButtonPanel,
		Image,
		ImagePanel,
		Dropdown,
	}

	/// <summary>
	/// IMPORTANT(Tristan): Needed for two different PropertyDrawer views.
	/// </summary>
	[Serializable]
	public class CreatePanelControl : PanelControl
	{

	}


	[Serializable]
	public class PanelControl
	{
		public PanelControlType controlType;
		public static Dictionary<PanelControlType, string> panelControlNames = new()
		{
			[PanelControlType.Text] = "labelEx",
			[PanelControlType.InputField] = "inputFieldEx",
			[PanelControlType.CheckBox] = "checkBoxEx",
			[PanelControlType.Slider] = "sliderEx",
			[PanelControlType.ButtonPanel] = "buttonPanelEx",
			[PanelControlType.Button] = "buttonEx",
			[PanelControlType.Image] = "imageEx",
			[PanelControlType.ImagePanel] = "imagePanelEx",
			[PanelControlType.Dropdown] = "dropdownEx",
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
				var field = typeof(PanelControl).GetField(controlName.Value);
				var control = field.GetValue(this);
				list.Add((IUIDataEx)control);
			}

			return list;
		}

		public Dictionary<PanelControlType, IUIDataEx> GetAllControlsByType()
		{
			var dict = new Dictionary<PanelControlType, IUIDataEx>();
			var allControls = GetAllControls();
			foreach (var data in allControls)
			{
				dict.Add(data.dataType, data);
			}

			return dict;
		}
	}

	/// <summary>
	/// A monobehaviour only used in the editor for building dynamic dialog boxes.
	/// This monobehaviour self-destructs on Start().
	/// </summary>
	[RequireComponent(typeof(DynamicPanel))]
	public class DynamicPanelOperator : MonoBehaviour
	{
		[SerializeField] private CreatePanelControl createPanelControl;
		[SerializeField] public List<PanelControl> panelControls;


		void Start()
		{
#if !EDITOR
			Destroy(this);
#endif
		}


		[Conditional("DEBUG")]
		public void ResetToLabelDefaults()
		{
			var allControls = createPanelControl.GetAllControls();
			foreach (IUIDataEx controlEx in allControls)
				controlEx.ResetToDefaults();
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

		[Conditional("DEBUG")]
		public void AddControl()
		{
			var panel = GetComponent<DynamicPanel>();
			panel.AddControl(createPanelControl);
		}

		[Conditional("DEBUG")]
		public void Remove(UIDesignObject uiDO)
		{
			var panel = GetComponent<DynamicPanel>();
			panel.RemoveControl(uiDO);
		}

		[Conditional("DEBUG")]
		public void ResetToDefaults(PanelControl panelControl)
		{
			var allControls = panelControl.GetAllControlsByType();
			allControls[panelControl.controlType].ResetToDefaults();
			panelControl.uiDesignObject.UpdateBackingData(allControls[panelControl.controlType]);

			var panel = GetComponent<DynamicPanel>();
			panel.RecalculateDimensions();
		}

		[Conditional("DEBUG")]
		public void RemoveControl(PanelControl control)
		{
			panelControls.Remove(control);
			var panel = GetComponent<DynamicPanel>();
			panel.RemoveControl(control.uiDesignObject);
		}

		[Conditional("DEBUG")]
		public void Clear()
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