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
	[SerializeField] private RectTransform fillBarArea;
	[SerializeField] private RectTransform fillBar;
	[SerializeField] private Image fillEndCap;
	[SerializeField] private RectTransform handle;
	[SerializeField] private RectTransform handleSlideArea;
	[SerializeField] private RectTransform rect;
	[SerializeField] private RectTransform units;
	[SerializeField] private RectTransform panel;
	[SerializeField] private UIExpandingLabel sliderUnitPrefab;
	[SerializeField] private UIExpandingLabel minUnit;
	[SerializeField] private UIExpandingLabel maxUnit;

	[SerializeField] private List<UIExpandingLabel> unitLabels;
	private SliderEx sliderEx;

	public void UpdateSlider(SliderEx sliderEx)
	{
		this.sliderEx = sliderEx;

		if (handle != null && handle.gameObject.activeSelf)
		{   // calculate the handle overhang
			Vector2 handleOverhang = Vector2.zero;

			Vector3 handleSize = handle.rect.size;
			var horzOverhang = handleSize.x / 2 - sliderEx.handleOffset.x;
			handleOverhang.x = Math.Max(0, horzOverhang);
			panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width - handleOverhang.x * 2);
		}


		handleSlideArea.offsetMin = new Vector2(sliderEx.handleOffset.x, handleSlideArea.offsetMin.y);
		handleSlideArea.offsetMax = new Vector2(-sliderEx.handleOffset.x, handleSlideArea.offsetMax.y);
		fillBar.offsetMin = new Vector2(sliderEx.handleOffset.x, fillBar.offsetMin.y);
		fillBar.offsetMax = new Vector2(-sliderEx.handleOffset.x, fillBar.offsetMax.y);

		var percent = Mathf.InverseLerp(sliderEx.minValue, sliderEx.maxValue, sliderEx.value);
		var fillBarSize = Mathf.Lerp(0, handleSlideArea.rect.width, percent);

		if (percent != 1)
		{
			emptyStartCap.enabled = false;
			emptyEndCap.enabled = true;
			fillStartCap.enabled = true;
			fillEndCap.enabled = false;
		}
		else
		{
			emptyStartCap.enabled = false;
			emptyEndCap.enabled = false;
			fillStartCap.enabled = true;
			fillEndCap.enabled = true;
		}

		fillBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillBarSize);

		var handlePos = Mathf.Lerp(0, handleSlideArea.rect.width, percent);
		handle.localPosition = new Vector2((-handleSlideArea.rect.width / 2) + handlePos, handle.localPosition.y);

		if (sliderEx.showUnits)     // TODO(Tristan): stop this from updating everytime anything on the slider is changed
		{                           // but also need a way to flag that text needs to be refreshed
			units.gameObject.SetActive(true);

			if (sliderEx.unitCount == unitLabels.Count + 2)
				return;
			foreach (var label in unitLabels)
			{
#if UNITY_EDITOR
				DestroyImmediate(label.gameObject);
#else
				Destroy(label.gameObject);
#endif
			}

			unitLabels.Clear();


			float nextUnit = sliderEx.minValue;
			var clone = (LabelEx)sliderEx.labelEx.Clone();
			clone.text = nextUnit.ToString();
			minUnit.UpdateBackingData(clone);

			clone = (LabelEx)sliderEx.labelEx.Clone();
			clone.text = sliderEx.maxValue.ToString();
			maxUnit.UpdateBackingData(clone);

			if (sliderEx.unitCount == 2)
				return;



			float unitDiff = 0;
			float distDiff = 0;
			if (sliderEx.wholeNumbers)
			{
				int range = (int)sliderEx.maxValue - (int)sliderEx.minValue;
				if (sliderEx.unitCount > range)
					sliderEx.unitCount = range;
				while (range % sliderEx.unitCount != 0)
				{
					if (--sliderEx.unitCount == 2)
					{
						return;
					}
				}

				unitDiff = Mathf.Lerp(0, range, 1.0f / (sliderEx.unitCount));
				distDiff =  Mathf.Lerp(0, handleSlideArea.rect.width, unitDiff/range);
			}
			//else
			//{
			//	int unitRem = ((int)(sliderEx.maxValue - sliderEx.minValue) + 1) % (sliderEx.unitCount);
			//	while (unitRem != 0)
			//	{
			//		if (--sliderEx.unitCount <= 2)
			//			return;

			//		unitRem = ((int)(sliderEx.maxValue - sliderEx.minValue) + 1) % (sliderEx.unitCount);
			//	}

			//	unitDiff = ((int)(sliderEx.maxValue - sliderEx.minValue) + 1) / (sliderEx.unitCount);
			//}
			Debug.Log(unitDiff);
			var totalLength = handleSlideArea.rect.width;
			float nextPos = 0;

			for (int i = 1; i < sliderEx.unitCount; ++i)
			{
				nextUnit += unitDiff;
				nextPos += distDiff;
				clone = (LabelEx)sliderEx.labelEx.Clone();
				clone.text = nextUnit.ToString();

				var label = Instantiate(sliderUnitPrefab, units.transform, true);
				label.GetComponent<RectTransform>().anchoredPosition = new Vector2(nextPos, 0);
				label.UpdateBackingData(clone);
				unitLabels.Add(label);

			}
		}
		else
			units.gameObject.SetActive(false);
	}

	public Vector2 GetMinDimensions()
	{
		// calculate amount that handle overhangs the rest of the UI
		bool handleWasOn = false;
		if (handle != null && (handleWasOn = handle.gameObject.activeSelf))
		{
			handle.gameObject.SetActive(false);
		}


		Vector3[] mostCorners = null;
		Vector3[] childCorners = new Vector3[4];
		foreach (var child in GetComponentsInChildren<RectTransform>())
		{
			if (!child.gameObject.activeSelf)
				continue;
			child.GetWorldCorners(childCorners);

			var bottomLeft = childCorners[0];
			var topRight = childCorners[2];

			if (mostCorners == null)
			{
				mostCorners = new Vector3[2];
				mostCorners[0] = new Vector3(bottomLeft.x, bottomLeft.y);
				mostCorners[1] = new Vector3(topRight.x, topRight.y);
				continue;
			}

			mostCorners[0].x = Math.Min(bottomLeft.x, mostCorners[0].x);
			mostCorners[0].y = Math.Min(bottomLeft.y, mostCorners[0].y);

			mostCorners[1].x = Math.Max(topRight.x, mostCorners[1].x);
			mostCorners[1].y = Math.Max(topRight.y, mostCorners[1].y);
		}

		Vector2 dims = new Vector3(mostCorners[1].x - mostCorners[0].x, mostCorners[1].y - mostCorners[0].y);

		if (handleWasOn)
		{   // calculate the handle overhang
			handle.gameObject.SetActive(true);

			Vector2 handleOverhang = Vector2.zero;

			Vector3 handleSize = handle.rect.size;
			var horzOverhang = handleSize.x / 2 - sliderEx.handleOffset.x;
			handleOverhang.x = Math.Max(0, horzOverhang);

			dims += handleOverhang;
		}


		Debug.Log($"{mostCorners[0]}    {mostCorners[1]}");
		Debug.Log(dims);


		return dims;
	}
}
