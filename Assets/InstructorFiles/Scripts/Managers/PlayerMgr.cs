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
	///     Handles the player using the pause input action
	/// </summary>
	public void PauseInput()
	{
		// Don't allow pausing if the player is dead/dying
		if (PlayerObject != null)
		{
			var deathHandler = PlayerObject.GetComponent<PlayerDeathHandler>();
			if (deathHandler != null && deathHandler.IsDead)
				return;
		}

		// Run pause from game manager
		GlobalContext.Instance.GameMgr.PauseGameToggle();
	}

	public void DebugAssignAsPlayer(GameObject player)
	{
		PlayerObject = player;
	}
}
