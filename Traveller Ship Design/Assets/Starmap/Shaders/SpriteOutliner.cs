using System;
using AtomosZ.ShaderTools;
using UnityEngine;

namespace AtomosZ.MG2eTraveller.Starmap
{
	[RequireComponent(typeof(SpriteRenderer))]
	public class SpriteOutliner : MonoBehaviour
	{
		[SerializeField] private Texture2D outlineTexture;
		[Range(1, 10)]
		[SerializeField] private int outlineThickness = 1;

		[SerializeField] private SpriteRenderer outlineRenderer;


		void Start()
		{
			outlineRenderer.sprite = SpriteOutlineCreator.CreateSpriteOutline(GetComponent<SpriteRenderer>().sprite, outlineThickness);
			SetPulseColor(new Color(0, 0, 0, 0), 0);
		}

		/// <summary>
		/// If alpha == 0, turns the outline renderer off.
		/// </summary>
		/// <param name="pulseColor">If alpha == 0, turns the outline renderer off.</param>
		/// <param name="pulseSpeed"></param>
		public void SetPulseColor(Color pulseColor, float pulseSpeed)
		{
			if (pulseColor.a == 0)
			{
				outlineRenderer.enabled = false;
				return;
			}

			outlineRenderer.enabled = true;

			if (Application.isPlaying)
			{
				outlineRenderer.material.SetColor("_Pulse_Color", pulseColor);
				outlineRenderer.material.SetFloat("_Pulse_Speed", pulseSpeed);
			}
			else
			{
				outlineRenderer.sharedMaterial.SetColor("_Pulse_Color", pulseColor);
				outlineRenderer.sharedMaterial.SetFloat("_Pulse_Speed", pulseSpeed);
			}
		}
	}
}