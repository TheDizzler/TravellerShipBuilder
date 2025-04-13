using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;

public interface ISelectable
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
	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode);
}
