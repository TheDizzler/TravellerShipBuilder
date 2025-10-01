using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	public class UITabControl : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private string _referenceName;
		/// <summary>
		/// Will TabControls get a UIDataEx?
		/// </summary>
		public string referenceName { get { return _referenceName; } }

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

		[UDictionary.Split(50, 50)]
		[SerializeField]
		public UDictionary<GameObject, UIPanel> tabPanels;

		public UIPanelScriptableObject panelExData;

		[Min(0)]
		public int selectedTabIndex;
		public float tabHorizontaloffset;
		public float panelWidthAdjust;

		public GameObject tabItemPrefab;
		public UIPanel panelPrefab;

		public Color selectedTabColor;
		public Color deselectedTabColor;
		[Tooltip("@TODO(Tristan): this")]
		public Color hiddenTabColor;
		public Transform tabItemsTransform;
		public RectTransform panelsTransform;

		public Sprite firstTabSprite;
		public Sprite tabSprite;




		[Conditional("DEBUG")]
		public void ClearControlsEditor()
		{
			for (int i = tabPanels.Count - 1; i > 1; ++i)
			{
				RemoveTab(i);
			}

			tabPanels.First().Value.ClearControlsEditor();

			GetMinDimensions();
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void RecordPrefabInstances()
		{
			foreach (var tabPanel in tabPanels)
				tabPanel.Value.RecordPrefabInstances();
		}

		public void AddTab(UIPanelScriptableObject panelExData)
		{
			var tabItem = Instantiate(tabItemPrefab, tabItemsTransform);
			var tabRect = tabItem.GetComponent<RectTransform>();
			var panel = Instantiate(panelPrefab, panelsTransform);
			panel.parentPanel = this;

			var defaultData = (PanelEx)panel.GetBackingData();

			if (defaultData == null
				|| (defaultData.scriptableObj == null && panelExData != null))
			{
				var panelEx = new PanelEx(panelExData);
				panelEx.referenceName = "Panel_" + (tabPanels.Count).ToString("00");
				panel.UpdateBackingData(panelEx);
			}


			tabPanels.Add(tabItem, panel);

			if (selectedTabIndex == -1)
				selectedTabIndex = 0;

			var label = tabItem.GetComponentInChildren<UIExpandingLabel>();
			var name = tabItem.name = "TabItem_" + (tabPanels.Count - 1).ToString("00");
			label.SetText(name, false);

#if !DEBUG
			Refresh();
#else
			GetMinDimensions();
#endif
		}

		/// <summary>
		/// Should this be an editor only function?
		/// In-game, just hiding a tab would probably suffice.
		/// </summary>
		/// <param name="tabIndex"></param>
		public void RemoveTab(int tabIndex)
		{
			int i = 0;
			GameObject removeTab = null;
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
				AddTab(panelExData);
			if (selectedTabIndex >= tabPanels.Count)
				selectedTabIndex = tabPanels.Count - 1;

#if DEBUG
			if (Application.isPlaying)
			{
				Destroy(removeTab);
				Destroy(removePanel.gameObject);
			}
			else
			{
				DestroyImmediate(removeTab);
				DestroyImmediate(removePanel.gameObject);
			}
#else
			Destroy(removeTab);
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


		public IUIDataEx GetBackingData()
		{
			throw new NotImplementedException();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			throw new NotImplementedException();
		}

		public void UpdateBackingData()
		{
			if (tabPanels.Count == 0)
			{
				return;
			}

			if (selectedTabIndex < 0)
			{
				selectedTabIndex = 0;
			}

			float nextXPos = 0;
			int i = 0;
			foreach (var tabPanel in tabPanels)
			{
				var tab = tabPanel.Key;
				tab.GetComponent<UIExpandingLabel>().UpdateBackingData();
				var panel = tabPanel.Value;
				var tabRect = tab.GetComponent<RectTransform>();
				tabRect.localPosition = new Vector3(nextXPos, 0, 0);
				nextXPos += tabRect.sizeDelta.x + tabHorizontaloffset;

				if (i == selectedTabIndex)
				{
					tab.transform.SetParent(transform);//.SetAsLastSibling();
					tab.GetComponent<Image>().color = selectedTabColor;

					panel.gameObject.SetActive(true);
					panel.RecalculateDimensions();

					// there is nothing special about the selected tab being used for the following. It's just convenient.
					var bottom = tab.GetComponentInChildren<TextMeshProUGUI>().margin.w;
					var height = tab.GetComponent<RectTransform>().sizeDelta.y;
					var panelOffset = bottom - height;
					var pos = panelsTransform.localPosition;
					pos.y = panelOffset;
					panelsTransform.localPosition = pos;
				}
				else
				{
					tab.transform.SetParent(tabItemsTransform);
					tab.GetComponent<Image>().color = deselectedTabColor;
					tab.transform.SetSiblingIndex(i);

					panel.gameObject.SetActive(false);
				}

				tab.GetComponent<Image>().sprite = i == 0 ? firstTabSprite : tabSprite;
				++i;
			}

			nextXPos += panelWidthAdjust;
			//panelsTransform.SetInsetAndSizeFromParentEdge(
			panelsTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, nextXPos - tabHorizontaloffset);
		}


		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			UIPanel panel = SelectedPanel();
			GameObject tab = SelectedTab();
			var tabDimen = tab.GetComponent<UIExpandingLabel>().GetMinDimensions();
			var rect = panel.GetComponent<RectTransform>();
			var size = rect.sizeDelta;
			size.y += tabDimen.y;
			size.x = Mathf.Max(size.x, tabDimen.x);
			return size;
		}

		private GameObject SelectedTab()
		{
			return tabPanels.Keys[selectedTabIndex];
		}

		public UIPanel SelectedPanel()
		{
			return tabPanels.Values[selectedTabIndex];
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