using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Abandoned
/// </summary>
public class GridGenerator : MonoBehaviour
{
	[SerializeField] private int width, height;
	[SerializeField] private float gridLineWidth;
	[SerializeField] private LineRenderer linePrefab;
	[SerializeField] private TextMeshPro textPrefab;
	[SerializeField] private Camera snapShotCamera;
	[SerializeField] private SpriteRenderer backgroundSprite;

	bool wait = true;
	private GameObject lineCanvas;
	private GameObject debugText;

	void Start()
	{
		GenerateGrid();
	}

	void GenerateGrid()
	{

		 debugText = new GameObject("Debug text");
		for (int x = 0; x < 16; ++x)
		{
			for (int y = 0; y < 10; ++y)
			{
				var tmp = Instantiate(textPrefab, debugText.transform);
				tmp.transform.position = new Vector3(x - (16 / 2), (10 / 2) - y, 0);
				tmp.text = $"{x},{y}";
			}
		}

		lineCanvas = new GameObject("line canvas");
		lineCanvas.transform.SetParent(this.transform);

		//width = 2;
		//height = 2;
		//for (int x = 0; x < width + 1; ++x)
		//{
		//	var line = Instantiate(linePrefab, lineCanvas.transform);
		//	line.startWidth = gridLineWidth;
		//	line.SetPosition(0, new Vector3(x - (width / 2), -height / 2, 0));
		//	line.SetPosition(1, new Vector3(x - (width / 2), height / 2, 0));
		//}

		//for (int y = 0; y < height + 1; ++y)
		//{
		//	var line = Instantiate(linePrefab, lineCanvas.transform);
		//	line.startWidth = gridLineWidth;
		//	line.SetPosition(0, new Vector3(-width / 2, y - (height / 2), 0));
		//	line.SetPosition(1, new Vector3(width / 2, y - (height / 2), 0));
		//}

		for (int x = 0; x < width - 1; ++x)
		{
			var line = Instantiate(linePrefab, lineCanvas.transform);
			line.startWidth = gridLineWidth;
			line.SetPosition(0, new Vector3(x - (width / 2), -height / 2, 0));
			line.SetPosition(1, new Vector3(x - (width / 2), height / 2, 0));
		}

		for (int y = 0; y < height + 1; ++y)
		{
			var line = Instantiate(linePrefab, lineCanvas.transform);
			line.startWidth = gridLineWidth;
			line.SetPosition(0, new Vector3(-width / 2, y - (height / 2), 0));
			line.SetPosition(1, new Vector3(width / 2, y - (height / 2), 0));
		}
	}


	void Update()
	{
		if (wait)
		{
			wait = false;
			return;
		}

		//if (!Input.GetKeyDown(KeyCode.Space))
		//	return;


		//lineCanvas.SetActive(true);


		var pixelsPerUnit = Screen.height / 10;
		//snapShotCamera.targetTexture.width = 2 * pixelsPerUnit;
		//snapShotCamera.targetTexture.height = 2 * pixelsPerUnit;
		var newTexture = new RenderTexture(2 * pixelsPerUnit, 2 * pixelsPerUnit, 32);
		newTexture.wrapMode = TextureWrapMode.Clamp;
		newTexture.filterMode = FilterMode.Bilinear;

		snapShotCamera.targetTexture = newTexture;

		var gridTex = ToTexture2D(newTexture);
		var gridByte = gridTex.EncodeToPNG();
		File.WriteAllBytes(@"E:\FirefoxDownloads\temp.png", gridByte);


		backgroundSprite.sprite = Sprite.Create(gridTex, new Rect(0, 0, gridTex.width, gridTex.height), new Vector2(.5f, .5f), pixelsPerUnit, 0, SpriteMeshType.FullRect);
		//lineCanvas.SetActive(false);
		enabled = false;
		snapShotCamera.targetTexture = null;

		debugText.gameObject.SetActive(false);
	}

	public Texture2D ToTexture2D(RenderTexture newTexture)
	{
		snapShotCamera.enabled = true;
		RenderTexture currentActiveRT = RenderTexture.active;
		RenderTexture.active = newTexture;

		snapShotCamera.Render();

		Texture2D tex = new Texture2D(newTexture.width,
			newTexture.height, TextureFormat.RGB24, false);
		tex.ReadPixels(new Rect(0, 0, newTexture.width, newTexture.height), 0, 0);
		tex.Apply();

		RenderTexture.active = currentActiveRT;
		snapShotCamera.enabled = false;
		return tex;
	}

}
