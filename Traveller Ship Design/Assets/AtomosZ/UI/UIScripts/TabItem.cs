using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class TabItem : MonoBehaviour
	{
		public UIControlType dataType { get; }

		[SerializeField] private UIPanel panel;
		[SerializeField] private RectTransform panelRect;


		public bool isDirty { get; set; }
		public string referenceName { get; set; }
	}
}