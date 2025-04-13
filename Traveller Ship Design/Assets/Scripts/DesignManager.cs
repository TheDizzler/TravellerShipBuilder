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

	public static RectTransform GetPrefab(UIPrefabType prefabType)
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
	}

	[UDictionary.Split(50, 50)]
	[SerializeField] private UDictionary<PrefabType, DesignObject> prefabs;
	[UDictionary.Split(50, 50)]
	[SerializeField] private UDictionary<UIPrefabType, RectTransform> uiPrefabs;

	public ToolTip toolTip;
	[SerializeField] private Canvas uiCanvas;
	[SerializeField] private UIPanel contextMenu;
	[SerializeField] private CustomCursor cursor;
	[SerializeField] private GameObject linePointIndicator;

	[SerializeField] private LayerMask designObjectLayerMask;
	[SerializeField] private LayerMask controlPointLayerMask;
	[SerializeField] private LayerMask wallSegmentColliderLayerMask;

	[SerializeField] private int minZoom = -2;
	[SerializeField] private int maxZoom = -30;

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
		/// <summary>
		/// The context menu is open.
		/// </summary>
		ContextMenu,
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
	private LayerMask uiLayerIndex;
	private bool isUIUpdate = false;
	/// <summary>
	/// A UI element has focus and must be dealt with before anything else can happen.
	/// </summary>
	private bool isBlockingUI = false;

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
		var mouseWorldPos = GetMouseWorldPos();
		var newCntrlPnt = Instantiate(prefabs[PrefabType.WallControlPointPrefab], mouseWorldPos, Quaternion.identity);
		var designObject = newCntrlPnt.GetComponent<DesignObject>();
		designObject.Clicked(mouseWorldPos, KeyInput.None, ref selectedObject, ref editMode);
		SetEditMode(EditMode.CreateObject);
	}

	public void StartCreateDoor()
	{
		var mouseWorldPos = GetMouseWorldPos();
		var door = Instantiate(prefabs[PrefabType.DoorPrefab], mouseWorldPos, Quaternion.identity);
		var designObject = door.GetComponent<DesignObject>();
		designObject.Clicked(mouseWorldPos, KeyInput.None, ref selectedObject, ref editMode);
		SetEditMode(EditMode.CreateObject);
	}

	private void ToggleUI(bool enableUI)
	{
		uiCanvas.enabled = enableUI;
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
		if (isBlockingUI)
		{   // must wait for blocking dialog (or whatever) to close before anything else can happen
			return;
		}

		if (!IsPointerOverUIElement(GetEventSystemRaycastResults(Input.mousePosition)))
		{
			ToggleUIMode(false);
			return;
		}

		// more ui stuff?

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
			isBlockingUI = false;
		}
	}

	void GridUpdate()
	{
		var uiHits = GetEventSystemRaycastResults(Input.mousePosition);
		if (IsPointerOverUIElement(uiHits))
		{
			ToggleUIMode(true);
			return;
		}

		Vector3 mouseWorldPos = GetMouseWorldPos();

		if (Input.mouseScrollDelta != Vector2.zero)
		{
			if (Input.GetKey(KeyCode.LeftControl))
			{
				var newY = mainCamera.transform.position.y + Input.mouseScrollDelta.y;
				mainCamera.transform.position = new Vector3(
					mainCamera.transform.position.x, newY, mainCamera.transform.position.z);
			}
			else if (Input.GetKey(KeyCode.LeftShift))
			{
				var newX = mainCamera.transform.position.x + Input.mouseScrollDelta.y;
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

		var keyInput = GetKeyInput();

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
						hoverObject.SetContextMenu(contextMenu, mouseWorldPos); // non-snapped pos is better
						editMode = EditMode.ContextMenu;
					}
					else
					{
						DeselectObject();
					}
				}
				else if (Input.GetKeyDown(KeyCode.Escape))
				{
					DeselectObject();
				}
			}
			break;

			case EditMode.ContextMenu:
			{
				if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
				{
					contextMenu.ClosePanel();
					editMode = EditMode.None;
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
				else if (Input.GetMouseButtonDown(1))
				{
					if (editMode == EditMode.CreateObject)
					{
						Destroy(selectedObject.gameObject);
					}
					else
					{
						selectedObject.ResetToLastPosition();
					}

					selectedObject = null;
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

	private void DeselectObject()
	{
		if (selectedObject != null)
			selectedObject.Deselect();
		selectedObject = null;
	}

	public enum KeyInput
	{
		None = 0x0,
		Ctrl = 0x1,
		Alt = 0x2,
	}
	private KeyInput GetKeyInput()
	{
		KeyInput input = KeyInput.None;

		if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
			input |= KeyInput.Ctrl;
		if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
			input |= KeyInput.Alt;
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

	/// <summary>
	/// Ignores the currently manipulating object.
	/// Not reccommended: there are priority issues.
	/// </summary>
	/// <param name="worldPos"></param>
	/// <param name="layerMask"></param>
	/// <param name="hitObject"></param>
	/// <returns></returns>
	[Obsolete("Use <c>private bool CheckForObject(Vector2 worldPos, out DesignObject hitObject)</c> instead")]
	private bool CheckForObject(Vector2 worldPos, int layerMask, out GameObject hitObject)
	{
		RaycastHit2D[] hits = Physics2D.RaycastAll(worldPos, Vector2.zero, 10.0f, layerMask);

		if (hits.Length > 0)
		{
			hitObject = null;
			foreach (var hit in hits)
			{
				var hitGO = hit.transform.gameObject;
				if (selectedObject != null && hitGO == selectedObject.transform)
				{
					continue;
				}

				hitObject = hitGO;
				//return true;
			}

			if (hitObject != null)
				return true;
		}

		hitObject = null;
		return false;
	}


	private bool IsPointerOverUIElement(List<RaycastResult> eventSystemRaycastResults)
	{
		for (int index = 0; index < eventSystemRaycastResults.Count; index++)
		{
			RaycastResult curRaysastResult = eventSystemRaycastResults[index];
			if (curRaysastResult.gameObject.layer == uiLayerIndex)
				return true;
		}

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

	public static void ShowDialog(RectTransform dialogRect)
	{
		instance._ShowDialog(dialogRect);
	}

	private void _ShowDialog(RectTransform dialogRect)
	{
		dialogRect.SetParent(uiCanvas.transform, false);
		ToggleUIMode(true);
		isBlockingUI = true;
	}

	public static void CloseDialog(RectTransform dialogRect)
	{
		instance._CloseDialog(dialogRect);
	}

	private void _CloseDialog(RectTransform dialogRect)
	{
		Destroy(dialogRect.gameObject);
		ToggleUIMode(false);
	}
}
