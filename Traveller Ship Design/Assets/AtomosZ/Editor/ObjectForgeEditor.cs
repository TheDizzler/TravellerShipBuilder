using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static AtomosZ.ObjectForge;

namespace AtomosZ.EditorZ
{
	[CustomEditor(typeof(ObjectForge))]
	public class ObjectForgeEditor : EditorEx
	{
		private ObjectForge forge;

		void OnEnable()
		{
			forge = (ObjectForge)target;
		}

		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			foreach (var prefab in forge.pooledPrefabDatas)
			{
				if (prefab.pooledObject != null)
				{
					//var pooledTypes = new List<System.Type>();
					//var pooledNames = new List<string>();
					PooledObject[] pooledObjects = prefab.pooledObject.GetComponents<PooledObject>();

					if (pooledObjects.Length == 0)
					{
						Log.Error("No PooledObject found on GameObject");
						continue;
					}

					if (pooledObjects.Length > 1)
					{
						Log.Error("More than one PooledObject found on GameObject. This may cause abnormal behaviour. Please remove all but one.");
					}

					prefab.pooledObject = pooledObjects[0];
					//Debug.Log($"{monos[0].ToString()}: type - {monos[0].GetType()}");
				}
			}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{

			}
		}
	}
}