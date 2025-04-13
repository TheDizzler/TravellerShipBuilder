using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DesignManager;

public class CustomCursor : MonoBehaviour
{
	public Vector2 debug;

	private static CustomCursor instance;
	[SerializeField] private Texture2D handOpen;
	[SerializeField] private Texture2D handClosed;
	[SerializeField] private Texture2D handPoint;
	[SerializeField] private Texture2D handPointUp;
	[SerializeField] private Texture2D resizeAHorizontal;
	[SerializeField] private Texture2D resizeACross;
	[SerializeField] private Texture2D resizeACrossDiagonal;
	[SerializeField] private Texture2D resizeCCross;
	[SerializeField] private Texture2D resizeCCrossDiagonal;

	[SerializeField] private Vector2 handOpenHotspot = new Vector2(24, 30);
	[SerializeField] private Vector2 handPointHotspot = new Vector2(24, 30);
	[SerializeField] private Vector2 handPointUpHotspot = new Vector2(29, 7);
	[SerializeField] private Vector2 resizeCrossHotspot = new Vector2(32, 32);
	[SerializeField] private Vector2 resizeHorizontalHotspot = new Vector2(32, 32);
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
		HoverWallControlPoint,
		Scroll,
		MoveDoor,
		MoveWallControlPoint,
		UI,
		HoverWall,
		HoverDoor,
		ResizeHorizontal
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
		//Debug.Log(cursorMode);
		if (isUICursor)
			uiCursorMode = cursorMode;
		else
			gridCursorMode = cursorMode;
		switch (cursorMode)
		{
			default:
			case CursorSpriteMode.Default:
			{
				Cursor.SetCursor(handOpen, handOpenHotspot, CursorMode.Auto);
			}
			break;

			case CursorSpriteMode.HoverWall:
			{
				Cursor.SetCursor(handPoint, handPointHotspot, CursorMode.Auto);
			}
			break;

			case CursorSpriteMode.HoverWallControlPoint:
			case CursorSpriteMode.HoverDoor:
			{
				Cursor.SetCursor(resizeACross, resizeCrossHotspot, CursorMode.Auto);
			}
			break;

			case CursorSpriteMode.MoveWallControlPoint:
			case CursorSpriteMode.MoveDoor:
			{
				Cursor.SetCursor(resizeACrossDiagonal, resizeCrossHotspot, CursorMode.Auto);
			}
			break;

			case CursorSpriteMode.ResizeHorizontal:
			{
				Cursor.SetCursor(resizeAHorizontal, resizeHorizontalHotspot, CursorMode.Auto);
			}
			break;

			case CursorSpriteMode.Scroll:
			{
				Cursor.SetCursor(handClosed, handOpenHotspot, CursorMode.Auto);
			}
			break;

			case CursorSpriteMode.UI:
			{
				Cursor.SetCursor(handPointUp, handPointUpHotspot, CursorMode.Auto);
			}
			break;
		}
	}
}
