using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static AtomosZ.UI.MagicWindowBase;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	/// <summary>
	/// Convenience class to hold a paired UITabItem and UIPanel.
	/// </summary>
	[Serializable]
	public class TabPanel
	{
		public UITabItem tabItem;
		public UIPanel panel;

		public TabPanel(UITabItem tabItem, UIPanel panel)
		{
			this.tabItem = tabItem;
			this.panel = panel;
		}
	}


	public class UITabControl : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.TabControl; } }

		public bool interactable { get; set; }

		[SerializeField] public List<TabPanel> tabPanels;
		private TabPanel GetTabPanelAtIndex(int tabIndex)
		{
			if (tabIndex < 0 || tabIndex > transform.childCount) // must include the Panels gameobject amongst the children
			{
				Debug.LogError("Invalid tab inded " + tabIndex);
				return null;
			}

			var tabTransform = transform.GetChild(tabIndex);
			if (tabTransform == panelsTransform)
				tabTransform = transform.GetChild(tabIndex + 1);

			var tabItem = tabTransform.GetComponent<UITabItem>();
			TabPanel tabPanel = GetTabPanel(tabItem.label);
			return tabPanel;
		}

		private TabPanel GetTabPanel(UIExpandingLabel tabLabel)
		{
			foreach (var tabPanel in tabPanels)
			{
				if (tabPanel.tabItem.label == tabLabel)
					return tabPanel;
			}

			return null;
		}


		public UITabControlScriptableObject tabControlData;

		[Min(0)]
		public int selectedTabIndex;

		public RectTransform panelsTransform;

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;

			foreach (var tabPanel in tabPanels)
			{
				var tabLabel = tabPanel.tabItem.GetControl(controlRefName);
				if (tabLabel != null)
					return tabLabel;
				var panel = tabPanel.panel.GetControl(controlRefName);
				if (panel != null)
					return panel;
			}

			return null;
		}


		public Sprite firstTabSprite
		{
			get
			{
				if (tabControlData == null)
				{
					return null;
				}

				if (tabControlData.tabSprites.Length == 0)
				{   // should this be an error?
					return null;
				}

				return tabControlData.tabSprites[0];
			}
		}

		public Sprite tabSprite
		{
			get
			{
				if (tabControlData == null)
				{
					return null;
				}

				if (tabControlData.tabSprites.Length == 0)
				{   // should this be an error?
					return null;
				}
				return tabControlData.tabSprites[1];
			}
		}

		public Sprite lastTabSprite
		{
			get
			{
				if (tabControlData == null)
				{
					return null;
				}

				if (tabControlData.tabSprites.Length == 1)
					return tabControlData.tabSprites[0];
				if (tabControlData.tabSprites.Length <= 2)
					return tabControlData.tabSprites[1];

				return tabControlData.tabSprites[2];
			}
		}

		public Sprite titlebarSprite
		{
			get
			{
				if (tabControlData == null)
				{
					return null;
				}

				return tabControlData.tabSprites[0];
			}
		}

		[SerializeField] Color _selectedTabColor;
		public Color selectedTabColor
		{
			get
			{
				return _selectedTabColor;
			}

			set
			{
				if (value == Color.clear)
				{
					if (tabControlData != null)
						value = tabControlData.selectedTabColor;
				}

				_selectedTabColor = value;
			}
		}

		[SerializeField] Color _deselectedTabColor;
		public Color deselectedTabColor
		{
			get { return _deselectedTabColor; }
			set
			{
				if (value == Color.clear)
				{
					if (tabControlData != null)
						value = tabControlData.deselectedTabColor;
				}

				_deselectedTabColor = value;
			}
		}

		[SerializeField] float _tabHorizontaloffset;
		public float tabHorizontaloffset
		{
			get { return _tabHorizontaloffset; }
			set
			{
				if (value == float.MinValue)
				{
					if (tabControlData != null)
						value = tabControlData.tabHorizontaloffset;
				}

				_tabHorizontaloffset = value;
			}
		}

		[SerializeField] private float _panelWidthAdjust;
		public float panelWidthAdjust
		{
			get { return _panelWidthAdjust; }
			set
			{
				if (value == float.MinValue)
					if (tabControlData != null)
						value = tabControlData.panelWidthAdjust;
				_panelWidthAdjust = value;
			}
		}
		[SerializeField] private float _panelVerticalOffset;
		public float panelVerticalOffset
		{
			get { return _panelVerticalOffset; }
			set
			{
				if (value == float.MinValue)
					if (tabControlData != null)
						value = tabControlData.panelVerticalOffset;
				_panelVerticalOffset = value;
			}
		}

		[SerializeField] private Vector4 _titleTextMargin;
		public Vector4 titleTextMargin
		{
			get { return _titleTextMargin; }
			set
			{
				if (value == Vector4.negativeInfinity)
					value = tabControlData.titleTextMargin;
				_titleTextMargin = value;
			}
		}

		[Tooltip("Because the owning UIPanel wants to control the width of it's children, this rect.sizeDelta always reports a 0 width, "
			+ "so we're saving the ACTUAL sizeDelta here so we can pass it to the parent.")]
		[SerializeField]
		private Vector2 preferredSize;
		public void ClearControls()
		{
			for (int i = tabPanels.Count - 1; i > 1; ++i)
			{
				RemoveTab(i);
			}

			tabPanels[0].panel.ClearControls();

			this.SetDirty();
		}


		[Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			referenceName = _referenceName;
			interactable = _interactable;

			//disabledColor = _disabledColor;
			//fontSize = _fontSize;
			//fontAsset = _fontAsset;
			//fontStyles = _fontStyles;
			//alignmentOptions = _alignmentOptions;
			//margin = _margin;
			//fillParentHorizontal = _fillParentHorizontal;
			//fillParentVertical = _fillParentVertical;
			//minDimensions = _minDimensions;
			//maxDimensions = _maxDimensions;
			if (Helpers.IsPrefabStage_EDITOR() && transform.parent.name == "Canvas (Environment)")
				RecalculateDimensions();
			else
				this.SetDirty();
		}

		[Conditional("UNITY_EDITOR")]
		public new void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			foreach (var tabPanel in tabPanels)
			{
				tabPanel.tabItem.RecordPrefabInstances();
				tabPanel.panel.RecordPrefabInstances();
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void ReconstructTabsFromTransform_EDITOR()
		{
			tabPanels.Clear();
			if (panelsTransform == null)
			{
				var newPanels = new GameObject("Panels", typeof(RectTransform));
				newPanels.transform.SetParent(rect, true);
				panelsTransform = newPanels.GetComponent<RectTransform>();
				panelsTransform.anchorMin = new Vector2(0, 1);
				panelsTransform.anchorMax = new Vector2(0, 1);
				panelsTransform.pivot = new Vector2(0, 1);
				panelsTransform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
				panelsTransform.localScale = Vector3.one;
			}



			var panels = panelsTransform.GetComponentsInChildren<UIPanel>(true).ToList();
			var tabs = transform.GetComponentsInChildren<UITabItem>(true).ToList();
			var tabPanelCount = Mathf.Max(panels.Count, tabs.Count);
			var newTabPanels = new TabPanel[tabPanelCount];
			for (int i = 0; i < tabs.Count; ++i)
			{
				if (tabs[i].panel == null)
				{
					Log.Error("I hate Unity seralization");
					continue;
				}
				if (tabs[i].tabIndex != tabs[i].panel.tabIndex)
				{
					Log.Error("I hate Unity seralization");
					continue;
				}

				if (newTabPanels[tabs[i].tabIndex] != null)
				{
					Log.Error("I hate Unity seralization");
					continue;
				}

				newTabPanels[tabs[i].tabIndex] = new TabPanel(tabs[i], tabs[i].panel);
				panels.Remove(tabs[i].panel);
			}

			tabPanels = newTabPanels.ToList();


			//foreach (UIPanel panel in panels)
			//{
			//	//if (panel.tabItem != null)
			//	//tabPanels.Add(new TabPanel(panel.tabItem, panel));
			//	//else
			//	//{
			//	if (tabs.Count > 0)
			//	{
			//		tabPanels.Add(new TabPanel(tabs[0], panel));
			//		tabs.RemoveAt(0);
			//	}
			//	else
			//		AddTab(null, null, panel);
			//	//}
			//}

			if (tabPanels.Count == 0)
			{
				if (!gameObject.scene.IsValid())
				{
					Log.Error("No tabs found and cannot instantiate objects while open in explorer.");
					return;
				}

				AddTab();
			}
		}

		public TabPanel AddTab(string tabText = null, UIPanelScriptableObject overridePanelData = null, UIPanel panel = null)
		{
			var tabItem = (UITabItem)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.TabItem, transform);
			if (panel == null)
				panel = (UIPanel)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.Panel, panelsTransform);

			panel.rect.anchorMin = new Vector2(0, 1);
			panel.rect.anchorMax = new Vector2(1, 1);
			panel.rect.offsetMin = new Vector2(0, 0);
			panel.rect.offsetMax = new Vector2(0, 0);

			panel.tabItem = tabItem;
			tabItem.panel = panel;

			var defaultData = (UIPanelScriptableObject)panel.GetBackingData();

			UIPanelScriptableObject panelEx = null;
			if (overridePanelData != null)
				panelEx = overridePanelData;
			else if (tabControlData != null && tabControlData.panelScriptableObj != null)
				panelEx = tabControlData.panelScriptableObj;
			else
				panelEx = defaultData;

			var refNum = 0;
			var panelRefName = "panel_" + refNum.ToString("00");

			bool checkAgain = true;
			while (checkAgain)
			{
				checkAgain = false;
				foreach (var tabPanel in tabPanels)
				{
					while (tabPanel.panel.referenceName == panelRefName)
					{
						panelRefName = "panel_" + (++refNum).ToString("00");
						checkAgain = true;
					}

					if (checkAgain)
						break;
				}
			}

			panel.referenceName = panelRefName;
			panel.UpdateBackingData(panelEx);

			refNum = 0;
			var tabRefName = "tab_" + refNum.ToString("00");
			checkAgain = true;
			while (checkAgain)
			{
				checkAgain = false;
				foreach (var tabPanel in tabPanels)
				{
					while (tabPanel.tabItem.referenceName == tabRefName)
					{
						tabRefName = "tab_" + (++refNum).ToString("00");
						checkAgain = true;
					}

					if (checkAgain)
						break;
				}
			}

			tabItem.referenceName = tabRefName;
			tabItem.name = tabRefName;

			if (tabControlData != null)
			{
				selectedTabColor = tabControlData.selectedTabColor;
				deselectedTabColor = tabControlData.deselectedTabColor;
				tabHorizontaloffset = tabControlData.tabHorizontaloffset;
				panelWidthAdjust = tabControlData.panelWidthAdjust;
				panelVerticalOffset = tabControlData.panelVerticalOffset;
				titleTextMargin = tabControlData.titleTextMargin;

				tabItem.label.alignmentOptions = tabControlData.titleTextAlignment;
				tabItem.label.color = tabControlData.titleBarFontColor;
				tabItem.label.margin = tabControlData.titleTextMargin;
				tabItem.label.fontSize = tabControlData.titleBarFontSize;

				var labelMinSize = tabControlData.titleBarMinSize;
				labelMinSize.x -= tabControlData.titleTextMargin.x + tabControlData.titleTextMargin.z;
				labelMinSize.y -= tabControlData.titleTextMargin.y + tabControlData.titleTextMargin.w;
				tabItem.label.minDimensions = labelMinSize;
			}

			tabItem.tabIndex = tabPanels.Count;
			panel.tabIndex = tabPanels.Count;
			var newTabPanel = new TabPanel(tabItem, panel);
			tabPanels.Add(newTabPanel);

			if (selectedTabIndex == -1)
				selectedTabIndex = 0;

			if (string.IsNullOrEmpty(tabText))
			{
				tabText = "TabItem_" + (tabPanels.Count - 1).ToString("00");
			}

			tabItem.label.text = tabText;


			this.SetDirty();
			RecalculateDimensions();

#if UNITY_EDITOR
			RecordPrefabInstances();

#endif
			return newTabPanel;
		}

		/// <summary>
		/// Should this be an editor only function?
		/// In-game, just hiding a tab would probably suffice.
		/// </summary>
		/// <param name="tabIndex"></param>
		public void RemoveTab(int tabIndex)
		{
			TabPanel removeTabPanel = GetTabPanelAtIndex(tabIndex);
			int i = 0;
			foreach (var tabPanel in tabPanels)
			{
				if (i++ != tabIndex)
					continue;

				removeTabPanel = tabPanel;
				break;
			}

			tabPanels.Remove(removeTabPanel);

			if (tabPanels.Count == 0)
				AddTab(null);
			if (selectedTabIndex >= tabPanels.Count)
				selectedTabIndex = tabPanels.Count - 1;

			this.SetDirty();
#if DEBUG
			if (!Application.isPlaying)
			{
				DestroyImmediate(removeTabPanel.tabItem.gameObject);
				DestroyImmediate(removeTabPanel.panel.gameObject);
				return;
			}
#endif
			removeTabPanel.tabItem.pooledObject.ReturnToPool();
			removeTabPanel.panel.pooledObject.ReturnToPool();
		}


		/// <summary>
		/// Sets the tab at tabIndex to the active tab and returns the TabPanel.
		/// </summary>
		/// <param name="tabIndex"></param>
		/// <returns></returns>
		public TabPanel SelectTab(int tabIndex)
		{
			TabPanel selectedTab = GetTabPanelAtIndex(tabIndex);
			if (selectedTab != null)
				selectedTabIndex = tabIndex;
			return selectedTab;
		}


		/// <summary>
		/// Searches for a tab by tabName and enables or disables it.
		/// </summary>
		/// <param name="tabName"></param>
		/// <param name="enable">False if tab not found.</param>
		public bool EnableTab(string tabName, bool enable)
		{
			foreach (var tabPanel in tabPanels)
			{
				if (tabPanel.tabItem.referenceName == tabName)
				{
					tabPanel.tabItem.interactable = enable;
					return true;
				}
			}

			return false;
		}

		public bool EnableTab(int tabIndex, bool enable)
		{
			if (tabIndex >= tabPanels.Count)
				return false;
			tabPanels[tabIndex].tabItem.interactable = false;
			return true;
		}



		public ScriptableObject GetBackingData()
		{
			return tabControlData;
		}

		public void UpdateBackingData(UITabControlScriptableObject dataEx)
		{
			tabControlData = dataEx;
			if (tabControlData != null)
			{
#if UNITY_EDITOR
				isDirty = true;
#endif
				selectedTabColor = tabControlData.selectedTabColor;
				deselectedTabColor = tabControlData.deselectedTabColor;
				tabHorizontaloffset = tabControlData.tabHorizontaloffset;
				panelWidthAdjust = tabControlData.panelWidthAdjust;
				panelVerticalOffset = tabControlData.panelVerticalOffset;
				titleTextMargin = tabControlData.titleTextMargin;
				foreach (var tabPanel in tabPanels)
				{
					tabPanel.tabItem.label.alignmentOptions = tabControlData.titleTextAlignment;
					tabPanel.tabItem.label.color = tabControlData.titleBarFontColor;
					tabPanel.tabItem.label.margin = tabControlData.titleTextMargin;
					tabPanel.tabItem.label.fontSize = tabControlData.titleBarFontSize;
				}

#if UNITY_EDITOR
				isDirty = false;
#endif
			}

			this.SetDirty();
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="dataEx"></param>
		public void UpdateBackingData(ScriptableObject dataEx)
		{
			UpdateBackingData((UITabControlScriptableObject)dataEx);
		}



		public override void RecalculateDimensions()
		{
			if (tabPanels.Count == 0)
			{
				Debug.LogError("A TabControl must never have 0 tabs");
				ReconstructTabsFromTransform_EDITOR();
			}

			if (selectedTabIndex < 0)
			{
				selectedTabIndex = 0;
			}


			float largestTabHeight = 0;
			int i = 0;
			// Set the tabs up and find the tallest one
			foreach (var tabPanel in tabPanels)
			{
				if (i == selectedTabIndex)
				{
					tabPanel.tabItem.transform.SetSiblingIndex(tabPanels.Count); // bring to the front
					tabPanel.tabItem.image.color = selectedTabColor;
				}
				else
				{
					tabPanel.tabItem.transform.SetSiblingIndex(i);
					tabPanel.tabItem.image.color = deselectedTabColor;
				}

				Sprite sprite = null;
				if (i == 0 || tabSprite == null)
					sprite = firstTabSprite;
				else if (i == tabPanels.Count - 1)
					sprite = lastTabSprite;
				else
					sprite = tabSprite;
				if (sprite != null)
					tabPanel.tabItem.sprite = sprite;

				tabPanel.tabItem.RecalculateDimensions();

				var panelDimensions = tabPanel.tabItem.label.GetDrawnSize();
				largestTabHeight = Mathf.Max(largestTabHeight, panelDimensions.y);

				++i;
			}

			panelsTransform.SetSiblingIndex(tabPanels.Count - 1);


			float newPanelWidth = 0;
			float nextXPos = 0;
			i = 0;
			foreach (var tabPanel in tabPanels)
			{
				var tab = tabPanel.tabItem;
				var panel = tabPanel.panel;
				var tabRect = tab.rect;
				tabRect.anchoredPosition = new Vector3(nextXPos, 0, 0);
				nextXPos += tabRect.sizeDelta.x + tabHorizontaloffset;

				tab.label.SetHeight(largestTabHeight);
				if (i == selectedTabIndex)
				{
					panel.gameObject.SetActive(true);
					var panelMinDimens = panel.GetPreferredSize();
					newPanelWidth = panelMinDimens.x;

					// set the height of the window to the currently opened tab
					var newUIControlHeight = largestTabHeight + panelMinDimens.y;
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight);
					preferredSize.y = newUIControlHeight;
				}
				else
				{
					panel.gameObject.SetActive(false);
				}

				++i;
			}

			// set the position of the panel to align with the tabs
			SetPanelPosition(largestTabHeight);

			nextXPos += panelWidthAdjust;
			newPanelWidth = Mathf.Max(newPanelWidth, nextXPos - tabHorizontaloffset);
			panelsTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newPanelWidth);    // this forces all panels to the same width
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newPanelWidth);               // this sets the width of the TabUIControl for proper sizing in parent layout

			preferredSize.x = newPanelWidth;
			isDirty = false;
		}




		/// <summary>
		/// set the position of the panel to align with the tabs
		/// </summary>
		/// <param name="tabHeight"></param>
		private void SetPanelPosition(float tabHeight)
		{
			var margin = titleTextMargin;
			var verticalOffset = margin.w - tabHeight;
			var panelnewPos = panelsTransform.localPosition;
			panelnewPos.x = 0;
			panelnewPos.y = verticalOffset + panelVerticalOffset;
			panelsTransform.anchoredPosition = panelnewPos;
		}


		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}

		public (UITabItem tab, UIPanel panel) SelectedTab()
		{
			TabPanel tabPanel = GetTabPanelAtIndex(selectedTabIndex);
			return (tabPanel.tabItem, tabPanel.panel);
		}

		public UIPanel SelectedPanel()
		{
			TabPanel tabPanel = GetTabPanelAtIndex(selectedTabIndex);
			return tabPanel.panel;
		}
	}
}