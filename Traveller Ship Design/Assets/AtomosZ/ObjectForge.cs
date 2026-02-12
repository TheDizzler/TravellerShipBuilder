using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using AtomosZ.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using static AtomosZ.ObjectForge;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;


namespace AtomosZ
{
	public static class PooledObjectExt
	{
		/// <summary>
		/// Use ReturnToPool().
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="pooledObject"></param>
		internal static void Return<T>(this IPooledObject<T> pooledObject) where T : MonoBehaviour, IPooledObject<T>
		{
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
			{ // this may or may not work, depending if the transform is on the base prefab (?)
			  //GameObject.DestroyImmediate((MonoBehaviour)pooledObject);
			  //GameObject.Destroy((MonoBehaviour)pooledObject);
			  //return;
			}
#endif

			if (pooledObject.pool == null)
			{
				Debug.LogError($"Object pool on {((MonoBehaviour)pooledObject).name} is null. This is verboten."
					+ "\nPooled objects should never be manually Destroy()ed. Use ObjectPool.Clear() instead!");

				if (Application.isPlaying)
					GameObject.Destroy(((MonoBehaviour)pooledObject).gameObject);
				else
					GameObject.DestroyImmediate(((MonoBehaviour)pooledObject).gameObject);
				return;
			}

			pooledObject.pool.Return((T)pooledObject);
		}

		public static void OnDestroyPooledObject<T>(this IPooledObject<T> pooledObject) where T : MonoBehaviour, IPooledObject<T>
		{
			if (pooledObject.pool != null)
			{
				if (Application.isPlaying)
				{
					Log.Error($"PooledObject {((MonoBehaviour)pooledObject).name} was not destroyed proplerly!"
						+ "\nPooled objects should never be manually Destroy()ed. Use ObjectPool.Clear() instead!");
				}

				pooledObject.pool.ReportDeleted((T)pooledObject);
			}
		}

		public static void OnSceneGUI<T>(this IPooledObject<T> pooledObject) where T : MonoBehaviour, IPooledObject<T>
		{
			Log.Warning("test");
		}
	}

	public class ObjectForge : MonoBehaviour
	{
		private static ObjectForge _instance;
		public static ObjectForge instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<ObjectForge>();
				return _instance;
			}
		}


		public Transform sleepingPooledObjects;

		/// <summary>
		/// Implement IPooledObject&lt;T> unless you want to have a bad time.
		/// </summary>
		public interface IPooledObject
		{
			public bool isLive { get; set; }
			public void ReturnToPool();
		}

		public interface IPooledObject<T> : IPooledObject where T : MonoBehaviour, IPooledObject<T>
		{
			public ObjectPool<T> pool { get; set; }
		}

		public interface IObjectPool
		{
			public int Count();
			public void Clear();
		}

		[Serializable]
		public class ObjectPool<T> : IObjectPool where T : MonoBehaviour, IPooledObject<T>
		{
			[SerializeField] private List<T> pool = new();
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			/// <returns></returns>
			public delegate T CreateNewDelegate();
			public delegate void OnAwakeDelegate(T t);
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			public CreateNewDelegate CreateNew;
			public OnAwakeDelegate OnAwake;
			public T prefab;


			/// <summary>
			/// 
			/// </summary>
			/// <param name="prefab"></param>
			/// <param name="initialSize"></param>
			/// <param name="createNew">Optional. The function to call when a new object of this type gets constructed. 
			/// Default is: <code> return Instantiate(prefab, sleepTransform);</code></param>
			public ObjectPool(T prefab, int initialSize = 2, CreateNewDelegate createNew = null)
			{
				this.prefab = prefab;
				if (createNew != null)
				{
					CreateNew += createNew;
				}
				else
				{
					CreateNew += () =>
					{
						var result = Instantiate(prefab, ObjectForge.instance.sleepingPooledObjects);
						return result;
					};
				}

				var allObjects = ObjectForge.instance.sleepingPooledObjects.GetComponentsInChildren<T>(true);

				int objectCount = 0;
				// collect all existing (and sleeping) objects of this type in the pool
				foreach (var objOfType in allObjects)
				{
					Return(objOfType);
					++objectCount;
				}

				for (; objectCount < initialSize; ++objectCount)
				{
					T result = CreateNew();
					pool.Add(result);
					result.pool = this;
					Return(result);
				}
			}

			public int Count()
			{
				return pool.Count;
			}

			public T GetNext()
			{
#if DEBUG
				if (CreateNew == null)
					CreateNew += () =>
				{
					var result = Instantiate(prefab, ObjectForge.instance.sleepingPooledObjects);
					return result;
				};

				if (!Application.isPlaying)
				{
					var t = prefab.GetType();
					var allObjectsOfType = ObjectForge.instance.sleepingPooledObjects.GetComponentsInChildren(t, true);
					foreach (T objOfType in allObjectsOfType)
					{   // make sure we don't pick up children of pooled objects that we want to keep together
						if (objOfType.transform.parent == ObjectForge.instance.sleepingPooledObjects)
							Return(objOfType);
					}
				}
#endif


				for (int i = 0; i < pool.Count; ++i)
				{
					if (!pool[i].isLive)
					{
						var p = pool[i];
						p.isLive = true;
						if (p == null)
						{
							if (Application.isPlaying)
							{
								Log.Warning($"A pooled object of type {typeof(T)} has disappeared. This should NOT happen."
									+ " Please use IPooledObject.ReturnToPool() to return objects and ObjectPool.Clear() to destroy.");
							}

							p = CreateNew();
							p.pool = this;
						}

						if (OnAwake != null)
							OnAwake(p);
						return p;
					}
				}


				T result = CreateNew();
				pool.Add(result);
				result.pool = this;
				result.isLive = true;
				if (OnAwake != null)
					OnAwake(result);
				return result;
			}

			[Conditional("DEBUG")]
			public void WakeAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					var p = pool[i];
					p.isLive = true;
					p.gameObject.SetActive(true);
				}
			}


			public void ReturnAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					Return(pool[i]);
				}
			}


			public void Return(T sleepObject)
			{
				sleepObject.gameObject.SetActive(false);
				sleepObject.isLive = false;

#if UNITY_EDITOR
				if (ObjectForge.instance.sleepingPooledObjects == null)
					Debug.LogError("Daduf");
#endif
				sleepObject.transform.SetParent(ObjectForge.instance.sleepingPooledObjects);

				foreach (var obj in pool)
				{
					if (obj == sleepObject)
					{
						return;
					}
				}

				// object was not found in pool. This COULD be an issue, but necessarily.
				// if the object was created in the editor it might not have been initialized from the pool.
				// I think adding it to the pool now should not be a problem.

				pool.Add(sleepObject);

#if UNITY_EDITOR
				var allObjects = ObjectForge.instance.sleepingPooledObjects.GetComponentsInChildren<MonoBehaviour>(true);
				var unique = new HashSet<GameObject>();
				foreach (var obj in allObjects)
					unique.Add(obj.gameObject);
				ObjectForge.instance.sleepingPooledObjects.name = $"ObjectPool ({unique.Count})";
#endif
			}


			public void ReportDeleted(T deletedObj)
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					var obj = pool[i];
					if (obj == deletedObj)
					{
						pool.Remove(obj);
						return;
					}
				}
			}

			/// <summary>
			/// Destroys all gameobjects in pool, unless they are alive.
			/// </summary>
			public void Clear()
			{
				if (pool == null)
					return;
				for (int i = 0; i < pool.Count; ++i)
				{
#if DEBUG
					if (!Application.isPlaying)
					{
						if (pool[i] == null)
							continue;

						pool[i].pool = null;
						if (!pool[i].isLive)
							DestroyImmediate(pool[i].gameObject);
						continue;
					}

#else
					pool[i].pool = null;
					if (!pool[i].isLive)
						Destroy(pool[i].gameObject);
#endif
				}

				pool.Clear();
			}
		}
	}
}