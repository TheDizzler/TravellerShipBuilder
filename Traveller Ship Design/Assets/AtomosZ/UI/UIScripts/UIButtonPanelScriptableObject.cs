using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UIButtonPanelScriptableObject")]
	public class UIButtonPanelScriptableObject : ScriptableObject
	{
		public ButtonEx okButton;
		public ButtonEx cancelButton;
		public ButtonEx yesButton;
		public ButtonEx noButton;
	}
}