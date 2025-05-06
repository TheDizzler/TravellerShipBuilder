using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static DesignManager;
using static ButtonPanel;

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
	[SerializeField] private List<UIDesignObject> items;


	void Awake()
	{
		ClearItems();
	}

	public Vector2 GetMinDimensions()
	{
		var minDim = Vector2.zero;
		var layout = GetComponent<VerticalLayoutGroup>();
		minDim.x = 0;
		minDim.y = layout.padding.top + layout.padding.bottom;
		var activeChildren = 0;
		foreach (var child in items)
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

	//public void AddImage()

	public void AddButtons(DialogButton buttons)
	{
		ButtonPanel buttonPanel = GetComponentInChildren<ButtonPanel>();
		if (buttonPanel == null)
		{
			if (buttons == DialogButton.None)
				return;
			var buttonPanelDO = Instantiate(DesignManager.GetUIPrefab(UIPrefabType.ButtonPanel), transform);
			buttonPanel = buttonPanelDO.GetComponent<ButtonPanel>();
			items.Add(buttonPanelDO);
		}
		else if (buttons == DialogButton.None)
		{
			items.Remove(buttonPanel.GetComponent<UIDesignObject>());
			Destroy(buttonPanel.gameObject);
			return;
		}

		buttonPanel.SetButtons(buttons, parentPanel);
	}

	public void AddText(string text)
	{
		var textBlock = Instantiate(DesignManager.GetUIPrefab(UIPrefabType.TextBlock), transform);
		var tmpRect = textBlock.GetComponent<RectTransform>();
		tmpRect.SetSiblingIndex(items.Count - 1);
		var tmp = textBlock.GetComponent<TextMeshProUGUI>();
		tmp.text = text;
		items.Add(textBlock);
	}

	public TMP_InputField AddInputField(string placeholderText, string defaultText = null)
	{
		var input = Instantiate(DesignManager.GetUIPrefab(UIPrefabType.TextInputField), transform);
		var inputRect = input.GetComponent<RectTransform>();
		inputRect.SetSiblingIndex(items.Count - 1);
		var inputTMP = input.GetComponent<TMP_InputField>();

		items.Add(input);
		inputTMP.placeholder.GetComponent<TextMeshProUGUI>().text = placeholderText;
		if (!string.IsNullOrEmpty(defaultText))
			inputTMP.text = defaultText;
		inputTMP.onSubmit.AddListener(SubmitText);
		return inputTMP;
	}

	private void SubmitText(string currentText)
	{
		parentPanel.SetDialogResultOK();
	}


	/// <summary>
	/// Editor script to keep anyone from tampering with the size!
	/// </summary>
	public void SetToParentsSize()
	{
		parentPanel.Refresh();
		var parentRect = parentPanel.GetComponent<RectTransform>();

		var rect = GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
	}


	/// <summary>
	/// Each UnityAction becomes one context menu item and the string becomes the text for the button.<br/>
	/// Can add multiple methods to a single UnityAction as below:<br/>
	/// <c>
	/// UnityAction action = null;<br/>
	/// action += () => FunctionWithParam("name");<br/>
	/// action += () => FunctionNoParam();<br/>
	/// action += delegate {// some code here};</c>
	/// </summary>
	/// <param name="clickActions"></param>
	public void SetContextMenuActions(Dictionary<string, DesignAction> clickActions)
	{
		ClearItems();
		foreach (var action in clickActions)
		{
			if (action.Value == null)
				AddDivider();
			else
				AddMenuItem(action.Value, action.Key);
		}
	}

	public void ClearItems()
	{
		foreach (var item in items)
			Destroy(item);
		items.Clear();

		parentPanel.RecalculateDimensions();
	}

	private void AddDivider()
	{
		if (items.Count == 0)
		{
			Debug.LogError("A divider may not be the first item in a context menu");
			return;
		}

		var divider = Instantiate(DesignManager.GetPrefab(UIPrefabType.MenuDivider), transform);

		items.Add(divider);
	}

	private void AddMenuItem(DesignAction clickAction, string buttonText)
	{
		clickAction += parentPanel.Close;
		var menuItem = Instantiate(DesignManager.GetPrefab(UIPrefabType.MenuItemButton), transform);

		menuItem.GetComponent<Button>().onClick.AddListener(clickAction.action);
		menuItem.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

		items.Add(menuItem);
	}
}
