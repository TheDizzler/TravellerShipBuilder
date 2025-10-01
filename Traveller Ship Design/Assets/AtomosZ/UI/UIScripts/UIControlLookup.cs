using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.UI
{
	[Serializable]
	public class CustomDictionary<TKey, TValue> : IDictionary<TKey, TValue>
	{
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

		public ICollection<TKey> Keys { get { return keys; } }
		public ICollection<TValue> Values { get { return values; } }
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
				Debug.LogException(new Exception("UIControlLookup exception: lists are out of sync!"));
			}
		}

		public bool Add(TKey controlRefName, TValue controlDO)
		{
			if (keys.Contains(controlRefName))
				return false;
			keys.Add(controlRefName);
			values.Add(controlDO);
			DEBUG_CheckIntegrity();
			return true;
		}

		/// <summary>
		/// What's the point of this?
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
		{
			if (keys.Contains(key))
				Debug.LogException(new Exception("UIControlLookup exception: key already exists in collection"));
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

	[Serializable]
	public class UIControlLookup : IDictionary<string, UIDesignObject>
	{
		[SerializeField] private List<string> names = new();
		[SerializeField] private List<UIDesignObject> controls = new();

		public UIDesignObject this[string key]
		{
			get
			{
				var index = names.IndexOf(key);
				if (index == -1)
					return null;
				return controls[index];
			}
			set
			{
				var index = names.IndexOf(key);
				if (index == -1)
					return;
				controls[index] = value;
				DEBUG_CheckIntegrity();
			}
		}

		public ICollection<string> Keys { get { return names; } }
		public ICollection<UIDesignObject> Values { get { return controls; } }
		/// <summary>
		/// Key count.
		/// </summary>
		public int Count
		{
			get
			{
				DEBUG_CheckIntegrity();
				return names.Count;
			}
		}

		/// <summary>
		/// Always false. For now?
		/// </summary>
		public bool IsReadOnly { get { return false; } }

		[System.Diagnostics.Conditional("DEBUG")]
		private void DEBUG_CheckIntegrity()
		{
			if (controls.Count != names.Count)
			{
				Debug.LogException(new Exception("UIControlLookup exception: lists are out of sync!"));
			}
		}

		public bool Add(string controlRefName, UIDesignObject controlDO)
		{
			if (names.Contains(controlRefName))
				return false;
			names.Add(controlRefName);
			controls.Add(controlDO);
			DEBUG_CheckIntegrity();
			return true;
		}

		/// <summary>
		/// What's the point of this?
		/// </summary>
		/// <param name="key"></param>
		/// <param name="value"></param>
		void IDictionary<string, UIDesignObject>.Add(string key, UIDesignObject value)
		{
			if (names.Contains(key))
				Debug.LogException(new Exception("UIControlLookup exception: key already exists in collection"));
			names.Add(key);
			controls.Add(value);
			DEBUG_CheckIntegrity();
		}


		public void Add(KeyValuePair<string, UIDesignObject> item)
		{
			if (names.Contains(item.Key))
				return;
			names.Add(item.Key);
			controls.Add(item.Value);
			DEBUG_CheckIntegrity();
		}


		public bool Remove(string key)
		{
			var index = names.IndexOf(key);
			if (index == -1)
				return false;
			names.RemoveAt(index);
			controls.RemoveAt(index);
			DEBUG_CheckIntegrity();
			return true;
		}

		public bool Remove(KeyValuePair<string, UIDesignObject> item)
		{
			var index = names.IndexOf(item.Key);
			if (index == -1)
				return false;
			if (item.Value != controls[index])
				return false;
			names.RemoveAt(index);
			controls.RemoveAt(index);
			DEBUG_CheckIntegrity();
			return true;
		}

		public bool TryGetValue(string key, out UIDesignObject value)
		{
			var index = names.IndexOf(key);
			if (index == -1)
			{
				value = null;
				return false;
			}

			value = controls[index];
			return true;
		}


		/// <summary>
		/// Clears internal list only. Does not destroy any objects.
		/// </summary>
		public void Clear()
		{
			names.Clear();
			controls.Clear();
			DEBUG_CheckIntegrity();
		}

		public bool Contains(KeyValuePair<string, UIDesignObject> item)
		{
			var index = names.IndexOf(item.Key);
			if (index == -1)
				return false;
			if (item.Value != controls[index])
				return false;
			return true;
		}

		public bool ContainsKey(string key)
		{
			var index = names.IndexOf(key);
			if (index == -1)
				return false;
			return true;
		}

		public void CopyTo(KeyValuePair<string, UIDesignObject>[] array, int arrayIndex)
		{
			for (int sourceIndex = arrayIndex, targetIndex = 0; sourceIndex < names.Count; ++sourceIndex, ++targetIndex)
			{
				array[targetIndex] = new KeyValuePair<string, UIDesignObject>(names[sourceIndex], controls[sourceIndex]);
			}
		}

		public IEnumerator<KeyValuePair<string, UIDesignObject>> GetEnumerator()
		{
			for (int i = 0; i < names.Count; ++i)
			{
				yield return new KeyValuePair<string, UIDesignObject>(names[i], controls[i]);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}