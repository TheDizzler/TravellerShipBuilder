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
		[SerializeField] private DynamicPanel chassisPanel;


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
			var dropdown = chassisPanel.GetControl("chassis_dropdown").GetComponent<TMP_Dropdown>();
			dropdown.RefreshShownValue();

			bool recalc = false;
			if (Application.isPlaying)
				recalc = true;
			var uiDO = chassisPanel.GetControl("msg_textLabel");
			var label = uiDO.GetComponent<UIExpandingLabel>();

			if (!VehicleComponents.chassisList.TryGetValue((ChassisType)selectionIndex, out Chassis chassis))
			{
				label.SetText($"{(ChassisType)selectionIndex} has not yet been implemented", recalc);
				label.SetColor(Color.red);
				return;
			}

			label.SetColor(Color.white);
			label.SetText($"{chassis.name}", recalc);
		}

	}
}