using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanel : MonoBehaviour
{
	public enum DialogButton
	{
		None = 0x0,
		OK = 0x1,
		OKCancel = 0x2,
		YesNoCancel = 0x3,
		YesNo = 0x4,
	}

	public enum DialogResult
	{
		None,
		OK,
		Cancel,
		Yes,
		No,
	}

	public List<RectTransform> items;
	public bool isDialog;

	/// <summary>
	/// These are min widths when font size is 24.
	/// TODO(Tristan): dynamic widths on font change!
	/// TODO(Tristan): show dictionary in editor.
	/// </summary>
	[SerializeField] private Dictionary<DialogButton, float> minButtonWidth = new()
	{
		[DialogButton.None] = 0,
		[DialogButton.OK] = 150,
		[DialogButton.OKCancel] = 300,
		[DialogButton.YesNoCancel] = 450,
		[DialogButton.YesNo] = 300,
	};

	[SerializeField] private DynamicPanel parentPanel;

	[SerializeField] private RectTransform buttonPanel;
	[SerializeField] private GameObject okButton;
	[SerializeField] private GameObject yesButton;
	[SerializeField] private GameObject noButton;
	[SerializeField] private GameObject cancelButton;



	public DialogButton buttons;
	public DialogResult result;


	public Vector2 GetMinDimensions()
	{
		var minDim = Vector2.zero;
		var layout = GetComponent<VerticalLayoutGroup>();
		minDim.x = minButtonWidth[buttons];
		minDim.y = layout.padding.top + layout.padding.bottom;
		var activeChildren = 0;
		foreach (var child in items)
		{
			if (!child.gameObject.activeSelf)
				continue;

			++activeChildren;
			// what is probably easiest here is to have a bespoke monobehavior for each
			// widget (if it requires special calculations) that implements GetMinDimensions().
			var tmp = child.GetComponent<TextMeshProUGUI>();
			if (tmp == null)
			{
				minDim.y += child.sizeDelta.y;
			}
			else
			{
				var tmpRect = child;
				var tmpWidth = tmpRect.sizeDelta.x;
				var preferredSize = tmp.GetPreferredValues();
				var renderedSize = tmp.GetRenderedValues();
				var newSize = preferredSize;
				if (newSize.x >= tmpWidth)
				{
					//if (newSize.x > maxTextBlockSize.x)
					//	newSize.x = maxTextBlockSize.x;
					var diff = newSize.x - tmpWidth;
					var parentWidth = parentPanel.GetComponent<RectTransform>().sizeDelta.x;
					if (minDim.x > parentWidth + diff + 1)
						minDim.x = parentWidth + diff + 1;
				}

				//if (newSize.y > maxTextBlockSize.y)
				//{   // set to paging? scrolling? toggle?
				//	newSize.y = maxTextBlockSize.y;
				//	tmp.overflowMode = TextOverflowModes.Ellipsis;
				//	Debug.LogWarning("We need to do something about terribly long text?");
				//}
				//else
				//{
				//	tmp.overflowMode = TextOverflowModes.Overflow;
				//}

				//if (newSize.y < singleLineSize.y)
				//	newSize.y = singleLineSize.y;
				tmpRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newSize.y);
				Debug.Log($"prefSize: {preferredSize}\trenderedSize: {renderedSize}\t newSIze: {newSize}"/*\t lines: " + lineCount*/);
				minDim.y += newSize.y;
			}
		}

		minDim.y += layout.spacing * (activeChildren - 1);
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
		buttonPanel.gameObject.SetActive(true);

		switch (buttons)
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

			case DialogButton.None:
			{
				buttonPanel.gameObject.SetActive(false);
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
