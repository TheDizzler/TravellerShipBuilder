using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderCustom : MonoBehaviour
{
	[SerializeField] private Image emptyStartCap;
	[SerializeField] private RectTransform emptyBar;
	[SerializeField] private Image emptyEndCap;
	[SerializeField] private Image fillStartCap;
	[SerializeField] private RectTransform fillBar;
	[SerializeField] private Image fillEndCap;
	[SerializeField] private RectTransform handle;
	[SerializeField] private RectTransform handleSlideArea;
	[SerializeField] private RectTransform rect;

	public void UpdateSlider(SliderEx sliderEx)
	{
		var percent = Mathf.InverseLerp(sliderEx.minValue, sliderEx.maxValue, sliderEx.value);
		var barSize = Mathf.Lerp(0, rect.sizeDelta.x, percent);

		if (percent == 0)
		{
			emptyStartCap.enabled = true;
			emptyEndCap.enabled = true;
			fillStartCap.enabled = false;
			fillEndCap.enabled = false;
		}
		else if (percent == 1)
		{
			emptyStartCap.enabled = false;
			emptyEndCap.enabled = false;
			fillStartCap.enabled = true;
			fillEndCap.enabled = true;
		}
		else
		{
			emptyStartCap.enabled = false;
			emptyEndCap.enabled = true;
			fillStartCap.enabled = true;
			fillEndCap.enabled = false;
		}

		fillBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, barSize);

		var handlePos = Mathf.Lerp(0, handleSlideArea.rect.width, percent);
		handle.localPosition = new Vector2((-handleSlideArea.rect.width / 2) + handlePos, handle.localPosition.y);
	}

	public Vector2 GetMinDimensions()
	{
		Vector2 minDim = Vector2.zero;
		foreach (var child in GetComponentsInChildren<RectTransform>())
		{
			if (child.rect.size.x > minDim.x)
				minDim.x = child.rect.size.x;
			
			if (child.rect.size.y > minDim.y)
				minDim.y = child.rect.size.y;
		}

		return minDim;
	}
}
