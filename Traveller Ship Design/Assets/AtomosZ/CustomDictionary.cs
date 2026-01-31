using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace AtomosZ
{
	public class CustomDictionary { }

	[Serializable]
	public class CustomDictionary<TKey, TValue> : CustomDictionary, IDictionary<TKey, TValue>
	{
		/// <summary>
		/// Changing this to hashset would speed up searches for large lists,
		/// but would make searching by index slow.
		/// </summary>
		public List<TKey> keys = new();
		public List<TValue> values = new();

		[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
		public TValue this[TKey key]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				var index = keys.IndexOf(key);
				if (index == -1)
					//return default;
					Debug.LogException(new Exception($"CustomDictionary exception: key {key} does not exist in collection"));
				return values[index];
			}
			set
			{
				var index = keys.IndexOf(key);
				if (index == -1)
					Add(key, value);
				else
					values[index] = value;
				DEBUG_CheckIntegrity();
			}
		}


		public TKey this[TValue tValue]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				var index = values.IndexOf(tValue);
				if (index == -1)
					Debug.LogException(new Exception($"CustomDictionary exception: value {tValue} does not exist in collection."));
				//return default;
				return keys[index];
			}
		}

		public KeyValuePair<TKey, TValue> this[int index]
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (index == -1 || index >= keys.Count)
					Debug.LogException(new Exception($"CustomDictionary exception: index {index} does not exist in collection."));
				//return default;
				return new KeyValuePair<TKey, TValue>(keys[index], values[index]);
			}
		}

		public ICollection<TKey> Keys { get { return keys; } }
		public ICollection<TValue> Values { get { return values; } }

		//public TValue GetValueAtIndex(int i)
		//{
		//	return values[i];
		//}

		/// <summary>
		/// Key count.
		/// </summary>
		public int Count
		{
			get
			{
				DEBUG_CheckIntegrity();
				return keys.Count;
			}
		}

		/// <summary>
		/// Always false. For now?
		/// </summary>
		public bool IsReadOnly { get { return false; } }

		[System.Diagnostics.Conditional("DEBUG")]
		private void DEBUG_CheckIntegrity()
		{
			if (values.Count != keys.Count)
			{
				Debug.LogException(new Exception("CustomDictionary exception: lists are out of sync!"));
			}
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(TKey key, TValue value)
		{
			if (keys.Contains(key))
			{
				Debug.Log($"CustomDictionary exception: key {key} already exists in collection");
				this[key] = value;
			}
			else
			{
				keys.Add(key);
				values.Add(value);
			}
			DEBUG_CheckIntegrity();
		}


		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (keys.Contains(item.Key))
			{
				Debug.Log(($"CustomDictionary exception: key {item.Key} already exists in collection"));
				this[item.Key] = item.Value;
			}
			else
			{
				keys.Add(item.Key);
				values.Add(item.Value);
			}
			DEBUG_CheckIntegrity();
		}

		/// <summary>
		/// Adds the keys to the dictionary with an initial default value.
		/// </summary>
		/// <param name="keys"></param>
		/// <param name="initValue"></param>
		public void AddRange(IEnumerable<TKey> keys, TValue initValue)
		{
			foreach (var key in keys)
				Add(key, initValue);
		}

		public void AddRange(CustomDictionary<TKey, TValue> values)
		{
			foreach (var val in values)
				Add(val);
		}

		public bool Remove(TKey key)
		{
			var index = keys.IndexOf(key);
			if (index == -1)
				return false;
			keys.RemoveAt(index);
			values.RemoveAt(index);
			DEBUG_CheckIntegrity();
			return true;
		}

		public bool Remove(KeyValuePair<TKey, TValue> item)
		{
			var index = keys.IndexOf(item.Key);
			if (index == -1)
				return false;
			if (!item.Value.Equals(values[index]))
				return false;
			keys.RemoveAt(index);
			values.RemoveAt(index);
			DEBUG_CheckIntegrity();
			return true;
		}

		public bool TryGetValue(TKey key, out TValue value)
		{
			var index = keys.IndexOf(key);
			if (index == -1)
			{
				value = default;
				return false;
			}

			value = values[index];
			return true;
		}


		/// <summary>
		/// Clears internal list only. Does not destroy any objects.
		/// </summary>
		public void Clear()
		{
			keys.Clear();
			values.Clear();
			DEBUG_CheckIntegrity();
		}

		public bool Contains(KeyValuePair<TKey, TValue> item)
		{
			var index = keys.IndexOf(item.Key);
			if (index == -1)
				return false;
			if (!item.Value.Equals(values[index]))
				return false;
			return true;
		}

		public bool ContainsKey(TKey key)
		{
			var index = keys.IndexOf(key);
			if (index == -1)
				return false;
			return true;
		}

		public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
		{
			for (int sourceIndex = arrayIndex, targetIndex = 0; sourceIndex < keys.Count; ++sourceIndex, ++targetIndex)
			{
				array[targetIndex] = new KeyValuePair<TKey, TValue>(keys[sourceIndex], values[sourceIndex]);
			}
		}

		public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
		{
			for (int i = 0; i < keys.Count; ++i)
			{
				yield return new KeyValuePair<TKey, TValue>(keys[i], values[i]);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public List<TKey> GetKeysFromValue(TValue value)
		{
			var foundKeys = new List<TKey>();
			for (int i = 0; i < keys.Count; ++i)
			{
				if (values[i].Equals(value))
					foundKeys.Add(keys[i]);
			}

			return foundKeys;
		}
	}
}