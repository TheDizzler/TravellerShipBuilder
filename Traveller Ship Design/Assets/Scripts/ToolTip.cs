using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ToolTip : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI hintText;

	void Start()
	{
		hintText.text = "";
	}

	public void SetToolTip(List<string> toolTips)
	{
		var text = "";
		foreach (var tip in toolTips)
		{
			text += "•" + tip + "\n";
		}

		hintText.text = text.Trim();
	}
}
