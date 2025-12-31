using System;

using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "ImageViewData", menuName = "AtomosZ/UI/UIImageViewScriptableObject")]
	public class UIImageViewScriptableObject : ScriptableObject
	{
		public UIExpandingLabelScriptableObject labelData;
		
		public bool isImageHidden = false;
		public bool isCaptionHidden = false;

		/// <summary>
		/// Image to show if there is sprite is null.
		/// </summary>
		public Sprite defaultSprite;
	}
}