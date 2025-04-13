using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridScroller : MonoBehaviour
{
	public Transform camTransform;
	private SpriteRenderer sprite;
	public float snapValue;

	void Start()
	{
		sprite = GetComponent<SpriteRenderer>();
	}


	void Update()
	{
		transform.position = new Vector2(
			Mathf.Round(camTransform.position.x / snapValue) * snapValue,
			Mathf.Round(camTransform.position.y / snapValue) * snapValue);

		var width = Mathf.CeilToInt(-camTransform.position.z * 3.6f);
		if (width % 2 != 0)
			++width;

		sprite.size = new Vector2(width, width);
	}
}
