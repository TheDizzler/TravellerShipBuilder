using System.Collections.Generic;
using UnityEngine;

public interface IHoverable
{
	public DesignObject designObject { get; }
	public void SetHover(bool isHovering);
	public void UpdateHover(Vector3 posOfHover);
	public Dictionary<string, DesignAction> GetContextMenuItems();
}
