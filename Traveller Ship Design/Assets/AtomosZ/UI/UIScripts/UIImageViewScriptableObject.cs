using System;

using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	[CreateAssetMenu(fileName = "Data", menuName = "AtomosZ/UIScriptableObjects/UIImageViewScriptableObject")]
	public class UIImageViewScriptableObject : ScriptableObject
	{
		public UIExpandingLabelScriptableObject labelData;
	}
}