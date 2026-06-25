using System;
using UnityEngine;
using static AtomosZ.UI.MagicWindowBase;

namespace AtomosZ.UI
{
	/// <summary>
	/// Remember to set the Layer of this object to UI!<br/>
	/// NOTE(Tristan): is there even a point to this now? I tried to keep everything to an interface to avoid abstract/inherited classes
	/// but with Unity's serialization that is basically impossible without some serious work and jankiness.
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

		public Vector2 minDimensions { get; set; }
		public Vector2 maxDimensions { get; set; }
		public bool fillParentHorizontal { get; set; }
		public bool fillParentVertical { get; set; }

		public UIMonoBehaviour uIMonoBehaviour { get; }
		public Vector2 GetDrawnSize();
		public Vector2 GetPreferredSize();
		public ScriptableObject GetBackingData();
		public void UpdateBackingData(ScriptableObject backingData);
		public void RecalculateDimensions();
		public void SetDirty();
	}
}
