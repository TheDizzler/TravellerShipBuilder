using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Events;
using static AtomosZ.Keyboard;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	/// <summary>
	/// A TabbedWindow cannot return a result
	/// </summary>
	[ExecuteInEditMode]
	public class MagicTabbedWindow : MagicWindowBase, IUIBehavior
	{
		public override UIControlType dataType { get { return UIControlType.TabbedWindow; } }

		public UITabControl rootTabControl;


		/// <summary>
		/// If this is a tabbed window, gets the currently selected panel
		/// </summary>
		public override UIPanel panel
		{
			get
			{
#if DEBUG
				if (rootTabControl == null)
				{
					Debug.Log("rootTabControl is null and this is fucked");
					rootTabControl = GetComponentInChildren<UITabControl>();
				}
#endif

				return rootTabControl.SelectedPanel();
			}
			protected set
			{

			}
		}

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
			}
		}



		[Conditional("UNITY_EDITOR")]
		public new void RecordPrefabInstances()
		{
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(gameObject);
			rootTabControl.RecordPrefabInstances();
		}

		private UITabControlScriptableObject tabControlData;

		[Conditional("UNITY_EDITOR")]
		public void CreateRootTabControl()
		{
			rootTabControl = (UITabControl)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.TabControl, transform);
			rootTabControl.referenceName = "rootTabControl";
			rootTabControl.tabPanels[0].panel.tabItem.label.referenceName = "panel_00";
			rootTabControl.tabPanels[0].tabItem.label.referenceName = "tab_00";
			rootTabControl.UpdateBackingData(tabControlData);
		}

		public void ClearControls()
		{
			rootTabControl.ClearControls();
		}



		void Start()
		{
			cursors = GetComponentInParent<UICursors>();
			UIPrefabProvider uiProvider = GetComponentInParent<UIPrefabProvider>();

			if (panelScriptObj == null)
				panelScriptObj = uiProvider.panelScriptObj;
			if (horizontalPanelScriptObj == null)
				horizontalPanelScriptObj = uiProvider.horizontalPanelScriptObj;
			if (textScriptObj == null)
				textScriptObj = uiProvider.textScriptObj;
			if (dropdownScriptObj == null)
				dropdownScriptObj = uiProvider.dropdownScriptObj;
			if (checkBoxScriptObj == null)
				checkBoxScriptObj = uiProvider.checkBoxScriptObj;
			if (inputFieldScriptObj == null)
				inputFieldScriptObj = uiProvider.inputFieldScriptObj;
			if (sliderScriptObj == null)
				sliderScriptObj = uiProvider.sliderScriptObj;
			if (buttonScriptObj == null)
				buttonScriptObj = uiProvider.buttonScriptObj;
			if (buttonPanelScriptObj == null)
				buttonPanelScriptObj = uiProvider.buttonPanelScriptObj;
			if (imageViewScriptObj == null)
				imageViewScriptObj = uiProvider.imageViewScriptObj;
			//if (imageViewPanelScriptObj == null)
			//	imageViewPanelScriptObj = uiProvider.imageViewPanelScriptObj;
			if (tabControlData == null)
				tabControlData = uiProvider.tabControlScriptObj;

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
			return rootTabControl.GetControl(searchControlReferenceName);
		}

		/// <summary>
		/// Gets controls of currently visible tab.
		/// </summary>
		/// <returns></returns>
		public List<UIMonoBehaviour> GetControls()
		{
			return panel.GetControls();
		}


#if DEBUG
		public List<UIMonoBehaviour> GetControlsFromTransform_DEBUG()
		{
			if (panel != null)
				return panel.GetControlsFromTransform_DEBUG();
			Debug.LogException(new Exception("Why is this null?"));
			return null;
		}
#endif


		public bool EnableTab(string tabName, bool enable)
		{
			return rootTabControl.EnableTab(tabName, enable);
		}


		public bool EnableTab(int tabIndex, bool enable)
		{
			return rootTabControl.EnableTab(tabIndex, enable);
		}


		/// <summary>
		/// Currently does nothing. Input should get passed to currently open panel.
		/// </summary>
		/// <param name="modifierKeys"></param>
		/// <returns>True if input consumed.</returns>
		/// <exception cref="Exception"></exception>
		public override bool Input(ModifierKey modifierKeys)
		{
			//return panel.Input(modifierKeys);
			return false;
		}




		public TabPanel AddTab(string tabText, UIPanelScriptableObject overridePanelData = null)
		{
			var tabPanel = rootTabControl.AddTab(tabText, overridePanelData);
			return tabPanel;
		}



		public override TabPanel SelectTab(int tabIndex)
		{
			return rootTabControl.SelectTab(tabIndex);
		}




		public ScriptableObject GetBackingData()
		{
			return tabControlData;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			tabControlData = (UITabControlScriptableObject)backingData;
			if (tabControlData != null)
				RecalculateDimensions();
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}


		public override void RecalculateDimensions()
		{
			GetDrawnSize();
		}

		public Vector2 GetDrawnSize()
		{
			isDirty = false;
			return rootTabControl.GetDrawnSize();
		}

		public Vector2 GetPreferredSize()
		{
			isDirty = false;
			return rootTabControl.GetPreferredSize();
		}
	}
}