using System;

using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "LabelData", menuName = "AtomosZ/UI/TextScriptableObject", order = 0)]
	public class UIExpandingLabelScriptableObject : ScriptableObject
	{
		[Tooltip("Default: 36")]
		public float fontSize = 36;
		public Vector4 textMargin = new Vector4(26, 8, 26, 16);
		public Color fontColor = Color.white;
		[Tooltip("Color to use when the control this label is part of is non-interactable.")]
		public Color disabledColor = new Color(.63f, .63f, .63f);
		public TMP_FontAsset fontAsset;
		public FontStyles fontStyles;
	}
}