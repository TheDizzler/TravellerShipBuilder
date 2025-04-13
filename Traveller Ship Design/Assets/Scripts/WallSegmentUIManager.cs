using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abandonded
/// </summary>
public class WallSegmentUIManager : MonoBehaviour
{
	[SerializeField] private GameObject wallSegmentEndPointA;
	[SerializeField] private GameObject wallSegmentEndPointB;
	private Wall wallSegment;

	public void DisplayWallEditUI(Wall wallSegmentToEdit)
	{
		wallSegment = wallSegmentToEdit;
		//wallSegmentEndPointA.transform.localPosition = wallSegmentToEdit.transform.

		this.gameObject.SetActive(true);
	}

}
