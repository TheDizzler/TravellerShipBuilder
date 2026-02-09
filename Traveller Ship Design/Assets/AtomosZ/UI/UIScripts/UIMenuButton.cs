using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;


namespace AtomosZ.UI
{
	public class UIMenuAction
	{
		public UnityAction action = null;
		public string buttonText;
		/// <summary>
		/// If you want the item to be visible but not selectable set to false.
		/// </summary>
		public bool enabled = true;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="newEditMode">EditMode to enable after action completes.</param>
		//public UIMenuAction(string actionName, EditMode newEditMode)
		public UIMenuAction(string actionName, UnityAction act)
		{
			buttonText = actionName;
			action += act;
		}

		public static UIMenuAction operator +(UIMenuAction da, UnityAction act)
		{
			da.action += act;
			return da;
		}
	}

	public class UIMenuButton : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.MenuButton; } }
		[SerializeField] private UIExpandingLabel label;
		[SerializeField] private Button button;
		public bool interactable
		{
			get { return _interactable = label.interactable; }
			set { _interactable = label.interactable = button.interactable = value; }
		}

		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				this.SetDirty();
			}
		}

		public Vector2 maxDimensions { get; set; }

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public Vector2 GetDrawnDimensions()
		{
			return rect.sizeDelta;
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