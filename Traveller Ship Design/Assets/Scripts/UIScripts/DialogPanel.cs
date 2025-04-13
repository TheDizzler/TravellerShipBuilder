using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogPanel : MonoBehaviour
{
	public enum DialogButton
	{
		OK = 0x0,
		OKCancel = 0x1,
		YesNoCancel = 0x2,
		YesNo = 0x3,
	}

	public enum DialogResult
	{
		None,
		OK,
		Cancel,
		Yes,
		No,
	}

	//[SerializeField] private RectTransform parentRect;
	[SerializeField] private BladedPanel parentPanel;
	/// <summary>
	/// When font size is 24
	/// </summary>
	[SerializeField] private float min1ButtonWidth = 150;
	/// <summary>
	/// When font size is 24
	/// </summary>
	[SerializeField] private float min2ButtonWidth = 300;
	/// <summary>
	/// When font size is 24
	/// </summary>
	[SerializeField] private float min3ButtonWidth = 450;
	[SerializeField] private GameObject okButton;
	[SerializeField] private GameObject yesButton;
	[SerializeField] private GameObject noButton;
	[SerializeField] private GameObject cancelButton;

	public DialogButton buttons;
	public DialogResult result;

	public float minWidth;

	/// <summary>
	/// temporary hardcoded text box. We will generalize this to work with and TMP text that is added to a panel.
	/// </summary>
	public TextMeshProUGUI tmp;
	public string text;
	[SerializeField] private Vector2 singleLineSize;
	[SerializeField] private Vector2 maxTextBlockSize;

	public List<RectTransform> items;

	public void SetText(string text)
	{
		tmp.text = text;
		tmp.ForceMeshUpdate();

		parentPanel.RecalculateDimensions();
	}

	public Vector2 GetMinDimensions()
	{
		var minDim = Vector2.zero;
		var layout = GetComponent<VerticalLayoutGroup>();
		minDim.x = minWidth;
		minDim.y = layout.padding.top + layout.padding.bottom;
		var activeChildren = 0;
		foreach (var child in items)
		{
			if (!child.gameObject.activeSelf)
				continue;

			++activeChildren;
			var tmp = child.GetComponent<TextMeshProUGUI>();
			if (tmp != null)
			{
				var tmpRect = child;
				var tmpWidth = tmpRect.sizeDelta.x;
				var preferredSize = tmp.GetPreferredValues();
				var renderedSize = tmp.GetRenderedValues();
				var newSize = preferredSize;
				if (newSize.x >= tmpWidth)
				{
					if (newSize.x > maxTextBlockSize.x)
						newSize.x = maxTextBlockSize.x;
					var diff = newSize.x - tmpWidth;
					var parentWidth = parentPanel.GetComponent<RectTransform>().sizeDelta.x;
					if (minDim.x > parentWidth + diff + 1)
						minDim.x = parentWidth + diff + 1;
					//parentRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, parentRect.sizeDelta.x + diff + 1);
				}

				//var lineCount = Mathf.CeilToInt(newSize.y / singleLineHeight);

				//Debug.Log("preferredSize: " + preferredSize + "\nrenderedSize: " + renderedSize);

				if (newSize.y > maxTextBlockSize.y)
				{   // set to paging? scrolling? toggle?
					newSize.y = maxTextBlockSize.y;
					tmp.overflowMode = TextOverflowModes.Ellipsis;
					Debug.LogWarning("We need to do something about terribly long text?");
				}
				else
				{
					tmp.overflowMode = TextOverflowModes.Overflow;
				}

				if (newSize.y < singleLineSize.y)
					newSize.y = singleLineSize.y;
				tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
				Debug.Log($"prefSize: {preferredSize}\trenderedSize: {renderedSize}\t newSIze: {newSize}"/*\t lines: " + lineCount*/);
				minDim.y += newSize.y;
			}
			else
			{
				minDim.y += child.sizeDelta.y;
			}
		}

		minDim.y += layout.spacing * (activeChildren - 1);


		//foreach (var child in items)
		//{
		//	if (child.name == "ButtonPanel")
		//		continue;
		//	if (child.gameObject.activeSelf)
		//		if (child.sizeDelta.x > minDim.x)
		//			minDim.x = child.sizeDelta.x;
		//}

		minDim.x += layout.padding.left + layout.padding.right;
		Debug.Log(minDim);
		return minDim;
	}


	public void SetButtons(DialogButton buttons)
	{
		this.buttons = buttons;

		okButton.SetActive(false);
		yesButton.SetActive(false);
		noButton.SetActive(false);
		cancelButton.SetActive(false);

		switch (buttons)
		{
			case DialogButton.OK:
			{
				okButton.SetActive(true);
				minWidth = min1ButtonWidth;
			}
			break;

			case DialogButton.OKCancel:
			{
				okButton.SetActive(true);
				cancelButton.SetActive(true);
				minWidth = min2ButtonWidth;
			}
			break;

			case DialogButton.YesNoCancel:
			{
				yesButton.SetActive(true);
				noButton.SetActive(true);
				cancelButton.SetActive(true);
				minWidth = min3ButtonWidth;
			}
			break;

			case DialogButton.YesNo:
			{
				yesButton.SetActive(true);
				noButton.SetActive(true);
				minWidth = min2ButtonWidth;
			}
			break;
		}

		parentPanel.RecalculateDimensions();
	}


	public void SetDialogResultOK()
	{
		this.result = DialogResult.OK;
	}

	public void SetDialogResultCancel()
	{
		this.result = DialogResult.Cancel;
	}

	public void SetDialogResultYes()
	{
		this.result = DialogResult.Yes;
	}

	public void SetDialogResultNo()
	{
		this.result = DialogResult.No;
	}
}
