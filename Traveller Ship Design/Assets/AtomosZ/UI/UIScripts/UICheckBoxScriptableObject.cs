using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UICheckBoxScriptableObject")]
	public class UICheckBoxScriptableObject : ScriptableObject
	{
		public LabelEx labelEx;
		//[Tooltip("Default: 36")]
		//public float fontSize = 36;
		//public Color fontColor = Color.black;
		//public TMP_FontAsset fontAsset;

		public Sprite boxSprite;
		public Sprite checkSprite;
	}
}