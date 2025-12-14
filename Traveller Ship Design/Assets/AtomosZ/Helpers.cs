using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static AtomosZ.DFDQ.Battle.GridManager;
using Object = UnityEngine.Object;

namespace AtomosZ
{
	/// <summary>
	/// Universal useful Unity functions:<br/>
	/// Camera camera<br/>
	/// WaitForSeconds GetWait(float)<br/>
	/// bool IsOverUI()<br/>
	/// List&lt;RaycastResult> GetUIRaycasts()<br/>
	/// Vector2 GetWorldPositionOfCanvasElement()<br/>
	/// void DeleteChildren(Transform)<br/>
	/// </summary>
	public static class Helpers
	{
		private static Camera _camera;
		/// <summary>
		/// Caches Camera.main.
		/// </summary>
		public static Camera camera
		{
			get
			{
				if (_camera == null)
					_camera = Camera.main;
				return _camera;
			}
		}

		public static Vector3 GetMouseWorldPos()
		{
			var mousePos = Input.mousePosition;
			mousePos.z = -camera.transform.position.z;
			var result = camera.ScreenToWorldPoint(mousePos);
			result.z = 0;
			return result;
		}

		public static Vector2Int Up(this Vector2Int vec)
		{
			return vec + Vector2Int.up;
		}

		public static Vector2Int Right(this Vector2Int vec)
		{
			return vec + Vector2Int.right;
		}
		public static Vector2Int Down(this Vector2Int vec)
		{
			return vec + Vector2Int.down;
		}
		public static Vector2Int Left(this Vector2Int vec)
		{
			return vec + Vector2Int.left;
		}

		public static Vector2Int GetNeighbourPos(this Vector2Int vec, CardinalDirection dir)
		{
			switch (dir)
			{
				case CardinalDirection.Up:
					return vec.Up();
				case CardinalDirection.Right:
					return vec.Right();
				case CardinalDirection.Down:
					return vec.Down();
				case CardinalDirection.Left:
					return vec.Left();
				default:
					throw new Exception($"what the heck is a {dir}?");
			}
		}

#if DEBUG
		/// <summary>
		/// Gets the world position of the mouse in the SceneView for use in OnSceneGUI in a custom Editor.
		/// </summary>
		/// <returns></returns>
		public static Vector2 GetSceneViewMousePosition()
		{
			// Gets the current scene view and scene view camera while design-time and play mode
			var sceneView = UnityEditor.SceneView.sceneViews.Count > 0 ? (UnityEditor.SceneView)UnityEditor.SceneView.sceneViews[0] : null;
			var sceneViewCamera = sceneView?.camera;

			if (sceneView == null || sceneViewCamera == null)
				return Vector2.zero;

			// Determine current mouse position with new input system
			var mouseScreenPosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();

			// Determine offsets and bars of the scene view
			var sceneViewTopBarsHeight = sceneView.rootVisualElement.worldBound.y;
			var sceneViewAbsoluteScreenOffset = sceneView.position.min;

			var adjustedMouseScreenPos = mouseScreenPosition;
			// Adjust y so it takes the top bars of the scene view into account and flip y-axis (cause of different coordinate systems)
			// and also adjust x/y regarding the scene view offset
			// (This is the correct code when calling from Update)
			//adjustedMouseScreenPos.y = sceneViewCamera.pixelHeight - (adjustedMouseScreenPos.y - sceneViewTopBarsHeight - sceneViewAbsoluteScreenOffset.y);
			//adjustedMouseScreenPos.x -= sceneViewAbsoluteScreenOffset.x;

			// Adjust y so it takes the top bars of the scene view into account and flip y-axis (cause of different coordinate systems)
			// (This would be the correct code when calling from OnSceneGUI)
			adjustedMouseScreenPos.y = sceneViewCamera.pixelHeight - (adjustedMouseScreenPos.y - sceneViewTopBarsHeight);

			var result = sceneViewCamera.ScreenToWorldPoint(adjustedMouseScreenPos);
			return result;
		}
#endif

		private static readonly Dictionary<float, WaitForSeconds> waitDict = new();
		public static WaitForSeconds GetWait(float time)
		{
			if (!waitDict.TryGetValue(time, out var waitForSeconds))
			{
				waitForSeconds = new WaitForSeconds(time);
				waitDict.Add(time, waitForSeconds);
			}

			return waitForSeconds;
		}


		/// <summary>
		/// Is the cursur over a canvas object?
		/// </summary>
		/// <returns></returns>
		public static bool IsOverUI()
		{
			return GetUIRaycasts().Count > 0;
		}

		/// <summary>
		/// Returns the RaycastResults for canvas objects under the current cursor position.
		/// </summary>
		/// <returns></returns>
		public static List<RaycastResult> GetUIRaycasts()
		{
			PointerEventData eventDataCurrentPos = new PointerEventData(EventSystem.current);
			eventDataCurrentPos.position = Input.mousePosition;
			List<RaycastResult> raycastResults = new List<RaycastResult>();
			EventSystem.current.RaycastAll(eventDataCurrentPos, raycastResults);
			return raycastResults;
		}

		/// <summary>
		/// Find world point of canvas element.
		/// </summary>
		/// <param name="element"></param>
		/// <returns></returns>
		public static Vector2 GetWorldPositionOfCanvasElement(RectTransform element)
		{
			RectTransformUtility.ScreenPointToWorldPointInRectangle(element, element.position, camera, out var result);
			return result;
		}

		public static void DeleteChildren(this Transform t)
		{
			foreach (Transform child in t)
			{
#if UNITY_EDITOR
				if (Application.isPlaying)
					Object.Destroy(child.gameObject);
				else
					Object.DestroyImmediate(child.gameObject);
#else
				Object.Destroy(child.gameObject);
#endif
			}
		}
	}
}