using System;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;

namespace AtomosZ.UI
{
	[ExecuteInEditMode]
	public class UIMenuDivider : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.MenuDivider; } }
		public bool interactable { get; set; }


		


		internal void ReturnToPool()
		{
			pooledObject.ReturnToPool();
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public override void RecalculateDimensions()
		{
			isDirty = false;
		}

		/// <summary>
		/// This intentionally returns width of 0 if _fillParentHorizontal to prevent confusion of parent panel.
		/// </summary>
		/// <returns></returns>
		public Vector2 GetDrawnSize()
		{
			RecalculateDimensions();
			if (_fillParentHorizontal)
				return new Vector2(0, rect.sizeDelta.y);
			return rect.sizeDelta;
		}

		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return minDimensions;
		}

		public ScriptableObject GetBackingData()
		{
			return null;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}



	}
}