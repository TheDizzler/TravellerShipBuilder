using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(MagicWindow))]
	public class MagicWindowEditor : Editor
	{
		private MagicWindow magicWindow;
		private RectTransform rect;
		//private Vector2 lastSize;
		//private SerializedProperty currentType;
		//private SerializedProperty controlList;
		private Dictionary<UIControlType, SerializedProperty> scriptableObjects;
		private Editor tabEditor;
		private UIPanelEditor panelEditor;
		private MagicWindow.WindowStyle prevWindowStyle;
		private Dictionary<UITabControlScriptableObject, UITabControlScriptableObjectEditor> tabScriptObjEditors = new();
		private bool isTabControlScriptObjFoldout;
		private bool isScriptObjFoldout;
		private bool isTitleBarFoldout;
		private Editor tabLabelEditor;

		void OnEnable()
		{
			magicWindow = (MagicWindow)target;
			rect = magicWindow.GetComponent<RectTransform>();

			//lastSize = rect.sizeDelta;
			//currentType = serializedObject.FindProperty("currentType");
			//controlList = serializedObject.FindProperty("controlList");

			scriptableObjects = new Dictionary<UIControlType, SerializedProperty>
			{
				[UIControlType.Panel] = serializedObject.FindProperty("panelScriptObj"),
				[UIControlType.HorizontalPanel] = serializedObject.FindProperty("horizontalPanelScriptObj"),
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
		}

		public override void OnInspectorGUI()
		{
			if (!magicWindow.gameObject.scene.IsValid())
			{
				EditorGUILayout.LabelField("Drag Magic Window into scene to edit");
				return;
			}

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_referenceName"));

			if (magicWindow.rootTabControl == null)
			{
				magicWindow.CreateRootTabControl();
			}

			var rootTabProp = serializedObject.FindProperty("rootTabControl");
			GUI.enabled = false;
			EditorGUILayout.PropertyField(rootTabProp);
			GUI.enabled = true;

			if (isScriptObjFoldout = EditorGUILayout.Foldout(isScriptObjFoldout, "ScriptObjects", true))
			{
				foreach (var scriptObj in scriptableObjects)
				{
					EditorGUILayout.PropertyField(scriptObj.Value);
				}
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("windowStyleDatas"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("windowStyle"));
			if (magicWindow.windowStyle != prevWindowStyle)
			{
				magicWindow.ChangeWindowStyle(magicWindow.windowStyle);
				EditorUtility.SetDirty(magicWindow);
			}

			prevWindowStyle = magicWindow.windowStyle;

			GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

			switch (magicWindow.windowStyle)
			{
				case MagicWindow.WindowStyle.Tabbed:
				{
					EditorGUILayout.LabelField("Tabbed Window Controls");

					++EditorGUI.indentLevel;

					Editor.CreateCachedEditor(magicWindow.rootTabControl, typeof(TabControlEditor), ref tabEditor);
					tabEditor.OnInspectorGUI();

					--EditorGUI.indentLevel;
				}
				break;

				case MagicWindow.WindowStyle.TitleBar:
				{
					EditorGUILayout.LabelField("Title Bar Window Controls");
					++EditorGUI.indentLevel;
					if (isTitleBarFoldout = EditorGUILayout.Foldout(isTitleBarFoldout, "Title Bar", true))
					{
						++EditorGUI.indentLevel;

						Editor.CreateCachedEditor(magicWindow.rootTabControl.tabPanels[0].Key, typeof(UILabelEditor), ref tabLabelEditor);
						tabLabelEditor.OnInspectorGUI();
						//var tabSO = new SerializedObject(magicWindow.rootTabControl.tabPanels[0].Key);
						//var tabLabelExProp = tabSO.FindProperty("labelEx");
						//EditorGUILayout.PropertyField(tabLabelExProp);
						//tabSO.ApplyModifiedProperties();

						--EditorGUI.indentLevel;
					}

					if (panelEditor == null)
						panelEditor = (UIPanelEditor)Editor.CreateEditor(
							magicWindow.rootTabControl.SelectedPanel(), typeof(UIPanelEditor));
					panelEditor.OnInspectorGUI();

					//EditorGUILayout.PropertyField(serializedObject.FindProperty("tabControlEx"));

					if (!tabScriptObjEditors.TryGetValue(magicWindow.rootTabControl.tabControlEx.scriptableObj, out var tabDataEditor)
						|| tabDataEditor == null)
					{
						tabDataEditor = (UITabControlScriptableObjectEditor)Editor.CreateEditor(magicWindow.rootTabControl.tabControlEx.scriptableObj);
						tabScriptObjEditors.Add(magicWindow.rootTabControl.tabControlEx.scriptableObj, tabDataEditor);
					}

					if (isTabControlScriptObjFoldout = EditorGUILayout.Foldout(isTabControlScriptObjFoldout, "Tabcontrol scriptable object", true))
					{
						GUI.enabled = false;
						++EditorGUI.indentLevel;
						tabDataEditor.OnInspectorGUI();
						--EditorGUI.indentLevel;
						GUI.enabled = true;
					}

					--EditorGUI.indentLevel;
				}
				break;


				case MagicWindow.WindowStyle.ContextMenu:
				{
					EditorGUILayout.LabelField("ContextMenu Window Controls");
					Debug.LogError("ContextMenu not yet implemented");
				}
				break;
			}


			GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

			//if (GUILayout.Button("Clear All UI Controls"))
			//{
			//	magicWindow.ClearControlsEditor();
			//}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				magicWindow.Refresh();
			}
		}
	}


	[CustomEditor(typeof(UITabControl))]
	public class TabControlEditor : Editor
	{
		public UITabControl tabControl;
		private TabLookupDictionary tabPanels;
		private int removeTabIndex;
		private Editor panelEditor;
		private static bool isTabEditFoldout = true;
		private bool isPanelEditFoldout = true;
		private bool isPanelFoldout;
		private int lastSelectedIndex = -2;
		private Dictionary<UITabControlScriptableObject, UITabControlScriptableObjectEditor> tabScriptObjEditors = new();
		private bool isScriptObjFoldout;
		private bool isTabLabelExFoldout;

		void OnEnable()
		{
			tabControl = (UITabControl)target;
			tabControl.ReconstructTabsFromTransform();
			tabPanels = tabControl.tabPanels;
			removeTabIndex = tabPanels.Count - 1;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_referenceName"));

			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("panelsTransform"));
			GUI.enabled = true;

			var windowStyle = tabControl.GetWindowStyle();
			var selectedIndex = serializedObject.FindProperty("selectedTabIndex");
			if (windowStyle == MagicWindow.WindowStyle.Tabbed)
			{
				EditorGUILayout.PropertyField(selectedIndex);
				if (selectedIndex.intValue >= tabPanels.Count)
				{
					selectedIndex.intValue = tabPanels.Count - 1;
				}
			}
			else
				selectedIndex.intValue = 0;

			var selectedTabPanel = tabPanels[selectedIndex.intValue];
			UIExpandingLabel selectedTab = selectedTabPanel.Key;

			if (isPanelFoldout = EditorGUILayout.Foldout(isPanelFoldout,
				"Selected Tab: " + selectedTab.referenceName + " - Edit and Add Controls", true))
			{
				++EditorGUI.indentLevel;

				//EditorGUILayout.TextArea(selectedTab.text
				if (isTabLabelExFoldout = EditorGUILayout.Foldout(isTabLabelExFoldout, "Tab Label Properties", true))
				{
					++EditorGUI.indentLevel;
					var labelSO = new SerializedObject(selectedTab);
					var labelExProp = labelSO.FindProperty("labelEx");
					EditorGUILayout.PropertyField(labelExProp);
					labelSO.ApplyModifiedProperties();
					--EditorGUI.indentLevel;
				}

				Editor.CreateCachedEditor(selectedTabPanel.Value, typeof(UIPanelEditor), ref panelEditor);
				panelEditor.OnInspectorGUI();
				if (((UIPanelEditor)panelEditor).isDeadEditor)
					lastSelectedIndex = -1;

				--EditorGUI.indentLevel;
			}

			if (windowStyle == MagicWindow.WindowStyle.Tabbed)
			{
				GUILayout.BeginHorizontal();
				{
					if (tabPanels.Count <= 1)
						GUI.enabled = false;
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

					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();


				if (GUILayout.Button("New Tab on " + tabControl.referenceName))
				{
					tabControl.AddTab();
					removeTabIndex = tabPanels.Count - 1;
					EditorUtility.SetDirty(tabControl);
				}
			}

			if (isPanelEditFoldout = EditorGUILayout.Foldout(isPanelEditFoldout, "Panel Settings", true))
			{
				++EditorGUI.indentLevel;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("panelPrefab"));// this is sometimes null if the foldout is closed?

				if (lastSelectedIndex != tabControl.selectedTabIndex)
				{
					var selectedPanel = selectedTabPanel.Value;
					Editor.CreateCachedEditor(selectedPanel, typeof(UIPanelEditor), ref panelEditor);
					lastSelectedIndex = tabControl.selectedTabIndex;
				}
				--EditorGUI.indentLevel;
			}

			if (isTabEditFoldout = EditorGUILayout.Foldout(isTabEditFoldout, "Tab Control Settings", true))
			{
				++EditorGUI.indentLevel;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("tabItemPrefab")); // this is sometimes null if the foldout is closed?

				EditorGUILayout.PropertyField(serializedObject.FindProperty("tabControlEx"));

				if (isScriptObjFoldout = EditorGUILayout.Foldout(isScriptObjFoldout, "Tabcontrol scriptable object", true))
				{
					if (!tabScriptObjEditors.TryGetValue(tabControl.tabControlEx.scriptableObj, out var tabEditor)
						|| tabEditor == null)
					{
						tabEditor = (UITabControlScriptableObjectEditor)Editor.CreateEditor(tabControl.tabControlEx.scriptableObj);
						tabScriptObjEditors.Add(tabControl.tabControlEx.scriptableObj, tabEditor);
					}

					GUI.enabled = false;
					++EditorGUI.indentLevel;
					tabEditor.OnInspectorGUI();
					--EditorGUI.indentLevel;
					GUI.enabled = true;
				}

				--EditorGUI.indentLevel;
			}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				//serializedObject.FindProperty("isDirty").boolValue = true;
				tabControl.SetDirty();
				//tabControl.Refresh(tabControl.gameObject);
			}

			//EditorGUILayout.Vector2Field("size:", tabControl.GetMinDimensions());
		}
	}


	[CustomEditor(typeof(UITabControlScriptableObject))]
	public class UITabControlScriptableObjectEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(serializedObject.FindProperty("panelScriptableObj"));
			var windowStyleProp = serializedObject.FindProperty("windowStyle");
			EditorGUILayout.PropertyField(windowStyleProp);
			var windowStyle = (MagicWindow.WindowStyle)windowStyleProp.enumValueIndex;
			switch (windowStyle)
			{
				case MagicWindow.WindowStyle.ContextMenu:
					break;

				case MagicWindow.WindowStyle.Tabbed:
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarFontColor"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("tabTextAlignment"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarSprites"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleTextMargin"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarMinSize"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarVerticalOffset"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedTabColor"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("deselectedTabColor"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("disabledTabColor"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("tabHorizontaloffset"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("panelWidthAdjust"));

				}
				break;

				case MagicWindow.WindowStyle.TitleBar:
				{
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarFontColor"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("tabTextAlignment"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarSprites"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleTextMargin"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarMinSize"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("titleBarVerticalOffset"));
					EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedTabColor"), new GUIContent("TitleBar Color"));
				}
				break;
			}

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				//PrefabUtility.RecordPrefabInstancePropertyModifications(target);
			}
		}
	}


	[CustomEditor(typeof(UIPanel))]
	public class UIPanelEditor : Editor
	{
		private Vector2 lastSize;
		private UIPanel panel;
		private RectTransform rect;
		private List<UIDesignObject> uiControls;
		private static Dictionary<UIDesignObject, bool> isFoldout = new();


		private Dictionary<string, TabControlEditor> tabControlEditors = new();
		private Dictionary<string, UISpinnerEditor> spinnerEditors = new();
		private Editor sliderEditor;
		private Editor panelEditor;
		private Editor dropdownEditor;
		private Editor checkboxEditor;
		private Editor labelEditor;

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
			[UIControlType.Text] = "labelEx",
			//[UIControlType.Spinner] = "spinnerEx",
		};
		private bool isUIControlsFoldout = true;
		public bool isDeadEditor;


		public void OnEnable()
		{
			panel = (UIPanel)target;
			rect = panel.GetComponent<RectTransform>();

			panel.GetControlsFromTransform();
			uiControls = panel.uiControls;

			foreach (var ctrl in uiControls)
			{
				if (ctrl == null)
				{
					Debug.LogWarning("A control is null. Are we creating a new control?");
					continue;
				}

				if (isFoldout.ContainsKey(ctrl))
					continue;
				isFoldout.Add(ctrl, false);
			}
		}



		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			GUIStyle delButtonStyle = new GUIStyle(GUI.skin.button);
			delButtonStyle.normal.textColor = Color.red;

			if (serializedObject == null || serializedObject.targetObject == null)
			{
				isDeadEditor = true;
				return;
			}

			var refProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refProp);

			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("tabLabel"));
			GUI.enabled = true;

			var panelExProp = serializedObject.FindProperty("panelEx");
			EditorGUILayout.PropertyField(panelExProp);

			var borderlessProp = serializedObject.FindProperty("_borderless");
			EditorGUILayout.PropertyField(borderlessProp);

			//EditorGUILayout.PropertyField(serializedObject.FindProperty("minDimensions"));
			currentType = (UIControlType)EditorGUILayout.EnumPopup("UI Control Type to add", currentType);

			if (GUILayout.Button($"Add {currentType} to Panel"))
			{
				IUIBehavior uiBehave = null;
				switch (currentType)
				{
					case UIControlType.Text:
						uiBehave = panel.AddUIControl(new LabelEx());
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
						uiBehave = panel.AddUIControl(new SliderEx());
						break;

					case UIControlType.TabControl:
						uiBehave = panel.AddTabControl();
						break;

					case UIControlType.Spinner:
						uiBehave = panel.AddUIControl(new SpinnerEx());
						break;

					case UIControlType.HorizontalPanel:
						uiBehave = panel.AddHorizontalPanel(null);
						break;

					case UIControlType.Panel:
						uiBehave = panel.AddPanel(null);
						break;

					//case UIControlType.:
					//	uiBehave = panel.AddUIControl(new Ex(null));
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

			if (isUIControlsFoldout = EditorGUILayout.Foldout(isUIControlsFoldout, $"UI Controls attached to panel (control count: {uiControls.Count})", true))
			{
				++EditorGUI.indentLevel;

				foreach (var control in uiControls)
				{
					if (control == null)
					{
						Debug.LogWarning("A control is null. Are we creating a new control?");
						continue;
					}

					var uiControl = control.GetComponent<IUIBehavior>();
					var data = uiControl.GetBackingData();
					if (data == null)
					{
						Debug.LogWarning($"{uiControl.referenceName} data is missing");
						continue;
					}

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

						case UIControlType.Spinner:
						{
							var uiCtrl = (UISpinner)uiControl;
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

						case UIControlType.Panel:
						case UIControlType.HorizontalPanel:
						{
							var uiCtrl = (UIPanel)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						default:
							Debug.Log($"{data.dataType} not yet implemented");
							continue;
					}

					var labelText = $"{(uiControl.designObject.gameObject.activeSelf ? "" : "(hidden) ")}{data.dataType.ToString()} - {referenceName}";
					isFoldout[control] = EditorGUILayout.Foldout(
						isFoldout[control], labelText, true);
					if (isFoldout[control])
					{
						++EditorGUI.indentLevel;
						uiControl.designObject.gameObject.SetActive(EditorGUILayout.Toggle("Is Visible", uiControl.designObject.gameObject.activeSelf));
						switch (data.dataType)
						{
							case UIControlType.TabControl:
							{
								if (!tabControlEditors.TryGetValue(referenceName, out TabControlEditor tabEditor))
								{
									tabEditor = (TabControlEditor)Editor.CreateEditor((UITabControl)uiControl, typeof(TabControlEditor));
									tabControlEditors.Add(referenceName, tabEditor);
								}

								if (tabEditor.tabControl == null)
								{
									tabEditor = (TabControlEditor)Editor.CreateEditor((UITabControl)uiControl, typeof(TabControlEditor));
									tabControlEditors[referenceName] = tabEditor;
								}

								tabEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Spinner:
							{
								if (!spinnerEditors.TryGetValue(referenceName, out UISpinnerEditor spinnerEditor))
								{
									spinnerEditor = (UISpinnerEditor)Editor.CreateEditor((UISpinner)uiControl, typeof(UISpinnerEditor));
									spinnerEditors.Add(referenceName, spinnerEditor);
								}

								if (spinnerEditor.spinner == null)
								{
									spinnerEditor = (UISpinnerEditor)Editor.CreateEditor((UISpinner)uiControl, typeof(UISpinnerEditor));
									spinnerEditors[referenceName] = spinnerEditor;
								}

								spinnerEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Slider:
							{
								Editor.CreateCachedEditor((UISlider)uiControl, typeof(UISliderEditor), ref sliderEditor);
								sliderEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Dropdown:
							{
								Editor.CreateCachedEditor((UIDropdown)uiControl, typeof(UIDropdownEditor), ref dropdownEditor);
								dropdownEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.CheckBox:
							{
								Editor.CreateCachedEditor((UICheckBox)uiControl, typeof(UICheckBoxEditor), ref checkboxEditor);
								checkboxEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Text:
							{
								Editor.CreateCachedEditor((UIExpandingLabel)uiControl, typeof(UILabelEditor), ref labelEditor);
								labelEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Panel:
							case UIControlType.HorizontalPanel:
							{
								Editor.CreateCachedEditor((UIPanel)uiControl, typeof(UIPanelEditor), ref panelEditor);
								panelEditor.OnInspectorGUI();
							}
							break;

							default:
							{
								EditorGUILayout.PropertyField(uiSO.FindProperty(propertyName[data.dataType]));
							}
							break;
						}

						uiSO.ApplyModifiedProperties();

						if (GUILayout.Button("Remove " + referenceName + " from " + target.name, delButtonStyle))
						{
							panel.RemoveControl(control);
							isFoldout.Remove(control);
							EditorUtility.SetDirty(panel);
							break;
						}

						--EditorGUI.indentLevel;
					}
				}

				--EditorGUI.indentLevel;
			}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				panel.SetDirty();
				panel.referenceName = refProp.stringValue;
				panel.borderless = borderlessProp.boolValue;
				lastSize = rect.sizeDelta;
			}

			//EditorGUILayout.Vector2Field("debug size:", panel.GetMinDimensions());
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
				Debug.LogWarning("label min/max dimensions have been removed from LabelEx");
				//imageViewPanel.gridLayout.cellSize = new Vector2(
				//	Mathf.Max(imageSize.x, viewDataEx.labelEx.maxLabelDimensions.x),
				//	imageSize.y + viewDataEx.labelEx.maxLabelDimensions.y); // will need to figure this out
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
					var newText = EditorGUILayout.TextField(image.Value.text);
					EditorGUILayout.EndHorizontal();

					if (newSprite != image.Key.sprite || newText != image.Value.text)
					{
						image.Key.sprite = newSprite;
						image.Value.text = newText;
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


	[CustomEditor(typeof(UIButtonPanel))]
	public class UIButtonPanelEditor : Editor
	{
		private UIButtonPanel buttonPanel;

		private void OnEnable()
		{
			buttonPanel = (UIButtonPanel)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonPanelEx"));

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				buttonPanel.referenceName = refNameProp.stringValue;
			}
		}
	}

	[CustomEditor(typeof(UICheckBox))]
	public class UICheckBoxEditor : Editor
	{
		private UICheckBox checkbox;

		private void OnEnable()
		{
			checkbox = (UICheckBox)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var textProp = serializedObject.FindProperty("_text");
			EditorGUILayout.PropertyField(textProp);

			var fontStylesProp = serializedObject.FindProperty("_fontStyles");
			EditorGUILayout.PropertyField(fontStylesProp);

			var alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			EditorGUILayout.PropertyField(alignmentProp);

			var marginProp = serializedObject.FindProperty("_margin");
			EditorGUILayout.PropertyField(marginProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_minLabelDimensions"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxLabelDimensions"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("onCheckChangedEvent"));

			EditorGUILayout.PropertyField(serializedObject.FindProperty("checkBoxEx"));

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				checkbox.referenceName = refNameProp.stringValue;
				checkbox.text = textProp.stringValue;
				checkbox.fontStyles = (FontStyles)fontStylesProp.enumValueFlag;
				checkbox.alignmentOptions = (TextAlignmentOptions)alignmentProp.enumValueFlag;
				checkbox.margin = marginProp.vector4Value;
			}
		}
	}


	[CustomEditor(typeof(UIExpandingInputField))]
	public class UIExpandingInputFieldEditor : Editor
	{
		private UIExpandingInputField inputField;

		private void OnEnable()
		{
			inputField = (UIExpandingInputField)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("inputFieldEx"));

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				inputField.referenceName = refNameProp.stringValue;
			}
		}
	}

	[CustomEditor(typeof(UIImageView))]
	public class ImageViewEditor : UIEditor<UIImageView> { }



	[CustomEditor(typeof(UIDropdown))]
	public class UIDropdownEditor : Editor
	{
		private UIDropdown dropdown;

		private void OnEnable()
		{
			dropdown = (UIDropdown)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var optionsDelegateProp = serializedObject.FindProperty("_optionsDelegate");
			EditorGUILayout.PropertyField(optionsDelegateProp);

			var optionsProp = serializedObject.FindProperty("_options");
			EditorGUILayout.PropertyField(optionsProp);


			var valueProp = serializedObject.FindProperty("_value");
			var oldValue = valueProp.intValue;
			//var newValue = EditorGUILayout.IntField(valueProp.intValue);

			int newValue;

			var isMultiSelectProp = serializedObject.FindProperty("_isMultiSelect");
			var newMultiValue = EditorGUILayout.Toggle("Multiselect", isMultiSelectProp.boolValue);
			if (isMultiSelectProp.boolValue)
			{
				newValue = EditorGUILayout.IntField(valueProp.intValue);
				var newSelection = 0;
				int bit = 1;
				++EditorGUI.indentLevel;
				for (int i = 0; i < optionsProp.arraySize; ++i)
				{
					var option = optionsProp.GetArrayElementAtIndex(i);
					var text = option.displayName;
					if (EditorGUILayout.Toggle(text, (newValue & bit) == bit))
						newSelection |= bit;
					bit <<= 1;
				}

				newValue = newSelection;
				--EditorGUI.indentLevel;
			}
			else
			{
				newValue = EditorGUILayout.IntSlider("Selection", valueProp.intValue, 0, optionsProp.arraySize - 1);
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("dropdownEx"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("onValueChangedAction"));


			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				dropdown.UpdateOptionsDelegate();
				dropdown.referenceName = refNameProp.stringValue;
				dropdown.value = newValue;
				valueProp.intValue = newValue;

				dropdown.isMultiSelect = newMultiValue;
				isMultiSelectProp.boolValue = newMultiValue;
			}
		}
	}


	[CustomEditor(typeof(UIButton))]
	public class UIButtonEditor : Editor
	{
		private UIButton button;
		private bool isButtonExFoldout;
		private Editor labelEditor;

		private void OnEnable()
		{
			button = (UIButton)target;
		}


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var txt = button.text; // this is required to keep changes to text in the actual label.
			var textProp = serializedObject.FindProperty("_text");
			EditorGUILayout.PropertyField(textProp);

			if (isButtonExFoldout = EditorGUILayout.Foldout(isButtonExFoldout, "ButtonEx", true))
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(serializedObject.FindProperty("buttonEx"));
				--EditorGUI.indentLevel;
			}

			Editor.CreateCachedEditor(button.label, typeof(UILabelEditor), ref labelEditor);
			labelEditor.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				button.text = textProp.stringValue;
			}
		}
	}


	[CustomEditor(typeof(UISlider))]
	public class UISliderEditor : Editor
	{
		public UISlider slider;
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


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var interactableProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(interactableProp);

			var wholeNumbersProp = serializedObject.FindProperty("_wholeNumbers");
			var minValueProp = serializedObject.FindProperty("_minValue");
			var maxValueProp = serializedObject.FindProperty("_maxValue");
			var valueProp = serializedObject.FindProperty("_value");

			EditorGUILayout.PropertyField(wholeNumbersProp);
			EditorGUILayout.PropertyField(minValueProp);
			EditorGUILayout.PropertyField(maxValueProp);
			var oldValue = valueProp.floatValue;
			float newValue;
			if (wholeNumbersProp.boolValue)
				newValue = EditorGUILayout.IntSlider("Value", Mathf.RoundToInt(valueProp.floatValue),
					Mathf.RoundToInt(minValueProp.floatValue), Mathf.RoundToInt(maxValueProp.floatValue));
			else
				newValue = EditorGUILayout.FloatField("Value", valueProp.floatValue);

			var showUnitsProp = serializedObject.FindProperty("_showUnits");
			var fontSizeProp = serializedObject.FindProperty("_fontSize");
			var fontColorProp = serializedObject.FindProperty("_fontColor");
			var unitOffsetProp = serializedObject.FindProperty("_unitVerticalOffset");
			var unitSpanProp = serializedObject.FindProperty("_unitSpan");

			EditorGUILayout.PropertyField(showUnitsProp);
			if (showUnitsProp.boolValue)
			{
				EditorGUILayout.PropertyField(unitSpanProp);
				EditorGUILayout.PropertyField(fontSizeProp);
				EditorGUILayout.PropertyField(fontColorProp);
				EditorGUILayout.PropertyField(unitOffsetProp);
			}

			var showHandleProp = serializedObject.FindProperty("_showHandle");
			var handleSpriteProp = serializedObject.FindProperty("_handleSprite");
			var handleOffsetProp = serializedObject.FindProperty("_handleOffset");

			EditorGUILayout.PropertyField(showHandleProp);
			if (showHandleProp.boolValue)
			{
				EditorGUILayout.PropertyField(handleSpriteProp);
				if (handleSpriteProp != null)
					EditorGUILayout.PropertyField(handleOffsetProp);
			}

			var fillHorzProp = serializedObject.FindProperty("_fillParentHorizontal");
			EditorGUILayout.PropertyField(fillHorzProp);


			var minDimenProp = serializedObject.FindProperty("_minDimensions");
			EditorGUILayout.PropertyField(minDimenProp);


			EditorGUILayout.PropertyField(serializedObject.FindProperty("onValueChanged"));
			//GUI.enabled = false;
			//EditorGUILayout.PropertyField(serializedObject.FindProperty("size"));
			//GUI.enabled = true;

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				slider.referenceName = refNameProp.stringValue;
				slider.interactable = interactableProp.boolValue;

				slider.minDimensions = minDimenProp.vector2Value;
				slider.fillParentHorizontal = fillHorzProp.boolValue;

				if (slider.showUnits)
				{
					slider.wholeNumbers = wholeNumbersProp.boolValue;
					slider.fontSize = fontSizeProp.floatValue;
					slider.fontColor = fontColorProp.colorValue;
					if (slider.wholeNumbers)
						slider.unitSpan = Mathf.RoundToInt(unitSpanProp.floatValue);
					else
						slider.unitSpan = unitSpanProp.floatValue;
					slider.unitVerticalOffset = unitOffsetProp.floatValue;
				}

				slider.minValue = minValueProp.floatValue;
				slider.maxValue = maxValueProp.floatValue;
				if (oldValue != newValue)
				{
					slider.value = newValue;
					valueProp.floatValue = slider.value;
				}

				slider.showUnits = showUnitsProp.boolValue;


				slider.showHandle = showHandleProp.boolValue;
				if (slider.showHandle)
				{
					slider.handleOffset = handleOffsetProp.vector2Value;
					slider.handleSprite = (Sprite)handleSpriteProp.objectReferenceValue;
				}

				//slider.Refresh(slider.gameObject);
			}


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

	[CustomEditor(typeof(UISpinner))]
	public class UISpinnerEditor : Editor
	{
		public UISpinner spinner;

		void OnEnable()
		{
			spinner = (UISpinner)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			/*
						EditorGUILayout.PropertyField(serializedObject.FindProperty("baseRect"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("leftButton"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("rightButton"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("inputField"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("text"));
						EditorGUILayout.PropertyField(serializedObject.FindProperty("placeholderText"));
			*/

			var activateProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(activateProp);

			var alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			EditorGUILayout.PropertyField(alignmentProp);

			var fontSizeProp = serializedObject.FindProperty("_fontSize");
			EditorGUILayout.PropertyField(fontSizeProp);


			var minValueProp = serializedObject.FindProperty("_minValue");
			var maxValueProp = serializedObject.FindProperty("_maxValue");
			var valueProp = serializedObject.FindProperty("_value");
			EditorGUILayout.PropertyField(minValueProp);
			EditorGUILayout.PropertyField(maxValueProp);
			var oldValue = valueProp.intValue;
			var newValue = EditorGUILayout.IntField("Value", valueProp.intValue);


			var minDimenProp = serializedObject.FindProperty("_minDimen");
			EditorGUILayout.PropertyField(minDimenProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("onValueChanged"));

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				spinner.referenceName = refNameProp.stringValue;
				spinner.interactable = activateProp.boolValue;
				spinner.minInputFieldDimensions = minDimenProp.vector2Value;
				spinner.alignmentOptions = (TextAlignmentOptions)alignmentProp.enumValueFlag;

				spinner.minValue = minValueProp.intValue;
				spinner.maxValue = maxValueProp.intValue;

				if (oldValue != newValue)
				{
					spinner.value = newValue;
					valueProp.intValue = newValue;
				}

				spinner.value = valueProp.intValue;
				spinner.fontSize = fontSizeProp.floatValue;
				//spinner.Refresh(spinner.gameObject);
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
		private bool isLabelExFoldout = true;

		void OnEnable()
		{
			textLabel = serializedObject.FindProperty("textLabel");
			image = serializedObject.FindProperty("image");
			labelExProp = serializedObject.FindProperty("labelEx");

			label = (UIExpandingLabel)target;
			//labelEx = (LabelEx)(label).GetBackingData();

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

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var textProp = serializedObject.FindProperty("_text");
			EditorGUILayout.PropertyField(textProp);

			var fontStylesProp = serializedObject.FindProperty("_fontStyles");
			EditorGUILayout.PropertyField(fontStylesProp);

			var alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			EditorGUILayout.PropertyField(alignmentProp);

			var marginProp = serializedObject.FindProperty("_margin");
			EditorGUILayout.PropertyField(marginProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("_minLabelDimensions"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxLabelDimensions"));

			EditorGUILayout.PropertyField(textLabel);
			EditorGUILayout.PropertyField(image);
			if (isLabelExFoldout = EditorGUILayout.Foldout(isLabelExFoldout, "Label Data", true))
			{
				++EditorGUI.indentLevel;
				EditorGUILayout.PropertyField(labelExProp);
				--EditorGUI.indentLevel;
			}


			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				label.referenceName = refNameProp.stringValue;
				label.text = textProp.stringValue;
				label.fontStyles = (FontStyles)fontStylesProp.enumValueFlag;
				label.alignmentOptions = (TextAlignmentOptions)alignmentProp.enumValueFlag;
				label.margin = marginProp.vector4Value;
			}
		}
	}



	public class UIEditor<T> : Editor where T : MonoBehaviour, IUIBehavior
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();


			base.OnInspectorGUI();

			var interactableProp = serializedObject.FindProperty("_interactable");
			if (interactableProp != null)
				EditorGUILayout.PropertyField(interactableProp);

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				if (interactableProp != null)
					((UICheckBox)target).interactable = interactableProp.boolValue;

				T buttonPanel = (T)target;
				buttonPanel.UpdateBackingData();
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





	[Obsolete("Delete after fixing ship design")]
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
				case UI.DynamicPanel.TitleLabelStyle.Bar:
					EditorGUILayout.PropertyField(centerTitleText);
					break;

				case UI.DynamicPanel.TitleLabelStyle.SquareTab:
				case UI.DynamicPanel.TitleLabelStyle.BladedTab:
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



}