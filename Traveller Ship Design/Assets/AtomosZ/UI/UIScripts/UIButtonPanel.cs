using System;
using System.Collections;
using System.Collections.Generic;

using TMPro;

using UnityEngine;
using UnityEngine.UI;


namespace AtomosZ.UI
{
	[Serializable]
	public class ButtonPanelEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.ButtonPanel; } }

		public UIButtonPanelScriptableObject scriptableObj;

		public ButtonPanelEx(UIButtonPanelScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
		}
	}

	[ExecuteAlways]
	public class UIButtonPanel : MonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.ButtonPanel; } }

		[SerializeField] private UIButton okButton;
		[SerializeField] private UIButton yesButton;
		[SerializeField] private UIButton noButton;
		[SerializeField] private UIButton cancelButton;

		public enum DialogButton
		{
			OK = 0x1,
			OKCancel = 0x2,
			YesNoCancel = 0x3,
			YesNo = 0x4,
		}

		[SerializeField] private DialogButton _buttons = DialogButton.OK;
		public DialogButton buttons
		{
			get { return _buttons; }
			set
			{
				_buttons = value;

				okButton.gameObject.SetActive(false);
				yesButton.gameObject.SetActive(false);
				noButton.gameObject.SetActive(false);
				cancelButton.gameObject.SetActive(false);


				SetButton(buttonPanelData.okButtonData, okButton);
				SetButton(buttonPanelData.cancelButtonData, cancelButton);
				SetButton(buttonPanelData.yesButtonData, yesButton);
				SetButton(buttonPanelData.noButtonData, noButton);


				switch (value)
				{
					case DialogButton.OK:
					{
						okButton.gameObject.SetActive(true);
					}
					break;

					case DialogButton.OKCancel:
					{
						okButton.gameObject.SetActive(true);
						cancelButton.gameObject.SetActive(true);
					}
					break;

					case DialogButton.YesNoCancel:
					{
						yesButton.gameObject.SetActive(true);
						noButton.gameObject.SetActive(true);
						cancelButton.gameObject.SetActive(true);
					}
					break;

					case DialogButton.YesNo:
					{
						yesButton.gameObject.SetActive(true);
						noButton.gameObject.SetActive(true);
					}
					break;
				}

				this.SetDirty();
			}
		}

		private void SetButton(UIButtonScriptableObject buttonData, UIButton button)
		{
			if (buttonData == null)
				return;
			button.UpdateBackingData(buttonData);
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

		[SerializeField] private UIButtonPanelScriptableObject buttonPanelData;

		[SerializeField] private string _referenceName = "buttonPanel";
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				this.SetGameObjectNameToReferenceName(gameObject);
			}
		}


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

		public bool isDirty { get; set; }

		
		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			if (okButton.GetComponent<UIButton>().referenceName == controlRefName)
				return okButton.GetComponent<UIButton>();
			if (yesButton.GetComponent<UIButton>().referenceName == controlRefName)
				return yesButton.GetComponent<UIButton>();
			if (yesButton.GetComponent<UIButton>().referenceName == controlRefName)
				return yesButton.GetComponent<UIButton>();
			if (cancelButton.GetComponent<UIButton>().referenceName == controlRefName)
				return cancelButton.GetComponent<UIButton>();
			return null;
		}


		[Obsolete("TODO: Replace with MagicWindow")]
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

		public void SetResultListeners(MagicWindow magicWindow)
		{
			Debug.LogWarning("Magic Window not set up for button panel!");
		}

		void OnEnable()
		{
			this.SetDirty();
		}


		public IUIDataEx GetBackingData()
		{
			return new ButtonPanelEx(buttonPanelData);
		}

		public void UpdateBackingData(UIButtonPanelScriptableObject backingData)
		{
			buttonPanelData = backingData;
			if (backingData != null)
			{
				okButton.UpdateBackingData(backingData.okButtonData);
				yesButton.UpdateBackingData(backingData.yesButtonData);
				noButton.UpdateBackingData(backingData.noButtonData);
				cancelButton.UpdateBackingData(backingData.cancelButtonData);
			}

			this.SetDirty();
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			UpdateBackingData(((ButtonPanelEx)backingData).scriptableObj);
		}


		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			var horzLayout = GetComponent<HorizontalLayoutGroup>();
			if (horzLayout == null)
				Debug.LogException(new Exception("No layout group found on panel"));

			Vector2 minDim = new Vector2(minButtonWidth[_buttons], 10);
			minDim.x = horzLayout.padding.left + horzLayout.padding.right;
			minDim.y = 0;

			var activeChildren = 0;
			var uiControls = new UIButton[]
			{
				okButton, yesButton, noButton, cancelButton
			};

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

			var rect = GetComponent<RectTransform>();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);

			isDirty = false;
		}



		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();

			return GetComponent<RectTransform>().sizeDelta;
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
