using UnityEngine;
using UnityEngine.UI;

namespace AtomosZ.MG2eTraveller.Starmap
{
	public class StarfieldScroller : MonoBehaviour
	{
		public Renderer bgRenderer;
		public float speed;
		private Transform cameraTransform;
		private Vector3 lastCamPos;

		public float bgWidth, bgHeight;
		void Start()
		{
			cameraTransform = Helpers.camera.transform;
			lastCamPos = cameraTransform.position;
		}

		[ContextMenu("Create Cube	")]
		public void CreateCube()
		{
			var mesh = new Mesh();
			Vector3[] vertices = new Vector3[4]
			{
				new Vector3(0, 0, 0),
				new Vector3(bgWidth, 0, 0),
				new Vector3(0, bgHeight, 0),
				new Vector3(bgWidth, bgHeight, 0)
			};

			mesh.vertices = vertices;

			int[] tri = new int[6]
			{
				0, 2, 1,
				2, 3, 1
			};

			mesh.triangles = tri;

			Vector3[] normals = new Vector3[4]
			{
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
				-Vector3.forward,
			};

			mesh.normals = normals;

			Vector2[] uv = new Vector2[4]
			{
				new Vector2(0, 0),
				new Vector2(1, 0),
				new Vector2(0, 1),
				new Vector2(1, 1),
			};

			mesh.uv = uv;

			GetComponent<MeshFilter>().mesh = mesh;
		}

		void Update()
		{
#if DEBUG
			if (cameraTransform == null)
				cameraTransform = Helpers.camera.transform;
#endif
			var change = lastCamPos - cameraTransform.position;
			bgRenderer.material.mainTextureOffset += new Vector2(change.x * speed, change.y * speed);
			lastCamPos = cameraTransform.position;
		}
	}
}