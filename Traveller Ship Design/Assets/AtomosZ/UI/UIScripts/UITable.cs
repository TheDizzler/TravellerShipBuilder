using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;
using static UnityEngine.UI.GridLayoutGroup;

namespace AtomosZ.UI
{
	[ExecuteAlways]
	public class UITable : UIMonoBehaviour, IUIBehavior
	{
		public UIControlType dataType { get { return UIControlType.Table; } }

		public bool interactable
		{
			get { return _interactable; }
			set
			{
				_interactable = value;
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

		public UIDataRow headerRow;

		[SerializeField] private UIMenuDivider headerLine;
		[SerializeField] private UIDataRow[] rows;

		[SerializeField] private Vector2 _layoutSpacing;
		public Vector2 layoutSpacing
		{
			get
			{
				return _layoutSpacing;
			}
			set
			{
				_layoutSpacing = value;
				layout.spacing = value.y;
				headerRow.layout.spacing = value.x;
				foreach (var row in rows)
				{
					row.layout.spacing = value.x;
				}

				this.SetDirty();
			}
		}

		[SerializeField] private float[] _columnWidths;

		public float[] columnWidths
		{
			get { return _columnWidths; }
			set
			{
				_columnWidths = value;
				if (headerRow != null)
				{
					headerRow.cellWidths = value;
				}

				for (int row = 0; row < rowCount; ++row)
				{
					rows[row].cellWidths = value;
				}


				this.SetDirty();
			}
		}



		[SerializeField] private float _rowHeight;
		public float rowHeight
		{
			get { return _rowHeight; }
			set
			{
				_rowHeight = value;
				for (int i = 0; i < rowCount; ++i)
					rows[i].cellHeight = value;
			}
		}

		[SerializeField] private float _headerHeight;
		public float headerHeight
		{
			get
			{
				if (headerRow != null)
					_headerHeight = headerRow.cellHeight;
				return _headerHeight;
			}
			set
			{
				if (headerRow == null)
					return;
				_headerHeight = headerRow.cellHeight = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Color[] _rowColors;
		public Color[] rowColors
		{
			get { return _rowColors; }
			set
			{
				_rowColors = value;
				this.SetDirty();
			}
		}

		//[Min(1)]
		[SerializeField] private int _rowCount;
		public int rowCount
		{
			get { return _rowCount = rows.Length; }
			set
			{
				if (rows.Length == value)
					return;
				_rowCount = value;

				var newRows = new UIDataRow[value];
				for (int i = 0; i < value; ++i)
				{
					if (i < rows.Length)
						newRows[i] = rows[i];
					else
						newRows[i] = NewRow(i);
					RenameRow(newRows[i], i);
					newRows[i].columnCount = columnCount;
				}

				for (int i = value; i < rows.Length; ++i)
				{
					rows[i].ReturnToPool();
				}

				rows = newRows;
				this.SetDirty();
			}
		}

		[Min(1)]
		[SerializeField] private int _columnCount;
		public int columnCount
		{
			get
			{
				if (headerRow != null)
					_columnCount = headerRow.columnCount;
				return _columnCount;
			}
			set
			{
				if (_columnCount == value)
					return;

				if (headerRow != null)
					headerRow.columnCount = value;
				_columnCount = value;

				var newColumnWidths = new float[value];


				for (int i = 0; i < _columnCount; ++i)
				{
					if (_columnWidths.Length > i)
						newColumnWidths[i] = _columnWidths[i];
					else
						newColumnWidths[i] = 128;
					SetHeader(i);
				}

				if (value == 0)
				{
					for (int i = 0; i < columnDividers.Length; ++i)
					{
						if (columnDividers[i] != null)
							columnDividers[i].ReturnToPool();
					}

					columnDividers = new UIMenuDivider[0];
				}
				else
				{
					var newColumnDividers = new UIMenuDivider[value + 1];

					if (columnDividers.Length > 0 && columnDividers[0] != null)
						newColumnDividers[0] = columnDividers[0];
					else
					{
						newColumnDividers[0] = (UIMenuDivider)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.VerticalDivider, transform);
						newColumnDividers[0].rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 4);
						newColumnDividers[0].rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, headerHeight + 4);
					}

					for (int i = 0; i < _columnCount; ++i)
					{
						if (columnDividers.Length > i + 1 && columnDividers[i + 1] != null)
							newColumnDividers[i + 1] = columnDividers[i + 1];
						else
						{
							newColumnDividers[i + 1] = (UIMenuDivider)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.VerticalDivider, transform);
							//newColumnDividers[i + 1].transform.rotation = Quaternion.Euler(0, 0, 90);
							//newColumnDividers[i + 1].rect.anchorMin = new Vector2(0, 1);
							//newColumnDividers[i + 1].rect.anchorMax = new Vector2(0, 1);
							//newColumnDividers[i + 1].rect.pivot = new Vector2(0, .5f);
							newColumnDividers[i + 1].rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 4);
							newColumnDividers[i + 1].rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, headerHeight + 4);
						}
					}

					for (int i = newColumnDividers.Length; i < columnDividers.Length; ++i)
					{
						if (columnDividers[i] != null)
							columnDividers[i].ReturnToPool();
					}

					columnDividers = newColumnDividers;
				}

				for (int i = 0; i < rows.Length; ++i)
				{
					rows[i].columnCount = value;
				}

				
				columnWidths = newColumnWidths;

				this.SetDirty();
			}
		}

		/// <summary>
		/// There are columnCount +1 dividers in a table.
		/// </summary>
		[SerializeField]
		private UIMenuDivider[]
		columnDividers;

		/// <summary>
		/// Called when waking up/constructed.
		/// </summary>
		public void Init(int startingColCount, int startingRowCount)
		{
			if (headerRow == null)
				ConstructHeaderRow();
			headerRow.Init(startingColCount);
			columnWidths = new float[startingColCount];
			for (int i = 0; i < startingColCount; ++i)
			{
				columnWidths[i] = 128;
			}

			_columnCount = -1;
			columnCount = startingColCount;
			_rowCount = -1;
			rowCount = startingRowCount;
			rowHeight = 64;
			headerHeight = 64;
			layoutSpacing = new Vector2(32, 0);
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			//if (headerRow == null)
			//	ConstructHeaderRow();
			referenceName = _referenceName;
			interactable = _interactable;

			RefreshControlsFormTransform_DEBUG();

			columnWidths = _columnWidths;

			var r = _rowCount;
			_rowCount = 0;

			var c = _columnCount;
			_columnCount = 0;
			columnCount = c;

			rowCount = r;

			layoutSpacing = _layoutSpacing;
			headerHeight = _headerHeight;
			rowColors = _rowColors;

			this.SetDirty();
		}

		public void ConstructHeaderRow()
		{
#if UNITY_EDITOR
			var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();

#endif
			if (headerRow != null)
			{
#if UNITY_EDITOR
				if (stage != null && stage.assetPath.Contains("UITable.prefab"))
				{ // this is prefab stage and you cannot destroy objects on the prefab stage apparently

				}
				else
				{
					headerRow.ReturnToPool();
				}
#else
				headerRow.ClearAndReturnToPool();
#endif
				headerRow = null;
			}

			headerRow = (UIDataRow)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.DataRow, transform);
			headerRow.referenceName = "HeaderRow";

#if UNITY_EDITOR
			if (stage != null)
				headerRow.name = headerRow.referenceName;
#endif

			headerRow.transform.SetAsFirstSibling();
			headerRow.columnCount = _columnCount;
			headerRow.layout.spacing = layoutSpacing.x;
			headerRow.cellWidths = _columnWidths;
			headerRow.cellHeight = _headerHeight;
			for (int i = 0; i < _columnCount; ++i)
			{
				SetHeader(i);
			}

			if (headerLine == null)
			{
				headerLine = (UIMenuDivider)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.MenuDivider, transform);
			}
		}

		private void SetHeader(int i)
		{
			UIExpandingLabel headerLabel;
			if (headerRow[i].control == null)
				headerLabel = (UIExpandingLabel)headerRow.SetControl(i, UIDataRow.UICellDataTypes.Text);
			else
				headerLabel = (UIExpandingLabel)headerRow[i].control;
			headerLabel.text = headerLabel.referenceName = "Header " + i;
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage())
				headerLabel.name = headerLabel.referenceName;
#endif
			headerLabel.color = Color.black;
			headerLabel.fontStyles = TMPro.FontStyles.Bold;
			headerLabel.alignmentOptions = TMPro.TextAlignmentOptions.Bottom;
			headerLabel.fitToParent = true;
			headerLabel.enabled = true;
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public new void RecordPrefabInstances()
		{
			PrefabUtility.RecordPrefabInstancePropertyModifications(this);
			PrefabUtility.RecordPrefabInstancePropertyModifications(headerRow);
			foreach (var row in rows)
				PrefabUtility.RecordPrefabInstancePropertyModifications(row);
		}

		void Awake()
		{
			if (transform.parent != null)
				this.SetDirty();

			RefreshControlsFormTransform_DEBUG();
		}

		public void Clear()
		{
			if (headerRow != null)
			{
				headerRow.ReturnToPool();
				headerRow = null;
			}

			headerLine = null;

			for (int i = 0; i < rows.Length; ++i)
			{
				if (rows[i] == null)
					continue;
				rows[i].ReturnToPool();
			}


			_rowCount = -1;
			rowCount = 0;
			_columnCount = -1;
			columnCount = 0;
		}

		public override void ReturnToPool()
		{
			Clear();

			base.ReturnToPool();
		}


		[Conditional("DEBUG")]
		public void RefreshControlsFormTransform_DEBUG()
		{
			var foundRows = new List<UIDataRow>();

			foreach (UIDataRow child in transform.GetComponentsInChildren<UIDataRow>())
			{
				if (child == headerRow)
					continue;
				foundRows.Add(child);
			}

			rows = foundRows.ToArray();
			if (rows.Length > 0)
				_columnWidths = rows[0].cellWidths;
			else if (headerRow != null)
				_columnWidths = headerRow.cellWidths;
			else
			{
				_columnWidths = new float[_columnCount];
				for (int i = 0; i < _columnCount; ++i)
					_columnWidths[i] = 128;
			}

			if (headerRow == null)
			{
				foreach (var row in GetComponentsInChildren<UIDataRow>())
				{
					if (row.name == "HeaderRow")
					{
						headerRow = row;
						break;
					}
				}
			}

			if (headerRow == null)
				ConstructHeaderRow();
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;

			foreach (var row in rows)
				return row.GetControl(controlRefName);

			return headerRow.GetControl(controlRefName);
		}


		private UIDataRow NewRow(int index)
		{
			var newRow = (UIDataRow)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.DataRow, transform);
			newRow.columnCount = _columnCount;
			newRow.cellWidths = _columnWidths;
			newRow.cellHeight = _rowHeight;
			newRow.layout.spacing = layoutSpacing.x;
			RenameRow(newRow, index);
			return newRow;
		}

		private void RenameRow(UIDataRow newRow, int index)
		{
			newRow.referenceName = $"{referenceName}_row_{index.ToString("00")}";
#if UNITY_EDITOR
			if (Helpers.IsPrefabStage())
				newRow.name = newRow.referenceName;
#endif
		}

		public UIDataRow AddRow()
		{
			var newRows = new List<UIDataRow>(rows);
			var newRow = NewRow(rows.Length);
			newRows.Add(newRow);
			rows = newRows.ToArray();

			this.SetDirty();
			return newRow;
		}


		public ScriptableObject GetBackingData()
		{
			return null;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}


		void Update()
		{
			if (isDirty)
				UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			//#if UNITY_EDITOR
			//			RefreshControlsFormTransform_DEBUG();
			//#endif

			float width = layout.padding.horizontal;
			for (int i = 0; i < columnCount; ++i)
			{
				width += _columnWidths[i];
			}

			width += _layoutSpacing.x * (columnCount - 1);

			float height = layout.padding.vertical;
			height += headerHeight;
			height += rowCount * rowHeight + Mathf.Max(0, rowCount - 1) * layoutSpacing.y;


			//var headerRect = headerRow.rect;
			//float headerHeight = headerRow.grid.cellSize.y;
			//headerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, headerHeight);
			//headerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

			headerLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);


			float x = 0;
			var divider = columnDividers[0];
			divider.rect.localPosition = new Vector3(0, 0, 0);
			divider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);

			for (int i = 0; i < columnCount - 1; ++i)
			{
				divider = columnDividers[i + 1];
				x += _columnWidths[i] + layoutSpacing.x * .5f;
				divider.rect.localPosition = new Vector3(x, 0, 0);
				divider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
			}

			divider = columnDividers[columnDividers.Length - 1];
			x += _columnWidths[columnCount - 1];
			divider.rect.localPosition = new Vector3(x, 0, 0);
			divider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);


			if (_rowColors.Length == 0)
			{
				var zeroAlpha = new Color(0, 0, 0, 0);
				for (int row = 0; row < rowCount; ++row)
				{
					rows[row].UpdateBackingData();
					rows[row].SetBackgroundColor(UIDataRow.zeroAlpha);
				}
			}
			else
			{
				int colorIndex = 0;
				for (int row = 0; row < rowCount; ++row)
				{
					if (colorIndex >= _rowColors.Length)
						colorIndex = 0;
					rows[row].UpdateBackingData();
					rows[row].SetBackgroundColor(_rowColors[colorIndex++]);
				}
			}


			isDirty = false;
		}

		public Vector2 GetMinDimensions()
		{
			if (isDirty)
				UpdateBackingData();
			return GetComponent<RectTransform>().sizeDelta;
		}

	}
}
