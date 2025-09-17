using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.ShaderGraph;

using UnityEngine;
using UnityEngine.EventSystems;

using static AtomosZ.UI.DynamicPanel;

namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(DynaPanelOp))]
	public class DynaPanelOpEditor : Editor
	{
		private DynaPanelOp dynaPanOp;
		//private BottomPanel bottomPanel;
		private RectTransform rect;
		private Vector2 lastSize;
		private SerializedProperty currentType;

		private SerializedProperty uiControls;
		private Dictionary<UIControlType, SerializedProperty> scriptableObjects;


		//private SerializedProperty scriptableObj;
		//private SerializedProperty inputFieldScriptObj;


		private DynamicPanel dynaPan;
		private SerializedObject dynaPanSO;
		private SerializedProperty titleStyle;
		private SerializedProperty titleText;
		private SerializedProperty centerTitleText;
		private SerializedProperty panelStyle;
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
			//bottomPanel = dynaPanOp.GetComponentInChildren<BottomPanel>();

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
			foreach (var control in currentControls)
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
			if (dynaPan.titleType == TitleLabelStyle.Bar)
				EditorGUILayout.PropertyField(centerTitleText);
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
				//dynPanelOp.UpdateData();
				lastSize = size;
			}
		}
	}


	//[CustomEditor(typeof(DynamicPanelOperator))]
	//public class DynamicPanelOperatorEditor : Editor
	//{
	//	private SerializedObject panelSO;
	//	private SerializedProperty titleStyle;
	//	private SerializedProperty titleText;
	//	private SerializedProperty centerTitleText;
	//	private SerializedProperty panelStyle;
	//	private SerializedProperty minDims;
	//	private SerializedProperty maxDims;
	//	private SerializedProperty alwaysShrink;
	//	private SerializedProperty showCloseButton;
	//	private SerializedProperty showMinimizeButton;
	//	private SerializedProperty showMaximizeButton;
	//	private SerializedProperty createPanelControl;
	//	private RectTransform rect;
	//	private Vector2 lastSize;
	//	private BottomPanel bottomPanel;
	//	private Image tabImage;
	//	private Image panelImage;
	//	private TextMeshProUGUI titleTextTMP;
	//	private DynamicPanelOperator dynPanelOp;
	//	private DynamicPanel dynPanel;
	//	private SerializedProperty panelControlsSO;


	//	void OnEnable()
	//	{
	//		dynPanelOp = (DynamicPanelOperator)target;
	//		dynPanel = dynPanelOp.GetComponent<DynamicPanel>();
	//		bottomPanel = dynPanelOp.GetComponentInChildren<BottomPanel>();
	//		tabImage = dynPanelOp.GetComponentInChildren<DragPanel>().GetComponent<Image>();
	//		panelImage = bottomPanel.GetComponent<Image>();
	//		titleTextTMP = tabImage.GetComponentInChildren<TextMeshProUGUI>();
	//		panelSO = new SerializedObject(dynPanel);
	//		titleStyle = panelSO.FindProperty("titleType");
	//		titleText = panelSO.FindProperty("_titleText");
	//		centerTitleText = panelSO.FindProperty("centerTitleText");
	//		panelStyle = panelSO.FindProperty("panelType");
	//		minDims = panelSO.FindProperty("minSize");
	//		maxDims = panelSO.FindProperty("maxSize");
	//		alwaysShrink = panelSO.FindProperty("alwaysShrinkToMinSize");
	//		showCloseButton = panelSO.FindProperty("showCloseButton");
	//		showMinimizeButton = panelSO.FindProperty("showMinimizeButton");
	//		showMaximizeButton = panelSO.FindProperty("showMaximizeButton");

	//		createPanelControl = serializedObject.FindProperty("createPanelControl");

	//		rect = dynPanelOp.GetComponent<RectTransform>();
	//		lastSize = rect.sizeDelta;

	//		BuildControlList();

	//		dynPanelOp.UpdateData();
	//	}

	//	private void BuildControlList()
	//	{
	//		var currentControls = bottomPanel.GetControlsFromTransform();
	//		var newControls = new List<PanelControl_dep>();
	//		foreach (var control in currentControls)
	//		{
	//			if (!control.TryGetComponent<IUIBehavior>(out var uiBehavior))
	//			{
	//				Debug.LogError($"control {control.name} has no UIBehavior");
	//				continue;
	//			}

	//			var dataEx = uiBehavior.GetBackingData();

	//			object panelControl = new PanelControl_dep
	//			{
	//				uiDesignObject = uiBehavior.designObject,
	//				controlType = dataEx.dataType,
	//			};

	//			typeof(PanelControl_dep).GetField(PanelControl_dep.panelControlNames[dataEx.dataType]).SetValue(panelControl, dataEx);
	//			newControls.Add((PanelControl_dep)panelControl);
	//		}

	//		dynPanelOp.panelControls = newControls;

	//		panelControlsSO = serializedObject.FindProperty("panelControls");
	//	}



	//	// how to prevent this from running everytime anytime something is changed in the DynaPanelOp
	//	private void UpdateControlList()
	//	{
	//		var currentControls = bottomPanel.GetControlsFromTransform();
	//		var pCtrls = dynPanelOp.panelControls;

	//		if (currentControls.Count != pCtrls.Count)
	//		{
	//			BuildControlList();
	//		}
	//		else if (pCtrls.Count == 0)
	//			return;
	//		else
	//		{
	//			Debug.Log("This is being called a lot, isn't it?");
	//			dynPanelOp.ClearAllUIControls();
	//			foreach (var ctrl in pCtrls)
	//			{
	//				//dynPanelOp.AddUIControl(ctrl.GetData());
	//				dynPanel.AddUIControl(ctrl.GetData());
	//			}
	//		}
	//	}

	//	public override void OnInspectorGUI()
	//	{
	//		EditorGUI.BeginChangeCheck();
	//		EditorGUILayout.PropertyField(titleStyle);
	//		EditorGUILayout.PropertyField(titleText);
	//		if (dynPanel.titleType == TitleLabelStyle.Bar)
	//			EditorGUILayout.PropertyField(centerTitleText);
	//		EditorGUILayout.PropertyField(panelStyle);
	//		EditorGUILayout.PropertyField(minDims);
	//		EditorGUILayout.PropertyField(maxDims);
	//		EditorGUILayout.PropertyField(alwaysShrink);
	//		EditorGUILayout.PropertyField(showCloseButton);
	//		EditorGUILayout.PropertyField(showMinimizeButton);
	//		//EditorGUILayout.PropertyField(showMaximizeButton);

	//		EditorGUILayout.PropertyField(createPanelControl);
	//		EditorGUILayout.PropertyField(panelControlsSO);

	//		var panelSOUpdated = panelSO.ApplyModifiedProperties();
	//		var soUpdated = serializedObject.ApplyModifiedProperties();
	//		if (soUpdated || panelSOUpdated)
	//		{
	//			dynPanelOp.UpdateData();
	//		}



	//		if (GUILayout.Button("Clear All"))
	//		{
	//			dynPanelOp.ClearAllUIControls();
	//		}


	//		if (EditorGUI.EndChangeCheck())
	//		{
	//			// how to prevent this from running everytime anytime something is changed in the DynaPanelOp
	//			UpdateControlList();

	//			PrefabUtility.RecordPrefabInstancePropertyModifications(dynPanel);
	//			PrefabUtility.RecordPrefabInstancePropertyModifications(tabImage);
	//			PrefabUtility.RecordPrefabInstancePropertyModifications(panelImage);
	//			PrefabUtility.RecordPrefabInstancePropertyModifications(titleTextTMP);
	//		}
	//	}

	//	public void OnSceneGUI()
	//	{
	//		var size = rect.sizeDelta;
	//		if (size != lastSize)
	//		{
	//			dynPanelOp.UpdateData();
	//			lastSize = size;
	//		}
	//	}
	//}



	[CustomEditor(typeof(BottomPanel))]
	public class BottomPanelEditor : Editor
	{
		private Vector2 lastSize;

		public void OnSceneGUI()
		{
			// lock the dynPanel size so it can't get changed except by it's parent 
			BottomPanel panel = (BottomPanel)target;
			var rect = panel.GetComponent<RectTransform>();
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
	public class UIDropdownEditor : UIEditor<UIDropdown>
	{ }

	[CustomEditor(typeof(UIButtonPanel))]
	public class UIButtonPanelEditor : UIEditor<UIButtonPanel>
	{ }

	[CustomEditor(typeof(UICheckBox))]
	public class UICheckBoxEditor : UIEditor<UICheckBox>
	{ }


	[CustomEditor(typeof(UIButton))]
	public class UIButtonEditor : UIEditor<UIButton>
	{ }

	[CustomEditor(typeof(UIExpandingInputField))]
	public class UIExpandingInputFieldEditor : UIEditor<UIExpandingInputField>
	{ }

	[CustomEditor(typeof(UIImageView))]
	public class ImageViewEditor : UIEditor<UIImageView>
	{ }


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
			GUILayout.Space(10);
			{
				EditorGUILayout.PropertyField(textLabel);
				EditorGUILayout.PropertyField(image);
				EditorGUILayout.Foldout(true, "Label Data");
				EditorGUILayout.PropertyField(labelExProp);
			}

			if (serializedObject.ApplyModifiedProperties())
			{
				labelEx.SetToScriptableObjectValues();
				label.UpdateBackingData();
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