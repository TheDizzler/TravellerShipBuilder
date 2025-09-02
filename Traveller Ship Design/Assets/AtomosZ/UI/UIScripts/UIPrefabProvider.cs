using System.Collections.Generic;

using TMPro;

using UnityEngine;

using static CustomCursor;

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
			/// <summary>
			/// this is not a base UI prefab.
			/// </summary>
			//GeomorphDisplayPanel,
		}

		[UDictionary.Split(50, 50)]
		public UDictionary<UIPrefabType, UIDesignObject> uiPrefabs;

		[SerializeField] private TMP_FontAsset defaultFont;
		[SerializeField] private Canvas uiCanvas;
		/// <summary>
		/// serialized for debugging
		/// </summary>
		[SerializeField]
		private LinkedList<DynamicPanel> dialogStack = new();


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
//			var panel = panelUIObject.GetComponent<DynamicPanel>();
//			//AddToCanvas(panelUIObject.transform);
//			return panel;
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


//		public static void CloseDialog(DynamicPanel panel)
//		{
//			instance._CloseDialog(panel);
//		}

//		/// <summary>
//		/// Currently destroys all dialogs that get closed.
//		/// Change this too object pool?
//		/// </summary>
//		/// <param name="dialogRect"></param>
//		private void _CloseDialog(DynamicPanel panel)
//		{
//			if (!dialogStack.Contains(panel))
//			{
//				Debug.LogError("Panel isn't in stack???");
//			}

//			dialogStack.Remove(panel);
//			if (panel.modalClickBlocker != null)
//			{
//				Destroy(panel.modalClickBlocker.gameObject);
//			}

//			Destroy(panel.gameObject);
//			// turn off UI mode, so next update will check for another panel in the stack.
//			// this will prevent any wierd click throughs
//			ToggleUIMode(false, CursorSpriteMode.Default);
//		}

//		public static void ShowErrorDialog(string errorMsg, string titleText = null)
//		{
//			Debug.LogError(titleText + "\n" + errorMsg);

//			var panel = GetDynamicPanel();
//			panel.showCloseButton = true;
//			panel.SetTitle(titleText, DynamicPanel.TitleLabelStyle.Bar);
//			panel.AddText_NoData(errorMsg);
//			panel.Show(Vector2.zero);
//		}



//#if UNITY_EDITOR
//		public void Test()
//		{
//			MakeModalPanel();
//		}

//		int panelCount = 1;
//		public void MakeModalPanel()
//		{
//			var panel = GetDynamicPanel();
//			panel.designObject.isModal = true;
//			panel.AddText(new LabelEx("Panel " + panelCount++));
//			var button = new ButtonEx
//			{
//				labelEx = new LabelEx("Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeModalPanel);
//			panel.AddButton(button);
//			button = new ButtonEx
//			{
//				labelEx = new LabelEx("Non Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeNonModalPanel);
//			panel.AddButton(button);
//			panel.SetTitle("Modal test", DynamicPanel.TitleLabelStyle.BladedBar);
//			panel.Show(Vector2.zero);
//		}

//		public void MakeNonModalPanel()
//		{
//			var panel = GetDynamicPanel();
//			panel.AddText(new LabelEx("Panel " + panelCount++));
//			var button = new ButtonEx
//			{
//				labelEx = new LabelEx("Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeModalPanel);
//			panel.AddButton(button);
//			button = new ButtonEx
//			{
//				labelEx = new LabelEx("Non Modal Panel"),
//				action = new UnityEngine.Events.UnityEvent(),
//			};
//			button.action.AddListener(MakeNonModalPanel);
//			panel.AddButton(button);
//			panel.SetTitle("Non Modal test", DynamicPanel.TitleLabelStyle.BladedBar);
//			panel.Show(Vector2.zero);
//		}
//#endif
	}
}