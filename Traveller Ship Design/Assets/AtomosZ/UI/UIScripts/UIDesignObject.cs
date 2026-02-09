using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AtomosZ.UI
{
	/// <summary>
	/// @NOTE(Tristan): while this is mostly vestigial, a class that inherits Monobehavior is required for serialization<br/>
	/// @UPDATE(Tristan): UIMonoBehaviour now has this role.
	/// The only constructive thing this class does now is confirming that UIControls are set to the UI layer.
	/// @TODO(Tristan): remove this MonoBeheviour from all UIControls.
	/// </summary>
	public class UIDesignObject : MonoBehaviour
	{
		private RectTransform _rect;
		public RectTransform rect
		{
			get
			{
				if (_rect == null)
					_rect = GetComponent<RectTransform>();
				return _rect;
			}
		}

		public bool isMoveable = false;
		public bool isModal = false;
		/// <summary>
		/// This is basically mandatory and should not be an option. Only useful for toggling off when creating a new control.
		/// </summary>
		public bool hasCustomDimensions = true;
		public bool hasUpdatableBackingData = false;

		private IUIBehavior uiBehavior;


		public List<string> tooltip;

		void Awake()
		{
			SearchForDesignObject();
		}

		private void SearchForDesignObject()
		{
#if DEBUG
			if (gameObject.layer != 5)
			{
				var trans = transform.parent;
				string name = "";
				while (trans != null)
				{
					name = name.Insert(0, trans.name + ":");
					trans = trans.parent;
				}

				Debug.LogError($"{name}:{gameObject.name} Layer is NOT set to UI!");
			}
#endif

			var components = GetComponents<MonoBehaviour>();
			foreach (var comp in components)
			{
				if (comp is IUIBehavior)
				{
					uiBehavior = (IUIBehavior)comp;
					return;
				}
			}

			if (isMoveable
				|| isModal
				|| hasCustomDimensions
				|| hasUpdatableBackingData)
				throw new Exception("UIDesignObject MUST have a IUIBehavior if any options are enabled!");
		}
	}
}