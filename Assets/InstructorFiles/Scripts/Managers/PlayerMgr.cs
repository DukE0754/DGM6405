using UnityEngine;

/// <summary>
///     A singleton for communicating with the player object when it exists.
///     This manager persists across scenes to safely handle player references.
/// </summary>
public class PlayerMgr : Singleton<PlayerMgr>
{
	private GameObject _playerObject;

	/// <summary>
	///     The current player object in the scene.
	///     Returns null if no player exists.
	/// </summary>
	public GameObject PlayerObject
	{
		get
		{
			// If we have a cached reference, check if it's still valid
			if (_playerObject != null)
			{
				return _playerObject;
			}

			// If not cached, try to find the player in the scene
			// This allows immediate testing in a scene that already has a player
			_playerObject = GameObject.FindWithTag("Player");

			// Fallback: try to find by component if tag is not set
			if (_playerObject == null)
			{
				var brain = FindFirstObjectByType<PlayerCommandBrain>();
				if (brain != null)
				{
					_playerObject = brain.gameObject;
				}
			}

			return _playerObject;
		}
	}

	public override void Awake()
	{
		base.Awake();
		
		// If we are the instance, we can try to find the player immediately
		if (Instance == this && _playerObject == null)
		{
			_playerObject = PlayerObject;
		}
	}

	/// <summary>
	///     Registers the player object with the manager.
	///     Called when a player is spawned or initialized.
	/// </summary>
	public void RegisterPlayer(GameObject player)
	{
		_playerObject = player;
	}

	/// <summary>
	///     Handles the player using the pause input action
	/// </summary>
	public void PauseInput()
	{
		// Run pause from game manager
		GameMgr.Instance.PauseGameToggle();
	}
}
