using System;
using System.Collections;
using System.Collections.Generic;

using AtomosZ.UI;

using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static CustomCursor;
using static DesignManager;

/// <summary>
/// Note(Tristan): For now, gameobjects are given MonoBehaviors that implement interfaces.
/// In the future, we could have one interface that all DesignObjects implement and select bools on the prefab
/// to indicate whether or not that object is capable of that.
/// Or just have lots of empty functions that don't do anything.
/// Having separate interfaces per action type is nice because it allows us to have separate MonoBehaviours on an object
/// that implement one (or more) interface keep classes smaller.
/// </summary>
[RequireComponent(typeof(IDesignBehavior))]
public class DesignObject : MonoBehaviour
{
	public CursorSpriteMode hoverCursorMode;
	public CursorSpriteMode moveCursorMode;
	public Color normalColor = Color.black;
	public Color hoverColor = new Color(1, .765f, 0, 1);
	public Color selectColor = new Color(1, 0, 1, 1);
	public Color minorSelectColor = new Color(1, .765f, 1, 1);
	public List<string> tooltip;

	public bool isHoverable = false;
	public bool isMoveable = false;
	public bool isCreateable = false;
	public bool isInteractable = false;
	public bool isSelectable = false;
	private IDesignBehavior designBehavior;


	void Awake()
	{
		SearchForDesignObject();
	}


	/// <summary>
	/// @Note(Tristan): This is unfortunately required because the references are lost on hot-reload.
	/// </summary>
	private void SearchForDesignObject()
	{
		var components = GetComponents<MonoBehaviour>();
		foreach (var comp in components)
		{
			if (comp is IDesignBehavior)
			{
				designBehavior = (IDesignBehavior)comp;
				return;
			}
		}
	}


	public void SetHover(bool isHover)
	{
		if (!isHoverable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		designBehavior.SetHover(isHover);
		if (isHover)
			CustomCursor.SetCursor(hoverCursorMode);
		else
			CustomCursor.SetCursor(CursorSpriteMode.Default);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		if (!isHoverable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		designBehavior.UpdateHover(posOfHover);
	}


	public UIDesignObject GetContextMenu(Vector2 openContextPosition)
	{
		if (!isHoverable)
			return null;

		if (designBehavior == null)
			SearchForDesignObject();

		var actionDict = new List<DesignAction>();
		designBehavior.GetContextMenuItems(actionDict);
		if (actionDict.Count > 0)
		{
			var contextMenu = DesignManager.GetDynamicPanel();
			contextMenu.SetTitle("", DynamicPanel.TitleLabelStyle.None);
			contextMenu.SetContextMenuActions(actionDict);
			contextMenu.Show(openContextPosition);
			return contextMenu.designObject;
		}

		return null;
	}

	public Vector3 SnapToGrid(Vector3 pos)
	{
		if (!isMoveable)
			return pos;

		if (designBehavior == null)
			SearchForDesignObject();

		return designBehavior.SnapToGrid(pos);
	}


	public void MouseDrag(Vector2 pos)
	{
		if (!isMoveable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		designBehavior.MouseDrag(pos);
	}

	public void EndDrag(Vector2 pos)
	{
		if (!isMoveable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		designBehavior.EndDrag(pos);
	}

	public void ResetToLastPosition()
	{
		if (!isMoveable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();
		designBehavior.ResetToLastPosition();
	}

	public EditMode Create(Vector3 pos, out DesignObject createdObject)
	{
		if (!isCreateable)
		{
			createdObject = null;
			return EditMode.None;
		}

		if (designBehavior == null)
			SearchForDesignObject();
		return designBehavior.Create(pos, out createdObject);
	}

	/// <summary>
	/// The other object that was passed in to Interact.
	/// Not sure if this is needed but may become relevant in the future.
	/// </summary>
	private DesignObject interactingObject;

	public bool IsDragging()
	{
		if (!isMoveable)
			return false;

		if (designBehavior == null)
			SearchForDesignObject();
		return designBehavior.IsDragging();
	}

	/// <summary>
	/// The action to take when a DesignObject is dragged by another DesignObject.<br/>
	/// Ex: a door is dragged over wall.
	/// </summary>
	/// <param name="otherObject"></param>
	/// <returns>True if objects have an interaction behaviour.</returns>
	public bool Interact(DesignObject otherObject)
	{
		if (!otherObject.isInteractable || !isInteractable)
			return false;

		if (designBehavior == null)
			SearchForDesignObject();
		otherObject.SearchForDesignObject();

		interactingObject = otherObject;
		return designBehavior.Interact(otherObject.designBehavior);
	}

	public void EndInteraction()
	{
		if (!isInteractable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		//Debug.Log($"End interaction between {this.name} and {interactingObject.name}");
		designBehavior.EndInteraction();
		//interactingObject.EndInteraction();
		interactingObject = null;
	}

	public DesignObject Select()
	{
		if (!isSelectable)
			return null;

		if (designBehavior == null)
			SearchForDesignObject();

		DesignManager.instance.toolTip.SetToolTip(tooltip);
		return designBehavior.Select();
	}

	public void Deselect()
	{
		if (!isSelectable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		DesignManager.instance.toolTip.SetToolTip(null);
		designBehavior.Deselect();
	}

	/// <summary>
	/// Actions to take when the user left clicks on the object.
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	/// <param name="keyInput"></param>
	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput,
		ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (!isSelectable)
			return;

		if (designBehavior == null)
			SearchForDesignObject();

		designBehavior.Clicked(mouseWorldPos, keyInput, ref currentlySelectedObject, ref editMode);
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="otherDesignObject"></param>
	/// <returns>True if these two objects share a common ancestry or are the same object.</returns>
	public bool PartOfSameHierarchy(DesignObject otherDesignObject)
	{
		if (otherDesignObject == null)
			return false;

		var thisHierarchy = new List<Transform>();
		var thisObject = this.transform;
		while (thisObject != null)
		{
			thisHierarchy.Add(thisObject);
			thisObject = thisObject.transform.parent;
		}

		var otherHierarchy = new List<Transform>();
		var otherObject = otherDesignObject.transform;
		while (otherObject != null)
		{
			otherHierarchy.Add(otherObject);
			otherObject = otherObject.transform.parent;
		}

		foreach (var thisObj in thisHierarchy)
		{
			foreach (var otherObj in otherHierarchy)
			{
				if (thisObj == otherObj)
					return true;
			}
		}

		return false;
	}
}
