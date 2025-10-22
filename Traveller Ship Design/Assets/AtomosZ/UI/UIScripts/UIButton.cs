using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace AtomosZ.UI
{
	[Serializable]
	public class ButtonEx : IUIDataEx
	{
		public UIControlType dataType { get { return UIControlType.Button; } }
		public string referenceName = "button";

		public UnityEvent action = null;
		public bool fillParentHorizontal = false;

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
			if (scriptableObj == null || scriptableObj.labelEx == null)
			{
				labelEx = new LabelEx()
				{
					fontColor = Color.black,
					fontSize = 36,
				};
			}
		}

		/// <summary>
		/// To change fontColor, manipulate ButtonEx.labelEx.fontColor.
		/// </summary>
		/// <param name="buttonText"></param>
		/// <param name="fontSize"></param>
		public ButtonEx(float fontSize = 36)
		{
			labelEx = new LabelEx()
			{
				fontColor = Color.black,
				fontSize = fontSize,
			};

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
		[SerializeField] public UIExpandingLabel label;
		[SerializeField] private Image image;

		public string referenceName { get { return buttonEx.referenceName; } set { buttonEx.referenceName = value; } }

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

		public IUIBehavior GetControl(string controlRefName)
		{
			if (referenceName == controlRefName)
				return this;
			return label.GetControl(controlRefName);
		}

		public LabelEx labelEx
		{
			get
			{
				if (buttonEx.scriptableObj == null)
				{
					return buttonEx.labelEx;
				}

				return buttonEx.scriptableObj.labelEx;
			}
		}

		[SerializeField] private string _text = "Button Text";
		[Tooltip("NOTE(Tristan): textmeshpro adds a mystery whitespace to the end of EVERY string, even if it's \"empty\", so the length will NEVER equal zero!")]
		public string text
		{
			get
			{
				_text = label.text;
				return _text;
			}
			set
			{
				label.text = _text = value;
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
			this.SetNameToReferenceName(gameObject);

			if (sprite != null)
				image.sprite = sprite;

			var layout = GetComponent<LayoutElement>();
			if (buttonEx.fillParentHorizontal)
				layout.flexibleWidth = 1;
			else
				layout.flexibleWidth = 0;

			TextMeshProUGUI textLabel = label.GetComponent<TextMeshProUGUI>();
			var labelHorzMargins = +textLabel.margin.x + textLabel.margin.z;
			layout.minWidth = labelEx.minLabelDimensions.x + labelHorzMargins;
			label.UpdateBackingData(labelEx);
			var labelDim = label.GetMinDimensions();
			layout.preferredWidth = labelDim.x + labelHorzMargins;


			var button = GetComponent<Button>();
			button.onClick.RemoveAllListeners();
			if (buttonEx.action != null)
			{
				button.onClick.AddListener(() => buttonEx.action.Invoke());
			}
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