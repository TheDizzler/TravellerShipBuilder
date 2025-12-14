using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "buttonData", menuName = "AtomosZ/UIScriptableObjects/UIButtonScriptableObject")]
	public class UIButtonScriptableObject : ScriptableObject
	{
		public Sprite sprite;
		public UIExpandingLabelScriptableObject labelData;
	}
}