// LevelMgr.cs : lines 1–118

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
///     Manages level ordering, progression, and save coordination.
///     Single source of truth for which level is current and what comes next.
/// </summary>
public class LevelMgr : Singleton<LevelMgr>, IGameStateListener
{
#region Serialized Data

	[SerializeField] private LevelData _levelData;

#endregion

	private void OnEnable()
	{
		GlobalEventBus.Instance.Register<IGameStateListener>(this);
	}

	private void OnDisable()
	{
		GlobalEventBus.Instance.Unregister<IGameStateListener>(this);
	}

	public void OnNextLevel(int timeMs)
	{
		CompleteCurrentLevel(timeMs);
	}

#region Runtime State

	public int CurrentLevelIndex { get; private set; } = -1;

	public bool IsLevelLoaded { get; private set; }

#endregion

#region Public Properties

	public LevelData LevelData => _levelData;

	public int LevelCount
	{
		get
		{
			if (!HasValidLevelData) return 0;
			var count = 0;
			foreach (var chapter in _levelData.Chapters)
			{
				if (chapter.Levels != null)
					count += chapter.Levels.Length;
			}
			return count;
		}
	}

	public bool HasValidLevelData =>
		_levelData != null &&
		_levelData.Chapters != null &&
		_levelData.Chapters.Length > 0;

	public bool HasNextLevel =>
		HasValidLevelData &&
		CurrentLevelIndex + 1 < LevelCount;

	public bool IsFinalLevel =>
		HasValidLevelData &&
		CurrentLevelIndex == LevelCount - 1;

	/// <summary>
	///     Gets the level info for the current level.
	///     Returns true if level info was found, false otherwise (e.g. test scenes).
	/// </summary>
	public bool TryGetCurrentLevelInfo(out LevelData.LevelInfo info)
	{
		info = null;
		if (!HasValidLevelData) return false;

		var absoluteIndex = 0;
		foreach (var chapter in _levelData.Chapters)
		{
			if (chapter.Levels == null) continue;
			for (var i = 0; i < chapter.Levels.Length; i++)
			{
				if (absoluteIndex == CurrentLevelIndex)
				{
					info = chapter.Levels[i];
					return true;
				}
				absoluteIndex++;
			}
		}

		// Fallback scenario: Check current level name
		var currentSceneName = SceneManager.GetActiveScene().name;
		absoluteIndex = 0;
		foreach (var chapter in _levelData.Chapters)
		{
			if (chapter.Levels == null) continue;
			for (var i = 0; i < chapter.Levels.Length; i++)
			{
				if (chapter.Levels[i].SceneName == currentSceneName)
				{
					CurrentLevelIndex = absoluteIndex;
					info = chapter.Levels[i];
					return true;
				}
				absoluteIndex++;
			}
		}

		return false;
	}

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
		IsLevelLoaded = false;
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

		GlobalContext.Instance.GameMgr.GameState = GameMgr.GameStates.Loading;
		GlobalEventBus.Instance.Raise<ISceneEventListener>(l =>
			l.OnLoadScene(GameScenes.Gameplay, GameMenus.InGameUI, () => StartCoroutine(LoadLevelRoutine())));
	}

	private IEnumerator LoadLevelRoutine()
	{
		LevelData.LevelInfo levelInfo = null;
		var absoluteIndex = 0;
		foreach (var chapter in _levelData.Chapters)
		{
			if (chapter.Levels == null) continue;
			var found = false;
			for (var i = 0; i < chapter.Levels.Length; i++)
			{
				if (absoluteIndex == CurrentLevelIndex)
				{
					levelInfo = chapter.Levels[i];
					found = true;
					break;
				}
				absoluteIndex++;
			}
			if (found) break;
		}

		if (levelInfo == null)
		{
			Debug.LogError($"LevelMgr: Could not find level info for index {CurrentLevelIndex}");
			yield break;
		}

		var levelName = levelInfo.SceneName;

		Debug.Log($"LevelMgr: Loading {levelName} additively");

		var asyncOperation =
			SceneManager.LoadSceneAsync(
				levelName, LoadSceneMode.Additive);

		while (asyncOperation is {isDone: false}) yield return null;

		Debug.Log("LevelMgr: Level loaded");

		IsLevelLoaded = true;
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
			GlobalEventBus.Instance.Raise<ISceneEventListener>(l => l.OnLoadScene(
				GameScenes.MainMenu,
				GameMenus.MainMenu,
				() => GlobalContext.Instance.GameMgr.GameState = GameMgr.GameStates.Menu
			));
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
