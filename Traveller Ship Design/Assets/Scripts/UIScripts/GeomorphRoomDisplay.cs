using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AtomosZ.MG2eTraveller.Ship
{
	public class PrefabRoom
	{
		public string roomName;

	}

	public class GeomorphRoomDisplay : MonoBehaviour
	{
		public static string ROOM_SAVE_PATH;

		//[SerializeField] private ImageViewPanel viewPanel;
		private List<PrefabRoom> roomPrefabs = new List<PrefabRoom>();

		public void Start()
		{
			ROOM_SAVE_PATH = Application.persistentDataPath;
			var roomFiles = Directory.GetFiles(ROOM_SAVE_PATH, $"*{RoomSerializer.roomExt}");
			foreach (var roomFile in roomFiles)
			{
				Debug.Log(roomFile);
			}
		}
	}
}