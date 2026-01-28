using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	public class UIMenuDivider : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.MenuDivider; } }
		public bool interactable { get; set; }

		public ScriptableObject GetBackingData()
		{
			return null;
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public Vector2 GetMinDimensions()
		{
			return rect.sizeDelta;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}
	}
}