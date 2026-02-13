using UnityEngine;

/// <summary>
///     Defines a spawn point for the player in a level.
/// </summary>
public class PlayerSpawnPoint : Singleton<PlayerSpawnPoint>
{
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
	}
}
