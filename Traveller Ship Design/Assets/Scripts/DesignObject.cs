using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
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
	private IHoverable hoverable;
	private IMoveable moveable;
	private ICreateable createable;
	private IInteractable interactable;
	private ISelectable selectable;

	private MonoBehaviour[] components;


	void Awake()
	{
		components = GetComponents<MonoBehaviour>();
		SearchForDesignObjects();
	}

	/// <summary>
	/// @Note(Tristan): This is unfortunately required because the references are lost on hot-reload.
	/// </summary>
	/// <exception cref="Exception"></exception>
	private void SearchForDesignObjects()
	{
		foreach (var comp in components)
		{
			if (comp is IHoverable)
			{
				if (hoverable != null)
					throw new Exception("Only one IHoverable per gameobject!");
				isHoverable = true;
				hoverable = (IHoverable)comp;
			}

			if (comp is IMoveable)
			{
				if (moveable != null)
					throw new Exception("Only one IMoveable per gameobject!");
				isMoveable = true;
				moveable = (IMoveable)comp;
			}

			if (comp is ICreateable)
			{
				if (createable != null)
					throw new Exception("Only one ICreatable per gameobject!");
				isCreateable = true;
				createable = (ICreateable)comp;
			}

			if (comp is IInteractable)
			{
				if (interactable != null)
					throw new Exception("Only one IInteractable per gameobject!");
				isInteractable = true;
				interactable = (IInteractable)comp;
			}

			if (comp is ISelectable)
			{
				if (selectable != null)
					throw new Exception("Only one ISelectable per gameobject!");
				isSelectable = true;
				selectable = (ISelectable)comp;
			}
		}
	}


	public void SetHover(bool isHover)
	{
		if (hoverable == null)
		{
			if (isHoverable)
				SearchForDesignObjects();
			else
				return;
		}

		hoverable.SetHover(isHover);
		if (isHover)
			CustomCursor.SetCursor(hoverCursorMode);
		else
			CustomCursor.SetCursor(CursorSpriteMode.Default);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		if (hoverable == null)
		{
			if (isHoverable)
				SearchForDesignObjects();
			else
				return;
		}

		hoverable.UpdateHover(posOfHover);
	}


	public void SetContextMenu(UIPanel contextMenu, Vector2 openContextPosition)
	{
		if (hoverable == null)
		{
			if (isHoverable)
				SearchForDesignObjects();
			else
				return;
		}

		var actions = hoverable.GetContextMenuItems();
		if (actions != null)
		{
			contextMenu.SetContextMenu(actions);
			contextMenu.ShowContextMenu(openContextPosition);
		}
	}

	public Vector3 SnapToGrid(Vector3 pos)
	{
		if (moveable == null)
		{
			if (isMoveable)
				SearchForDesignObjects();
			else
				return pos;
		}

		return moveable.SnapToGrid(pos);
	}


	public void MouseDrag(Vector2 pos)
	{
		if (moveable == null)
		{
			if (isMoveable)
				SearchForDesignObjects();
			else
				return;
		}

		moveable.MouseDrag(pos);
	}

	public void EndDrag(Vector2 pos)
	{
		if (moveable == null)
		{
			if (isMoveable)
				SearchForDesignObjects();
			else
				return;
		}

		moveable.EndDrag(pos);
	}

	public void ResetToLastPosition()
	{
		if (moveable == null)
		{
			if (isMoveable)
				SearchForDesignObjects();
			else
				return;
		}

		moveable.ResetToLastPosition();
	}

	public EditMode Create(Vector3 pos, out DesignObject createdObject)
	{
		if (createable == null)
		{
			if (isCreateable)
				SearchForDesignObjects();
			else
			{
				createdObject = null;
				return EditMode.None;
			}
		}

		return createable.Create(pos, out createdObject);
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
		if (moveable == null)
		{
			SearchForDesignObjects();
		}

		return moveable.IsDragging();
	}

	/// <summary>
	/// The action to take when a DesignObject is dragged by another DesignObject.<br/>
	/// Ex: a door is dragged over wall.
	/// </summary>
	/// <param name="otherObject"></param>
	/// <returns>True if objects have an interaction behaviour.</returns>
	public bool Interact(DesignObject otherObject)
	{
		if (!otherObject.isInteractable)
			return false;

		if (interactable == null)
		{
			if (isInteractable)
			{
				SearchForDesignObjects();
				otherObject.SearchForDesignObjects();
			}
			else
			{
				return false;
			}
		}

		interactingObject = otherObject;
		return interactable.Interact(otherObject.interactable);
	}

	public void EndInteraction()
	{
		if (interactable == null)
		{
			if (isInteractable)
			{
				SearchForDesignObjects();
			}
			else
			{
				return;
			}
		}

		//Debug.Log($"End interaction between {this.name} and {interactingObject.name}");
		interactable.EndInteraction();
		//interactingObject.EndInteraction();
		interactingObject = null;
	}

	public DesignObject Select()
	{
		if (selectable == null)
		{
			if (isSelectable)
			{
				SearchForDesignObjects();
			}
			else
			{
				return null;
			}
		}

		DesignManager.instance.toolTip.SetToolTip(tooltip);
		return selectable.Select();
	}

	public void Deselect()
	{
		if (selectable == null)
		{
			if (isSelectable)
			{
				SearchForDesignObjects();
			}
			else
			{
				return;
			}
		}

		selectable.Deselect();
	}

	/// <summary>
	/// Actions to take when the user left clicks on the object.
	/// </summary>
	/// <param name="mouseWorldPos"></param>
	/// <param name="keyInput"></param>
	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (selectable == null)
		{
			if (isSelectable)
			{
				SearchForDesignObjects();
			}
			else
			{
				return;
			}
		}

		selectable.Clicked(mouseWorldPos, keyInput, ref currentlySelectedObject, ref editMode);
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
