using System;
using System.Collections.Generic;

using AtomosZ.UI;

using UnityEngine;

using static AtomosZ.Keyboard;
using static AtomosZ.MG2eTraveller.Ship.DesignManager;

namespace AtomosZ.MG2eTraveller.Ship
{
	public class RoomLabel : MonoBehaviour, IDesignBehavior
	{
		[SerializeField] private ExpandingLabel roomLabel;
		[SerializeField] private Room room;
		[SerializeField] private SpriteRenderer lockIcon;

		private DesignObject _designObject;
		public DesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<DesignObject>();
				return _designObject;
			}
		}

		public string text
		{
			get { return roomLabel.text; }
			set { roomLabel.text = value; }
		}



		public void ShowLockIcon(bool showIcon)
		{
			lockIcon.gameObject.SetActive(showIcon);
		}

		public DesignObject Select()
		{
			return room.Select();
		}

		public void Deselect()
		{
			room.Deselect();
		}

		public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput,
			ref DesignObject currentlySelectedObject, ref EditMode editMode)
		{
			room.Clicked(mouseWorldPos, keyInput, ref currentlySelectedObject, ref editMode);
		}

		public bool IsDragging()
		{
			return room.IsDragging();
		}

		public void MouseDrag(Vector2 worldPos)
		{
			room.MouseDrag(worldPos);
		}


		public void ResetToLastPosition()
		{
			room.ResetToLastPosition();
		}

		public void EndDrag(Vector2 pos)
		{
			room.EndDrag(pos);
		}

		public Vector3 SnapToGrid(Vector3 pos)
		{
			return room.SnapToGrid(pos);
		}


		public void GetContextMenuItems(List<UIMenuAction> actionDict)
		{
			room.GetContextMenuItems(actionDict);
		}



		public void SetHover(bool isHovering)
		{
			room.SetHover(isHovering);
		}

		public void UpdateHover(Vector3 posOfHover)
		{
			room.UpdateHover(posOfHover);
		}

		public void SetHoverColor(bool isHovering)
		{
			if (isHovering)
				roomLabel.color = designObject.hoverColor;
			else
				roomLabel.color = designObject.normalColor;
		}

		public bool Interact(IDesignBehavior otherObject)
		{
			throw new NotImplementedException();
		}

		public void EndInteraction()
		{
			throw new NotImplementedException();
		}

		public EditMode Create(Vector3 pos, out DesignObject createdObject)
		{
			throw new NotImplementedException();
		}

		public void Delete()
		{
			throw new NotImplementedException();
		}
	}
}