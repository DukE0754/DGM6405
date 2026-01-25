using System.ComponentModel;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
///     This manages the main game loop,
///     which should only start when the scene is fully ready
///     This manager is not a singleton and does not survive the game loop
///     This interacts with the <see cref="GameMgr" /> for any persistant data or states
/// </summary>
public class GameLoopManager : MonoBehaviour
{
	/// <summary>
	///     Timer for use with the <see cref="_isCountdownTimer" />
	/// </summary>
	[UsedImplicitly] // Accessible in case UI wants to show value
	public float GameTimer { get; private set; }

	private void Start()
	{
		if (GameMgr.Instance.GameState != GameMgr.GameStates.Loading)
			UIMgr.Instance.ShowMenu(GameMenus.InGameUI, StartGame);
	}

	private void Update()
	{
		if (GameMgr.Instance.IsGameRunning)
		{
			GameTimer += Time.timeScale * Time.deltaTime; // Count up for now, may change later.
		}
	}

	public void StartGame()
	{
		GameMgr.Instance.StartGame();
	}

	/// <summary>
	///     End the game loop
	///     Either if time expires or some other reason
	/// </summary>
	private void GameOver()
	{
		GameMgr.Instance.GameOver();
	}

	public void OnPause()
	{
		GameMgr.Instance.PauseGameToggle();
	}
}
