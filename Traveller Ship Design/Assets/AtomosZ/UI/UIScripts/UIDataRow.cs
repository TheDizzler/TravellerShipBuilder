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


		[HideInInspector] public UIDataCell[] cells;

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

		[HideInInspector]
		[SerializeField] private int _columnCount = 1;
		public int columnCount
		{
			get { return _columnCount; }
			set
			{
				_columnCount = value;
				var newCtrls = new UIDataCell[value];
				var newWidths = new float[value];

				for (int i = 0; i < value; ++i)
				{
					if (i < cells.Length)
					{
						newCtrls[i] = cells[i];
					}

					if (newCtrls[i] == null)
					{
						newCtrls[i] = (UIDataCell)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.DataCell, transform);
					}

					if (cellWidths.Length > i)
						newWidths[i] = cellWidths[i];
					else
						newWidths[i] = 32;

					if (newCtrls[i].control != null)
						newCtrls[i].control.rect.sizeDelta = new Vector2(newWidths[i], _minDimensions.y);
					SetCellName(newCtrls[i], i);
					newCtrls[i].gameObject.SetActive(true);
				}

				for (int i = value; i < cells.Length; ++i)
				{
					if (cells[i] != null)
						cells[i].ReturnToPool();
				}

				_cellWidths = newWidths;
				cells = newCtrls;
				this.SetDirty();
			}
		}

		public UIMenuDivider gridLine;

		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				for (int i = 0; i < columnCount; ++i)
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
				for (int i = 0; i < columnCount; ++i)
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
			cells = new UIDataCell[columnCount];


			foreach (var cell in GetComponentsInChildren<UIDataCell>())
			{
				if (cell.transform.GetSiblingIndex() >= cells.Length)
				{
					cell.ReturnToPool();
					continue;
				}

				cells[cell.transform.GetSiblingIndex()] = cell;
			}

			var newWidths = new List<float>(_cellWidths);
			//float newHeight = Mathf.Max(16, _cellHeight);
			for (int i = 0; i < columnCount; ++i)
			{
				if (cells[i] == null)
				{
					cells[i] = (UIDataCell)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.DataCell, transform);
					SetCellName(cells[i], i);
					cells[i].gameObject.SetActive(true);
				}

				if (newWidths.Count <= i)
					//newWidths.Add(Mathf.Max(Mathf.Max(32, cellWidths[i]), cells[i].rect.sizeDelta.x));
					newWidths.Add(Mathf.Max(32, cellWidths[i]));
				else
					//newWidths[i] = Mathf.Max(Mathf.Max(32, cellWidths[i]), cells[i].rect.sizeDelta.x);
					newWidths[i] = Mathf.Max(32, cellWidths[i]);
				//newHeight = Mathf.Max(newHeight, cells[i].rect.sizeDelta.y);
			}

			cellWidths = newWidths.ToArray();
			minDimensions = _minDimensions;

			CreateGridLine();
		}

		public void CreateGridLine()
		{
			gridLine = GetComponentInChildren<UIMenuDivider>();
			if (gridLine == null)
				gridLine = (UIMenuDivider)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.MenuDivider, transform);
			gridLine.referenceName = referenceName + "_Row Divider";
			gridLine.layout.enabled = true;
			gridLine.layout.ignoreLayout = true;
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
			if (Helpers.IsPrefabStage())
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
			for (int i = 0; i < columnCount; ++i)
			{
				width += _cellWidths[i];
			}

			width += layout.spacing * (columnCount - 1);

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

			if (gridLine.gameObject.activeSelf)
			{
				height = height + gridLine.rect.sizeDelta.y;
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
			for (int i = 0; i < cells.Length; ++i)
			{
#if DEBUG
				if (cells[i] == null)
				{
					Log.Warning("I really wish stuff wouldn't just disappear like this");
					continue;
				}
#endif

				cells[i].ReturnToPool();
				cells[i] = null;
			}

			_columnCount = -1;
			columnCount = 0;
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