using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[Serializable]
	public class TabLookupDictionary : CustomDictionary<UIExpandingLabel, UIPanel>
	{
		public bool Contains(string refName)
		{
			foreach (var tab in Keys)
			{
				if (tab.referenceName == refName)
					return true;
			}

			foreach (var panel in Values)
			{
				if (panel.referenceName == refName)
					return true;
			}

			return false;
		}

		public bool ContainsTab(string panelRefName)
		{
			foreach (var tab in Keys)
			{
				if (tab.referenceName == panelRefName)
					return true;
			}

			return false;
		}

		public bool ContainsPanel(string panelRefName)
		{
			foreach (var panel in Values)
			{
				if (panel.referenceName == panelRefName)
					return true;
			}

			return false;
		}
	}

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

	[Serializable]
	public class TabControlEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.TabControl; } }
		public UITabControlScriptableObject scriptableObj;


		public TabControlEx(UITabControlScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
			if (scriptObj == null)
			{
			}
		}
	}

	public class UITabControl : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private string _referenceName;
		public string referenceName { get { return _referenceName; } set { _referenceName = value; } }

		public UIDesignObject _designObject;

		public UIDesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<UIDesignObject>();
				return _designObject;
			}
		}

		[SerializeField]
		public TabLookupDictionary tabPanels;

		public TabControlEx tabControlEx;

		[Min(0)]
		public int selectedTabIndex;

		public GameObject tabItemPrefab;
		public UIPanel panelPrefab;


		public Transform tabItemsTransform;
		public RectTransform panelsTransform;

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			foreach (var tabPanel in tabPanels)
			{
				var ctrlGO = tabPanel.Key;
				if (ctrlGO.name == controlRefName)
					return ctrlGO.GetComponent<IUIBehavior>();
				var ctrl = tabPanel.Value.GetControl(controlRefName);
				if (ctrl != null)
					return ctrl;
			}

			return null;
		}


		public Sprite firstTabSprite
		{
			get
			{
				if (tabControlEx.scriptableObj == null)
				{
					return null;
				}

				return tabControlEx.scriptableObj.titleBarSprites[0];
			}
		}

		public Sprite tabSprite
		{
			get
			{
				if (tabControlEx.scriptableObj == null)
				{
					return null;
				}

				return tabControlEx.scriptableObj.titleBarSprites[1];
			}
		}

		public Sprite titlebarSprite
		{
			get
			{
				if (tabControlEx.scriptableObj == null)
				{
					return null;
				}

				return tabControlEx.scriptableObj.titleBarSprites[0];
			}
		}


		public void ClearControls()
		{
			for (int i = tabPanels.Count - 1; i > 1; ++i)
			{
				RemoveTab(i);
			}

#if DEBUG
			if (Application.isPlaying)
				tabPanels.First().Value.ClearControls();
			else
				tabPanels.First().Value.ClearControls_EditorOnly();
#else
			tabPanels.First().Value.ClearControls();
#endif

			GetMinDimensions();
		}

		[Conditional("UNITY_EDITOR")]
		public void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			foreach (var tabPanel in tabPanels)
			{
				PrefabUtility.RecordPrefabInstancePropertyModifications(tabPanel.Key);
				tabPanel.Value.RecordPrefabInstances();
			}
		}

		[Conditional("DEBUG")]
		public void ReconstructTabsFromTransform()
		{
			tabPanels.Clear();
			var panels = panelsTransform.GetComponentsInChildren<UIPanel>(true);
			foreach (UIPanel child in panels)
			{
				if (child.tabLabel != null)
					tabPanels.Add(child.tabLabel, child);
			}

			if (tabPanels.Count == 0)
				AddTab();
		}

		public TabPanel AddTab(string tabText = null, UIPanelScriptableObject overridePanelData = null)
		{
			var tabItem = Instantiate(tabItemPrefab, tabItemsTransform);
			var tabLabel = tabItem.GetComponent<UIExpandingLabel>();
			var tabRect = tabItem.GetComponent<RectTransform>();
			var panel = Instantiate(panelPrefab, panelsTransform);
			panel.parentPanel = this;
			panel.tabLabel = tabLabel;


			var defaultData = (PanelEx)panel.GetBackingData();

			PanelEx panelEx = null;
			if (overridePanelData != null)
				panelEx = new PanelEx(overridePanelData);
			else if (tabControlEx.scriptableObj.panelScriptableObj == null)
				panelEx = defaultData;
			else
				panelEx = new PanelEx(tabControlEx.scriptableObj.panelScriptableObj);

			var refNum = 0;
			var panelRefName = "panel_" + refNum.ToString("00");
			while (tabPanels.ContainsPanel(panelRefName))
				panelRefName = "panel_" + (++refNum).ToString("00");
			panelEx.referenceName = panelRefName;
			panel.name = panelRefName;
			panel.UpdateBackingData(panelEx);

			refNum = 0;
			var tabRefName = "tab_" + refNum.ToString("00");
			while (tabPanels.ContainsTab(tabRefName))
				tabRefName = "tab_" + (++refNum).ToString("00");
			tabLabel.referenceName = tabRefName;
			tabItem.name = tabRefName;

			tabPanels.Add(tabLabel, panel);

			if (selectedTabIndex == -1)
				selectedTabIndex = 0;

			if (string.IsNullOrEmpty(tabText))
			{
				tabText = "TabItem_" + (tabPanels.Count - 1).ToString("00");
			}

			tabLabel.text = tabText;

#if !DEBUG
			Refresh();
#else
			GetMinDimensions();
#endif

			return new TabPanel(tabLabel, panel);
		}

		/// <summary>
		/// Should this be an editor only function?
		/// In-game, just hiding a tab would probably suffice.
		/// </summary>
		/// <param name="tabIndex"></param>
		public void RemoveTab(int tabIndex)
		{
			int i = 0;
			UIExpandingLabel removeTab = null;
			UIPanel removePanel = null;
			foreach (var tabPanel in tabPanels)
			{
				if (i++ != tabIndex)
					continue;

				removeTab = tabPanel.Key;
				removePanel = tabPanel.Value;
				break;
			}

			tabPanels.Remove(removeTab);

			if (tabPanels.Count == 0)
				AddTab(null);
			if (selectedTabIndex >= tabPanels.Count)
				selectedTabIndex = tabPanels.Count - 1;

#if DEBUG
			if (Application.isPlaying)
			{
				Destroy(removeTab.gameObject);
				Destroy(removePanel.gameObject);
			}
			else
			{
				DestroyImmediate(removeTab.gameObject);
				DestroyImmediate(removePanel.gameObject);
			}
#else
			Destroy(removeTab.gameObject);
			Destroy(removePanel.gameObject);
#endif

#if !DEBUG
			Refresh();
#else
			UpdateBackingData();
#endif
		}

		public void SetSelectedTab(int tabIndex)
		{
			selectedTabIndex = tabIndex;
		}

		public WindowStyle GetWindowStyle()
		{
			if (tabControlEx.scriptableObj == null)
				UnityEngine.Debug.LogException(new Exception("tabControlEx.scriptableObj may not be null"));
			return tabControlEx.scriptableObj.windowStyle;
		}

		public IUIDataEx GetBackingData()
		{
			return tabControlEx;
		}

		public void UpdateBackingData(IUIDataEx dataEx)
		{
			tabControlEx = (TabControlEx)dataEx;
			UpdateBackingData();
		}


		public void UpdateBackingData()
		{
			if (tabPanels.Count == 0)
			{
				UnityEngine.Debug.LogException(new Exception("A TabControl must never have 0 tabs"));
			}

			if (selectedTabIndex < 0)
			{
				selectedTabIndex = 0;
			}

			if (tabControlEx.scriptableObj == null)
				return;

			switch (tabControlEx.scriptableObj.windowStyle)
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

				}
				break;
			}
		}

		private void SetupTitleBar()
		{
			var rect = GetComponent<RectTransform>();
			var tabPanel = tabPanels[0];

			var tabLabel = tabPanel.Key;
			if (titlebarSprite != null)
				tabLabel.GetComponent<Image>().sprite = titlebarSprite;
			tabLabel.GetComponent<Image>().color = tabControlEx.scriptableObj.selectedTabColor;
			tabLabel.transform.SetParent(transform);

			if (tabLabel.text.StartsWith("TabItem_"))
				tabLabel.text = "Title";

			tabLabel.SetColor(tabControlEx.scriptableObj.titleBarFontColor);
			tabLabel.alignmentOptions = tabControlEx.scriptableObj.tabTextAlignment;

			tabLabel.UpdateBackingData();
			//var tabRect = tab.GetComponent<RectTransform>();

			var panel = tabPanel.Value;
			panel.gameObject.SetActive(true);
			panel.RecalculateDimensions();
			var panelDimens = panel.GetMinDimensions();
			var tabDimens = tabLabel.GetMinDimensions();
			var newUIControlHeight = tabDimens.y + panelDimens.y;
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight);




			var tmp = tabLabel.GetComponentInChildren<TextMeshProUGUI>();
			tmp.margin = tabControlEx.scriptableObj.titleTextMargin;
			var bottom = tmp.margin.w;
			var tabHeight = tabLabel.GetComponent<RectTransform>().sizeDelta.y;
			var panelOffset = bottom - tabHeight;
			var pos = panelsTransform.localPosition;
			pos.y = panelOffset + tabControlEx.scriptableObj.titleBarVerticalOffset;
			panelsTransform.localPosition = pos;


			if (panelDimens.x < tabDimens.x)
				panelDimens.x = tabDimens.x;

			tabLabel.SetSize(panelDimens.x);


			panelsTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x); // this forces all panels to the same width
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelDimens.x);           // this sets the width of the TabUIControl for proper sizeing in parent layout
		}



		private void SetUpTabs()
		{
			if (tabPanels[0].Key.text.StartsWith("Title"))
			{
				tabPanels[0].Key.text = "TabItem_00";
			}

			var rect = GetComponent<RectTransform>();
			float newPanelWidth = 0;
			float nextXPos = 0;
			int i = 0;
			foreach (var tabPanel in tabPanels)
			{
				var tabLabel = tabPanel.Key;

				var panel = tabPanel.Value;
				var tabRect = tabLabel.GetComponent<RectTransform>();
				tabRect.localPosition = new Vector3(nextXPos, 0, 0);
				nextXPos += tabRect.sizeDelta.x + tabControlEx.scriptableObj.tabHorizontaloffset;

				if (i == selectedTabIndex)
				{
					tabLabel.transform.SetParent(transform);
					tabLabel.GetComponent<Image>().color = tabControlEx.scriptableObj.selectedTabColor;

					panel.gameObject.SetActive(true);
					panel.RecalculateDimensions();
					var panelMinDimens = panel.GetMinDimensions();

					var newUIControlHeight = tabLabel.GetMinDimensions().y + panelMinDimens.y;
					rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newUIControlHeight);

					// there is nothing special about the selected tab being used for the following. It's just convenient since it only needs to run once.

					var tmp = tabLabel.GetComponentInChildren<TextMeshProUGUI>();
					tmp.margin = tabControlEx.scriptableObj.titleTextMargin;
					var bottom = tmp.margin.w;
					var height = tabLabel.GetComponent<RectTransform>().sizeDelta.y;
					var panelOffset = bottom - height;
					var pos = panelsTransform.localPosition;
					pos.y = panelOffset;
					panelsTransform.localPosition = pos;

					newPanelWidth = panelMinDimens.x;
				}
				else
				{
					var tmp = tabLabel.GetComponentInChildren<TextMeshProUGUI>();
					tmp.margin = tabControlEx.scriptableObj.titleTextMargin;

					tabLabel.transform.SetParent(tabItemsTransform);
					tabLabel.GetComponent<Image>().color = tabControlEx.scriptableObj.deselectedTabColor;
					tabLabel.transform.SetSiblingIndex(i);

					panel.gameObject.SetActive(false);
				}

				Sprite sprite = null;
				if (i == 0)
					sprite = firstTabSprite;
				else
					sprite = tabSprite;
				if (sprite != null)
					tabLabel.GetComponent<Image>().sprite = sprite;

				tabLabel.alignmentOptions = tabControlEx.scriptableObj.tabTextAlignment;
				tabLabel.SetColor(tabControlEx.scriptableObj.titleBarFontColor);
				tabLabel.margin = tabControlEx.scriptableObj.titleTextMargin;


				var labelMinSize = tabControlEx.scriptableObj.titleBarMinSize;
				labelMinSize.x -= tabControlEx.scriptableObj.titleTextMargin.x + tabControlEx.scriptableObj.titleTextMargin.z;
				labelMinSize.y -= tabControlEx.scriptableObj.titleTextMargin.y + tabControlEx.scriptableObj.titleTextMargin.w;
				tabLabel.minLabelDimensions = labelMinSize;
				tabLabel.UpdateBackingData();
				++i;
			}



			nextXPos += tabControlEx.scriptableObj.panelWidthAdjust;
			newPanelWidth = Mathf.Max(newPanelWidth, nextXPos + tabControlEx.scriptableObj.tabHorizontaloffset);
			panelsTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newPanelWidth); // this forces all panels to the same width
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newPanelWidth);           // this sets the width of the TabUIControl for proper sizing in parent layout
		}

		[Conditional("DEBUG")]
		private void ToggleMultitab_DEBUG(bool enableMultitab)
		{
			// TODO(Tristan): these SHOULD get deleted on Start(), or ideally, at compile time.
			// but we need this for a flexible editor
			if (!enableMultitab)
				SetSelectedTab(0);

			tabItemsTransform.gameObject.SetActive(enableMultitab);
			for (int i = 1; i < tabPanels.Count; ++i)
			{
				tabPanels[i].Key.gameObject.SetActive(enableMultitab);
				tabPanels[i].Value.gameObject.SetActive(enableMultitab);
			}


		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
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

		private UIExpandingLabel SelectedTab()
		{
			return tabPanels[selectedTabIndex].Key;
		}

		public UIPanel SelectedPanel()
		{
			return tabPanels[selectedTabIndex].Value;
		}

		public void SetHover(bool isHover)
		{
			throw new NotImplementedException();
		}

		public void UpdateHover(Vector3 posOfHover)
		{
			throw new NotImplementedException();
		}

		public void ResetToLastPosition()
		{
			throw new NotImplementedException();
		}

		public UIDesignObject Select()
		{
			throw new NotImplementedException();
		}

		public void Deselect()
		{
			throw new NotImplementedException();
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new NotImplementedException();
		}
	}
}