using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UIButtonScriptableObject")]
	public class UIButtonScriptableObject : ScriptableObject
	{
		public Sprite sprite;
		public LabelEx labelEx;
	}
}