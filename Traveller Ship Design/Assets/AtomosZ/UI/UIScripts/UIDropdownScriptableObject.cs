using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "dropdownData", menuName = "AtomosZ/UIScriptableObjects/UIDropdownScriptableObject")]
	public class UIDropdownScriptableObject : ScriptableObject
	{
		public UIExpandingLabelScriptableObject labelData;
		public Sprite arrowSprite;
		//[Tooltip("Default: 18")]
		//public float fontSize = 18;
		//public Color placeholderFontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 128.0f / 256);
		//public Color fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
		//public TMP_FontAsset fontAsset;
	}
}
