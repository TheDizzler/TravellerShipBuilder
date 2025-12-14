using System;
using AtomosZ.DFDQ.Battle;
using AtomosZ.DFDQ.Battle.Units;
using TMPro;
using UnityEngine;
using UnityEngine.U2D.Animation;
using static AtomosZ.DFDQ.Battle.GridManager;
using static AtomosZ.ObjectForge;

namespace AtomosZ.DFDQ.Tiles
{
	public class TerrainHighlighter : MonoBehaviour, PooledObject<TerrainHighlighter>
	{
		public int poolID { get; set; }
		bool PooledObject<TerrainHighlighter>.isLive { get; set; }

		public HighlightType highlightType;
		[SerializeField] private SpriteRenderer main;
		[SerializeField] private SpriteRenderer top, left, bottom, right;
		[SerializeField] private SpriteResolver topResolver, rightResolver, bottomResolver, leftResolver;
		[SerializeField] private TextMeshPro text;

		[Serializable, Flags]
		public enum Borders
		{
			Top = 0x1,
			Left = Top << 1,
			Bottom = Left << 1,
			Right = Bottom << 1,
		}

		[SerializeField] private Borders activeBorders;
		public enum TransparentColor
		{
			Blue, Green, Red, Yellow, Grey
		}
		[SerializeField] private TransparentColor transparentColor;
		public enum BorderColor
		{
			Blue, Green, Red, Yellow
		}
		[SerializeField] private BorderColor borderColor;

		[SerializeField] private Vector2Int _position;


		public Vector2Int position
		{
			get { return _position; }
			set
			{
				_position = value;
				transform.position = (Vector2)value;
			}
		}


		public void SetBorders(Borders borders)
		{
			activeBorders = borders;
			left.gameObject.SetActive(((borders & Borders.Left) == Borders.Left));
			top.gameObject.SetActive(((borders & Borders.Top) == Borders.Top));
			right.gameObject.SetActive(((borders & Borders.Right) == Borders.Right));
			bottom.gameObject.SetActive(((borders & Borders.Bottom) == Borders.Bottom));
		}
		public void RemoveBorder(Borders side)
		{
			SetBorders(activeBorders & ~side);
		}


		public void SetTileHighlight(HighlightType highlightType, GridTile tile)
		{
			this.highlightType = highlightType;
			if (tile == null)
			{
				gameObject.SetActive(false);
				return;
			}

			gameObject.SetActive(true);
			transform.SetParent(tile.transform);
			position = tile.position;

			switch (highlightType)
			{
				case HighlightType.FactionHeroMovement:
					SetTransparentColor(TerrainHighlighter.TransparentColor.Blue);
					SetBorderColor(TerrainHighlighter.BorderColor.Blue);
					break;
				case HighlightType.FactionMonsterMovement:
					SetTransparentColor(TerrainHighlighter.TransparentColor.Red);
					SetBorderColor(TerrainHighlighter.BorderColor.Red);
					break;
				case HighlightType.AttackTemplate:
				{
					if (tile.IsOccupied())
						SetTransparentColor(TerrainHighlighter.TransparentColor.Red);
					else
						SetTransparentColor(TerrainHighlighter.TransparentColor.Yellow);
					SetBorderColor(TerrainHighlighter.BorderColor.Yellow);
				}
				break;

#if DEBUG
				case HighlightType.DEBUG_PathFrontier:
					SetTransparentColor(TransparentColor.Red);
					SetBorderColor(BorderColor.Yellow);
					break;
				case HighlightType.DEBUG_PathVisited:
					SetTransparentColor(TransparentColor.Yellow);
					SetBorderColor(BorderColor.Yellow);
					break;
				case HighlightType.DEBUG_PathShortest:
					SetTransparentColor(TransparentColor.Grey);
					SetBorderColor(BorderColor.Green);
					break;
#endif
			}
		}


		private void SetTransparentColor(TransparentColor newColor)
		{
			transparentColor = newColor;
			var resolver = GetComponent<SpriteResolver>();
			resolver.SetCategoryAndLabel("Transparents", newColor.ToString().ToLower() + "_pressed");
			resolver.ResolveSpriteToSpriteRenderer();
		}

		private void SetBorderColor(BorderColor newColor)
		{
			borderColor = newColor;
			string color = newColor.ToString().ToLower() + "_border";
			string huh = rightResolver.GetLabel();
			topResolver.SetCategoryAndLabel("TopBorders", $"{color}_top");
			bottomResolver.SetCategoryAndLabel("BottomBorders", $"{color}_bottom");
			rightResolver.SetCategoryAndLabel("RightBorders", $"{color}_right");
			leftResolver.SetCategoryAndLabel("LeftBorders", $"{color}_left");

			topResolver.ResolveSpriteToSpriteRenderer();
			bottomResolver.ResolveSpriteToSpriteRenderer();
			rightResolver.ResolveSpriteToSpriteRenderer();
			leftResolver.ResolveSpriteToSpriteRenderer();
		}

		public void SetText(string newText)
		{
			text.text = newText;
		}
	}
}