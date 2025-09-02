using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using static UnityEngine.Object;
using static DesignManager;

public static class RoomSerializer
{
	[Serializable]
	public class SerializedRoom
	{
		public string roomLabel { get; set; }
		public SerializedWall wall { get; set; }
	}

	[Serializable]
	public class SerializedWall
	{
		public List<SerializedControlPoint> controlPoints { get; set; }
		public List<SerializedDoor> doors { get; set; }
		public bool endPointsConnected { get; set; }
	}


	[Serializable]
	public class SerializedControlPoint
	{
		public float x { get; set; }
		public float y { get; set; }
	}

	[Serializable]
	public class SerializedDoor
	{
		public float x { get; set; }
		public float y { get; set; }
		public int wallSegmentIndex { get; set; }
	}

	public const string roomExt = ".room.tsd";
	/// <summary>
	/// The following reserved characters are replaced with a filesystem safe glyph:
	/// < (less than)
	/// > (greater than)
	/// : (colon)
	/// " (double quote)
	/// / (forward slash)
	/// \ (backslash)
	/// | (vertical bar or pipe)
	/// ? (question mark)
	/// * (asterisk)
	/// </summary>
	/// <param name="roomLabel"></param>
	/// <returns></returns>
	public static string SanitizeForWindowsFilesystem(string roomLabel)
	{
		return roomLabel.Replace("<", "&lt;")
			.Replace(">", "&gt;")
			.Replace(":", "&c;")
			.Replace("\"", "&quot;")
			.Replace("/", "&fs;")
			.Replace("\\", "&bs;")
			.Replace("|", "&p;")
			.Replace("?", "&qm;")
			.Replace("*", "&ast;");
	}

	public static bool SaveRoom(SerializedRoom savedRoom, string roomLabel)
	{
		try
		{
			string destination = Path.Combine(
				Application.persistentDataPath, SanitizeForWindowsFilesystem(roomLabel) + roomExt);
			FileStream file;

			if (File.Exists(destination))
				file = File.OpenWrite(destination);
			else
				file = File.Create(destination);

			BinaryFormatter bf = new BinaryFormatter();
			bf.Serialize(file, savedRoom);
			file.Close();

			return true;
		}
		catch (Exception e)
		{
			DesignManager.ShowErrorDialog(e.StackTrace, e.Message);
			return false;
		}
	}

	public static List<SerializedRoom> GetRoomGeomorphs()
	{
		try
		{
			var rooms = new List<SerializedRoom>();
			var roomFiles = Directory.GetFiles(Application.persistentDataPath, "*" + roomExt);
			foreach (var roomFile in roomFiles)
			{
				var room = GetSerializedRoomFromFile(roomFile);
				if (room != null)
					rooms.Add(room);
			}

			return rooms;
		}
		catch (Exception e)
		{
			DesignManager.ShowErrorDialog(e.StackTrace, e.Message);
			return null;
		}
	}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="room">Room name (not extension), or filepath to serialized room.</param>
	/// <returns></returns>
	public static SerializedRoom GetSerializedRoomFromFile(string room)
	{
		try
		{
			string destination;
			if (room.EndsWith(roomExt))
				destination = room;
			else
				destination = Path.Combine(
					Application.persistentDataPath, SanitizeForWindowsFilesystem(room) + roomExt);
			FileStream file;

			if (File.Exists(destination))
				file = File.OpenRead(destination);
			else
			{
				DesignManager.ShowErrorDialog(
					$"No file found with name {SanitizeForWindowsFilesystem(room)}.",
					"File not found");
				return null;
			}

			BinaryFormatter bf = new BinaryFormatter();
			SerializedRoom data = (SerializedRoom)bf.Deserialize(file);
			file.Close();

			return data;
		}
		catch (Exception e)
		{
			DesignManager.ShowErrorDialog(e.StackTrace, e.Message);
			return null;
		}
	}

	public static bool IsNameUnique(string roomLabel)
	{
		string destination = Path.Combine(Application.persistentDataPath, SanitizeForWindowsFilesystem(roomLabel) + roomExt);
		return !File.Exists(destination);
	}


	public static Sprite CreateSpriteOfGeomorph(SerializedRoom roomData, int layerNum)
	{
		try
		{
			var wallDObj = UnityEngine.Object.Instantiate(DesignManager.GetPrefab(PrefabType.WallSegmentPrefab));
			var wall = wallDObj.GetComponent<Wall>();
			wall.CreateFromSerializedData(roomData.wall);
			var room = wall.ConvertToRoom(roomData.roomLabel);
			var children = room.GetComponentsInChildren<Transform>(includeInactive: true);
			room.ToggleRoomLabel(false);
			foreach (var child in children)
			{
				child.gameObject.layer = layerNum;
			}

			var mainCamera = Camera.main;
			Camera.main.enabled = false;

			// Step 1: Enable the screenshot camera and set its background color to transparent
			var screenshotCamera = DesignManager.GetScreenshotCamera();
			screenshotCamera.gameObject.SetActive(true);
			screenshotCamera.clearFlags = CameraClearFlags.SolidColor;


			// Step 2: Create a high-resolution RenderTexture with transparency
			//var res = Screen.currentResolution;
			var res = room.GetDimensions();
			screenshotCamera.transform.position = new Vector3(res.center.x, res.center.y, -5);

			RenderTexture rt = new RenderTexture((int)res.width * 100, (int)res.height * 100, 24, RenderTextureFormat.ARGB32);
			screenshotCamera.targetTexture = rt;

			// Step 3: Render the screenshot camera view to the RenderTexture
			var prevText = RenderTexture.active;
			RenderTexture.active = rt;
			screenshotCamera.Render();

			// Step 4: Create a high-resolution Texture2D with transparency to store the rendered image
			Texture2D screenShot = new Texture2D((int)res.width * 100, (int)res.height * 100, TextureFormat.RGBA32, false);
			screenShot.ReadPixels(new Rect(0, 0, res.width * 100, res.height * 100), 0, 0);
			screenShot.Apply();
			var sprite = TextureToSprite(screenShot);

			// Step 5: Save the Texture2D as a PNG file with transparency
			byte[] bytes = screenShot.EncodeToPNG();
			string filename = Path.Combine(Application.persistentDataPath, SanitizeForWindowsFilesystem(roomData.roomLabel) + "_thumbnail.png");
			File.WriteAllBytes(filename, bytes);

			// Clean up
			screenshotCamera.targetTexture = null;
			RenderTexture.active = prevText;
			Destroy(rt);

			// Step 6: Disable the screenshot camera
			screenshotCamera.gameObject.SetActive(false);

			mainCamera.enabled = true;

			room.gameObject.SetActive(false);
			Destroy(room.gameObject);
			return sprite;
		}
		catch (Exception e)
		{
			DesignManager.ShowErrorDialog(e.StackTrace, e.Message);
			return null;
		}
	}


	public static Sprite TextureToSprite(Texture2D texture)
	{
		return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
	}
}
