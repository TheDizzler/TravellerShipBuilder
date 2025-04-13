using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridTile : MonoBehaviour
{
	[SerializeField] private Color baseColor, offsetColor;
	[SerializeField] private new SpriteRenderer renderer;
	[SerializeField] private GameObject highlight;

	public void Init(bool isOffset)
	{
		renderer.color = isOffset ? offsetColor : baseColor;
	}

	void OnMouseEnter()
	{
		highlight.SetActive(true);
	}

	void OnMouseExit()
	{
		highlight.SetActive(false);
	}
}
