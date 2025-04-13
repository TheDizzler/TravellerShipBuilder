using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DesignManager;

public class Room : MonoBehaviour, IMoveable
{
	[SerializeField] private RoomLabel roomLabel;
	private DesignObject _designObject;

	private Wall wall;


	public DesignObject designObject
	{
		get
		{
			if (_designObject == null)
				_designObject = GetComponent<DesignObject>();
			return _designObject;
		}
	}


	public void SetRoom(Wall wall, string name, Vector2 roomCenterPoint)
	{
		this.wall = wall;
		roomLabel.text = name;
		roomLabel.transform.position = roomCenterPoint;
	}

	public DesignObject Select()
	{

		return designObject;
	}

	public void Deselect()
	{

	}

	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput, ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (currentlySelectedObject != null)
		{
			currentlySelectedObject.Deselect();
		}

		currentlySelectedObject = designObject;
		wall.StartDrag(mouseWorldPos);
		editMode = EditMode.MoveObject;

		currentlySelectedObject.Select();
	}


	public bool IsDragging()
	{
		return wall.IsDragging();
	}


	public void MouseDrag(Vector2 worldPos)
	{
		wall.MouseDrag(worldPos);
		roomLabel.transform.position = wall.GetCenter(); // this could get slow if calculating every frame
	}

	public void EndDrag(Vector2 pos)
	{
		wall.EndDrag(pos);
	}


	public void ResetToLastPosition()
	{
		wall.ResetToLastPosition();
	}


	public Vector3 SnapToGrid(Vector3 pos)
	{
		return wall.SnapToGrid(pos);
	}

	public void SetHover(bool isHovering)
	{
		roomLabel.SetHoverColor(isHovering);
		// hover behavior on wall should be different when contained in a Room?
		//wall.SetHover(isHovering);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		// hover behavior on wall should be different when contained in a Room?
		//wall.UpdateHover(posOfHover);
	}


	public Dictionary<string, DesignAction> GetContextMenuItems()
	{
		var actionDict = new Dictionary<string, DesignAction>();

		var renameAction = new DesignAction(EditMode.None);
		renameAction += ShowRenameDialog;
		actionDict.Add("Rename Room", renameAction);

		var saveRoomAction = new DesignAction(EditMode.None);

		//var lockAction = new DesignAction(EditMode.None);
		//lockAction += wall.LockControlPoints;
		//actionDict.Add("Lock Control Points", lockAction);

		actionDict.Add("divider", null);

		var dismantleAction = new DesignAction(EditMode.None);
		dismantleAction += Dismantle;
		actionDict.Add("Dismantle Room", dismantleAction);
		return actionDict;
	}

	private TMP_InputField inputField;

	public void ShowRenameDialog()
	{
		var panelRect = Instantiate(DesignManager.GetPrefab(UIPrefabType.DynamicPanel));
		var panel = panelRect.GetComponent<DynamicPanel>();
		panel.SetTitle("Enter room name", DynamicPanel.TitleLabelType.Bladed);
		panel.SetButtons(BottomPanel.DialogButton.OKCancel);
		panel.OnClose += RenameDialogClosed;
		inputField = panel.AddInputField("Enter new room name", roomLabel.text);
		DesignManager.ShowDialog(panelRect);
	}

	private void RenameDialogClosed(DynamicPanel panel)
	{
		if (panel.result == BottomPanel.DialogResult.OK)
		{
			roomLabel.text = inputField.text;
		}

		inputField = null;
		DesignManager.CloseDialog(panel.GetComponent<RectTransform>());
	}


	public void Dismantle()
	{
		wall.transform.SetParent(null);
		wall.RevertFromRoom();
		Destroy(gameObject);
	}
}
