using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEditor;

using UnityEngine;
using UnityEngine.UI;

using static AtomosZ.UI.UIButtonPanel;
using static AtomosZ.UI.UIPrefabProvider;

namespace AtomosZ.UI
{
	[Obsolete("Replaced with MagicWindow")]
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

		private Dictionary<string, UIDesignObject> uiControls = new();


		[System.Diagnostics.Conditional("DEBUG")]
		public void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(GetComponent<VerticalLayoutGroup>());
		}


		public Vector2 GetMinDimensions()
		{
			var minDim = Vector2.zero;
			var layout = GetComponent<VerticalLayoutGroup>();
			minDim.x = 0;
			minDim.y = layout.padding.top + layout.padding.bottom;
			var activeChildren = 0;
			//GetControlsFromTransform();
			foreach (var child in uiControls)
			{
				if (!child.Value.gameObject.activeSelf)
					continue;

				++activeChildren;
				var childMinDim = child.Value.GetMinDimensions();
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
			foreach (var child in uiControls)
			{
				child.Value.gameObject.SetActive(showControls);
			}
		}


		public Dictionary<string, UIDesignObject> GetControls()
		{
			return uiControls;
		}

		public Dictionary<string, UIDesignObject> GetControlsFromTransform()
		{
			uiControls.Clear();
			foreach (Transform child in transform)
			{
				var uiObject = child.GetComponent<UIDesignObject>();
				uiControls.Add(uiObject.name, uiObject);
			}

			return uiControls;
		}

		public void ReorderControls(List<Transform> newOrder)
		{
			transform.DetachChildren();
			foreach (Transform child in newOrder)
				child.SetParent(transform);
		}


		public DialogButton GetPanelButtons()
		{
			DialogButton buttons = (DialogButton)(-1);
			foreach (var control in uiControls)
			{
				var data = control.Value.GetBackingData();
				if (data == null)
					continue;
				if (data.dataType == UIControlType.ButtonPanel)
					buttons = ((ButtonPanelEx)data).buttons;
			}

			return buttons;
		}

		public UIDesignObject GetControl(string controlRefName)
		{
			if (!uiControls.TryGetValue(controlRefName, out var obj))
				return null;
			return obj;
		}


		public IUIBehavior AddUIControl(IUIDataEx uiDataEx)
		{
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

				default:
					Debug.LogException(new Exception($"Panel Control type {uiDataEx.dataType} not yet implemented."));
					return null;
			}
		}

		private UIButton AddButton(ButtonEx dataEx)
		{
			var prefabType = UIPrefabType.Button;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiButton = uiDO.GetComponent<UIButton>();
			uiButton.UpdateBackingData(dataEx);
			AddControl(prefabType, uiDO);
			dataEx.referenceName = uiDO.name;
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
				AddControl(UIPrefabType.ButtonPanel, uiDO);
				dataEx.referenceName = uiDO.name;
			}

			buttonPanel.UpdateBackingData(dataEx);
			buttonPanel.SetResultListeners(parentPanel);
			return buttonPanel;
		}

		private UIDropdown AddDropdown(DropdownEx dataEx)
		{
			var prefabType = UIPrefabType.Dropdown;
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(prefabType), transform);
			var uiControl = uiDO.GetComponent<UIDropdown>();
			uiControl.UpdateBackingData(dataEx);
			AddControl(prefabType, uiDO);
			dataEx.referenceName = uiDO.name;
			return uiControl;
		}

		private UIImageViewPanel AddImagePanel(ImageViewDataEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageViewPanel), transform);
			var imagePanel = uiDO.GetComponent<UIImageViewPanel>();
			imagePanel.UpdateBackingData(dataEx);
			AddControl(UIPrefabType.ImageViewPanel, uiDO);
			dataEx.referenceName = uiDO.name;
			return imagePanel;
		}

		private UIImageView AddImage(ImageEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ImageView), transform);
			var image = uiDO.GetComponent<UIImageView>();
			image.UpdateBackingData(dataEx);
			AddControl(UIPrefabType.ImageView, uiDO);
			dataEx.referenceName = uiDO.name;
			return image;
		}

		private UISlider AddSlider(SliderEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.Slider), transform);
			var slider = uiDO.GetComponent<UISlider>();
			slider.UpdateBackingData(dataEx);
			AddControl(UIPrefabType.Slider, uiDO);
			dataEx.referenceName = uiDO.name;
			return slider;
		}

		private UICheckBox AddCheckBox(CheckBoxEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.CheckBox), transform);
			var checkBox = uiDO.GetComponent<UICheckBox>();
			checkBox.UpdateBackingData(dataEx);
			AddControl(UIPrefabType.CheckBox, uiDO);
			dataEx.referenceName = uiDO.name;
			return checkBox;
		}

		private UIExpandingInputField AddInputField(InputFieldEx dataEx)
		{
			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.InputField), transform);
			var inputRect = uiDO.GetComponent<RectTransform>();
			var inputField = uiDO.GetComponent<UIExpandingInputField>();
			inputField.UpdateBackingData(dataEx);
			var inputTMP = uiDO.GetComponent<TMP_InputField>();
			inputTMP.onSubmit.AddListener(SubmitText);
			AddControl(UIPrefabType.InputField, uiDO);
			dataEx.referenceName = uiDO.name;
			return inputField;
		}

		private UIExpandingLabel AddText(LabelEx dataEx)
		{
			//if (string.IsNullOrEmpty(dataEx.text))
			//{
			//	Debug.LogException(new Exception("Text may not be empty"));
			//	return null;
			//}

			var uiDO = Instantiate(UIPrefabProvider.GetUIPrefab(UIPrefabType.ExpandingText), transform);
			var label = uiDO.GetComponent<UIExpandingLabel>();
			label.UpdateBackingData(dataEx);
			AddControl(UIPrefabType.ExpandingText, uiDO);
			dataEx.referenceName = uiDO.name;
			return label;
		}


		private void AddControl(UIPrefabType prefabType, UIDesignObject uiDO)
		{
			if (string.IsNullOrEmpty(uiDO.name))
			{
				int count = 0;
				var controlName = $"{prefabType}_{count.ToString("00")}";
				while (!uiControls.TryAdd(controlName, uiDO))
				{
					++count;
					controlName = $"{prefabType}_{count.ToString("00")}";
				}
				uiDO.name = controlName;
			}
			else
			{
				int count = 0;
				var controlName = uiDO.name;
				while (!uiControls.TryAdd(controlName, uiDO))
				{
					++count;
					controlName = $"{uiDO.name}_{count.ToString("00")}";
				}
				uiDO.name = controlName;
			}
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

		public void RemoveControl(IUIDataEx data)
		{
			foreach (var cntrl in uiControls.Values)
			{
				if (cntrl.GetBackingData() == data)
				{
					uiControls.Remove(cntrl.name);
#if DEBUG
					if (Application.isEditor && !Application.isPlaying)
						DestroyImmediate(cntrl.gameObject);
					else
						Destroy(cntrl.gameObject);
#else
					Destroy(cntrl.gameObject);
#endif
					return;
				}
			}
		}

		public void RemoveControl(UIDesignObject uiDO)
		{
			uiControls.Remove(uiDO.name);
#if DEBUG
			if (uiDO == null || uiDO.gameObject == null)
			{
				Debug.LogException(new Exception("UI controls not delete themselves properly!"));
				return;
			}
			if (Application.isEditor && !Application.isPlaying)
				DestroyImmediate(uiDO.gameObject);
			else
				Destroy(uiDO.gameObject);
#else
			Destroy(uiDO.gameObject);
#endif
		}

		public void ClearControls()
		{
			foreach (var control in uiControls)
				Destroy(control.Value.gameObject);
			uiControls.Clear();
		}

#if UNITY_EDITOR
		public void ClearControlsEditor()
		{
			foreach (var control in uiControls)
				DestroyImmediate(control.Value.gameObject);

			if (transform.childCount > 0)
			{
				foreach (var childDO in transform.GetComponentsInChildren<UIDesignObject>())
					DestroyImmediate(childDO.gameObject);
			}

			uiControls.Clear();
		}
#endif

		private void AddDivider()
		{
			if (uiControls.Count == 0)
			{
				Debug.LogError("A divider may not be the first control in a context menu");
				return;
			}

			var divider = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuDivider), transform);

			AddControl(UIPrefabType.MenuDivider, divider);
		}

		private void AddMenuControl(DesignAction clickAction)
		{
			clickAction += parentPanel.Close;
			var menuControl = Instantiate(UIPrefabProvider.GetPrefab(UIPrefabType.MenuControlButton), transform);

			var button = menuControl.GetComponent<Button>();
			button.onClick.AddListener(clickAction.action);
			button.interactable = clickAction.enabled;
			menuControl.GetComponentInChildren<UIExpandingLabel>().text = clickAction.buttonText;

			AddControl(UIPrefabType.MenuControlButton, menuControl);
		}
	}
}