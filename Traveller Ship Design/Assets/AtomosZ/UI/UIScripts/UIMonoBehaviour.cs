using System;
using UnityEditor;
using UnityEngine;
using static AtomosZ.ObjectForge;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	/// <summary>
	/// This class is here to allow for simple serialization of our UI controls.
	/// </summary>
	public class UIMonoBehaviour : MonoBehaviour, IPooledObject<UIMonoBehaviour>
	{
		[SerializeField] protected string _referenceName;// = "no reference name yet";
		/// <summary>
		/// TODO(Tristan): let's tokenize (hash?) this so were are not performing a string lookup on every control.
		/// </summary>
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				SetGameObjectNameToReferenceName();
			}
		}


		private RectTransform _rect;

		public RectTransform rect
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (_rect == null)
					_rect = GetComponent<RectTransform>();
				return _rect;
			}
		}

		public UIMonoBehaviour uIMonoBehaviour { get { return this; } }
		public bool isDirty;

		[SerializeField] protected bool _interactable = true;

		private IUIBehavior _iUIBehavior;
		public IUIBehavior iUIBehavior
		{
			get
			{
				if (_iUIBehavior == null)
					_iUIBehavior = GetComponent<IUIBehavior>();
				return _iUIBehavior;
			}
		}

		public bool isLive { get; set; }
		public ObjectPool<UIMonoBehaviour> pool { get; set; }

		public UIControlType GetDataType() { return iUIBehavior.dataType; }

		void OnDestroy()
		{
			this.OnDestroyPooledObject();
		}


		public void SetDirty()
		{
			if (isDirty)
				return; // by contract, if this is already true then all parents should already have been notified
			isDirty = true;
			if (transform.parent != null)
			{
				var parent = transform.parent.GetComponentInParent<UIMonoBehaviour>();
				if (parent == null)
				{   // assume this is the root and start to refresh (only in edit mode?)
					//uIBehavior.GetMinDimensions();
					return;
				}

				parent.SetDirty();
			}
		}

		private void SetGameObjectNameToReferenceName()
		{
			if (string.IsNullOrEmpty(referenceName))
				referenceName = gameObject.name;
#if UNITY_EDITOR
			// Prefabs need to maintain their prefab name
			if (!Helpers.IsPrefabStage())
				gameObject.name = referenceName;
#else
			if (gameObject.scene.IsValid()) // this line is probably unnecessary
				gameObject.name = referenceName;
#endif
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void RecordPrefabInstances()
		{
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
		}

		public virtual void ReturnToPool()
		{
			if (pool == null)
				pool = UIPrefabProvider.GetPoolOfType(iUIBehavior.dataType);


			foreach (var pooledChild in transform.GetComponentsInChildren<UIMonoBehaviour>())
			{
				if (pooledChild == this)
					continue;
				pooledChild.ReturnToPool();
			}

			this.Return();
		}
	}
}