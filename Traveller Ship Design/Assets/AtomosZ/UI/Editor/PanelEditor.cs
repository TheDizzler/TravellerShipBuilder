using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using static AtomosZ.UI.DynamicPanel;


namespace AtomosZ.UI.EditorZ
{
	[CustomEditor(typeof(DynamicPanel))]
	public class DynamicPanelEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();

			if (EditorGUI.EndChangeCheck())
			{
				DynamicPanel panel = (DynamicPanel)target;
				panel.Refresh();
			}
		}
	}

	[CustomEditor(typeof(DynamicPanelOperator))]
	public class DynamicPanelOperatorEditor : Editor
	{
		private SerializedObject panelSO;
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
		private SerializedProperty createPanelControl;
		private RectTransform rect;
		private Vector2 lastSize;
		private BottomPanel bottomPanel;
		private DynamicPanelOperator adder;
		private DynamicPanel panel;
		private SerializedProperty panelControlsSO;


		void OnEnable()
		{
			adder = (DynamicPanelOperator)target;
			panel = adder.GetComponent<DynamicPanel>();
			bottomPanel = adder.GetComponentInChildren<BottomPanel>();

			panelSO = new SerializedObject(panel);
			titleStyle = panelSO.FindProperty("titleType");
			titleText = panelSO.FindProperty("_titleText");
			centerTitleText = panelSO.FindProperty("centerTitleText");
			panelStyle = panelSO.FindProperty("panelType");
			minDims = panelSO.FindProperty("minSize");
			maxDims = panelSO.FindProperty("maxSize");
			alwaysShrink = panelSO.FindProperty("alwaysShrinkToMinSize");
			showCloseButton = panelSO.FindProperty("showCloseButton");
			showMinimizeButton = panelSO.FindProperty("showMinimizeButton");
			showMaximizeButton = panelSO.FindProperty("showMaximizeButton");

			createPanelControl = serializedObject.FindProperty("createPanelControl");

			rect = adder.GetComponent<RectTransform>();
			lastSize = rect.sizeDelta;

			BuildControlList();

			adder.Refresh();
		}

		private void BuildControlList()
		{
			var currentControls = bottomPanel.GetControlsFromTransform();
			var newControls = new List<PanelControl>();
			foreach (var control in currentControls)
			{
				if (!control.TryGetComponent<IUIBehavior>(out var uiBehavior))
				{
					Debug.Log($"control {control.name} has not UIBehavior");
					continue;
				}

				var dataEx = uiBehavior.GetBackingData();
				object panelControl = new PanelControl
				{
					uiDesignObject = uiBehavior.designObject,
					controlType = dataEx.dataType,
				};

				typeof(PanelControl).GetField(PanelControl.panelControlNames[dataEx.dataType]).SetValue(panelControl, dataEx);
				newControls.Add((PanelControl)panelControl);
			}

			if (adder.panelControls.Count == newControls.Count)
			{
				var newOrder = new List<UIDesignObject>(newControls.Count);
				bool isOrderChanged = false;
				for (int i = 0; i < newControls.Count; ++i)
				{
					newOrder.Add(adder.panelControls[i].uiDesignObject);
					if (newControls[i].uiDesignObject != adder.panelControls[i].uiDesignObject)
					{
						isOrderChanged = true;
						//Debug.Log("Order changed!");
					}
				}

				if (isOrderChanged)
				{
					for (int i = 0; i < newOrder.Count; ++i)
					{
						bottomPanel.ReorderControls(newOrder);
					}

					BuildControlList();
					return;
				}
			}

			adder.panelControls = newControls;
			panelControlsSO = serializedObject.FindProperty("panelControls");
		}

		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			EditorGUILayout.PropertyField(titleStyle);
			EditorGUILayout.PropertyField(titleText);
			if (panel.titleType == TitleLabelStyle.Bar)
				EditorGUILayout.PropertyField(centerTitleText);
			EditorGUILayout.PropertyField(panelStyle);
			EditorGUILayout.PropertyField(minDims);
			EditorGUILayout.PropertyField(maxDims);
			EditorGUILayout.PropertyField(alwaysShrink);
			EditorGUILayout.PropertyField(showCloseButton);
			EditorGUILayout.PropertyField(showMinimizeButton);
			//EditorGUILayout.PropertyField(showMaximizeButton);

			EditorGUILayout.PropertyField(createPanelControl);
			EditorGUILayout.PropertyField(panelControlsSO);


			if (serializedObject.ApplyModifiedProperties() || panelSO.ApplyModifiedProperties())
				adder.Refresh();

			if (GUILayout.Button("Clear All"))
				adder.Clear();

			if (EditorGUI.EndChangeCheck())
				BuildControlList();
		}

		public void OnSceneGUI()
		{
			var size = rect.sizeDelta;
			if (size != lastSize)
			{
				adder.Refresh();
				lastSize = size;
			}
		}
	}



	[CustomEditor(typeof(BottomPanel))]
	public class BottomPanelEditor : Editor
	{
		private Vector2 lastSize;

		public void OnSceneGUI()
		{
			// lock the panel size so it can't get changed except by it's parent 
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
		private ImageViewDataEx viewData;
		private Dictionary<ImageEx, ImageView> images = new();
		private bool imagesVisible = true;
		private SerializedObject gridLayoutSO;
		private SerializedObject scrollRectSO;
		private SerializedObject contentRectSO;

		public void OnEnable()
		{
			imageViewPanel = (UIImageViewPanel)target;
			viewData = (ImageViewDataEx)imageViewPanel.GetBackingData();
			gridLayoutSO = new SerializedObject(imageViewPanel.gridLayout);
			scrollRectSO = new SerializedObject(imageViewPanel.scrollRect);
			contentRectSO = new SerializedObject(imageViewPanel.scrollRect.content);
			BuildImageList();
		}

		private void BuildImageList()
		{
			var imageViews = imageViewPanel.GetComponentsInChildren<ImageView>();
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


			int imagesOnRow = Mathf.FloorToInt(
				(viewData.maxPanelSize.x - (imageViewPanel.gridLayout.padding.left + imageViewPanel.gridLayout.padding.right))
				/ (viewData.imageSize.x + imageViewPanel.gridLayout.spacing.x));
			var minWidthReq = viewData.imageSize.x * imagesOnRow + imageViewPanel.gridLayout.spacing.x * (imagesOnRow - 1);

			GUILayout.BeginHorizontal();
			EditorGUILayout.FloatField("min panel width", minWidthReq);
			EditorGUILayout.IntField("images on row", imagesOnRow);

			GUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				imageViewPanel.UpdateBackingData();
				imageViewPanel.gridLayout.cellSize = new Vector2(
					Mathf.Max(viewData.imageSize.x, viewData.labelEx.maxLabelDimensions.x),
					viewData.imageSize.y + viewData.labelEx.maxLabelDimensions.y);
				gridLayoutSO.ApplyModifiedProperties();

				contentRectSO.ApplyModifiedProperties();
			}

			if (GUILayout.Button("Add Image"))
			{
				imageViewPanel.AddImage(new ImageEx
				{
					sprite = viewData.defaultSprite,
					labelEx = viewData.labelEx,
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

	[CustomEditor(typeof(ImageView))]
	public class ImageViewEditor : UIEditor<ImageView>
	{ }




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
		private SerializedProperty labelEx;
		private SerializedProperty textLabel;
		private SerializedProperty image;

		void OnEnable()
		{
			labelEx = serializedObject.FindProperty("labelEx");


			textLabel = serializedObject.FindProperty("textLabel");
			image = serializedObject.FindProperty("image");

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
			EditorGUILayout.PropertyField(labelEx);

			GUILayout.Space(10);
			{
				EditorGUILayout.PropertyField(textLabel);
				EditorGUILayout.PropertyField(image);
			}

			if (serializedObject.ApplyModifiedProperties())
			{
				var panel = (UIExpandingLabel)target;
				panel.UpdateBackingData();
			}
		}
	}

	[CustomEditor(typeof(UISlider))]
	public class SliderCustomEditor : Editor
	{
		private UISlider slider;
		private RectTransform rect;
		private Vector2 lastSize;

		void OnEnable()
		{
			slider = (UISlider)target;
			rect = slider.GetComponent<RectTransform>();
			lastSize = rect.sizeDelta;
		}


		public override void OnInspectorGUI()
		{
			EditorGUI.BeginChangeCheck();
			base.OnInspectorGUI();
			if (EditorGUI.EndChangeCheck())
			{
				slider.UpdateBackingData();
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