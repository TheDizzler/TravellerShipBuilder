using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;


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

		public string text
		{
			get { return label.text; }
			set { label.text = value; }
		}

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public override void RecalculateDimensions()
		{
			label.RecalculateDimensions();
			preferredSize = label.rect.sizeDelta;
			isDirty = false;
		}

		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}
	}
}