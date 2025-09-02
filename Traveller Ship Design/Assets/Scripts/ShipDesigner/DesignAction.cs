using UnityEngine;
using UnityEngine.Events;
using static DesignManager;

public class DesignAction
{
	public UnityAction action = null;
	public string buttonText;
	/// <summary>
	/// If you want the item to be visible but not selectable set to false.
	/// </summary>
	public bool enabled = true;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="newEditMode">EditMode to enable after action completes.</param>
	public DesignAction(string actionName, EditMode newEditMode)
	{
		buttonText = actionName;
		action += delegate
		{
			DesignManager.instance.ContextMenuCallback(newEditMode);
		};
	}

	public static DesignAction operator +(DesignAction da, UnityAction act)
	{
		da.action += act;
		return da;
	}
}
