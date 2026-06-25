using System;

using UnityEngine;

namespace AtomosZ.UI
{
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


		[SerializeField] public Canvas uiCanvas;
		private RectTransform _uiCanvasRect;
		public RectTransform uiCanvasRect
		{
			get
			{
				if (_uiCanvasRect == null)
					_uiCanvasRect = uiCanvas.GetComponent<RectTransform>();
				return _uiCanvasRect;
			}
		}

		

		[SerializeField] public Camera uiCamera;

		public static Vector2 GetUICoordinates(Vector2 pos)
		{
			return instance.GetUICoordinatesFromPos(pos);
		}

		public Vector2 GetUICoordinatesFromPos(Vector2 pos)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				uiCanvasRect, pos, uiCamera, out Vector2 uiPos);
			return uiPos;
		}

		public Vector2 GetUICoordinatesFromMousePos()
		{
			var mousePos = Mouse.pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				uiCanvasRect, mousePos, uiCamera, out Vector2 uiPos);
			return uiPos;
		}


		/// <summary>
		/// Uses null camera in ScreenPointToLocalPointInRectangle(). 
		/// This doesn't seem very useful at all, but may depend on camera projection and render type.
		/// </summary>
		/// <returns></returns>
		public Vector2 GetNullCameraUICoordinatesFromMousePos()
		{
			Vector2 mousePos = Mouse.pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				uiCanvasRect, mousePos, null, out Vector2 uiPos);
			return uiPos;
		}

		/// <summary>
		/// Gets the screen coordinates transposed over the UI. <br/>(0, 0) is at the center of the screen.
		/// </summary>
		/// <returns></returns>
		public Vector2 GetMainCameraUICoordinatesFromMousePos()
		{
			Vector2 mousePos = Mouse.pos;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(
				uiCanvasRect, mousePos, Helpers.camera, out Vector2 uiPos);
			return uiPos;
		}

		/// <summary>
		/// Get the world position of the mouse as if it were projected to screenZDepth.
		/// </summary>
		/// <param name="screenZDepth"></param>
		/// <returns></returns>
		public Vector3 GetMouseWorldPos(float screenZDepth)
		{
			Vector3 mousePoint = Mouse.pos;
			mousePoint.z = screenZDepth;
			return Helpers.camera.ScreenToWorldPoint(mousePoint);
		}
		public Vector3 GetMouseUIWorldPos(float screenZDepth)
		{
			Vector3 mousePoint = Mouse.pos;
			mousePoint.z = screenZDepth;
			return uiCamera.ScreenToWorldPoint(mousePoint);
		}
	}
}