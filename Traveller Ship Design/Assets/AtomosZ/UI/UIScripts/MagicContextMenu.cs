using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using static AtomosZ.Keyboard;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	[ExecuteInEditMode]
	public class MagicContextMenu : MagicWindowBase, IUIBehavior
	{
		[SerializeField] private UIPanelScriptableObject magicContextPanelData;

		[SerializeField] private UIPanel _panel;
		public override UIPanel panel
		{
			[DebuggerStepThrough]
			get
			{
				if (_panel == null)
					CreateMainPanel();
				return _panel;
			}
			protected set { _panel = value; }
		}

		public override UIControlType dataType { get { return UIControlType.ContextMenu; } }
		public bool interactable { get; set; }


		void Start()
		{
			UpdateBackingData(magicContextPanelData);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				// this resets the window to how it should look. Putting [ExecuteInEditMode] back on to all controls might have the same effect?
				SetDirty_Editor();
#endif
		}

		public UIMonoBehaviour GetControl(string searchControlReferenceName)
		{
			if (referenceName == searchControlReferenceName)
				return this;
			return panel.GetControl(searchControlReferenceName);
		}

		public void ClearControls()
		{
			panel.ClearControls();
		}

		//[Conditional("UNITY_EDITOR")]
		public void CreateMainPanel()
		{
			foreach (UIPanel child in GetComponentsInChildren<UIPanel>())
			{
				if (child.referenceName == "mainPanel")
				{
					panel = child;
					break;
				}
			}

			if (panel == null)
			{
				panel = (UIPanel)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.Panel, transform);
				panel.referenceName = "mainPanel";
				panel.rect.anchorMin = new Vector2(0, 1);
				panel.rect.anchorMax = new Vector2(0, 1);
				panel.rect.pivot = new Vector2(0, 1);
				panel.rect.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				panel.rect.localScale = Vector3.one;
			}

			if (magicContextPanelData != null)
				panel.UpdateBackingData(magicContextPanelData);
			panel.tabItem = null;
			this.SetDirty();
		}


		/// <summary>
		/// NOTE: A MagicContextMenu has no tab/titlebar.
		/// </summary>
		/// <param name="tabIndex"></param>
		/// <returns></returns>
		public override TabPanel SelectTab(int tabIndex)
		{
			return new TabPanel(null, panel);
		}


		public override bool Input(ModifierKey modifierKeys)
		{
			if ((modifierKeys & ModifierKey.Esc) == ModifierKey.Esc
				&& isModal)
			{
				Close();
				return true;
			}

			return false;
		}

		/// <summary>
		/// Can add multiple methods to a single UnityAction as below:<br/>
		/// <c>
		/// UnityAction action = null;<br/>
		/// action += () => FunctionWithParam("name");<br/>
		/// action += () => FunctionNoParam();<br/>
		/// action += delegate {// some code here};</c>
		/// </summary>
		/// <param name="clickActions"></param>
		public void SetContextMenuActions(List<UIMenuAction> clickActions)
		{
			foreach (var action in clickActions)
			{
				if (action == null)
					continue; // divider
				action.action += Close;
			}

			panel.SetContextMenuActions(clickActions);
			RecalculateDimensions();
		}


		public ScriptableObject GetBackingData()
		{
			return magicContextPanelData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			magicContextPanelData = (UIPanelScriptableObject)backingData;
			if (magicContextPanelData != null)
				panel.UpdateBackingData(magicContextPanelData);
			RecalculateDimensions();
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}


		public override void RecalculateDimensions()
		{
			isDirty = false;

			panel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.sizeDelta.x);
			panel.RecalculateDimensions();
			var panelDimens = panel.GetPreferredSize();
			//if (panelDimens.x < panel.minDimensions.x || panelDimens.x > panel.maxDimensions.x)
			//{
			//	panelDimens.x = Mathf.Max(panel.minDimensions.x, panelDimens.x);
			//	panelDimens.x = Mathf.Min(panel.maxDimensions.x, panelDimens.x);
			//	panel.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x);
			//	panel.RecalculateDimensions();
			//	//panelDimens = panel.GetDrawnDimensions();
			//}

			//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelDimens.y);
		}

		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return panel.GetDrawnSize();
		}


		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return panel.GetDrawnSize();
		}
	}
}