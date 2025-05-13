using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonPanel : MonoBehaviour, IUIBehavior
{
	public enum DialogButton
	{
		None = 0x0,
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
		[DialogButton.None] = 0,
		[DialogButton.OK] = 150,
		[DialogButton.OKCancel] = 300,
		[DialogButton.YesNoCancel] = 450,
		[DialogButton.YesNo] = 300,
	};

	private DialogButton buttons;
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

	public void SetButtons(DialogButton newButtons, DynamicPanel parent)
	{
		buttons = newButtons;
		okButton.SetActive(false);
		yesButton.SetActive(false);
		noButton.SetActive(false);
		cancelButton.SetActive(false);

		switch (buttons)
		{
			case DialogButton.OK:
			{
				okButton.SetActive(true);
				okButton.GetComponent<Button>().onClick.RemoveAllListeners();
				okButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultOK);
			}
			break;

			case DialogButton.OKCancel:
			{
				okButton.SetActive(true);
				okButton.GetComponent<Button>().onClick.RemoveAllListeners();
				okButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultOK);
				cancelButton.SetActive(true);
				cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
				cancelButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultCancel);
			}
			break;

			case DialogButton.YesNoCancel:
			{
				yesButton.SetActive(true);
				yesButton.GetComponent<Button>().onClick.RemoveAllListeners();
				yesButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultYes);
				noButton.SetActive(true);
				noButton.GetComponent<Button>().onClick.RemoveAllListeners();
				noButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultNo);
				cancelButton.SetActive(true);
				cancelButton.GetComponent<Button>().onClick.RemoveAllListeners();
				cancelButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultCancel);
			}
			break;

			case DialogButton.YesNo:
			{
				yesButton.SetActive(true);
				yesButton.GetComponent<Button>().onClick.RemoveAllListeners();
				yesButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultYes);
				noButton.SetActive(true);
				noButton.GetComponent<Button>().onClick.RemoveAllListeners();
				noButton.GetComponent<Button>().onClick.AddListener(parent.SetDialogResultNo);
			}
			break;

			case DialogButton.None:
			{
				Debug.LogException(new System.Exception("Invalid state call"));
			}
			break;
		}
	}

	public Vector2 GetMinDimensions()
	{
		return new Vector2(minButtonWidth[buttons], GetComponent<RectTransform>().sizeDelta.y);
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
