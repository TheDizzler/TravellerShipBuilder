using System.Collections.Generic;

using TMPro;

using UnityEngine;

namespace AtomosZ.UI
{
	public class UIPrefabProvider : MonoBehaviour
	{
		public static UIPrefabProvider instance
		{
			get
			{
				if (_instance == null)
					_instance = GameObject.FindAnyObjectByType<UIPrefabProvider>();
				return _instance;
			}
		}

		private static UIPrefabProvider _instance;

		public enum UIPrefabType
		{
			DynamicPanel,
			Button,
			MenuControlButton,
			MenuDivider,
			ExpandingText,
			InputField,
			ButtonPanel,
			CheckBox,
			Slider,
			ModalClickBlocker,
			ImageView,
			ImageViewPanel,
			Dropdown,
			TabControl,
			Spinner,
			/// <summary>
			/// this is not a base UI prefab.
			/// </summary>
			//GeomorphDisplayPanel,
		}

		[SerializeField] public UIExpandingLabelScriptableObject textScriptObj;
		[SerializeField] public UIExpandingLabelScriptableObject dropdownScriptObj;
		[SerializeField] public UICheckBoxScriptableObject checkBoxScriptObj;
		[SerializeField] public UIExpandingInputFieldScriptableObject inputFieldScriptObj;
		[SerializeField] public UISliderScriptableObject sliderScriptObj;
		[SerializeField] public UIButtonScriptableObject buttonScriptObj;
		[SerializeField] public UIButtonPanelScriptableObject buttonPanelScriptObj;
		[SerializeField] public UIImageViewScriptableObject imageViewScriptObj;
		[SerializeField] public UIImageViewPanelScriptableObject imageViewPanelScriptObj;

		[SerializeField] public UITabControlScriptableObject tabbedWindowScriptObj;
		[SerializeField] public UITabControlScriptableObject titleBarWindowScriptObj;
		[SerializeField] public UITabControlScriptableObject contextMenuWindowScriptObj;


		[Tooltip("This is populated by an Editor script. Editing manually is futile.")]
		[UDictionary.Split(50, 50)]
		public UDictionary<UIPrefabType, UIDesignObject> uiPrefabs;

		[SerializeField] private TMP_FontAsset defaultFont;
		[SerializeField] private Canvas uiCanvas;
		

		public static UIDesignObject GetPrefab(UIPrefabType prefabType)
		{
			return instance.uiPrefabs[prefabType];
		}

		public static UIDesignObject GetUIPrefab(UIPrefabType prefabType)
		{
			return instance.uiPrefabs[prefabType];
		}

		/// <summary>
		/// Is this necessary? Nullifying the fontasset has the same effect.
		/// </summary>
		/// <returns></returns>
		public static TMP_FontAsset GetDefaultFont()
		{
			return instance.defaultFont;
		}

//		public static DynamicPanel GetDynamicPanel()
//		{
//			return instance._GetDynamicPanel();
//		}

//		private DynamicPanel _GetDynamicPanel()
//		{
//			var panelUIObject = Instantiate(GetUIPrefab(UIPrefabType.DynamicPanel));
//			var panelRect = panelUIObject.GetComponent<DynamicPanel>();
//			//AddToCanvas(panelUIObject.transform);
//			return panelRect;
//		}


//		private void AddToCanvas(Transform transform)
//		{
//			transform.SetParent(uiCanvas.transform, false);
//		}


//		public static void ShowDialog(DynamicPanel dialog)
//		{
//			instance._ShowDialog(dialog);
//		}


//		private void _ShowDialog(DynamicPanel dialog)
//		{
//			if (dialog.designObject.isModal)
//			{
//				var blocker = Instantiate(GetUIPrefab(UIPrefabType.ModalClickBlocker));
//				AddToCanvas(blocker.transform);
//				dialog.modalClickBlocker = blocker;
//			}

//			AddToCanvas(dialog.transform);
//			dialogStack.AddLast(dialog);
//			ToggleUIMode(true, CursorSpriteMode.UI_Default);
//		}


//		public static void CloseDialog(DynamicPanel panelRect)
//		{
//			instance._CloseDialog(panelRect);
//		}

//		/// <summary>
//		/// Currently destroys all dialogs that get closed.
//		/// Change this too object pool?
//		/// </summary>
//		/// <param name="dialogRect"></param>
//		private void _CloseDialog(DynamicPanel panelRect)
//		{
//			if (!dialogStack.Contains(panelRect))
//			{
//				Debug.LogError("Panel isn't in stack???");
//			}

//			dialogStack.Remove(panelRect);
//			if (panelRect.modalClickBlocker != null)
//			{
//				Destroy(panelRect.modalClickBlocker.gameObject);
//			}

//			Destroy(panelRect.gameObject);
//			// turn off UI mode, so next update will check for another panelRect in the stack.
//			// this will prevent any wierd click throughs
//			ToggleUIMode(false, CursorSpriteMode.Default);
//		}

//		public static void ShowErrorDialog(string errorMsg, string titleText = null)
//		{
//			Debug.LogError(titleText + "\n" + errorMsg);

//			var panelRect = GetDynamicPanel();
//			panelRect.showCloseButton = true;
//			panelRect.SetTitle(titleText, DynamicPanel.TitleLabelStyle.Bar);
//			panelRect.AddText_NoData(errorMsg);
//			panelRect.Show(Vector2.zero);
//		}



//#if UNITY_EDITOR
//		public void Test()
//		{
//			MakeModalPanel();
//		}

//		int panelCount = 1;
//		public void MakeModalPanel()
//		{
//			var panelRect = GetDynamicPanel();
//			panelRect.designObject.isModal = true;
//			panelRect.AddText(new LabelEx("Panel " + panelCount++));
//			var button = new ButtonEx
//			{
//				labelEx = new LabelEx("Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeModalPanel);
//			panelRect.AddButton(button);
//			button = new ButtonEx
//			{
//				labelEx = new LabelEx("Non Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeNonModalPanel);
//			panelRect.AddButton(button);
//			panelRect.SetTitle("Modal test", DynamicPanel.TitleLabelStyle.BladedBar);
//			panelRect.Show(Vector2.zero);
//		}

//		public void MakeNonModalPanel()
//		{
//			var panelRect = GetDynamicPanel();
//			panelRect.AddText(new LabelEx("Panel " + panelCount++));
//			var button = new ButtonEx
//			{
//				labelEx = new LabelEx("Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeModalPanel);
//			panelRect.AddButton(button);
//			button = new ButtonEx
//			{
//				labelEx = new LabelEx("Non Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeNonModalPanel);
//			panelRect.AddButton(button);
//			panelRect.SetTitle("Non Modal test", DynamicPanel.TitleLabelStyle.BladedBar);
//			panelRect.Show(Vector2.zero);
//		}
//#endif
	}
}