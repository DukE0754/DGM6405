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
	/// <summary>
	///     Timer for use with the <see cref="_isCountdownTimer" />
	/// </summary>
	[UsedImplicitly] // Accessible in case UI wants to show value
	public float GameTimer { get; private set; }

	private void Update()
	{
		if (GameMgr.Instance.IsGameRunning)
			GameTimer += Time.timeScale * Time.deltaTime; // Count up for now, may change later.
	}
}
