using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "ButtonData", menuName = "AtomosZ/UI/ButtonScriptableObject")]
	public class UIButtonScriptableObject : ScriptableObject
	{
		public Sprite sprite;
		[Tooltip("If true, sprite is used as a 9-slice. False for a single image button with no text.")]
		public bool spriteIsBackground = true;
		public bool noText;
		public UIExpandingLabelScriptableObject labelData;
	}
}