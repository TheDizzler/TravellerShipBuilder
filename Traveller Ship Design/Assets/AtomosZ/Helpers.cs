using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

		/// <summary>
		/// Distance (in hexes) between two hex cells.
		/// </summary>
		/// <param name="a"></param>
		/// <param name="b"></param>
		/// <returns></returns>
		public static int Distance(Vector3Int a, Vector3Int b)
		{
			int xd = a.y - b.y;
			int yd = a.x - ((a.y - (a.y & 1)) / 2) - (b.x - (b.y - (b.y & 1)) / 2);
			int dist = (Mathf.Abs(xd) + Mathf.Abs(xd + yd) + Mathf.Abs(yd)) / 2;
			return dist;
		}

		/// <summary>
		/// Vector3Int(1, 0, 0)
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3Int Up(this Vector3Int vec)
		{
			return vec + Vector3Int.right;
		}
		/// <summary>
		/// Vector3Int(0, 1, 0)
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3Int UpRight(this Vector3Int vec)
		{
			return vec + Vector3Int.up;
		}
		/// <summary>
		/// Vector3Int(-1, 1, 0)
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3Int DownRight(this Vector3Int vec)
		{
			return vec + Vector3Int.left + Vector3Int.up;
		}
		/// <summary>
		/// Vector3Int(-1, 0, 0)
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3Int Down(this Vector3Int vec)
		{
			return vec + Vector3Int.left;
		}
		/// <summary>
		/// Vector3Int(-1, -1, 0)
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3Int DownLeft(this Vector3Int vec)
		{
			return vec + Vector3Int.left + Vector3Int.down;
		}
		/// <summary>
		/// Vector3Int(0, -1, 0)
		/// </summary>
		/// <param name="vec"></param>
		/// <returns></returns>
		public static Vector3Int UpLeft(this Vector3Int vec)
		{
			return vec + Vector3Int.down;
		}

		public static Vector3Int GetNeighbourPos(this Vector3Int vec, CanonicalDirection dir)
		{
			switch (dir)
			{
				case CanonicalDirection.Up:
					return vec.Up();
				case CanonicalDirection.UpRight:
					return vec.UpRight();
				case CanonicalDirection.DownRight:
					return vec.DownRight();
				case CanonicalDirection.Down:
					return vec.Down();
				case CanonicalDirection.DownLeft:
					return vec.DownLeft();
				case CanonicalDirection.UpLeft:
					return vec.UpLeft();
				default:
					throw new Exception($"what the heck is a {dir}?");
			}
		}

		/// <summary>
		/// The 6 directions for a <i><b>FLAT</b></i>-top hex.
		/// </summary>
		public enum CanonicalDirection
		{
			Up = 0x0, UpRight, DownRight, Down, DownLeft, UpLeft,
			North = 0x0, NorthEast, SouthEast, South, SouthWest, NorthWest,
		}

		/// <summary>
		/// The four compass point directions.
		/// </summary>
		public enum CardinalDirection
		{
			Up = 0x0, Right, Down, Left,
			//North = 0x0, East, South, West,
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

		public static Texture2D ToTexture2D(Camera snapShotCamera, RenderTexture newTexture)
		{
			snapShotCamera.enabled = true;
			RenderTexture currentActiveRT = RenderTexture.active;
			RenderTexture.active = newTexture;

			snapShotCamera.Render();

			Texture2D tex = new Texture2D(newTexture.width,
				newTexture.height, TextureFormat.RGB24, false);
			tex.ReadPixels(new Rect(0, 0, newTexture.width, newTexture.height), 0, 0);
			tex.Apply();

			RenderTexture.active = currentActiveRT;
			snapShotCamera.enabled = false;
			return tex;
		}

		/// <summary>
		/// Get the height of an image according to the rendered sprite and the desired width(if is not specified, it will take the width of the own image's recTransform)
		/// </summary>
		public static float GetDesiredHeight(this Image img, float desiredWidth = default)
		{
			RectTransform ImageRect = img.rectTransform;
			float _bodyWidth = desiredWidth == default ? ImageRect.rect.width : desiredWidth;
			float _imageWidth = (float)img.sprite.texture.width;
			float _imageHeight = (float)img.sprite.texture.height;
			float _ratio = _imageWidth / _imageHeight;
			float _expectedHeight = _bodyWidth / _ratio;
			return _expectedHeight;
		}

		public static float GetDesiredWidth(this Image img, float desiredHeight = default)
		{
			RectTransform ImageRect = img.rectTransform;
			float _bodyHeight = desiredHeight == default ? ImageRect.rect.height : desiredHeight;
			float _imageWidth = (float)img.sprite.texture.width;
			float _imageHeight = (float)img.sprite.texture.height;
			float _ratio = _imageWidth / _imageHeight;
			float _expectedWidth = _bodyHeight / _ratio;
			return _expectedWidth;
		}

		public static T GetSingleTon<T>() where T : MonoBehaviour
		{
			return GameObject.FindAnyObjectByType<T>();
		}

#if UNITY_EDITOR
		public static bool IsPrefabStage()
		{
			var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			return stage != null;
		}
#endif
	}


	public static class Log
	{
		[System.Diagnostics.DebuggerStepThrough]
		[HideInCallstack]
		public static void Warning(string msg)
		{
			Debug.LogWarning(msg);
		}
		public static void Error(string msg)
		{
			Debug.LogError(msg);
		}
		public static void Exception(string msg)
		{
			Debug.LogException(new Exception(msg));
		}
	}
}