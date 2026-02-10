using UnityEngine;

/// <summary>
///     A singleton for communicating with the player object when it exists.
///     This manager persists across scenes to safely handle player references.
/// </summary>
public class PlayerMgr : Singleton<PlayerMgr>
{
	[SerializeField] private GameObject _playerPrefab;
	
	/// <summary>
	///     The current player object in the scene.
	///     Returns null if no player exists.
	/// </summary>
	public GameObject PlayerObject { get; private set; }

	public bool HasSpawnedPlayer => PlayerObject != null;
	
	public void SpawnPlayer(Vector3 position, Quaternion rotation)
	{
		if (PlayerObject)
		{
			Debug.LogError("Player already spawned!");
			return;
		}

		PlayerObject = Instantiate(_playerPrefab, position, rotation);
		Debug.Log("Player spawned");
	}
	
	/// <summary>
	///     Registers the player object with the manager.
	///     Called when a player is spawned or initialized.
	/// </summary>
	public void RegisterPlayer(GameObject player)
	{
		PlayerObject = player;
		GlobalEventBus.Instance.Raise<IPlayerGlobalListener>(l => l.OnPlayerSpawned(player));
	}

	/// <summary>
	///     Handles the player using the pause input action
	/// </summary>
	public void PauseInput()
	{
		// Run pause from game manager
		GameMgr.Instance.PauseGameToggle();
	}

	public void DebugAssignAsPlayer(GameObject player)
	{
		PlayerObject = player;
	}
}
