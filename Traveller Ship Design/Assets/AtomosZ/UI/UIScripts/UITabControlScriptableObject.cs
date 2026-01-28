using TMPro;
using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "TabControlData", menuName = "AtomosZ/UI/TabControlScriptableObject")]
	public class UITabControlScriptableObject : ScriptableObject
	{
		[Tooltip("The UIPanelScriptableObject that new child panels will inherit.")]
		public UIPanelScriptableObject panelScriptableObj;
		public MagicWindow.WindowStyle windowStyle;
		[Tooltip("Sometimes the tab sprites need to be different depending on the position."
			+ "\n[0]: first tab in control.\n[1]: all other tabs.")]
		public Sprite[] titleBarSprites;

		public float titleBarFontSize = 36;
		public Color titleBarFontColor = new Color(1, 1, 1, 1);
		[Tooltip("Ex: Vector4(12, 8, 46, 16) for edged tabs, Vector4(12, 8, 12, 4) for titlebars.")]
		public Vector4 titleTextMargin = new Vector4(12, 8, 46, 12);
		public Vector2 titleBarMinSize;
		[Tooltip("Offset from bottom of tab to top of panel. Pos to move up, neg to move down.")]
		public float titleBarVerticalOffset;
		[Tooltip("Used for and WindowStyle.TitleBar & WindowStyle.Tabbed")]
		public Color selectedTabColor = new Color(1, 1, 1, 1);
		[Tooltip("Used for WindowStyle.Tabbed")]
		public Color deselectedTabColor = new Color(.5f, .5f, .5f, 1);
		[Tooltip("@TODO(Tristan): this")]
		public Color disabledTabColor = new Color(1, 1, 1, 1);

		public TextAlignmentOptions tabTextAlignment = TextAlignmentOptions.Center;

		[Tooltip("Offset applied to distance between tabs. Neg to move left (closer together), pos to move right (further apart).")]
		public float tabHorizontaloffset = -36;
		[Tooltip("Width to add/subtract to a panel after tab width calculation.\nIn general, this should be 0 for a non-tabbed window.")]
		public float panelWidthAdjust = 0;
	}
}
