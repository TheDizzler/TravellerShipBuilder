using System;
using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	/// <summary>
	/// Remember to set the Layer of this object to UI!
	/// </summary>
	public interface IUIBehavior
	{
		public UIControlType dataType { get; }
		public string referenceName { get; set; }

		public GameObject gameObject { get; }
		public UIMonoBehaviour GetControl(string controlRefName);

		/// <summary>
		/// <code>
		/// public bool interactable
		/// {
		///	get { return _interactable; }
		///	set
		///	{
		///		_interactable = value;
		///	}
		///}</code>
		/// </summary>
		public bool interactable { get; set; }

		public UIMonoBehaviour uIMonoBehaviour { get; }
		public Vector2 GetMinDimensions();
		public ScriptableObject GetBackingData();
		public void UpdateBackingData(ScriptableObject backingData);
		public void UpdateBackingData();
		public void SetDirty();
	}
}
