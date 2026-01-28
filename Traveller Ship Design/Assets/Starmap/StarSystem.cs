using System;
using System.Collections;
using System.Collections.Generic;
using AtomosZ.MG2eTraveller.Starmap.Tiles;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using static AtomosZ.MG2eTraveller.Starmap.SectorTilemap;
using static AtomosZ.MG2eTraveller.Starmap.Starmap;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class StarSystem : MonoBehaviour, ISelectable
	{
		public Tilemap tilemap;
		public Vector3Int cellCoordinates;
		public CircleCollider2D starCollider;
		public SystemTile systemData;

		public SubSectorMap subSector;
		public string worldName { get { return worldNameTMP.text; } }

		public InteractionState interactionState;
		public LineRenderer lineRenderer;

		[SerializeField] private SpriteRenderer solarObject;
		[SerializeField] private MeshRenderer meshRenderer;

		[SerializeField] private TextMeshPro coordsTMP;
		[SerializeField] private TextMeshPro starportTMP;
		[SerializeField] private TextMeshPro worldNameTMP;

		[SerializeField] private Color neutralBGColor = new Color(0, 0, 0, 0);


		[Serializable]
		public class FleetLog
		{
			public Fleet fleet;
			public ImperialDate dateEnteredSystem;
			public ImperialDate dateExitedSystem;
		}

		[Tooltip("The complete history of fleets that have entered and exited the system.")]
		public List<FleetLog> fleetHistoryLog = new List<FleetLog>();

		public string GetStringCoordinates()
		{
			return coordsTMP.text;
		}

		void Awake()
		{
			CreateFilledHex();
		}

		void Start()
		{
			interactionState = InteractionState.MouseOver;
			SetInteractionState(InteractionState.None);
		}

		[ContextMenu("Create Hex Mesh")]
		private void CreateFilledHex()
		{
			Vector3[] linePoints = new Vector3[lineRenderer.positionCount];
			lineRenderer.GetPositions(linePoints);

			int numTriangles = 4;
			int[] triangles = new int[numTriangles * 3];
			int i = 0;

			triangles[i++] = 0;
			triangles[i++] = 1;
			triangles[i++] = 2;

			triangles[i++] = 2;
			triangles[i++] = 3;
			triangles[i++] = 4;

			triangles[i++] = 4;
			triangles[i++] = 5;
			triangles[i++] = 0;

			triangles[i++] = 0;
			triangles[i++] = 2;
			triangles[i++] = 4;

			Mesh mesh = new Mesh();
			mesh.vertices = linePoints;
			mesh.triangles = triangles;

			GetComponent<MeshFilter>().mesh = mesh;
		}



		public void SetSystemData(Vector3Int posInSector, SystemTile systemTile, string world)
		{
			systemData = systemTile;
			solarObject.sprite = systemTile.sprite;
			transform.position = tilemap.CellToWorld(posInSector);
			cellCoordinates = posInSector;

			var coords = $"{(posInSector.y + 2).ToString("00")}{(-posInSector.x + 1).ToString("00")}";
			this.coordsTMP.text = coords;
			if (systemTile.type == SystemType.Empty)
			{
				name = $"({coords}) void";
				worldNameTMP.text = "Empty Space";

				worldNameTMP.gameObject.SetActive(false);
				starportTMP.gameObject.SetActive(false);
				this.coordsTMP.gameObject.SetActive(false);
			}
			else
			{
				name = $"({coords}) {world}";
				starportTMP.gameObject.SetActive(false);
				worldNameTMP.text = world;
			}
		}

		private IEnumerator HighlightUpdate(Color color)
		{
			meshRenderer.material.SetColor("_Color", color);

			while ((highlightTimer -= Time.deltaTime) > 0)
			{
				yield return null;
				var nextColor = Color.Lerp(neutralBGColor, color, highlightTimer / highlightFadeOutTime);
				meshRenderer.material.SetColor("_Color", nextColor);
			}

			meshRenderer.material.SetColor("_Color", neutralBGColor);

			highlightCoroutine = null;
		}

		private const float highlightFadeOutTime = .35f;
		private float highlightTimer = 0;
		private Coroutine highlightCoroutine = null;
		public void SetBackground(Color color)
		{
			if (Application.isPlaying)
			{
				highlightTimer = highlightFadeOutTime;
				if (highlightCoroutine == null)
					highlightCoroutine = StartCoroutine(HighlightUpdate(color));
			}
		}


		public void SetInteractionState(InteractionState newState, bool forcedStateChange = false)
		{
			if (forcedStateChange)
				interactionState = newState;
			else if (!this.CheckState(newState, ref interactionState))
				return;
			var highlightData = Starmap.instance.systemHighlightData[interactionState];

			lineRenderer.startWidth = highlightData.thickness;
			lineRenderer.endWidth = highlightData.thickness;

			if (Application.isPlaying)
			{
				lineRenderer.material.SetColor("_Color", highlightData.color);
				lineRenderer.material.SetFloat("_Pulse_Speed", highlightData.pulseSpeed);

				worldNameTMP.fontMaterial.SetInt(ShaderUtilities.Keyword_Glow, highlightData.textGlowPower >= 1 ? 0 : 1);
				worldNameTMP.fontMaterial.SetFloat(ShaderUtilities.ID_GlowPower, highlightData.textGlowPower);
				worldNameTMP.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, highlightData.color);
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
				worldNameTMP.fontSharedMaterial.SetInt(ShaderUtilities.Keyword_Glow, highlightData.textGlowPower >= 1 ? 0 : 1);
				worldNameTMP.fontSharedMaterial.SetFloat(ShaderUtilities.ID_GlowPower, highlightData.textGlowPower);
				worldNameTMP.fontSharedMaterial.SetColor(ShaderUtilities.ID_GlowColor, highlightData.color);
				//worldName.UpdateMeshPadding();
			}

			var pos = transform.localPosition;
			pos.Set(pos.x, pos.y, highlightData.zPopOut);
			transform.localPosition = pos;
		}


		void OnTriggerEnter2D(Collider2D other)
		{
			Debug.Log(other.gameObject.name + " has been detected entering " + name + " system");
			var fleet = other.GetComponent<Fleet>();
			Starmap.instance.FleetEnteredSystem(this, fleet);
			fleet.UpdatePosition(this, true);

			fleetHistoryLog.Add(new FleetLog
			{
				fleet = fleet,
				dateEnteredSystem = Starmap.instance.currentDate.LogDate(),
				dateExitedSystem = null,
			});
		}
	}
}