using System;
using System.Collections.Generic;
using System.Diagnostics;
using AtomosZ.UI;
using UnityEngine;
using static AtomosZ.ObjectForge;
using Debug = UnityEngine.Debug;


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
					Debug.Log($"PooledObject {((MonoBehaviour)pooledObject).name} was not destroyed proplerly!"
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
		[Obsolete()]
		internal static ObjectForge instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<ObjectForge>();
				return _instance;
			}
		}


		public Transform sleepingPooledObjectsParentTransform;

		[Serializable]
		public class PrefabData
		{
			//public GameObject gameObj;
			public int initialPoolSize;
			public Transform pooledObjectsSleepingParentTransform;
			//public System.Type type;
			public PooledObject pooledObject;
			//public string objectID;
		}

		[Tooltip("The prefab and initial pool size.")]
		public List<PrefabData> pooledPrefabDatas;
		public Dictionary<string, ObjectPool> pools;

		void Awake()
		{
			CreatePools();
		}

		private void CreatePools()
		{
			if (pools == null)
				pools = new();
			if (sleepingPooledObjectsParentTransform == null)
			{
				for (int i = 0; i < transform.childCount; ++i)
				{
					var child = transform.GetChild(i);
					if (child.name == "ObjectPool")
					{
						sleepingPooledObjectsParentTransform = child;
						break;
					}
				}

				if (sleepingPooledObjectsParentTransform == null)
				{
					var poolObj = new GameObject("ObjectPool");
					poolObj.transform.SetParent(transform);
					sleepingPooledObjectsParentTransform = poolObj.transform;
				}
			}

			foreach (var prefab in pooledPrefabDatas)
			{
				if (!pools.TryGetValue(prefab.pooledObject.prefabID, out ObjectPool pool))
				{

					if (prefab.pooledObjectsSleepingParentTransform == null)
						prefab.pooledObjectsSleepingParentTransform = sleepingPooledObjectsParentTransform;
					pool = new ObjectPool(prefab.pooledObject, prefab.initialPoolSize, prefab.pooledObjectsSleepingParentTransform);
					if (string.IsNullOrEmpty(prefab.pooledObject.prefabID))
					{
						Log.Error("prefab objectID must not be null!");
					}

					if (!pools.TryAdd(prefab.pooledObject.prefabID, pool))
						Log.Error($"Prefab ID {prefab.pooledObject.prefabID} is not unique! Could not construct pool.");
				}

				for (int i = 0; i < prefab.pooledObjectsSleepingParentTransform.childCount; ++i)
				{
					var child = prefab.pooledObjectsSleepingParentTransform.GetChild(i);
					if (!child.TryGetComponent(out PooledObject pooledChild)
						|| pooledChild.prefabID != prefab.pooledObject.prefabID)
						continue;

					pool.TryAdd(pooledChild);
				}
			}
		}

		public static ObjectPool GetPoolByID(string poolID)
		{
			return instance.GetPool(poolID);
		}

		public ObjectPool GetPool(string poolID)
		{
#if UNITY_EDITOR
			if (pools == null)
			{
				CreatePools();
			}
#endif

			if (!pools.TryGetValue(poolID, out var pool))
			{
				Log.Error($"No pool found with id {poolID}");
				return null;
			}

			return pool;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="prefab"></param>
		/// <param name="sleepingPooledObjectsParentTrans">Where the sleeping objects are parented.
		/// If null, uses the default set in ObjectForge.</param>
		/// <param name="initialSize"></param>
		/// <returns></returns>
		public ObjectPool CreatePool(PooledObject prefab, int initialSize = 2, Transform sleepingPooledObjectsParentTrans = null)
		{
			var pool = new ObjectPool(prefab, initialSize,
				sleepingPooledObjectsParentTrans == null ? this.sleepingPooledObjectsParentTransform : sleepingPooledObjectsParentTrans);
			pools.Add(prefab.name, pool);
			return pool;
		}


		/// <summary>
		/// Implement IPooledObject&lt;T> unless you want to have a bad time.
		/// </summary>
		public interface IPooledObject
		{
			public bool isLive { get; set; }
			public void ReturnToPool();
		}


			[Obsolete]
		public interface IPooledObject<T> : IPooledObject where T : MonoBehaviour, IPooledObject<T>
		{
			public ObjectPool<T> pool { get; set; }
		}
	
		public interface IObjectPool
		{
			//public void Awake();
			public int Count();
			public void Clear();
		}

		[Serializable]
		public class ObjectPool : IObjectPool
		{
			[SerializeField] private List<PooledObject> pool = new();
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed. If null, a default CreateNew() is used.
			/// </summary>
			/// <returns></returns>
			public delegate PooledObject CreateNewDelegate();
			/// <summary>
			/// Optional. A function to call when an object is woken up with GetNext().
			/// </summary>
			/// <param name="pooledObject"></param>
			public delegate void OnAwakeDelegate(PooledObject pooledObject);
			/// <summary>
			/// Optional. A function
			/// </summary>
			/// <param name="pooledObject"></param>
			public delegate void OnSleepDelegate(PooledObject pooledObject);
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			public CreateNewDelegate CreateNew;
			/// <summary>
			/// Optional. A function to call when an object is woken up with GetNext().
			/// </summary>
			public OnAwakeDelegate OnAwake;
			public OnSleepDelegate OnSleep;
			public PooledObject prefab { get; private set; }
			public Transform sleepingPooledObjectsParentTransform { get; private set; }

			public ObjectPool(PooledObject prefab, int initialSize, Transform sleepingPooledObjectsParentTransform)
			{
				this.prefab = prefab;
				if (sleepingPooledObjectsParentTransform == null)
				{
					Debug.LogException(new Exception("sleepingPooledObjectsParentTransform may not be null"));
				}

				this.sleepingPooledObjectsParentTransform = sleepingPooledObjectsParentTransform;
				CreateNew += () =>
				{
					var result = Instantiate(prefab, sleepingPooledObjectsParentTransform);
					return result;
				};


				for (int objectCount = 0; objectCount < initialSize; ++objectCount)
				{
					PooledObject result = CreateNew();
					pool.Add(result);
					result.pool = this;
					Return(result);
				}
			}

			public int Count()
			{
				return pool.Count;
			}

			/// <summary>
			/// When finished with the PooledObject, use PooledObject.ReturnToPool() and 
			/// nullify the reference (or you're goinbg to have a bad time).
			/// </summary>
			/// <returns></returns>
			public PooledObject GetNext()
			{
#if DEBUG
				if (CreateNew == null)
					CreateNew += () =>
				{
					var result = Instantiate(prefab, sleepingPooledObjectsParentTransform);
					return result;
				};

#endif


				for (int i = 0; i < pool.Count; ++i)
				{
#if UNITY_EDITOR
					if (pool[i] == null)
					{
						Log.Warning("WHERE ARE MY POOLED OBJECTS???!");
						continue;
					}
#endif
					if (!pool[i].isLive)
					{
						var p = pool[i];
						p.isLive = true;
						if (p == null)
						{
							if (Application.isPlaying)
							{
								Log.Warning($"A pooled object has disappeared. This should NOT happen."
									+ " Please use PooledObject.ReturnToPool() to return objects and ObjectPool.Clear() to destroy.");
							}

							p = CreateNew();
							p.pool = this;
						}

						if (OnAwake != null)
							OnAwake(p);

#if UNITY_EDITOR
						sleepingPooledObjectsParentTransform.name = $"ObjectPool ({sleepingPooledObjectsParentTransform.childCount})";
#endif
						return p;
					}
				}


				PooledObject result = CreateNew();
				pool.Add(result);
				result.pool = this;
				result.isLive = true;
				if (OnAwake != null)
					OnAwake(result);
				return result;
			}

			public void Return(PooledObject sleepObject)
			{
				if (OnSleep != null)
					OnSleep(sleepObject);
				sleepObject.gameObject.SetActive(false);
				sleepObject.isLive = false;

				//#if UNITY_EDITOR
				if (sleepingPooledObjectsParentTransform == null)
				{
					Debug.LogError("sleepingPooledObjectsParentTransform not set");
					return;
				}
				//#endif
				sleepObject.transform.SetParent(sleepingPooledObjectsParentTransform);

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

				sleepObject.pool = this;
				pool.Add(sleepObject);

#if UNITY_EDITOR
				sleepingPooledObjectsParentTransform.name = $"ObjectPool ({sleepingPooledObjectsParentTransform.childCount})";
#endif
			}

			public void Clear()
			{
				throw new NotImplementedException();
			}

			internal void TryAdd(PooledObject notSureIfPooled)
			{
				if (pool.Contains(notSureIfPooled))
					return;
				pool.Add(notSureIfPooled);
			}
		}

		[Serializable]
		[Obsolete]
		public class ObjectPool<T> : IObjectPool where T : MonoBehaviour, IPooledObject<T>
		{
			[SerializeField] private List<T> pool = new();
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			/// <returns></returns>
			public delegate T CreateNewDelegate();
			/// <summary>
			/// Optional. A function to call when an object is woken up with GetNext().
			/// </summary>
			/// <param name="t"></param>
			public delegate void OnAwakeDelegate(T t);
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			public CreateNewDelegate CreateNew;
			/// <summary>
			/// Optional. A function to call when an object is woken up with GetNext().
			/// </summary>
			public OnAwakeDelegate OnAwake;
			public PrefabWrapper prefab;
			[Serializable]
			public class PrefabWrapper
			{
				[SerializeField] public MonoBehaviour prefab;
				internal PrefabWrapper(MonoBehaviour prefab)
				{
					this.prefab = prefab;
				}
			}

			/// <summary>
			/// 
			/// </summary>
			/// <param name="prefab"></param>
			/// <param name="initialSize"></param>
			/// <param name="createNew">Optional. The function to call when a new object of this type gets constructed. 
			/// Default is: <code> return Instantiate(prefab, sleepTransform);</code></param>
			internal ObjectPool(T prefab, int initialSize = 2, CreateNewDelegate createNew = null)
			{
				this.prefab = new PrefabWrapper(prefab);
				if (createNew != null)
				{
					CreateNew += createNew;
				}
				else
				{
					CreateNew += () =>
					{
						var result = Instantiate(prefab, ObjectForge.instance.sleepingPooledObjectsParentTransform);
						return result;
					};
				}

				//var allObjects = ObjectForge.instance.sleepingPooledObjectsParentTransform.GetComponentsInChildren<T>(true);

				int objectCount = 0;
				// collect all existing (and sleeping) objects of this type in the pool
				//foreach (var objOfType in allObjects)
				//{
				//	Return(objOfType);
				//	++objectCount;
				//}

				for (; objectCount < initialSize; ++objectCount)
				{
					T result = CreateNew();
					pool.Add(result);
					result.pool = this;
					Return(result);
				}
			}


			/// <summary>
			/// Call this before using if ObjectPool was created at edit time.
			/// </summary>
			//void Awake()
			//{
			//	//FindPrefabsOfType();
			//	if (CreateNew == null)
			//		CreateNew += () =>
			//	{
			//		var result = Instantiate((T)(prefab.prefab), ObjectForge.instance.sleepingPooledObjectsParentTransform);
			//		return result;
			//	};
			//}

			//private void FindPrefabsOfType()
			//{
			//	var t = prefab.prefab.GetType();
			//	var allObjectsOfType = ObjectForge.instance.sleepingPooledObjectsParentTransform.GetComponentsInChildren(t, true);
			//	foreach (T objOfType in allObjectsOfType)
			//	{   // make sure we don't pick up children of pooled objects that we want to keep together
			//		if (objOfType.transform.parent == ObjectForge.instance.sleepingPooledObjectsParentTransform)
			//			Return(objOfType);
			//	}
			//}

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
					var result = Instantiate(((T)prefab.prefab), ObjectForge.instance.sleepingPooledObjectsParentTransform);
					return result;
				};

				//if (!Application.isPlaying)
				//{
				//	FindPrefabsOfType();
				//}
#endif


				for (int i = 0; i < pool.Count; ++i)
				{
#if UNITY_EDITOR
					if (pool[i] == null)
						Log.Warning("WHERE ARE MY POOLED OBJECTS???!");
#endif
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
				if (ObjectForge.instance.sleepingPooledObjectsParentTransform == null)
					Debug.LogError("Dafuq");
#endif
				sleepObject.transform.SetParent(ObjectForge.instance.sleepingPooledObjectsParentTransform);

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

				sleepObject.pool = this;
				pool.Add(sleepObject);

#if UNITY_EDITOR
				//var allObjects = ObjectForge.instance.sleepingPooledObjectsParentTransform.GetComponentsInChildren<MonoBehaviour>(true);
				//var unique = new HashSet<GameObject>();
				//foreach (var obj in allObjects)
				//	unique.Add(obj.gameObject);
				ObjectForge.instance.sleepingPooledObjectsParentTransform.name = $"ObjectPool ({ObjectForge.instance.sleepingPooledObjectsParentTransform.childCount})";
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