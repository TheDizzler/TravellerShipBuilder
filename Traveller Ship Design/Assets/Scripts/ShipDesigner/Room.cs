using System;
using System.Collections;
using System.Collections.Generic;

using AtomosZ.UI;

using TMPro;

using UnityEngine;

using static AtomosZ.Keyboard;
using static DesignManager;
using static RoomSerializer;


public class Room : MonoBehaviour, IDesignBehavior
{
	[SerializeField] private RoomLabel roomLabel;
	private DesignObject _designObject;

	private Wall wall;
	public bool isLayoutLocked = false;

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

	public SerializedRoom GetSerializableData()
	{
		var orgPos = transform.position;
		wall.StartDrag(Vector2.zero);
		var serializedRoom = new SerializedRoom()
		{
			roomLabel = roomLabel.text,
			wall = wall.GetSerializableData(),
		};
		ResetToLastPosition();
		return serializedRoom;
	}

	public static Room CreateFromSerializedData(SerializedRoom roomData)
	{
		var wallDObj = Instantiate(DesignManager.GetPrefab(PrefabType.WallSegmentPrefab));
		var wall = wallDObj.GetComponent<Wall>();
		wall.CreateFromSerializedData(roomData.wall);
		var room = wall.ConvertToRoom(roomData.roomLabel);
		return room;
	}

	public void ToggleRoomLabel(bool showLabel)
	{
		roomLabel.gameObject.SetActive(showLabel);
	}

	public Rect GetDimensions()
	{
		return wall.GetDimensions();
	}

	public DesignObject Select()
	{
		return designObject;
	}

	public void Deselect()
	{

	}

	public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput,
		ref DesignObject currentlySelectedObject, ref EditMode editMode)
	{
		if (currentlySelectedObject != null)
		{
			currentlySelectedObject.Deselect();
		}

		currentlySelectedObject = designObject;
		if (!isLayoutLocked)
		{
			wall.StartDrag(mouseWorldPos);
			editMode = EditMode.MoveObject;
		}

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
		roomLabel.transform.position = wall.GetCenter();
	}


	public void ResetToLastPosition()
	{
		wall.ResetToLastPosition();
		roomLabel.transform.position = wall.GetCenter();
	}


	public Vector3 SnapToGrid(Vector3 pos)
	{
		return wall.SnapToGrid(pos);
	}

	public void SetHover(bool isHovering)
	{
		roomLabel.SetHoverColor(isHovering);
		roomLabel.ShowLockIcon(isHovering && isLayoutLocked);

		// hover behavior on wall should be different when contained in a Room?
		//wall.SetHover(isHovering);
	}

	public void UpdateHover(Vector3 posOfHover)
	{
		// hover behavior on wall should be different when contained in a Room?
		//wall.UpdateHover(posOfHover);
	}


	public void GetContextMenuItems(List<DesignAction> actionDict)
	{
		var renameAction = new DesignAction("Rename Room", EditMode.None);
		renameAction += ShowRenameDialog;
		actionDict.Add(renameAction);

		if (!isLayoutLocked)
		{
			var lockAction = new DesignAction("Lock Layout", EditMode.None);
			lockAction += LockRoom;
			actionDict.Add(lockAction);
		}
		else
		{
			var lockAction = new DesignAction("Unlock Layout", EditMode.None);
			lockAction += LockRoom;
			actionDict.Add(lockAction);
		}

		var saveRoomAction = new DesignAction("Save Room Layout", EditMode.None);
		saveRoomAction += ShowSaveRoomDialog;
		actionDict.Add(saveRoomAction);

		//var lockAction = new DesignAction(EditMode.None);
		//lockAction += wall.LockControlPoints;
		//actionDict.Add("Lock Control Points", lockAction);

		actionDict.Add(null);

		var dismantleAction = new DesignAction("Dismantle Room", EditMode.None);
		dismantleAction.enabled = !isLayoutLocked;
		dismantleAction += Dismantle;
		actionDict.Add(dismantleAction);
	}

	private void LockRoom()
	{
		isLayoutLocked = !isLayoutLocked;
	}

	private void ShowSaveRoomDialog()
	{
		var savedRoom = GetSerializableData();
		var panel = DesignManager.GetDynamicPanel();
		panel.name = "SaveRoomLayoutPanel";
		panel.designObject.isModal = true;
		panel.showCloseButton = true;
		panel.SetTitle("Save Room Layout", DynamicPanel.TitleLabelStyle.Bar);
		if (RoomSerializer.IsNameUnique(savedRoom.roomLabel))
		{
			if (!RoomSerializer.SaveRoom(savedRoom, savedRoom.roomLabel))
			{
				panel.SetTitle("Error while saving", DynamicPanel.TitleLabelStyle.Bar);
				panel.AddText_NoData("Could not save file");
			}
			else
			{
				var label = panel.AddText(new LabelEx());
				label.text = "Room saved";
				panel.AddButtonPanel(new ButtonPanelEx(UIButtonPanel.DialogButton.OK));
			}
		}
		else
		{
			panel.SetTitle("Room already exists", DynamicPanel.TitleLabelStyle.Bar);
			panel.AddText_NoData($"Room with same name already exists. Overwrite or save as alternate?");
			var checkBox = (UICheckBox)panel.AddUIControl(UIControlType.CheckBox);
			var checkBoxData = (CheckBoxEx)checkBox.GetBackingData();
			checkBoxData.isOnByDefault = false;
			checkBoxData.action = new UnityEngine.Events.UnityEvent<bool>();
			checkBoxData.action.AddListener(OnOverwriteToggled);

			int altNum = 0;
			string altName;
			string rootName = roomLabel.text;
			var underscoreIndex = rootName.LastIndexOf('_');
			if (underscoreIndex != -1)
			{
				var tail = rootName.Substring(underscoreIndex + 1);
				if (!int.TryParse(tail, out altNum))
					altNum = 0;
				else
					rootName = rootName.Substring(0, underscoreIndex);
			}

			do
			{
				if (altNum < 100)
					altName = rootName + "_" + (++altNum).ToString("00");
				else
					altName = rootName + "_" + (++altNum);
			}
			while (!RoomSerializer.IsNameUnique(altName));

			inputField = panel.AddInputField(new InputFieldEx("Enter new room name", altName)).GetComponent<TMP_InputField>();
			panel.AddButtonPanel(new ButtonPanelEx(UIButtonPanel.DialogButton.OKCancel));

			panel.OnClose += SaveRenamePanelClosed;
		}

		panel.Show(Vector2.zero);
	}

	private void SaveRenamePanelClosed(DynamicPanel panel)
	{
		if (panel.result == BottomPanel.DialogResult.OK)
		{
			if (!inputField.interactable)
			{ // overwrite
				var savedRoom = GetSerializableData();
				RoomSerializer.SaveRoom(savedRoom, savedRoom.roomLabel);
			}
			else
			{
				SetRoomName(inputField.text);
				ShowSaveRoomDialog();
			}
		}
	}

	private void OnOverwriteToggled(bool isOn)
	{
		inputField.interactable = !isOn;
	}



	private TMP_InputField inputField;

	public void ShowRenameDialog()
	{
		var panel = DesignManager.GetDynamicPanel();
		panel.designObject.isModal = true;
		inputField = panel.AddInputField(new InputFieldEx("Enter new room name", roomLabel.text)).GetComponent<TMP_InputField>();
		panel.AddButtonPanel(new ButtonPanelEx(UIButtonPanel.DialogButton.OKCancel));
		panel.SetTitle("Enter room name", DynamicPanel.TitleLabelStyle.BladedBar);

		panel.OnClose += RenameDialogClosed;

		panel.Show(Vector2.zero);
	}

	private void RenameDialogClosed(DynamicPanel panel)
	{
		if (panel.result == BottomPanel.DialogResult.OK)
		{
			SetRoomName(inputField.text);
		}

		inputField = null;
	}

	public void SetRoomName(string newName)
	{
		roomLabel.text = newName;
	}

	public void Dismantle()
	{
		wall.transform.SetParent(null);
		wall.RevertFromRoom();
		Destroy(gameObject);
	}


	public EditMode Create(Vector3 pos, out DesignObject createdObject)
	{
		createdObject = designObject;
		return EditMode.None;
	}

	public bool Interact(IDesignBehavior otherObject)
	{
		throw new NotImplementedException();
	}

	public void EndInteraction()
	{
		throw new NotImplementedException();
	}


	public void Delete()
	{
		throw new NotImplementedException();
	}
}
