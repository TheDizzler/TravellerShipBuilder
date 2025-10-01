using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using static UnityEngine.Rendering.VirtualTexturing.Debugging;

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
		[SerializeField] private RectTransform baseRect;
		[SerializeField] private RectTransform units;
		[SerializeField] private RectTransform panelRect;
		[SerializeField] private RectTransform bgRect;
		[SerializeField] private UIExpandingLabel sliderUnitPrefab;
		[SerializeField] private UIExpandingLabel minUnit;
		[SerializeField] private UIExpandingLabel maxUnit;

		[SerializeField] private List<UIExpandingLabel> unitLabels;
		private SliderEx sliderEx;

		private bool showHandle
		{
			get
			{
				if (sliderEx.useCustomShowHandle || sliderEx.scriptableObj == null)
					return sliderEx.showHandle;
				return sliderEx.scriptableObj.showHandle;
			}
		}

		public bool showUnits
		{
			get
			{
				if (sliderEx.useCustomShowUnits || sliderEx.scriptableObj == null)
					return sliderEx.showUnits;
				return sliderEx.scriptableObj.showUnits;
			}
		}

		public float unitSpan
		{
			get
			{
				if (sliderEx.useCustomUnitSpan || sliderEx.scriptableObj == null)
					return sliderEx.unitSpan;
				return sliderEx.scriptableObj.unitSpan;
			}
		}

		public LabelEx labelEx
		{
			get
			{
				if (sliderEx.scriptableObj == null)
				{
					return sliderEx.labelEx;
				}

				return sliderEx.scriptableObj.labelEx;
			}
		}



		public void UpdateSlider(SliderEx sliderEx)
		{
			this.sliderEx = sliderEx;

			handleSlideArea.gameObject.SetActive(showHandle);
			if (showHandle)
			{
				// calculate the handle overhang
				Vector2 largest = Vector2.zero;
				foreach (var child in fillBarArea.GetComponentsInChildren<RectTransform>())
				{
					if (!child.gameObject.activeSelf)
						continue;
					largest.x = Mathf.Max(child.rect.size.x, largest.x);
					largest.y = Mathf.Max(child.rect.size.y, largest.y);
				}

				Vector2 halfHandleSize = handle.rect.size / 2; // this will report the wrong height immediately after becoming active :(
				var vertOverhang = halfHandleSize.y - largest.y / 2;

				var horzOverhang = halfHandleSize.x - sliderEx.handleOffset.x;

				Vector2 handleOverhang = new Vector2(Math.Max(0, horzOverhang), Math.Max(0, vertOverhang));
				panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseRect.rect.width - handleOverhang.x * 2);
				panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, handleOverhang.y + 4);

				handlePanel.offsetMin = new Vector2(sliderEx.handleOffset.x, -sliderEx.handleOffset.y);
				handlePanel.offsetMax = new Vector2(-sliderEx.handleOffset.x, handleSlideArea.offsetMax.y);

				fillBar.offsetMin = new Vector2(sliderEx.handleOffset.x, fillBar.offsetMin.y);
				fillBar.offsetMax = new Vector2(-sliderEx.handleOffset.x, fillBar.offsetMax.y);
			}
			else
			{
				panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, baseRect.rect.width);
				panelRect.anchoredPosition = new Vector2(panelRect.anchoredPosition.x, 4);
				handlePanel.offsetMin = new Vector2(sliderEx.handleOffset.x, 0);
				handlePanel.offsetMax = new Vector2(-sliderEx.handleOffset.x, 0);
				//handlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelRect.sizeDelta.x);
				//handlePanel.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, panelRect.sizeDelta.y);
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

			if (showHandle)
			{
				var handlePos = Mathf.Lerp(0, handleSlideArea.rect.width, percent);
				handle.localPosition = new Vector2((-handleSlideArea.rect.width / 2) + handlePos, handle.localPosition.y);
			}

			if (showUnits)     // @TODO(Tristan): stop this from updating everytime anything on the slider is changed
			{                  // but also need a way to flag that text needs to be refreshed
				units.gameObject.SetActive(true);
				units.anchoredPosition = new Vector2(units.anchoredPosition.x, sliderEx.unitVerticalOffset);
				if (showHandle)
				{
					units.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 64.0f);
					units.localPosition = new Vector2(units.localPosition.x, units.localPosition.y + 4);

				}
				else
					units.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 32.0f);

				float nextUnit = sliderEx.minValue;

				var labelData = labelEx;
				var clone = (LabelEx)labelData.Clone();
				clone.text = nextUnit.ToString();
				minUnit.UpdateBackingData(clone);

				clone = (LabelEx)labelData.Clone();
				clone.text = sliderEx.maxValue.ToString();
				maxUnit.UpdateBackingData(clone);

				ClearLabels();

				float unitDiff = unitSpan;
				if (unitDiff > 0)
				{
					float range = (sliderEx.maxValue - sliderEx.minValue);
					float distDiff = Mathf.Lerp(0, units.rect.width, unitDiff / (range));
					float nextPos = 0;
					while ((nextUnit += unitDiff) < sliderEx.maxValue)
					{
						nextPos += distDiff;
						clone = (LabelEx)labelData.Clone();
						clone.text = nextUnit.ToString();

						var newLabel = Instantiate(sliderUnitPrefab, units.transform, false);
						newLabel.GetComponent<RectTransform>().anchoredPosition = new Vector2(nextPos, 0);
						newLabel.UpdateBackingData(clone);
						unitLabels.Add(newLabel);
					}
				}
			}
			else
			{
				units.gameObject.SetActive(false);
				ClearLabels();
			}

			var boundingBox = GetLargestBoundingBox();
			if (boundingBox.height < sliderEx.minDimensions.y)
				boundingBox.height = sliderEx.minDimensions.y;
			baseRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, boundingBox.height);
			//if (boundingBox.width > baseRect.rect.width) // this creates an infinite growth
			//	baseRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, boundingBox.width);
		}


		private void ClearLabels()
		{
#if DEBUG
			var labels = GetComponentsInChildren<UIExpandingLabel>();
			foreach (var label in labels)
			{
				if (label.name.Contains("Max") || label.name.Contains("Min"))
					continue;
				unitLabels.Remove(label);
				if (!Application.isPlaying)
					DestroyImmediate(label.gameObject);
				else
					Destroy(label.gameObject);
			}
#endif
			foreach (var label in unitLabels)
			{
				if (label == null)
					continue;
#if DEBUG
				if (!Application.isPlaying)
					DestroyImmediate(label.gameObject);
				else
					Destroy(label.gameObject);
#else
				Destroy(label.gameObject);
#endif
			}

			unitLabels.Clear();
		}

		private Rect GetLargestBoundingBox()
		{
			//var panelDims = new Rect(panelRect.position.x, panelRect.position.y, panelRect.rect.width, panelRect.rect.height);

			float minPosX = float.MaxValue;
			float minPosY = float.MaxValue;
			float maxHeight = 0;
			float maxWidth = 0;
			if (showUnits)
			{
				var minUnitRect = minUnit.GetComponent<RectTransform>();
				var maxUnitRect = maxUnit.GetComponent<RectTransform>();

				minPosX = Math.Min(minPosX, minUnitRect.rect.x);
				minPosY = Math.Min(minPosY, minUnitRect.position.y);

				float xDiff = Math.Abs(minUnitRect.position.x) + Math.Abs(maxUnitRect.position.x + maxUnitRect.rect.width);
				maxWidth = Math.Max(maxWidth, xDiff);
				maxHeight = Math.Max(maxHeight, units.rect.height + minUnitRect.rect.height);
			}

			if (showHandle)
			{
				maxHeight = Math.Max(maxHeight, handle.rect.height);
			}

			minPosX = Math.Min(minPosX, bgRect.position.x);
			minPosY = Math.Min(minPosY, bgRect.position.y);
			maxWidth = Math.Max(maxWidth, bgRect.rect.width);
			maxHeight = Math.Max(maxHeight, bgRect.rect.height);
			minPosX = Math.Min(minPosX, fillBar.position.x);
			minPosY = Math.Min(minPosY, fillBar.position.y);
			maxWidth = Math.Max(maxWidth, fillBar.rect.width);
			maxHeight = Math.Max(maxHeight, fillBar.rect.height);

			return new Rect(minPosX, minPosY, maxWidth, maxHeight);
		}

		public Vector2 GetMinDimensions()
		{
			return baseRect.sizeDelta;
		}
	}
}