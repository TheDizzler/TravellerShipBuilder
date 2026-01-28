using System;
using UnityEngine;

namespace AtomosZ.UI
{
	public class UICursors : MonoBehaviour
	{
		public enum UICursorMode
		{
			[Tooltip("If you don't want the UI to override the current cursor.")]
			None,
			[Tooltip("The normal UI cursor.")]
			Default,

			Drag,
		}


		[SerializeField] private CustomDictionary<UICursorMode, Texture2D> cursorTextures;
		[SerializeField] private CustomDictionary<Texture2D, Vector2> cursorHotspots;

		public void SetCursor(UICursorMode cursorMode)
		{
			var texture = cursorTextures[cursorMode];
			if (!cursorHotspots.TryGetValue(texture, out var hotspot))
			{
				hotspot = Vector2.zero;
			}

			Cursor.SetCursor(texture, hotspot, CursorMode.Auto);
		}
	}
}