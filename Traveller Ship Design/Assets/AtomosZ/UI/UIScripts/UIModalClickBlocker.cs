using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	/// <summary>
	/// A simple class to block any clicks from being registered when a modal dialog box is open.
	/// </summary>
	public class UIModalClickBlocker : UIPooledMonoBehaviour<UIModalClickBlocker>, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.ModalClickBlocker; } }

		public bool interactable
		{
			get { return _interactable; }
			set { _interactable = value; }
		}

		public Vector2 minDimensions { get; set; }
		public Vector2 maxDimensions { get; set; }

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