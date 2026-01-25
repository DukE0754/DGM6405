// Assets/Editor/TMPDestroyTracker.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
static class TMPDestroyTracker
{
	static HashSet<int> _lastIds = new HashSet<int>();

	static TMPDestroyTracker()
	{
		CacheCurrent();
		ObjectChangeEvents.changesPublished += OnChanges;
	}

	static void OnChanges(ref ObjectChangeEventStream stream)
	{
		bool relevant = false;

		for (int i = 0; i < stream.length; i++)
		{
			var kind = stream.GetEventType(i);
			if (kind == ObjectChangeKind.DestroyGameObjectHierarchy ||
				kind == ObjectChangeKind.ChangeGameObjectStructure ||
				kind == ObjectChangeKind.ChangeGameObjectStructureHierarchy)
			{
				relevant = true;
				break;
			}
		}

		if (!relevant)
			return;

		var now = GetCurrentIds();
		foreach (var id in _lastIds)
		{
			if (!now.Contains(id))
			{
				UnityEngine.Debug.Log(
					$"TMP destroyed instanceID={id}\n{new StackTrace(2, true)}"
				);
			}
		}

		_lastIds = now;
	}

	static HashSet<int> GetCurrentIds()
	{
		return new HashSet<int>(
			Resources.FindObjectsOfTypeAll<TMP_Text>()
				.Where(t => t != null)
				.Select(t => t.GetInstanceID())
		);
	}

	static void CacheCurrent()
	{
		_lastIds = GetCurrentIds();
	}
}
#endif
