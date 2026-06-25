using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;


namespace AtomosZ.UI
{
	public class SliderCustom : MonoBehaviour
	{
		[SerializeField] private UISlider slider;

		[SerializeField] private UIExpandingLabelScriptableObject _labelData;
		internal UIExpandingLabelScriptableObject labelData
		{
			get { return _labelData; }
			set
			{
				_labelData = value;
				minUnit.UpdateBackingData(_labelData);
				maxUnit.UpdateBackingData(_labelData);
			}
		}

		[SerializeField] private Image emptyStartCap;
		[SerializeField] private Image emptyEndCap;
		[SerializeField] private Image fillStartCap;
		[SerializeField] private Image fillEndCap;
		[SerializeField] private Image handleImage;

		[SerializeField] private RectTransform fillAreaRect;
		[SerializeField] private RectTransform fillBarAreaRect;
		[SerializeField] private RectTransform fillBarRect;

		[SerializeField] private RectTransform handle;
		[SerializeField] private RectTransform handlePanel;
		[SerializeField] private RectTransform handleSlideArea;

		[SerializeField] private RectTransform units;
		[SerializeField] private RectTransform panelRect;
		[SerializeField] private RectTransform bgRect;

		[SerializeField] private UIExpandingLabel minUnit;
		[SerializeField] private UIExpandingLabel maxUnit;

		[Tooltip("Holds created unit labels, NOT minUnit and maxUnit")]
		[SerializeField] private List<UIExpandingLabel> unitLabels;


		public Sprite handleSprite
		{
			get { return handleImage.sprite; }
			set
			{
				if (value == null)
				{
					handleSlideArea.gameObject.SetActive(false);
				}
				else
				{
					handleImage.sprite = value;
				}
			}
		}


		public void ShowHandle(bool show)
		{
			handleSlideArea.gameObject.SetActive(show);
			UpdateSlider();
		}


		public void ShowUnits(bool show)
		{
			units.gameObject.SetActive(show);
			RecalculateDimensions();
		}

		public void SetFontSize(float newFontSize)
		{
			minUnit.fontSize = newFontSize;
			maxUnit.fontSize = newFontSize;
			foreach (var unit in unitLabels)
				unit.fontSize = newFontSize;

			RecalculateDimensions();
		}

		public void SetFontColor(Color newColor)
		{
			minUnit.color = newColor;
			maxUnit.color = newColor;
			foreach (var unit in unitLabels)
				unit.color = newColor;
		}

		private float handleHorzOverhang;
		public void UpdateSlider()
		{
			handleHorzOverhang = 0;
			if (slider.showHandle)
			{
				// calculate the handle overhang
				Vector2 largest = Vector2.zero;
				foreach (var child in fillBarAreaRect.GetComponentsInChildren<RectTransform>())
				{
					if (!child.gameObject.activeSelf || child == panelRect)
						continue;
					largest.x = Mathf.Max(child.rect.size.x, largest.x);
					largest.y = Mathf.Max(child.rect.size.y, largest.y);
				}

				Vector2 halfHandleSize = handle.rect.size / 2; // this will report the wrong height immediately after becoming active :(
				var vertOverhang = halfHandleSize.y - largest.y / 2;
				handleHorzOverhang = halfHandleSize.x - slider.handleOffset.x;

				Vector2 handleOverhang = new Vector2(/*Math.Max(0, horzOverhang)*/0, Math.Max(0, vertOverhang));
				bgRect.anchoredPosition = handleOverhang;
				fillAreaRect.anchoredPosition = handleOverhang;
				handlePanel.anchoredPosition = handleOverhang;

				fillBarRect.offsetMin = new Vector2(slider.handleOffset.x, fillBarRect.offsetMin.y);
				fillBarRect.offsetMax = new Vector2(-slider.handleOffset.x, fillBarRect.offsetMax.y);
			}
			else
			{
				bgRect.anchoredPosition = Vector2.zero;
				fillAreaRect.anchoredPosition = Vector2.zero;
				handlePanel.anchoredPosition = Vector2.zero;
			}

			SetValueFill();


			if (slider.showUnits)
			{
				CreateUnitLabels();
			}
			else
			{
				units.gameObject.SetActive(false);
				ClearLabels();
			}

			RecalculateDimensions();
		}

		private float showHandleUnitOffset = 42.0f;
		private float hideHandleUnitOffset = 32.0f;

		internal void CreateUnitLabels()
		{
			units.gameObject.SetActive(true);
			units.anchoredPosition = new Vector2(units.anchoredPosition.x, slider.unitVerticalOffset);
			if (slider.showHandle)
				units.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, showHandleUnitOffset);
			else
				units.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hideHandleUnitOffset);

			float nextUnit = slider.minValue;


			minUnit.text = nextUnit.ToString();
			minUnit.UpdateBackingData(labelData);


			maxUnit.text = slider.maxValue.ToString();
			maxUnit.UpdateBackingData(labelData);

			ClearLabels();

			float unitDiff = slider.unitSpan;
			if (unitDiff > 0)
			{
				float range = (slider.maxValue - slider.minValue);
				float distDiff = Mathf.Lerp(0, units.rect.width, unitDiff / range);
				float nextPos = 0;
				int count = 0;
				while ((nextUnit += unitDiff) < slider.maxValue)
				{
					nextPos += distDiff;

					var newLabel = (UIExpandingLabel)UIPrefabProvider.GetMagicUIControl(
						UIPrefabProvider.UIPrefabType.SliderUnit, units.transform);
					newLabel.referenceName = "unit_" + (count++).ToString("00");
					//newLabel.rect.anchorMin = new Vector2(0, 1);
					//newLabel.rect.anchorMax = new Vector2(0, 1);
					//newLabel.rect.pivot = new Vector2(0.5f, 0);
					//newLabel.rect.rotation = Quaternion.identity;
					newLabel.rect.anchoredPosition = new Vector2(nextPos, 0);
					newLabel.text = nextUnit.ToString();
					newLabel.fontSize = slider.fontSize;
					newLabel.color = slider.fontColor;
					
					newLabel.UpdateBackingData(labelData);
					unitLabels.Add(newLabel);
				}
			}
		}

		internal void SetValueFill()
		{
			var percent = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
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

			fillBarRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, fillBarSize);


			if (slider.showHandle)
			{
				var handlePos = Mathf.Lerp(0, handleSlideArea.rect.width, percent);
				handle.localPosition = new Vector2((-handleSlideArea.rect.width / 2) + handlePos, handle.localPosition.y);
			}
		}


		private void RecalculateDimensions()
		{
			var sizeData = GetHeightAndLeftUnitOverhang(slider.showHandle, slider.showUnits);
			if (sizeData.y < slider.minDimensions.y)
				sizeData.y = slider.minDimensions.y;
			panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, sizeData.y);

			slider.size.x = Math.Min(sizeData.x, handleHorzOverhang);
			slider.size.y = sizeData.y;

			panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal,
				slider.GetComponent<RectTransform>().rect.width + slider.size.x);
		}


		/// <summary>
		/// x: overhang of leftmost digit.
		/// y: total height of control.
		/// </summary>
		/// <param name="showHandle"></param>
		/// <param name="showUnits"></param>
		/// <returns></returns>
		private Vector2 GetHeightAndLeftUnitOverhang(bool showHandle, bool showUnits)
		{
			//var panelDims = new Rect(panelRect.position.x, panelRect.position.y, panelRect.rect.width, panelRect.rect.height);

			//float minPosX = float.MaxValue;
			float minPosY = float.MaxValue;
			float maxPosY = float.MinValue;

			float overhang = 0;
			if (showUnits)
			{
				var minUnitRect = minUnit.GetComponent<RectTransform>();
				var maxUnitRect = maxUnit.GetComponent<RectTransform>();

				var unitsAdjust = (units.anchoredPosition.y + units.rect.height - units.rect.y);
				if (showHandle)
					unitsAdjust += showHandleUnitOffset - hideHandleUnitOffset; // why this works, I have no idea
				var handleAdjust = (handlePanel.anchoredPosition.y + handlePanel.rect.height + handlePanel.rect.y);
				var posYFromPanel = unitsAdjust + handleAdjust + minUnitRect.anchoredPosition.y + minUnitRect.rect.y;

				minPosY = Math.Min(minPosY, posYFromPanel);
				maxPosY = Math.Max(maxPosY, posYFromPanel + minUnitRect.rect.height);

				//minPosY = Math.Min(minPosY, minUnitRect.position.y + minUnitRect.rect.y);
				//maxPosY = Math.Max(maxPosY, minUnitRect.position.y + minUnitRect.rect.y + minUnitRect.rect.height);


				//var minUnitPos = minUnitRect.position.x + minUnitRect.rect.x;

				//var fillAreaLeftMost = fillAreaRect.position.x + fillAreaRect.rect.x;
				//overhang = Math.Min(0, minUnitPos - fillAreaLeftMost);
			}

			if (showHandle)
			{
				var handleAdjust = (handlePanel.anchoredPosition.y + handlePanel.rect.height + handlePanel.rect.y);
				var handleSlideAdjust = handleSlideArea.anchoredPosition.y + handleSlideArea.rect.height + handleSlideArea.rect.y;
				var posYFromPanel = handleAdjust + handleSlideAdjust + handle.anchoredPosition.y + handle.rect.y;

				//var minHandlPos = handle.position.y + handle.rect.y;
				//var maxHandlPos = handle.position.y + handle.rect.y + handle.rect.height;
				//minPosY = Math.Min(minPosY, minHandlPos);
				//maxPosY = Math.Max(maxPosY, maxHandlPos);
				minPosY = Math.Min(minPosY, posYFromPanel);
				maxPosY = Math.Max(maxPosY, posYFromPanel + handle.rect.height);
			}

			//minPosY = Math.Min(minPosY, fillAreaRect.position.y + fillAreaRect.rect.y);
			//maxPosY = Math.Max(maxPosY, fillAreaRect.position.y + fillAreaRect.rect.y + fillAreaRect.rect.height);

			var fillAreaAdjust = fillAreaRect.anchoredPosition.y + fillAreaRect.rect.y;
			var fillAreaMax = fillAreaAdjust + fillAreaRect.rect.height;
			minPosY = Math.Min(minPosY, fillAreaAdjust);
			maxPosY = Math.Max(maxPosY, fillAreaMax);

			float height = maxPosY - minPosY;

			return new Vector2(overhang, height);
			//return new Rect(minPosX, minPosY, 0, height);
		}



		private void ClearLabels()
		{
			foreach (var label in unitLabels)
			{
#if UNITY_EDITOR
				if (label == null)
				{
					Debug.LogError($"GD labels disappearing on {slider.referenceName}!");
					continue; // this shouldn't happen, right??
				}
#endif
				label.ReturnToPool();
			}

			unitLabels.Clear();
		}

		internal void ReturnToPool()
		{
			ClearLabels();
		}
	}
}