using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.UI;
using UnityEngine;

using static AtomosZ.Keyboard;

namespace AtomosZ.UI
{
	/// <summary>
	/// Remember to set the Layer of this object to UI!
	/// </summary>
	public interface IUIBehavior
	{
		public UIControlType dataType { get; }
		/// <summary>
		/// Replace this auto-generated property with the following:
		/// <code>
		/// public UIDesignObject _designObject;
		/// public UIDesignObject designObject
		/// {
		///		get
		/// 	{
		/// 		if (_designObject == null)
		/// 			_designObject = GetComponent&lt;UIDesignObject>();
		/// 		return _designObject;
		///		}
		/// }
		/// </code>
		/// </summary>
		public UIDesignObject designObject { get; }
		public string referenceName { get; set; }
		public bool isDirty { get; set; }

		public IUIBehavior GetControl(string controlRefName);

		public void SetHover(bool isHover);
		public void UpdateHover(Vector3 posOfHover);
		public void ResetToLastPosition();
		/// <summary>
		/// Actions (usually UI feedback?) to take when an object is selected.
		/// If this object is not itself selectable, returns a parent or a related object that is part of this "group".
		/// </summary>
		/// <returns></returns>
		public UIDesignObject Select();

		public void Deselect();
		/// <summary>
		/// TODO(Tristan): This appears to be never used. Is this vestigial from IBehvaior object? How is it different from Select()?
		/// Actions to take when the user left clicks on the object.
		/// </summary>
		/// <param name="mouseWorldPos">This <i><b>should</b></i> be on or at least within the vicinity of this object.</param>
		/// <param name="keyInput"></param>
		public void Clicked(Vector3 mouseWorldPos, ModifierKey keyInput, ref UIDesignObject currentlySelectedObject);
		//public void RecalculateDimension();
		public Vector2 GetMinDimensions();
		public IUIDataEx GetBackingData();
		public void UpdateBackingData(IUIDataEx backingData);
		public void UpdateBackingData();
	}

	public static class IUIBehaviorExtensions
	{
		/// <summary>
		/// Recurse up object tree flagging all parents that this object is dirty.
		/// </summary>
		/// <param name="uIBehavior"></param>
		public static void SetDirty(this IUIBehavior uIBehavior)
		{
			if (uIBehavior.isDirty)
				return; // by contract, if this is already true then all parents should already have been notified
			uIBehavior.isDirty = true;
			if (uIBehavior.designObject == null)
				Debug.LogException(new Exception($"Why is {uIBehavior.referenceName}'s design object null?"));
			if (uIBehavior.designObject.transform.parent != null)
			{
				var parent = uIBehavior.designObject.transform.parent.GetComponentInParent<IUIBehavior>();
				if (parent == null)
				{   // assume this is the root and start to refresh (only in edit mode?)
					//uIBehavior.GetMinDimensions();
					return;
				}

				if (parent.designObject == null)
					Debug.LogException(new Exception($"{parent.referenceName} is null???"));
				parent.SetDirty();
			}
		}

		internal static void SetGameObjectNameToReferenceName(this IUIBehavior uiBehavior, GameObject gameObject)
		{
			if (string.IsNullOrEmpty(uiBehavior.referenceName))
				uiBehavior.referenceName = gameObject.name;
#if UNITY_EDITOR
			// Prefabs need to maintain their prefab name
			var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			if (stage == null)
				gameObject.name = uiBehavior.referenceName;
#else
			if (gameObject.scene.IsValid()) // this line is probably unnecessary
				gameObject.name = uiBehavior.referenceName;
#endif
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public static void RecordPrefabInstances(this IUIBehavior uiBehavior)
		{
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(uiBehavior.designObject.gameObject);
		}
	}
}
