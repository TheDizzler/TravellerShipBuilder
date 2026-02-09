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

		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				// set size here so control can set size to it
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
				rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
				if (control != null)
				{
					control.iUIBehavior.minDimensions = new Vector2(value.x, value.y);
					control.fillParentHorizontal = true;
				}

				this.SetDirty();
			}
		}

		public Vector2 maxDimensions { get; set; }


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

		public void RecalculateDimensions()
		{
			var height = _minDimensions.y;
			if (control != null)
			{
				var ctrlSize = control.iUIBehavior.GetDrawnDimensions();
				height = Mathf.Max(height, ctrlSize.y);
			}

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

			isDirty = false;
		}



		public Vector2 GetDrawnDimensions()
		{
			if (isDirty)
				RecalculateDimensions();

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
			control.ReturnToPool();
			control = null;
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