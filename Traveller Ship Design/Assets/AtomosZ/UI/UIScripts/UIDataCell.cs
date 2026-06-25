using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindowBase;
using static AtomosZ.UI.UIDataRow;
using static AtomosZ.UI.UIPrefabProvider;

namespace AtomosZ.UI
{
	[ExecuteInEditMode]
	/// <summary>
	/// A placeholder control to fill grid cells.
	/// </summary>
	public class UIDataCell : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.DataCell; } }
		public bool interactable { get; set; }

		public UIMonoBehaviour control;


		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			referenceName = _referenceName;
			interactable = _interactable;
		}

		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			if (control != null)
				return control.iUIBehavior.GetControl(controlRefName);
			return null;
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public override void RecalculateDimensions()
		{
			var height = _minDimensions.y;

			// set size so control can set size to it
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _minDimensions.x);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _minDimensions.y);
			if (control != null)
			{
				control.iUIBehavior.minDimensions = _minDimensions;
				control.fillParentHorizontal = true;
			}

			if (control != null)
			{
				var ctrlSize = control.iUIBehavior.GetDrawnSize();
				height = Mathf.Max(height, ctrlSize.y);
			}

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

			preferredSize.x = _minDimensions.x;
			preferredSize.y = height;
			isDirty = false;
		}



		public Vector2 GetDrawnSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		[SerializeField] private Vector2 preferredSize;
		public Vector2 GetPreferredSize()
		{
			if (isDirty)
				RecalculateDimensions();
			return preferredSize;
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
			RemoveControl();

			var parent = GetComponentInParent<UIDataRow>();

			UIPrefabType prefabType = ConvertCellDataTypeToPrefabType(controlType);

			control = UIPrefabProvider.GetMagicUIControl(prefabType, transform);
			control.referenceName = referenceName + $"_{prefabType}";
			control.rect.anchorMin = new Vector2(.5f, .5f);
			control.rect.anchorMax = new Vector2(.5f, .5f);
			control.rect.anchoredPosition = Vector3.zero;
			// @TODO?(Tristan): should set min and max size of control instead ?
			control.iUIBehavior.minDimensions = minDimensions;

			//control.fitToParent = true; // implement this!

			control.fillParentHorizontal = true;
			if (control.TryGetComponent<UIExpandingLabel>(out var label))
			{
				label.autoSizeFont = true;
				label.fontSizeMin = 3;
				label.text = "Cell Text";
			}

		}

		internal void RemoveControl()
		{
			if (control == null)
				return;
			((ObjectForge.IPooledObject)control).ReturnToPool();
			control = null;
		}


		public void ReturnToPool()
		{
			if (control != null)
			{
				if (control.TryGetComponent(out PooledObject pooledControl))
					pooledControl.ReturnToPool();
				control = null;
			}

			GetComponent<Image>().enabled = false;
			GetComponent<PooledObject>().ReturnToPool();
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