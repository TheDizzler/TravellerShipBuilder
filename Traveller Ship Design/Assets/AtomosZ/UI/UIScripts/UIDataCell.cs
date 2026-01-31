using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.UIDataRow;
using static AtomosZ.UI.UIPrefabProvider;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	/// <summary>
	/// A placeholder control to fill grid cells.
	/// </summary>
	public class UIDataCell : UIMonoBehaviour, IUIBehavior
	{
		public MagicWindow.UIControlType dataType { get { return MagicWindow.UIControlType.DataCell; } }
		public bool interactable { get; set; }

		public UIMonoBehaviour control;


		public void UpdateBackingData()
		{
			if (control != null)
				control.iUIBehavior.UpdateBackingData();
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			if (control != null)
				return control.iUIBehavior.GetControl(controlRefName);
			return null;
		}


		public override void ReturnToPool()
		{
			if (control != null)
			{
				control.ReturnToPool();
				control = null;
			}

			GetComponent<Image>().enabled = false;
			base.ReturnToPool();
		}

		public Vector2 GetMinDimensions()
		{
			return rect.sizeDelta;
		}

		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

		internal void SetControl(UICellDataTypes controlType)
		{
			if (control != null)
			{
				if (control.pool == null)
					control.pool = UIPrefabProvider.GetPoolOfType(control.iUIBehavior.dataType);
				control.ReturnToPool();
			}

			var parent = GetComponentInParent<UIDataRow>();

			UIPrefabType prefabType = ConvertCellDataTypeToPrefabType(controlType);

			control = UIPrefabProvider.GetMagicUIControl(prefabType, transform);
			control.referenceName = referenceName + $"_{prefabType}";
			control.rect.anchorMin = new Vector2(.5f, .5f);
			control.rect.anchorMax = new Vector2(.5f, .5f);
			control.rect.anchoredPosition = Vector3.zero;
			// @TODO(Tristan): should set min and max size of control instead
			//control.enabled = false; // this prevents the UI control from autosizing itself

			//control.fitToParent = true; // implement this!

			if (control.TryGetComponent<TextMeshProUGUI>(out var tmp))
			{
				tmp.enableAutoSizing = true;
				tmp.fontSizeMin = 3;
			}

		}


		public UIPrefabType ConvertCellDataTypeToPrefabType(UICellDataTypes dataType)
		{
			switch (dataType)
			{
				case UICellDataTypes.Text:
					return UIPrefabType.ExpandingLabel;

				case UICellDataTypes.Button:
					return UIPrefabType.Button;

				case UICellDataTypes.CheckBox:
					return UIPrefabType.CheckBox;

				case UICellDataTypes.Dropdown:
					return UIPrefabType.Dropdown;

				case UICellDataTypes.HorizontalPanel:
					return UIPrefabType.HorizontalPanel;

				case UICellDataTypes.Image:
					return UIPrefabType.ImageView;

				case UICellDataTypes.InputField:
					return UIPrefabType.InputField;

				case UICellDataTypes.Panel:
					return UIPrefabType.Panel;

				case UICellDataTypes.Slider:
					return UIPrefabType.Slider;

				case UICellDataTypes.Spinner:
					return UIPrefabType.Spinner;

					//case UICellDataTypes.:
					//return UIPrefabType.;
			}

			throw new Exception($"{dataType} not implemented");

		}
	}
}