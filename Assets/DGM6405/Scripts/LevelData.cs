using System;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
	public LevelInfo[] Levels;

	[Serializable]
	public class LevelInfo
	{
		public string SceneName;
		public string LevelName;
		public int ParTimeMs;

		[Header("Abilities")]
		public bool AllowBlock = true;

		public bool AllowShoot = true;
		public bool AllowMelee = true;
	}
}
