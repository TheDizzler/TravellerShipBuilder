using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static AtomosZ.UI.UIButtonPanel;
using static AtomosZ.UI.UIPrefabProvider;
using static DesignManager;

namespace AtomosZ.UI
{
	public class BottomPanel : MonoBehaviour
	{
		public enum DialogResult
		{
			None,
			OK,
			Cancel,
			Yes,
			No,
		}


		[SerializeField] private DynamicPanel parentPanel;

		/// <summary>
		/// This is Serialized for debugging
		/// </summary>
		[Tooltip("This is Serialized for debugging")]
		[SerializeField] private List<UIDesignObject> controls;

		public Vector2 GetMinDimensions()
		{
			var minDim = Vector2.zero;
			var layout = GetComponent<VerticalLayoutGroup>();
			minDim.x = 0;
			minDim.y = layout.padding.top + layout.padding.bottom;
			var activeChildren = 0;
			//GetControlsFromTransform();
			foreach (var child in controls)
			{
				if (!child.gameObject.activeSelf)
					continue;

				++activeChildren;
				var childMinDim = child.GetMinDimensions();
				minDim.y += childMinDim.y;
				if (minDim.x < childMinDim.x)
					minDim.x = childMinDim.x; // this might require a recalculation of any text children
			}

			minDim.y += layout.spacing * (activeChildren - 1);
			minDim.x += layout.padding.left + layout.padding.right;
			//Debug.Log(minDim);
			return minDim;
		}

		public void ShowControls(bool showControls)
		{
			foreach (var child in controls)
			{
				child.gameObject.SetActive(showControls);
			}
		}


		public List<UIDesignObject> GetControls()
		{
			return controls;
		}

		public List<UIDesignObject> GetControlsFromTransform()
		{
			controls.Clear();
			foreach (Transform child in transform)
				controls.Add(child.GetComponent<UIDesignObject>());
			return controls;
		}

		public void ReorderControls(List<UIDesignObject> newOrder)
		{
			foreach (Transform child in transform)
				child.SetSiblingIndex(newOrder.FindIndex(ui => ui == child.GetComponent<UIDesignObject>()));
		}


		public DialogButton GetPanelButtons()
		{
			DialogButton buttons = (DialogButton)(-1);
			foreach (var control in controls)
			{
				var data = control.GetBackingData();
				if (data == null)
					continue;
				if (data.dataType == PanelControlType.ButtonPanel)
					buttons = ((ButtonPanelEx)data).buttons;
			}

			return buttons;
		}

		public void AddButtonPanel(ButtonPanelEx buttons)
		{
			UIButtonPanel buttonPanel = GetComponentInChildren<UIButtonPanel>();
			if (buttonPanel == null)
			{
				var buttonPanelDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ButtonPanel), transform);
				buttonPanel = buttonPanelDO.GetComponent<UIButtonPanel>();
				controls.Add(buttonPanelDO);
			}

			buttonPanel.UpdateBackingData(buttons);
			buttonPanel.SetResultListeners(parentPanel);
		}

		public Button AddButton(ButtonEx buttonEx)
		{
			var buttonDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.Button), transform);
			var uiButton = buttonDO.GetComponent<UIButton>();
			uiButton.UpdateBackingData(buttonEx);
			controls.Add(buttonDO);
			var button = buttonDO.GetComponent<Button>();
			return button;
		}

		public UIDropdown AddDropdown(DropdownEx dropdownEx)
		{
			var dropdownDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.Dropdown), transform);
			var uiControl = dropdownDO.GetComponent<UIDropdown>();
			uiControl.UpdateBackingData(dropdownEx);
			controls.Add(dropdownDO);
			return uiControl;
		}

		public UIImageViewPanel AddImagePanel(ImageViewDataEx viewData)
		{
			var imagePanelDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageViewPanel), transform);
			var imagePanel = imagePanelDO.GetComponent<UIImageViewPanel>();
			imagePanel.UpdateBackingData(viewData);
			controls.Add(imagePanelDO);
			return imagePanel;
		}

		public ImageView AddImage(ImageEx imageEx)
		{
			var imageDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageView), transform);
			var image = imageDO.GetComponent<ImageView>();
			image.UpdateBackingData(imageEx);
			controls.Add(imageDO);
			return image;
		}


		public UISlider AddSlider(SliderEx sliderEx)
		{
			var sliderDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.Slider), transform);
			var slider = sliderDO.GetComponent<UISlider>();
			slider.UpdateBackingData(sliderEx);
			controls.Add(sliderDO);
			return slider;
		}

		public UICheckBox AddCheckBox(CheckBoxEx checkBoxData)
		{
			var checkBoxDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.CheckBox), transform);
			var checkBox = checkBoxDO.GetComponent<UICheckBox>();
			checkBox.UpdateBackingData(checkBoxData);
			checkBox.parentPanel = parentPanel;
			controls.Add(checkBoxDO);
			return checkBox;
		}

		public void AddText(LabelEx labelEx)
		{
			if (string.IsNullOrEmpty(labelEx.text))
			{
				Debug.LogWarning("Text may not be empty");
				return;
			}

			var textBlock = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ExpandingText), transform);
			var label = textBlock.GetComponent<UIExpandingLabel>();
			label.UpdateBackingData(labelEx);
			controls.Add(textBlock);
		}



		/// <summary>
		/// TODO(Tristan): Now that using LabelEx use of this property should be strongly discouraged.
		/// </summary>
		/// <param name="text"></param>
		[Obsolete("Use AddText(LabelEx labelEx) instead.")]
		public void AddText(string text)
		{
			var textBlock = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ExpandingText), transform);
			var label = textBlock.GetComponent<UIExpandingLabel>();
			label.text = text;
			controls.Add(textBlock);
		}

		public TMP_InputField AddInputField(InputFieldEx inputFieldEx)
		{
			var input = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.InputField), transform);
			var inputRect = input.GetComponent<RectTransform>();
			var inputField = input.GetComponent<UIExpandingInputField>();
			inputField.UpdateBackingData(inputFieldEx);
			var inputTMP = input.GetComponent<TMP_InputField>();
			inputTMP.onSubmit.AddListener(SubmitText);
			controls.Add(input);
			return inputTMP;
		}



		private void SubmitText(string currentText)
		{
			parentPanel.SetDialogResultOK();
		}


		/// <summary>
		/// Editor script to keep anyone from tampering with the size!
		/// </summary>
		public void SetToParentSize()
		{
			parentPanel.Refresh();
			var rect = GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
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
			var layout = GetComponent<VerticalLayoutGroup>();
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

		public void RemoveControl(UIDesignObject uiDO)
		{
			controls.Remove(uiDO);
#if UNITY_EDITOR
			if (Application.isEditor && !Application.isPlaying)
				DestroyImmediate(uiDO.gameObject);
			else
				Destroy(uiDO.gameObject);
#else
		Destroy(uiDO.gameObject);
#endif

			parentPanel.RecalculateDimensions();
		}

		public void ClearControls()
		{
			foreach (var control in controls)
				Destroy(control.gameObject);
			controls.Clear();

			parentPanel.RecalculateDimensions();
		}

#if UNITY_EDITOR
		public void ClearControlsEditor()
		{
			foreach (var control in controls)
				DestroyImmediate(control.gameObject);

			if (transform.childCount > 0)
			{
				foreach (var childDO in transform.GetComponentsInChildren<UIDesignObject>())
					DestroyImmediate(childDO.gameObject);
			}

			controls.Clear();
		}
#endif

		private void AddDivider()
		{
			if (controls.Count == 0)
			{
				Debug.LogError("A divider may not be the first control in a context menu");
				return;
			}

			var divider = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuDivider), transform);

			controls.Add(divider);
		}

		private void AddMenuControl(DesignAction clickAction)
		{
			clickAction += parentPanel.Close;
			var menuControl = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuControlButton), transform);

			var button = menuControl.GetComponent<Button>();
			button.onClick.AddListener(clickAction.action);
			button.interactable = clickAction.enabled;
			menuControl.GetComponentInChildren<UIExpandingLabel>().text = clickAction.buttonText;

			controls.Add(menuControl);
		}
	}
}