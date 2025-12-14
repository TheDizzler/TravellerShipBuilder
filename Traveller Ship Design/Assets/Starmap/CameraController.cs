using UnityEngine;
using static AtomosZ.Keyboard;

namespace AtomosZ.MG2eTraveller.Starmap
{
	[ExecuteInEditMode]
	public class CameraController : MonoBehaviour
	{
		public SubSectorMap subSector;
		private Vector3 scrollStartPos;

		public StarSystem selectedSystem;
		public StarSystem hoverSystem;

		[SerializeField] private int minZoom = -2;
		[SerializeField] private int maxZoom = -30;
		[Tooltip("Scrolling feels weird when the grid doesn't look like it's moving, so don't use a value of 1.")]
		[Range(0.2f, 3.0f)]
		[SerializeField] private float zoomMultiplier = 1.1f;

		void Update()
		{
			Vector3 mouseWorldPos = Helpers.GetMouseWorldPos();
			ModifierKey modifierKeys = GetModifierKeyInput();

			var mouseCell = subSector.GetCellAtWorldPosition(mouseWorldPos);
			var mouseSystem = subSector.GetSystemAt(mouseCell);


			if (mouseSystem != null)
			{
				if (mouseSystem == selectedSystem)
				{
					selectedSystem.SetHighlight(StarSystem.SystemHighlightState.SelectedMouseOver);
				}
				else
				{
					if (hoverSystem != null && hoverSystem != mouseSystem)
						hoverSystem.SetHighlight(StarSystem.SystemHighlightState.None);
					mouseSystem.SetHighlight(StarSystem.SystemHighlightState.MouseOver);
					if (selectedSystem != null)
						selectedSystem.SetHighlight(StarSystem.SystemHighlightState.Selected);
				}

				hoverSystem = mouseSystem;
			}

			if (Input.GetMouseButtonDown(0))
			{
				if (selectedSystem != null && selectedSystem != mouseSystem)
					selectedSystem.SetHighlight(StarSystem.SystemHighlightState.None);

				if (mouseSystem != null)
					mouseSystem.SetHighlight(StarSystem.SystemHighlightState.Selected);
				selectedSystem = mouseSystem;

				scrollStartPos = mouseWorldPos;
			}
			else if (Input.GetMouseButton(0))
			{
				var diff = mouseWorldPos - scrollStartPos;
				var newX = Helpers.camera.transform.position.x - diff.x;
				var newY = Helpers.camera.transform.position.y - diff.y;
				Helpers.camera.transform.position = new Vector3(newX, newY, Helpers.camera.transform.position.z);
				mouseWorldPos = Helpers.GetMouseWorldPos();
				scrollStartPos = mouseWorldPos;
			}
			else if (Input.GetMouseButtonUp(0))
			{
				//EndScroll();
			}



			if (Input.mouseScrollDelta != Vector2.zero)
			{
				if ((modifierKeys & ModifierKey.Ctrl) == ModifierKey.Ctrl)
				{
					var newY = Helpers.camera.transform.position.y + Input.mouseScrollDelta.y * zoomMultiplier;
					Helpers.camera.transform.position = new Vector3(
						Helpers.camera.transform.position.x, newY, Helpers.camera.transform.position.z);
				}
				else if ((modifierKeys & ModifierKey.Shift) == ModifierKey.Shift)
				{
					var newX = Helpers.camera.transform.position.x + Input.mouseScrollDelta.y * zoomMultiplier;
					Helpers.camera.transform.position = new Vector3(
						newX, Helpers.camera.transform.position.y, Helpers.camera.transform.position.z);
				}
				else
				{
					var newZ = Helpers.camera.transform.position.z + Input.mouseScrollDelta.y;
					if (newZ >= minZoom)
						newZ = minZoom;
					else if (newZ < maxZoom)
						newZ = maxZoom;
					Helpers.camera.transform.position = new Vector3(
						Helpers.camera.transform.position.x, Helpers.camera.transform.position.y, newZ);
				}
			}
		}
	}
}