using UnityEngine;
namespace AtomosZ.UI
{
	/// <summary>
	/// TODO(Tristan): Horizontal Layout Panel
	/// </summary>
	[CreateAssetMenu(fileName = "UIPanelData", menuName = "AtomosZ/UIScriptableObjects/UIPanelScriptableObject")]
	public class UIPanelScriptableObject : ScriptableObject
	{
		[Tooltip("The sprite for the panel.")]
		public Sprite backgroundSprite;

		public Vector2 minDimensions = new Vector2(96, 64);
		
		public Vector4 layoutPadding = new Vector4(32, 32, 16, 16);
		public float layoutSpacing = 8;
	}
}