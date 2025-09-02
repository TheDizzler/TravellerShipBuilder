using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;

public class CustomCursor : MonoBehaviour
{
	public Vector2 debug;

	private static CustomCursor instance;

	[UDictionary.Split(50, 50)]
	[SerializeField] private UDictionary<CursorSpriteMode, Texture2D> cursorTextures;
	[UDictionary.Split(50, 50)]
	[SerializeField] private UDictionary<Texture2D, Vector2> cursorHotspots;

	/// <summary>
	/// public for debugging purposes.
	/// </summary>
	public CursorSpriteMode gridCursorMode;
	/// <summary>
	/// public for debugging purposes.
	/// </summary>
	public CursorSpriteMode uiCursorMode;

	public enum CursorSpriteMode
	{
		Default,

		MoveDoor,
		MoveWallControlPoint,
		HoverDoor,
		HoverWall,
		HoverWallControlPoint,

		Scroll,
		ResizeHorizontal,

		ZoomIn,

		UI_Default,
		UI_Move,
		UI_Caret,
		UI_Menu,

		Disabled,
	}


	void Start()
	{
		instance = this;
		SetCursor(CursorSpriteMode.Default, false);
	}


	void Update()
	{
		debug = GetMouseWorldPos();
	}


	public static void SetCursor(CursorSpriteMode cursorMode, bool isUICursor = false)
	{
		if (instance == null)
			instance = GameObject.FindAnyObjectByType<CustomCursor>();
		instance._SetCursor(cursorMode, isUICursor);
	}

	private void _SetCursor(CursorSpriteMode cursorMode, bool isUICursor)
	{
		if (isUICursor)
			uiCursorMode = cursorMode;
		else
			gridCursorMode = cursorMode;

		var texture = cursorTextures[cursorMode];
		if (!cursorHotspots.TryGetValue(texture, out var hotspot))
		{
			hotspot = Vector2.zero;
		}

		Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
	}
}
