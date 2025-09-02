using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class ButtonEx : IUIDataEx
	{
		public PanelControlType dataType { get { return PanelControlType.Button; } }
		public UnityEvent action = null;

		public LabelEx labelEx = new LabelEx
		{
			text = "Button Text",
			fontColor = Color.black,
		};


		public void ResetToDefaults()
		{
			labelEx.ResetToDefaults();
			labelEx.text = "Button Text";
			labelEx.fontColor = Color.black;
			action = null;
		}

		public object Clone()
		{
			var clone = (ButtonEx)this.MemberwiseClone();
			clone.labelEx = (LabelEx)labelEx.Clone();
			return clone;
		}
	}

	public class UIButton : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private ButtonEx buttonEx;
		[SerializeField] private UIExpandingLabel label;



		public UIDesignObject _designObject;
		public UIDesignObject designObject
		{
			get
			{
				if (_designObject == null)
					_designObject = GetComponent<UIDesignObject>();
				return _designObject;
			}
		}

		public IUIDataEx GetBackingData()
		{
			// if I'm not mistaken, we should not need to get the labelEx since
			// it SHOULD be the same reference.
			//buttonEx.labelEx = (LabelEx)label.GetBackingData();
			return buttonEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			buttonEx = (ButtonEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			label.UpdateBackingData(buttonEx.labelEx);
			var button = GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			if (buttonEx.action != null)
			{
				button.onClick.AddListener(() => buttonEx.action.Invoke());
			}
		}


		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			return label.GetMinDimensions();
		}


		public void Clicked(Vector3 mouseWorldPos, DesignManager.KeyInput keyInput,
			ref UIDesignObject currentlySelectedObject)
		{
			throw new NotImplementedException();
		}

		public void Deselect()
		{
			throw new NotImplementedException();
		}

		public void ResetToLastPosition()
		{
			throw new NotImplementedException();
		}

		public UIDesignObject Select()
		{
			throw new NotImplementedException();
		}

		public void SetHover(bool isHover)
		{
		}

		public void UpdateHover(Vector3 posOfHover)
		{
		}
	}
}