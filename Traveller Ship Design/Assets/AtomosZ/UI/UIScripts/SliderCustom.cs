using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	public class SliderCustom : MonoBehaviour
	{
		[SerializeField] private Image emptyStartCap;
		[SerializeField] private RectTransform emptyBar; // not used?
		[SerializeField] private Image emptyEndCap;
		[SerializeField] private Image fillStartCap;
		[SerializeField] private RectTransform fillBarArea;
		[SerializeField] private RectTransform fillBar;
		[SerializeField] private Image fillEndCap;
		[SerializeField] private RectTransform handle;
		[SerializeField] private RectTransform handlePanel;
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

			if (sliderEx.showHandle && handle != null && handle.gameObject.activeSelf)
			{
				handleSlideArea.gameObject.SetActive(true);
				// calculate the handle overhang
				Vector2 largest = Vector2.zero;
				foreach (var child in fillBarArea.GetComponentsInChildren<RectTransform>())
				{
					if (!child.gameObject.activeSelf)
						continue;
					largest.x = Mathf.Max(child.rect.size.x, largest.x);
					largest.y = Mathf.Max(child.rect.size.y, largest.y);
				}

				Vector3 halfHandleSize = handle.rect.size / 2;
				var vertOverhang = halfHandleSize.y - largest.y / 2;

				var horzOverhang = halfHandleSize.x - sliderEx.handleOffset.x;

				Vector2 handleOverhang = new Vector2(Math.Max(0, horzOverhang), Math.Max(0, vertOverhang));
				panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width - handleOverhang.x * 2);
				panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, handleOverhang.y);

				handlePanel.offsetMin = new Vector2(sliderEx.handleOffset.x, -sliderEx.handleOffset.y);
				handlePanel.offsetMax = new Vector2(-sliderEx.handleOffset.x, handleSlideArea.offsetMax.y);

				fillBar.offsetMin = new Vector2(sliderEx.handleOffset.x, fillBar.offsetMin.y);
				fillBar.offsetMax = new Vector2(-sliderEx.handleOffset.x, fillBar.offsetMax.y);
			}
			else
			{
				panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.rect.width);
				panel.anchoredPosition = new Vector2(panel.anchoredPosition.x, 0);
				handleSlideArea.gameObject.SetActive(false);
				handlePanel.offsetMin = new Vector2(sliderEx.handleOffset.x, 0);
				handlePanel.offsetMax = new Vector2(-sliderEx.handleOffset.x, 0);
				//handlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panel.sizeDelta.x);
				//handlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panel.sizeDelta.y);
			}


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
				units.anchoredPosition = new Vector2(units.anchoredPosition.x, sliderEx.unitVerticalOffset);

				float nextUnit = sliderEx.minValue;
				var clone = (LabelEx)sliderEx.labelEx.Clone();
				clone.text = nextUnit.ToString();
				minUnit.UpdateBackingData(clone);

				clone = (LabelEx)sliderEx.labelEx.Clone();
				clone.text = sliderEx.maxValue.ToString();
				maxUnit.UpdateBackingData(clone);


				ClearLabels();


				float unitDiff = sliderEx.unitSpan;
				if (unitDiff > 0)
				{
					float range = (sliderEx.maxValue - sliderEx.minValue);
					float distDiff = Mathf.Lerp(0, units.rect.width, unitDiff / (range));
					float nextPos = 0;
					while ((nextUnit += unitDiff) < sliderEx.maxValue)
					{
						nextPos += distDiff;
						clone = (LabelEx)sliderEx.labelEx.Clone();
						clone.text = nextUnit.ToString();

						var label = Instantiate(sliderUnitPrefab, units.transform, true);
						label.GetComponent<RectTransform>().anchoredPosition = new Vector2(nextPos, 0);
						label.UpdateBackingData(clone);
						unitLabels.Add(label);
					}
				}
			}
			else
			{
				units.gameObject.SetActive(false);
				ClearLabels();
			}

			var boundingBox = GetLargestBoundingBox();
			rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, boundingBox.height);
		}

		private void ClearLabels()
		{
			foreach (var label in unitLabels)
			{
#if UNITY_EDITOR
				if (Application.isEditor && !Application.isPlaying)
					DestroyImmediate(label.gameObject);
				else
					Destroy(label.gameObject);
#else
				Destroy(label.gameObject);
#endif
			}

			unitLabels.Clear();
		}

		public Rect GetLargestBoundingBox()
		{
			Vector3[] mostCorners = null;
			Vector3[] childCorners = new Vector3[4];
			foreach (var child in GetComponentsInChildren<RectTransform>())
			{
				if (!child.gameObject.activeSelf || child == rect)
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

			return new Rect(
				mostCorners[0].x,
				mostCorners[0].y,
				mostCorners[1].x - mostCorners[0].x,
				mostCorners[1].y - mostCorners[0].y);
		}

		public Vector2 GetMinDimensions()
		{
			return rect.sizeDelta;
		}
	}
}