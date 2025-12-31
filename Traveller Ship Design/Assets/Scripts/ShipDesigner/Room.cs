using System;
using System.Collections;
using System.Collections.Generic;

using AtomosZ.UI;

using TMPro;

using UnityEngine;

using static AtomosZ.Keyboard;
using static AtomosZ.MG2eTraveller.Ship.DesignManager;
using static AtomosZ.MG2eTraveller.Ship.RoomSerializer;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.MG2eTraveller.Ship
{
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
			var window = DesignManager.GetMagicWindow();
			window.name = "SaveRoomLayoutPanel";
			window.designObject.isModal = true;
			window.showCloseButton = true;
			window.SetTitle("Save Room Layout");
			if (RoomSerializer.IsNameUnique(savedRoom.roomLabel))
			{
				if (!RoomSerializer.SaveRoom(savedRoom, savedRoom.roomLabel))
				{
					window.SetTitle("Error while saving");
					window.AddText("Could not save file");
				}
				else
				{
					var label = window.AddText();
					label.text = "Room saved";
					var buttonPanel = window.AddButtonPanel();
					buttonPanel.buttons = UIButtonPanel.DialogButton.OK;
				}
			}
			else
			{
				window.SetTitle("Room already exists");
				window.AddText($"Room with same name already exists. Overwrite or save as alternate?");
				var checkBox = (UICheckBox)window.AddUIControl(UIControlType.CheckBox);
				checkBox.AddListener(OnOverwriteToggled);

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

				var inputField = window.AddInputField();
				inputFieldText = inputField.GetComponent<TMP_InputField>();
				inputField.SetText("Enter new room name", altName);
				var buttonPanel = window.AddButtonPanel();
				buttonPanel.buttons = UIButtonPanel.DialogButton.OKCancel;
				window.OnClose += SaveRenamePanelClosed;
			}

			window.Show(Vector2.zero);
		}

		private void SaveRenamePanelClosed(MagicWindow panel)
		{
			if (panel.result == DialogResult.OK)
			{
				if (!inputFieldText.interactable)
				{ // overwrite
					var savedRoom = GetSerializableData();
					RoomSerializer.SaveRoom(savedRoom, savedRoom.roomLabel);
				}
				else
				{
					SetRoomName(inputFieldText.text);
					ShowSaveRoomDialog();
				}
			}
		}

		private void OnOverwriteToggled(UICheckBox checkbox, bool isOn)
		{
			inputFieldText.interactable = !isOn;
		}



		private TMP_InputField inputFieldText;

		public void ShowRenameDialog()
		{
			var window = DesignManager.GetMagicWindow();
			window.designObject.isModal = true;
			var inputField = window.AddInputField();
			inputFieldText = inputField.GetComponent<TMP_InputField>();
			inputField.SetText("Enter new room name", roomLabel.text);
			var buttonPanel = window.AddButtonPanel();
			buttonPanel.buttons = UIButtonPanel.DialogButton.OKCancel;
			window.SetTitle("Enter room name");

			window.OnClose += RenameDialogClosed;

			window.Show(Vector2.zero);
		}

		private void RenameDialogClosed(MagicWindow panel)
		{
			if (panel.result == DialogResult.OK)
			{
				SetRoomName(inputFieldText.text);
			}

			inputFieldText = null;
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
}