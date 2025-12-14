using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;


namespace AtomosZ
{
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

		public interface PooledObject<T> where T : MonoBehaviour
		{
			public int poolID { get; set; }
			internal bool isLive { get; set; }
		}

		[Serializable]
		public class ObjectPoolTest<T> where T : MonoBehaviour, PooledObject<T>
		{
			public Transform sleepTransform = ObjectForge.instance.sleepingPooledObjects;

			[SerializeField] private List<T> pool = new();
			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			/// <returns></returns>
			public delegate T CreateNewDelegate();
			public CreateNewDelegate CreateNew;
			public T prefab;

			public ObjectPoolTest(T prefab)
			{
				this.prefab = prefab;
				CreateNew += () =>
				{
					var result = Instantiate(prefab, sleepTransform);
					return result;
				};
			}


			public T GetNext()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (!pool[i].isLive)
					{
						pool[i].isLive = true;
						return pool[i];
					}
				}

#if DEBUG
				if (CreateNew == null)
					CreateNew += () =>
				{
					var result = Instantiate(prefab, sleepTransform);
					return result;
				};
#endif

				T result = CreateNew();
				result.poolID = pool.Count;
				result.isLive = true;
				pool.Add(result);
				return result;
			}

			[Conditional("DEBUG")]
			public void WakeAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					pool[i].isLive = true;
					pool[i].gameObject.SetActive(true);
				}
			}

			public void ReturnAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					Return(pool[i]);
				}
			}

			public void Return(T obj)
			{
				obj.isLive = false;
				obj.gameObject.SetActive(false);
				obj.transform.SetParent(sleepTransform);
			}

			public void Return(int id)
			{
				var obj = pool[id];
				obj.isLive = false;
				obj.gameObject.SetActive(false);
			}

			/// <summary>
			/// Destroys all gameobjects in pool.
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
						DestroyImmediate(pool[i].gameObject);
					}
					else
						Destroy(pool[i].gameObject);
#else
				Destroy(pool[i].gameObject);
#endif
				}

				pool.Clear();
			}
		}

		public Transform sleepingPooledObjects;

		public ObjectPool<T> CreateObjectPool<T>(ObjectPool<T>.CreateNewDelegate createNewFunction)
		{
			var newPool = new ObjectPool<T>(createNewFunction);
			return newPool;
		}

		public MonoBehaviourObjectPool<T> CreateObjectPool<T>(T prefab) where T : MonoBehaviour
		{
			var newPool = new MonoBehaviourObjectPool<T>(prefab);
			return newPool;
		}

		public MonoBehaviourObjectPool<T> CreateObjectPool<T>(MonoBehaviourObjectPool<T>.CreateNewDelegate createNewFunction) where T : MonoBehaviour
		{
			var newPool = new MonoBehaviourObjectPool<T>(createNewFunction);
			return newPool;
		}


		[Serializable]
		public class ObjectPool { };
		public class MonoBehaviourObjectPool<T> : ObjectPool where T : MonoBehaviour
		{
			[Serializable]
			private class ObjectLive
			{
				public T mb;
				public bool isLive;
			}

			[SerializeField] private List<ObjectLive> pool = new List<ObjectLive>();

			/// <summary>
			/// Optional. The function to call when a new object of this type gets constructed.
			/// </summary>
			/// <returns></returns>
			public delegate T CreateNewDelegate();
			private CreateNewDelegate CreateNew;


			/// <summary>
			/// Optional. The function to call when the object is awoken (isLive becomes true).
			/// </summary>
			/// <param name="obj"></param>
			public delegate void AwakeDelegate(MonoBehaviour obj);
			public AwakeDelegate OnAwake;

			/// <summary>
			/// Optional. The function to call when the object is put to sleep (isLive becomes false).
			/// </summary>
			/// <param name="obj"></param>
			public delegate void SleepDelegate(MonoBehaviour obj);
			public SleepDelegate OnSleep;

			/// <summary>
			/// Optional. The function to call when the objects are destroyed permanently.
			/// </summary>
			/// <param name="obj"></param>
			public delegate void OnDestroyDelegate(MonoBehaviour obj);
			public OnDestroyDelegate OnDestroy;
			private T prefab;

			public MonoBehaviourObjectPool(CreateNewDelegate createNewMethod,
				AwakeDelegate awakeMethod = null, SleepDelegate sleepMethod = null, OnDestroyDelegate deleteMethod = null)
			{
				if (createNewMethod != null)
					CreateNew += createNewMethod;

				if (awakeMethod != null)
					OnAwake += awakeMethod;
				else
					OnAwake += WakeMonoBehaviour;
				if (sleepMethod != null)
					OnSleep += sleepMethod;
				else
					OnSleep += SleepMonoBehaviour;
				if (deleteMethod != null)
					OnDestroy += deleteMethod;
				else
					OnDestroy += DestroyMonoBehaviour;
			}

			public MonoBehaviourObjectPool(T prefab)
			{
				this.prefab = prefab;
			}

			private T CreateNewWithPrefab()
			{
				var result = Instantiate(prefab);
				return result;
			}

			public T GetNext()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (!pool[i].isLive)
					{
						pool[i].isLive = true;
						if (OnAwake != null)
							OnAwake(pool[i].mb);
						return pool[i].mb;
					}
				}

				T result;
				if (CreateNew() != null)
					result = CreateNew();
				else
					result = CreateNewWithPrefab();
				pool.Add(new ObjectLive { mb = result, isLive = true });
				if (OnAwake != null)
					OnAwake(result);
				return result;
			}

			public void WakeAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					pool[i].isLive = true;
					if (OnAwake != null)
						OnAwake(pool[i].mb);
				}
			}

			public void SleepAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					pool[i].isLive = false;
					if (OnSleep != null)
						OnSleep(pool[i].mb);
				}
			}

			public void Return(MonoBehaviour obj)
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (pool[i].mb.Equals(obj))
					{
						pool[i].isLive = false;
						if (OnSleep != null)
							OnSleep(obj);
						break;
					}
				}
			}

			public void Clear()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (OnDestroy != null)
						OnDestroy(pool[i].mb);
				}

				pool.Clear();
			}

			/// <summary>
			/// <c>obj.gameObject.SetActive(true);</c>
			/// </summary>
			/// <param name="obj"></param>
			internal void WakeMonoBehaviour(MonoBehaviour obj)
			{
				obj.gameObject.SetActive(true);
			}

			internal void SleepMonoBehaviour(MonoBehaviour obj)
			{
				obj.gameObject.SetActive(false);
				obj.transform.SetParent(ObjectForge.instance.sleepingPooledObjects);
			}

			internal void DestroyMonoBehaviour(MonoBehaviour obj)
			{
#if DEBUG
				if (!Application.isPlaying)
					DestroyImmediate(obj.gameObject);
				else
					Destroy(obj.gameObject);
#else
				Destroy(obj.gameObject);
#endif
			}
		}

		[Serializable]
		public class ObjectPool<T> : ObjectPool
		{
			[Serializable]
			private class ObjectLive
			{
				public T obj;
				public bool isLive;
			}
			[SerializeField] private List<ObjectLive> pool = new List<ObjectLive>();

			/// <summary>
			/// Obligatory (set in constructor). The function to call when a new object of this type gets constructed.
			/// </summary>
			/// <returns></returns>
			public delegate T CreateNewDelegate();
			private CreateNewDelegate CreateNew;

			/// <summary>
			/// Optional. The function to call when the object is awoken (isLive becomes true).
			/// </summary>
			/// <param name="obj"></param>
			public delegate void AwakeDelegate(T obj);
			public AwakeDelegate OnAwake;

			/// <summary>
			/// Optional. The function to call when the object is put to sleep (isLive becomes false).
			/// </summary>
			/// <param name="obj"></param>
			public delegate void SleepDelegate(T obj);
			public SleepDelegate OnSleep;

			/// <summary>
			/// Optional. The function to call when the objects are destroyed permanently.
			/// </summary>
			/// <param name="obj"></param>
			public delegate void OnDestroyDelegate(T obj);
			public OnDestroyDelegate OnDestroy;


			public ObjectPool(CreateNewDelegate createNewFunction)
			{
				CreateNew += createNewFunction;
			}

			public T GetNext()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (!pool[i].isLive)
					{
						pool[i].isLive = true;
						if (OnAwake != null)
							OnAwake(pool[i].obj);
						return pool[i].obj;
					}
				}

				var result = CreateNew();
				pool.Add(new ObjectLive { obj = result, isLive = true });
				if (OnAwake != null)
					OnAwake(result);
				return result;
			}

			public void WakeAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					pool[i].isLive = true;
					if (OnAwake != null)
						OnAwake(pool[i].obj);
				}
			}

			public void SleepAll()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					pool[i].isLive = false;
					if (OnSleep != null)
						OnSleep(pool[i].obj);
				}
			}

			public void Return(T obj)
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (pool[i].obj.Equals(obj))
					{
						pool[i].isLive = false;
						if (OnSleep != null)
							OnSleep(obj);
						break;
					}
				}
			}

			public void Clear()
			{
				for (int i = 0; i < pool.Count; ++i)
				{
					if (OnDestroy != null)
						OnDestroy(pool[i].obj);
				}

				pool.Clear();
			}

			internal void WakeUnityGameObject(T obj)
			{
				var unityObj = obj as UnityEngine.GameObject;
				unityObj.SetActive(true);
			}


			internal void SleepUnityGameObject(T obj)
			{
				var unityObj = obj as UnityEngine.GameObject;
				unityObj.SetActive(false);
				unityObj.transform.SetParent(ObjectForge.instance.sleepingPooledObjects);
			}


			internal void DestroyGameObject(T obj)
			{
				var gameObject = obj as GameObject;
#if DEBUG
				if (!Application.isPlaying)
					DestroyImmediate(gameObject);
				else
					Destroy(gameObject);
#else
				Destroy(obj);
#endif
			}

			internal void DestroyUnityObject(T obj)
			{
				var unityObj = obj as UnityEngine.Object;
#if DEBUG
				if (!Application.isPlaying)
					DestroyImmediate(unityObj);
				else
					Destroy(unityObj);
#else
				Destroy(obj);
#endif
			}
		}
	}
}