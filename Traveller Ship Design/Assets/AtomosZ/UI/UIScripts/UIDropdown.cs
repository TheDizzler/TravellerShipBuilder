using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using TMPro;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	[Serializable]
	public class DropdownEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Dropdown; } }
		public string referenceName;

		public bool fillParentHorizontal = true;
		public Vector2 minDimensions = new Vector2(256, 64);

		public UIExpandingLabelScriptableObject scriptableObj;

		public bool useCustomFontSize = false;
		public bool useCustomFontColor = false;
		public bool useCustomFontAsset = false;

		[Tooltip("Default: 14.")]
		public float fontSize = 14;
		[Tooltip("Default: Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1).")]
		public Color fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
		[Tooltip("Default: null")]
		public TMP_FontAsset fontAsset = null;
		public List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData{ text = "Option 1" },
			new TMP_Dropdown.OptionData{ text = "Option 2" },
			new TMP_Dropdown.OptionData{ text = "Option 3" },
			new TMP_Dropdown.OptionData{ text = "Option 4" },
		};

		/// <summary>
		/// A delegate to auto-populate the options list.<br/>
		/// NOTE: any changes to the list will be erased when the control refreshes unless the listeners are removed from this.
		/// </summary>
		public UnityEvent<DropdownEx> SetOptions = null;


		[Tooltip("Ordinal of default selection. -1 == always select last option, 0 == always first option.")]
		public int defaultSelection = 0;
		public bool isMultiSelect = false;
		public UnityEvent<int> onValueChangedAction = null;


		public DropdownEx(UIExpandingLabelScriptableObject textScriptObj, List<TMP_Dropdown.OptionData> options)
		{
			this.options = options;
		}

		public DropdownEx(UIExpandingLabelScriptableObject textScriptObj, List<string> stringOptions)
		{
			this.options.Clear();
			foreach (var opt in stringOptions)
				options.Add(new TMP_Dropdown.OptionData { text = opt });
		}

		public DropdownEx(UIExpandingLabelScriptableObject textScriptObj)
		{
			this.scriptableObj = textScriptObj;

			if (scriptableObj == null)
			{
				useCustomFontSize = true;
				useCustomFontColor = true;
				useCustomFontAsset = true;
			}
		}
	}


	public class UIDropdown : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private DropdownEx dropdownEx;
		public string referenceName { get { return dropdownEx.referenceName; } set { dropdownEx.referenceName = value; } }

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

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return null;
		}

		public TMP_FontAsset fontAsset
		{
			get
			{
				if (dropdownEx.useCustomFontAsset || dropdownEx.scriptableObj == null)
					return dropdownEx.fontAsset;
				return dropdownEx.scriptableObj.fontAsset;
			}
		}

		public float fontSize
		{
			get
			{
				if (dropdownEx.useCustomFontSize || dropdownEx.scriptableObj == null)
					return dropdownEx.fontSize;
				return dropdownEx.scriptableObj.fontSize;
			}
		}

		public Color fontColor
		{
			get
			{
				if (dropdownEx.useCustomFontColor || dropdownEx.scriptableObj == null)
					return dropdownEx.fontColor;
				return dropdownEx.scriptableObj.fontColor;
			}
		}



		public void OnEnable()
		{
			UpdateBackingData(dropdownEx);
		}

		public UIDesignObject Select()
		{
			dropdown.Show();
			return designObject;
		}

		public void Deselect()
		{
			dropdown.Hide();
		}


		public IUIDataEx GetBackingData()
		{
			return dropdownEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			dropdownEx = (DropdownEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			this.SetNameToReferenceName(gameObject);

			dropdown.ClearOptions();
			if (dropdownEx.SetOptions != null)
				dropdownEx.SetOptions.Invoke(dropdownEx);
			dropdown.AddOptions(dropdownEx.options);

			dropdown.MultiSelect = dropdownEx.isMultiSelect;
			dropdown.value = dropdownEx.defaultSelection;

			dropdown.onValueChanged.RemoveAllListeners();
			if (dropdownEx.onValueChangedAction != null)
			{
				dropdown.onValueChanged.AddListener(delegate
					{
						dropdownEx.onValueChangedAction.Invoke(dropdown.value);
					});
			}

			dropdown.captionText.font = fontAsset;
			dropdown.captionText.fontSize = fontSize;
			dropdown.captionText.color = fontColor;

			var layout = GetComponent<LayoutElement>();
			if (dropdownEx.fillParentHorizontal)
				layout.flexibleWidth = 1;
			else
				layout.flexibleWidth = -1;

			layout.minWidth = dropdownEx.minDimensions.x;
			layout.minHeight = dropdownEx.minDimensions.y;
		}

		/// <summary>
		/// Adds action to the listener and the backingdata.
		/// </summary>
		/// <param name="action"></param>
		public void AddListener(UnityEvent<int> action)
		{
			dropdownEx.onValueChangedAction.AddListener(delegate
			{ action.Invoke(dropdown.value); });
		}

		/// <summary>
		/// Adds action to the listener and the backingdata.
		/// </summary>
		/// <param name="action"></param>
		public void AddListener(Action<int> action)
		{
			dropdownEx.onValueChangedAction.AddListener(delegate
			{ action.Invoke(dropdown.value); });
		}

		[Conditional("DEBUG")]
		private void TestDebug(int num)
		{
			string selected = "";
			if (dropdownEx.isMultiSelect)
			{
				if (num == 0)
					selected = "Nothing";
				else
				{
					int bit = 0x1;
					for (int i = 0; i < dropdownEx.options.Count; ++i)
					{
						if ((num & bit) == bit)
							selected += " • Option " + i;
						bit <<= 1;
					}
				}
			}
			else
				selected = "Option " + num;
			Debug.Log("Selected: " + selected);
		}


		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			var rect = GetComponent<RectTransform>();
			var sDelta = rect.sizeDelta;
			return sDelta;
		}


		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}


		public void ResetToLastPosition()
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