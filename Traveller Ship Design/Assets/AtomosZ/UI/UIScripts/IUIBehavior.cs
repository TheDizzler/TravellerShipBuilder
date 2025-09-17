using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using static AtomosZ.Keyboard;

namespace AtomosZ.UI
{
	/// <summary>
	/// Remember to set the Layer of this object to UI!
	/// </summary>
	public interface IUIBehavior
	{
		public UIDesignObject designObject { get; }
		public void SetHover(bool isHover);
		public void UpdateHover(Vector3 posOfHover);
		public void ResetToLastPosition();
		/// <summary>
		/// Actions (usually UI feedback?) to take when an object is selected.
		/// If this object is not itself selectable, returns a parent or a related object that is part of this "group".
		/// </summary>
		/// <returns></returns>
		public UIDesignObject Select();

		public void Deselect();
		/// <summary>
		/// TODO(Tristan): This appears to be never used. Is this vestigial from IBehvaior object? How is it different from Select()?
		/// Actions to take when the user left clicks on the object.
		/// </summary>
		/// <param name="mouseWorldPos">This <i><b>should</b></i> be on or at least within the vicinity of this object.</param>
		/// <param name="keyInput"></param>
		public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput, ref UIDesignObject currentlySelectedObject);
		public Vector2 GetMinDimensions();
		public IUIDataEx GetBackingData();
		public void UpdateBackingData(IUIDataEx backingData);
		public void UpdateBackingData();
	}
}
