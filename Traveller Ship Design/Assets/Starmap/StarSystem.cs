using System;
using AtomosZ.MG2eTraveller.Starmap.Tiles;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class StarSystem : MonoBehaviour
	{
		public Tilemap tilemap;
		public Vector3Int tilePos;
		public CircleCollider2D starCollider;
		public SystemTile systemData;
		public TextMeshPro coords;
		public TextMeshPro starport;
		public TextMeshPro worldName;
		public SubSectorMap subSector;
		public SystemHighlightState highlightState;
		public LineRenderer lineRenderer;


		public enum SystemHighlightState
		{
			None,
			MouseOver,
			Selected,
			SelectedMouseOver,
		}


		void Start()
		{
			highlightState = SystemHighlightState.SelectedMouseOver;
			SetHighlight(SystemHighlightState.None);
		}

		public void SetSystemData(Vector3Int pos, SystemTile systemTile, string world)
		{
			systemData = systemTile;
			GetComponent<SpriteRenderer>().sprite = systemTile.sprite;
			transform.localPosition = tilemap.CellToWorld(pos);
			tilePos = pos;

			var coords = $"{(pos.y + 2).ToString("00")}{(-pos.x + 1).ToString("00")}";
			if (systemTile.type == SubSectorMap.SystemType.Empty)
			{
				worldName.gameObject.SetActive(false);
				starport.gameObject.SetActive(false);
				this.coords.gameObject.SetActive(false);
				name = $"({coords}) void";
			}
			else
			{
				name = $"({coords}) {systemTile.type}";
				this.coords.text = coords;
				starport.gameObject.SetActive(false);
				worldName.text = world;
			}
		}

		[System.Diagnostics.Conditional("DEBUG")]
		public void SetHighlightTest()
		{
			if ((highlightState += 1) > SystemHighlightState.Selected)
				highlightState = SystemHighlightState.None;
			SetHighlight(highlightState);

		}

		public void SetHighlight(SystemHighlightState status)
		{
			if (status == highlightState)
				return;
			highlightState = status;
			var highlightData = subSector.highlightData[status];
			if (Application.isPlaying)
			{
				lineRenderer.material.SetColor("_Color", highlightData.color);
				lineRenderer.material.SetFloat("_Pulse_Speed", highlightData.pulseSpeed);

				worldName.fontMaterial.SetInt(ShaderUtilities.Keyword_Glow, highlightData.textGlowPower >= 1 ? 0 : 1);
				worldName.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, highlightData.textGlowPower);
				worldName.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, highlightData.color);
			}
			else
			{
				lineRenderer.sharedMaterial.SetColor("_Color", highlightData.color);
				lineRenderer.sharedMaterial.SetFloat("_Pulse_Speed", highlightData.pulseSpeed);

				/*	public static int ID_GlowColor;
					public static int ID_GlowOffset;
					public static int ID_GlowPower;
					public static int ID_GlowOuter;
					public static int ID_GlowInner; */
				worldName.fontSharedMaterial.SetInt(ShaderUtilities.Keyword_Glow, highlightData.textGlowPower >= 1 ? 0 : 1);
				worldName.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, highlightData.textGlowPower);
				worldName.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, highlightData.color);
				//worldName.UpdateMeshPadding();
			}

			var pos = transform.localPosition;
			pos.Set(pos.x, pos.y, highlightData.zPopOut);
			transform.localPosition = pos;
		}
	}
}