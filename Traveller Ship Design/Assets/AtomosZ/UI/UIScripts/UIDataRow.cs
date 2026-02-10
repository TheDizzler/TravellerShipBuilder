using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UIDataRow : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.DataRow; } }
		public static Color zeroAlpha = new Color(0, 0, 0, 0);

		public enum UICellDataTypes
		{
			Text,
			Button,
			CheckBox,
			Dropdown,
			Image,
			InputField,
			Slider,
			HorizontalPanel,
			Spinner,
			Panel
		}

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
				// need a way to disable controls but not enable controls that should be locked?
				//foreach (var ctrl in uiControls)
				//{
				//	var b = ctrl.iUIBehavior;
				//	if (b != null)
				//	{
				//		b.interactable = value;
				//	}
				//}
			}
		}


		public UIDataCell[] cells;

		public UIDataCell this[int i]
		{
			get { return cells[i]; }
		}

		[HideInInspector]
		[SerializeField] private HorizontalLayoutGroup _layout;
		public HorizontalLayoutGroup layout
		{
			get
			{
				if (_layout == null)
					_layout = GetComponent<HorizontalLayoutGroup>();
				return _layout;
			}
		}





		public RectMask2D rectMask;
		public UIMenuDivider gridLine;

		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				for (int i = 0; i < cells.Length; ++i)
				{
					cells[i].minDimensions = new Vector2(cellWidths[i], value.y);
				}

				this.SetDirty();
			}
		}

		public Vector2 maxDimensions { get; set; }

		[HideInInspector]
		[SerializeField] private float[] _cellWidths;
		internal float[] cellWidths
		{
			get { return _cellWidths; }
			set
			{
				_cellWidths = value;
				for (int i = 0; i < cells.Length; ++i)
				{
#if UNITY_EDITOR
					if (i >= value.Length)
						Debug.Break();
#endif
					SetCellWidth(i, value[i]);
				}
			}
		}

		public void SetCellWidth(int index, float newWidth)
		{
			_cellWidths[index] = newWidth;
			cells[index].minDimensions = new Vector2(_cellWidths[index], _minDimensions.y);
			this.SetDirty();
		}



		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			RefreshControlsFormTransform_DEBUG();

			referenceName = _referenceName;
			interactable = _interactable;
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void RefreshControlsFormTransform_DEBUG()
		{
			var cellList = new List<UIDataCell>();


			foreach (var cell in GetComponentsInChildren<UIDataCell>())
			{
				cellList.Add(cell);
				if (cell.transform.GetSiblingIndex() != cellList.Count)
					Debug.LogError($"{referenceName} cells out of order!");
			}

			cells = cellList.ToArray();

			if (cells.Length != _cellWidths.Length)
			{
				Debug.LogError($"{referenceName} cells and widths out of sync!");
				cellWidths = new float[cells.Length];
			}

			minDimensions = _minDimensions;

			if (!Helpers.IsPrefabStage_EDITOR())
				CreateGridLine();
		}

		public void CreateGridLine()
		{
			gridLine = GetComponentInChildren<UIMenuDivider>();
			if (gridLine == null)
				gridLine = (UIMenuDivider)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.MenuDivider, rectMask.transform);
			gridLine.referenceName = referenceName + "_RowDivider";
			gridLine.layoutElement.enabled = true;
			gridLine.layoutElement.ignoreLayout = true;
			gridLine.rect.rotation = Quaternion.Euler(0, 0, 0);
			gridLine.rect.pivot = new Vector2(0, 0);
			gridLine.rect.anchoredPosition = new Vector2(0, 0);
		}

		public void ShowGridLine(bool show)
		{
			gridLine.gameObject.SetActive(show);
			this.SetDirty();
		}

		private void SetCellName(UIDataCell uIDataCell, int i)
		{
			uIDataCell.referenceName = $"{referenceName}_dataCell_{i.ToString("00")}";

#if UNITY_EDITOR
			if (Helpers.IsPrefabStage_EDITOR())
			{
				uIDataCell.name = uIDataCell.referenceName;
			}
#endif
		}



		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			foreach (var cell in cells)
				cell.GetControl(controlRefName);
			return null;
		}

		public UIMonoBehaviour SetControl(int cellIndex, UICellDataTypes controlType)
		{
			cells[cellIndex].SetControl(controlType);
			cells[cellIndex].control.rect.sizeDelta = new Vector2(_cellWidths[cellIndex], _minDimensions.y);
			return cells[cellIndex].control;
		}

		/// <summary>
		/// Removes and sends the control back to its pool.
		/// </summary>
		/// <param name="i"></param>
		public void RemoveControl(int cellIndex)
		{
			cells[cellIndex].RemoveControl();
		}


		/// <summary>
		/// Sets the background of the row to color. If color.a == 0, disables image.
		/// </summary>
		/// <param name="color"></param>
		public void SetBackgroundColor(Color color)
		{
			if (color.a == 0)
			{
				GetComponent<Image>().enabled = false;
			}
			else
			{
				var image = GetComponent<Image>();
				image.color = color;
				image.enabled = true;
			}
		}

		public void SetCellColors(Color color)
		{
			foreach (var ctrl in cells)
			{
				if (color.a == 0)
				{
					ctrl.GetComponent<Image>().enabled = false;
				}
				else
				{
					var image = ctrl.GetComponent<Image>();
					image.color = color;
					image.enabled = true;
				}
			}
		}

		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public void RecalculateDimensions()
		{
			float width = layout.padding.horizontal;
			for (int i = 0; i < cells.Length; ++i)
			{
				width += _cellWidths[i];
			}

			width += layout.spacing * (cells.Length - 1);

			var height = _minDimensions.y;
			for (int i = 0; i < cells.Length; ++i)
			{
				var controlHeight = cells[i].GetDrawnDimensions().y;
				height = Mathf.Max(height, controlHeight);
			}


			if (height != _minDimensions.y)
			{   // set all height of cells in row to this height for consistency
				for (int i = 0; i < cells.Length; ++i)
				{
					cells[i].rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
					cells[i].minDimensions = new Vector2(cellWidths[i], height);
					cells[i].RecalculateDimensions();
				}
			}

			if (rectMask.gameObject.activeSelf)
			{
				height = height + gridLine.rect.sizeDelta.y;
				rectMask.rectTransform.anchoredPosition = new Vector2(0, 0);
				rectMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
				rectMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
				gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
			}

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

			isDirty = false;
		}


		public Vector2 GetDrawnDimensions()
		{
			if (isDirty)
				RecalculateDimensions();
			return rect.sizeDelta;
		}

		internal void Clear()
		{
			for (int i = cells.Length - 1; i >= 0; --i)
				RemoveCell(i);
		}

		public void RemoveCell(int i)
		{
			cells[i].ReturnToPool();

			var cellList = new List<UIDataCell>(cells);
			cellList.RemoveAt(i);

			var widthList = new List<float>(cellWidths);
			widthList.RemoveAt(i);

			cells = cellList.ToArray();
			cellWidths = widthList.ToArray();
			this.SetDirty();
		}

		public void AddCell()
		{
			var newCtrls = new List<UIDataCell>(cells);
			var newWidths = new List<float>(cellWidths);

			newCtrls.Add((UIDataCell)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.DataCell, transform));
			newWidths.Add(128);

			cells = newCtrls.ToArray();
			cellWidths = newWidths.ToArray();

			this.SetDirty();
		}

		public override void ReturnToPool()
		{
			Clear();

			base.ReturnToPool();
		}



		public ScriptableObject GetBackingData()
		{
			throw new System.NotImplementedException();
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}
	}
}