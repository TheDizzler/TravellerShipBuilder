using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// A simple class to block any clicks from being registered when a modal dialog boxImage is open.
	/// </summary>
	public class ModalClickBlocker : MonoBehaviour, IUIBehavior
	{
		public UIDesignObject designObject { get; }
		/// <summary>
		/// ModalClickBlocker should not need a reference name.
		/// </summary>
		public string referenceName { get { return null; } }

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

		public IUIDataEx GetBackingData()
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

		public void UpdateBackingData(IUIDataEx backingData)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData()
		{
			throw new System.NotImplementedException();
		}
	}
}