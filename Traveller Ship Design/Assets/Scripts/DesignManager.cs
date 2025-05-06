using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static CustomCursor;


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
	public static UIDesignObject GetPrefab(UIPrefabType prefabType)
	{
		return instance.uiPrefabs[prefabType];
	}

	public static UIDesignObject GetUIPrefab(UIPrefabType prefabType)
	{
		return instance.uiPrefabs[prefabType];
	}



	public enum PrefabType
	{
		WallSegmentPrefab,
		WallControlPointPrefab,
		WallSegmentColliderPrefab,
		DoorPrefab,
		RoomPrefab,
	}

	public enum UIPrefabType
	{
		DynamicPanel,
		BoltedButton,
		MenuItemButton,
		MenuDivider,
		TextBlock,
		TextInputField,
		ButtonPanel,
	}

	[UDictionary.Split(50, 50)]
	[SerializeField] private UDictionary<PrefabType, DesignObject> prefabs;
	[UDictionary.Split(50, 50)]
	[SerializeField] private UDictionary<UIPrefabType, UIDesignObject> uiPrefabs;

	public ToolTip toolTip;
	[SerializeField] private Canvas uiCanvas;
	//[SerializeField] private UIPanel contextMenu;
	[SerializeField] private GameObject objectPicker;
	[SerializeField] private CustomCursor cursor;
	[SerializeField] private GameObject linePointIndicator;

	[SerializeField] private LayerMask designObjectLayerMask;
	[SerializeField] private LayerMask controlPointLayerMask;
	[SerializeField] private LayerMask wallSegmentColliderLayerMask;

	[SerializeField] private int minZoom = -2;
	[SerializeField] private int maxZoom = -30;
	/// <summary>
	///  scrolling feels weird when the grid doesn't look like it's moving
	/// </summary>
	[Tooltip("Scrolling feels weird when the grid doesn't look like it's moving, so don't use a value of 1.")]
	[Range(0.2f, 3.0f)]
	[SerializeField] private float scrollMultiplier = 1.1f;

	[SerializeField] List<string> noContextTips;
	private int nextTip = -1;

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


	public static string wallSegmentColliderTag = "WallSegmentCollider";
	private Camera mainCamera;
	[SerializeField] private Camera uiCamera;
	private LayerMask uiLayerIndex;
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
	private UIDesignObject uiHoverObject;
	/// <summary>
	/// serialized for debugging
	/// </summary>
	[SerializeField]
	private UIDesignObject selectedUIObject;


	void Start()
	{
		mainCamera = Camera.main;
		uiLayerIndex = LayerMask.NameToLayer("UI");
		CustomCursor.SetCursor(CursorSpriteMode.Default);

		_instance = this;
	}

	public static Vector3 GetMouseWorldPos()
	{
		var mousePos = Input.mousePosition;
		mousePos.z = -Camera.main.transform.position.z;
		var result = Camera.main.ScreenToWorldPoint(mousePos);
		result.z = 0;
		return result;
	}

	public void SnapToGridToggle(bool snapEnabled)
	{
		snapToGrid = snapEnabled;
	}


	public void StartCreateWallSegmentMode()
	{
		if (selectedUIObject != null)
			return; // I don't like doing this. Would prefer to lock all UI from being usable except for the currently selected object.
		var mouseWorldPos = GetMouseWorldPos();
		var newCntrlPnt = Instantiate(prefabs[PrefabType.WallControlPointPrefab], mouseWorldPos, Quaternion.identity);
		var designObject = newCntrlPnt.GetComponent<DesignObject>();
		designObject.Clicked(mouseWorldPos, KeyInput.None, ref selectedObject, ref editMode);
		SetEditMode(EditMode.CreateObject);
	}

	public void StartCreateDoor()
	{
		if (selectedUIObject != null)
			return; // I don't like doing this. Would prefer to lock all UI from being usable except for the currently selected object.
		var mouseWorldPos = GetMouseWorldPos();
		var door = Instantiate(prefabs[PrefabType.DoorPrefab], mouseWorldPos, Quaternion.identity);
		var designObject = door.GetComponent<DesignObject>();
		designObject.Clicked(mouseWorldPos, KeyInput.None, ref selectedObject, ref editMode);
		SetEditMode(EditMode.CreateObject);
	}

	private void ToggleUI(bool enableUI)
	{
		//uiCanvas.enabled = enableUI;
		objectPicker.SetActive(enableUI);
	}


	private void ToggleUIMode(bool enableUIMode)
	{
		isUIUpdate = enableUIMode;
		if (enableUIMode)
		{
			CustomCursor.SetCursor(CursorSpriteMode.UI, enableUIMode);
		}
		else
		{
			CustomCursor.SetCursor(CursorSpriteMode.Default);
			if (selectedUIObject != null)
				throw new Exception("Let's make sure to clean up selected UI objects before leaving UI mode");
		}
	}

	void Update()
	{
		Rect screenRect = new Rect(0, 0, Screen.width, Screen.height);
		if (!screenRect.Contains(Input.mousePosition))
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
		if (selectedUIObject != null)
		{   // must wait for blocking dialog (or whatever) to close before anything else can happen
			var keyInput = GetKeyInput();
			if ((keyInput & KeyInput.Esc) == KeyInput.Esc || Input.GetMouseButtonDown(1))
			{
				DeselectUIObject();
				ToggleUIMode(false);
			}

			return;
		}

		if (IsPointerOverUIElement(GetEventSystemRaycastResults(Input.mousePosition),
			out UIDesignObject mouseOverUIObject))
		{
		}
		else
		{
			ToggleUIMode(false);
			return;
		}

		//Vector3 worldPos = GetMouseWorldPos();
		// more ui stuff?

	}


	void GridUpdate()
	{
		var uiHits = GetEventSystemRaycastResults(Input.mousePosition);
		if (IsPointerOverUIElement(uiHits, out UIDesignObject mouserOverUIObject))
		{
			uiHoverObject = mouserOverUIObject;
			ToggleUIMode(true);
			UIUpdate();
			return;
		}

		Vector3 mouseWorldPos = GetMouseWorldPos();
		KeyInput keyInput = GetKeyInput();

		if (Input.mouseScrollDelta != Vector2.zero)
		{
			if ((keyInput & KeyInput.Ctrl) == KeyInput.Ctrl)
			{
				var newY = mainCamera.transform.position.y + Input.mouseScrollDelta.y * scrollMultiplier;
				mainCamera.transform.position = new Vector3(
					mainCamera.transform.position.x, newY, mainCamera.transform.position.z);
			}
			else if ((keyInput & KeyInput.Ctrl) == KeyInput.Shift)
			{
				var newX = mainCamera.transform.position.x + Input.mouseScrollDelta.y * scrollMultiplier;
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

				CustomCursor.SetCursor(CursorSpriteMode.Default);
			}
		}

		var nonSnappedPos = mouseWorldPos;
		if (selectedObject != null && selectedObject.IsDragging())
		{
			if (snapToGrid)
			{
				if ((keyInput & KeyInput.Ctrl) != KeyInput.Ctrl)
				{
					mouseWorldPos = selectedObject.SnapToGrid(mouseWorldPos);
				}
			}
			else
			{
				if ((keyInput & KeyInput.Ctrl) == KeyInput.Ctrl)
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
						hoverObject.Clicked(mouseWorldPos, keyInput, ref selectedObject, ref editMode);
						SetEditMode(editMode);
					}
				}
				else if (Input.GetMouseButton(1))
				{
					if (hoverObject != null)
					{
						var contextMenu = hoverObject.GetContextMenu(GetUICoordinatesFromMousePos());
						if (contextMenu != null)
						{
							if (selectedUIObject != null)
								Debug.LogError("This shouldn't happen, right?");
							AddToCanvas(contextMenu.transform);
							selectedUIObject = contextMenu;
							ToggleUIMode(true);
						}
						// else DeselectObject()?
					}
					else
					{
						DeselectObject();
					}
				}
				else if (keyInput == KeyInput.Esc)
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
					mouseWorldPos = GetMouseWorldPos();
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
							selectedObject.Clicked(mouseWorldPos, keyInput, ref selectedObject, ref editMode);
						}
						else
						{
							//selectedObject = null;
						}

						SetEditMode(newEditMode);
					}
				}
				else if (Input.GetMouseButtonDown(1) || keyInput == KeyInput.Esc)
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

	public static Vector2 GetUICoordinatesFromMousePos()
	{
		return instance._GetUICoordinatesFromMousePos();
	}

	private Vector2 _GetUICoordinatesFromMousePos()
	{
		Vector2 mousePos = Input.mousePosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			uiCanvas.GetComponent<RectTransform>(), mousePos, uiCamera, out Vector2 uiPos);
		return uiPos;
	}

	private void DeselectUIObject()
	{
		if (selectedUIObject != null)
		{
			selectedUIObject.Deselect();
			Destroy(selectedUIObject.gameObject);
		}

		selectedUIObject = null;
		if (selectedObject != null)
			toolTip.SetToolTip(selectedObject.tooltip);
		else
			toolTip.SetToolTip(NextTip());
	}

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

	public enum KeyInput
	{
		None = 0x0,
		Ctrl = 0x1,
		Alt = 0x2,
		Esc = 0x4,
		Shift = 0x8,
	}

	private KeyInput GetKeyInput()
	{
		KeyInput input = KeyInput.None;

		if (Input.GetKey(KeyCode.Escape))
			return KeyInput.Esc;
		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			input |= KeyInput.Ctrl;
		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
			input |= KeyInput.Alt;
		if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
			input |= KeyInput.Shift;
		return input;
	}

	private void SetEditMode(EditMode newEditMode)
	{
		editMode = newEditMode;
		ToggleUI(editMode == EditMode.None);
	}

	private void StartScroll(Vector3 worldPos)
	{
		preScrollEditMode = editMode;
		SetEditMode(EditMode.Scrolling);
		CustomCursor.SetCursor(CursorSpriteMode.Scroll);
		scrollStartPos = worldPos;
	}


	private void EndScroll()
	{
		SetEditMode(preScrollEditMode);
		CustomCursor.SetCursor(CursorSpriteMode.Default);
	}



	private bool OverlapsInteractable(DesignObject selectedObject, out List<IInteractable> overlappedObjects)
	{
		var collider = selectedObject.GetComponent<Collider2D>();
		var results = new List<Collider2D>();
		if (collider.Overlap(results) == 0)
		{
			overlappedObjects = null;
			return false;
		}

		overlappedObjects = new List<IInteractable>();
		foreach (var result in results)
		{
			var dObj = result.GetComponent<IInteractable>();
			if (dObj != null)
			{
				if (result.transform.parent != selectedObject.transform.parent)
					overlappedObjects.Add(dObj);
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


	private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults,
		out UIDesignObject mouserOverUIObject)
	{
		for (int index = 0; index < eventSystemRaycastResults.Count; index++)
		{
			RaycastResult curRaysastResult = eventSystemRaycastResults[index];
			if (curRaysastResult.gameObject.layer == uiLayerIndex)
			{
				mouserOverUIObject = curRaysastResult.gameObject.GetComponent<UIDesignObject>();
				return true;
			}
		}

		mouserOverUIObject = null;
		return false;
	}

	static List<RaycastResult> GetEventSystemRaycastResults(Vector3 screenPos)
	{
		PointerEventData eventData = new PointerEventData(EventSystem.current);
		eventData.position = screenPos;
		List<RaycastResult> raysastResults = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, raysastResults);
		return raysastResults;
	}

	public static void ShowDialog(UIDesignObject dialog)
	{
		instance._ShowDialog(dialog);
	}

	private void _ShowDialog(UIDesignObject dialog)
	{
		AddToCanvas(dialog.transform);

		selectedUIObject = dialog;
		ToggleUIMode(true);
	}

	private void AddToCanvas(Transform transform)
	{
		transform.SetParent(uiCanvas.transform, false);
	}

	public static void CloseDialog(RectTransform dialogRect)
	{
		instance._CloseDialog(dialogRect);
	}

	private void _CloseDialog(RectTransform dialogRect)
	{
		selectedUIObject = null;
		Destroy(dialogRect.gameObject);
		ToggleUIMode(false);
	}
}
