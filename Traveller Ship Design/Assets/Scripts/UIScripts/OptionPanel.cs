using System;
using AtomosZ.MG2eTraveller.Vehicle;
using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.MG2eTraveller
{
	public class OptionPanel : MonoBehaviour
	{
		[SerializeField] private DualTextPanel textPanelPrefab;

		[SerializeField] private UIExpandingLabel nameLabel;
		[SerializeField] private UIExpandingLabel descLabel;
		[SerializeField] private UICheckBox variantCheckbox;
		[SerializeField] private RectTransform modsPanel;

		public void SetOption(Option option)
		{
			nameLabel.text = option.name;
			if (string.IsNullOrEmpty(option.description))
				descLabel.text = option.description;
			foreach (var optionMod in option.optionModValues)
			{
				var dataPanel = Instantiate(textPanelPrefab, modsPanel);
				string label = optionMod.Key.ToString().Replace('_', ' ');
				string value = null;

				switch (optionMod.Key)
				{
					case OptionModType.MultiMod:
					{
						var multiMod = (MultiMod)optionMod.Value;
						variantCheckbox.gameObject.SetActive(true);
						variantCheckbox.text = multiMod.altOptionName;
						//variantCheckbox.AddListener(
						foreach (var mod in multiMod.altOptionModdedValues)
						{
							//switch (mod.
						}
					}
					break;

					case OptionModType.MinTechLevel:
					{
						label = "Tech Level";
						value = ((int)optionMod.Value).ToString();
					}
					break;

					case OptionModType.Skill:
					{
						Skill skill = ((Skill)optionMod.Value);
						value = skill.ToString().Replace('_', ' ');
					}
					break;

					case OptionModType.Agility:
					{
						var agi = ((int)optionMod.Value);
						if (agi >= 0)
							value = $"+{agi}";
						else
							value = $"{agi}";

					}
					break;

					case OptionModType.Spaces:
					{
						var intValue = ((int)optionMod.Value);
						int loByte = intValue & 0x00FF;
						int hiByte = (intValue & 0xFF00) >> 8;
						value = $"{loByte}-{hiByte}";
						//if (split.Length != 2)
						//{
						//	value = "INVALID SPACES FORMAT";
						//	Debug.LogError(value);
						//}
						//else
						//	value = $"{split[0]}-{split[1]}";
					}
					break;

					case OptionModType.CostPerSpace:
					{
						var cost = (int)optionMod.Value;

						if (cost >= 0)
							value = $"+Cr{cost}";
						else
							value = $"-Cr{Mathf.Abs(cost)}";
					}
					break;

					case OptionModType.SpeedBandAdjust:
					{
						label = "Tech Table";
						var inc = (int)optionMod.Value;
						if (inc > 0)
							value = $"Increase Speed by {inc} band";
						else
							value = $"Decrease Speed by {inc} band";
					}
					break;

					//case OptionModType.:
					//{
					//	value = ;
					//}
					//break;

					default:
						value = "NOT YET IMPLEMENTED";
						break;
				}

				dataPanel.SetText(label + ":", value);
			}
		}
	}
}