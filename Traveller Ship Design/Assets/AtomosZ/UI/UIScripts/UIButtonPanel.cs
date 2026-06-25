using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;


namespace AtomosZ.UI
{
	public class UIButtonPanel : UIMonoBehaviour, IUIBehavior
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

		// <summary>
		// These are min widths when font size is 24.
		// TODO(Tristan): dynamic widths on font change!
		// TODO(Tristan): show dictionary in editor.
		// </summary>
		//[SerializeField] private CustomDictionary<DialogButton, float> minButtonWidth = new()
		//{
		//	[DialogButton.OK] = 150,
		//	[DialogButton.OKCancel] = 300,
		//	[DialogButton.YesNoCancel] = 450,
		//	[DialogButton.YesNo] = 300,
		//};

		[SerializeField] private UIButtonPanelScriptableObject buttonPanelData;

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable
					= okButton.interactable
					= yesButton.interactable
					= noButton.interactable
					= cancelButton.interactable
					= value;

			}
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



		public UIMonoBehaviour GetControl(string controlRefName)
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

		public void SetResultListeners(MagicWindow magicWindow)
		{
			Debug.LogWarning("Magic Window not set up for button panel!");
		}

		void OnEnable()
		{
			this.SetDirty();
		}


		public ScriptableObject GetBackingData()
		{
			return buttonPanelData;
		}


		public void UpdateBackingData(ScriptableObject backingData)
		{
			buttonPanelData = (UIButtonPanelScriptableObject)backingData;
			if (buttonPanelData != null)
			{
				okButton.UpdateBackingData(buttonPanelData.okButtonData);
				yesButton.UpdateBackingData(buttonPanelData.yesButtonData);
				noButton.UpdateBackingData(buttonPanelData.noButtonData);
				cancelButton.UpdateBackingData(buttonPanelData.cancelButtonData);
			}

			this.SetDirty();
		}



		public override void RecalculateDimensions()
		{
			var horzLayout = GetComponent<HorizontalLayoutGroup>();
			if (horzLayout == null)
				Debug.LogException(new Exception("No layout group found on panel"));

			horzLayout.childForceExpandWidth = fillParentHorizontal;

			//Vector2 minDim = new Vector2(minButtonWidth[_buttons], 10);
			Vector2 minDim = new Vector2(horzLayout.padding.horizontal, horzLayout.padding.vertical);

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
				var childMinDim = child.GetDrawnSize();
				minDim.x += childMinDim.x;
				if (minDim.y < childMinDim.y)
					minDim.y = childMinDim.y;
			}


			minDim.x += horzLayout.spacing * (activeChildren - 1);
			minDim.y += horzLayout.padding.top + horzLayout.padding.bottom;

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, minDim.y);

			preferredSize.x = minDim.x;
			preferredSize.y = minDim.y;

			isDirty = false;
		}

		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();

			return GetComponent<RectTransform>().sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
		}
	}
}
