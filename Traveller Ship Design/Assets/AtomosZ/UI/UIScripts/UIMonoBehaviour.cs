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
		protected bool isDirty;

		[SerializeField] protected bool _interactable = true;

		private IUIBehavior _iUIBehavior;
		public IUIBehavior iUIBehavior
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (_iUIBehavior == null)
					_iUIBehavior = GetComponent<IUIBehavior>();
				return _iUIBehavior;
			}
		}

		//[Min(1)] // this doesn't actually work, does it.
		//[SerializeField] protected Vector2 _minDimensions = new Vector2(64, 64);

		//[Min(1)]
		//[SerializeField] protected Vector2 _maxDimensions = new Vector2(512, 512);


		[SerializeField] protected bool _fillParentHorizontal = false;
		public bool fillParentHorizontal
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return _fillParentHorizontal; }
			set
			{
				_fillParentHorizontal = value;
				if (value)
				{
#if UNITY_EDITOR
					if (transform.parent == null)
					{
						_fillParentHorizontal = false;
						return;
					}
#endif
					// this may be unneccessary as layout.flexibleWidth = 1 does the same thing (but only on Vertical Layout?)
					var parentSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
					iUIBehavior.minDimensions = new Vector2(parentSize.x, iUIBehavior.minDimensions.y);
					iUIBehavior.maxDimensions = new Vector2(parentSize.x, iUIBehavior.maxDimensions.y);
				}

				this.SetDirty();
			}
		}

		[SerializeField] protected bool _fillParentVertical = false;
		public bool fillParentVertical
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return _fillParentVertical; }
			set
			{
				_fillParentVertical = value;
				if (_fillParentVertical)
				{
#if UNITY_EDITOR
					if (transform.parent == null)
					{
						_fillParentVertical = false;
						return;
					}
#endif
					var parentSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
					iUIBehavior.minDimensions = new Vector2(iUIBehavior.minDimensions.x, parentSize.y);
					iUIBehavior.maxDimensions = new Vector2(iUIBehavior.maxDimensions.x, parentSize.y);
				}
				this.SetDirty();
			}
		}

		// Object pool variables
		public bool isLive { get; set; }
		public ObjectPool<UIMonoBehaviour> pool { get; set; }
		//

		[System.Diagnostics.DebuggerStepThrough]
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
					//uIBehavior.GetDrawnDimensions();
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