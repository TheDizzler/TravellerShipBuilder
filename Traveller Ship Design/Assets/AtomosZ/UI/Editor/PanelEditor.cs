using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEngine;
using Unity.VisualScripting;

using AtomosZ.EditorZ;
using static AtomosZ.UI.MagicWindow;
using Object = UnityEngine.Object;
using TMPro;
using static AtomosZ.UI.UIDataRow;

namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(MagicWindow))]
	public class MagicWindowEditor : Editor
	{
		private MagicWindow magicWindow;
		private RectTransform rect;

		private Dictionary<UIControlType, SerializedProperty> scriptableObjects;
		private Editor tabEditor;
		private Editor tabLabelEditor;
		private Editor panelEditor;
		private MagicWindow.WindowStyle prevWindowStyle;
		private Dictionary<UITabControlScriptableObject, UITabControlScriptableObjectEditor> tabScriptObjEditors = new();
		private bool isTabControlScriptObjFoldout;
		private bool isScriptObjFoldout;
		private bool isTitleBarFoldout;


		void OnEnable()
		{
			magicWindow = (MagicWindow)target;
			rect = magicWindow.GetComponent<RectTransform>();

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
				//[UIControlType.ImagePanel] = serializedObject.FindProperty("imageViewPanelScriptObj"),
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

					Editor.CreateCachedEditor(magicWindow.rootTabControl, typeof(UITabControlEditor), ref tabEditor);
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

						Editor.CreateCachedEditor(magicWindow.rootTabControl.tabPanels[0].tabLabel, typeof(UIExpandingLabelEditor), ref tabLabelEditor);
						tabLabelEditor.OnInspectorGUI();

						--EditorGUI.indentLevel;
					}

					Editor.CreateCachedEditor(magicWindow.rootTabControl.SelectedPanel(), typeof(UIPanelEditor), ref panelEditor);
					panelEditor.OnInspectorGUI();

					if (!tabScriptObjEditors.TryGetValue(magicWindow.rootTabControl.tabControlData, out var tabDataEditor)
						|| tabDataEditor == null)
					{
						tabDataEditor = (UITabControlScriptableObjectEditor)Editor.CreateEditor(magicWindow.rootTabControl.tabControlData);
						tabScriptObjEditors.Add(magicWindow.rootTabControl.tabControlData, tabDataEditor);
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
				magicWindow.RecordPrefabInstances();
				magicWindow.Refresh();
			}
		}
	}


	[CustomEditor(typeof(UITabControl))]
	public class UITabControlEditor : EditorEx
	{
		public UITabControl tabControl;
		private List<TabPanel> tabPanels;
		private int removeTabIndex;
		private Editor panelEditor;
		private static bool isTabEditFoldout = true;
		private bool isPanelEditFoldout = true;
		private bool isPanelFoldout;
		private Dictionary<UITabControlScriptableObject, UITabControlScriptableObjectEditor> tabScriptObjEditors = new();
		private bool isScriptObjFoldout;
		private bool isTabLabelExFoldout;
		private Editor tabLabelEditor;

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

			var tabConData = serializedObject.FindProperty("tabControlData");
			if (tabConData.boxedValue == null)
			{
				EditorGUILayout.LabelField("TABCONTROLDATA REQUIRED!");
				EditorGUILayout.PropertyField(tabConData);

				serializedObject.ApplyModifiedProperties();

				if (EditorGUI.EndChangeCheck())
				{
					tabControl.SetDirty();
				}
				return;
			}

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
			UIExpandingLabel selectedTab = selectedTabPanel.tabLabel;

			if (isPanelFoldout = EditorGUILayout.Foldout(isPanelFoldout,
				"Selected Tab: " + selectedTab.referenceName + " - Edit and Add Controls", true))
			{
				++indentLevel;

				if (isTabLabelExFoldout = EditorGUILayout.Foldout(isTabLabelExFoldout, "Tab Label Properties", true))
				{
					++indentLevel;

					Editor.CreateCachedEditor(selectedTabPanel.tabLabel, typeof(UIExpandingLabelEditor), ref tabLabelEditor);
					tabLabelEditor.OnInspectorGUI();

					--indentLevel;
				}

				Editor.CreateCachedEditor(selectedTabPanel.panel, typeof(UIPanelEditor), ref panelEditor);
				panelEditor.OnInspectorGUI();
				//if (((UIPanelEditor)panelEditor).isDeadEditor)
				//	lastSelectedIndex = -1;

				--indentLevel;
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

					if (GUILayout.Button("Remove " + tabPanels[removeTabIndex].tabLabel.name))
					{
						tabControl.RemoveTab(removeTabIndex);
						EditorUtility.SetDirty(tabControl);
					}

					GUI.enabled = true;
				}
				GUILayout.EndHorizontal();


				if (GUILayout.Button("New Tab on " + tabControl.referenceName, GUILayout.ExpandWidth(false)))
				{
					tabControl.AddTab();
					removeTabIndex = tabPanels.Count - 1;
					EditorUtility.SetDirty(tabControl);
				}

				GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));
			}

			if (isPanelEditFoldout = EditorGUILayout.Foldout(isPanelEditFoldout, "Panel Settings", true))
			{
				++EditorGUI.indentLevel;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("panelPrefab"));// this is sometimes null if the foldout is closed?

				var selectedPanel = selectedTabPanel.panel;
				Editor.CreateCachedEditor(selectedPanel, typeof(UIPanelEditor), ref panelEditor);
				panelEditor.OnInspectorGUI();

				--EditorGUI.indentLevel;
			}

			if (isTabEditFoldout = EditorGUILayout.Foldout(isTabEditFoldout, "Tab Control Settings", true))
			{
				++EditorGUI.indentLevel;

				EditorGUILayout.PropertyField(serializedObject.FindProperty("tabItemPrefab")); // this is sometimes null if the foldout is closed?

				EditorGUILayout.PropertyField(tabConData);

				if (isScriptObjFoldout = EditorGUILayout.Foldout(isScriptObjFoldout, "Tabcontrol scriptable object", true))
				{
					if (!tabScriptObjEditors.TryGetValue(tabControl.tabControlData, out var tabEditor)
						|| tabEditor == null)
					{
						tabEditor = (UITabControlScriptableObjectEditor)Editor.CreateEditor(tabControl.tabControlData);
						tabScriptObjEditors.Add(tabControl.tabControlData, tabEditor);
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
				tabControl.SetDirty();
			}

			//EditorGUILayout.Vector2Field("size:", tabControl.GetMinDimensions());
		}
	}


	[CustomEditor(typeof(UITabControlScriptableObject))]
	public class UITabControlScriptableObjectEditor : EditorEx
	{
		public override void OnInspectorGUI()
		{
			BeginChangeCheck();
			PropertyField(FindProperty("panelScriptableObj"));
			var windowStyleProp = FindProperty("windowStyle");
			PropertyField(windowStyleProp);
			var windowStyle = (MagicWindow.WindowStyle)windowStyleProp.enumValueIndex;
			switch (windowStyle)
			{
				case MagicWindow.WindowStyle.ContextMenu:
					break;

				case MagicWindow.WindowStyle.Tabbed:
				{
					PropertyField(FindProperty("titleBarFontSize"), new GUIContent("Tab Text Font Size"));
					PropertyField(FindProperty("titleBarFontColor"), new GUIContent("Tab Text Font Color"));
					PropertyField(FindProperty("tabTextAlignment"));
					PropertyField(FindProperty("titleBarSprites"), new GUIContent("Tab Sprites"));
					PropertyField(FindProperty("titleTextMargin"), new GUIContent("Tab text Margin"));
					PropertyField(FindProperty("titleBarMinSize"), new GUIContent("Tab min Size"));
					PropertyField(FindProperty("titleBarVerticalOffset"), new GUIContent("Tab Vertical Offset"));
					PropertyField(FindProperty("selectedTabColor"));
					PropertyField(FindProperty("deselectedTabColor"));
					PropertyField(FindProperty("disabledTabColor"));
					PropertyField(FindProperty("tabHorizontaloffset"));
					PropertyField(FindProperty("panelWidthAdjust"));

				}
				break;

				case MagicWindow.WindowStyle.TitleBar:
				{
					PropertyField(FindProperty("titleBarFontSize"));
					PropertyField(FindProperty("titleBarFontColor"));
					PropertyField(FindProperty("tabTextAlignment"));
					PropertyField(FindProperty("titleBarSprites"));
					PropertyField(FindProperty("titleTextMargin"));
					PropertyField(FindProperty("titleBarMinSize"));
					PropertyField(FindProperty("titleBarVerticalOffset"));
					PropertyField(FindProperty("selectedTabColor"), new GUIContent("TitleBar Color"));
				}
				break;
			}

			serializedObject.ApplyModifiedProperties();
			if (EndChangeCheck())
			{ }
		}
	}


	[CustomEditor(typeof(UIPanel))]
	public class UIPanelEditor : EditorEx
	{
		private Vector2 lastSize;
		private UIPanel panel;
		private RectTransform rect;
		private SerializedProperty minDimProp;
		private SerializedProperty borderlessProp;
		private SerializedProperty spriteProp;
		private SerializedProperty paddingProp;
		private SerializedProperty spacingProp;
		private SerializedProperty panelDataProp;
		private List<UIMonoBehaviour> uiControls;
		private Dictionary<UIMonoBehaviour, bool> isFoldout = new();

		private Editor tabEditor;
		private Editor spinnerEditor;
		private Editor sliderEditor;
		private Editor panelEditor;
		private Editor dropdownEditor;
		private Editor checkboxEditor;
		private Editor labelEditor;
		private Editor inputEditor;
		private Editor buttonEditor;
		private Editor panelDataEditor;
		private Editor buttonPanelEditor;
		private Editor imageEditor;

		private PanelControlType currentType;
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
			[UIControlType.Table] = "tableEx",
			//[UIControlType.Spinner] = "spinnerEx",
		};
		private bool isUIControlsFoldout = true;
		private bool isPanelDataFoldout = false;
		public bool isDeadEditor;
		private Editor tableEditor;
		private Editor dividerEditor;
		private Editor rowEditor;

		public void OnEnable()
		{
			panel = (UIPanel)target;
			rect = panel.GetComponent<RectTransform>();

			var _ = panel.sprite; // this is required to show the sprite in the editor

			minDimProp = serializedObject.FindProperty("_minDimensions");
			borderlessProp = serializedObject.FindProperty("_borderless");
			spriteProp = serializedObject.FindProperty("_sprite");
			paddingProp = serializedObject.FindProperty("_layoutPadding");
			spacingProp = serializedObject.FindProperty("_layoutSpacing");
			panelDataProp = serializedObject.FindProperty("panelData");


			uiControls = panel.GetControlsFromTransform_DEBUG();

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
			delButtonStyle.stretchWidth = false;
			delButtonStyle.fontStyle = FontStyle.BoldAndItalic;

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

			PropertyField(spriteProp);
			PropertyField(paddingProp);
			PropertyField(spacingProp);
			PropertyField(borderlessProp);
			PropertyField(minDimProp);

			var oldValue = (UIPanelScriptableObject)panelDataProp.boxedValue;
			//SODataDisplay<UIPanelScriptableObject, UIPanelScriptableObjectEditor>(
			//	"Panel scriptable object", buttonProp, ref buttonDataEditor, ref isPanelDataFoldout);
			//PropertyField(panelDataProp);

			//if (panelDataProp.boxedValue != null)
			//{
			//	if (this.CreateFoldout(ref isPanelDataFoldout, ))
			//	{
			CreateScriptObjectEditor<UIPanelScriptableObject, UIPanelEditor>(
				"Panel scriptable object", panelDataProp, oldValue,
				ref panelDataEditor, ref isPanelDataFoldout, panel, UpdateBackingData);
			//}
			//}

			this.CreateBorder(2);

			currentType = (PanelControlType)EditorGUILayout.EnumPopup("UI Control Type to add", currentType);


			if (GUILayout.Button($"Add {currentType} to {panel.referenceName} panel"))
			{
				UIMonoBehaviour uiBehave = null;
				switch (currentType)
				{
					case PanelControlType.Text:
						uiBehave = panel.AddText(null);
						break;

					case PanelControlType.Button:
						uiBehave = panel.AddButton(null);
						break;

					case PanelControlType.ButtonPanel:
						uiBehave = panel.AddButtonPanel(null);
						break;

					case PanelControlType.CheckBox:
						uiBehave = panel.AddCheckBox(null);
						break;

					case PanelControlType.Dropdown:
						uiBehave = panel.AddDropdown(null);
						break;

					case PanelControlType.Image:
						uiBehave = panel.AddImage(null);
						break;

					//case PanelControlType.imagepanel:
					//	uibehave = panel.addimagepane(null);
					//	break;

					case PanelControlType.InputField:
						uiBehave = panel.AddInputField(null);
						break;

					case PanelControlType.Slider:
						uiBehave = panel.AddSlider(null);
						break;

					case PanelControlType.TabControl:
						uiBehave = panel.AddTabControl();
						break;

					case PanelControlType.Spinner:
						uiBehave = panel.AddSpinner(null);
						break;

					case PanelControlType.HorizontalPanel:
						uiBehave = panel.AddHorizontalPanel(null);
						break;

					case PanelControlType.Panel:
						uiBehave = panel.AddPanel(null);
						break;

					case PanelControlType.MenuDivider:
						uiBehave = panel.AddDivider();
						break;

					case PanelControlType.Table:
						uiBehave = panel.AddTable();
						break;

					//case PanelControlType.:
					//	uiBehave = panel.AddUIControl(new Ex(null));
					//	break;

					default:
						Debug.LogWarning($"{currentType} not yet implemented");
						break;
				}

				if (uiBehave != null)
				{
					isFoldout.Add(uiBehave, true);
					EditorUtility.SetDirty(panel);
				}
			}

			GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

			if (this.CreateFoldout(ref isUIControlsFoldout, $"UI Controls attached to panel (control count: {uiControls.Count})"))
			{
				++EditorGUI.indentLevel;

				foreach (var uiControl in uiControls)
				{
					if (uiControl == null)
					{
						Debug.LogWarning("A control is null. Are we creating a new control?");
						continue;
					}

					SerializedObject uiSO = null;
					string referenceName = null;
					switch (uiControl.iUIBehavior.dataType)
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


						//case UIControlType.ImagePanel:
						//{
						//	var uiCtrl = (UIImageViewPanel)uiControl;
						//	uiSO = new SerializedObject(uiCtrl);
						//	referenceName = uiCtrl.referenceName;
						//}
						//break;

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

						case UIControlType.Table:
						{
							var uiCtrl = (UITable)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.MenuDivider:
						{
							var uiCtrl = (UIMenuDivider)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						case UIControlType.MenuButton:
						{
							var uiCtrl = (UIMenuButton)uiControl;
							uiSO = new SerializedObject(uiCtrl);
							referenceName = uiCtrl.referenceName;
						}
						break;

						default:
							Debug.LogWarning($"{uiControl.iUIBehavior.dataType} not yet implemented");
							continue;
					}

					EditorGUILayout.BeginHorizontal();
					{
						var labelText = $"{(uiControl.gameObject.activeSelf ? "" : "(hidden) ")}{uiControl.iUIBehavior.dataType.ToString()} - {referenceName}";
						isFoldout[uiControl] = EditorGUILayout.Foldout(isFoldout[uiControl], labelText, true);
						if (Button("Select"))
						{
							Selection.objects = new Object[] { uiControl.gameObject };
						}
						EditorGUILayout.EndHorizontal();
					}

					if (isFoldout[uiControl])
					{
						++EditorGUI.indentLevel;
						uiControl.gameObject.SetActive(EditorGUILayout.Toggle("Is Visible", uiControl.gameObject.activeSelf));

						switch (uiControl.iUIBehavior.dataType)
						{
							case UIControlType.TabControl:
							{
								Editor.CreateCachedEditor((UITabControl)uiControl, typeof(UITabControlEditor), ref tabEditor);
								tabEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Spinner:
							{
								Editor.CreateCachedEditor((UISpinner)uiControl, typeof(UISpinnerEditor), ref spinnerEditor);
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

							case UIControlType.Button:
							{
								Editor.CreateCachedEditor((UIButton)uiControl, typeof(UIButtonEditor), ref buttonEditor);
								buttonEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.ButtonPanel:
							{
								Editor.CreateCachedEditor((UIButtonPanel)uiControl, typeof(UIButtonPanelEditor), ref buttonPanelEditor);
								buttonPanelEditor.OnInspectorGUI();
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
								Editor.CreateCachedEditor((UIExpandingLabel)uiControl, typeof(UIExpandingLabelEditor), ref labelEditor);
								labelEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.InputField:
							{
								Editor.CreateCachedEditor((UIExpandingInputField)uiControl, typeof(UIExpandingInputFieldEditor), ref inputEditor);
								inputEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Image:
							{
								Editor.CreateCachedEditor((UIImageView)uiControl, typeof(UIImageViewEditor), ref imageEditor);
								imageEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Panel:
							case UIControlType.HorizontalPanel:
							{
								Editor.CreateCachedEditor((UIPanel)uiControl, typeof(UIPanelEditor), ref panelEditor);
								panelEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Table:
							{
								Editor.CreateCachedEditor((UITable)uiControl, typeof(UITableEditor), ref tableEditor);
								tableEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.DataRow:
							{
								Editor.CreateCachedEditor((UIDataRow)uiControl, typeof(UIDataRowEditor), ref rowEditor);
								rowEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.MenuDivider:
							{
								Editor.CreateCachedEditor((UIMenuDivider)uiControl, typeof(UIDividerEditor), ref dividerEditor);
								dividerEditor.OnInspectorGUI();
							}
							break;

							default:
							{
								EditorGUILayout.PropertyField(uiSO.FindProperty(propertyName[uiControl.iUIBehavior.dataType]));
							}
							break;
						}

						uiSO.ApplyModifiedProperties();

						GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						if (GUILayout.Button("Remove " + referenceName + " from " + target.name, delButtonStyle))
						{
							panel.RemoveControl(uiControl);
							isFoldout.Remove(uiControl);
							EditorUtility.SetDirty(panel);
							break;
						}
						GUILayout.EndHorizontal();

						GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));
						--EditorGUI.indentLevel;
					}
				}

				--EditorGUI.indentLevel;
			}

			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				panel.referenceName = refProp.stringValue;
				panel.minDimensions = minDimProp.vector2Value;
				panel.sprite = (Sprite)spriteProp.boxedValue;
				panel.layoutPadding = (RectOffset)paddingProp.GetUnderlyingValue();
				panel.layoutSpacing = spacingProp.floatValue;
				panel.borderless = borderlessProp.boolValue;
				lastSize = rect.sizeDelta;
				panel.RecordPrefabInstances();
			}

			//EditorGUILayout.Vector2Field("debug size:", panel.GetMinDimensions());
		}

		private void UpdateBackingData(UIPanelScriptableObject obj)
		{
			panel.UpdateBackingData(obj);
			minDimProp.vector2Value = obj.minDimensions;
			spriteProp.boxedValue = obj.backgroundSprite;
			paddingProp.SetUnderlyingValue(obj.layoutPadding);
			spacingProp.floatValue = obj.layoutSpacing;


			lastSize = rect.sizeDelta;
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




	//[CustomEditor(typeof(UIImageViewPanel))]
	//public class UIImageViewPanelEditor : Editor
	//{
	//	private UIImageViewPanel imageViewPanel;
	//	private ImageViewDataEx viewDataEx;
	//	private Dictionary<ImageEx, UIImageView> images = new();
	//	private bool imagesVisible = true;
	//	private SerializedObject gridLayoutSO;
	//	private SerializedObject scrollRectSO;
	//	private SerializedObject contentRectSO;

	//	public void OnEnable()
	//	{
	//		imageViewPanel = (UIImageViewPanel)target;
	//		viewDataEx = (ImageViewDataEx)imageViewPanel.GetBackingData();
	//		gridLayoutSO = new SerializedObject(imageViewPanel.gridLayout);
	//		scrollRectSO = new SerializedObject(imageViewPanel.scrollRect);
	//		contentRectSO = new SerializedObject(imageViewPanel.scrollRect.content);
	//		BuildImageList();
	//	}

	//	private void BuildImageList()
	//	{
	//		var imageViews = imageViewPanel.GetComponentsInChildren<UIImageView>();
	//		images.Clear();
	//		foreach (var imageView in imageViews)
	//		{
	//			images.Add((ImageEx)imageView.GetBackingData(), imageView);
	//		}

	//		imageViewPanel.images = images;
	//	}

	//	public override void OnInspectorGUI()
	//	{
	//		EditorGUI.BeginChangeCheck();
	//		base.OnInspectorGUI();
	//		var maxPanelSize = viewDataEx.scriptableObj.maxPanelSize;
	//		var imageSize = viewDataEx.scriptableObj.imageSize;
	//		int imagesOnRow = Mathf.FloorToInt(
	//			(maxPanelSize.x - (imageViewPanel.gridLayout.padding.left + imageViewPanel.gridLayout.padding.right))
	//			/ (imageSize.x + imageViewPanel.gridLayout.spacing.x));
	//		var minWidthReq = imageSize.x * imagesOnRow + imageViewPanel.gridLayout.spacing.x * (imagesOnRow - 1);

	//		GUILayout.BeginHorizontal();
	//		EditorGUILayout.FloatField("min panel width", minWidthReq);
	//		EditorGUILayout.IntField("images on row", imagesOnRow);

	//		GUILayout.EndHorizontal();

	//		if (EditorGUI.EndChangeCheck())
	//		{
	//			imageViewPanel.UpdateBackingData();
	//			Debug.LogWarning("label min/max dimensions have been removed from LabelEx");
	//			//imageViewPanel.gridLayout.cellSize = new Vector2(
	//			//	Mathf.Max(imageSize.x, viewDataEx.labelEx.maxLabelDimensions.x),
	//			//	imageSize.y + viewDataEx.labelEx.maxLabelDimensions.y); // will need to figure this out
	//			gridLayoutSO.ApplyModifiedProperties();

	//			contentRectSO.ApplyModifiedProperties();
	//		}

	//		if (GUILayout.Button("Add Image"))
	//		{
	//			imageViewPanel.AddImage(new ImageEx(viewDataEx.scriptableObj.imageViewData)
	//			{
	//				sprite = viewDataEx.scriptableObj.defaultSprite,
	//			});
	//		}

	//		imagesVisible = EditorGUILayout.BeginFoldoutHeaderGroup(imagesVisible, "Images");
	//		if (imagesVisible)
	//		{
	//			ImageEx remove = null;
	//			foreach (var image in images)
	//			{
	//				var rect = EditorGUILayout.BeginHorizontal();
	//				var newSprite = (Sprite)EditorGUILayout.ObjectField(image.Key.sprite, typeof(Sprite), false);
	//				var newText = EditorGUILayout.TextField(image.Value.text);
	//				EditorGUILayout.EndHorizontal();

	//				if (newSprite != image.Key.sprite || newText != image.Value.text)
	//				{
	//					image.Key.sprite = newSprite;
	//					image.Value.text = newText;
	//					image.Value.UpdateBackingData();
	//				}

	//				if (GUILayout.Button("Remove"))
	//				{
	//					remove = image.Key;

	//				}

	//				GUILayout.Space(15);
	//			}

	//			if (remove != null)
	//			{
	//				imageViewPanel.RemoveImage(remove);
	//			}
	//		}
	//	}

	//	//public void OnSceneGUI()
	//	//{
	//	//	var size = rect.sizeDelta;
	//	//	if (size != lastSize)
	//	//	{
	//	//		imageViewPanel.UpdateBackingData();
	//	//		lastSize = size;
	//	//	}
	//	//}
	//}


	[CustomEditor(typeof(UIButtonPanel))]
	public class UIButtonPanelEditor : EditorEx
	{
		private UIButtonPanel buttonPanel;

		private void OnEnable()
		{
			buttonPanel = (UIButtonPanel)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = FindProperty("_referenceName");
			PropertyField(refNameProp);

			var activateProp = FindProperty("_interactable");
			PropertyField(activateProp);

			var buttonsProp = FindProperty("_buttons");
			PropertyField(buttonsProp);

			if (buttonPanel.buttons == UIButtonPanel.DialogButton.OKCancel
				|| buttonPanel.buttons == UIButtonPanel.DialogButton.OK)
				PropertyField(FindProperty("okButton"));
			if (buttonPanel.buttons == UIButtonPanel.DialogButton.YesNoCancel
				|| buttonPanel.buttons == UIButtonPanel.DialogButton.YesNoCancel)
			{
				PropertyField(FindProperty("yesButton"));
				PropertyField(FindProperty("noButton"));
			}

			if (buttonPanel.buttons == UIButtonPanel.DialogButton.OKCancel
				|| buttonPanel.buttons == UIButtonPanel.DialogButton.YesNoCancel)
				PropertyField(FindProperty("cancelButton"));

			var fillParentProp = FindProperty("_fillParentHorizontal");
			PropertyField(fillParentProp);

			var dataProp = FindProperty("buttonPanelData");
			var oldData = dataProp.boxedValue;
			PropertyField(dataProp);

			bool refreshPanelData = false;
			if (dataProp.boxedValue != null)
				refreshPanelData = GUILayout.Button("Reset to scriptable object settings");

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				buttonPanel.referenceName = refNameProp.stringValue;
				buttonPanel.interactable = activateProp.boolValue;
				buttonPanel.buttons = (UI.UIButtonPanel.DialogButton)buttonsProp.enumValueFlag;
				buttonPanel.fillParentHorizontal = fillParentProp.boolValue;
				if (refreshPanelData || oldData != dataProp.boxedValue)
					buttonPanel.UpdateBackingData((UIButtonPanelScriptableObject)dataProp.boxedValue);
			}
		}
	}

	[CustomEditor(typeof(UICheckBox))]
	public class UICheckBoxEditor : EditorEx
	{
		private UICheckBox checkbox;
		private SerializedProperty fontColorProp;
		private SerializedProperty marginProp;
		private SerializedProperty boxSpriteProp;
		private SerializedProperty checkSpriteProp;
		private SerializedProperty fontStylesProp;
		private SerializedProperty disabledFontProp;
		private SerializedProperty minLabelProp;
		private SerializedProperty maxLabelProp;
		private Editor checkboxDataEditor;
		private bool isDataFoldout;

		private void OnEnable()
		{
			checkbox = (UICheckBox)target;

			fontColorProp = serializedObject.FindProperty("_fontColor");
			marginProp = serializedObject.FindProperty("_margin");
			boxSpriteProp = serializedObject.FindProperty("_boxSprite");
			checkSpriteProp = serializedObject.FindProperty("_checkSprite");
			fontStylesProp = serializedObject.FindProperty("_fontStyles");
			disabledFontProp = serializedObject.FindProperty("_disabledFontColor");

			minLabelProp = serializedObject.FindProperty("_minLabelDimensions");
			maxLabelProp = serializedObject.FindProperty("_maxLabelDimensions");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var activateProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(activateProp);
			EditorGUILayout.PropertyField(boxSpriteProp);
			EditorGUILayout.PropertyField(checkSpriteProp);

			var textProp = serializedObject.FindProperty("_text");
			EditorGUILayout.PropertyField(textProp);
			EditorGUILayout.PropertyField(fontColorProp);
			EditorGUILayout.PropertyField(disabledFontProp);
			EditorGUILayout.PropertyField(fontStylesProp);

			var alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			EditorGUILayout.PropertyField(alignmentProp);
			EditorGUILayout.PropertyField(marginProp);

			EditorGUILayout.PropertyField(minLabelProp);
			EditorGUILayout.PropertyField(maxLabelProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("onCheckChangedEvent"));

			var dataProp = serializedObject.FindProperty("checkBoxData");
			var oldData = (UICheckBoxScriptableObject)dataProp.boxedValue;
			CreateScriptObjectEditor<UICheckBoxScriptableObject, UICheckBoxScriptableObjectEditor>(
				"Panel scriptable object", dataProp, oldData,
				ref checkboxDataEditor, ref isDataFoldout, checkbox, UpdateBackingData);
			//EditorGUILayout.PropertyField(dataProp);
			//if (dataProp.boxedValue != null)
			//{
			//	if (this.CreateFoldout(ref isDataFoldout, "Checkbox ScriptableObject Data"))
			//	{
			//		this.CreateScriptObjectEditor(typeof(UICheckBoxScriptableObjectEditor),
			//			oldData, (UICheckBoxScriptableObject)dataProp.boxedValue,
			//			ref checkboxDataEditor, checkbox, UpdateBackingData);
			//	}
			//}


			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				checkbox.referenceName = refNameProp.stringValue;
				checkbox.text = textProp.stringValue;
				checkbox.fontStyles = (FontStyles)fontStylesProp.enumValueFlag;
				checkbox.alignmentOptions = (TextAlignmentOptions)alignmentProp.enumValueFlag;
				checkbox.margin = marginProp.vector4Value;
				checkbox.boxSprite = (Sprite)boxSpriteProp.boxedValue;
				checkbox.checkSprite = (Sprite)checkSpriteProp.boxedValue;
				checkbox.interactable = activateProp.boolValue;

				checkbox.minLabelDimensions = minLabelProp.vector2Value;
				checkbox.maxLabelDimensions = maxLabelProp.vector2Value;

				checkbox.fontColor = fontColorProp.colorValue;
				checkbox.disabledFontColor = disabledFontProp.colorValue;
			}
		}

		private void UpdateBackingData(UICheckBoxScriptableObject checkboxData)
		{
			checkbox.UpdateBackingData(checkboxData);
			boxSpriteProp.boxedValue = checkboxData.boxSprite;
			checkSpriteProp.boxedValue = checkboxData.checkSprite;
			if (checkboxData.labelData != null)
			{
				fontStylesProp.boxedValue = checkboxData.labelData.fontStyles;
				marginProp.vector4Value = checkboxData.labelData.textMargin;
				fontColorProp.boxedValue = checkboxData.labelData.fontColor;
				disabledFontProp.boxedValue = checkboxData.labelData.disabledColor;
			}
		}
	}


	[CustomEditor(typeof(UIExpandingInputField))]
	public class UIExpandingInputFieldEditor : EditorEx
	{
		private UIExpandingInputField inputField;
		private SerializedProperty fontAssetProp;
		private SerializedProperty fontColorProp;
		private SerializedProperty placeholderFontColorProp;
		private SerializedProperty fontSizeProp;
		private SerializedProperty dataProp;
		private SerializedProperty alignmentProp;
		private SerializedProperty fillParentProp;
		private SerializedProperty minDimProp;
		private SerializedProperty maxDimProp;
		//private SerializedProperty fieldDimenProp;
		private Editor inputFieldDataEditor;
		private bool isDataFoldout;
		private Editor dataEditor;

		private void OnEnable()
		{
			inputField = (UIExpandingInputField)target;

			fontAssetProp = serializedObject.FindProperty("_fontAsset");
			fontColorProp = serializedObject.FindProperty("_fontColor");
			placeholderFontColorProp = serializedObject.FindProperty("_placeholderFontColor");
			fontSizeProp = serializedObject.FindProperty("_fontSize");
			dataProp = serializedObject.FindProperty("inputFieldData");


			alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			fillParentProp = serializedObject.FindProperty("_fillParentHorizontal");
			//fieldDimenProp = serializedObject.FindProperty("_fieldDimensions");
			minDimProp = serializedObject.FindProperty("_minDimensions");
			maxDimProp = serializedObject.FindProperty("_maxDimensions");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var activateProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(activateProp);

			var textProp = serializedObject.FindProperty("_text");
			EditorGUILayout.PropertyField(textProp);
			var placeholderTextProp = serializedObject.FindProperty("_placeholderText");
			EditorGUILayout.PropertyField(placeholderTextProp);


			EditorGUILayout.PropertyField(fontAssetProp);
			EditorGUILayout.PropertyField(fontColorProp);
			EditorGUILayout.PropertyField(placeholderFontColorProp);
			EditorGUILayout.PropertyField(fontSizeProp);

			EditorGUILayout.PropertyField(alignmentProp);


			EditorGUILayout.PropertyField(fillParentProp);
			EditorGUILayout.PropertyField(minDimProp);
			EditorGUILayout.PropertyField(maxDimProp);
			//EditorGUILayout.PropertyField(fieldDimenProp);


			var oldData = (UIExpandingInputFieldScriptableObject)dataProp.boxedValue;
			CreateScriptObjectEditor<UIExpandingInputFieldScriptableObject, UIExpandingInputFieldEditor>(
				"ScriptableObject Data", dataProp, oldData,
				ref dataEditor, ref isDataFoldout, inputField, UpdateBackingData);
			//EditorGUILayout.PropertyField(fieldDataProp);
			//if (fieldDataProp.boxedValue != null)
			//{
			//if (this.CreateFoldout(ref isDataFoldout, "ScriptableObject Data"))
			//	{
			//this.CreateScriptObjectEditor(typeof(UIExpandingInputFieldScriptableObjectEditor),
			//	oldValue, (UIExpandingInputFieldScriptableObject)fieldDataProp.boxedValue,
			//	ref dataEditor, inputField, UpdataBackingData);
			//	}
			//}

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				inputField.referenceName = refNameProp.stringValue;
				inputField.interactable = activateProp.boolValue;
				inputField.placeholderText = placeholderTextProp.stringValue;
				inputField.text = textProp.stringValue;
				inputField.fontAsset = (TMP_FontAsset)fontAssetProp.boxedValue;
				inputField.fontColor = fontColorProp.colorValue;
				inputField.placeholderFontColor = placeholderFontColorProp.colorValue;
				inputField.fontSize = fontSizeProp.floatValue;
				inputField.alignmentOptions = (TextAlignmentOptions)alignmentProp.enumValueFlag;
				inputField.fillParentHorizontal = fillParentProp.boolValue;
				inputField.minDimensions = minDimProp.vector2Value;
				inputField.maxDimensions = maxDimProp.vector2Value;
				//inputField.fieldDimensions = fieldDimenProp.vector2Value;
			}
		}

		private void UpdateBackingData(UIExpandingInputFieldScriptableObject fieldData)
		{
			inputField.UpdateBackingData(fieldData);
			fontAssetProp.boxedValue = fieldData.fontAsset;
			//fontStylesProp.enumValueFlag = (int)fieldData.fontStyles;
			fontSizeProp.floatValue = fieldData.fontSize;
			fontColorProp.colorValue = fieldData.fontColor;
			placeholderFontColorProp.colorValue = fieldData.placeholderFontColor;
			//marginProp.vector4Value = fieldData.textMargin;
		}
	}



	[CustomEditor(typeof(UIDataCell))]
	public class UIDataCellEditor : EditorEx
	{
		private UIDataCell cell;

		private void OnEnable()
		{
			cell = (UIDataCell)target;
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				cell.UpdateBackingData_EDITOR();
			}
		}

	}

	[CustomEditor(typeof(UIDataRow))]
	public class UIDataRowEditor : EditorEx
	{
		private UIDataRow row;
		private Editor spinnerEditor;
		private Editor sliderEditor;
		private Editor dropdownEditor;
		private Editor buttonEditor;
		private Editor checkboxEditor;
		private Editor labelEditor;
		private Editor inputEditor;
		private Editor imageEditor;
		private Editor panelEditor;
		private UICellDataTypes currentType;

		private Dictionary<UIMonoBehaviour, bool> foldOuts = new();

		private void OnEnable()
		{
			row = (UIDataRow)target;
			row.RefreshControlsFormTransform_DEBUG();
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			for (int i = 0; i < row.cells.Length; ++i)
			{
				GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));
				if (row.cells[i].control != null)
				{
					if (!foldOuts.TryGetValue(row.cells[i].control, out bool isFoldOut))
						foldOuts.Add(row.cells[i].control, true);

					EditorGUILayout.BeginHorizontal();
					{
					isFoldOut = this.CreateFoldout(ref isFoldOut,
						$"Cell {i}: {row.cells[i].control.iUIBehavior.dataType} - {row.cells[i].control.referenceName}");
					foldOuts[row.cells[i].control] = isFoldOut;
						if (Button("Select"))
						{
							Selection.objects = new Object[] { row.cells[i].control.gameObject };
						}
					}
					EditorGUILayout.EndHorizontal();

					if (isFoldOut)
					{
						GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));
						++indentLevel;
						SerializedObject uiSO = null;

						var uiControl = row.cells[i].control;
						uiSO = new SerializedObject(uiControl);
						uiControl.gameObject.SetActive(EditorGUILayout.Toggle("Is Visible", uiControl.gameObject.activeSelf));
						switch (uiControl.iUIBehavior.dataType)
						{
							case UIControlType.Spinner:
							{
								Editor.CreateCachedEditor((UISpinner)uiControl, typeof(UISpinnerEditor), ref spinnerEditor);
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

							case UIControlType.Button:
							{
								Editor.CreateCachedEditor((UIButton)uiControl, typeof(UIButtonEditor), ref buttonEditor);
								buttonEditor.OnInspectorGUI();
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
								var uiCtrl = (UIExpandingLabel)uiControl;
								//uiSO = new SerializedObject(uiCtrl);
								Editor.CreateCachedEditor(uiCtrl, typeof(UIExpandingLabelEditor), ref labelEditor);
								labelEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.InputField:
							{
								Editor.CreateCachedEditor((UIExpandingInputField)uiControl, typeof(UIExpandingInputFieldEditor), ref inputEditor);
								inputEditor.OnInspectorGUI();
							}
							break;

							case UIControlType.Image:
							{
								Editor.CreateCachedEditor((UIImageView)uiControl, typeof(UIImageViewEditor), ref imageEditor);
								imageEditor.OnInspectorGUI();
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
								//EditorGUILayout.PropertyField(uiSO.FindProperty(propertyName[uiControl.iUIBehavior.dataType]));
								Debug.LogWarning($"{uiControl.iUIBehavior.dataType} is not supported in a UIDataCell");
							}
							break;
						}

						uiSO.ApplyModifiedProperties();

						GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

						GUILayout.BeginHorizontal();
						GUILayout.FlexibleSpace();
						GUIStyle delButtonStyle = new GUIStyle(GUI.skin.button);
						delButtonStyle.normal.textColor = Color.red;
						delButtonStyle.stretchWidth = false;
						delButtonStyle.fontStyle = FontStyle.BoldAndItalic;
						if (GUILayout.Button("Remove " + uiControl.referenceName + " from " + target.name, delButtonStyle))
						{
							foldOuts.Remove(row.cells[i].control);
							row.RemoveControl(i);
							EditorUtility.SetDirty(row);
						}
						GUILayout.EndHorizontal();
						GUILayout.Box(GUIContent.none, GUILayout.Width(Screen.width), GUILayout.Height(2));

						--indentLevel;
					}
				}
				else
				{
					BeginHorizontal();
					currentType = (UICellDataTypes)EditorGUILayout.EnumPopup($"Cell {i}: Select Control to add", currentType);


					if (GUILayout.Button($"Add {currentType} to {row.cells[i].referenceName}"))
					{
						UIMonoBehaviour uiBehave = row.SetControl(i, currentType);

						if (uiBehave != null)
						{
							EditorUtility.SetDirty(row);
						}
					}

					EndHorizontal();
				}
			}

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				row.UpdateBackingData_EDITOR();
			}
		}
	}

	[CustomEditor(typeof(UITable))]
	public class UITableEditor : EditorEx
	{
		private UITable table;
		private Editor dataRowEditor;
		private bool isRowsFoldout;
		private Dictionary<int, bool> rowFoldouts = new();

		private void OnEnable()
		{
			table = (UITable)target;
			table.RefreshControlsFormTransform_DEBUG();

			var d = table.headerHeight;
		}

		public override void OnInspectorGUI()
		{
			GUIStyle delButtonStyle = new GUIStyle(GUI.skin.button);
			delButtonStyle.normal.textColor = Color.red;
			delButtonStyle.stretchWidth = false;
			delButtonStyle.fontStyle = FontStyle.BoldAndItalic;

			EditorGUI.BeginChangeCheck();
			if (Button("Create Header"))
				table.CreateHeaderRow();
			base.OnInspectorGUI();

			this.CreateLabel($"Rows ({table.rows.Length})");
			{
				this.CreateBorder(2);
				++indentLevel;
				for (int i = 0; i < table.rows.Length; ++i)
				{
					var row = table.rows[i];

					if (!rowFoldouts.TryGetValue(i, out bool isRowFoldout))
					{
						rowFoldouts.Add(i, true);
						isRowFoldout = i == 0 ? true : false;
					}

					if (rowFoldouts[i] = this.CreateFoldout(ref isRowFoldout, $"Row {i}"))
					{
						++indentLevel;
						Editor.CreateCachedEditor(row, typeof(UIDataRowEditor), ref dataRowEditor);
						dataRowEditor.OnInspectorGUI();

						if (GUILayout.Button("Remove " + row.referenceName + " from " + target.name, delButtonStyle))
						{
							table.RemoveRow(i);
						}
						--indentLevel;
					}

				}
				--indentLevel;
			}


			if (Button("Add Row"))
				table.AddRow();

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				table.UpdateBackingData_EDITOR();
			}
		}
	}

	[CustomEditor(typeof(UIMenuDivider))]
	public class UIDividerEditor : EditorEx
	{
		private UIMenuDivider divider;
		private void OnEnable()
		{
			divider = (UIMenuDivider)target;
			//dataProp = serializedObject.FindProperty("imageData");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{

			}
		}
	}

	[CustomEditor(typeof(UIImageView))]
	public class UIImageViewEditor : EditorEx
	{
		private UIImageView image;
		private SerializedProperty dataProp;
		private Editor imageViewDataEditor;

		private void OnEnable()
		{
			image = (UIImageView)target;
			dataProp = serializedObject.FindProperty("imageData");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			var refNameProp = FindProperty("_referenceName");
			PropertyField(refNameProp);

			//var activateProp = FindProperty("_interactable");
			//PropertyField(activateProp);

			var minDimenProp = FindProperty("_minDimensions");
			PropertyField(minDimenProp);

			var showImageProp = FindProperty("_showImage");
			PropertyField(showImageProp);

			var spriteProp = FindProperty("_sprite");
			PropertyField(spriteProp);

			var showCaptionProp = FindProperty("_showCaption");
			PropertyField(showCaptionProp);


			var textProp = FindProperty("_text");
			PropertyField(textProp);


			var oldData = dataProp.boxedValue;
			EditorGUILayout.PropertyField(dataProp);
			if (oldData != dataProp.boxedValue)
			{
				image.UpdateBackingData((ScriptableObject)dataProp.boxedValue);
			}

			if (dataProp.boxedValue != null)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				var imageData = (UIImageViewScriptableObject)dataProp.boxedValue;
				if (GUILayout.Button($"Reset To ScriptableObject data", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
				{
					image.UpdateBackingData(imageData);

					if (imageData.labelData != null)
					{
					}
				}
				EditorGUILayout.EndHorizontal();

				GUI.enabled = false;
				++EditorGUI.indentLevel;
				Editor.CreateCachedEditor(imageData, typeof(UIImageViewScriptableObjectEditor), ref imageViewDataEditor);
				imageViewDataEditor.OnInspectorGUI();
				--EditorGUI.indentLevel;
				GUI.enabled = true;
			}

			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				//image.interactable = activateProp.boolValue;
				image.referenceName = refNameProp.stringValue;
				image.minDimensions = minDimenProp.vector2Value;
				image.sprite = (Sprite)spriteProp.boxedValue;
				image.showImage = showImageProp.boolValue;
				image.showCaption = showCaptionProp.boolValue;
				image.text = textProp.stringValue;
				//image.fillParentHorizontal = fillProp.boolValue;
			}
		}
	}



	[CustomEditor(typeof(UIDropdown))]
	public class UIDropdownEditor : EditorEx
	{
		private UIDropdown dropdown;
		private Editor dropboxDataEditor;
		private SerializedProperty optionsProp;
		private SerializedProperty arrowProp;
		private SerializedProperty fontColorProp;

		private void OnEnable()
		{
			dropdown = (UIDropdown)target;
			var _ = dropdown.arrowSprite; // this is required to show the sprite in the editor properly

			optionsProp = serializedObject.FindProperty("_options");
			arrowProp = serializedObject.FindProperty("_arrowSprite");
			fontColorProp = serializedObject.FindProperty("_fontColor");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			EditorGUILayout.PropertyField(serializedObject.FindProperty("arrow"));

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var activateProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(activateProp);

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

			var arrowProp = serializedObject.FindProperty("_arrowSprite");
			EditorGUILayout.PropertyField(arrowProp);


			var fontColorProp = serializedObject.FindProperty("_fontColor");
			EditorGUILayout.PropertyField(fontColorProp);

			var alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			EditorGUILayout.PropertyField(alignmentProp);


			var fillProp = serializedObject.FindProperty("_fillParentHorizontal");
			EditorGUILayout.PropertyField(fillProp);

			var minDimenProp = serializedObject.FindProperty("_minDimensions");
			EditorGUILayout.PropertyField(minDimenProp);

			var dataProp = serializedObject.FindProperty("dropdownData");
			var oldData = dataProp.boxedValue;
			EditorGUILayout.PropertyField(dataProp);
			if (oldData != dataProp.boxedValue)
			{
				dropdown.UpdateBackingData((UIDropdownScriptableObject)dataProp.boxedValue);
			}

			if (dataProp.boxedValue != null)
			{
				EditorGUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
				var dropdownData = (UIDropdownScriptableObject)dataProp.boxedValue;
				if (GUILayout.Button($"Reset To ScriptableObject data", EditorStyles.miniButtonRight, GUILayout.ExpandWidth(false)))
				{
					dropdown.UpdateBackingData(dropdownData);
					arrowProp.boxedValue = dropdownData.arrowSprite;
					if (dropdownData.labelData != null)
					{
						//fontStylesProp.boxedValue = dropdownData.labelData.fontStyles;
						//marginProp.vector4Value = dropdownData.labelData.textMargin;
						fontColorProp.boxedValue = dropdownData.labelData.fontColor;
						//disabledFontProp.boxedValue = dropdownData.labelData.disabledColor;
					}
				}
				EditorGUILayout.EndHorizontal();

				GUI.enabled = false;
				++EditorGUI.indentLevel;
				Editor.CreateCachedEditor(dropdownData, typeof(UIDropdownScriptableObjectEditor), ref dropboxDataEditor);
				dropboxDataEditor.OnInspectorGUI();
				--EditorGUI.indentLevel;
				GUI.enabled = true;
			}

			EditorGUILayout.PropertyField(serializedObject.FindProperty("onValueChangedAction"));




			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				dropdown.interactable = activateProp.boolValue;
				dropdown.UpdateOptionsDelegate();
				dropdown.referenceName = refNameProp.stringValue;
				dropdown.minDimensions = minDimenProp.vector2Value;
				dropdown.fillParentHorizontal = fillProp.boolValue;
				dropdown.value = newValue;
				valueProp.intValue = newValue;

				dropdown.isMultiSelect = newMultiValue;
				isMultiSelectProp.boolValue = newMultiValue;
			}
		}
	}


	[CustomEditor(typeof(UIButton))]
	public class UIButtonEditor : EditorEx
	{
		private UIButton button;
		private SerializedProperty buttonProp;
		private bool isButtonExFoldout;
		private Editor labelEditor;
		private bool isLabelEditFoldout;
		private Editor buttonDataEditor;

		private void OnEnable()
		{
			button = (UIButton)target;
			var _ = button.sprite; // this is required to show the sprite in the editor
			buttonProp = serializedObject.FindProperty("buttonData");
		}


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();

			var refNameProp = serializedObject.FindProperty("_referenceName");
			EditorGUILayout.PropertyField(refNameProp);

			var activateProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(activateProp);

			var minSizeProp = Property("_minButtonSize");

			var spriteProp = serializedObject.FindProperty("_sprite");
			EditorGUILayout.PropertyField(spriteProp);

			var spriteColorProp = FindProperty("_spriteColor");
			PropertyField(spriteColorProp);

			var spriteIsBGProp = serializedObject.FindProperty("_spriteIsBackground");
			EditorGUILayout.PropertyField(spriteIsBGProp);

			var fillProp = serializedObject.FindProperty("_fillParentHorizontal");
			EditorGUILayout.PropertyField(fillProp);

			EditorGUILayout.PropertyField(serializedObject.FindProperty("onClickedEvent"));


			var hideTextProp = serializedObject.FindProperty("_hideText");
			EditorGUILayout.PropertyField(hideTextProp);

			if (!hideTextProp.boolValue)
			{
				if (isLabelEditFoldout = EditorGUILayout.Foldout(isLabelEditFoldout, "Text Label", true))
				{
					++EditorGUI.indentLevel;
					//GUI.enabled = false;
					var textProp = serializedObject.FindProperty("textLabel");

					//GUI.enabled = true;
					EditorGUILayout.PropertyField(textProp);
					if (button.textLabel != null)
					{
						++EditorGUI.indentLevel;
						Editor.CreateCachedEditor(button.textLabel, typeof(UIExpandingLabelEditor), ref labelEditor);
						labelEditor.OnInspectorGUI();
						--EditorGUI.indentLevel;
					}
					--EditorGUI.indentLevel;
				}
			}

			object oldButtonData = buttonProp.boxedValue;
			SODataDisplay<UIButtonScriptableObject, UIButtonScriptableObjectEditor>(
				"Button Data", buttonProp, ref buttonDataEditor, ref isButtonExFoldout);


			serializedObject.ApplyModifiedProperties();
			if (EditorGUI.EndChangeCheck())
			{
				if (button.textLabel == null)
				{
					Debug.LogError("A textLabel is required");
					return;
				}

				button.interactable = activateProp.boolValue;
				button.minButtonSize = minSizeProp.vector2Value;
				button.referenceName = refNameProp.stringValue;
				button.sprite = (Sprite)spriteProp.boxedValue;
				button.spriteColor = spriteColorProp.colorValue;
				button.fillParentHorizontal = fillProp.boolValue;
				button.spriteIsBackground = spriteIsBGProp.boolValue;
				button.hideText = hideTextProp.boolValue;
				//button.text = textProp.stringValue;
				if (oldButtonData != buttonProp.boxedValue)
				{
					button.UpdateBackingData((UIButtonScriptableObject)buttonProp.boxedValue);
				}
			}
		}
	}


	[CustomEditor(typeof(UISlider))]
	public class UISliderEditor : EditorEx
	{
		public UISlider slider;
		private RectTransform rect;
		private Vector2 lastSize;
		private SerializedProperty showHandleProp;
		private SerializedProperty handleSpriteProp;
		private SerializedProperty handleOffsetProp;
		private SerializedProperty showUnitsProp;
		private SerializedProperty unitSpanProp;
		private Editor dataEditor;
		private bool isDataFoldout;

		void OnEnable()
		{
			slider = (UISlider)target;
			rect = slider.GetComponent<RectTransform>();
			lastSize = rect.sizeDelta;

			showHandleProp = serializedObject.FindProperty("_showHandle");
			handleSpriteProp = serializedObject.FindProperty("_handleSprite");
			handleOffsetProp = serializedObject.FindProperty("_handleOffset");
			showUnitsProp = serializedObject.FindProperty("_showUnits");
			unitSpanProp = serializedObject.FindProperty("_unitSpan");

			if (slider.transform.parent != null)
				slider.RecalculateDimensions();
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
			var fontSizeProp = serializedObject.FindProperty("_fontSize");
			var fontColorProp = serializedObject.FindProperty("_fontColor");
			var unitOffsetProp = serializedObject.FindProperty("_unitVerticalOffset");



			EditorGUILayout.PropertyField(showUnitsProp);
			if (showUnitsProp.boolValue)
			{
				EditorGUILayout.PropertyField(unitSpanProp);
				EditorGUILayout.PropertyField(fontSizeProp);
				EditorGUILayout.PropertyField(fontColorProp);
				EditorGUILayout.PropertyField(unitOffsetProp);
			}



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

			var dataProp = this.FindProperty("sliderData");
			var oldData = (UISliderScriptableObject)dataProp.boxedValue;
			CreateScriptObjectEditor<UISliderScriptableObject, UISliderEditor>(
				"ScriptableObject Data", dataProp, oldData, ref dataEditor, ref isDataFoldout, slider, UpdateBackingData);
			//EditorGUILayout.PropertyField(sliderDataProp);
			//if (sliderDataProp.boxedValue != null)
			//{
			//if (this.CreateFoldout(ref isDataFoldout, "Scriptable Label Data"))
			//	{
			//this.CreateScriptObjectEditor(typeof(UIExpandingLabelScriptableObjectEditor),
			//oldData, (UISliderScriptableObject)dataProp.boxedValue, ref dataEditor, slider, UpdateBackingData);
			//	}
			//}

			GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("rectTransform"));
			// debug stuff
			EditorGUILayout.PropertyField(serializedObject.FindProperty("size"), new GUIContent("Size (debug)"));
			GUI.enabled = true;

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

		private void UpdateBackingData(UISliderScriptableObject sliderData)
		{
			slider.UpdateBackingData(sliderData);

			showHandleProp.boolValue = sliderData.showHandle;
			handleSpriteProp.boxedValue = sliderData.handleSprite;
			handleOffsetProp.vector2Value = sliderData.handleOffset;
			showUnitsProp.boolValue = sliderData.showUnits;
			unitSpanProp.floatValue = sliderData.unitSpan;
			EditorUtility.SetDirty(slider);
			slider.RecalculateDimensions();
			slider.RecordPrefabInstances();

		}

		public void OnSceneGUI()
		{
			var size = rect.sizeDelta;
			if (size != lastSize)
			{
				slider.RecalculateDimensions();
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
		private Editor okButtonEditor;

		public void OnEnable()
		{
			var panel = (UIButtonPanelScriptableObject)target;
			panelSO = new SerializedObject(panel);

			okButtonProp = panelSO.FindProperty("okButtonData");
			cancelButtonProp = panelSO.FindProperty("cancelButtonData");
			yesButtonProp = panelSO.FindProperty("yesButtonData");
			noButtonProp = panelSO.FindProperty("noButtonData");
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
				if (okButtonProp.boxedValue != null)
				{
					GUI.enabled = false;
					Editor.CreateCachedEditor((UIButtonScriptableObject)okButtonProp.boxedValue, typeof(UIButtonScriptableObjectEditor), ref okButtonEditor);
					okButtonEditor.OnInspectorGUI();
					GUI.enabled = true;
				}
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


			var minDimenProp = serializedObject.FindProperty("_minDimensions");
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


	[CanEditMultipleObjects]
	[CustomEditor(typeof(UIExpandingLabel))]
	public class UIExpandingLabelEditor : EditorEx
	{
		private SerializedProperty textLabel;
		private SerializedProperty image;
		private SerializedProperty dataProp;
		//private SerializedProperty fontSizeProp;
		private SerializedProperty fontAssetProp;
		private SerializedProperty fontStylesProp;
		private SerializedProperty colorProp;
		private SerializedProperty disabledColorProp;
		private SerializedProperty marginProp;
		private UIExpandingLabel label;
		private bool isDataFoldout = false;
		private Editor dataEditor;

		void OnEnable()
		{
			textLabel = serializedObject.FindProperty("textLabel");
			image = serializedObject.FindProperty("image");
			dataProp = serializedObject.FindProperty("labelData");

			//fontSizeProp = serializedObject.FindProperty("_fontSize");
			fontAssetProp = serializedObject.FindProperty("_fontAsset");
			fontStylesProp = serializedObject.FindProperty("_fontStyles");
			colorProp = serializedObject.FindProperty("_color");
			disabledColorProp = serializedObject.FindProperty("_disabledColor");
			marginProp = serializedObject.FindProperty("_margin");

			label = (UIExpandingLabel)target;

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

			var interactableProp = serializedObject.FindProperty("_interactable");
			EditorGUILayout.PropertyField(interactableProp);

			var textProp = serializedObject.FindProperty("_text");
			EditorGUILayout.PropertyField(textProp);


			EditorGUILayout.PropertyField(colorProp);
			EditorGUILayout.PropertyField(disabledColorProp);


			if (!label.autoSizeFont)
				label.fontSize = EditorGUILayout.FloatField("Font Size", label.fontSize);
			else
			{
				GUI.enabled = false;
				EditorGUILayout.FloatField("Font Size", label.fontSize);
				GUI.enabled = true;
			}

			++indentLevel;
			label.autoSizeFont = EditorGUILayout.Toggle("Auto size", label.autoSizeFont);
			if (label.autoSizeFont)
			{
				var orgWidth = EditorGUIUtility.labelWidth;
				EditorGUILayout.LabelField("Auto size options");
				++indentLevel;
				label.fontSizeMin = EditorGUILayout.FloatField("min", label.fontSizeMin);
				label.fontSizeMax = EditorGUILayout.FloatField("max", label.fontSizeMax);
				--indentLevel;

				EditorGUIUtility.labelWidth = orgWidth;
			}
			--indentLevel;

			EditorGUILayout.PropertyField(fontAssetProp);
			EditorGUILayout.PropertyField(fontStylesProp);

			var alignmentProp = serializedObject.FindProperty("_alignmentOptions");
			EditorGUILayout.PropertyField(alignmentProp);

			EditorGUILayout.PropertyField(marginProp);

			var fitHorz = this.Property("_fillParentHorizontal").boolValue;
			var fitVert = this.Property("_fillParentVertical").boolValue;
			if (fitHorz && fitVert)
				GUI.enabled = false;
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_minDimensions"));
			EditorGUILayout.PropertyField(serializedObject.FindProperty("_maxDimensions"));
			GUI.enabled = true;

			EditorGUILayout.PropertyField(textLabel);
			EditorGUILayout.PropertyField(image);

			var oldData = (UIExpandingLabelScriptableObject)dataProp.boxedValue;
			CreateScriptObjectEditor<UIExpandingLabelScriptableObject, UIExpandingLabelEditor>(
				"ScriptableObject Data", dataProp, oldData,
				ref dataEditor, ref isDataFoldout, label, UpdateBackingData);
			//EditorGUILayout.PropertyField(labelDataProp);
			//if (labelDataProp.boxedValue != null)
			//{
			//if (this.CreateFoldout(ref isDataFoldout, "Scriptable Label Data"))
			//	{
			//this.CreateScriptObjectEditor(typeof(UIExpandingLabelScriptableObjectEditor),
			//oldValue, (UIExpandingLabelScriptableObject)labelDataProp.boxedValue, ref dataEditor, label, UpdateBackingData);
			//	}
			//}


			serializedObject.ApplyModifiedProperties();

			if (EditorGUI.EndChangeCheck())
			{
				label.UpdateBackingData_EDITOR();
				//label.referenceName = refNameProp.stringValue;
				//label.interactable = interactableProp.boolValue;
				//label.text = textProp.stringValue;
				//label.color = colorProp.colorValue;
				//label.disabledColor = disabledColorProp.colorValue;
				//label.fontSize = fontSizeProp.floatValue;
				//label.fontAsset = (TMP_FontAsset)fontAssetProp.boxedValue;
				//label.fontStyles = (FontStyles)fontStylesProp.enumValueFlag;
				//label.alignmentOptions = (TextAlignmentOptions)alignmentProp.enumValueFlag;
				//label.margin = marginProp.vector4Value;
			}
		}

		private void UpdateBackingData(UIExpandingLabelScriptableObject labelData)
		{
			label.UpdateBackingData(labelData);
			fontAssetProp.boxedValue = labelData.fontAsset;
			fontStylesProp.enumValueFlag = (int)labelData.fontStyles;
			//fontSizeProp.floatValue = labelData.fontSize;
			colorProp.colorValue = labelData.fontColor;
			disabledColorProp.colorValue = labelData.disabledColor;
			marginProp.vector4Value = labelData.textMargin;
			EditorUtility.SetDirty(label);
			label.RecalculateDimensions();
			label.RecordPrefabInstances();
		}
	}



	[CustomEditor(typeof(UIPanelScriptableObject))]
	public class UIPanelScriptableObjectEditor : Editor { }

	[CustomEditor(typeof(UIDropdownScriptableObject))]
	public class UIDropdownScriptableObjectEditor : Editor { }

	[CustomEditor(typeof(UICheckBoxScriptableObject))]
	public class UICheckBoxScriptableObjectEditor : EditorEx
	{
		private UICheckBoxScriptableObject checkboxData;
		private SerializedProperty labelDataProp;
		private Editor labelEditor;
		private bool isLabelFoldout;

		void OnEnable()
		{
			checkboxData = (UICheckBoxScriptableObject)target;

			labelDataProp = serializedObject.FindProperty("labelData");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			var oldLabelData = (UIExpandingLabelScriptableObject)labelDataProp.boxedValue;
			this.DrawDefaultInspector();

			CreateScriptObjectEditor<UIExpandingLabelScriptableObject, UICheckBoxScriptableObjectEditor>(
				"ScriptableObject Data", labelDataProp,
				oldLabelData, ref labelEditor, ref isLabelFoldout, null, UpdateBackingData);
			//if (labelDataProp != null
			//	&& this.CreateFoldout(ref isLabelFoldout, "Text Label Data"))
			//{
			//	this.CreateScriptObjectEditor(typeof(UIExpandingLabelScriptableObjectEditor),
			//		oldLabel, (UIExpandingLabelScriptableObject)labelDataProp.boxedValue, ref labelEditor, null, UpdateBackingData);
			//	this.CreateBorder(2);
			//}

			if (EditorGUI.EndChangeCheck())
			{

			}
		}

		private void UpdateBackingData(UIExpandingLabelScriptableObject updateBackingData)
		{

		}
	}

	[CustomEditor(typeof(UIExpandingInputFieldScriptableObject))]
	public class UIExpandingInputFieldScriptableObjectEditor : Editor { }

	[CustomEditor(typeof(UIExpandingLabelScriptableObject))]
	public class UIExpandingLabelScriptableObjectEditor : Editor { }

	[CustomEditor(typeof(UIButtonScriptableObject))]
	public class UIButtonScriptableObjectEditor : Editor { }

	[CustomEditor(typeof(UIImageViewScriptableObject))]
	public class UIImageViewScriptableObjectEditor : Editor { }



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