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
		[SerializeField] private DynamicPanel chassisPanel;

		void Start()
		{
			//chassisPanel.AddText(new LabelEx
			//{
			//	text = "Select Chassis Type:",
			//	fontSize = 36,
			//});

		}

		public static void SetChassisOptions(DropdownEx dropdown)
		{
			dropdown.options.Clear();
			foreach (ChasisType chassis in Enum.GetValues(typeof(ChasisType)))
			{
				dropdown.options.Add(new TMP_Dropdown.OptionData
				{
					text = chassis.ToString(),
				});
			}
		}

		public List<TMP_Dropdown.OptionData> GetChassisOptions()
		{
			var list = new List<TMP_Dropdown.OptionData>();
			foreach (ChasisType chassis in Enum.GetValues(typeof(ChasisType)))
			{
				list.Add(new TMP_Dropdown.OptionData
				{
					text = chassis.ToString(),
				});
			}

			return list;
		}

	}
}