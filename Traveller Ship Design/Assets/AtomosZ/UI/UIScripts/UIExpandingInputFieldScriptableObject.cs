using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "inputFieldData", menuName = "AtomosZ/UIScriptableObjects/UIInputFieldScriptableObject")]
	public class UIExpandingInputFieldScriptableObject : ScriptableObject
	{
		[Tooltip("Default: 18")]
		public float fontSize = 18;
		public Color placeholderFontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 128.0f / 256);
		public Color fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
		public TMP_FontAsset fontAsset;
	}
}