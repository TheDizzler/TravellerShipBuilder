using System;
using System.Collections.Generic;
using AtomosZ.UI;
using TMPro;
using UnityEngine;
using static AtomosZ.UI.MagicWindowBase;


namespace AtomosZ.MG2eTraveller.Vehicle
{
	public class VehicleFactory : MonoBehaviour
	{
		[SerializeField] private UIInput uiInput;
		[SerializeField] private MagicTabbedWindow designWindow;
		[SerializeField] private MagicWindow dataSheetWindow;
		[SerializeField] private MagicWindow techTableWindow;


		[SerializeField] public OptionPanel optionPanelPrefab;

		[SerializeField] public UIExpandingLabelScriptableObject techTableScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject dataSheetLabelScriptObj;
		[SerializeField] public UIPanelScriptableObject horizontalPanelData;


		public static void SetChassisOptions(UIDropdown dropdown)
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

			var msgLabel = (UIExpandingLabel)designWindow.GetControl("msg_textLabel");
			if (msgLabel == null)
			{
				Debug.LogError("msg_textLabel is missing");
				return;
			}

			var spacesSpinner = (UISpinner)designWindow.GetControl("spaces_spinner");


			var chassis = GetSelectedChassis();
			if (chassis == null)
			{
				msgLabel.text = $"{(ChassisType)selectionIndex} has not yet been implemented";
				msgLabel.color = Color.red;
				techTableWindow.ClearControls();

				spacesSpinner.interactable = false;
				designWindow.EnableTab("options_tab", false);
				return;
			}

			spacesSpinner.interactable = true;

			msgLabel.color = Color.white;
			msgLabel.text = $"{chassis.name}";

			spacesSpinner.minValue = (int)chassis.minSpace;
			spacesSpinner.maxValue = (int)chassis.maxSpace;

			var tlSlider = (UISlider)designWindow.GetControl("techLevel_slider");

			if (tlSlider == null)
			{
				Debug.LogError("techLevel_slider is missing");
				return;
			}

			tlSlider.interactable = true;
			var evnt = tlSlider.onValueChanged;
			tlSlider.onValueChanged = null;
			var orgValue = tlSlider.value;
			tlSlider.maxValue = 16;
			tlSlider.minValue = -1;
			tlSlider.value = 0;
			tlSlider.onValueChanged = evnt;
			tlSlider.minValue = chassis.techLevel;
			tlSlider.value = orgValue;

			var optionsPanel = (UIPanel)designWindow.GetControl("options_panel");
			if (optionsPanel == null)
			{
				var tabPanel = designWindow.AddTab("Options");
				optionsPanel = tabPanel.panel;
				optionsPanel.referenceName = "options_panel";
				var tab = tabPanel.tabItem;
				tab.referenceName = "options_tab";
			}

			UpdateOptionsPanel();
		}

		private void UpdateOptionsPanel()
		{
			var optionsPanel = (UIPanel)designWindow.GetControl("options_panel");
			optionsPanel.ClearControls();
			var chassis = GetSelectedChassis();

			foreach (var option in chassis.options)
			{
				var optionPanel = Instantiate(optionPanelPrefab, transform);
				optionPanel.SetOption(option);
				optionsPanel.AddCustomControl(optionPanel.GetComponent<UIMonoBehaviour>());
			}
		}

		public void OnTechLevelChanged(UISlider slider, float techLevel)
		{
			dataSheetWindow.gameObject.SetActive(true);
			techTableWindow.gameObject.SetActive(true);

			var tlLabel = (UIExpandingLabel)dataSheetWindow.GetControl("techLevel_label");
			tlLabel.text = techLevel + "";

			techTableWindow.ClearControls();

			var labelWidth = 100;
			var techRowPanel = (UIPanel)techTableWindow.AddUIControl(UIControlType.HorizontalPanel);
			techRowPanel.referenceName = "techTableHeader_panel";

			var label = ((UIExpandingLabel)techRowPanel.AddText(techTableScriptObj));
			label.referenceName = techRowPanel.referenceName + "_tl_label";
			label.fontStyles = FontStyles.Bold;
			var labelDimen = label.minDimensions;
			labelDimen.x = labelWidth;
			label.minDimensions = labelDimen;
			label.text = "TL";

			label = ((UIExpandingLabel)techRowPanel.AddText(techTableScriptObj));
			label.referenceName = techRowPanel.referenceName + "_speed_label";
			label.fontStyles = FontStyles.Bold;
			label.minDimensions = labelDimen;
			label.text = "SPEED";

			label = ((UIExpandingLabel)techRowPanel.AddText(techTableScriptObj));
			label.referenceName = techRowPanel.referenceName + "_range_label";
			label.fontStyles = FontStyles.Bold;
			label.alignmentOptions = TextAlignmentOptions.TopRight;
			label.minDimensions = labelDimen;
			label.text = "RANGE";


			var chassis = GetSelectedChassis();
			if (chassis == null)
				return;

			foreach (var row in chassis.techTable.indices)
			{
				techRowPanel = (UIPanel)techTableWindow.AddUIControl(UIControlType.HorizontalPanel);
				techRowPanel.referenceName = "techTable_dataRow_panel_row_" + row;

				var techRow = chassis.techTable[row];
				label = ((UIExpandingLabel)techRowPanel.AddText(techTableScriptObj));
				label.minDimensions = labelDimen;
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

				label = ((UIExpandingLabel)techRowPanel.AddText(techTableScriptObj));
				label.referenceName = techRowPanel.referenceName + "_speed_label_row_" + row;
				label.minDimensions = labelDimen;
				label.text = techRow.speed.ToString().Replace('_', ' ');

				label = ((UIExpandingLabel)techRowPanel.AddText(techTableScriptObj));
				label.minDimensions = labelDimen;
				label.alignmentOptions = TextAlignmentOptions.TopRight;
				label.text = techRow.range + "";
			}

			techTableWindow.GetDrawnSize();

			/// Update DataSheet
			var skillLabel = (UIExpandingLabel)dataSheetWindow.GetControl("skill_label");
			skillLabel.text = chassis.skill.ToString().Replace('_', ' ');

			var agiLabel = (UIExpandingLabel)dataSheetWindow.GetControl("agility_label");
			agiLabel.text = chassis.agility.ToString("+#;-#;+0");


			var spacesLabel = (UIExpandingLabel)dataSheetWindow.GetControl("spaces_label");
			var spacesSpinner = (UISpinner)designWindow.GetControl("spaces_spinner");
			spacesLabel.text = spacesSpinner.value.ToString();


			var techTableRow = chassis.techTable[(uint)techLevel];
			var speedLabel = (UIExpandingLabel)dataSheetWindow.GetControl("speed_label");
			speedLabel.text = techTableRow.speed.ToString().Replace('_', ' ');

			var rangeLabel = (UIExpandingLabel)dataSheetWindow.GetControl("range_label");
			rangeLabel.text = techTableRow.range.ToString();
		}

		private Chassis GetSelectedChassis()
		{
			var chassisDropdownCtrl = (UIDropdown)designWindow.GetControl("chassis_dropdown");
			if (!VehicleComponents.chassisList.TryGetValue((ChassisType)chassisDropdownCtrl.SelectedIndex(), out var chassis))
			{
				return null;
			}

			return chassis;
		}

		public void OnSpacesChanged(UISpinner spinner, int spaceCount)
		{
			var costLabel = (UIExpandingLabel)dataSheetWindow.GetControl("cost_label");
			var shippingLabel = (UIExpandingLabel)dataSheetWindow.GetControl("shipping_label");
			var hullLabel = (UIExpandingLabel)dataSheetWindow.GetControl("hull_label");
			var spacesLabel = (UIExpandingLabel)dataSheetWindow.GetControl("spaces_label");

			var chassis = GetSelectedChassis();
			spacesLabel.text = spaceCount.ToString();
			costLabel.text = "Cr" + (chassis.costPerSpace * spaceCount);
			shippingLabel.text = (chassis.shippingTonsPerSpace * spaceCount) + " tons";
			hullLabel.text = chassis.hullPerSpace * spaceCount + "";
		}




		void HackFix()
		{
			var ctrls = dataSheetWindow.GetComponentsInChildren<UIExpandingLabel>();
			foreach (var ctrl in ctrls)
			{
				ctrl.UpdateBackingData(dataSheetLabelScriptObj);
				ctrl.minDimensions = new Vector2(120, 10);
				ctrl.alignmentOptions = TextAlignmentOptions.Left;
			}

			var panels = dataSheetWindow.GetComponentsInChildren<UIPanel>();
			foreach (var panel in panels)
			{
				if (!panel.IsHorizontal())
					continue;
				panel.UpdateBackingData(horizontalPanelData);
			}

			dataSheetWindow.RecalculateDimensions();
		}
	}
}