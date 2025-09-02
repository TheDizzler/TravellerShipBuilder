using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class PointIndicator : MonoBehaviour
{
	private MeshRenderer meshRenderer;

	void Start()
	{
		meshRenderer = GetComponent<MeshRenderer>();
		meshRenderer.enabled = false;
	}

	public void UpdatePosition(Vector3 pos)
	{
		transform.position = pos;
	}

	public void ToggleIndicator(bool enable)
	{
		meshRenderer.enabled = enable;
	}
}
