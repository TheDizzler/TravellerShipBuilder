using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "CheckBoxData", menuName = "AtomosZ/UI/CheckBoxScriptableObject")]
	public class UICheckBoxScriptableObject : ScriptableObject
	{
		public Sprite boxSprite;
		public Sprite checkSprite;
		//public float fontSize = 36;
		//public Color fontColor = Color.black;
		//public TMP_FontAsset fontAsset;
		public UIExpandingLabelScriptableObject labelData;
	}
}