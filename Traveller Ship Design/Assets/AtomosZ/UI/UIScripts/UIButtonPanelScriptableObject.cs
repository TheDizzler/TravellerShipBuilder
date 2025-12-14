using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// 
	/// </summary>
	[CreateAssetMenu(fileName = "buttonData", menuName = "AtomosZ/UIScriptableObjects/UIButtonPanelScriptableObject")]
	public class UIButtonPanelScriptableObject : ScriptableObject
	{
		public UIButtonScriptableObject okButtonData;
		public UIButtonScriptableObject cancelButtonData;
		public UIButtonScriptableObject yesButtonData;
		public UIButtonScriptableObject noButtonData;
	}
}