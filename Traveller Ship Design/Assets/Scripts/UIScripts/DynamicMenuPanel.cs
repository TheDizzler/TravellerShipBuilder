using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static DesignManager;


public class UIPanel : MonoBehaviour, IUIBehavior
{
	//[SerializeField] private Button buttonPrefab;
	//[SerializeField] private GameObject dividerPrefab;
	[SerializeField] private int panelMinHeight = 58;

	[SerializeField] private List<UIDesignObject> items;

	private RectTransform _transRect;
	private RectTransform transRect
	{
		get
		{
			if (_transRect == null)
				_transRect = GetComponent<RectTransform>();
			return _transRect;
		}
	}

	private VerticalLayoutGroup _layout;
	private VerticalLayoutGroup layout
	{
		get
		{
			if (_layout == null)
				_layout = GetComponent<VerticalLayoutGroup>();
			return _layout;
		}
	}

	public UIDesignObject designObject { get; }

	void Start()
	{
		ClosePanel();
	}

	private void Test(string inputString)
	{
		Debug.Log("this is " + inputString);
	}

	private void Test()
	{
		Debug.Log("hi");
	}

	int num = 1;
	public void TestAdd()
	{
		UnityAction action1 = null;
		action1 += () => Test();

		if (num > 2)
		{
			AddDivider();
		}

		var dict = new Dictionary<string, DesignAction>();
		for (int i = 0; i < num; ++i)
		{
			var newAction = new DesignAction(EditMode.None);
			newAction += action1;
			dict.Add("Test " + i, newAction);
		}

		++num;

		SetContextMenu(dict);
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
	public void SetContextMenu(Dictionary<string, DesignAction> clickActions)
	{
		foreach (var action in clickActions)
		{
			if (action.Value == null)
				AddDivider();
			else
				AddMenuItem(action.Value, action.Key);
		}
	}

	public void ShowContextMenu(Vector2 pos)
	{
		transform.position = pos;
		gameObject.SetActive(true);
	}

	public void ClosePanel()
	{
		gameObject.SetActive(false);
		ClearItems();
	}

	public void ClearItems()
	{
		num = 1;
		foreach (var item in items)
			Destroy(item);
		items.Clear();


		transRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelMinHeight);
	}

	private void AddDivider()
	{
		if (items.Count == 0)
		{
			Debug.LogError("A divider may not be the first item in a context menu");
		}

		var divider = Instantiate(DesignManager.GetPrefab(UIPrefabType.MenuDivider), transform);

		items.Add(divider);
		var dividerRect = divider.GetComponent<RectTransform>();
		transRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transRect.sizeDelta.y + dividerRect.sizeDelta.y + layout.spacing);
	}

	private void AddMenuItem(DesignAction clickAction, string buttonText)
	{
		clickAction += ClosePanel;
		var menuItem = Instantiate(DesignManager.GetPrefab(UIPrefabType.MenuItemButton), transform);

		menuItem.GetComponent<Button>().onClick.AddListener(clickAction.action);
		menuItem.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;

		items.Add(menuItem);

		var itemRect = menuItem.GetComponent<RectTransform>();
		float growBy = itemRect.sizeDelta.y;
		if (items.Count == 1)
			growBy -= (layout.spacing + layout.padding.top);
		else
			growBy += layout.spacing;
		transRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, transRect.sizeDelta.y + growBy);
	}

	public void MouseDrag(Vector2 screenPosition)
	{
		throw new NotImplementedException();
	}

	public bool IsDragging()
	{
		throw new NotImplementedException();
	}

	public void EndDrag(Vector2 pos)
	{
		throw new NotImplementedException();
	}

	public void ResetToLastPosition()
	{
		throw new NotImplementedException();
	}

	public DesignObject Select()
	{
		throw new NotImplementedException();
	}

	public void Deselect()
	{
		throw new NotImplementedException();
	}

	public void Clicked(Vector3 mouseWorldPos, KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
	{
		throw new NotImplementedException();
	}
}
