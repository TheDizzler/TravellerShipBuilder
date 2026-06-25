using UnityEngine;
using UnityEngine.InputSystem;

namespace AtomosZ
{
	public static class Keyboard
	{
		/// <summary>
		/// Ctrl (left & right), Alt (left & right), Esc, Shift (left & right)
		/// </summary>
		public enum ModifierKey
		{
			None = 0x0,
			Ctrl = 0x1,
			Alt = 0x2,
			Esc = 0x4,
			Shift = 0x8,
		}

		public static bool IsKeyPressed(Key key)
		{
			return UnityEngine.InputSystem.Keyboard.current[key].isPressed;
		}

		public static bool IsKeyDown(Key key)
		{
			return UnityEngine.InputSystem.Keyboard.current[key].wasPressedThisFrame;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool IsKeyUp(Key key)
		{
			return UnityEngine.InputSystem.Keyboard.current[key].wasReleasedThisFrame;
		}

		[Tooltip("Ctrl (left & right), Alt (left & right), Esc, Shift (left & right)")]
		public static ModifierKey GetModifierKeyInput()
		{
			ModifierKey input = ModifierKey.None;

			if (UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
				return ModifierKey.Esc;
			if (UnityEngine.InputSystem.Keyboard.current.ctrlKey.wasPressedThisFrame)
				input |= ModifierKey.Ctrl;
			if (UnityEngine.InputSystem.Keyboard.current.altKey.wasPressedThisFrame)
				input |= ModifierKey.Alt;
			if (UnityEngine.InputSystem.Keyboard.current.shiftKey.wasPressedThisFrame)
				input |= ModifierKey.Shift;

			return input;
		}
	}
}