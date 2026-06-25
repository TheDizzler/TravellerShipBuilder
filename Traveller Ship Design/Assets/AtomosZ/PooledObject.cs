using System;
using UnityEngine;
using static AtomosZ.ObjectForge;

namespace AtomosZ
{
	[Serializable]
	public class PooledObject : MonoBehaviour
	{
		[HideInInspector]
		public bool isLive;
		[Tooltip("Should the child PooledObjects stay attached to this object or be returned to their own pools?")]
		public bool doNotReturnChildrenToTheirOwnPools;
		public string prefabID;

		[NonSerialized]
		public ObjectPool pool;

		void Awake()
		{
			if (pool == null)
			{
				pool = ObjectForge.GetPoolByID(prefabID);
			}
		}

		public virtual void ReturnToPool()
		{
#if UNITY_EDITOR
			if (pool == null)
			{
				pool = ObjectForge.GetPoolByID(prefabID);
			}
#endif
			if (pool == null)
			{
				Log.Error($"Pool is null on {gameObject.name}! Was this created via an ObjectPool?");
				return;
			}

			if (!doNotReturnChildrenToTheirOwnPools)
			{
				for (int childIndex = transform.childCount - 1; childIndex >= 0; --childIndex)
				//foreach (PooledObject pooledChild in transform.GetComponentsInChildren<PooledObject>(true))
				{
					var pooledChild = transform.GetChild(childIndex).GetComponent<PooledObject>();
					if (pooledChild == this)
						continue;
					pooledChild.ReturnToPool();
				}
			}

			this.Return();
		}

		protected void Return()
		{
//#if UNITY_EDITOR
//			if (Helpers.IsPrefabStage_EDITOR())
//			{ // this may or may not work, depending if the transform is on the base prefab (?)
//			  //GameObject.DestroyImmediate((MonoBehaviour)pooledObject);
//			  //GameObject.Destroy((MonoBehaviour)pooledObject);
//			  //return;
//			}
//#endif

			if (Application.isPlaying)
			{
				if (pool == null)
				{
					Debug.LogError($"Object pool on {name} is null. This is verboten."
						+ "\nPooled objects should never be manually Destroy()ed. Use ObjectPool.Clear() instead!");

					GameObject.Destroy(gameObject);
					return;
				}
			}
			else
			{
				GameObject.DestroyImmediate(gameObject);
				return;
			}

			pool.Return(this);
		}

	}
}
