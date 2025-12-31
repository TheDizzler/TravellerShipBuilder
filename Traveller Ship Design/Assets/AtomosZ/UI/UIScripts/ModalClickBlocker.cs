using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	/// <summary>
	/// A simple class to block any clicks from being registered when a modal dialog box is open.
	/// </summary>
	public class ModalClickBlocker : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get; }
		public UIDesignObject designObject { get; }
		public bool isDirty { get; set; }
		/// <summary>
		/// ModalClickBlocker should not need a reference name.
		/// </summary>
		public string referenceName { get; set; }

		public bool interactable { get; set; }

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public void SetHover(bool isHover)
		{
		}

		public void UpdateHover(Vector3 posOfHover)
		{
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}

		public void Deselect()
		{
			throw new System.NotImplementedException();
		}

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public Vector2 GetMinDimensions()
		{
			throw new System.NotImplementedException();
		}

		public void ResetToLastPosition()
		{
			throw new System.NotImplementedException();
		}

		public UIDesignObject Select()
		{
			throw new System.NotImplementedException();
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