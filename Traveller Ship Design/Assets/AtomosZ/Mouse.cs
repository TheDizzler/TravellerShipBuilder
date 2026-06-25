using System;
using UnityEngine;

namespace AtomosZ
{
	public class Mouse
	{
		/// <summary>
		/// Current position in window space.
		/// </summary>
		public static Vector3 pos { get { return UnityEngine.InputSystem.Mouse.current.position.ReadValue(); } }
		/// <summary>
		/// Current window-space motion of pointer.
		/// </summary>
		public static Vector2 posDelta { get { return UnityEngine.InputSystem.Mouse.current.delta.ReadValue(); } }
		public static Vector2 scrollDelta { get { return UnityEngine.InputSystem.Mouse.current.scroll.ReadValue(); } }
		public static bool leftButtonPressed { get { return UnityEngine.InputSystem.Mouse.current.leftButton.isPressed; } }
		public static bool leftButtonDown { get { return UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame; } }
		public static bool leftButtonUp { get { return UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame; } }
		public static bool rightButtonPressed { get { return UnityEngine.InputSystem.Mouse.current.rightButton.isPressed; } }
		public static bool rightButtonDown { get { return UnityEngine.InputSystem.Mouse.current.rightButton.wasPressedThisFrame; } }
		public static bool rightButtonUp { get { return UnityEngine.InputSystem.Mouse.current.rightButton.wasReleasedThisFrame; } }
		public static bool middleButtonPressed { get { return UnityEngine.InputSystem.Mouse.current.middleButton.isPressed; } }
		public static bool middleButtonDown { get { return UnityEngine.InputSystem.Mouse.current.middleButton.wasPressedThisFrame; } }
		public static bool middleButtonUp { get { return UnityEngine.InputSystem.Mouse.current.middleButton.wasReleasedThisFrame; } }


		/// <summary>
		/// Returns true during the frame the mouse button was pressed.
		/// </summary>
		/// <param name="mouseButtonIndex"></param>
		/// <returns></returns>
		public static bool GetMouseButtonDown(int mouseButtonIndex)
		{
			switch (mouseButtonIndex)
			{
				case 0:
					return leftButtonDown;
				case 1:
					return rightButtonDown;
				case 2:
					return middleButtonDown;
				default:
					return false;
			}
		}

		/// <summary>
		/// Returns true during the frame the mouse button was released.
		/// </summary>
		/// <param name="mouseButtonIndex"></param>
		/// <returns></returns>
		public static bool GetMouseButtonUp(int mouseButtonIndex)
		{
			switch (mouseButtonIndex)
			{
				case 0:
					return leftButtonUp;
				case 1:
					return rightButtonUp;
				case 2:
					return middleButtonUp;
				default:
					return false;
			}
		}

		/// <summary>
		/// Returns trude if the button is currently pressed.
		/// </summary>
		/// <param name="mouseButtonIndex"></param>
		/// <returns></returns>
		public static bool GetMouseButton(int mouseButtonIndex)
		{
			switch (mouseButtonIndex)
			{
				case 0:
					return leftButtonPressed;
				case 1:
					return rightButtonPressed;
				case 2:
					return middleButtonPressed;
				default:
					return false;
			}
		}
	}
}