#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DebugCharacterController))]
public class CharacterControllerDebugEditor : Editor
{
	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();

		var debug =
			(DebugCharacterController) target;

		if (GUILayout.Button("Print Debug Info")) debug.PrintDebugInfo();
	}
}
#endif
