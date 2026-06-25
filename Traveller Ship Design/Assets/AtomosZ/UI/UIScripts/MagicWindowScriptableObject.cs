using TMPro;
using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "MagicContextMenuData", menuName = "AtomosZ/UI/MagicContextMenuScriptableObject")]
	public abstract class MagicUIScriptableObject : ScriptableObject
	{
		[Tooltip("The UIPanelScriptableObject that new child panels will inherit (including the main panel).")]
		public UIPanelScriptableObject panelScriptableObj;


		public float titleBarFontSize = 36;
		public Color titleBarFontColor = new Color(1, 1, 1, 1);
		[Tooltip("Ex: Vector4(12, 8, 46, 16) for edged tabs, Vector4(12, 8, 12, 4) for titlebars.")]
		public Vector4 titleTextMargin = new Vector4(12, 8, 46, 12);
		public Vector2 titleBarMinSize;
		

		public TextAlignmentOptions titleTextAlignment = TextAlignmentOptions.Center;

		[Tooltip("Should the control panel be offset by the tab height? If false, controls in panel are offset by tab height instead.")]
		public bool offsetPanelByTitleHeight;
		[Tooltip("Offset from bottom of tab to top of panel. Pos to move panel up, neg to move panel down.")]
		public float panelVerticalOffset;
		[Tooltip("Width to add/subtract to a panel after tab width calculation.\nIn general, this would be 0 for a non-tabbed window (but your artist might disagree).")]
		public float panelWidthAdjust = 0;
	}


	[CreateAssetMenu(fileName = "MagicWindowData", menuName = "AtomosZ/UI/MagicWindowScriptableObject")]
	public class MagicWindowScriptableObject : MagicUIScriptableObject
	{
		public Sprite titleBarSprite;
		public Color titleBarColor = new Color(1, 1, 1, 1);
	}
}
