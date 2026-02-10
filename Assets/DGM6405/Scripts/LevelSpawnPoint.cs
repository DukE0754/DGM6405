using DGM6405.Events;
using UnityEngine;

/// <summary>
///     Defines a spawn point for the player in a level.
/// </summary>
public class LevelSpawnPoint : MonoBehaviour
{
	private void Start()
	{
		GlobalEventBus.Instance.Raise<ILevelSpawnListener>(l => l.OnSpawnPointReady(this));
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, 0.5f);
		Gizmos.DrawLine(transform.position, transform.position + transform.forward * 1f);
	}
}
