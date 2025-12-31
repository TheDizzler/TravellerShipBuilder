#define DEBUG_TEXTURE
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace AtomosZ.ShaderTools
{
	public static class SpriteOutlineCreator
	{
		public static Dictionary<Sprite, (Texture2D tex, int spriteWidth, int spriteHeight)> spriteDictionary = new();

		public static Sprite CreateSpriteOutline(Sprite sprite, int outlineThickness)
		{
			if (spriteDictionary.TryGetValue(sprite, out var outlineTex))
				return Sprite.Create(outlineTex.tex, new Rect(0, 0, outlineTex.spriteWidth, outlineTex.spriteHeight),
					new Vector2(0.5f, 0.5f));
#if DEBUG_TEXTURE
			var timer = new Stopwatch();
			timer.Start();
#endif


			var spriteRect = sprite.rect;

			int width = (int)spriteRect.width;
			int height = (int)spriteRect.height;
			var colors = new Color32[width][];

			{
				var textureWidth = sprite.texture.width;
				var textureHeight = sprite.texture.height;
				int spriteStartX = (int)spriteRect.x;
				int spriteStartY = (int)spriteRect.y; // this is from bottom of texture
				var texColor32s = sprite.texture.GetPixels32(); // gets pixels from bottom left

#if DEBUG_TEXTURE
				Texture2D testText = new Texture2D(width, height);
#endif
				for (int i = 0, w = spriteStartX; i < width; ++w, ++i)
				{
					colors[i] = new Color32[height];
					for (int j = 0, h = spriteStartY; j < height; ++h, ++j)
					{
						colors[i][j] = texColor32s[h * textureWidth + w];
#if DEBUG_TEXTURE
						testText.SetPixel(i, j, colors[i][j]);
#endif
					}
				}
#if DEBUG_TEXTURE
				File.WriteAllBytes(@$"D:\github\Traveller Ship Design\Traveller Ship Design\Assets\Starmap\Shaders\{sprite.name} test tex.png", testText.EncodeToPNG());
#endif
			}

			int outlineWidth = width + outlineThickness * 2;
			int outlineHeight = height + outlineThickness * 2;
			var fullMaxSqrDist = outlineThickness * outlineThickness + outlineThickness * outlineThickness;
			var maxSqrDist = outlineThickness * outlineThickness;
			var outlineTexture = new Texture2D(outlineWidth, outlineHeight, TextureFormat.RGBA32, false, false);
			outlineTexture.filterMode = FilterMode.Point;
			outlineTexture.anisoLevel = 0;
			for (int outlineTexH = 0; outlineTexH < outlineHeight; ++outlineTexH)
			{
				for (int outlineTexW = 0; outlineTexW < outlineWidth; ++outlineTexW)
				{
					var textureOffsetX = outlineTexW - outlineThickness;
					var texturOffsetY = outlineTexH - outlineThickness;
					var pixel = GetPixel(colors, width, height, textureOffsetX, texturOffsetY);
					if (pixel.a == 255)
					{   // inside a sprite
						float alpha = 0.0f;
#if DEBUG_TEXTURE
						if (Application.isPlaying)
							alpha = 0.0f;
#endif
						outlineTexture.SetPixel(outlineTexW, outlineTexH, new Color(1, 0, 1, alpha));
						continue;
					}

					float closestSqrDist = fullMaxSqrDist;
					for (int y = -outlineThickness; y <= outlineThickness; ++y)
					{
						for (int x = -outlineThickness; x <= outlineThickness; ++x)
						{
							var checkX = x + textureOffsetX;
							var checkY = y + texturOffsetY;
							var checkPixel = GetPixel(colors, width, height, checkX, checkY);
							if (checkPixel.a == 0)
								continue;

							var sqrDist = y * y + x * x;
							closestSqrDist = Mathf.Min(closestSqrDist, sqrDist);
						}
					}

					Color outlinePixel = new Color(1, 0, 1, 0);

					outlinePixel.a = 1 - ((closestSqrDist - 2) / maxSqrDist);

#if DEBUG_TEXTURE
					if (closestSqrDist - 2 > maxSqrDist)
					{
						outlinePixel.a = 0;
						if (Application.isPlaying)
							outlinePixel.a = 0.0f;
						outlinePixel.r = 0;
						outlinePixel.g = (float)(outlineTexW) / (outlineWidth);
						outlinePixel.b = (float)(outlineTexH) / (outlineHeight);
					}
#endif
					outlineTexture.SetPixel(outlineTexW, outlineTexH, outlinePixel);
				}
			}

#if DEBUG_TEXTURE
			File.WriteAllBytes($@"D:\github\Traveller Ship Design\Traveller Ship Design\Assets\Starmap\Shaders\{sprite.name} outline tex.png", outlineTexture.EncodeToPNG());
			Debug.Log("Texture create time: " + timer.Elapsed.TotalSeconds + "s");
#endif

			outlineTexture.Apply();

			spriteDictionary.Add(sprite, (outlineTexture, outlineWidth, outlineHeight));
			return Sprite.Create(outlineTexture, new Rect(0, 0, outlineWidth, outlineHeight),
				new Vector2(0.5f, 0.5f));
		}


		private static Color32 GetPixel(Color32[][] color32s, int width, int height, int w, int h)
		{
			if (w < 0 || h < 0 || w >= width || h >= height)
				return new Color32(0, 0, 0, 0);
			return color32s[w][h];
		}

		private static Color32 GetPixel(Color32[] color32s, int width, int h, int w)
		{
			if (w < 0 || h < 0 || w >= width)
				return new Color32(0, 0, 0, 0);
			var index = h * width + w;
			try
			{
				if (index < 0 || index >= color32s.Length)
					return new Color32(0, 0, 0, 0);
				return color32s[index];
			}
			catch (Exception e)
			{
				Log.Error("Exception in GetPixel():\n" + e.Message);
				return new Color32(0, 0, 0, 0);
			}
		}
	}
}