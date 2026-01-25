using System;
using UnityEngine;

/// <summary>
///     Manages the gameplay, start, end, score, etc.
///     Works with the <see cref="GameLoopManager" />
///     This has the responsibility of managing non-loop related elements,
///     such as score, states, etc. that may need to be available in other scenes
/// </summary>
public class GameMgr : Singleton<GameMgr>
{
	public enum GameStates
	{
		Menu,
		InGame,
		Paused,
		GameOver,
		Loading
	}

	[SerializeField] private GameStates _gameState = GameStates.Menu;

	public GameStates GameState
	{
		get => _gameState;
		set => _gameState = value;
	}

	/// <summary>
	///     Are we actively in the gameplay state.
	///     Should the game loop be looping
	/// </summary>
	public bool IsGameRunning => _gameState == GameStates.InGame;
	public bool IsPaused => _gameState == GameStates.Paused;

	public event Action<bool> PauseStateChanged;

#region Score
	/// <summary>
	///     Example of GameMgr responsibility.
	///     Score may need to survive the game loop for Game Over screen
	/// </summary>
	public float Score { get; private set; }

	/// <summary>
	///     Reset the score, assumes starting at zero
	/// </summary>
	public void ResetScore()
	{
		Score = 0;
	}

	/// <summary>
	///     Gain score from a source
	/// </summary>
	/// <param name="value"></param>
	public void AddScore(float value)
	{
		Score += value;
	}

	/// <summary>
	///     Subtract score, don't allow lower than zero
	/// </summary>
	/// <param name="value"></param>
	public void SubtractScore(float value)
	{
		Score = Mathf.Max(0, Score - value);
	}
	#endregion

	/// <summary>
	///     Begin the game and start the game loop
	///     This should only happen after the game scene is fully loaded
	///     and any-pre loop functionality has resolved
	/// </summary>
	public void StartGame()
	{
		_gameState = GameStates.InGame;
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.Locked;
	}

	/// <summary>
	///     Handle the result of the game ending
	/// </summary>
	public void GameOver()
	{
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		_gameState = GameStates.GameOver;
		SceneMgr.Instance.LoadScene(GameScenes.GameOver, GameMenus.GameOverMenu, () => GameState = GameStates.GameOver);
	}

	public void NextLevel(int timeMs)
	{
		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.None;
		_gameState = GameStates.GameOver;
		LevelMgr.Instance.CompleteCurrentLevel(timeMs);
		SceneMgr.Instance.LoadScene(
			GameScenes.GameOver, GameMenus.LevelCompleteMenu, () => GameState = GameStates.GameOver);
	}

	/// <summary>
	///     Toggle the game state
	/// </summary>
	public void PauseGameToggle()
	{
		SetPaused(!IsPaused);
	}

	public void SetPaused(bool paused)
	{
		if (paused)
		{
			if (IsPaused)
				return;

			if (!IsGameRunning)
			{
				Debug.LogWarning("Game is not running, cannot toggle pause");
				return;
			}

			Time.timeScale = 0f;
			Cursor.lockState = CursorLockMode.None;
			_gameState = GameStates.Paused;
			Debug.Log("Pause state enabled");
			UIMgr.Instance.ShowMenu(GameMenus.PauseMenu);
			PauseStateChanged?.Invoke(true);
			return;
		}

		if (!IsPaused)
			return;

		Time.timeScale = 1f;
		Cursor.lockState = CursorLockMode.Locked;
		_gameState = GameStates.InGame;
		Debug.Log("Pause state disabled");
		UIMgr.Instance.HideMenu(GameMenus.PauseMenu);
		PauseStateChanged?.Invoke(false);
	}
}
