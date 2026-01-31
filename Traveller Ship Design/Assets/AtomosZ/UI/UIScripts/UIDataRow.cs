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
						newCtrls[i].control.rect.sizeDelta = new Vector2(newWidths[i], _cellHeight);
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

		[Min(1)]
		[SerializeField] private float _cellHeight;
		public float cellHeight
		{
			get { return _cellHeight; }
			set
			{
				_cellHeight = value;
				for (int i = 0; i < columnCount; ++i)
				{
					cells[i].rect.sizeDelta = new Vector2(_cellWidths[i], _cellHeight);
					if (cells[i].control != null)
						cells[i].control.rect.sizeDelta = new Vector2(_cellWidths[i], _cellHeight);
				}

				this.SetDirty();
			}
		}

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
			//for (int i = 0; i < columnCount; ++i)
			{
				cells[index].rect.sizeDelta = new Vector2(_cellWidths[index], _cellHeight);
				if (cells[index].control != null)
					cells[index].control.rect.sizeDelta = new Vector2(_cellWidths[index], _cellHeight);
			}

			this.SetDirty();
		}

		public void Init(int startingColCount)
		{
			columnCount = startingColCount;
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
			cellHeight = _cellHeight;
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

		public UIMonoBehaviour SetControl(int i, UICellDataTypes controlType)
		{
			//if (i >= cells.Length)
			//	columnCount = i;
			cells[i].SetControl(controlType);
			cells[i].control.rect.sizeDelta = new Vector2(_cellWidths[i], _cellHeight);
			return cells[i].control;
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
				UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			float width = layout.padding.horizontal;
			for (int i = 0; i < columnCount; ++i)
			{
				width += _cellWidths[i];
			}

			width += layout.spacing * (columnCount - 1);

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, cellHeight);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

			isDirty = false;
		}


		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
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