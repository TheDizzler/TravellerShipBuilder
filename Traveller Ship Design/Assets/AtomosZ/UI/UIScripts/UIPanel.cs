using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.UIPrefabProvider;

namespace AtomosZ.UI
{
	[Serializable]
	public class PanelEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Panel; } }

		public UIPanelScriptableObject scriptableObj;

		public Sprite backgroundSprite;
		public Vector2 minDimensions = new Vector2(96, 64);
		public RectOffset layoutPadding = new RectOffset(32, 32, 16, 16);
		public float layoutSpacing = 8;


		public bool useCustomBackgroundSprite = false;
		public bool useCustomMinDimensions = false;
		public bool useCustomLayoutPadding = false;
		public bool useCustomLayoutSpacing = false;

		public PanelEx(UIPanelScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
			if (scriptableObj == null)
			{
				useCustomBackgroundSprite = true;
				useCustomMinDimensions = true;
				useCustomLayoutPadding = true;
				useCustomLayoutSpacing = true;
			}
		}
	}

	[ExecuteAlways]
	public class UIPanel : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private PanelEx panelEx;
		[SerializeField] private string _referenceName;
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				this.SetGameObjectNameToReferenceName(gameObject);
			}
		}

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

		public bool isDirty { get; set; }

		public Sprite sprite
		{
			get
			{
				if (panelEx.scriptableObj == null || panelEx.useCustomBackgroundSprite)
					return panelEx.backgroundSprite;
				else
					return panelEx.scriptableObj.backgroundSprite;
			}

			set
			{
				panelEx.backgroundSprite = value;
				panelEx.useCustomBackgroundSprite = true;
				UpdateBackingData();
			}
		}

		public RectOffset layoutPadding
		{
			get
			{
				if (panelEx.scriptableObj == null || panelEx.useCustomLayoutPadding)
					return panelEx.layoutPadding;
				else
					return new RectOffset(
						(int)panelEx.scriptableObj.layoutPadding.x,
						(int)panelEx.scriptableObj.layoutPadding.y,
						(int)panelEx.scriptableObj.layoutPadding.z,
						(int)panelEx.scriptableObj.layoutPadding.w);
			}

			set
			{
				panelEx.layoutPadding = value;
				panelEx.useCustomLayoutPadding = true;
				UpdateBackingData();
			}
		}

		public float layoutSpacing
		{
			get
			{
				if (panelEx.scriptableObj == null || panelEx.useCustomLayoutPadding)
					return panelEx.layoutSpacing;
				else
					return panelEx.scriptableObj.layoutSpacing;
			}

			set
			{
				panelEx.layoutSpacing = value;
				panelEx.useCustomLayoutSpacing = true;
				UpdateBackingData();
			}
		}

		[SerializeField] private bool _borderless;
		public bool borderless
		{
			get { return _borderless; }
			set
			{
				_borderless = value;
				GetComponent<Image>().enabled = !value;
				var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
				if (_borderless)
				{
					panelEx.useCustomLayoutPadding = true;
					panelEx.layoutPadding = new RectOffset(0, 0, 0, 0);
					layout.padding = layoutPadding;
				}
				else
				{
					panelEx.useCustomLayoutPadding = false;
					layout.padding = layoutPadding;
				}
			}
		}

		public Vector2 minDimensions
		{
			get
			{
				if (panelEx.scriptableObj == null || panelEx.useCustomMinDimensions)
					return panelEx.minDimensions;
				else
					return panelEx.scriptableObj.minDimensions;
			}

			set
			{
				panelEx.minDimensions = value;
				panelEx.useCustomMinDimensions = true;
				UpdateBackingData();
			}
		}


		[Tooltip("The tab associated with this panel (if context menu, this tab will be inactive).")]
		public UIExpandingLabel tabLabel;
		public IUIBehavior parentPanel;
		[SerializeField] public List<UIDesignObject> uiControls;

		public RectTransform rect;


		[System.Diagnostics.Conditional("DEBUG")]
		public void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(GetComponent<HorizontalOrVerticalLayoutGroup>());
		}

		void Awake()
		{
			if (transform.parent != null)
				this.SetDirty();
		}

		public bool IsHorizontal()
		{
			return GetComponent<HorizontalLayoutGroup>() != null;
		}

		public IUIDataEx GetBackingData()
		{
			return panelEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			panelEx = (PanelEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			if (sprite != null)
				GetComponent<Image>().sprite = sprite;

			var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
			layout.padding = layoutPadding;
			layout.spacing = layoutSpacing;
			isDirty = false;
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void RecalculateDimensions()
		{
			UpdateBackingData();
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			var minDim = Vector2.zero;
			var vertLayout = GetComponent<VerticalLayoutGroup>();
			if (vertLayout != null)
			{
				minDim.x = 0;
				minDim.y = vertLayout.padding.top + vertLayout.padding.bottom;

				var activeChildren = 0;
				foreach (var child in uiControls)
				{
#if UNITY_EDITOR
					if (child == null || child.gameObject == null)
					{
						GetControlsFromTransform();
						return GetMinDimensions();
					}
#endif

					if (!child.gameObject.activeSelf)
						continue;

					++activeChildren;
					var childMinDim = child.GetMinDimensions();
					minDim.y += childMinDim.y;
					if (minDim.x < childMinDim.x)
						minDim.x = childMinDim.x;
				}

				minDim.y += vertLayout.spacing * (activeChildren - 1);
				minDim.x += vertLayout.padding.left + vertLayout.padding.right;
			}
			else
			{
				var horzLayout = GetComponent<HorizontalLayoutGroup>();
				if (horzLayout == null)
					Debug.LogException(new Exception("No layout group found on panel"));

				minDim.x = horzLayout.padding.left + horzLayout.padding.right;
				minDim.y = 0;

				var activeChildren = 0;
				foreach (var child in uiControls)
				{
					if (!child.gameObject.activeSelf)
						continue;

					++activeChildren;
					var childMinDim = child.GetMinDimensions();
					minDim.x += childMinDim.x;
					if (minDim.y < childMinDim.y)
						minDim.y = childMinDim.y;
				}


				minDim.x += horzLayout.spacing * (activeChildren - 1);
				minDim.y += horzLayout.padding.top + horzLayout.padding.bottom;
			}

			if (minDim.x < minDimensions.x)
				minDim.x = minDimensions.x;
			if (minDim.y < minDimensions.y)
				minDim.y = minDimensions.y;

			if (rect == null)
				rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);

			return minDim;
		}

		/// <summary>
		/// This will not show controls on sub-panel.
		/// </summary>
		/// <param name="showControls"></param>
		public void ShowControls(bool showControls)
		{
			foreach (var child in uiControls)
			{
				child.gameObject.SetActive(showControls);
			}
		}


		public List<UIDesignObject> GetControls()
		{
			return uiControls;
		}

		public List<UIDesignObject> GetControlsFromTransform()
		{
			uiControls.Clear();
			foreach (Transform child in transform)
			{
				var uiObject = child.GetComponent<UIDesignObject>();
				uiControls.Add(uiObject);
			}

			return uiControls;
		}

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			foreach (var control in uiControls)
			{
				var ctrl = control.GetUIBehavior().GetControl(controlRefName);
				if (ctrl != null)
					return ctrl;
			}

			return null;
		}



		public IUIBehavior AddUIControl(IUIDataEx uiDataEx)
		{
			this.SetDirty();
			switch (uiDataEx.dataType)
			{
				case UIControlType.Button:
					return AddButton((ButtonEx)uiDataEx);
				case UIControlType.ButtonPanel:
					return AddButtonPanel((ButtonPanelEx)uiDataEx);
				case UIControlType.CheckBox:
					return AddCheckBox((CheckBoxEx)uiDataEx);
				case UIControlType.Dropdown:
					return AddDropdown((DropdownEx)uiDataEx);
				case UIControlType.Image:
					return AddImage((ImageEx)uiDataEx);
				case UIControlType.ImagePanel:
					return AddImagePanel((ImageViewDataEx)uiDataEx);
				case UIControlType.InputField:
					return AddInputField((InputFieldEx)uiDataEx);
				case UIControlType.Slider:
					return AddSlider((SliderEx)uiDataEx);
				case UIControlType.Text:
					return AddText((LabelEx)uiDataEx);
				case UIControlType.Spinner:
					return AddSpinner((SpinnerEx)uiDataEx);

				default:
					Debug.LogException(new Exception($"Panel Control type {uiDataEx.dataType} not yet implemented."));
					return null;
			}
		}

		public UIPanel AddPanel(UIPanelScriptableObject verticalPanelScriptObj)
		{
			var prefabType = UIPrefabType.Panel;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var panel = uiDO.GetComponent<UIPanel>();
			if (verticalPanelScriptObj != null)
			{
				panel.panelEx.scriptableObj = verticalPanelScriptObj;
				panel.RecalculateDimensions();
			}

			SetReferenceNameAndAddControl(prefabType, uiDO);

			return panel;
		}

		public UIPanel AddHorizontalPanel(UIPanelScriptableObject horizontalPanelScriptObj)
		{
			var prefabType = UIPrefabType.HorizontalPanel;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var panel = uiDO.GetComponent<UIPanel>();
			if (horizontalPanelScriptObj != null)
			{
				panel.panelEx.scriptableObj = horizontalPanelScriptObj;
				panel.RecalculateDimensions();
			}

			SetReferenceNameAndAddControl(prefabType, uiDO);

			return panel;
		}

		public UITabControl AddTabControl()
		{
			var prefabType = UIPrefabType.TabControl;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var tabControl = uiDO.GetComponent<UITabControl>();

			SetReferenceNameAndAddControl(prefabType, uiDO);

			return tabControl;
		}

		private UISpinner AddSpinner(SpinnerEx dataEx)
		{
			var prefabType = UIPrefabType.Spinner;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiSpinner = uiDO.GetComponent<UISpinner>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
			uiSpinner.UpdateBackingData(dataEx);

			return uiSpinner;
		}

		private UIButton AddButton(ButtonEx dataEx)
		{
			var prefabType = UIPrefabType.Button;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiButton = uiDO.GetComponent<UIButton>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
			uiButton.UpdateBackingData(dataEx);

			return uiButton;
		}

		/// <summary>
		/// @TODO(Tristan): make sure ButtonPanel is always the last in the controls list?
		/// </summary>
		/// <param name="dataEx"></param>
		/// <returns></returns>
		private UIButtonPanel AddButtonPanel(ButtonPanelEx dataEx)
		{
			UIButtonPanel buttonPanel = GetComponentInChildren<UIButtonPanel>();
			if (buttonPanel == null)
			{
				var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ButtonPanel), transform);
				buttonPanel = uiDO.GetComponent<UIButtonPanel>();
				SetReferenceNameAndAddControl(UIPrefabType.ButtonPanel, uiDO);
			}

			buttonPanel.UpdateBackingData(dataEx);

			var magicWindow = GetComponentInParent<MagicWindow>();

			if (magicWindow == null)
			{
				var dynamicPanel = GetComponentInParent<DynamicPanel>();
				if (dynamicPanel != null)
				{
					Debug.LogWarning("Time to upgrade away from DynamicPanel");
					buttonPanel.SetResultListeners(dynamicPanel);
				}
			}
			else
			{
				buttonPanel.SetResultListeners(magicWindow);
			}

			return buttonPanel;
		}


		private UIDropdown AddDropdown(DropdownEx dataEx)
		{
			var prefabType = UIPrefabType.Dropdown;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiControl = uiDO.GetComponent<UIDropdown>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
			uiControl.UpdateBackingData(dataEx);

			return uiControl;
		}

		private UIImageViewPanel AddImagePanel(ImageViewDataEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageViewPanel), transform);
			var imagePanel = uiDO.GetComponent<UIImageViewPanel>();

			SetReferenceNameAndAddControl(UIPrefabType.ImageViewPanel, uiDO);
			imagePanel.UpdateBackingData(dataEx);

			return imagePanel;
		}

		private UIImageView AddImage(ImageEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageView), transform);
			var image = uiDO.GetComponent<UIImageView>();

			SetReferenceNameAndAddControl(UIPrefabType.ImageView, uiDO);
			image.UpdateBackingData(dataEx);

			return image;
		}

		private UISlider AddSlider(SliderEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.Slider), transform);
			var slider = uiDO.GetComponent<UISlider>();

			SetReferenceNameAndAddControl(UIPrefabType.Slider, uiDO);
			slider.UpdateBackingData(dataEx);

			return slider;
		}

		private UICheckBox AddCheckBox(CheckBoxEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.CheckBox), transform);
			var checkBox = uiDO.GetComponent<UICheckBox>();

			SetReferenceNameAndAddControl(UIPrefabType.CheckBox, uiDO);
			checkBox.UpdateBackingData(dataEx);

			return checkBox;
		}

		private UIExpandingInputField AddInputField(InputFieldEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.InputField), transform);
			var inputRect = uiDO.GetComponent<RectTransform>();
			var inputField = uiDO.GetComponent<UIExpandingInputField>();

			SetReferenceNameAndAddControl(UIPrefabType.InputField, uiDO);
			inputField.UpdateBackingData(dataEx);

			var inputTMP = uiDO.GetComponent<TMP_InputField>();
			inputTMP.onSubmit.AddListener(SubmitText);

			return inputField;
		}

		private UIExpandingLabel AddText(LabelEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ExpandingText), transform);
			var label = uiDO.GetComponent<UIExpandingLabel>();
			label.referenceName = null;
			SetReferenceNameAndAddControl(UIPrefabType.ExpandingText, uiDO);
			label.alignmentOptions = label.alignmentOptions;
			label.UpdateBackingData(dataEx);

			return label;
		}

		public void AddCustomControl(IUIBehavior uiBehavior)
		{
			this.SetDirty();
			uiControls.Add(uiBehavior.designObject);
		}

		/// <summary>
		/// @TODO(Tristan): Have this check control names in parent(s) well.
		/// </summary>
		/// <param name="prefabType"></param>
		/// <param name="uiDO"></param>
		private void SetReferenceNameAndAddControl(UIPrefabType prefabType, UIDesignObject uiDO)
		{
			this.SetDirty();

			int count = 0;
			var controlName = $"{referenceName}_{prefabType}_{count.ToString("00")}";
			while (GetControl(controlName) != null)
			{
				++count;
				controlName = $"{referenceName}_{prefabType}_{count.ToString("00")}";
			}

			uiDO.GetUIBehavior().referenceName = controlName;
			uiControls.Add(uiDO);
		}




		/// <summary>
		/// Editor script to keep anyone from tampering with the size!
		/// </summary>
		public void SetToParentSize()
		{
			var magicWindow = GetComponentInParent<MagicWindow>();
			if (magicWindow != null)
			{
				magicWindow.Refresh();
			}
			else
			{
				var parentPanel = GetComponentInParent<DynamicPanel>();
				if (parentPanel != null)
				{
					Debug.LogWarning("Time to upgrade away from DynamicPanel");
					parentPanel.Refresh();
				}
			}

			var rect = GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
		}




		public void RemoveControl(UIDesignObject control)
		{
			uiControls.Remove(control);
#if DEBUG
			if (control == null || control.gameObject == null)
			{
				Debug.LogException(new Exception("UI control did not delete themselves properly!"));
				return;
			}

			if (Application.isEditor && !Application.isPlaying)
				DestroyImmediate(control.gameObject);
			else
				Destroy(control.gameObject);

			RecordPrefabInstances();
#else
			Destroy(control.gameObject);
#endif
		}

		public void RemoveControl(IUIDataEx data)
		{
			foreach (var cntrl in uiControls)
			{
				if (cntrl.GetBackingData() == data)
				{
					uiControls.Remove(cntrl);
#if DEBUG
					if (Application.isEditor && !Application.isPlaying)
						DestroyImmediate(cntrl.gameObject);
					else
						Destroy(cntrl.gameObject);

					RecordPrefabInstances();
#else
					Destroy(cntrl.gameObject);
#endif
					return;
				}
			}
		}


		public void ClearControls()
		{
#if UNITY_EDITOR
			ClearControls_EditorOnly();
#else
			foreach (var control in uiControls)
				Destroy(control.gameObject);
			uiControls.Clear();
#endif

			this.SetDirty();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		private void ClearControls_EditorOnly()
		{
			foreach (var control in uiControls)
			{
				if (Application.isPlaying)
					Destroy(control.gameObject);
				else
					DestroyImmediate(control.gameObject);
			}

			if (transform.childCount > 0)
			{
				foreach (var childDO in transform.GetComponentsInChildren<UIDesignObject>())
					DestroyImmediate(childDO.gameObject);
			}

			uiControls.Clear();
		}

		/// <summary>
		/// Can add multiple methods to a single UnityAction as below:<br/>
		/// <c>
		/// UnityAction action = null;<br/>
		/// action += () => FunctionWithParam("name");<br/>
		/// action += () => FunctionNoParam();<br/>
		/// action += delegate {// some code here};</c>
		/// 
		/// Add a null object to add a divider.
		/// </summary>
		/// <param name="clickActions"></param>
		public void SetContextMenuActions(List<DesignAction> clickActions)
		{
			var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
			layout.spacing = 12;
			layout.padding = new RectOffset(layout.padding.left, layout.padding.right, layout.padding.top, 0);
			ClearControls();
			foreach (var action in clickActions)
			{
				if (action == null)
					AddDivider();
				else
					AddMenuControl(action);
			}
		}


		private void AddDivider()
		{
			if (uiControls.Count == 0)
			{
				Debug.LogError("A divider may not be the first control in a context menu");
				return;
			}

			var divider = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuDivider), transform);

			SetReferenceNameAndAddControl(UIPrefabType.MenuDivider, divider);
		}

		private void AddMenuControl(DesignAction clickAction)
		{
			//	clickAction += parentPanel.Close;
			//	var menuControl = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuControlButton), transform);

			//	var button = menuControl.GetComponent<Button>();
			//	button.onClick.AddListener(clickAction.action);
			//	button.interactable = clickAction.enabled;
			//	menuControl.GetComponentInChildren<UIExpandingLabel>().SetText(clickAction.buttonText, false);

			//	AddControl(UIPrefabType.MenuControlButton, menuControl);
		}



		private void SubmitText(string currentText)
		{
			//throw new Exception("AddButtonPanel not yet implemented");
			var parentPanel = GetComponentInParent<DynamicPanel>();
			if (parentPanel != null)
				parentPanel.SetDialogResultOK();
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