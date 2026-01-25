using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DualGridSpawner))]
public class DualGridSpawnerEditor : Editor
{
	private const string AutoRefreshWarning =
		"AutoRefresh is enabled on this DualGridSpawner while entering Play Mode. " +
		"It should be off by default in play mode; enable it at runtime only if needed.";

	[InitializeOnLoadMethod]
	private static void RegisterPlayModeWarning()
	{
		EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
		EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
	}

	private static void OnPlayModeStateChanged(PlayModeStateChange state)
	{
		if (state != PlayModeStateChange.ExitingEditMode)
			return;

		var spawners = FindSceneSpawnersWithAutoRefresh();
		foreach (var spawner in spawners)
		{
			Debug.LogWarning(AutoRefreshWarning, spawner);
		}
	}

	private static List<DualGridSpawner> FindSceneSpawnersWithAutoRefresh()
	{
		var results = new List<DualGridSpawner>();
		var spawners = Resources.FindObjectsOfTypeAll<DualGridSpawner>();

		foreach (var spawner in spawners)
		{
			if (!spawner || EditorUtility.IsPersistent(spawner))
				continue;

			if (spawner.AutoRefresh)
				results.Add(spawner);
		}

		return results;
	}

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Grid Controls", EditorStyles.boldLabel);

		var spawner = (DualGridSpawner)target;

		EditorGUI.BeginDisabledGroup(!spawner);
		if (GUILayout.Button("Clear All Spawned 3D Grid Tiles"))
		{
			Undo.RegisterCompleteObjectUndo(spawner, "Clear DualGrid Tiles");
			spawner.ClearAllSpawnedTiles();
			EditorUtility.SetDirty(spawner);
		}

		if (GUILayout.Button("Clear All Spawned 3D Grid Tiles and Rebuild Once"))
		{
			Undo.RegisterCompleteObjectUndo(spawner, "Clear and Rebuild DualGrid");
			spawner.ClearAndRebuildOnce();
			EditorUtility.SetDirty(spawner);
		}

		if (GUILayout.Button("Rebuild Grid Once"))
		{
			Undo.RegisterCompleteObjectUndo(spawner, "Rebuild DualGrid");
			spawner.RebuildOnce();
			EditorUtility.SetDirty(spawner);
		}

		if (GUILayout.Button(spawner.AutoRefresh ? "Disable Auto Refresh" : "Enable Auto Refresh"))
		{
			Undo.RegisterCompleteObjectUndo(spawner, "Toggle DualGrid Auto Refresh");
			spawner.ToggleAutoRefresh();
			EditorUtility.SetDirty(spawner);
		}
		EditorGUI.EndDisabledGroup();
	}
}
