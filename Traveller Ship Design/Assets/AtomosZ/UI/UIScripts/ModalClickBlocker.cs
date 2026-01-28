using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	/// <summary>
	/// A simple class to block any clicks from being registered when a modal dialog box is open.
	/// </summary>
	public class ModalClickBlocker : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.ModalClickBlocker; } }

		public bool interactable
		{
			get { return _interactable; }
			set { _interactable = value; }
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
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