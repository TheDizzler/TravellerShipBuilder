using System;

using UnityEditor;

using UnityEngine;

using static AtomosZ.UI.UIPrefabProvider;
namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(UIPrefabProvider))]
	public class PrefabGatherer : Editor
	{
		UIPrefabProvider provider;

		void OnEnable()
		{
			provider = (UIPrefabProvider)target;
			provider.uiPrefabs.Clear();
			foreach (UIPrefabType prefabType in Enum.GetValues(typeof(UIPrefabType)))
			{
				var prefab = (UIDesignObject)AssetDatabase.LoadAssetAtPath($"Assets/AtomosZ/UI/BaseUIPrefabs/UI{prefabType}.prefab", typeof(UIDesignObject));
				if (prefab == null)
					Debug.LogWarning("No prefab found for " + prefabType);
				else
					provider.uiPrefabs.Add(prefabType, prefab);
			}
		}
	}
}