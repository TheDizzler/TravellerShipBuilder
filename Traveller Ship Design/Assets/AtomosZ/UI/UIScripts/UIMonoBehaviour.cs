using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.ObjectForge;
using static AtomosZ.UI.MagicWindowBase;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	/// <summary>
	/// This class is here to allow for simple serialization of our UI controls.
	/// </summary>
	public abstract class UIMonoBehaviour : MonoBehaviour
	{
		[SerializeField] protected string _referenceName;// = "no reference name yet";
		[Tooltip("TODO(Tristan): let's tokenize (hash?) this so were are not performing a string lookup on every control.")]
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				SetGameObjectNameToReferenceName();
			}
		}

		private PooledObject _pooledObject;
		public PooledObject pooledObject
		{
			get
			{
				if (_pooledObject == null)
				{
					if (!TryGetComponent<PooledObject>(out _pooledObject))
						Debug.LogError(name + " has no pooled object!");
				}

				return _pooledObject;
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


		[SerializeField] protected Vector2 _minDimensions = new Vector2(64, 16);
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				//value.x = Mathf.Min(value.x, maxDimensions.x);
				//value.y = Mathf.Min(value.y, maxDimensions.y);
				value.x = Mathf.Max(value.x, 8);
				value.y = Mathf.Max(value.y, 8);
				_minDimensions = value;
				if (layoutElement == null)
				{
					Debug.LogError(name + " does not have a LayoutElement yet");
				}
				else
				{
					layoutElement.minWidth = minDimensions.x;
					layoutElement.minHeight = minDimensions.y;
				}
				this.SetDirty();
			}
		}


		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		[SerializeField] protected Vector2 _maxDimensions = new Vector2(1025, 256);


		[Tooltip("Max height may cause issues with reported height when TextWrappingMode is set to Normal.")]
		public Vector2 maxDimensions
		{
			get { return _maxDimensions; }
			set
			{
				//				if (_fillParentVertical)
				//				{
				//#if UNITY_EDITOR
				//					if (transform.parent == null)
				//					{
				//						_fillParentVertical = false;
				//						return;
				//					}
				//#endif
				//					var parentSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
				//					_minDimensions = new Vector2(_minDimensions.x, parentSize.y);
				//					_maxDimensions = new Vector2(_maxDimensions.x, parentSize.y);
				//				}
				//				else
				//				{
				value.x = Mathf.Max(value.x, minDimensions.x);
				value.y = Mathf.Max(value.y, minDimensions.y);
				value.x = Mathf.Max(value.x, 8);
				value.y = Mathf.Max(value.y, 8);
				_maxDimensions = value;
				//if (layoutElement == null)
				//	Debug.LogError(name + " has no LayoutElement");
				//else
				//{
				//	if (!fillParentHorizontal)
				//		layoutElement.preferredWidth = value.x;
				//	else
				//		layoutElement.preferredWidth = -1;
				//}

				this.SetDirty();
			}
		}


		private LayoutElement _layoutElement;
		public LayoutElement layoutElement
		{
			get
			{
				if (_layoutElement == null)
					_layoutElement = GetComponent<LayoutElement>();
				return _layoutElement;
			}
		}

		[SerializeField] protected bool _fillParentHorizontal = false;
		public bool fillParentHorizontal
		{
			[System.Diagnostics.DebuggerStepThrough]
			get
			{
				if (layoutElement == null)     // @TODO(Tristan): add layout to all UIMonoBehaviours then remove this check
					return _fillParentHorizontal;
				return _fillParentHorizontal = layoutElement.flexibleWidth > 0;
			}
			set
			{
				_fillParentHorizontal = value;
				if (layoutElement == null)
				{
#if UNITY_EDITOR
					if (transform.parent == null)
					{
						_fillParentHorizontal = false;
						return;
					}
#endif

					//// this may be unneccessary as layout.flexibleWidth = 1 does the same thing (but only on Vertical Layout?)
					//var parentSize = transform.parent.GetComponent<RectTransform>().sizeDelta;
					//iUIBehavior.minDimensions = new Vector2(parentSize.x, iUIBehavior.minDimensions.y);
					//iUIBehavior.maxDimensions = new Vector2(parentSize.x, iUIBehavior.maxDimensions.y);
				}
				else
				{
					if (_fillParentHorizontal)
					{
						layoutElement.flexibleWidth = 1;
					}
					else
					{
						layoutElement.preferredWidth = -1;
						layoutElement.flexibleWidth = -1;
					}
				}

				this.SetDirty();
			}
		}

		[SerializeField]
		protected bool _fillParentVertical = false;
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


		[System.Diagnostics.DebuggerStepThrough]
		public UIControlType GetDataType() { return iUIBehavior.dataType; }


		public abstract void RecalculateDimensions();

		[Conditional("UNITY_EDITOR")]
		public void SetDirty_Editor()
		{
			SetDirty();
			foreach (Transform child in transform)
			{
				if (child.TryGetComponent<UIMonoBehaviour>(out UIMonoBehaviour childUI))
				{
					childUI.SetDirty_Editor();
				}
			}
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
#if UNITY_EDITOR
					if (Helpers.IsPrefabStage_EDITOR() && transform.parent.name == "Canvas (Environment)")
						RecalculateDimensions();
#endif
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
			if (!Helpers.IsPrefabStage_EDITOR())
				gameObject.name = referenceName;
#endif
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void RecordPrefabInstances()
		{
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
		}
	}
}