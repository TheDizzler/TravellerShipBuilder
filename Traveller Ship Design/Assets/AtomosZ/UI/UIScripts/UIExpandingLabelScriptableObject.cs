using System;

using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UITextScriptableObject", order = 1)]
	public class UIExpandingLabelScriptableObject : ScriptableObject
	{
		[Tooltip("Default: 36")]
		public float fontSize = 36;
		public Color fontColor = Color.white;
		public Color disabledColor = new Color(0xA1 / 255, 0xA1 / 255, 0xA1 / 255);
		public TMP_FontAsset fontAsset;
	}
}