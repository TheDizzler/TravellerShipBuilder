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

	[ExecuteAlways]
	public class UIDropdown : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private DropdownEx dropdownEx;

		[SerializeField] private string _referenceName = "dropdown";
		public string referenceName
		{
			get { return _referenceName; }
			set
			{
				_referenceName = value;
				this.SetGameObjectNameToReferenceName(gameObject);
			}
		}


		public bool isDirty { get; set; } = true;
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

		public UnityEvent<UIDropdown, int> onValueChangedAction = null;

		[SerializeField] private int _value;
		public int value
		{
			get { return _value; }
			set
			{
				var oldValue = _value;
				dropdown.value = _value = value;
				if (oldValue != value)
				{
					this.SetDirty();
					if (onValueChangedAction != null)
						onValueChangedAction.Invoke(this, value);
				}
			}
		}

		[SerializeField] private bool _isMultiSelect = false;
		public bool isMultiSelect
		{
			get { return _isMultiSelect; }
			set
			{
				dropdown.MultiSelect = _isMultiSelect = value;
				// do we need to clamp a multiselect value to a single select value?
			}
		}

		[SerializeField]
		private List<TMP_Dropdown.OptionData> _options;

		public List<TMP_Dropdown.OptionData> options
		{
			get { return _options = dropdown.options; }
			set
			{
				_options = dropdown.options = value;
				this.SetDirty();
			}
		}

		[SerializeField] private UnityEvent<UIDropdown> _optionsDelegate = null;

		[Tooltip("A delegate to auto-populate the options list.")]
		public UnityEvent<UIDropdown> optionsDelegate
		{
			get { return _optionsDelegate; }
			set
			{
				_optionsDelegate = value;
				if (optionsDelegate != null)
					_optionsDelegate.Invoke(this);
			}
		}

		[Conditional("UNITY_EDITOR")]
		public void UpdateOptionsDelegate()
		{
			optionsDelegate = _optionsDelegate;
		}

		public void AddOption(TMP_Dropdown.OptionData option)
		{
			dropdown.AddOptions(new List<TMP_Dropdown.OptionData> { option });
		}

		public void AddOptions(List<TMP_Dropdown.OptionData> options)
		{
			dropdown.AddOptions(options);
		}

		public void ClearOption()
		{
			dropdown.ClearOptions();
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


		public int SelectedIndex()
		{
			return value;
		}

		public string SelectedValue()
		{
			return _options[value].text;
		}


		public void OnEnable()
		{
			if (optionsDelegate != null)
				optionsDelegate.Invoke(this);
			else
				dropdown.ClearOptions();

			this.SetDirty();
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


		void Update()
		{
			if (isDirty)
				UpdateBackingData();
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
			this.SetGameObjectNameToReferenceName(gameObject);

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

			isDirty = false;
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			var sDelta = GetComponent<RectTransform>().sizeDelta;
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