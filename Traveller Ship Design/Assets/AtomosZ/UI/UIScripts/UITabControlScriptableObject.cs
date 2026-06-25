using TMPro;
using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "TabControlData", menuName = "AtomosZ/UI/TabControlScriptableObject")]
	public class UITabControlScriptableObject : MagicUIScriptableObject
	{
		[Tooltip("Sometimes the tab sprites need to be different depending on the position."
			+ "\n[0]: first tab in control.\n[1]: all tabs between [0] and [count -1] (exclusive).\n[2]: last tab")]
		public Sprite[] tabSprites;

		public Color selectedTabColor = new Color(1, 1, 1, 1);
		public Color deselectedTabColor = new Color(.5f, .5f, .5f, 1);
		[Tooltip("@TODO(Tristan): this")]
		public Color disabledTabColor = new Color(1, 1, 1, 1);

		[Tooltip("Offset applied to distance between tabs. Neg to move left (closer together), pos to move right (further apart).")]
		public float tabHorizontaloffset = -36;
	}
}
