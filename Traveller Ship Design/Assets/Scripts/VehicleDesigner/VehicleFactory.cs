using System;
using System.Collections.Generic;

using AtomosZ.UI;

using TMPro;

using UnityEngine;


namespace AtomosZ.MG2eTraveller.Vehicle
{
	public class VehicleFactory : MonoBehaviour
	{
		[SerializeField] private UIInput uiInput;
		[SerializeField] private MagicWindow designWindow;
		[SerializeField] private MagicWindow dataSheetWindow;
		[SerializeField] private MagicWindow techTableWindow;

		[SerializeField] public UIExpandingLabelScriptableObject techTableScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject dataSheetLabelScriptObj;
		[SerializeField] public UIPanelScriptableObject horizontalPanelData;

		//private Chassis chassis;

		public static void SetChassisOptions(DropdownEx dropdown)
		{
			dropdown.options.Clear();
			foreach (ChassisType chassis in Enum.GetValues(typeof(ChassisType)))
			{
				dropdown.options.Add(new TMP_Dropdown.OptionData
				{
					text = chassis.ToString().Replace('_', ' '),
				});
			}
		}

		public List<TMP_Dropdown.OptionData> GetChassisOptions()
		{
			var list = new List<TMP_Dropdown.OptionData>();
			foreach (ChassisType chassis in Enum.GetValues(typeof(ChassisType)))
			{
				list.Add(new TMP_Dropdown.OptionData
				{
					text = chassis.ToString(),
				});
			}

			return list;
		}

		public void OnChassisChanged(UIDropdown dropdown, int selectionIndex)
		{
			var chassisDropdown = dropdown.GetComponent<TMP_Dropdown>();
			chassisDropdown.RefreshShownValue();

			HackFix();

			var uiDO = designWindow.GetControl("msg_textLabel");
			if (uiDO == null)
			{
				Debug.LogError("msg_textLabel is missing");
				return;
			}

			var label = (UIExpandingLabel)uiDO;

			if (!VehicleComponents.chassisList.TryGetValue((ChassisType)selectionIndex, out var chassis))
			{
				label.text = $"{(ChassisType)selectionIndex} has not yet been implemented";
				label.SetColor(Color.red);
				return;
			}

			label.SetColor(Color.white);
			label.text = $"{chassis.name}";

			var spacesSpinner = (UISpinner)designWindow.GetControl("spaces_spinner");
			spacesSpinner.minValue = (int)chassis.minSpace;
			spacesSpinner.maxValue = (int)chassis.maxSpace;

			var tlSlider = (UISlider)designWindow.GetControl("techLevel_slider");

			if (tlSlider == null)
			{
				Debug.LogError("techLevel_slider is missing");
				return;
			}

			tlSlider.interactable = true;
			tlSlider.minValue = chassis.techLevel;
			tlSlider.maxValue = 16;

			//var tabPanel = designWindow.AddTab();
		}


		public void OnTechLevelChanged(UISlider slider, float tl)
		{
			var tlLabel = (UIExpandingLabel)dataSheetWindow.GetControl("techLevel_label");
			tlLabel.text = tl + "";

			techTableWindow.ClearControls();

			var labelWidth = 100;
			var panel = (UIPanel)techTableWindow.AddUIControl(UIControlType.HorizontalPanel);

			var label = ((UIExpandingLabel)panel.AddUIControl(new LabelEx(techTableScriptObj)));
			label.fontStyles = FontStyles.Bold;
			((LabelEx)label.GetBackingData()).minLabelDimensions.x = labelWidth;
			label.text = "TL";

			label = ((UIExpandingLabel)panel.AddUIControl(new LabelEx(techTableScriptObj)));
			label.fontStyles = FontStyles.Bold;
			((LabelEx)label.GetBackingData()).minLabelDimensions.x = labelWidth;
			label.text = "SPEED";

			label = ((UIExpandingLabel)panel.AddUIControl(new LabelEx(techTableScriptObj)));
			label.fontStyles = FontStyles.Bold;
			label.alignmentOptions = TextAlignmentOptions.TopRight;
			((LabelEx)label.GetBackingData()).minLabelDimensions.x = labelWidth;
			label.text = "RANGE";

			var chassisDropdownCtrl = (UIDropdown)designWindow.GetControl("chassis_dropdown");

			if (!VehicleComponents.chassisList.TryGetValue((ChassisType)chassisDropdownCtrl.SelectedIndex, out var chassis))
			{
				label.text = $"{(ChassisType)chassisDropdownCtrl.SelectedIndex} has not yet been implemented";
				label.SetColor(Color.red);
				return;
			}

			foreach (var row in chassis.techTable.indices)
			{
				panel = (UIPanel)techTableWindow.AddUIControl(UIControlType.HorizontalPanel);

				var techRow = chassis.techTable[row];
				label = ((UIExpandingLabel)panel.AddUIControl(new LabelEx(techTableScriptObj)));
				((LabelEx)label.GetBackingData()).minLabelDimensions.x = labelWidth;
				var minLevel = techRow.techLevel;
				var maxLevel = minLevel;

				if (techRow == chassis.techTable.Last())
				{
					label.text = minLevel + "+";
				}
				else
				{
					while (chassis.techTable[maxLevel + 1] == techRow)
						++maxLevel;

					if (minLevel == maxLevel)
						label.text = minLevel.ToString();
					else
						label.text = minLevel + "-" + maxLevel;
				}

				label = ((UIExpandingLabel)panel.AddUIControl(new LabelEx(techTableScriptObj)));
				((LabelEx)label.GetBackingData()).minLabelDimensions.x = labelWidth;
				label.text = techRow.speed.ToString().Replace('_', ' ');

				label = ((UIExpandingLabel)panel.AddUIControl(new LabelEx(techTableScriptObj)));
				((LabelEx)label.GetBackingData()).minLabelDimensions.x = labelWidth;
				label.alignmentOptions = TextAlignmentOptions.TopRight;
				label.text = techRow.range + "";
			}

			var skillLabel = (UIExpandingLabel)dataSheetWindow.GetControl("skill_label");
			skillLabel.text = chassis.skill.ToString().Replace('_', ' ');

			var agiLabel = (UIExpandingLabel)dataSheetWindow.GetControl("agility_label");
			agiLabel.text = chassis.agility.ToString("+#;-#");


			var spacesLabel = (UIExpandingLabel)dataSheetWindow.GetControl("spaces_label");
			var spacesSpinner = (UISpinner)designWindow.GetControl("spaces_spinner");
			agiLabel.text = spacesSpinner.value.ToString();


			var techTableRow = chassis.techTable[(uint)tl];
			var speedLabel = (UIExpandingLabel)dataSheetWindow.GetControl("speed_label");
			speedLabel.text = techTableRow.speed.ToString();

			var rangeLabel = (UIExpandingLabel)dataSheetWindow.GetControl("range_label");
			rangeLabel.text = techTableRow.range.ToString();

			techTableWindow.Refresh();
		}

		void HackFix()
		{
			var ctrls = dataSheetWindow.GetComponentsInChildren<UIExpandingLabel>();
			foreach (var ctrl in ctrls)
			{
				var bd = ((LabelEx)ctrl.GetBackingData());
				bd.scriptableObj = dataSheetLabelScriptObj;
				bd.useCustomFontAsset = bd.useCustomFontColor = bd.useCustomFontSize = false;
				ctrl.minLabelDimensions = new Vector2(120, 10);
				ctrl.alignmentOptions = TextAlignmentOptions.Left;
			}

			var panels = dataSheetWindow.GetComponentsInChildren<UIPanel>();
			foreach (var panel in panels)
			{
				if (!panel.IsHorizontal())
					continue;
				
				var bd = ((PanelEx)panel.GetBackingData());
				bd.scriptableObj = horizontalPanelData;
				bd.useCustomMinDimensions = false;
			}

			dataSheetWindow.Refresh();
		}
	}
}