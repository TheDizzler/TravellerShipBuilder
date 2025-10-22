using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	/// <summary>
	/// @TODO(Tristan): maxSigFigs
	/// </summary>
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UISliderScriptableObject")]
	public class UISliderScriptableObject : ScriptableObject
	{
		public bool showHandle = true;
		public Sprite handleSprite;
		public Vector2 handleOffset = new Vector2(16, 16);

		public bool showUnits = true;
		public float unitSpan = 1;
		//public int unitVerticalOffset = 0;

		public LabelEx labelEx;
	}
}