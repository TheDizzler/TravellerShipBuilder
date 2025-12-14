using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "checkBoxData", menuName = "AtomosZ/UIScriptableObjects/UICheckBoxScriptableObject")]
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