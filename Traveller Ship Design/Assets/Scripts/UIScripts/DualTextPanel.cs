using AtomosZ.UI;
using UnityEngine;

namespace AtomosZ.MG2eTraveller
{
	public class DualTextPanel : MonoBehaviour
	{
		public UIExpandingLabel leftLabel;
		public UIExpandingLabel rightLabel;

		public void SetText(string staticText, string valueText)
		{
			leftLabel.text = staticText;
			rightLabel.text = valueText;
		}
	}
}