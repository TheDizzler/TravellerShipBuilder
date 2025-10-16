using System;
using System.Collections.Generic;

using AtomosZ.UI;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.MG2eTraveller.Vehicle
{
	public class VehicleFactory : MonoBehaviour
	{
		[SerializeField] private UIInput uiInput;
		[SerializeField] private MagicWindow designWindow;


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

		public void OnChassisChanged(int selectionIndex)
		{
			var chassisDropdownCtrl = designWindow.GetControl("chassis_dropdown");
			if (chassisDropdownCtrl == null)
			{
				Debug.LogError("chassis_dropdown is missing");
				return;
			}

			var chassisDropdown = ((UIDropdown)chassisDropdownCtrl).GetComponent<TMP_Dropdown>();
			chassisDropdown.RefreshShownValue();

			bool recalc = false;
			if (Application.isPlaying)
				recalc = true;
			var uiDO = designWindow.GetControl("msg_textLabel");
			if (uiDO == null)
			{
				Debug.LogError("msg_textLabel is missing");
				return;
			}

			var label = (UIExpandingLabel)uiDO;

			if (!VehicleComponents.chassisList.TryGetValue((ChassisType)selectionIndex, out Chassis chassis))
			{
				label.SetText($"{(ChassisType)selectionIndex} has not yet been implemented", recalc);
				label.SetColor(Color.red);
				return;
			}

			label.SetColor(Color.white);
			label.SetText($"{chassis.name}", recalc);

			var slider = (UISlider)designWindow.GetControl("techLevel_slider");

			if (slider == null)
			{
				Debug.LogError("techLevel_slider is missing");
				return;
			}

			slider.min = chassis.techLevel;
			slider.max = 16;

			//var tabPanel = designWindow.AddTab();
		}
	}
}