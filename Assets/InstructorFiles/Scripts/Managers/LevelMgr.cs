// LevelMgr.cs : lines 1–118

using UnityEngine;

/// <summary>
///     Manages level ordering, progression, and save coordination.
///     Single source of truth for which level is current and what comes next.
/// </summary>
public class LevelMgr : Singleton<LevelMgr>
{
#region Serialized Data

	[SerializeField] private LevelData _levelData;

#endregion

#region Runtime State

	public int CurrentLevelIndex { get; private set; } = -1;

#endregion

#region Public Properties

	public LevelData LevelData => _levelData;

	public int LevelCount => _levelData != null ? _levelData.Levels.Length : 0;

	public bool HasValidLevelData =>
		_levelData != null &&
		_levelData.Levels != null &&
		_levelData.Levels.Length > 0;

	public bool HasNextLevel =>
		HasValidLevelData &&
		CurrentLevelIndex + 1 < LevelCount;

	public bool IsFinalLevel =>
		HasValidLevelData &&
		CurrentLevelIndex == LevelCount - 1;

#endregion


#region Level Resolution

	public int GetNewGameLevelIndex()
	{
		return 0;
	}

	public int GetContinueLevelIndex()
	{
		if (!HasValidLevelData)
			return -1;

		var nextIndex = SaveUtil.SavedValues.HighestLevelCompleted + 1;
		return Mathf.Clamp(nextIndex, 0, LevelCount - 1);
	}

	public bool IsLevelUnlocked(int levelIndex)
	{
		if (!HasValidLevelData)
			return false;

		return levelIndex <= SaveUtil.SavedValues.HighestLevelCompleted + 1;
	}

#endregion

#region Level Loading

	public void LoadLevel(int levelIndex)
	{
		if (!HasValidLevelData)
		{
			Debug.LogError("LevelMgr: No LevelData assigned");
			return;
		}

		if (levelIndex < 0 || levelIndex >= LevelCount)
		{
			Debug.LogError($"LevelMgr: Invalid level index {levelIndex}");
			return;
		}

		CurrentLevelIndex = levelIndex;

		var sceneName = _levelData.Levels[levelIndex].SceneName;

		GameMgr.Instance.GameState = GameMgr.GameStates.Loading;
		SceneMgr.Instance.LoadScene(sceneName, GameMenus.InGameUI, GameMgr.Instance.StartGame);
	}

	public void ReloadCurrentLevel()
	{
		LoadLevel(CurrentLevelIndex);
	}

	public void LoadNextLevelOrFinish()
	{
		if (HasNextLevel)
			LoadLevel(CurrentLevelIndex + 1);
		else
			SceneMgr.Instance.LoadScene(
				GameScenes.MainMenu,
				GameMenus.MainMenu,
				() => GameMgr.Instance.GameState = GameMgr.GameStates.Menu
			);
	}

#endregion

#region Completion & Save

	public void CompleteCurrentLevel(int timeMs)
	{
		if (!HasValidLevelData || CurrentLevelIndex < 0)
			return;

		EnsureBestTimeArray();

		var previousBest = SaveUtil.SavedValues.BestTimeMs[CurrentLevelIndex];
		if (previousBest <= 0 || timeMs < previousBest) SaveUtil.SavedValues.BestTimeMs[CurrentLevelIndex] = timeMs;

		SaveUtil.SavedValues.HighestLevelCompleted =
			Mathf.Max(SaveUtil.SavedValues.HighestLevelCompleted, CurrentLevelIndex);

		SaveUtil.Save();
	}

	private void EnsureBestTimeArray()
	{
		if (SaveUtil.SavedValues.BestTimeMs == null ||
			SaveUtil.SavedValues.BestTimeMs.Length != LevelCount)
			SaveUtil.SavedValues.BestTimeMs = new int[LevelCount];
	}

#endregion
}
