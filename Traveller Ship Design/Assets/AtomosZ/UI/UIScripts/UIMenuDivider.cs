using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	public class UIMenuDivider : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.MenuDivider; } }
		public bool interactable { get; set; }

		public Vector2 minDimensions { get; set; }
		public Vector2 maxDimensions { get; set; }

		public LayoutElement layout;

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

		public Vector2 GetDrawnDimensions()
		{
			return rect.sizeDelta;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		public void RecalculateDimensions()
		{
			throw new System.NotImplementedException();
		}
	}
}