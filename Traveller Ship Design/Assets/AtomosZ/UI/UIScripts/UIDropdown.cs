using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

using TMPro;

using UnityEngine;

using Debug = UnityEngine.Debug;

namespace AtomosZ.UI
{
	[Serializable]
	public class DropdownEx : IUIDataEx
	{
		public PanelControlType dataType { get { return PanelControlType.Dropdown; } }

		public float fontSize = 14;
		public Color fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);

		public TMP_FontAsset fontAsset;
		public List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>
		{
			new TMP_Dropdown.OptionData{ text = "Option 1" },
			new TMP_Dropdown.OptionData{ text = "Option 2" },
			new TMP_Dropdown.OptionData{ text = "Option 3" },
			new TMP_Dropdown.OptionData{ text = "Option 4" },
		};

		[Tooltip("Ordinal of default selection. -1 == always select last option, 0 == always first option.")]
		public int defaultSelection = 0;
		public bool isMultiSelect = false;
		public List<UIDropdown.OnSelectionChangedDelegate> onValueChangedActions = new();

		public void ResetToDefaults()
		{
			options.Clear();
			options.Add(new TMP_Dropdown.OptionData { text = "Option 1" });
			options.Add(new TMP_Dropdown.OptionData { text = "Option 2" });
			options.Add(new TMP_Dropdown.OptionData { text = "Option 3" });
			options.Add(new TMP_Dropdown.OptionData { text = "Option 4" });
			isMultiSelect = false;
			defaultSelection = 0;
			onValueChangedActions.Clear();

			fontSize = 14;
			fontColor = new Color(50.0f / 256, 50.0f / 256, 50.0f / 256, 1);
			fontAsset = null;
		}

		public object Clone()
		{
			var clone = (DropdownEx)this.MemberwiseClone();
			return clone;
		}
	}


	public class UIDropdown : MonoBehaviour, IUIBehavior
	{
		public delegate void OnSelectionChangedDelegate(int num);

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


		[SerializeField] private TMP_Dropdown dropdown;
		[SerializeField] private DropdownEx dropdownEx;



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
			dropdown.captionText.font = dropdownEx.fontAsset;
			dropdown.captionText.fontSize = dropdownEx.fontSize;
			dropdown.captionText.color = dropdownEx.fontColor;


			dropdown.ClearOptions();
			dropdown.AddOptions(dropdownEx.options);

			dropdown.MultiSelect = dropdownEx.isMultiSelect;

			dropdown.onValueChanged.RemoveAllListeners();
			foreach (var action in dropdownEx.onValueChangedActions)
				dropdown.onValueChanged.AddListener(delegate { action.Invoke(dropdown.value); });

			dropdown.onValueChanged.AddListener(delegate { SelectionChangedDebug(dropdown.value); });

			dropdown.value = dropdownEx.defaultSelection;
		}

		[Conditional("DEBUG")]
		private void SelectionChangedDebug(int num)
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

		public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput, ref UIDesignObject currentlySelectedObject)
		{
			throw new System.NotImplementedException();
		}




		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			var rect = GetComponent<RectTransform>();
			var sDelta = rect.sizeDelta;
			return sDelta;
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