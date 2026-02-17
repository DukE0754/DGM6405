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

	/// <summary>
	///     Gets the level info for the current level.
	///     Returns true if level info was found, false otherwise (e.g. test scenes).
	/// </summary>
	public bool TryGetCurrentLevelInfo(out LevelData.LevelInfo info)
	{
		info = null;
		if (!HasValidLevelData) return false;

		if (CurrentLevelIndex < 0 || CurrentLevelIndex >= LevelCount)
		{
			// Fallback scenario: Check current level name and if it exists in _levelData.Levels
			var currentSceneName = SceneManager.GetActiveScene().name;
			for (var i = 0; i < _levelData.Levels.Length; i++)
				if (_levelData.Levels[i].SceneName == currentSceneName)
				{
					CurrentLevelIndex = i;
					info = _levelData.Levels[i];
					return true;
				}

			return false;
		}

		info = _levelData.Levels[CurrentLevelIndex];
		return true;
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
		var levelName = _levelData.Levels[CurrentLevelIndex].SceneName;

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
