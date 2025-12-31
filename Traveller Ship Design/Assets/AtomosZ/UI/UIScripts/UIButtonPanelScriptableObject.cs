using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "ButtonData", menuName = "AtomosZ/UI/UIButtonPanelScriptableObject")]
	public class UIButtonPanelScriptableObject : ScriptableObject
	{
		public UIButtonScriptableObject okButtonData;
		public UIButtonScriptableObject cancelButtonData;
		public UIButtonScriptableObject yesButtonData;
		public UIButtonScriptableObject noButtonData;
	}
}