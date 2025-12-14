using System.Collections.Generic;
using AtomosZ.UI;
using UnityEngine;

using static AtomosZ.Keyboard;
using static AtomosZ.MG2eTraveller.Ship.DesignManager;

namespace AtomosZ.MG2eTraveller.Ship
{
	public interface IDesignBehavior
	{
		public DesignObject designObject { get; }
		/// <summary>
		/// Actions (usually UI feedback?) to take when an object is selected.
		/// If this object is not itself selectable, returns a parent or a related object that is part of this "group".
		/// </summary>
		/// <returns></returns>
		public DesignObject Select();
		public void Deselect();
		/// <summary>
		/// Actions to take when the user left clicks on the object.
		/// </summary>
		/// <param name="mouseWorldPos">This <i><b>should</b></i> be on or at least within the vicinity of this object.</param>
		/// <param name="keyInput"></param>
		public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode);
		public void SetHover(bool isHovering);
		public void UpdateHover(Vector3 posOfHover);
		public void GetContextMenuItems(List<DesignAction> actionDict);
		public Vector3 SnapToGrid(Vector3 pos);
		public void MouseDrag(Vector2 worldPos);
		public bool IsDragging();
		public void EndDrag(Vector2 pos);
		public void ResetToLastPosition();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="otherObject"></param>
		/// <returns>True if objects have an interaction behaviour.</returns>
		public bool Interact(IDesignBehavior otherObject);
		public void EndInteraction();
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="createdObject"></param>
		/// <returns>The edit mode that we want to default to after the object is created.</returns>
		public EditMode Create(Vector3 pos, out DesignObject createdObject);
		public void Delete();
	}
}