using TMPro;
using UnityEngine;
using static AtomosZ.UI.UIExpandingLabel;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "UITabControlData", menuName = "AtomosZ/UIScriptableObjects/UITabControlScriptableObject")]
	public class UITabControlScriptableObject : ScriptableObject
	{
		[Tooltip("The UIPanelScriptableObject that new child panels will inherit.")]
		public UIPanelScriptableObject panelScriptableObj;
		public MagicWindow.WindowStyle windowStyle;
		[Tooltip("Sometimes the tab sprites need to be different depending on the position."
			+ "\n[0]: first tab in control.\n[1]: all other tabs.")]
		public Sprite[] titleBarSprites;

		public Color titleBarFontColor = new Color(1, 1, 1, 1);
		public Vector4 titleTextMargin;
		public Vector2 titleBarMinSize;
		[Tooltip("Offset from bottom of tab to top of panel.")]
		public float titleBarVerticalOffset;
		[Tooltip("Used for and WindowStyle.TitleBar & WindowStyle.Tabbed")]
		public Color selectedTabColor = new Color(1, 1, 1, 1);
		[Tooltip("Used for WindowStyle.Tabbed")]
		public Color deselectedTabColor = new Color(.5f, .5f, .5f, 1);
		[Tooltip("@TODO(Tristan): this")]
		public Color disabledTabColor = new Color(1, 1, 1, 1);

		public TextAlignmentOptions tabTextAlignment = TextAlignmentOptions.Center;

		[Tooltip("Offset applied to distance between tabs")]
		public float tabHorizontaloffset = -36;
		[Tooltip("Width to add/subtract after tab width calculation")]
		public float panelWidthAdjust = 0;
	}
}
