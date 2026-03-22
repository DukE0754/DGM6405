using UnityEngine;

/// <summary>
///     Simple implementation of ITargetProvider that returns the player transform.
/// </summary>
public class PlayerTargetProvider : MonoBehaviour, ITargetProvider, IHealthListener
{
	[SerializeField] private Vector3 _offset = new(0, 1f, 0);

	private GameObject _trackedPlayerObject;
	private LocalEventBus _trackedPlayerBus;
	private bool _isTargetDead;

	public bool HasTarget
	{
		get
		{
			var target = ResolveCurrentTarget();
			return target != null && !_isTargetDead;
		}
	}

	private void OnDisable()
	{
		UnbindTrackedPlayer();
	}

	public Transform GetTarget()
	{
		var target = ResolveCurrentTarget();
		if (target == null || _isTargetDead) return null;
		return target.transform;
	}

	public Vector3 GetTargetOffset()
	{
		return _offset;
	}

	public Vector3 GetTargetPosition()
	{
		var target = GetTarget();
		if (target != null) return target.position + GetTargetOffset();
		return transform.position;
	}

	public void OnHealthChanged(float current, float max)
	{
		_isTargetDead = current <= 0f;
	}

	public void OnDamageTaken(int amount, Vector3 direction)
	{
	}

	public void OnDied()
	{
		_isTargetDead = true;
	}

	private GameObject ResolveCurrentTarget()
	{
		var playerObject = PlayerMgr.Instance != null && PlayerMgr.Instance.HasSpawnedPlayer
			? PlayerMgr.Instance.PlayerObject
			: null;

		if (playerObject != _trackedPlayerObject) BindTrackedPlayer(playerObject);

		return _trackedPlayerObject;
	}

	private void BindTrackedPlayer(GameObject playerObject)
	{
		UnbindTrackedPlayer();

		_trackedPlayerObject = playerObject;
		_isTargetDead = false;

		if (_trackedPlayerObject == null) return;

		if (_trackedPlayerObject.TryGetComponent(out Health health))
			_isTargetDead = health.IsDead;

		if (_trackedPlayerObject.TryGetComponent(out PlayerDeathHandler deathHandler) && deathHandler.IsDead)
			_isTargetDead = true;

		if (_trackedPlayerObject.TryGetComponent(out LocalEventBus bus))
		{
			_trackedPlayerBus = bus;
			_trackedPlayerBus.Register<IHealthListener>(this);
		}
	}

	private void UnbindTrackedPlayer()
	{
		if (_trackedPlayerBus != null) _trackedPlayerBus.Unregister<IHealthListener>(this);

		_trackedPlayerBus = null;
		_trackedPlayerObject = null;
		_isTargetDead = false;
	}
}
