using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UIButtonPanel;

[Serializable]
public class ButtonPanelDataEx : UIDataEx
{
	public PanelItemType dataType { get { return PanelItemType.ButtonPanel; } }

	public DialogButton buttons = DialogButton.OK;

	public ButtonPanelDataEx() { }

	public ButtonPanelDataEx(DialogButton buttonType)
	{
		buttons = buttonType;
	}


	public void ResetToDefaults()
	{
		buttons = DialogButton.OK;
	}

	public object Clone()
	{
		return this.MemberwiseClone();
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

	[SerializeField] private ButtonPanelDataEx buttons;

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

	[SerializeField] private GameObject okButton;
	[SerializeField] private GameObject yesButton;
	[SerializeField] private GameObject noButton;
	[SerializeField] private GameObject cancelButton;


	public UIDataEx GetBackingData()
	{
		return buttons;
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

	public void UpdateBackingData(UIDataEx backingData)
	{
		buttons = (ButtonPanelDataEx)backingData;

		UpdateBackingData();
	}


	public void UpdateBackingData()
	{
		okButton.SetActive(false);
		yesButton.SetActive(false);
		noButton.SetActive(false);
		cancelButton.SetActive(false);

		switch (buttons.buttons)
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

	public Vector2 GetMinDimensions()
	{
		UpdateBackingData();
		return new Vector2(minButtonWidth[buttons.buttons], GetComponent<RectTransform>().sizeDelta.y);
	}


	public void ResetToLastPosition()
	{
		throw new System.NotImplementedException();
	}

	/// <summary>
	/// This should return the parent panel? Ideally this should never be relevant.
	/// </summary>
	/// <returns></returns>
	public UIDesignObject Select()
	{
		return designObject;
	}

	public void Deselect()
	{
		throw new System.NotImplementedException();
	}

	public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
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
