using System;
using System.Collections.Generic;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// Make sure your implementing class has [Serializable] !
	/// </summary>
	public interface IUIDataEx
	{
		public UIControlType dataType { get; }
	}

	public static class IUDataExExtensions
	{
		public static string GetReferenceName(this IUIDataEx dataEx)
		{
			switch (dataEx.dataType)
			{
				case UIControlType.Button:
					return ((ButtonEx)dataEx).referenceName;
				case UIControlType.ButtonPanel:
					return ((ButtonPanelEx)dataEx).referenceName;
				case UIControlType.CheckBox:
					return ((CheckBoxEx)dataEx).referenceName;
				case UIControlType.Dropdown:
					return ((DropdownEx)dataEx).referenceName;
				case UIControlType.Image:
					return ((ImageEx)dataEx).referenceName;
				case UIControlType.ImagePanel:
					return ((ImageViewDataEx)dataEx).referenceName;
				case UIControlType.InputField:
					return ((InputFieldEx)dataEx).referenceName;
				case UIControlType.Slider:
					return ((SliderEx)dataEx).referenceName;
				case UIControlType.Text:
					return ((LabelEx)dataEx).referenceName;
				case UIControlType.Panel:
					return ((PanelEx)dataEx).referenceName;
				//case UIControlType.Spinner:
				//	return ((SpinnerEx)dataEx).referenceName;
				//case UIControlType.TabControl:
				//	return ((TabControlEx)dataEx).referenceName;

				default:
					throw new Exception($"{dataEx.dataType} not yet implemented");
			}
		}
	}

	public enum UIControlType
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
		TabControl,
		Panel,
		Spinner,
	}

#if DEBUG
	/// <summary>
	/// This purely and Editor class for creating and manipulating UI controls.
	/// </summary>
	[Serializable]
	public class UIControl
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
			[UIControlType.Panel] = "panelEx",
		};

		[SerializeReference] public LabelEx labelEx;
		[SerializeReference] public InputFieldEx inputFieldEx;
		[SerializeReference] public CheckBoxEx checkBoxEx;
		[SerializeReference] public SliderEx sliderEx;
		[SerializeReference] public ButtonEx buttonEx;
		[SerializeReference] public ImageEx imageEx;
		[SerializeReference] public ImageViewDataEx imagePanelEx;
		[SerializeReference] public ButtonPanelEx buttonPanelEx;
		[SerializeReference] public DropdownEx dropdownEx;
		[SerializeReference] public PanelEx panelEx;

		public IUIDataEx GetData()
		{
			var field = typeof(UIControl).GetField(UIControl.panelControlNames[controlType]);
			var control = (IUIDataEx)field.GetValue(this);
			return control;
		}
	}
#endif

	/// <summary>
	/// This will not keep any (non-editor) data.
	/// This will completely be a utility class for the inspector that will self-destruct on Start().
	/// Maybe this will keep a list of all the objects that need PrefabUtility.RecordPrefabInstancePropertyModifications() applied to them.<br/>
	/// This Monobehavior self-destructs at Start(). 
	/// </summary>
	[ExecuteAlways]
	public class DynaPanelOp : MonoBehaviour
	{
		public DynamicPanel dynaPan;
		public UIPrefabProvider uiProvider;
		[SerializeField] public UIControlType currentType;
		[SerializeField] public List<UIControl> uiControls = new();

		[SerializeField] public UIExpandingLabelScriptableObject textScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject dropdownScriptObj;
		[SerializeField] public UICheckBoxScriptableObject checkBoxScriptObj;
		[SerializeField] public UIExpandingInputFieldScriptableObject inputFieldScriptObj;
		[SerializeField] public UISliderScriptableObject sliderScriptObj;
		[SerializeField] public UIButtonScriptableObject buttonScriptObj;
		[SerializeField] public UIButtonPanelScriptableObject buttonPanelScriptObj;
		[SerializeField] public UIImageViewScriptableObject imageViewScriptObj;
		[SerializeField] public UIImageViewPanelScriptableObject imageViewPanelScriptObj;

		//#if UNITY_EDITOR
		//		readonly Dictionary<UIControlType, System.Reflection.MethodInfo> functions = new()
		//		{
		//			[UIControlType.Text] = typeof(DynamicPanel).GetMethod("AddText"),
		//			[UIControlType.InputField] = typeof(DynamicPanel).GetMethod("AddInputField"),
		//			[UIControlType.CheckBox] = typeof(DynamicPanel).GetMethod("AddCheckBox"),
		//			[UIControlType.Slider] = typeof(DynamicPanel).GetMethod("AddSlider"),
		//			[UIControlType.Button] = typeof(DynamicPanel).GetMethod("AddButton"),
		//			[UIControlType.ButtonPanel] = typeof(DynamicPanel).GetMethod("AddButtonPanel"),
		//			[UIControlType.Image] = typeof(DynamicPanel).GetMethod("AddImage"),
		//			[UIControlType.ImagePanel] = typeof(DynamicPanel).GetMethod("AddImagePanel"),
		//			[UIControlType.Dropdown] = typeof(DynamicPanel).GetMethod("AddDropdown"),
		//		};


		public IUIBehavior AddUIControl()
		{
			switch (currentType)
			{
				case UIControlType.Text:
					return dynaPan.AddUIControl(new LabelEx(textScriptObj));

				case UIControlType.InputField:
					return dynaPan.AddUIControl(new InputFieldEx(inputFieldScriptObj));

				case UIControlType.Dropdown:
					return dynaPan.AddUIControl(new DropdownEx(dropdownScriptObj));

				case UIControlType.CheckBox:
					return dynaPan.AddUIControl(new CheckBoxEx(checkBoxScriptObj));

				case UIControlType.Slider:
					return dynaPan.AddUIControl(new SliderEx(sliderScriptObj));

				case UIControlType.Button:
					return dynaPan.AddUIControl(new ButtonEx(buttonScriptObj));

				case UIControlType.ButtonPanel:
					return dynaPan.AddUIControl(new ButtonPanelEx(buttonPanelScriptObj));

				case UIControlType.Image:
					return dynaPan.AddUIControl(new ImageEx(imageViewScriptObj));

				case UIControlType.ImagePanel:
					return dynaPan.AddUIControl(new ImageViewDataEx(imageViewPanelScriptObj));

				default:
					Debug.LogException(new Exception($"{currentType} not yet implemented"));
					return null;
			}
		}


		void Start()
		{
#if !DEBUG
			Destroy(this);
#endif

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
			dynaPan.RemoveControl(uiData);
		}
	}
}