using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static AtomosZ.UI.MagicWindow;


namespace AtomosZ.UI
{
	[ExecuteInEditMode]
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

		[SerializeField] private RectOffset _borderMargins;
		public RectOffset borderMargins
		{
			get { return _borderMargins; }
			set
			{
				_borderMargins = value;
				layout.padding = _borderMargins;
				columnDividerMask.padding = new Vector4(borderMargins.left, borderMargins.bottom, borderMargins.right, borderMargins.top);
				this.SetDirty();
			}
		}

		[Min(0)]
		[SerializeField] private int _gridThickness;
		public int gridThickness
		{
			get { return _gridThickness; }
			set
			{
				if (_gridThickness == value)
					return;
				_gridThickness = value;
				if (headerRow != null)
				{
					headerRow.gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
					headerRow.SetDirty();
				}
				foreach (var row in rows)
				{
					row.gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
					row.SetDirty();
				}
				foreach (var border in columnDividers)
					border.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value);
			}
		}

		[HideInInspector]
		[SerializeField] public UIDataRow[] rows;

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
				if (headerRow != null)
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

				for (int row = 0; row < rows.Length; ++row)
				{
					rows[row].cellWidths = value;
				}


				this.SetDirty();
			}
		}

		[Min(1)]
		[SerializeField] private float _rowMinHeight;
		public float rowMinHeight
		{
			get { return _rowMinHeight; }
			set
			{
				if (_rowMinHeight == value)
					return;
				_rowMinHeight = value;
				for (int i = 0; i < rows.Length; ++i)
					rows[i].minDimensions = new Vector2(-1, value);
			}
		}

		[Min(0)]
		[SerializeField] private float _headerHeight;
		public float headerHeight
		{
			get
			{
				if (headerRow != null)
					_headerHeight = headerRow.minDimensions.y;
				return _headerHeight;
			}
			set
			{
				if (_headerHeight == value)
					return;
				if (headerRow == null)
					return;
				headerRow.minDimensions = new Vector2(headerRow.minDimensions.x, value);
				_headerHeight = value;
				this.SetDirty();
			}
		}

		[SerializeField] private Color[] _rowColors;
		public Color[] rowColors
		{
			get { return _rowColors; }
			set
			{
				if (_rowColors == value)
					return;
				_rowColors = value;

				if (_rowColors.Length == 0)
				{
					var zeroAlpha = new Color(0, 0, 0, 0);
					for (int row = 0; row < rows.Length; ++row)
					{
						rows[row].SetBackgroundColor(UIDataRow.zeroAlpha);
					}
				}
				else
				{
					int colorIndex = 0;
					for (int row = 0; row < rows.Length; ++row)
					{
						if (colorIndex >= _rowColors.Length)
							colorIndex = 0;
						rows[row].SetBackgroundColor(_rowColors[colorIndex++]);
					}
				}
			}
		}


		[HideInInspector]
		[SerializeField] private int columnCount;


		[SerializeField] private Vector2 _minDimensions;
		public Vector2 minDimensions
		{
			get { return _minDimensions; }
			set
			{
				_minDimensions = value;
				this.SetDirty();
			}
		}

		public Vector2 maxDimensions { get; set; }

		/// <summary>
		/// There are columnCount -1 dividers in a table.
		/// </summary>
		[HideInInspector]
		[SerializeField] private UIMenuDivider[] columnDividers;
		//[HideInInspector]
		//[SerializeField] private UIMenuDivider[] rowDividers;
		[SerializeField] private RectMask2D columnDividerMask;

		/// <summary>
		/// Called when waking up/constructed.
		/// </summary>
		public void Init(int startingColCount, int startingRowCount)
		{
			if (headerRow == null)
				CreateHeaderRow();

			rowMinHeight = 64;
			headerHeight = 64;
			layoutSpacing = new Vector2(32, 0);

			for (int i = 0; i < startingColCount; ++i)
			{
				AddColumn();
			}

			for (int i = 0; i < startingRowCount; ++i)
				AddRow();
		}

		[System.Diagnostics.Conditional("UNITY_EDITOR")]
		public void UpdateBackingData_EDITOR()
		{
			referenceName = _referenceName;
			interactable = _interactable;

			RefreshControlsFormTransform_DEBUG();

			columnWidths = _columnWidths;

			layoutSpacing = _layoutSpacing;

			if (_headerHeight != headerHeight)
			{
				var hh = _headerHeight;
				_headerHeight = -1;
				headerHeight = hh;
			}

			rowColors = _rowColors;

			borderMargins = _borderMargins;

			var gt = _gridThickness;
			_gridThickness = -1;
			gridThickness = gt;

			//if (_rowMinHeight != rowMinHeight)
			{
				var rmh = _rowMinHeight;
				_rowMinHeight = -1;
				rowMinHeight = rmh;
			}
		}

		public void CreateHeaderRow()
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
			headerRow.gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gridThickness);
			headerRow.columnCount = columnCount;
			headerRow.layout.spacing = layoutSpacing.x;
			headerRow.cellWidths = _columnWidths;
			headerRow.minDimensions = new Vector2(0, _headerHeight);
			for (int i = 0; i < columnCount; ++i)
			{
				SetHeader(i);
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
			headerLabel.fillParentHorizontal = true;
			headerLabel.fillParentVertical = true;
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

			for (int i = 0; i < rows.Length; ++i)
			{
				if (rows[i] == null)
					continue;
				rows[i].ReturnToPool();
			}

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

			if (headerRow != null)
				columnCount = headerRow.columnCount;
			else if (rows.Length > 0)
				columnCount = rows[0].columnCount;
			else
				columnCount = 0;

			if (_columnWidths == null || _columnWidths.Length != columnCount)
			{
				var columnWidthList = new float[columnCount];
				for (int i = 0; i < columnCount; ++i)
				{
					if (i < _columnWidths.Length)
						columnWidthList[i] = _columnWidths[i];
					else
						columnWidthList[i] = 128;
				}

				_columnWidths = columnWidthList;
			}

			var foundVerticalDividers = new List<UIMenuDivider>();
			foreach (UIMenuDivider child in columnDividerMask.GetComponentsInChildren<UIMenuDivider>())
			{
				if (foundVerticalDividers.Count < columnCount - 1)
					foundVerticalDividers.Add(child);
				else
					child.ReturnToPool();
			}

			columnDividers = foundVerticalDividers.ToArray();
		}


		public UIMonoBehaviour GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;

			foreach (var row in rows)
				return row.GetControl(controlRefName);

			return headerRow.GetControl(controlRefName);
		}

		public void AddColumn()
		{
			++columnCount;
			if (columnCount >= 2)
			{
				var dividerList = new List<UIMenuDivider>(columnDividers);
				var newDivider = (UIMenuDivider)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.MenuDivider, transform);
				newDivider.layout.enabled = false;
				newDivider.rect.rotation = Quaternion.Euler(0, 0, 270);
				newDivider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gridThickness);
				newDivider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, headerHeight + 4);
				dividerList.Add(newDivider);
				columnDividers = dividerList.ToArray();
			}

			var columnWidthList = new List<float>(_columnWidths);
			columnWidthList.Add(128);
			_columnWidths = columnWidthList.ToArray();

			if (headerRow != null)
			{
				headerRow.columnCount = columnCount;
				headerRow.cellWidths = _columnWidths;
				SetHeader(columnCount - 1);
			}

			foreach (var row in rows)
			{
				row.columnCount = columnCount;
				row.cellWidths = _columnWidths;
			}

			this.SetDirty();
		}


		public UIDataRow AddRow()
		{
			rows[rows.Length - 1].ShowGridLine(true);
			rows[rows.Length - 1].gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gridThickness);
			var newRows = new List<UIDataRow>(rows);
			var newRow = NewRow(rows.Length);
			newRows.Add(newRow);
			rows = newRows.ToArray();

			this.SetDirty();
			return newRow;
		}

		private UIDataRow NewRow(int index)
		{
			var newRow = (UIDataRow)UIPrefabProvider.GetMagicUIControl(UIPrefabProvider.UIPrefabType.DataRow, transform);
			newRow.columnCount = columnCount;
			newRow.cellWidths = _columnWidths;
			newRow.minDimensions = new Vector2(0, _rowMinHeight);
			newRow.layout.spacing = layoutSpacing.x;
			newRow.CreateGridLine();
			newRow.gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gridThickness);
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

		public void RemoveRow(int rowIndex)
		{
			var rowList = new UIDataRow[rows.Length - 1];
			for (int i = 0; i < rowIndex; ++i)
				rowList[i] = rows[i];
			rows[rowIndex].ReturnToPool();
			for (int i = rowIndex + 1; i < rows.Length; ++i)
				rowList[i - 1] = rows[i];
			rows = rowList;

			this.SetDirty();
		}


		void Update()
		{
			if (isDirty)
				RecalculateDimensions();
		}

		public void RecalculateDimensions()
		{
			float width = 0;
			for (int i = 0; i < columnCount; ++i)
			{
				width += _columnWidths[i];
			}

			width += _layoutSpacing.x * Mathf.Max(0, (columnCount - 1));

			float height = layout.padding.vertical;
			if (headerRow != null)
			{
				height += headerRow.GetDrawnDimensions().y;
				headerRow.gridLine.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, gridThickness);
			}

			for (int i = 0; i < rows.Length; ++i)
			{
				if (i == rows.Length - 1)
					rows[i].ShowGridLine(false);
				height += rows[i].GetDrawnDimensions().y;
			}

			height += Mathf.Max(0, rows.Length - 1) * (layoutSpacing.y/* + gridThickness*/);


			float dividerX = borderMargins.left;
			for (int i = 0; i < columnDividers.Length; ++i)
			{
				var columnWidth = columnWidths[i];
				var columnDivider = columnDividers[i];

				dividerX += columnWidth + (layoutSpacing.x) * .5f;
				columnDivider.rect.anchoredPosition = new Vector3(dividerX, 0, 0);
				columnDivider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, height);
				columnDivider.rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _gridThickness);
				dividerX += (layoutSpacing.x) * .5f;
			}

			columnDividerMask.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);

			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Mathf.Max(minDimensions.y, height));
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, Mathf.Max(minDimensions.x, width + layout.padding.right));

			isDirty = false;
		}

		public Vector2 GetDrawnDimensions()
		{
			if (isDirty)
				RecalculateDimensions();
			return GetComponent<RectTransform>().sizeDelta;
		}



		public ScriptableObject GetBackingData()
		{
			return null;
		}

		public void UpdateBackingData(ScriptableObject backingData)
		{
			throw new System.NotImplementedException();
		}

	}
}
