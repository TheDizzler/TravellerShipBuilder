using UnityEngine;
namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UIPanelScriptableObject")]
	public class UIPanelScriptableObject : ScriptableObject
	{
		public Sprite backgroundSprite;
		public Vector2 minDimensions = new Vector2(96, 64);
		public Vector4 layoutPadding = new Vector4(32, 32, 16, 16);
		public float layoutSpacing = 8;
	}
}