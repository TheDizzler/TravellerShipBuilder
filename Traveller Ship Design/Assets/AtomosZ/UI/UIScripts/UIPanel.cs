using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

		public PanelEx(UIPanelScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
		}
	}

	[ExecuteAlways]
	public class UIPanel : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Panel; } }

		[SerializeField] private UIPanelScriptableObject panelData;
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

		[SerializeField] private Sprite _sprite;
		public Sprite sprite
		{
			get { return _sprite = GetComponent<Image>().sprite; }

			set
			{
				if (_sprite == value)
					return;
				if (value == null && panelData != null)
					_sprite = GetComponent<Image>().sprite = panelData.backgroundSprite;
				else
					_sprite = GetComponent<Image>().sprite = value;
				this.SetDirty();
			}
		}

		[SerializeField] private RectOffset _layoutPadding;
		[Tooltip("A value of null will set the padding to the scriptable object values, if it exists.")]
		public RectOffset layoutPadding
		{
			get { return _layoutPadding = GetComponent<HorizontalOrVerticalLayoutGroup>().padding; }
			set
			{
				if (value == null)
				{
					if (panelData != null)
					{
						var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
						_layoutPadding = layout.padding = panelData.layoutPadding;
					}
				}
				else
				{
					var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
					_layoutPadding = layout.padding = value;
				}

				this.SetDirty();
			}
		}

		[SerializeField] private float _layoutSpacing;
		public float layoutSpacing
		{
			get { return _layoutSpacing = GetComponent<HorizontalOrVerticalLayoutGroup>().spacing; }
			set
			{
				_layoutSpacing = GetComponent<HorizontalOrVerticalLayoutGroup>().spacing = value;
				this.SetDirty();
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
				this.SetDirty();
			}
		}

		[SerializeField] private Vector2 _minDimensions;
		[Tooltip("A value of Vector2.zero will reset min dimensions to scriptable object value, if it exists")]
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				if (value == Vector2.zero && panelData != null)
					_minDimensions = panelData.minDimensions;

				else
					_minDimensions = value;

				this.SetDirty();
			}
		}


		[Tooltip("The tab associated with this panel (if context menu, this tab will be inactive).")]
		public UIExpandingLabel tabLabel;
		public IUIBehavior parentPanel;
		[SerializeField] public List<UIDesignObject> uiControls;

		public RectTransform rect;


		//[System.Diagnostics.Conditional("DEBUG")]
		//public void RecordPrefabInstances()
		//{
		//	PrefabUtility.RecordPrefabInstancePropertyModifications(this);
		//	PrefabUtility.RecordPrefabInstancePropertyModifications(GetComponent<HorizontalOrVerticalLayoutGroup>());
		//}

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
			return new PanelEx(panelData);
		}

		public void UpdateBackingData(UIPanelScriptableObject backingData)
		{
			panelData = backingData;
			if (backingData != null)
			{
				minDimensions = backingData.minDimensions;
				layoutPadding = backingData.layoutPadding;
				layoutSpacing = backingData.layoutSpacing;
				if (backingData.backgroundSprite != null)
					sprite = backingData.backgroundSprite;
			}

			this.SetDirty();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			panelData = ((PanelEx)backingData).scriptableObj;
			this.SetDirty();
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		private Vector2 preferredChildSize;
		public void UpdateBackingData()
		{
			var minDim = new Vector2(layoutPadding.left, layoutPadding.top);
			var vertLayout = GetComponent<VerticalLayoutGroup>();
			if (vertLayout != null)
			{
				var activeChildren = 0;
				foreach (var child in uiControls)
				{
#if UNITY_EDITOR
					if (child == null || child.gameObject == null)
					{
						GetControlsFromTransform();
						return;
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

				if (preferredChildSize.x != minDim.x)
				{
					preferredChildSize = minDim;
					// resize controls to fit parent if needed
					foreach (var child in uiControls)
					{
						if (!child.gameObject.activeSelf)
							continue;
						var behave = child.GetUIBehavior();
						//behave.SetPreferredWidth(minDim.x);
					}
				}

				if (activeChildren > 0)
					minDim.y += vertLayout.spacing * (activeChildren - 1);
			}
			else
			{
				var horzLayout = GetComponent<HorizontalLayoutGroup>();
				if (horzLayout == null)
					Debug.LogException(new Exception("No layout group found on panel"));

				minDim.x = Mathf.Max(minDim.x, horzLayout.padding.left + horzLayout.padding.right);

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

				if (activeChildren > 0)
					minDim.x += horzLayout.spacing * (activeChildren - 1);
			}

			minDim.x += layoutPadding.right;
			minDim.y += layoutPadding.bottom;
			minDim.y = Mathf.Max(minDim.y, minDimensions.y);

			if (rect == null)
				rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);
			//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minDim.x);


			isDirty = false;
		}


		public void RecalculateDimensions()
		{
			UpdateBackingData();
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();

			return rect.sizeDelta;
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
					return AddButton(((ButtonEx)uiDataEx).scriptableObj);
				case UIControlType.ButtonPanel:
					return AddButtonPanel(((ButtonPanelEx)uiDataEx).scriptableObj);
				case UIControlType.CheckBox:
					return AddCheckBox(((CheckBoxEx)uiDataEx).scriptableObj);
				case UIControlType.Dropdown:
					return AddDropdown(((DropdownEx)uiDataEx).scriptableObj);
				case UIControlType.Image:
					return AddImage((ImageEx)uiDataEx);
				case UIControlType.ImagePanel:
					return AddImagePanel((ImageViewDataEx)uiDataEx);
				case UIControlType.InputField:
					return AddInputField(((InputFieldEx)uiDataEx).scriptableObj);
				case UIControlType.Slider:
					return AddSlider((SliderEx)uiDataEx);
				case UIControlType.Text:
					return AddText(((LabelEx)uiDataEx).scriptableObj);
				case UIControlType.Spinner:
					return AddSpinner((SpinnerEx)uiDataEx);

				default:
					Debug.LogException(new Exception($"Panel Control type {uiDataEx.dataType} not yet implemented."));
					return null;
			}
		}


		public UITabControl AddTabControl()
		{
			var prefabType = UIPrefabType.TabControl;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var tabControl = uiDO.GetComponent<UITabControl>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
			return tabControl;
		}

		public UIPanel AddPanel(UIPanelScriptableObject panelData)
		{
			var prefabType = UIPrefabType.Panel;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var panel = uiDO.GetComponent<UIPanel>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
#if UNITY_EDITOR
			if (panelData == null && transform.parent.name == "Canvas (Environment)")
				return panel;
#endif
			panel.UpdateBackingData(panelData);
			return panel;
		}



		public UIPanel AddHorizontalPanel(UIPanelScriptableObject panelData)
		{
			var prefabType = UIPrefabType.HorizontalPanel;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var panel = uiDO.GetComponent<UIPanel>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
#if UNITY_EDITOR
			if (panelData == null && transform.parent.name == "Canvas (Environment)")
				return panel;
#endif
			panel.UpdateBackingData(panelData);
			return panel;
		}


		private UISpinner AddSpinner(SpinnerEx dataEx)
		{
			var prefabType = UIPrefabType.Spinner;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiSpinner = uiDO.GetComponent<UISpinner>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return uiSpinner;
#endif
			uiSpinner.UpdateBackingData(dataEx);

			return uiSpinner;
		}

		private UIButton AddButton(UIButtonScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Button;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiButton = uiDO.GetComponent<UIButton>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return uiButton;
#endif
			uiButton.UpdateBackingData(dataEx);

			return uiButton;
		}

		/// <summary>
		/// @TODO(Tristan): make sure ButtonPanel is always the last in the controls list?
		/// </summary>
		/// <param name="dataEx"></param>
		/// <returns></returns>
		private UIButtonPanel AddButtonPanel(UIButtonPanelScriptableObject dataEx)
		{
			UIButtonPanel buttonPanel = GetComponentInChildren<UIButtonPanel>();
			if (buttonPanel == null)
			{
				var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ButtonPanel), transform);
				buttonPanel = uiDO.GetComponent<UIButtonPanel>();
				SetReferenceNameAndAddControl(UIPrefabType.ButtonPanel, uiDO);
			}



			var magicWindow = GetComponentInParent<MagicWindow>();

			if (magicWindow == null)
			{
				var dynamicPanel = GetComponentInParent<DynamicPanel>();
				if (dynamicPanel != null)
				{
					Debug.LogError("Time to upgrade away from DynamicPanel");
					buttonPanel.SetResultListeners(dynamicPanel);
				}
			}
			else
			{
				buttonPanel.SetResultListeners(magicWindow);
			}

#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return buttonPanel;
#endif

			buttonPanel.UpdateBackingData(dataEx);
			return buttonPanel;
		}


		private UIDropdown AddDropdown(UIDropdownScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Dropdown;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var dropdown = uiDO.GetComponent<UIDropdown>();

			SetReferenceNameAndAddControl(prefabType, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return dropdown;
#endif
			dropdown.UpdateBackingData(dataEx);

			return dropdown;
		}

		private UIImageViewPanel AddImagePanel(ImageViewDataEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageViewPanel), transform);
			var imagePanel = uiDO.GetComponent<UIImageViewPanel>();

			SetReferenceNameAndAddControl(UIPrefabType.ImageViewPanel, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return imagePanel;
#endif
			imagePanel.UpdateBackingData(dataEx);

			return imagePanel;
		}

		private UIImageView AddImage(ImageEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageView), transform);
			var image = uiDO.GetComponent<UIImageView>();

			SetReferenceNameAndAddControl(UIPrefabType.ImageView, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return image;
#endif
			image.UpdateBackingData(dataEx);

			return image;
		}

		private UISlider AddSlider(SliderEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.Slider), transform);
			var slider = uiDO.GetComponent<UISlider>();

			SetReferenceNameAndAddControl(UIPrefabType.Slider, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return slider;
#endif
			slider.UpdateBackingData(dataEx);

			return slider;
		}

		private UICheckBox AddCheckBox(UICheckBoxScriptableObject dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.CheckBox), transform);
			var checkBox = uiDO.GetComponent<UICheckBox>();

			SetReferenceNameAndAddControl(UIPrefabType.CheckBox, uiDO);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return checkBox;
#endif
			checkBox.UpdateBackingData(dataEx);

			return checkBox;
		}

		private UIExpandingInputField AddInputField(UIExpandingInputFieldScriptableObject dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.InputField), transform);
			var inputRect = uiDO.GetComponent<RectTransform>();
			var inputField = uiDO.GetComponent<UIExpandingInputField>();

			SetReferenceNameAndAddControl(UIPrefabType.InputField, uiDO);
			var inputTMP = uiDO.GetComponent<TMP_InputField>();
			inputTMP.onSubmit.AddListener(SubmitText);

#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return inputField;
#endif
			inputField.UpdateBackingData(dataEx);

			return inputField;
		}

		private UIExpandingLabel AddText(UIExpandingLabelScriptableObject dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ExpandingText), transform);
			var label = uiDO.GetComponent<UIExpandingLabel>();
			label.referenceName = null;
			SetReferenceNameAndAddControl(UIPrefabType.ExpandingText, uiDO);
			label.alignmentOptions = label.alignmentOptions;
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return label;
#endif
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

			this.RecordPrefabInstances();
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

					this.RecordPrefabInstances();
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