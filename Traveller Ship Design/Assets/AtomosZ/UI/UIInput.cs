using System;

using UnityEngine;

public class UIInput : MonoBehaviour
{
	private static UIInput _instance;
	public static UIInput instance
	{
		get
		{
			if (_instance == null)
				_instance = GameObject.FindAnyObjectByType<UIInput>();
			return _instance;
		}
	}


	[SerializeField] private Canvas uiCanvas;
	[SerializeField] private Camera uiCamera;


	public static Vector2 GetUICoordinates(Vector2 pos)
	{
		return instance.GetUICoordinatesFromPos(pos);
	}

	public Vector2 GetUICoordinatesFromPos(Vector2 pos)
	{
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			uiCanvas.GetComponent<RectTransform>(), pos, uiCamera, out Vector2 uiPos);
		return uiPos;
	}

	public Vector2 GetUICoordinatesFromMousePos()
	{
		Vector2 mousePos = Input.mousePosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(
			uiCanvas.GetComponent<RectTransform>(), mousePos, uiCamera, out Vector2 uiPos);
		return uiPos;
	}
}
