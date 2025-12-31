using UnityEngine;
namespace AtomosZ.UI
{
	/// <summary>
	/// TODO(Tristan): Horizontal Layout Panel
	/// </summary>
	[CreateAssetMenu(fileName = "panelData", menuName = "AtomosZ/UI/UIPanelScriptableObject")]
	public class UIPanelScriptableObject : ScriptableObject
	{
		[Tooltip("The sprite for the panel.")]
		public Sprite backgroundSprite;
		public Vector2 minDimensions = new Vector2(96, 64);

		public RectOffset layoutPadding;
		public float layoutSpacing = 8;
	}
}