using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
	public DesignObject designObject { get; }
	/// <summary>
	/// 
	/// </summary>
	/// <param name="otherObject"></param>
	/// <returns>True if objects have an interaction behaviour.</returns>
	public bool Interact(IInteractable otherObject);
	public void EndInteraction();
}
