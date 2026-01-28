using AtomosZ.UI;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "ButtonPanelData", menuName = "AtomosZ/UI/ButtonPanelScriptableObject")]
	public class UIButtonPanelScriptableObject : ScriptableObject
	{
		public UIButtonScriptableObject okButtonData;
		public UIButtonScriptableObject cancelButtonData;
		public UIButtonScriptableObject yesButtonData;
		public UIButtonScriptableObject noButtonData;
	}
}