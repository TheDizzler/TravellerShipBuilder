using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;
using static AtomosZ.UI.UIButtonPanel;
using static AtomosZ.UI.UIPrefabProvider;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	/// <summary>
	/// From now on, (vertical) UIPanels have their width set by the parent.
	/// Then the resulting height of all the contaned controls is returned to the parent.
	/// </summary>
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
			get { return _sprite = GetComponent<Image>().sprite; }

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
		[Tooltip("A value of null will set the padding to the scriptable object values, if it exists, or all 0 if it doesn't.")]
		public RectOffset layoutPadding
		{
			get { return _layoutPadding = GetComponent<HorizontalOrVerticalLayoutGroup>().padding; }
			set
			{
				var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
				if (value == null)
				{
					if (panelData != null)
					{
						_layoutPadding = layout.padding = new RectOffset(
							panelData.layoutPadding.left, panelData.layoutPadding.right,
							panelData.layoutPadding.top, panelData.layoutPadding.bottom);
					}
					else
						_layoutPadding = layout.padding = new RectOffset(0, 0, 0, 0);
				}
				else
				{
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


		[Tooltip("The tab associated with this panel (if context menu, this will be inactive).")]
		public UITabItem tabItem;
		[SerializeField] public List<UIMonoBehaviour> uiControls;
		[SerializeField] internal int tabIndex;


		[Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			isDirty = true;
			referenceName = _referenceName;
			minDimensions = _minDimensions;
			maxDimensions = _maxDimensions;
			layoutSpacing = _layoutSpacing;
			layoutPadding = _layoutPadding;
			sprite = _sprite;
			borderless = _borderless;
			isDirty = false;
			if (Helpers.IsPrefabStage_EDITOR() && transform.parent.name == "Canvas (Environment)")
				RecalculateDimensions();
			else
				this.SetDirty();
		}

		[Conditional("DEBUG")]
		public new void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(GetComponent<HorizontalOrVerticalLayoutGroup>());
		}

		void Awake()
		{
			//if (transform.parent != null)
			//this.SetDirty();

			//GetControlsFromTransform_DEBUG();
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
#if UNITY_EDITOR
				if (Helpers.IsPrefabStage_EDITOR())
					isDirty = true;
#endif
				minDimensions = backingData.minDimensions;
				layoutPadding = new RectOffset(
					backingData.layoutPadding.left, backingData.layoutPadding.right,
					backingData.layoutPadding.top, backingData.layoutPadding.bottom);
				layoutSpacing = backingData.layoutSpacing;
				if (backingData.backgroundSprite != null)
					sprite = backingData.backgroundSprite;
#if UNITY_EDITOR
				isDirty = false;
#endif
			}

			this.SetDirty();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			UpdateBackingData((UIPanelScriptableObject)backingData);
		}


		[Tooltip("Serialized for debugging. The minimum requested dimensions of the panel.")]
		[SerializeField] private Vector2 preferredSize;

		public void RecalculateAllChildren()
		{
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

				child.RecalculateDimensions();
			}
		}

		/// <summary>
		/// The panel attempts to create itself in the smallest possible dimensions,
		/// saves the value in minDimensionsRequest, then sets it's own height using this value. The panel owner is responsible for setting the width.
		/// 
		/// </summary>
		public override void RecalculateDimensions()
		{
			isDirty = false;
			preferredSize = new Vector2(0, layoutPadding.top);

			//var minChildRequest = new Vector2(minDimensions.x, minDimensions.y);
			//var maxChildRequest = new Vector2(maxDimensions.x, maxDimensions.y);
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

					//child.maxDimensions = new Vector2(Mathf.Min(child.maxDimensions.x, maxDimensions.x), Mathf.Min(child.maxDimensions.y, maxDimensions.y));
					var childMinDim = child.iUIBehavior.GetPreferredSize();
					preferredSize.y += childMinDim.y;
					preferredSize.x = Mathf.Max(preferredSize.x, childMinDim.x);
				}

				if (activeChildren > 0)
					preferredSize.y += vertLayout.spacing * (activeChildren - 1);
				preferredSize.x += layoutPadding.horizontal;
			}
			else
			{
				var horzLayout = GetComponent<HorizontalLayoutGroup>();
				if (horzLayout == null)
					Debug.LogException(new Exception("No layout group found on panel"));

				var activeChildren = 0;
				foreach (var child in uiControls)
				{
					if (!child.gameObject.activeSelf)
						continue;

					++activeChildren;
					var childMinDim = child.iUIBehavior.GetPreferredSize();
					preferredSize.x += childMinDim.x;
					if (preferredSize.y < childMinDim.y)
					{
						//if (preferredSize.y > layoutPadding.top)
						//	this.SetDirty(); // refresh child controls
						preferredSize.y = childMinDim.y;
					}
				}

				if (activeChildren > 0)
					preferredSize.x += horzLayout.spacing * (activeChildren - 1);

				preferredSize.x += layoutPadding.horizontal;
				preferredSize.x = Mathf.Max(preferredSize.x, minDimensions.x);
				preferredSize.x = Mathf.Min(preferredSize.x, maxDimensions.x);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, preferredSize.x);
			}


			preferredSize.y += layoutPadding.bottom;
			preferredSize.y = Mathf.Max(preferredSize.y, minDimensions.y);
			preferredSize.y = Mathf.Min(preferredSize.y, maxDimensions.y);


			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, preferredSize.y);

#if UNITY_EDITOR
			if (transform.parent.name == "Canvas (Environment)")
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(minDimensions.x, preferredSize.x)); // this usually gets set by parent 
#endif
		}

		/// <summary>
		/// Gets the minimum requested size of the panel.
		/// </summary>
		/// <returns></returns>
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}


		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
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

			if (Helpers.IsPrefabStage_EDITOR() && transform.parent.name == "Canvas (Environment)")
				RecalculateDimensions();
			else
				this.SetDirty();
			return uiControls;
		}
#endif

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			foreach (var control in uiControls)
			{
				var controlFound = control.iUIBehavior.GetControl(controlRefName);
				if (controlFound != null)
					return controlFound;
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

		public UITabControl AddTabControl(UITabControlScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.TabControl;
			var tabControl = (UITabControl)UIPrefabProvider.GetMagicUIControl(prefabType, transform);
			SetReferenceNameAndAddControl(prefabType, tabControl);

#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return tabControl;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.tabControlData;
				}
			}
#endif
			tabControl.UpdateBackingData(dataEx);
			return tabControl;
		}

		public UITable AddTable()
		{
			var prefabType = UIPrefabType.Table;
			UITable ctrl = (UITable)UIPrefabProvider.GetMagicUIControl(prefabType, transform);
			SetReferenceNameAndAddControl(prefabType, ctrl);
#if UNITY_EDITOR
			if (!Application.isPlaying)
				ctrl.Init(2, 1);
#endif

			ctrl.UpdateBackingData(null);
			return ctrl;
		}

		public UIPanel AddPanel(UIPanelScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Panel;
			var panel = (UIPanel)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, panel);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return panel;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.panelScriptObj;
				}
			}
#endif
			panel.UpdateBackingData(dataEx);
			return panel;
		}



		public UIPanel AddHorizontalPanel(UIPanelScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.HorizontalPanel;
			var panel = (UIPanel)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, panel);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return panel;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.horizontalPanelScriptObj;
				}
			}
#endif
			panel.UpdateBackingData(dataEx);
			return panel;
		}


		public UISpinner AddSpinner(UISpinnerScriptableObject dataEx)
		{
			var prefabType = UIPrefabType.Spinner;
			var uiSpinner = (UISpinner)GetMagicUIControl(prefabType, transform);

			SetReferenceNameAndAddControl(prefabType, uiSpinner);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return uiSpinner;
				//else
				//{
				//	var window = GetComponentInParent<MagicWindowBase>();
				//	dataEx = window.spinnerScriptObj;
				//}
			}
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
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return uiButton;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.buttonScriptObj;
				}
			}

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
			var magicWindow = GetComponentInParent<MagicWindow>();
			if (magicWindow == null)
			{
				Log.Error("Only a MagicWindow may have a DialogResult, therefore only a MagicWindow may have a ButtonPanel.");
				return null;
			}

			UIButtonPanel buttonPanel = GetComponentInChildren<UIButtonPanel>();
			if (buttonPanel == null)
			{
				buttonPanel = (UIButtonPanel)GetMagicUIControl(UIPrefabType.ButtonPanel, transform);
				SetReferenceNameAndAddControl(UIPrefabType.ButtonPanel, buttonPanel);
			}


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
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return dropdown;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.dropdownScriptObj;
				}
			}
#endif
			dropdown.UpdateBackingData(dataEx);

			return dropdown;
		}



		public UIImageView AddImage(UIImageViewScriptableObject dataEx)
		{
			var image = (UIImageView)GetMagicUIControl(UIPrefabType.ImageView, transform);

			SetReferenceNameAndAddControl(UIPrefabType.ImageView, image);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return image;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.imageViewScriptObj;
				}
			}
#endif
			image.UpdateBackingData(dataEx);

			return image;
		}

		public UISlider AddSlider(UISliderScriptableObject dataEx)
		{
			var slider = (UISlider)GetMagicUIControl(UIPrefabType.Slider, transform);

			SetReferenceNameAndAddControl(UIPrefabType.Slider, slider);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return slider;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.sliderScriptObj;
				}
			}
#endif
			slider.UpdateBackingData(dataEx);

			return slider;
		}

		private void SetLayoutOfControl(UIMonoBehaviour ctrl)
		{
			var vertLayout = GetComponent<VerticalLayoutGroup>();
			if (vertLayout == null)
			{
				ctrl.layoutElement.flexibleWidth = -1;
				ctrl.layoutElement.flexibleHeight = 1;
			}
			else
			{
				ctrl.layoutElement.flexibleWidth = 1;
				ctrl.layoutElement.flexibleHeight = -1;
			}
		}

		public UICheckBox AddCheckBox(UICheckBoxScriptableObject dataEx)
		{
			var checkBox = (UICheckBox)GetMagicUIControl(UIPrefabType.CheckBox, transform);

			SetLayoutOfControl(checkBox);

			SetReferenceNameAndAddControl(UIPrefabType.CheckBox, checkBox);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return checkBox;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.checkBoxScriptObj;
				}
			}
#endif
			checkBox.UpdateBackingData(dataEx);

			return checkBox;
		}

		public UIInputField AddInputField(UIExpandingInputFieldScriptableObject dataEx)
		{
			var inputField = (UIInputField)GetMagicUIControl(UIPrefabType.InputField, transform);
			var inputRect = inputField.GetComponent<RectTransform>();

			SetReferenceNameAndAddControl(UIPrefabType.InputField, inputField);
			var inputTMP = inputField.GetComponent<TMP_InputField>();
			inputTMP.onSubmit.AddListener(SetDialogResult);

			SetLayoutOfControl(inputField);

#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return inputField;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.inputFieldScriptObj;
				}
			}
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
			SetLayoutOfControl(label);
#if UNITY_EDITOR
			if (dataEx == null)
			{
				if (transform.parent.name == "Canvas (Environment)")
					return label;
				else
				{
					var window = GetComponentInParent<MagicWindowBase>();
					dataEx = window.textScriptObj;
				}
			}
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



		[Conditional("UNITY_EDITOR")]
		/// <summary>
		/// Editor script to keep anyone from tampering with the size!
		/// </summary>
		public void SetToParentSize()
		{
			var magicWindow = GetComponentInParent<MagicWindow>();
			if (magicWindow != null)
			{
				magicWindow.RecalculateDimensions();
			}
			else
			{
				var tabWindow = GetComponentInParent<MagicTabbedWindow>();
				if (tabWindow == null)
				{   // Are we in prefab edit mode?
					return;
				}

				tabWindow.RecalculateDimensions();
			}

			var rect = GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
		}


		public void RemoveControl(UIMonoBehaviour control)
		{
			uiControls.Remove(control);
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				DestroyImmediate(control.gameObject);
				this.SetDirty();
				return;
			}
#endif

			if (control.TryGetComponent(out PooledObject pooledObject))
				pooledObject.ReturnToPool();
			else
				((ObjectForge.IPooledObject)control).ReturnToPool();
			this.SetDirty();
		}


		public void ClearControls()
		{
			for (int i = uiControls.Count - 1; i >= 0; --i)
				RemoveControl(uiControls[i]);

			uiControls.Clear();
			this.SetDirty();
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
			//var layout = GetComponent<HorizontalOrVerticalLayoutGroup>();
			//layout.spacing = 12;
			//layout.padding = new RectOffset(layout.padding.left, layout.padding.right, layout.padding.top, 0);
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

			this.SetDirty();
			return divider;
		}

		private UIMenuButton AddMenuControl(UIMenuAction clickAction)
		{
			UIMenuButton menuControl = (UIMenuButton)UIPrefabProvider.GetMagicUIControl(UIPrefabType.MenuButton, transform);
			SetReferenceNameAndAddControl(UIPrefabType.MenuButton, menuControl);

			var button = menuControl.GetComponent<Button>();
			button.onClick.AddListener(clickAction.action);
			button.interactable = clickAction.enabled;
			menuControl.text = clickAction.buttonText;

			this.SetDirty();
			return menuControl;
		}



		private void SetDialogResult(string currentText)
		{
			var magicWindow = GetComponentInParent<MagicWindow>();
			if (magicWindow != null)
			{
				magicWindow.SetDialogResultOK();
				return;
			}
		}
	}
}