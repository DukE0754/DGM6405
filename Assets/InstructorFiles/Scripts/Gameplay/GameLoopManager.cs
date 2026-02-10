using DGM6405.Events;
using JetBrains.Annotations;
using UnityEngine;

/// <summary>
///     This manages the main game loop,
///     which should only start when the scene is fully ready
///     This manager is not a singleton and does not survive the game loop
///     This interacts with the <see cref="GameMgr" /> for any persistant data or states
/// </summary>
public class GameLoopManager : MonoBehaviour
{
	[Header("Spawning")]
	[SerializeField] private GameObject _playerPrefab;

	[SerializeField] private LevelSpawnPoint _spawnPoint;

	/// <summary>
	///     Timer for use with the <see cref="_isCountdownTimer" />
	/// </summary>
	[UsedImplicitly] // Accessible in case UI wants to show value
	public float GameTimer { get; private set; }

	private void Start()
	{
		// Only auto-start and show UI if we are in a proper game flow or if configured to do so
		if (GameMgr.Instance.GameState != GameMgr.GameStates.Loading)
			UIMgr.Instance.ShowMenu(GameMenus.InGameUI, StartGame);

		SpawnPlayer();
		GlobalEventBus.Instance.Raise<ILevelListener>(l => l.OnLevelReady());
	}

	private void Update()
	{
		if (GameMgr.Instance.IsGameRunning)
			GameTimer += Time.timeScale * Time.deltaTime; // Count up for now, may change later.
	}

	private void SpawnPlayer()
	{
		var spawnPos = Vector3.zero;
		var spawnRot = Quaternion.identity;

		if (_spawnPoint != null)
		{
			spawnPos = _spawnPoint.transform.position;
			spawnRot = _spawnPoint.transform.rotation;
		}

		// If player already exists (test scene), just move them
		var existingPlayer = PlayerMgr.Instance != null ? PlayerMgr.Instance.PlayerObject : null;
		if (existingPlayer != null && existingPlayer.activeInHierarchy)
		{
			existingPlayer.transform.SetPositionAndRotation(spawnPos, spawnRot);
			return;
		}

		// Otherwise spawn new
		if (_playerPrefab != null)
		{
			var player = Instantiate(_playerPrefab, spawnPos, spawnRot);
			PlayerMgr.Instance?.RegisterPlayer(player);
		}
		else
		{
			Debug.LogWarning("GameLoopManager: No Player Prefab assigned and no player in scene.");
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
}
