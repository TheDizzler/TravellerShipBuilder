using System;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	[Serializable]
	public class TabPanel
	{
		public UIExpandingLabel tabLabel;
		public UIPanel panel;

		public TabPanel(UIExpandingLabel label, UIPanel panel)
		{
			tabLabel = label;
			this.panel = panel;
		}
	}


	[ExecuteAlways]
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

			var tabLabel = tabTransform.GetComponent<UIExpandingLabel>();

			TabPanel tabPanel = GetTabPanel(tabLabel);
			return tabPanel;
		}

		private TabPanel GetTabPanel(UIExpandingLabel tabLabel)
		{
			foreach (var tabPanel in tabPanels)
			{
				if (tabPanel.tabLabel == tabLabel)
					return tabPanel;
			}

			return null;
		}


		public UITabControlScriptableObject tabControlData;

		[Min(0)]
		public int selectedTabIndex;

		public GameObject tabItemPrefab;
		public UIPanel panelPrefab;

		public RectTransform panelsTransform;

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;

			foreach (var tabPanel in tabPanels)
			{
				var tabLabel = tabPanel.tabLabel.GetControl(controlRefName);
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

				return tabControlData.titleBarSprites[0];
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

				return tabControlData.titleBarSprites[1];
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

				return tabControlData.titleBarSprites[0];
			}
		}

		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set { _minDimensions = value; }
		}

		public Vector2 maxDimensions { get; set; }

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
		public new void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			foreach (var tabPanel in tabPanels)
			{
				tabPanel.tabLabel.RecordPrefabInstances();
				tabPanel.panel.RecordPrefabInstances();
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void ReconstructTabsFromTransform()
		{
			tabPanels.Clear();
			if (panelsTransform == null)
			{
				var newPanels = new GameObject("Panels", typeof(RectTransform));
				newPanels.transform.SetParent(GetComponent<RectTransform>());
				var rect = newPanels.GetComponent<RectTransform>();
				panelsTransform = rect;
			}


			var panels = panelsTransform.GetComponentsInChildren<UIPanel>(true);
			foreach (UIPanel child in panels)
			{
				if (child.tabLabel != null)
					tabPanels.Add(new TabPanel(child.tabLabel, child));
			}

			if (tabPanels.Count == 0)
				AddTab();
		}

		public TabPanel AddTab(string tabText = null, UIPanelScriptableObject overridePanelData = null)
		{
			var tabItem = Instantiate(tabItemPrefab, this.transform);
			var tabLabel = tabItem.GetComponent<UIExpandingLabel>();
			var tabRect = tabItem.GetComponent<RectTransform>();
			var panel = Instantiate(panelPrefab, panelsTransform);
			panel.parentPanel = this;
			panel.tabLabel = tabLabel;


			var defaultData = (UIPanelScriptableObject)panel.GetBackingData();

			UIPanelScriptableObject panelEx = null;
			if (overridePanelData != null)
				panelEx = overridePanelData;
			else if (tabControlData.panelScriptableObj == null)
				panelEx = defaultData;
			else
				panelEx = tabControlData.panelScriptableObj;

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
					while (tabPanel.tabLabel.referenceName == tabRefName)
					{
						tabRefName = "tab_" + (++refNum).ToString("00");
						checkAgain = true;
					}

					if (checkAgain)
						break;
				}
			}

			tabLabel.referenceName = tabRefName;
			tabItem.name = tabRefName;

			var newTabPanel = new TabPanel(tabLabel, panel);
			tabPanels.Add(newTabPanel);

			if (selectedTabIndex == -1)
				selectedTabIndex = 0;

			if (string.IsNullOrEmpty(tabText))
			{
				tabText = "TabItem_" + (tabPanels.Count - 1).ToString("00");
			}

			tabLabel.text = tabText;

			GetDrawnDimensions();

			return newTabPanel;
		}

		/// <summary>
		/// Should this be an editor only function?
		/// In-game, just hiding a tab would probably suffice.
		/// </summary>
		/// <param name="tabIndex"></param>
		public void RemoveTab(int tabIndex)
		{
			int i = 0;
			//UIExpandingLabel removeTab = null;
			//UIPanel removePanel = null;

			TabPanel removeTabPanel = GetTabPanelAtIndex(tabIndex);

			foreach (var tabPanel in tabPanels)
			{
				if (i++ != tabIndex)
					continue;

				//removeTab = tabPanel.Key;
				//removePanel = tabPanel.Value;
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
				DestroyImmediate(removeTabPanel.tabLabel.gameObject);
				DestroyImmediate(removeTabPanel.panel.gameObject);
				return;
			}
#endif
			Destroy(removeTabPanel.tabLabel.gameObject);
			Destroy(removeTabPanel.panel.gameObject);
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
				if (tabPanel.tabLabel.referenceName == tabName)
				{
					tabPanel.tabLabel.interactable = enable;
					return true;
				}
			}

			return false;
		}

		public WindowStyle GetWindowStyle()
		{
			if (tabControlData == null)
				UnityEngine.Debug.LogException(new Exception("tabControlData may not be null"));
			return tabControlData.windowStyle;
		}


		public ScriptableObject GetBackingData()
		{
			return tabControlData;
		}

		/// <summary>
		/// "Please let's deprecate this."
		/// </summary>
		/// <param name="dataEx"></param>
		public void UpdateBackingData(ScriptableObject dataEx)
		{
			tabControlData = ((UITabControlScriptableObject)dataEx);
			this.SetDirty();
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		/// <summary>
		/// "Please get rid of this, or atleast change to a better name. And keep it private."
		/// </summary>
		public void RecalculateDimensions()
		{
			if (tabPanels.Count == 0)
			{
				Debug.LogException(new Exception("A TabControl must never have 0 tabs"));
			}

			if (selectedTabIndex < 0)
			{
				selectedTabIndex = 0;
			}

			if (tabControlData == null)
				return;

			//Debug.LogError("TabControl is updating again");
			switch (tabControlData.windowStyle)
			{
				case WindowStyle.Tabbed:
				{
					ToggleMultitab_DEBUG(true);

					SetUpTabs();
				}
				break;

				case WindowStyle.TitleBar:
				{
					ToggleMultitab_DEBUG(false);

					SetupTitleBar();
				}
				break;

				case WindowStyle.ContextMenu:
				{
					ToggleMultitab_DEBUG(false);
					Debug.LogError("ContextMenu not yet implemented");
				}
				break;
			}

			isDirty = false;
		}

		private void SetupTitleBar()
		{
			var rect = GetComponent<RectTransform>();
			var tabPanel = tabPanels[0];

			var tabLabel = tabPanel.tabLabel;
			if (titlebarSprite != null)
				tabLabel.GetComponent<Image>().sprite = titlebarSprite;
			tabLabel.GetComponent<Image>().color = tabControlData.selectedTabColor;

			if (tabLabel.text.StartsWith("TabItem_"))
			{
				tabLabel.text = "Title";
				tabLabel.referenceName = "titlebar";
			}

			tabLabel.color = tabControlData.titleBarFontColor;
			tabLabel.alignmentOptions = tabControlData.tabTextAlignment;
			tabLabel.fontSize = tabControlData.titleBarFontSize;

			tabLabel.RecalculateDimensions();
			//var tabRect = tab.GetComponent<RectTransform>();

#if UNITY_EDITOR
			if (tabPanel.panel == null)
			{
				tabPanel.panel = (UIPanel)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.Panel, panelsTransform);
			}
#endif

			var panel = tabPanel.panel;
			panel.gameObject.SetActive(true);
			var panelDimens = panel.GetDrawnDimensions();
			var tabDimens = tabLabel.GetDrawnDimensions();
			var newUIControlHeight = tabDimens.y + panelDimens.y;
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight);




			var tmp = tabLabel.GetComponentInChildren<TextMeshProUGUI>();
			tmp.margin = tabControlData.titleTextMargin;
			var bottom = tmp.margin.w;
			var tabHeight = tabLabel.GetComponent<RectTransform>().sizeDelta.y;
			var panelOffset = bottom - tabHeight;
			var newPanelPos = panelsTransform.localPosition;
			newPanelPos.y = panelOffset + tabControlData.titleBarVerticalOffset;
			panelsTransform.localPosition = newPanelPos;


			if (panelDimens.x < tabDimens.x)
				panelDimens.x = tabDimens.x;

			tabLabel.SetWidth(panelDimens.x);


			panelsTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x); // this forces all panels to the same width
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x);           // this sets the width of the TabUIControl for proper sizeing in parent layout
		}



		private void SetUpTabs()
		{
			ReconstructTabsFromTransform();

			if (tabPanels[0].tabLabel.text.StartsWith("Title"))
			{
				tabPanels[0].tabLabel.text = "TabItem_00";
				tabPanels[0].tabLabel.referenceName = "tabItem_00";
			}

			var rect = GetComponent<RectTransform>();
			float largestTabHeight = 0;

			int i = 0;
			// Set the tabs up and find the tallest one
			foreach (var tabPanel in tabPanels)
			{
				if (i == selectedTabIndex)
				{
					tabPanel.tabLabel.transform.SetSiblingIndex(tabPanels.Count);
					tabPanel.tabLabel.GetComponent<Image>().color = tabControlData.selectedTabColor;
				}
				else
				{
					tabPanel.tabLabel.transform.SetSiblingIndex(i);
					tabPanel.tabLabel.GetComponent<Image>().color = tabControlData.deselectedTabColor;
				}

				Sprite sprite = null;
				if (i == 0)
					sprite = firstTabSprite;
				else
					sprite = tabSprite;
				if (sprite != null)
					tabPanel.tabLabel.GetComponent<Image>().sprite = sprite;


				tabPanel.tabLabel.alignmentOptions = tabControlData.tabTextAlignment;
				tabPanel.tabLabel.color = tabControlData.titleBarFontColor;
				tabPanel.tabLabel.margin = tabControlData.titleTextMargin;
				tabPanel.tabLabel.fontSize = tabControlData.titleBarFontSize;

				var labelMinSize = tabControlData.titleBarMinSize;
				labelMinSize.x -= tabControlData.titleTextMargin.x + tabControlData.titleTextMargin.z;
				labelMinSize.y -= tabControlData.titleTextMargin.y + tabControlData.titleTextMargin.w;
				tabPanel.tabLabel.minDimensions = labelMinSize;

				largestTabHeight = Mathf.Max(largestTabHeight, tabPanel.tabLabel.GetDrawnDimensions().y);

				++i;
			}

			panelsTransform.SetSiblingIndex(tabPanels.Count - 1);

			float newPanelWidth = 0;
			float nextXPos = 0;
			i = 0;
			foreach (var tabPanel in tabPanels)
			{
				var tab = tabPanel.tabLabel;

				var panel = tabPanel.panel;
				var tabRect = tab.GetComponent<RectTransform>();
				tabRect.localPosition = new Vector3(nextXPos, 0, 0);
				nextXPos += tabRect.sizeDelta.x + tabControlData.tabHorizontaloffset;

				tab.SetHeight(largestTabHeight);
				if (i == selectedTabIndex)
				{
					panel.gameObject.SetActive(true);
					var panelMinDimens = panel.GetDrawnDimensions();

					newPanelWidth = panelMinDimens.x;

					// set the height of the window to the currently opened tab
					var newUIControlHeight = largestTabHeight + panelMinDimens.y;
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight);
				}
				else
				{
					panel.gameObject.SetActive(false);
				}

				++i;
			}

			// set the position of the panel to align with the tabs
			var margin = tabControlData.titleTextMargin;
			var bottom = margin.w;
			var height = largestTabHeight;
			var panelOffset = bottom - height;
			var newPanelPos = panelsTransform.localPosition;
			newPanelPos.y = panelOffset + tabControlData.titleBarVerticalOffset;
			panelsTransform.localPosition = newPanelPos;

			nextXPos += tabControlData.panelWidthAdjust;
			newPanelWidth = Mathf.Max(newPanelWidth, nextXPos);
			panelsTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newPanelWidth); // this forces all panels to the same width
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newPanelWidth);           // this sets the width of the TabUIControl for proper sizing in parent layout
		}


		[Conditional("DEBUG")]
		private void ToggleMultitab_DEBUG(bool enableMultitab)
		{
			// TODO(Tristan): these SHOULD get deleted on Start(), or ideally, at compile time.
			// but we need this for a flexible editor
			if (!enableMultitab)
				SelectTab(0);

			for (int i = 1; i < tabPanels.Count; ++i)
			{
				if (tabPanels[i].tabLabel == null || tabPanels[i].tabLabel.gameObject == null)
				{
					Debug.LogError("This should not be happening!!");
					ReconstructTabsFromTransform();
					return;
				}

				tabPanels[i].tabLabel.gameObject.SetActive(enableMultitab);
				tabPanels[i].panel.gameObject.SetActive(enableMultitab);
			}
		}

		public Vector2 GetDrawnDimensions()
		{
			if (isDirty)
				RecalculateDimensions();
			return GetComponent<RectTransform>().sizeDelta;
			//UIPanel panel = SelectedPanel();
			//GameObject tab = SelectedTab();
			//var tabDimen = tab.GetComponent<UIExpandingLabel>().GetMinDimensions();
			//var rect = panel.GetComponent<RectTransform>();
			////var panelPadding = panel.GetComponent<VerticalLayoutGroup>().padding;

			//Vector2 size = new Vector2(rect.rect.width, rect.rect.height);
			//size.y += tabDimen.y/* + panelPadding.vertical*/;
			//size.x = Mathf.Max(size.x /*+ panelPadding.horizontal*/, tabDimen.x);
			//return size;
		}

		public UIExpandingLabel SelectedTab()
		{
			TabPanel tabPanel = GetTabPanelAtIndex(selectedTabIndex);
			return tabPanel.tabLabel;
		}

		public UIPanel SelectedPanel()
		{
			TabPanel tabPanel = GetTabPanelAtIndex(selectedTabIndex);
			return tabPanel.panel;
		}
	}
}