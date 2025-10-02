using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using static AtomosZ.UI.DynamicPanel;

namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(DynaPanelOp))]
	public class DynaPanelOpEditor : Editor
	{
		private DynaPanelOp dynaPanOp;

		private RectTransform rect;
		private Vector2 lastSize;
		private SerializedProperty currentType;

		private SerializedProperty uiControls;
		private Dictionary<UIControlType, SerializedProperty> scriptableObjects;


		private DynamicPanel dynaPan;
		private SerializedObject dynaPanSO;
		private SerializedProperty titleStyle;
		private SerializedProperty titleText;
		private SerializedProperty centerTitleText;
		private SerializedProperty panelStyle;
		private SerializedProperty tabs;
		private SerializedProperty selectedTabIndex;
		private SerializedProperty minDims;
		private SerializedProperty maxDims;
		private SerializedProperty alwaysShrink;
		private SerializedProperty showCloseButton;
		private SerializedProperty showMinimizeButton;
		private SerializedProperty showMaximizeButton;
		private bool isVisible;

		void OnEnable()
		{
			dynaPanOp = (DynaPanelOp)target;


			rect = dynaPanOp.GetComponent<RectTransform>();


			lastSize = rect.sizeDelta;
			currentType = serializedObject.FindProperty("currentType");
			uiControls = serializedObject.FindProperty("uiControls");

			scriptableObjects = new Dictionary<UIControlType, SerializedProperty>
			{
				[UIControlType.Text] = serializedObject.FindProperty("textScriptObj"),
				[UIControlType.InputField] = serializedObject.FindProperty("inputFieldScriptObj"),
				[UIControlType.Dropdown] = serializedObject.FindProperty("dropdownScriptObj"),
				[UIControlType.CheckBox] = serializedObject.FindProperty("checkBoxScriptObj"),
				[UIControlType.Slider] = serializedObject.FindProperty("sliderScriptObj"),
				[UIControlType.Button] = serializedObject.FindProperty("buttonScriptObj"),
				[UIControlType.ButtonPanel] = serializedObject.FindProperty("buttonPanelScriptObj"),
				[UIControlType.Image] = serializedObject.FindProperty("imageViewScriptObj"),
				[UIControlType.ImagePanel] = serializedObject.FindProperty("imageViewPanelScriptObj"),
			};


			dynaPan = dynaPanOp.GetComponent<DynamicPanel>();
			dynaPanSO = new SerializedObject(dynaPan);
			titleStyle = dynaPanSO.FindProperty("titleType");
			titleText = dynaPanSO.FindProperty("_titleText");
			centerTitleText = dynaPanSO.FindProperty("centerTitleText");
			panelStyle = dynaPanSO.FindProperty("panelType");
			tabs = dynaPanSO.FindProperty("tabs");
			selectedTabIndex = dynaPanSO.FindProperty("selectedTabIndex");
			minDims = dynaPanSO.FindProperty("minSize");
			maxDims = dynaPanSO.FindProperty("maxSize");
			alwaysShrink = dynaPanSO.FindProperty("alwaysShrinkToMinSize");
			showCloseButton = dynaPanSO.FindProperty("showCloseButton");
			showMinimizeButton = dynaPanSO.FindProperty("showMinimizeButton");
			showMaximizeButton = dynaPanSO.FindProperty("showMaximizeButton");

			BuildControlList();
		}

		private void BuildControlList()
		{
			var currentControls = dynaPan.GetComponentInChildren<BottomPanel>().GetControlsFromTransform();
			dynaPanOp.uiControls.Clear();
			foreach (var control in currentControls.Values)
			{
				if (!control.TryGetComponent<IUIBehavior>(out var uiBehavior))
				{
					Debug.LogError($"control {control.name} has no UIBehavior");
					continue;
				}

				var dataEx = uiBehavior.GetBackingData();
				var newCtrl = new UIControl();
				newCtrl.controlType = dataEx.dataType;
				dynaPanOp.uiControls.Add(newCtrl);
				typeof(UIControl).GetField(UIControl.panelControlNames[dataEx.dataType]).SetValue(newCtrl, dataEx);
			}

			dynaPan.Refresh();
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(titleStyle);
			EditorGUILayout.PropertyField(titleText);
			switch (dynaPan.titleType)
			{
				case TitleLabelStyle.Bar:
					EditorGUILayout.PropertyField(centerTitleText);
					break;

				case TitleLabelStyle.SquareTab:
				case TitleLabelStyle.BladedTab:
				{
					EditorGUILayout.PropertyField(tabs);
					EditorGUILayout.PropertyField(selectedTabIndex);
				}
				break;
			}

			EditorGUILayout.PropertyField(panelStyle);
			EditorGUILayout.PropertyField(minDims);
			EditorGUILayout.PropertyField(maxDims);
			EditorGUILayout.PropertyField(alwaysShrink);
			EditorGUILayout.PropertyField(showCloseButton);
			EditorGUILayout.PropertyField(showMinimizeButton);
			//EditorGUILayout.PropertyField(showMaximizeButton);

			isVisible = EditorGUILayout.Foldout(isVisible, "Default UI Scriptable Objects", true);
			if (isVisible)
			{
				foreach (var so in scriptableObjects)
					EditorGUILayout.PropertyField(so.Value);
			}

			EditorGUILayout.PropertyField(currentType);
			EditorGUILayout.PropertyField(scriptableObjects[dynaPanOp.currentType]);

			if (GUILayout.Button($"Add {dynaPanOp.currentType} to Panel"))
			{
				dynaPanOp.AddUIControl();
				BuildControlList();
			}

			EditorGUILayout.PropertyField(uiControls);

			if (GUILayout.Button("Clear All"))
			{
				dynaPan.ClearControlsEditor();
			}

			var soUpdated = serializedObject.ApplyModifiedProperties();
			var panelSOUpdated = dynaPanSO.ApplyModifiedProperties();
			if (soUpdated || panelSOUpdated)
			{
				//dynaPan.UpdateData(dynaPanOp.uiControls);
			}

			if (EditorGUI.EndChangeCheck())
			{
				dynaPan.Refresh();
				BuildControlList();
			}
		}

		/// <summary>
		/// This is to prevent the user from manually resizing the panel.
		/// Maybe we can toggle this?
		/// </summary>
		public void OnSceneGUI()
		{
			var size = rect.sizeDelta;
			if (size != lastSize)
			{
				lastSize = size;
			}
		}
	}



	[CustomEditor(typeof(MagicWindow))]
	public class MagicWindowEditor : Editor
	{
		private MagicWindow magicWindow;
		private RectTransform rect;
		private Vector2 lastSize;
		private SerializedProperty currentType;
		private SerializedProperty controlList;
		private Dictionary<UIControlType, SerializedProperty> scriptableObjects;

		void OnEnable()
		{
			magicWindow = (MagicWindow)target;
			rect = magicWindow.GetComponent<RectTransform>();

			lastSize = rect.sizeDelta;
			currentType = serializedObject.FindProperty("currentType");
			controlList = serializedObject.FindProperty("controlList");

			scriptableObjects = new Dictionary<UIControlType, SerializedProperty>
			{
				[UIControlType.Text] = serializedObject.FindProperty("textScriptObj"),
				[UIControlType.InputField] = serializedObject.FindProperty("inputFieldScriptObj"),
				[UIControlType.Dropdown] = serializedObject.FindProperty("dropdownScriptObj"),
				[UIControlType.CheckBox] = serializedObject.FindProperty("checkBoxScriptObj"),
				[UIControlType.Slider] = serializedObject.FindProperty("sliderScriptObj"),
				[UIControlType.Button] = serializedObject.FindProperty("buttonScriptObj"),
				[UIControlType.ButtonPanel] = serializedObject.FindProperty("buttonPanelScriptObj"),
				[UIControlType.Image] = serializedObject.FindProperty("imageViewScriptObj"),
				[UIControlType.ImagePanel] = serializedObject.FindProperty("imageViewPanelScriptObj"),
			};

			BuildControlList();
		}

		private void BuildControlList()
		{
			var currentControls = magicWindow.GetControlsFromTransform();
			magicWindow.controlList.Clear();
			foreach (var control in currentControls.Values)
			{
				if (!control.TryGetComponent<IUIBehavior>(out var uiBehavior))
				{
					Debug.LogError($"control {control.name} has no UIBehavior");
					continue;
				}

				var dataEx = uiBehavior.GetBackingData();
				var newCtrl = new UIControl();
				newCtrl.controlType = dataEx.dataType;
				magicWindow.controlList.Add(newCtrl);
				typeof(UIControl).GetField(UIControl.panelControlNames[dataEx.dataType]).SetValue(newCtrl, dataEx);
			}

			magicWindow.Refresh(magicWindow.gameObject);
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_referenceName"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("tabControl"));

			EditorGUILayout.PropertyField(currentType);
			EditorGUILayout.PropertyField(scriptableObjects[magicWindow.currentType]);

			if (GUILayout.Button($"Add {magicWindow.currentType} to Panel"))
			{
				magicWindow.AddUIControl();
				BuildControlList();
			}

			EditorGUILayout.PropertyField(controlList);

			if (GUILayout.Button("Clear All"))
			{
				magicWindow.ClearControlsEditor();
			}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				magicWindow.Refresh(magicWindow.gameObject);
			}
		}
	}


	[CustomEditor(typeof(UITabControl))]
	public class TabControlEditor : Editor
	{
		private UITabControl tabControl;
		private TabLookupDictionary tabPanels;
		private int removeTabIndex;
		private static bool isTabEditFoldout = true;
		private bool isPanelEditFoldout = true;
		private bool isPanelFoldout;

		void OnEnable()
		{
			tabControl = (UITabControl)target;
			tabPanels = tabControl.tabPanels;
			removeTabIndex = tabPanels.Count - 1;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_referenceName"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("panelExData"));



			EditorGUILayout.PropertyField(serializedObject.FindProperty("tabItemsTransform"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("panelsTransform"));


			EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedTabIndex"));

			if (isPanelEditFoldout = EditorGUILayout.Foldout(isPanelEditFoldout, "Panel Edit", true))
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("panelPrefab"));

				EditorGUILayout.PropertyField(serializedObject.FindProperty("panelWidthAdjust"));


				var selectedTabPanel = tabPanels[tabControl.selectedTabIndex];
				var selectedTab = selectedTabPanel.Key;
				var selectedPanel = selectedTabPanel.Value;
				UIPanelEditor panelEditor = (UIPanelEditor)Editor.CreateEditor(selectedPanel, typeof(UIPanelEditor));

				if (isPanelFoldout = EditorGUILayout.Foldout(isPanelFoldout, "Edit selected Panel " + selectedTab.name, true))
				{
					++EditorGUI.indentLevel;

					panelEditor.OnInspectorGUI();
					--EditorGUI.indentLevel;
				}

				--EditorGUI.indentLevel;
			}

			if (isTabEditFoldout = EditorGUILayout.Foldout(isTabEditFoldout, "Tab Edit", true))
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("tabItemPrefab"));

				if (GUILayout.Button("Add Tab"))
				{
					tabControl.AddTab(tabControl.panelExData);
					removeTabIndex = tabPanels.Count - 1;
					EditorUtility.SetDirty(tabControl);
				}

				EditorGUILayout.PropertyField(serializedObject.FindProperty("tabHorizontaloffset"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("firstTabSprite"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("tabSprite"));

				EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedTabColor"));
				EditorGUILayout.PropertyField(serializedObject.FindProperty("deselectedTabColor"));

				GUILayout.BeginHorizontal();
				{
					removeTabIndex = EditorGUILayout.IntField("Remove Tab at index:", removeTabIndex);
					if (removeTabIndex >= tabPanels.Count)
						removeTabIndex = tabPanels.Count - 1;
					if (removeTabIndex < 0)
						removeTabIndex = 0;

					if (GUILayout.Button("Remove " + tabPanels[removeTabIndex].Key.name))
					{
						tabControl.RemoveTab(removeTabIndex);
						EditorUtility.SetDirty(tabControl);
					}
				}
				GUILayout.EndHorizontal();
				--EditorGUI.indentLevel;
			}



			serializedObject.ApplyModifiedProperties();
			if (tabControl.selectedTabIndex >= tabPanels.Count)
				tabControl.selectedTabIndex = tabPanels.Count - 1;

			if (EditorGUI.EndChangeCheck())
			{
				tabControl.Refresh(tabControl.gameObject);
			}
		}
	}


	[CustomEditor(typeof(UIPanel))]
	public class UIPanelEditor : Editor
	{
		private Vector2 lastSize;
		private UIPanel panel;
		private RectTransform rect;
		private PanelEx panelEx;
		private ControlLookupDictionary uiControls;
		private Dictionary<UIDesignObject, bool> isFoldout = new();
		//private Dictionary<UIControlType, SerializedProperty> scriptableObjects;
		private UIControlType currentType;
		private Dictionary<UIControlType, string> propertyName = new()
		{
			[UIControlType.Button] = "buttonEx",
			[UIControlType.ButtonPanel] = "buttonPanelEx",
			[UIControlType.CheckBox] = "checkBoxEx",
			[UIControlType.Dropdown] = "dropdownEx",
			[UIControlType.Image] = "imageEx",
			[UIControlType.ImagePanel] = "imagePanelEx",
			[UIControlType.InputField] = "inputFieldEx",
			[UIControlType.Slider] = "sliderEx",
			[UIControlType.TabControl] = "tabControlEx",
			[UIControlType.Text] = "labelEx",
		};
		private bool isUIControlsFoldout = true;

		public void OnEnable()
		{
			panel = (UIPanel)target;
			rect = panel.GetComponent<RectTransform>();

			panelEx = (PanelEx)panel.GetBackingData();
			panel.GetControlsFromTransform();
			uiControls = panel.uiControls;
			isFoldout.Clear();
			foreach (var ctrl in uiControls)
				isFoldout.Add(ctrl.Value, false);
			//scriptableObjects = new Dictionary<UIControlType, SerializedProperty>
			//{
			//	[UIControlType.Text] = serializedObject.FindProperty("textScriptObj"),
			//	[UIControlType.InputField] = serializedObject.FindProperty("inputFieldScriptObj"),
			//	[UIControlType.Dropdown] = serializedObject.FindProperty("dropdownScriptObj"),
			//	[UIControlType.CheckBox] = serializedObject.FindProperty("checkBoxScriptObj"),
			//	[UIControlType.Slider] = serializedObject.FindProperty("sliderScriptObj"),
			//	[UIControlType.Button] = serializedObject.FindProperty("buttonScriptObj"),
			//	[UIControlType.ButtonPanel] = serializedObject.FindProperty("buttonPanelScriptObj"),
			//	[UIControlType.Image] = serializedObject.FindProperty("imageViewScriptObj"),
			//	[UIControlType.ImagePanel] = serializedObject.FindProperty("imageViewPanelScriptObj"),
			//};

			//if (scriptableObjects[UIControlType.Text] == null)
			//	scriptableObjects = null;
		}



		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("panelEx"));
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("minDimensions"));
			currentType = (UIControlType)EditorGUILayout.EnumPopup("UI Control Type", currentType);
			//EditorGUILayout.PropertyField(currentType);
			//if (scriptableObjects != null)
			//	EditorGUILayout.PropertyField(scriptableObjects[currentType]);

			if (GUILayout.Button($"Add {currentType} to Panel"))
			{
				IUIBehavior uiBehave = null;
				switch (currentType)
				{
					case UIControlType.Text:
						uiBehave = panel.AddUIControl(new LabelEx("Label Text"));
						break;

					case UIControlType.Button:
						uiBehave = panel.AddUIControl(new ButtonEx(null));
						break;

					case UIControlType.ButtonPanel:
						uiBehave = panel.AddUIControl(new ButtonPanelEx(null));
						break;


					case UIControlType.CheckBox:
						uiBehave = panel.AddUIControl(new CheckBoxEx(null));
						break;

					case UIControlType.Dropdown:
						uiBehave = panel.AddUIControl(new DropdownEx(null));
						break;

					case UIControlType.Image:
						uiBehave = panel.AddUIControl(new ImageEx(null));
						break;

					case UIControlType.ImagePanel:
						uiBehave = panel.AddUIControl(new ImageViewDataEx(null));
						break;

					case UIControlType.InputField:
						uiBehave = panel.AddUIControl(new InputFieldEx(null));
						break;

					case UIControlType.Slider:
						uiBehave = panel.AddUIControl(new SliderEx(null));
						break;

					//case UIControlType.TabControl:
					//	panel.AddUIControl(new TabControlEx(null));
					//	break;

					//case UIControlType.:
					//	panel.AddUIControl(new Ex(null));
					//	break;

					default:
						Debug.LogWarning($"{currentType} not yet implemented");
						break;
				}

				if (uiBehave != null)
				{
					isFoldout.Add(uiBehave.designObject, true);
					EditorUtility.SetDirty(panel);
				}
			}

			if (isUIControlsFoldout = EditorGUILayout.Foldout(isUIControlsFoldout, "UI Controls attached to panel", true))
			{
				++EditorGUI.indentLevel;
				foreach (var control in uiControls)
				{
					var uiControl = control.Value.GetComponent<IUIBehavior>();
					var data = uiControl.GetBackingData();
					SerializedObject uiSO = null;
					string referenceName = null;
					switch (data.dataType)
					{
						case UIControlType.Text:
						{
							var uiCtrl = (UIExpandingLabel)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.Button:
						{
							var uiCtrl = (UIButton)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.ButtonPanel:
						{
							var uiCtrl = (UIButtonPanel)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.CheckBox:
						{
							var uiCtrl = (UICheckBox)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.Dropdown:
						{
							var uiCtrl = (UIDropdown)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.Image:
						{
							var uiCtrl = (UIImageView)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;


						case UIControlType.ImagePanel:
						{
							var uiCtrl = (UIImageViewPanel)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.InputField:
						{
							var uiCtrl = (UIExpandingInputField)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.Slider:
						{
							var uiCtrl = (UISlider)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.TabControl:
						{
							var uiCtrl = (UITabControl)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						default:
							Debug.Log($"{data.dataType} not yet implemented");
							continue;
					}

					isFoldout[control.Value] = EditorGUILayout.Foldout(isFoldout[control.Value], (data.dataType.ToString() + " - " + referenceName), true);
					if (isFoldout[control.Value])
					{
						++EditorGUI.indentLevel;

						EditorGUILayout.PropertyField(uiSO.FindProperty(propertyName[data.dataType]));
						--EditorGUI.indentLevel;
						uiSO.ApplyModifiedProperties();
						if (GUILayout.Button("Remove"))
						{
							panel.RemoveControl(control);
							isFoldout.Remove(control.Value);
							EditorUtility.SetDirty(panel);
							break;
						}
						//else

					}
				}
				--EditorGUI.indentLevel;
			}

			bool changed = serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				panel.RecalculateDimensions();
				lastSize = rect.sizeDelta;
			}
		}


		public void OnSceneGUI()
		{
			// lock the panel size so it can't get changed except by it's parent 
			if (lastSize != rect.sizeDelta)
			{
				panel.SetToParentSize();
				//lastSize = rect.sizeDelta;
			}
		}
	}



	[CustomEditor(typeof(BottomPanel))]
	public class BottomPanelEditor : Editor
	{
		private Vector2 lastSize;
		private BottomPanel panel;
		private RectTransform rect;

		public void OnEnable()
		{
			panel = (BottomPanel)target;
			rect = panel.GetComponent<RectTransform>();
		}

		public void OnSceneGUI()
		{
			// lock the panel size so it can't get changed except by it's parent 
			if (lastSize != rect.sizeDelta)
			{
				panel.SetToParentSize();
				lastSize = rect.sizeDelta;
			}
		}
	}



	[CustomEditor(typeof(UIImageViewPanel))]
	public class UIImageViewPanelEditor : Editor
	{
		private UIImageViewPanel imageViewPanel;
		private ImageViewDataEx viewDataEx;
		private Dictionary<ImageEx, UIImageView> images = new();
		private bool imagesVisible = true;
		private SerializedObject gridLayoutSO;
		private SerializedObject scrollRectSO;
		private SerializedObject contentRectSO;

		public void OnEnable()
		{
			imageViewPanel = (UIImageViewPanel)target;
			viewDataEx = (ImageViewDataEx)imageViewPanel.GetBackingData();
			gridLayoutSO = new SerializedObject(imageViewPanel.gridLayout);
			scrollRectSO = new SerializedObject(imageViewPanel.scrollRect);
			contentRectSO = new SerializedObject(imageViewPanel.scrollRect.content);
			BuildImageList();
		}

		private void BuildImageList()
		{
			var imageViews = imageViewPanel.GetComponentsInChildren<UIImageView>();
			images.Clear();
			foreach (var imageView in imageViews)
			{
				images.Add((ImageEx)imageView.GetBackingData(), imageView);
			}

			imageViewPanel.images = images;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			var maxPanelSize = viewDataEx.scriptableObj.maxPanelSize;
			var imageSize = viewDataEx.scriptableObj.imageSize;
			int imagesOnRow = Mathf.FloorToInt(
				(maxPanelSize.x - (imageViewPanel.gridLayout.padding.left + imageViewPanel.gridLayout.padding.right))
				/ (imageSize.x + imageViewPanel.gridLayout.spacing.x));
			var minWidthReq = imageSize.x * imagesOnRow + imageViewPanel.gridLayout.spacing.x * (imagesOnRow - 1);

			GUILayout.BeginHorizontal();
			EditorGUILayout.FloatField("min panel width", minWidthReq);
			EditorGUILayout.IntField("images on row", imagesOnRow);

			GUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				imageViewPanel.UpdateBackingData();
				imageViewPanel.gridLayout.cellSize = new Vector2(
					Mathf.Max(imageSize.x, viewDataEx.labelEx.maxLabelDimensions.x),
					imageSize.y + viewDataEx.labelEx.maxLabelDimensions.y);
				gridLayoutSO.ApplyModifiedProperties();

				contentRectSO.ApplyModifiedProperties();
			}

			if (GUILayout.Button("Add Image"))
			{
				imageViewPanel.AddImage(new ImageEx(viewDataEx.scriptableObj.imageViewData)
				{
					sprite = viewDataEx.scriptableObj.defaultSprite,
					labelEx = viewDataEx.labelEx,
				});
			}

			imagesVisible = EditorGUILayout.BeginFoldoutHeaderGroup(imagesVisible, "Images");
			if (imagesVisible)
			{
				ImageEx remove = null;
				foreach (var image in images)
				{
					var rect = EditorGUILayout.BeginHorizontal();
					var newSprite = (Sprite)EditorGUILayout.ObjectField(image.Key.sprite, typeof(Sprite), false);
					var newText = EditorGUILayout.TextField(image.Key.labelEx.text);
					EditorGUILayout.EndHorizontal();

					if (newSprite != image.Key.sprite || newText != image.Key.labelEx.text)
					{
						image.Key.sprite = newSprite;
						image.Key.labelEx.text = newText;
						image.Value.UpdateBackingData();
					}

					if (GUILayout.Button("Remove"))
					{
						remove = image.Key;

					}

					GUILayout.Space(15);
				}

				if (remove != null)
				{
					imageViewPanel.RemoveImage(remove);
				}
			}
		}

		//public void OnSceneGUI()
		//{
		//	var size = rect.sizeDelta;
		//	if (size != lastSize)
		//	{
		//		imageViewPanel.UpdateBackingData();
		//		lastSize = size;
		//	}
		//}
	}


	[CustomEditor(typeof(UIDropdown))]
	public class UIDropdownEditor : UIEditor<UIDropdown> { }

	[CustomEditor(typeof(UIButtonPanel))]
	public class UIButtonPanelEditor : UIEditor<UIButtonPanel> { }

	[CustomEditor(typeof(UICheckBox))]
	public class UICheckBoxEditor : UIEditor<UICheckBox> { }


	[CustomEditor(typeof(UIButton))]
	public class UIButtonEditor : UIEditor<UIButton> { }

	[CustomEditor(typeof(UIExpandingInputField))]
	public class UIExpandingInputFieldEditor : UIEditor<UIExpandingInputField> { }

	[CustomEditor(typeof(UIImageView))]
	public class ImageViewEditor : UIEditor<UIImageView> { }


	[CustomEditor(typeof(UISlider))]
	public class SliderCustomEditor : UIEditor<UISlider>
	{
		private UISlider slider;
		private RectTransform rect;
		private Vector2 lastSize;

		void OnEnable()
		{
			slider = (UISlider)target;
			rect = slider.GetComponent<RectTransform>();
			lastSize = rect.sizeDelta;

			if (slider.transform.parent != null)
				slider.UpdateBackingData();
		}


		public void OnSceneGUI()
		{
			var size = rect.sizeDelta;
			if (size != lastSize)
			{
				slider.UpdateBackingData();
				lastSize = size;
			}
		}
	}

	[CustomEditor(typeof(UIButtonPanelScriptableObject))]
	public class UIButtonPanelScriptableObjectEditor : Editor
	{
		private SerializedObject panelSO;
		private SerializedProperty okButtonProp;
		private SerializedProperty cancelButtonProp;
		private SerializedProperty yesButtonProp;
		private SerializedProperty noButtonProp;
		private static bool isOKButtonFoldout = true;
		private static bool isCancelButtonFoldout = true;
		private static bool isYesButtonFoldout = true;
		private static bool isNoButtonFoldout = true;

		public void OnEnable()
		{
			var panel = (UIButtonPanelScriptableObject)target;
			panelSO = new SerializedObject(panel);

			okButtonProp = panelSO.FindProperty("okButton");
			cancelButtonProp = panelSO.FindProperty("cancelButton");
			yesButtonProp = panelSO.FindProperty("yesButton");
			noButtonProp = panelSO.FindProperty("noButton");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			//base.OnInspectorGUI();

			isOKButtonFoldout = EditorGUILayout.Foldout(isOKButtonFoldout, "OK Button Settings", true);
			if (isOKButtonFoldout)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(okButtonProp);
				--EditorGUI.indentLevel;
			}

			isCancelButtonFoldout = EditorGUILayout.Foldout(isCancelButtonFoldout, "Cancel Button Settings", true);
			if (isCancelButtonFoldout)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(cancelButtonProp);
				--EditorGUI.indentLevel;
			}


			isYesButtonFoldout = EditorGUILayout.Foldout(isYesButtonFoldout, "Yes Button Settings", true);
			if (isYesButtonFoldout)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(yesButtonProp);
				--EditorGUI.indentLevel;
			}


			isNoButtonFoldout = EditorGUILayout.Foldout(isNoButtonFoldout, "No Button Settings", true);
			if (isNoButtonFoldout)
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(noButtonProp);
				--EditorGUI.indentLevel;
			}


			if (EditorGUI.EndChangeCheck())
			{
				panelSO.ApplyModifiedProperties();
			}
		}
	}


	public class UIEditor<T> : Editor where T : MonoBehaviour, IUIBehavior
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			if (EditorGUI.EndChangeCheck())
			{
				T buttonPanel = (T)target;
				buttonPanel.UpdateBackingData();
			}
		}
	}


	[CustomEditor(typeof(UIExpandingLabel))]
	public class UILabelEditor : Editor
	{
		private SerializedProperty textLabel;
		private SerializedProperty image;
		private SerializedProperty labelExProp;
		private UIExpandingLabel label;
		private LabelEx labelEx;
		private bool isLabelExFoldout = true;

		void OnEnable()
		{
			textLabel = serializedObject.FindProperty("textLabel");
			image = serializedObject.FindProperty("image");
			labelExProp = serializedObject.FindProperty("labelEx");

			label = (UIExpandingLabel)target;
			labelEx = (LabelEx)(label).GetBackingData();

			/// Keeping this here for future reference!
			//var tmpSP = serializedObject.FindProperty("textLabel");
			//var targetObjectClassType = EditorHelper.GetTargetObjectOfProperty(tmpSP);
			//if (targetObjectClassType != null)
			//{
			//	tmp = (TextMeshProUGUI)targetObjectClassType;
			//}
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			GUILayout.Space(10);
			{
				EditorGUILayout.PropertyField(textLabel);
				EditorGUILayout.PropertyField(image);
				if (isLabelExFoldout = EditorGUILayout.Foldout(isLabelExFoldout, "Label Data", true))
				{
					++EditorGUI.indentLevel;
					EditorGUILayout.PropertyField(labelExProp);
					--EditorGUI.indentLevel;
				}
			}

			if (serializedObject.ApplyModifiedProperties())
			{
			}

			if (EditorGUI.EndChangeCheck())
			{
				label.Refresh(label.gameObject);
			}
		}
	}




	/// <summary>
	/// This is for the NON-canvas text label!
	/// </summary>
	[CustomEditor(typeof(ExpandingLabel))]
	public class LabelEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			if (EditorGUI.EndChangeCheck())
			{
				var panel = (ExpandingLabel)target;
				panel.UpdateText();
			}
		}
	}
}