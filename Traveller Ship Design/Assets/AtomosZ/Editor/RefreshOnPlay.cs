using UnityEditor;

[InitializeOnLoad]
public class RefreshOnPlay
{
	static RefreshOnPlay()
	{
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		// Right before entering Play Mode, force an Asset Database refresh
		if (state == PlayModeStateChange.ExitingEditMode)
		{
			AssetDatabase.Refresh();
		}
	}
}
