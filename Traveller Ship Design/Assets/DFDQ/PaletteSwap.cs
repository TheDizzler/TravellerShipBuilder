using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AtomosZ
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class PaletteSwap : MonoBehaviour
	{
		public CustomDictionary<Color32, Color32> colorSwapDict = new();
		[SerializeField] private Texture2D colorSwapTexture;

		[ContextMenu("Get Palette From Texture")]
		public void GetPaletteFromTexture()
		{
			colorSwapDict.Clear();
			var colors = new HashSet<Color32>();

			var color32s = GetComponent<SpriteRenderer>().sprite.texture.GetPixels32();
			foreach (var c32 in color32s)
			{
				if (c32.a != 0)
					colors.Add(c32);
			}

			foreach (var color in colors)
				colorSwapDict.Add(color, color);
		}

#if UNITY_EDITOR
		private void OnValidate()
		{
			// (called whenever the object is updated)
			CreatePaletteSwapTexture();
		}
#endif

		[ContextMenu("Create Texture")]
		public void CreatePaletteSwapTexture()
		{
			colorSwapTexture = new Texture2D(256, 1, TextureFormat.RGBA32, false, false);
			colorSwapTexture.filterMode = FilterMode.Point;

			for (int i = 0; i < colorSwapTexture.width; ++i)
				colorSwapTexture.SetPixel(i, 0, new Color(0.0f, 0.0f, 0.0f, 0.0f));


			foreach (var swap in colorSwapDict)
			{
				colorSwapTexture.SetPixel(swap.Key.r, 0, swap.Value);
			}

			colorSwapTexture.Apply();

			var sr = GetComponent<SpriteRenderer>();
			if (Application.isPlaying)
			{ // @TODO: Must instantiate and set new mat before doing this.
				sr.material.SetTexture("_SwatchTexture", colorSwapTexture);
			}
			else
			{
				sr.sharedMaterial.SetTexture("_SwatchTexture", colorSwapTexture);

				File.WriteAllBytes(@"D:\github\Traveller Ship Design\Traveller Ship Design\Assets\DFDQ\Battle\Sprites\swap tex.png", colorSwapTexture.EncodeToPNG());
			}
		}
	}
}