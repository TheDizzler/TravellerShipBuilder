using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.UI;
using UnityEngine;
using UnityEngine.EventSystems;

using static AtomosZ.Keyboard;
using static AtomosZ.MG2eTraveller.Ship.DesignerCustomCursor;
using static AtomosZ.UI.MagicWindowBase;
using static AtomosZ.UI.UIPrefabProvider;

namespace AtomosZ.MG2eTraveller.Ship
{
	public class DesignManager : MonoBehaviour
	{
		public static DesignManager instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<DesignManager>();
				return _instance;
			}
		}

		private static DesignManager _instance;

		public void ContextMenuCallback(EditMode newEditMode)
		{
			editMode = newEditMode;
		}

		public static DesignObject GetPrefab(PrefabType prefabType)
		{
			return instance.prefabs[prefabType];
		}

		public static MagicContextMenu GetContextMenu()
		{
			return instance._GetContextMenu();
		}

		private MagicContextMenu _GetContextMenu()
		{
			var window = (MagicContextMenu)GetMagicUIControl(UIPrefabType.MagicContextMenu, uiCanvas.transform);
			return window;
		}

		public static MagicWindow GetMagicWindow()
		{
			return instance._GetMagicWindow();
		}

		private MagicWindow _GetMagicWindow()
		{
			var window = (MagicWindow)GetMagicUIControl(UIPrefabType.MagicWindow, uiCanvas.transform);
			return window;
		}

		public static Camera GetScreenshotCamera()
		{
			return instance.screenshotCamera;
		}


		private void AddToCanvas(Transform transform)
		{
			transform.SetParent(uiCanvas.transform, false);
		}

		public enum PrefabType
		{
			WallSegmentPrefab,
			WallControlPointPrefab,
			WallSegmentColliderPrefab,
			DoorPrefab,
			RoomPrefab,
		}


		[UDictionary.Split(50, 50)]
		[SerializeField] private UDictionary<PrefabType, DesignObject> prefabs;

		public ToolTip toolTip;
		[SerializeField] private Canvas uiCanvas;
		[SerializeField] private UIInput uiInput;
		[SerializeField] private GameObject objectPicker;
		[SerializeField] private MagicWindow roomGeomorphTab;
		[SerializeField] private GameObject cursorPrefab;
		[SerializeField] private DesignerCustomCursor cursor;
		[SerializeField] private GameObject linePointIndicator;

		[SerializeField] private LayerMask designObjectLayerMask;
		[SerializeField] private LayerMask controlPointLayerMask;
		[SerializeField] private LayerMask wallSegmentColliderLayerMask;

		[SerializeField] private int minZoom = -2;
		[SerializeField] private int maxZoom = -30;
		[Tooltip("Scrolling feels weird when the grid doesn't look like it's moving, so don't use a value of 1.")]
		[Range(0.2f, 3.0f)]
		[SerializeField] private float zoomMultiplier = 1.1f;

		[SerializeField] private List<string> noContextTips;
		[SerializeField]
		private List<List<string>> moveObjectToolTips = new List<List<string>>
		{
			new List<string> {"Hold ctrl to disable grid snap." },
			new List<string> {"Hold ctrl to enable grid snap." }
		};

		private int nextTip = -1;

		public static TagHandle blockerTag;
		public static TagHandle wallSegmentColliderTag;

		public enum EditMode
		{
			None,
			/// <summary>
			/// Moving the map around.
			/// </summary>
			Scrolling,
			/// <summary>
			/// And object is selected and it's being dragged around the map.
			/// </summary>
			MoveObject,
			/// <summary>
			/// Creating an object. Probably initiated from pushing a button.
			/// </summary>
			CreateObject,
		}



		/// <summary>
		/// serialized for debugging
		/// </summary>
		public EditMode editMode = EditMode.None;
		private EditMode preScrollEditMode;

		private Vector3 scrollStartPos;

		public static bool snapToGrid = true;


		private Camera mainCamera;
		[SerializeField] private Camera uiCamera;
		[SerializeField] private Camera screenshotCamera;
		[SerializeField] private Transform screenshotLayer;
		private LayerMask uiLayerIndex;
		/// <summary>
		/// serialized for debugging
		/// </summary>
		[SerializeField]
		private bool isUIUpdate = false;

		/// <summary>
		/// serialized for debugging
		/// </summary>
		[SerializeField]
		private DesignObject hoverObject;
		/// <summary>
		/// serialized for debugging
		/// </summary>
		[SerializeField]
		private DesignObject selectedObject;
		/// <summary>
		/// serialized for debugging
		/// </summary>
		[SerializeField]
		private UIMonoBehaviour uiHoverObject;
		/// <summary>
		/// serialized for debugging
		/// </summary>
		[SerializeField]
		private LinkedList<MagicWindowBase> dialogStack = new();

		void Awake()
		{
			blockerTag = TagHandle.GetExistingTag("ClickBlocker");
			wallSegmentColliderTag = TagHandle.GetExistingTag("WallSegmentCollider");
		}

		void Start()
		{
			mainCamera = Camera.main;
			uiLayerIndex = LayerMask.NameToLayer("UI");
			blockerTag = TagHandle.GetExistingTag("ClickBlocker");
			cursor = Instantiate(cursorPrefab).GetComponent<DesignerCustomCursor>();
			DesignerCustomCursor.SetCursor(CursorSpriteMode.Default);

			_instance = this;
		}



#if UNITY_EDITOR
		public void Test()
		{
			MakeModalPanel(null);
		}

		int panelCount = 1;
		public void MakeModalPanel(UIButton sender)
		{
			var window = GetMagicWindow();
			Debug.LogException(new Exception("Because of how we rearranged the UIControl stuff, we need to re-implement modal windows!"));
			//window.isModal = true;
			var label = window.panel.AddText(null);
			label.text = "Panel " + panelCount++;
			var button = (UIButton)window.AddUIControl(UIControlType.Button);
			button.text = "Modal Panel";
			button.AddListener(MakeModalPanel);
			button = (UIButton)window.AddUIControl(UIControlType.Button);
			button.text = "Non Modal Panel";
			button.AddListener(MakeNonModalPanel);
			window.SetTitle("Modal test");
			window.Show(Vector2.zero);
		}

		public void MakeNonModalPanel(UIButton sender)
		{
			var window = DesignManager.GetMagicWindow();
			var label = (UIExpandingLabel)window.AddUIControl(UIControlType.Text);
			label.text = "Panel " + panelCount++;
			var button = (UIButton)window.AddUIControl(UIControlType.Button);
			button.text = "Modal Panel";
			button.AddListener(MakeModalPanel);
			button = (UIButton)window.AddUIControl(UIControlType.Button);
			button.text = "Non Modal Panel";
			button.AddListener(MakeNonModalPanel);
			window.SetTitle("Non Modal test");
			window.Show(Vector2.zero);
		}
#endif



		public void SnapToGridToggle()
		{
			snapToGrid = !snapToGrid;
		}

		public void ShowRoomGeomorphs()
		{
			Debug.LogError("ImageViewPanel has not yet been implemented");
			//var serializedRooms = RoomSerializer.GetRoomGeomorphs();
			//if (roomGeomorphTab == null)
			//{
			//	var prefab = (UIDesignObject)AssetDatabase.LoadAssetAtPath($"Assets/Prefabs/UIPrefabs/GeomorphRoomDisplay.prefab", typeof(UIDesignObject));
			//	roomGeomorphTab = Instantiate(prefab, uiCanvas.transform).GetComponent<MagicWindow>();
			//}

			//roomGeomorphTab.gameObject.SetActive(true);
			//var imagePanel = roomGeomorphTab.GetComponentInChildren<UIImageViewPanel>();
			//imagePanel.ClearImages();
			//var screenshotLayer = LayerMask.NameToLayer("Geomorph Screenshot");
			//foreach (var room in serializedRooms)
			//{
			//	Sprite sprite = RoomSerializer.CreateSpriteOfGeomorph(room, screenshotLayer);
			//	imagePanel.AddImage(sprite, room.roomLabel, () => DesignManager.instance.StartCreateRoomFromSerializedData(room));
			//}
		}



		public void StartCreateWallSegmentMode()
		{
			var mouseWorldPos = Helpers.GetMouseWorldPos();
			var newCntrlPnt = Instantiate(prefabs[PrefabType.WallControlPointPrefab], mouseWorldPos, Quaternion.identity);
			var designObject = newCntrlPnt.GetComponent<DesignObject>();
			designObject.Clicked(mouseWorldPos, ModifierKey.None, ref selectedObject, ref editMode);
			SetEditMode(EditMode.CreateObject);
			isUIUpdate = true;  // we want to consume a frame of input before switching out of UI mode
		}

		public void StartCreateRoomFromSerializedData(RoomSerializer.SerializedRoom roomData)
		{
			var room = Room.CreateFromSerializedData(roomData);
			var mouseWorldPos = Helpers.GetMouseWorldPos();
			room.Clicked(mouseWorldPos, ModifierKey.None, ref selectedObject, ref editMode);
			SetEditMode(EditMode.CreateObject);
			isUIUpdate = true;  // we want to consume a frame of input before switching out of UI mode
		}

		public void StartCreateDoor()
		{
			var mouseWorldPos = Helpers.GetMouseWorldPos();
			var door = Instantiate(prefabs[PrefabType.DoorPrefab], mouseWorldPos, Quaternion.identity);
			var designObject = door.GetComponent<DesignObject>();
			designObject.Clicked(mouseWorldPos, ModifierKey.None, ref selectedObject, ref editMode);
			SetEditMode(EditMode.CreateObject);
			isUIUpdate = true;  // we want to consume a frame of input before switching out of UI mode
		}

		private void ToggleUI(bool enableUI)
		{
			//uiCanvas.enabled = enableUI;
			objectPicker.Show();
		}


		private void ToggleUIMode(bool enableUIMode, CursorSpriteMode newSpriteMode)
		{
			isUIUpdate = enableUIMode;
			if (enableUIMode)
			{
				if (hoverObject != null)
				{
					hoverObject.SetHover(false);
					hoverObject = null;
				}

				DesignerCustomCursor.SetCursor(newSpriteMode, enableUIMode);
			}
			else
			{
				DesignerCustomCursor.SetCursor(newSpriteMode);
			}
		}

		public UIExpandingLabel inputCoordsLabel;
		public UIExpandingLabel gridCoordsLabel;
		public UIExpandingLabel mainCamUICoordsLabel;
		public UIExpandingLabel uiCoordsLabel;
		public UIExpandingLabel viewportCoordsLabel;

		void Update()
		{
			var mousePos = Mouse.pos;
#if UNITY_EDITOR
			// input
			inputCoordsLabel.text = $"x:{mousePos.x.ToString("000.0")} y:{mousePos.y.ToString("000.0")}";

			// grid
			Vector3 mouseWorldPos = Helpers.GetMouseWorldPos();
			gridCoordsLabel.text = $"x:{mouseWorldPos.x.ToString("000.0")} y:{mouseWorldPos.y.ToString("000.0")}";

			// ui cam
			var uiCoords = uiInput.GetUICoordinatesFromMousePos();
			uiCoordsLabel.text = $"x:{uiCoords.x.ToString("000.0")} y:{uiCoords.y.ToString("000.0")}";

			// main cam
			var manCamUICoords = uiInput.GetMainCameraUICoordinatesFromMousePos();
			mainCamUICoordsLabel.text = $"x:{manCamUICoords.x.ToString("000.0")} y:{manCamUICoords.y.ToString("000.0")}";

			// viewport coords
			var viewPortCoords = Helpers.camera.WorldToViewportPoint(mouseWorldPos);
			viewportCoordsLabel.text = $"x:{viewPortCoords.x.ToString("0.0000")} y:{viewPortCoords.y.ToString("0.0000")}";



			if (Mouse.GetMouseButtonDown(1))
			{
				var contextMenu = DesignManager.GetContextMenu();
				contextMenu.Show(manCamUICoords);
			}
#endif

			Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
			if (!screenRect.Contains(mousePos))
				return;

			if (isUIUpdate)
			{
				UIUpdate();
			}
			else
			{
				GridUpdate();
			}
		}




		void UIUpdate()
		{
			if (editMode == EditMode.CreateObject)
			{ // we want to consume a frame of input before switching out of UI mode
				isUIUpdate = false;
				return;
			}

			// is there a topmost panelRect?
			// is the topmost panelRect busy with something (i.e. dragging)
			//		this is to prevent fast dragging movements from flickering the mousecursor
			// 

			MagicWindowBase topDialog = null;
			if (dialogStack.Count != 0)
			{
				topDialog = dialogStack.Last.Value;
				if (topDialog.isDragging)
					return;
			} // else we are in a "temporary" ui state

			var isHoveringUI = IsPointerOverUIObject(
				Helpers.GetUIRaycasts(), out UIMonoBehaviour mouseOverUIObject);

			ModifierKey modifierKeys = GetModifierKeyInput();
			if (topDialog != null)
			{
				if (topDialog.Input(modifierKeys))
					return;
				if (topDialog.isDragging)
					return;

				if (/*!topDialog.designObject.isModal && */!isHoveringUI)
				{
					DesignerCustomCursor.SetCursor(CursorSpriteMode.Default);
					ToggleUIMode(false, CursorSpriteMode.Default);
					return;
				}

				if (topDialog.dataType == UIControlType.ContextMenu &&
					Mouse.GetMouseButtonDown(1))
				{
					topDialog.Close();
					return;
				}

				//if (isHoveringUI)
				//{
				//	topDialog.designObject.UpdateHover(Input.mousePosition);
				//}
			}
			else if (isHoveringUI)
			{
				//DesignerCustomCursor.SetCursor(mouseOverUIObject.hoverCursorMode);
				DesignerCustomCursor.SetCursor(CursorSpriteMode.UI_Default);
			}
			else if (!isHoveringUI)
			{
				ToggleUIMode(false, CursorSpriteMode.Default);
				return;
			}
			else
				Debug.Log("Does this ever happen?");
		}


		void GridUpdate()
		{
			//if(editMode != EditMode.CreateObject)
			var uiHits = Helpers.GetUIRaycasts();
			if (IsPointerOverUIObject(uiHits, out UIMonoBehaviour mouserOverUIObject))
			{
				uiHoverObject = mouserOverUIObject;
				//ToggleUIMode(true, uiHoverObject.hoverCursorMode);
				ToggleUIMode(true, DesignerCustomCursor.CursorSpriteMode.UI_Default);
				UIUpdate();
				return;
			}

			Vector3 mouseWorldPos = Helpers.GetMouseWorldPos();
			ModifierKey modifierKeys = GetModifierKeyInput();
			if (dialogStack.Count != 0)
			{
				var topDialog = dialogStack.Last.Value;
				//if (topDialog.designObject.isModal)
				//return;
				if (topDialog.dataType == UIControlType.ContextMenu)
				{
					if ((modifierKeys & ModifierKey.Esc) == ModifierKey.Esc
						|| Mouse.GetMouseButtonDown(0) || Mouse.GetMouseButtonDown(1))
					{
						topDialog.Close();
						return;
					}
				}
			}

			if (Mouse.scrollDelta != Vector2.zero)
			{
				if ((modifierKeys & ModifierKey.Ctrl) == ModifierKey.Ctrl)
				{
					var newY = mainCamera.transform.position.y + Input.mouseScrollDelta.y * zoomMultiplier;
					mainCamera.transform.position = new Vector3(
						mainCamera.transform.position.x, newY, mainCamera.transform.position.z);
				}
				else if ((modifierKeys & ModifierKey.Shift) == ModifierKey.Shift)
				{
					var newX = mainCamera.transform.position.x + Input.mouseScrollDelta.y * zoomMultiplier;
					mainCamera.transform.position = new Vector3(
						newX, mainCamera.transform.position.y, mainCamera.transform.position.z);
				}
				else
				{
					var newZ = mainCamera.transform.position.z + Input.mouseScrollDelta.y;
					if (newZ >= minZoom)
						newZ = minZoom;
					else if (newZ < maxZoom)
						newZ = maxZoom;
					mainCamera.transform.position = new Vector3(
						mainCamera.transform.position.x, mainCamera.transform.position.y, newZ);
				}
			}

			{
				if (CheckForObject(mouseWorldPos, out DesignObject mouseOverObject))
				{
					if (mouseOverObject == hoverObject)
					{
						hoverObject.UpdateHover(mouseWorldPos);
					}
					else
					{
						if (hoverObject != null)
						{
							hoverObject.SetHover(false);
							if (selectedObject != null && selectedObject.IsDragging())
							{   // door may have been locked to wall and needs to be reset
								selectedObject.EndInteraction();
							}
						}

						hoverObject = mouseOverObject;
						hoverObject.SetHover(true);
					}
				}
				else if (hoverObject != null)
				{
					hoverObject.SetHover(false);
					hoverObject = null;
					if (selectedObject != null)
					{
						selectedObject.Select();
						if (selectedObject.IsDragging())
						{   // door may have been locked to wall and needs to be reset
							selectedObject.EndInteraction();
						}
					}

					DesignerCustomCursor.SetCursor(CursorSpriteMode.Default);
				}
			}

			//var nonSnappedPos = mouseWorldPos;
			if (selectedObject != null && selectedObject.IsDragging())
			{
				if (snapToGrid)
				{
					if ((modifierKeys & ModifierKey.Ctrl) != ModifierKey.Ctrl)
					{
						mouseWorldPos = selectedObject.SnapToGrid(mouseWorldPos);
					}
				}
				else
				{
					if ((modifierKeys & ModifierKey.Ctrl) == ModifierKey.Ctrl)
					{
						mouseWorldPos = selectedObject.SnapToGrid(mouseWorldPos);
					}
				}
			}

			switch (editMode)
			{
				case EditMode.None:
				{
					if (Input.GetMouseButtonDown(0))
					{
						if (hoverObject == null)
						{   // scroll map
							StartScroll(mouseWorldPos);
						}
						else
						{
							hoverObject.Clicked(mouseWorldPos, modifierKeys, ref selectedObject, ref editMode);
							SetEditMode(editMode);
						}
					}
					else if (Input.GetMouseButtonDown(1))
					{
						if (hoverObject != null)
						{
							var contextMenu = hoverObject.GetContextMenu(uiInput.GetUICoordinatesFromMousePos());
						}
						else
						{
							DeselectObject();
						}
					}
					else if (modifierKeys == ModifierKey.Esc)
					{
						DeselectObject();
					}
				}
				break;


				case EditMode.Scrolling:
				{
					if (Input.GetMouseButtonUp(0))
					{
						EndScroll();
					}
					else
					{
						var diff = mouseWorldPos - scrollStartPos;
						var newX = mainCamera.transform.position.x - diff.x;
						var newY = mainCamera.transform.position.y - diff.y;
						mainCamera.transform.position = new Vector3(newX, newY, mainCamera.transform.position.z);
						mouseWorldPos = Helpers.GetMouseWorldPos();
						scrollStartPos = mouseWorldPos;
					}
				}
				break;

				case EditMode.CreateObject:
				case EditMode.MoveObject:
				{
					if (Input.GetMouseButtonUp(0))
					{
						selectedObject.EndDrag(mouseWorldPos);
						if (editMode == EditMode.MoveObject)
						{
							selectedObject = selectedObject.Select();
							SetEditMode(EditMode.None);
						}
						else
						{
							var newEditMode = selectedObject.Create(mouseWorldPos, out DesignObject createdObject);

							if (selectedObject != createdObject)
							{
								Destroy(selectedObject.gameObject); // this SHOULD be just a temporary cursor object/icon.
								selectedObject = createdObject;
								selectedObject.Clicked(mouseWorldPos, modifierKeys, ref selectedObject, ref editMode);
							}
							else
							{
								//selectedObject = null;
							}

							SetEditMode(newEditMode);
						}
					}
					else if (Input.GetMouseButtonDown(1) || modifierKeys == ModifierKey.Esc)
					{
						if (editMode == EditMode.CreateObject)
						{
							Destroy(selectedObject.gameObject);
						}
						else
						{
							selectedObject.ResetToLastPosition();
						}

						DeselectObject();
						SetEditMode(EditMode.None);
					}
					else
					{
						selectedObject.MouseDrag(mouseWorldPos);
						if (hoverObject != null)
						{
							selectedObject.Interact(hoverObject);
						}
					}
				}
				break;
			}
		}

		//public static Vector2 GetUICoordinatesFromMousePos()
		//{
		//	return instance._GetUICoordinatesFromMousePos();
		//}

		//private Vector2 _GetUICoordinatesFromMousePos()
		//{
		//	Vector2 mousePos = Input.mousePosition;
		//	RectTransformUtility.ScreenPointToLocalPointInRectangle(
		//		uiCanvas.GetComponent<RectTransform>(), mousePos, uiCamera, out Vector2 uiPos);
		//	return uiPos;
		//}

		//private void DeselectUIObject()
		//{
		//	if (selectedUIObject != null)
		//	{
		//		selectedUIObject.Deselect();
		//	}

		//	selectedUIObject = null;
		//	if (selectedObject != null)
		//		toolTip.SetToolTip(selectedObject.tooltip);
		//	else
		//		toolTip.SetToolTip(NextTip());
		//}

		private void DeselectObject()
		{
			if (selectedObject != null)
				selectedObject.Deselect();
			selectedObject = null;
			toolTip.SetToolTip(NextTip());
		}


		private List<string> NextTip()
		{
			if (++nextTip >= noContextTips.Count)
				nextTip = 0;
			return new List<string> { noContextTips[nextTip] };
		}

		//public enum ModifierKey
		//{
		//	None = 0x0,
		//	Ctrl = 0x1,
		//	Alt = 0x2,
		//	Esc = 0x4,
		//	Shift = 0x8,
		//}

		//private ModifierKey GetModifierKeyInput()
		//{
		//	ModifierKey input = ModifierKey.None;

		//	if (Input.GetKeyDown(KeyCode.Escape))
		//		return ModifierKey.Esc;
		//	if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
		//		input |= ModifierKey.Ctrl;
		//	if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
		//		input |= ModifierKey.Alt;
		//	if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
		//		input |= ModifierKey.Shift;
		//	return input;
		//}

		private void SetEditMode(EditMode newEditMode)
		{
			editMode = newEditMode;
			ToggleUI(editMode == EditMode.None);
			switch (editMode)
			{
				case EditMode.MoveObject:
				case EditMode.CreateObject:
				{
					toolTip.SetToolTip(moveObjectToolTips[snapToGrid ? 0 : 1]);
				}
				break;
			}
		}

		private void StartScroll(Vector3 worldPos)
		{
			preScrollEditMode = editMode;
			SetEditMode(EditMode.Scrolling);
			DesignerCustomCursor.SetCursor(CursorSpriteMode.Scroll);
			scrollStartPos = worldPos;
		}


		private void EndScroll()
		{
			SetEditMode(preScrollEditMode);
			DesignerCustomCursor.SetCursor(CursorSpriteMode.Default);
		}



		private bool OverlapsInteractable(DesignObject selectedObject, out List<IDesignBehavior> overlappedObjects)
		{
			var collider = selectedObject.GetComponent<Collider2D>();
			var results = new List<Collider2D>();
			if (collider.Overlap(results) == 0)
			{
				overlappedObjects = null;
				return false;
			}

			overlappedObjects = new List<IDesignBehavior>();
			foreach (var result in results)
			{
				var dBehave = result.GetComponent<IDesignBehavior>();
				if (dBehave != null)
				{
					var dObj = result.GetComponent<DesignObject>();
					if (dObj.isInteractable && result.transform.parent != selectedObject.transform.parent)
						overlappedObjects.Add(dBehave);
				}
			}

			return overlappedObjects.Count > 0;
		}

		private bool Overlaps(DesignObject selectedObject, out List<DesignObject> overlappedObjects)
		{
			var collider = selectedObject.GetComponent<Collider2D>();
			var results = new List<Collider2D>();
			if (collider.Overlap(results) == 0)
			{
				overlappedObjects = null;
				return false;
			}

			overlappedObjects = new List<DesignObject>();
			foreach (var result in results)
			{
				var dObj = result.GetComponent<DesignObject>();
				if (dObj != null)
					overlappedObjects.Add(dObj);
			}

			return overlappedObjects.Count > 0;
		}

		/// <summary>
		/// Ignores the currently manipulating object (selectedObject.IsDragging()).
		/// </summary>
		/// <param name="worldPos"></param>
		/// <param name="hitObject"></param>
		/// <returns></returns>
		private bool CheckForObject(Vector2 worldPos, out DesignObject hitObject)
		{
			RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero, 10.0f, designObjectLayerMask);

			if (hits.Length > 0)
			{
				Collider2D currentHit = null;
				for (int i = 0; i < hits.Length; ++i)
				{
					var hit = hits[i];
					var hitGO = hit.transform.gameObject;
					if (selectedObject != null)
					{
						if (hitGO == selectedObject.gameObject && selectedObject.IsDragging())
						{
							continue;
						}
					}

					if (currentHit == null
						|| hit.collider.layerOverridePriority > currentHit.layerOverridePriority)
						currentHit = hit.collider;
				}

				if (currentHit != null)
				{
					hitObject = currentHit.transform.GetComponent<DesignObject>();
					return true;
				}
			}

			hitObject = null;
			return false;
		}


		private bool IsPointerOverUIObject(List<RaycastResult> eventSystemRaycastResults,
			out UIMonoBehaviour mouserOverUIObject)
		{
			for (int index = 0; index < eventSystemRaycastResults.Count; index++)
			{
				RaycastResult curRaysastResult = eventSystemRaycastResults[index];
				if (curRaysastResult.gameObject.layer == uiLayerIndex)
				{
					mouserOverUIObject = curRaysastResult.gameObject.GetComponent<UIMonoBehaviour>();
					if (mouserOverUIObject == null)
					{
						//mouserOverUIObject = mouserOverUIObject;
						continue;
					}

					return true;
				}
			}

			mouserOverUIObject = null;
			return false;
		}


		public static void ShowDialog(MagicWindow dialog)
		{
			instance._ShowDialog(dialog);
		}


		private void _ShowDialog(MagicWindow dialog)
		{
			Debug.LogWarning("Because of how we rearranged the UIControl stuff, we need to re-implement modal windows!");
			//if (dialog.designObject.isModal)
			{
				//var blocker = Instantiate(GetUIPrefab(UIPrefabType.ModalClickBlocker));
				//AddToCanvas(blocker.transform);
				//dialog.modalClickBlocker = blocker;
			}

			AddToCanvas(dialog.transform);
			dialogStack.AddLast(dialog);
			ToggleUIMode(true, CursorSpriteMode.UI_Default);
		}


		public static void CloseDialog(MagicWindow panel)
		{
			instance._CloseDialog(panel);
		}

		/// <summary>
		/// Currently destroys all dialogs that get closed.
		/// Change this too object pool?
		/// </summary>
		/// <param name="dialogRect"></param>
		private void _CloseDialog(MagicWindow panel)
		{
			if (!dialogStack.Contains(panel))
			{
				Debug.LogError("Panel isn't in stack???");
			}

			dialogStack.Remove(panel);
			//if (panel.modalClickBlocker != null)
			//{
			//	Destroy(panel.modalClickBlocker.gameObject);
			//}

			Destroy(panel.gameObject);
			// turn off UI mode, so next update will check for another panelRect in the stack.
			// this will prevent any wierd click throughs
			ToggleUIMode(false, CursorSpriteMode.Default);
		}

		public static void ShowErrorDialog(string errorMsg, string titleText = null)
		{
			Debug.LogError(titleText + "\n" + errorMsg);

			var window = DesignManager.GetMagicWindow();
			var buttons = (UIButtonPanel)window.AddUIControl(UIControlType.ButtonPanel);
			buttons.buttons = UIButtonPanel.DialogButton.OKCancel;
			window.showCloseButton = true;
			window.SetTitle(titleText);
			window.AddText(errorMsg);
			window.Show(Vector2.zero);
		}
	}
}