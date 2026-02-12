using UnityEngine;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteInEditMode]
	public class UITabItem : UIPooledMonoBehaviour<UITabItem>, IUIBehavior
	{
		public UIControlType dataType { get; }
		public bool interactable { get; set; }
		public Vector2 minDimensions { get; set; }
		public Vector2 maxDimensions { get; set; }

		[SerializeField] private UIPanel panel;
		[SerializeField] private RectTransform panelRect;

		/// <summary>
		/// Overriden so as not to return child TextLabel to pool.
		/// </summary>
		public override void ReturnToPool()
		{
			if (pool == null)
				pool = (ObjectForge.ObjectPool<UITabItem>)UIPrefabProvider.GetPoolOfType(iUIBehavior.dataType);
			this.Return();
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			throw new System.NotImplementedException();
		}

		public Vector2 GetDrawnDimensions()
		{
			throw new System.NotImplementedException();
		}

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		public void RecalculateDimensions()
		{
			throw new System.NotImplementedException();
		}
	}
}