using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.DFDQ.Battle.Units
{
	[CreateAssetMenu(fileName = "ScriptableUnit", menuName = "Scriptable Objects/ScriptableUnit")]
	public class ScriptableUnit : ScriptableObject
	{
		public Faction faction;
		public Sprite sprite;
		public CustomDictionary<Color32, Color32> colorSwapDict;
		public string unitName;
		public bool blocksLineOfSight;

		public void GetPaletteFromTexture()
		{
			colorSwapDict.Clear();
			var colors = new HashSet<Color32>();

			var color32s = sprite.texture.GetPixels32();
			foreach (var c32 in color32s)
			{
				if (c32.a != 0)
					colors.Add(c32);
			}

			foreach (var color in colors)
				colorSwapDict.Add(color, color);
		}
	}

	public enum Faction
	{
		Hero = 0,
		Monster = 1,
	}
}