using UnityEngine.Events;
using static DesignManager;

public class DesignAction
{
	public UnityAction action = null;

	/// <summary>
	/// 
	/// </summary>
	/// <param name="newEditMode">EditMode to enable after action completes.</param>
	public DesignAction(EditMode newEditMode)
	{
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
