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
		/// Changing this to hashset would speed up searches for large lists.
		/// </summary>
		public List<TKey> keys = new();
		public List<TValue> values = new();

		public TValue this[TKey key]
		{
			get
			{
				var index = keys.IndexOf(key);
				if (index == -1)
					return default;
				return values[index];
			}
			set
			{
				var index = keys.IndexOf(key);
				if (index == -1)
					return;
				values[index] = value;
				DEBUG_CheckIntegrity();
			}
		}


		public TKey this[TValue key]
		{
			get
			{
				var index = values.IndexOf(key);
				if (index == -1)
					return default;
				return keys[index];
			}
		}

		public KeyValuePair<TKey, TValue> this[int index]
		{
			get
			{
				if (index == -1 || index >= keys.Count)
					return default;
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
		/// What's the point of this?
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void Add(TKey key, TValue value)
		{
			if (keys.Contains(key))
				Debug.LogException(new Exception("CustomDictionary exception: key already exists in collection"));
			keys.Add(key);
			values.Add(value);
			DEBUG_CheckIntegrity();
		}


		public void Add(KeyValuePair<TKey, TValue> item)
		{
			if (keys.Contains(item.Key))
				return;
			keys.Add(item.Key);
			values.Add(item.Value);
			DEBUG_CheckIntegrity();
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
	}
}