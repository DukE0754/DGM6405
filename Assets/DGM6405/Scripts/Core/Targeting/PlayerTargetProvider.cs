using UnityEngine;

/// <summary>
///     Simple implementation of ITargetProvider that returns the player transform.
/// </summary>
public class PlayerTargetProvider : MonoBehaviour, ITargetProvider
{
	[SerializeField] private Vector3 _offset = new(0, 1f, 0);

	public bool HasTarget => PlayerMgr.Instance != null && PlayerMgr.Instance.HasSpawnedPlayer;

	public Transform GetTarget()
	{
		if (HasTarget) return PlayerMgr.Instance.PlayerObject.transform;
		return null;
	}

	public Vector3 GetTargetPosition()
	{
		if (HasTarget) return PlayerMgr.Instance.PlayerObject.transform.position + _offset;
		return transform.position;
	}
}
