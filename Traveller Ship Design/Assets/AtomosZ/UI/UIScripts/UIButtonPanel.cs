using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;

using static AtomosZ.UI.UIButtonPanel;


namespace AtomosZ.UI
{
	[Serializable]
	public class ButtonPanelEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.ButtonPanel; } }

		public UIButtonPanelScriptableObject scriptableObj;

		public DialogButton buttons = DialogButton.OK;

		public bool useCustomOKButton = false;
		public bool useCustomCancelButton = false;
		public bool useCustomYesButton = false;
		public bool useCustomNoButton = false;
		public ButtonEx okButton;
		public ButtonEx cancelButton;
		public ButtonEx yesButton;
		public ButtonEx noButton;

		public ButtonPanelEx(UIButtonPanelScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
		}

		public ButtonPanelEx(DialogButton buttonType)
		{
			buttons = buttonType;

			useCustomOKButton = true;
			okButton.labelEx.fontSize = 24;
			okButton.labelEx.fontColor = Color.white;

			useCustomCancelButton = true;
			cancelButton.labelEx.fontSize = 24;
			cancelButton.labelEx.fontColor = Color.white;

			useCustomYesButton = true;
			yesButton.labelEx.fontSize = 24;
			yesButton.labelEx.fontColor = Color.white;

			useCustomNoButton = true;
			noButton.labelEx.fontSize = 24;
			noButton.labelEx.fontColor = Color.white;
		}


		public void ResetToDefaults()
		{
			buttons = DialogButton.OK;
		}
	}


	public class UIButtonPanel : MonoBehaviour, IUIBehavior
	{
		public enum DialogButton
		{
			OK = 0x1,
			OKCancel = 0x2,
			YesNoCancel = 0x3,
			YesNo = 0x4,
		}

		/// <summary>
		/// These are min widths when font size is 24.
		/// TODO(Tristan): dynamic widths on font change!
		/// TODO(Tristan): show dictionary in editor.
		/// </summary>
		[UDictionary.Split(50, 50)]
		private Dictionary<DialogButton, float> minButtonWidth = new()
		{
			[DialogButton.OK] = 150,
			[DialogButton.OKCancel] = 300,
			[DialogButton.YesNoCancel] = 450,
			[DialogButton.YesNo] = 300,
		};

		[SerializeField] private ButtonPanelEx buttonPanelEx;

		private UIDesignObject _designObject;
		public UIDesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<UIDesignObject>();
				return _designObject;
			}
		}

		public ButtonEx okButtonData
		{
			get
			{
				if (buttonPanelEx.useCustomOKButton || buttonPanelEx.scriptableObj == null)
				{
					return buttonPanelEx.okButton;
				}

				return buttonPanelEx.scriptableObj.okButton;
			}
		}

		public ButtonEx cancelButtonData
		{
			get
			{
				if (buttonPanelEx.useCustomCancelButton || buttonPanelEx.scriptableObj == null)
				{
					return buttonPanelEx.cancelButton;
				}

				return buttonPanelEx.scriptableObj.cancelButton;
			}
		}

		public ButtonEx yesButtonData
		{
			get
			{
				if (buttonPanelEx.useCustomYesButton || buttonPanelEx.scriptableObj == null)
				{
					return buttonPanelEx.yesButton;
				}

				return buttonPanelEx.scriptableObj.yesButton;
			}
		}

		public ButtonEx noButtonData
		{
			get
			{
				if (buttonPanelEx.useCustomNoButton || buttonPanelEx.scriptableObj == null)
				{
					return buttonPanelEx.noButton;
				}

				return buttonPanelEx.scriptableObj.noButton;
			}
		}


		public Sprite ButtonSpriteAsset(ButtonEx buttonEx)
		{
			if (buttonEx.useCustomSprite || buttonEx.scriptableObj == null)
				return buttonEx.sprite;
			return buttonEx.scriptableObj.sprite;
		}

		public TMP_FontAsset ButtonFontAsset(ButtonEx buttonEx)
		{
			if (buttonEx.scriptableObj == null)
				return buttonEx.labelEx.fontAsset;
			if (buttonEx.scriptableObj.labelEx.useCustomFontAsset || buttonEx.scriptableObj.labelEx.scriptableObj == null)
				return buttonEx.scriptableObj.labelEx.fontAsset;
			return buttonEx.scriptableObj.labelEx.scriptableObj.fontAsset;
		}

		public Color ButtonFontColor(ButtonEx buttonEx)
		{
			if (buttonEx.scriptableObj == null)
				return buttonEx.labelEx.fontColor;
			if (buttonEx.scriptableObj.labelEx.useCustomFontColor || buttonEx.scriptableObj.labelEx.scriptableObj == null)
				return buttonEx.scriptableObj.labelEx.fontColor;
			return buttonEx.scriptableObj.labelEx.scriptableObj.fontColor;
		}

		public float ButtonFontSize(ButtonEx buttonEx)
		{
			if (buttonEx.scriptableObj == null)
				return buttonEx.labelEx.fontSize;
			if (buttonEx.scriptableObj.labelEx.useCustomFontSize || buttonEx.scriptableObj.labelEx.scriptableObj == null)
				return buttonEx.scriptableObj.labelEx.fontSize;
			return buttonEx.scriptableObj.labelEx.scriptableObj.fontSize;
		}


		[SerializeField] private GameObject okButton;
		[SerializeField] private GameObject yesButton;
		[SerializeField] private GameObject noButton;
		[SerializeField] private GameObject cancelButton;


		public IUIDataEx GetBackingData()
		{
			return buttonPanelEx;
		}

		public void SetResultListeners(DynamicPanel parent)
		{
			okButton.GetComponent<Button>().onClick.RemoveAllListeners();
			okButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultOK);
			cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
			cancelButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultCancel);
			yesButton.GetComponent<Button>().onClick.RemoveAllListeners();
			yesButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultYes);
			noButton.GetComponent<Button>().onClick.RemoveAllListeners();
			noButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultNo);
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			buttonPanelEx = (ButtonPanelEx)backingData;
			UpdateBackingData();
		}


		public void UpdateBackingData()
		{
			okButton.SetActive(false);
			yesButton.SetActive(false);
			noButton.SetActive(false);
			cancelButton.SetActive(false);


			SetButton(okButtonData, okButton);
			SetButton(cancelButtonData, cancelButton);
			SetButton(yesButtonData, yesButton);
			SetButton(noButtonData, noButton);


			switch (buttonPanelEx.buttons)
			{
				case DialogButton.OK:
				{
					okButton.SetActive(true);
				}
				break;

				case DialogButton.OKCancel:
				{
					okButton.SetActive(true);
					cancelButton.SetActive(true);
				}
				break;

				case DialogButton.YesNoCancel:
				{
					yesButton.SetActive(true);
					noButton.SetActive(true);
					cancelButton.SetActive(true);
				}
				break;

				case DialogButton.YesNo:
				{
					yesButton.SetActive(true);
					noButton.SetActive(true);
				}
				break;

				//case DialogButton.None:
				//{
				//	Debug.LogException(new System.Exception("Invalid state call"));
				//}
				//break;
			}
		}

		private void SetButton(ButtonEx buttonData, GameObject button)
		{
			if (buttonData == null)
				return;
			var sprite = ButtonSpriteAsset(buttonData);
			if (sprite != null)
				button.GetComponent<Image>().sprite = sprite;
			var tmp = button.GetComponentInChildren<TextMeshProUGUI>();
			tmp.font = ButtonFontAsset(buttonData);
			tmp.color = ButtonFontColor(buttonData);
			tmp.fontSize = ButtonFontSize(buttonData);
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			return new Vector2(minButtonWidth[buttonPanelEx.buttons], GetComponent<RectTransform>().sizeDelta.y);
		}


		public void ResetToLastPosition()
		{
			throw new System.NotImplementedException();
		}

		public UIDesignObject Select()
		{
			throw new System.NotImplementedException();
		}

		public void Deselect()
		{
			throw new System.NotImplementedException();
		}

		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}

		public void SetHover(bool isHover)
		{
			throw new System.NotImplementedException();
		}

		public void UpdateHover(Vector3 posOfHover)
		{
			throw new System.NotImplementedException();
		}
	}
}
