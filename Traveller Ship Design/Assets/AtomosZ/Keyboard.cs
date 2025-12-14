using UnityEngine;

namespace AtomosZ
{
	public static class Keyboard 
	{
		public enum ModifierKey
		{
			None = 0x0,
			Ctrl = 0x1,
			Alt = 0x2,
			Esc = 0x4,
			Shift = 0x8,
		}

		public static ModifierKey GetModifierKeyInput()
		{
			ModifierKey input = ModifierKey.None;
			
			if (Input.GetKeyDown(KeyCode.Escape))
				return ModifierKey.Esc;
			if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
				input |= ModifierKey.Ctrl;
			if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
				input |= ModifierKey.Alt;
			if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
				input |= ModifierKey.Shift;
			return input;
		}
	}
}