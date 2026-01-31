using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;
using static AtomosZ.UI.UIButtonPanel;
using static AtomosZ.UI.UIPrefabProvider;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UIPanel : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Panel; } }

		[SerializeField] private UIPanelScriptableObject panelData;

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
			}
		}

		[SerializeField] private Sprite _sprite;
		public Sprite sprite
		{
			get
			{
				return _sprite = GetComponent<Image>().sprite;
			}

			set
			{
				if (value == null && panelData != null)
				{
					_sprite = GetComponent<Image>().sprite = panelData.backgroundSprite;
				}
				else
				{
					_sprite = GetComponent<Image>().sprite = value;
				}

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
		[SerializeField] public List<UIMonoBehaviour> uiControls;


		[System.Diagnostics.Conditional("DEBUG")]
		public new void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(GetComponent<HorizontalOrVerticalLayoutGroup>());
		}

		void Awake()
		{
			if (transform.parent != null)
				this.SetDirty();

			GetControlsFromTransform_DEBUG();
		}

		public bool IsHorizontal()
		{
			return GetComponent<HorizontalLayoutGroup>() != null;
		}

		public ScriptableObject GetBackingData()
		{
			return panelData;
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

		public void UpdateBackingData(ScriptableObject backingData)
		{
			panelData = ((UIPanelScriptableObject)backingData);
			this.SetDirty();
		}

		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		[SerializeField] private Vector2 minDim;
		public void UpdateBackingData()
		{
			minDim = new Vector2(0, layoutPadding.top);
			var vertLayout = GetComponent<VerticalLayoutGroup>();
			if (vertLayout != null)
			{
				var activeChildren = 0;
				foreach (var child in uiControls)
				{
#if UNITY_EDITOR
					if (child == null || child.gameObject == null)
					{
						GetControlsFromTransform_DEBUG();
						return;
					}
#endif

					if (!child.gameObject.activeSelf)
						continue;

					++activeChildren;
					var childMinDim = child.iUIBehavior.GetMinDimensions();
					minDim.y += childMinDim.y;
					minDim.x = Mathf.Max(minDim.x, childMinDim.x);
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
					var childMinDim = child.iUIBehavior.GetMinDimensions();
					minDim.x += childMinDim.x;
					if (minDim.y < childMinDim.y)
						minDim.y = childMinDim.y;
				}

				if (activeChildren > 0)
					minDim.x += horzLayout.spacing * (activeChildren - 1);
			}

			minDim.x += layoutPadding.horizontal;
			minDim.y += layoutPadding.bottom;
			minDim.y = Mathf.Max(minDim.y, minDimensions.y);

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);
			//rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, minDim.x); // this gets set by tabcontrol


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


		public List<UIMonoBehaviour> GetControls()
		{
			return uiControls;
		}

#if DEBUG
		public List<UIMonoBehaviour> GetControlsFromTransform_DEBUG()
		{
			if (uiControls == null)
				uiControls = new(); // why does this happen?
			uiControls.Clear();
			foreach (Transform child in transform)
			{
				uiControls.Add(child.GetComponent<UIMonoBehaviour>());
			}

			return uiControls;
		}
#endif

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			foreach (var control in uiControls)
			{
				return control.iUIBehavior.GetControl(controlRefName);
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>-1 if panel does not have a ButtonPanel.</returns>
		public DialogButton GetPanelButtons()
		{
			foreach (var uiB in uiControls)
			{
				if (uiB.iUIBehavior.dataType == UIControlType.ButtonPanel)
				{
					return ((UIButtonPanel)uiB).buttons;
				}
			}

			return (DialogButton)(-1);
		}

		public UITabControl AddTabControl()
		{
			var prefabType = UIPrefabType.TabControl;
			var tabControl = (UITabControl)UIPrefabProvider.GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, tabControl);
			return tabControl;
		}

		public UITable AddTable()
		{
			var prefabType = UIPrefabType.Table;
			var ctrl = (UITable)UIPrefabProvider.GetMagicUIControl(prefabType, transform);
			SetReferenceNameAndAddControl(prefabType, ctrl);
			ctrl.Init(2, 1);
			return ctrl;
		}

		public UIPanel AddPanel(UIPanelScriptableObject panelData)
		{
			var prefabType = UIPrefabType.Panel;
			var panel = (UIPanel)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, panel);
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
			var panel = (UIPanel)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, panel);
#if UNITY_EDITOR
			if (panelData == null && transform.parent.name == "Canvas (Environment)")
				return panel;
#endif
			panel.UpdateBackingData(panelData);
			return panel;
		}


		public UISpinner AddSpinner(UISpinnerScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Spinner;
			var uiSpinner = (UISpinner)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, uiSpinner);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return uiSpinner;
#endif
			uiSpinner.UpdateBackingData(dataEx);

			return uiSpinner;
		}

		public UIButton AddButton(UIButtonScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Button;
			var uiButton = (UIButton)GetMagicUIControl(UIPrefabType.Button, transform);

			SetReferenceNameAndAddControl(prefabType, uiButton);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return uiButton;
#endif
			uiButton.UpdateBackingData(dataEx);

			return uiButton;
		}


		/// <summary>
		/// Only one ButtonPanel allowed per panel.<br/>
		/// @TODO(Tristan): make sure ButtonPanel is always the last in the controls list?
		/// </summary>
		/// <param name="dataEx"></param>
		/// <returns></returns>
		public UIButtonPanel AddButtonPanel(UIButtonPanelScriptableObject dataEx)
		{
			UIButtonPanel buttonPanel = GetComponentInChildren<UIButtonPanel>();
			if (buttonPanel == null)
			{
				buttonPanel = (UIButtonPanel)GetMagicUIControl(UIPrefabType.ButtonPanel, transform);
				SetReferenceNameAndAddControl(UIPrefabType.ButtonPanel, buttonPanel);
			}

			var magicWindow = GetComponentInParent<MagicWindow>();
			buttonPanel.SetResultListeners(magicWindow);

#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return buttonPanel;
#endif

			buttonPanel.UpdateBackingData(dataEx);
			return buttonPanel;
		}


		public UIDropdown AddDropdown(UIDropdownScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Dropdown;
			var dropdown = (UIDropdown)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, dropdown);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return dropdown;
#endif
			dropdown.UpdateBackingData(dataEx);

			return dropdown;
		}



		public UIImageView AddImage(UIImageViewScriptableObject dataEx)
		{
			var image = (UIImageView)GetMagicUIControl(UIPrefabType.ImageView, transform);

			SetReferenceNameAndAddControl(UIPrefabType.ImageView, image);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return image;
#endif
			image.UpdateBackingData(dataEx);

			return image;
		}

		public UISlider AddSlider(UISliderScriptableObject dataEx)
		{
			var slider = (UISlider)GetMagicUIControl(UIPrefabType.Slider, transform);

			SetReferenceNameAndAddControl(UIPrefabType.Slider, slider);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return slider;
#endif
			slider.UpdateBackingData(dataEx);

			return slider;
		}

		public UICheckBox AddCheckBox(UICheckBoxScriptableObject dataEx)
		{
			var checkBox = (UICheckBox)GetMagicUIControl(UIPrefabType.CheckBox, transform);

			SetReferenceNameAndAddControl(UIPrefabType.CheckBox, checkBox);
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return checkBox;
#endif
			checkBox.UpdateBackingData(dataEx);

			return checkBox;
		}

		public UIExpandingInputField AddInputField(UIExpandingInputFieldScriptableObject dataEx)
		{
			var inputField = (UIExpandingInputField)GetMagicUIControl(UIPrefabType.InputField, transform);
			var inputRect = inputField.GetComponent<RectTransform>();

			SetReferenceNameAndAddControl(UIPrefabType.InputField, inputField);
			var inputTMP = inputField.GetComponent<TMP_InputField>();
			inputTMP.onSubmit.AddListener(SetDialogResult);

#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return inputField;
#endif
			inputField.UpdateBackingData(dataEx);

			return inputField;
		}

		public UIExpandingLabel AddText_(string text)
		{
			var label = AddText(null);
			label.text = text;
			return label;
		}

		public UIExpandingLabel AddText(UIExpandingLabelScriptableObject dataEx)
		{
			var label = (UIExpandingLabel)GetMagicUIControl(UIPrefabType.ExpandingLabel, transform);
			label.referenceName = null;
			SetReferenceNameAndAddControl(UIPrefabType.ExpandingLabel, label);
			label.alignmentOptions = label.alignmentOptions;
#if UNITY_EDITOR
			if (dataEx == null && transform.parent.name == "Canvas (Environment)")
				return label;
#endif
			label.text = "New Label";
			label.UpdateBackingData(dataEx);
			return label;
		}

		public void AddCustomControl(UIMonoBehaviour uiBehavior)
		{
			uiControls.Add(uiBehavior);
			this.SetDirty();
		}

		/// <summary>
		/// @TODO(Tristan): Have this check control names in parent(s) well.
		/// </summary>
		/// <param name="prefabType"></param>
		/// <param name="uiBeh"></param>
		private void SetReferenceNameAndAddControl(UIPrefabType prefabType, UIMonoBehaviour uiBeh)
		{
			this.SetDirty();

			int count = 0;
			var controlName = $"{prefabType}_{count.ToString("00")}_{referenceName}";
			while (GetControl(controlName) != null)
			{
				++count;
				controlName = $"{prefabType}_{count.ToString("00")}_{referenceName}";
			}

			uiBeh.referenceName = controlName;
			uiControls.Add(uiBeh);
		}




		/// <summary>
		/// Editor script to keep anyone from tampering with the size!
		/// </summary>
		public void SetToParentSize()
		{
			var magicWindow = GetComponentInParent<MagicWindow>();
#if DEBUG
			if (magicWindow == null)
			{   // Are we in prefab edit mode?
				return;
			}
#endif

			magicWindow.Refresh();

			var rect = GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
		}


		public void RemoveControl(UIMonoBehaviour control)
		{
			this.SetDirty();
			uiControls.Remove(control);
			if (control.pool == null)
			{
				control.pool = UIPrefabProvider.GetPoolOfType(control.GetDataType());
				if (control.pool == null)
					return; // pool was purposefully not created?
			}

			control.ReturnToPool();
		}


		public void ClearControls()
		{
			this.SetDirty();
			foreach (var ctrl in uiControls)
				RemoveControl(ctrl);

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
		public void SetContextMenuActions(List<UIMenuAction> clickActions)
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


		public UIMonoBehaviour AddDivider()
		{
			if (uiControls.Count == 0)
			{
				Debug.LogError("A divider may not be the first control in a context menu");
				return null;
			}

			var divider = UIPrefabProvider.GetMagicUIControl(UIPrefabType.MenuDivider, transform);
			SetReferenceNameAndAddControl(UIPrefabType.MenuDivider, divider);
			return divider;
		}

		private void AddMenuControl(UIMenuAction clickAction)
		{
			//	clickAction += parentPanel.Close;
			//	var menuControl = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuControlButton), transform);

			//	var button = menuControl.GetComponent<Button>();
			//	button.onClick.AddListener(clickAction.action);
			//	button.interactable = clickAction.enabled;
			//	menuControl.GetComponentInChildren<UIExpandingLabel>().SetText(clickAction.buttonText, false);

			//	AddControl(UIPrefabType.MenuControlButton, menuControl);
		}



		private void SetDialogResult(string currentText)
		{
			var parentPanel = GetComponentInParent<MagicWindow>();
			if (parentPanel != null)
				parentPanel.SetDialogResultOK();
		}
	}
}