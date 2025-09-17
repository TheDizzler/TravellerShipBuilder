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
		public UIControlType dataType { get { return UIControlType.Button; } }
		public UnityEvent action = null;

		public UIButtonScriptableObject scriptableObj;

		public bool useCustomSprite = false;
		public Sprite sprite;

		public LabelEx labelEx;


		/// <summary>
		/// To change the TextMeshPro label, manipulate ButtonEx.labelEx.
		/// </summary>
		public ButtonEx(UIButtonScriptableObject scriptObj)
		{
			scriptableObj = scriptObj;
			//if (scriptableObj.labelEx == null)
			//	labelEx = new LabelEx
			//	{
			//		text = "Button Text",
			//		fontColor = Color.black,
			//	};
		}

		/// <summary>
		/// To change fontColor, manipulate ButtonEx.labelEx.fontColor.
		/// </summary>
		/// <param name="buttonText"></param>
		/// <param name="fontSize"></param>
		public ButtonEx(string buttonText, float fontSize = 36)
		{
			labelEx = new LabelEx
			{
				fontColor = Color.black,
			};

			labelEx.text = buttonText;
			labelEx.fontSize = fontSize;

			useCustomSprite = true;
		}


		public void AddListener(UnityAction newAction)
		{
			if (action == null)
				action = new UnityEngine.Events.UnityEvent();
			action.AddListener(newAction);
		}
	}

	public class UIButton : MonoBehaviour, IUIBehavior
	{
		[SerializeField] private ButtonEx buttonEx;
		[SerializeField] private UIExpandingLabel label;
		[SerializeField] private Image image;



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

		public IUIDataEx labelEx
		{
			get
			{
				if (buttonEx.scriptableObj == null)
				{
					if (buttonEx.labelEx == null)
						throw new Exception("A LabelEx is required");
					return buttonEx.labelEx;
				}

				return buttonEx.scriptableObj.labelEx;
			}
		}

		public Sprite sprite
		{
			get
			{
				if (buttonEx.useCustomSprite || buttonEx.scriptableObj == null)
				{
					return buttonEx.sprite;
				}

				return buttonEx.scriptableObj.sprite;
			}
		}

		public IUIDataEx GetBackingData()
		{
			return buttonEx;
		}

		public void UpdateBackingData(IUIDataEx backingData)
		{
			buttonEx = (ButtonEx)backingData;
			UpdateBackingData();
		}

		public void UpdateBackingData()
		{
			label.UpdateBackingData(labelEx);
			var button = GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			if (buttonEx.action != null)
			{
				button.onClick.AddListener(() => buttonEx.action.Invoke());
			}

			if (sprite != null)
				image.sprite = sprite;
		}

		/// <summary>
		/// Adds action to the listener and the backingdata.
		/// </summary>
		/// <param name="action"></param>
		public void AddListener(UnityEvent action)
		{
			buttonEx.action.AddListener(() => action.Invoke());
		}

		/// <summary>
		/// Adds action to the listener and the backingdata.
		/// </summary>
		/// <param name="action"></param>
		public void AddListener(Action action)
		{
			buttonEx.action.AddListener(() => action.Invoke());
		}

		public Vector2 GetMinDimensions()
		{
			UpdateBackingData();
			return label.GetMinDimensions();
		}


		public void Clicked(Vector3 mouseWorldPos, Keyboard.ModifierKey keyInput,
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