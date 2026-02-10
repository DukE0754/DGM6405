using DGM6405.Events;
using UnityEngine;

/// <summary>
///     A singleton for communicating with the player object when it exists.
///     This manager persists across scenes to safely handle player references.
/// </summary>
public class PlayerMgr : Singleton<PlayerMgr>
{
	/// <summary>
	///     The current player object in the scene.
	///     Returns null if no player exists.
	/// </summary>
	public GameObject PlayerObject { get; private set; }

	public override void Awake()
	{
		base.Awake();

		// If we are the instance, we can try to find the player immediately
		if (Instance == this && PlayerObject == null) PlayerObject = PlayerObject;
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
}
