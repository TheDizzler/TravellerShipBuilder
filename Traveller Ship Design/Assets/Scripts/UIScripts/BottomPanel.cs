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
#if UNITY_EDITOR
			DestroyImmediate(buttonPanel.gameObject);
#else
			Destroy(buttonPanel.gameObject);
#endif
			return;
		}

		buttonPanel.SetButtons(buttons, parentPanel);
	}

	public void AddText(LabelEx labelEx)
	{
		if (string.IsNullOrEmpty(labelEx.text))
		{
			Debug.LogWarning("Text may not be empty");
			return;
		}

		var textBlock = Instantiate(DesignManager.GetUIPrefab(UIPrefabType.TextBlock), transform);
		var label = textBlock.GetComponent<UIExpandingLabel>();
		label.UpdateLabel(labelEx);
		items.Add(textBlock);
	}



	/// <summary>
	/// TODO(Tristan): Now that using LabelEx use of this property should be strongly discouraged.
	/// </summary>
	/// <param name="text"></param>
	[Obsolete("Use AddText(LabelEx labelEx) instead.")]
	public void AddText(string text)
	{
		var textBlock = Instantiate(DesignManager.GetUIPrefab(UIPrefabType.TextBlock), transform);
		var label = textBlock.GetComponent<UIExpandingLabel>();
		label.text = text;
		items.Add(textBlock);
	}

	public TMP_InputField AddInputField(InputFieldEx inputFieldEx)
	{
		var input = Instantiate(DesignManager.GetUIPrefab(UIPrefabType.TextInputField), transform);
		var inputRect = input.GetComponent<RectTransform>();
		var inputField = input.GetComponent<UIExpandingInputField>();
		inputField.UpdateInputField(inputFieldEx);
		var inputTMP = input.GetComponent<TMP_InputField>();
		inputTMP.onSubmit.AddListener(SubmitText);
		items.Add(input);
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
			Destroy(item.gameObject);
		items.Clear();

		parentPanel.RecalculateDimensions();
	}

#if UNITY_EDITOR
	public void ClearItemsEditor()
	{
		foreach (var item in items)
			DestroyImmediate(item.gameObject);

		if (transform.childCount > 0)
		{
			foreach (var childDO in transform.GetComponentsInChildren<UIDesignObject>())
				DestroyImmediate(childDO.gameObject);
		}

		items.Clear();
	}
#endif

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
